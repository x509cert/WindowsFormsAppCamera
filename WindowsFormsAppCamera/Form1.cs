using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace WindowsFormsAppCamera
{
    public partial class Form1 : Form
    {
        #region Helper classes
        // used to track RGB pixel colors
        struct RGBTotal
        {
            private long b;
            private long g;
            private long r;

            public long R { get => r; set => r = value; }
            public long G { get => g; set => g = value; }
            public long B { get => b; set => b = value; }

            public void Init()
            {
                r = g = b = 0L;
            }
        }

        // keeps track of log entries for uploading to Azure
        class LogQueue : Queue<string>
        {
            private const int MAX = 20;
            private readonly int _max;

            public LogQueue(int max = MAX)
            {
                _max = max;
            }

            public void Enqueue2(string s)
            {
                if (s.Length == 0)
                    return;

                // if we have reached the max size of the queue, then remove the oldest item
                if (Count > _max)
                    Dequeue();

                Enqueue(s);
            }
        }
#endregion

        #region Class member variables
        UsbCamera               _camera;

        Config                  _cfg = null;

        LogQueue                _logQueue;
        string                  _sLogFilePath;
        const string            _dateTemplate = "yyyy MMM dd, HH:mm:ss";
        string                  _gatewayIp = null;

        const int               _xHitBoxStart = 200,    // this is the hit box rectangle 
                                _yHitBoxStart = 200,
                                _xHitBoxEnd = 460,
                                _yHitBoxEnd = 270,
                                _widthHitBox = _xHitBoxEnd - _xHitBoxStart,
                                _heightHitBox = _yHitBoxEnd - _yHitBoxStart;
        Rectangle               _rectHitBox = new Rectangle(_xHitBoxStart, _yHitBoxStart, _widthHitBox, _heightHitBox);

        Thread                  _threadWorker = null;
        Thread                  _threadLog = null;
        Thread                  _threadPinger = null;

        SerialPort              _sComPort = null;
        const int               _ComPortSpeed = 9600;

        bool                    _fKillThreads = false;
        System.Timers.Timer     _skillTimer = null;
        System.Timers.Timer      _heartbeatTimer = null;

        bool                    _fUsingLiveScreen = true;
        TimeSpan                _elapseBetweenDrones = new TimeSpan(0, 0, 9);       // cooldown before we look for drones after detected
        TimeSpan                _longestTimeBetweenDrones = new TimeSpan(0, 0, 31); // longest time we can go without seeing a drone, used to send out an emergency EMP
        int                     _heartBeatSent = 0;

        SmsAlert                _smsAlert = null;

        readonly Brush          _colorInfo = Brushes.AliceBlue;
        readonly SolidBrush     _brushYellow = new SolidBrush(Color.FromArgb(99, Color.Yellow));
        readonly Pen            _penYellow = new Pen(Color.FromKnownColor(KnownColor.Yellow));

        DateTime                _startTraceTimer;                       // this is for dumping a trace of the screenshots for 20secs - approx 100 images
        readonly TimeSpan       _maxTraceTime = new TimeSpan(0, 0, 20); // trace for 20secs

#endregion

        #region Logs
        // writes log data to a local log file
        private void WriteLog(string s)
        {
            DateTime dt = DateTime.UtcNow;
            var dts = dt.ToString(_dateTemplate);

            // Arduino messages are only one char long, so add a little more context
            if (s.Length == 1)
            {
                var ch = s[0];
                s = "    Msg to Arduino: ";

                switch (ch)
                {
                    case 'E': s += "EMP"; break;
                    case 'T': s += "Turret"; break;
                    case 'U': s += "LB/RB up"; break;
                    case 'V': s += "Verify comms"; break;
                    case 'R': s += "Inc RB sweep (+1)"; break;
                    case 'r': s += "Dec RB sweep (-1)"; break;
                    case 'L': s += "Inc LB sweep (+1)"; break;
                    case 'l': s += "Dec LB sweep (-1)"; break;
                    case 'H': s += "Heartbeat"; break;
                    default:  s += "!!Unknown command!!"; break;
                }    
            }

            // text for log file entry
            string entry = dts + ", " + s;
            
            // log filename
            string sLogFile = _sLogFilePath + "\\DivGrind-" + dt.Year.ToString() + dt.Month.ToString() + dt.Day.ToString() + ".log";

            // log to file
            try
            {
                using (StreamWriter w = File.AppendText(sLogFile))
                {
                    w.WriteLine(entry);
                    _logQueue.Enqueue2(entry);
                }
            }
            catch (Exception)
            {
                // keep on chugging
            }
        }

        // uploads the last log N-entries to Azure every few secs
        private void UploadLogs()
        {
            // if no log upload UI is set, then don't attempt to upload the data
            if (string.IsNullOrEmpty(_cfg.LogUri))
                return;

            // title for the log collection
            // by default this is the machine name or whatever was passed in on the command-line using -n
            var title = _cfg.MachineName;

            // build the packet of data that goes to Azure
            var sb = new StringBuilder(512);
            const string delim = "|";

            sb.Append(title);
            sb.Append(delim);

            sb.Append("Last update [UTC:" + DateTime.UtcNow.ToString(_dateTemplate) + "][Local:" + DateTime.Now.ToString(_dateTemplate)  + "]  Using ");
            sb.Append(_fUsingLiveScreen ? "live video" : "timer");
            sb.Append(delim);

            // loop through each log entry, add to the structure to send to Azure and remove from the queue
            while (_logQueue.Count > 0)
            {
                sb.Append(_logQueue.Dequeue());
                sb.Append(delim);
            }

            // push up to an Azure Function
            try
            {
                WebClient wc = new WebClient();
                wc.Headers.Add("user-agent", "DivGrind C# Client");
                wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";

                wc.UploadString(_cfg.LogUri, sb.ToString());
            } catch (Exception ex)
            {
                WriteLog($"EXCEPTION: Error uploading to Azure {ex.Message}.");
            }
        }
        #endregion

        #region Arduino Interface

        // Send command to the Arduino over the COM port (ie; USB port)
        // The USB port is treated as a COM port
        // R - increase RB sweep (+/- 5)
        // r - decrease RB sweep (+/- 5)
        // L - increase LB sweep (+/- 5)
        // l - decrease LB sweep (+/- 5)
        // E - deploy EMP
        // T - deploy Turret
        // U - all triggers up
        // V - verify comms
        // H - heartbeat - notifies the Arduino that this code is alive
        void TriggerArduino(string msg)
        {
            WriteLog(msg);

            if (_sComPort == null)
            {
                WriteLog("FATAL: COM Port is not set.");
                return;
            }       

            if (_sComPort.IsOpen == false)
            { 
                WriteLog("FATAL: COM Port is not open.");
                return;
            }       

            try
            {
                _sComPort.Write(msg);
                Thread.Sleep(100);
            }
            catch (Exception ex)
            {
                WriteLog($"EXCEPTION: Arduino COM error {ex.Message}.");
            }
        }

        void DeploySkill(Object source, ElapsedEventArgs e)
        {
            WriteLog("Deploy turret");
            TriggerArduino("T");

            // if not use the camera, then pop the EMP, too
            if (!_fUsingLiveScreen)
            {
                WriteLog("Deploy EMP");
                TriggerArduino("E");
            }
        }

        void SendHeartbeat(Object source, ElapsedEventArgs e)
        {
            WriteLog("Heartbeat sent");
            TriggerArduino("H");

            _heartBeatSent = 6;
        }
        #endregion

        #region Secondary Thread Functions
        // Press the turret ever 15secs
        // if the screen is blank, then hit the EMP too
        private void SetSkillTimer()
        {
            if (_skillTimer == null)
            {
                _skillTimer = new System.Timers.Timer(15000);
                _skillTimer.Elapsed += DeploySkill;
                _skillTimer.AutoReset = true;
                _skillTimer.Enabled = true;
            }
            else
            {
                _skillTimer.Start();
            }
        }

        private void KillSkillTimer()
        {
            if (_skillTimer != null)
            {
                _skillTimer.Stop();
                _skillTimer = null;
            }
        }

        // this is a message sent to the Arduino to indicate the DivGrind is alive
        // this prevents the Arduino from going into failsafe mode
        private void SetHeartbeat()
        {
            if (_heartbeatTimer == null)
            {
                _heartbeatTimer = new System.Timers.Timer(15000);
                _heartbeatTimer.Elapsed += SendHeartbeat;
                _heartbeatTimer.AutoReset = true;
                _heartbeatTimer.Enabled = true;
                _heartbeatTimer.Start();
            } 
            else
            {
                _heartbeatTimer.Start();
            }
        }

        private void StopHeartbeat()
        {
            if (_heartbeatTimer != null)
            {
                _heartbeatTimer.Stop();
                _heartbeatTimer = null;
            }
        }


        // Code to ping the local gateway and ubisoft every 30secs
        private void PingerThread()
        {
            while (_fKillThreads == false)
            {
                // if there's no gateway IP address, then use tracert to get it
                if (String.IsNullOrEmpty(_gatewayIp))
                {
                    Process p = new Process();

                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.FileName = "tracert";
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.Arguments = "-h 2 -d ubisoft.com";
                    p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden | ProcessWindowStyle.Minimized;
                    p.Start();

                    string output = p.StandardOutput.ReadToEnd();
                    p.WaitForExit();

                    // simplified (lazy) IP address regex
                    Regex rx = new Regex(@"([0-9]{1,3})\.([0-9]{1,3})\.([0-9]{1,3})\.([0-9]{1,3})");

                    string[] lines = output.Split('\n');
                    for (int i = 2; i < lines.Length; i++)
                    {
                        MatchCollection matches = rx.Matches(lines[i]);
                        if (matches.Count > 0)
                        {
                            _gatewayIp = matches[0].ToString();
                            break;
                        }
                    }
                } 
                else 
                {
                    // we have a gateway IP address
                    Ping pingSender = new Ping();
                    PingReply replyGateway = pingSender.Send(_gatewayIp, 1000);
                    WriteLog("Ping: " + _gatewayIp + " " + replyGateway.Status.ToString() + "  " + replyGateway.RoundtripTime + "ms");

                    PingReply replyRemote = pingSender.Send("ubisoft.com", 10000);
                    WriteLog("Ping: ubisoft.com " + replyRemote.Status.ToString() + " " + replyRemote.RoundtripTime + "ms");
                }

                SpinDelay(30);
            }
        }

        // a thread function that uploads log data to Azure
        private void UploadLogThreadFunc()
        {
            while (!_fKillThreads)
            {
                UploadLogs();
                SpinDelay(60);
            }
        }

        // starts all the worker threads
        private void StartAllThreads()
        {
            _fKillThreads = false;

            _threadWorker = new Thread(WorkerThreadFunc);
            _threadWorker.Start();

            _threadLog = new Thread(UploadLogThreadFunc);
            _threadLog.Start();

            _threadPinger = new Thread(PingerThread);
            _threadPinger.Start();
        }

        #endregion

        #region UI Elements

        // start drone monitoring
        private void btnStart_Click(object sender, EventArgs e)
        {
            // TODO: Need more work here... not all UI elements are enabled/disabled when approp. 
            btnStart.Enabled = false;
            btnStop.Enabled = true;

            cmbCamera.Enabled = false;
            cmbCameraFormat.Enabled = false;
            cmbComPorts.Enabled = false;

            btnTestComPort.Enabled = false;

            StartAllThreads();
        }

        // Stop the drone monitoring
        private void button3_Click(object sender, EventArgs e)
        {
            btnStop.Enabled = false;
            btnStart.Enabled = true;

            cmbCamera.Enabled = true;
            cmbCameraFormat.Enabled = true;
            cmbComPorts.Enabled = true;

            btnTestComPort.Enabled = true;

            TriggerArduino("U");
            
            _fKillThreads = true;
            KillSkillTimer();

            // DO NOT KILL THE HEARTBEAT TIMER
        }

        // save current camera image to a bitmap
        private void btnSaveBmp_Click(object sender, EventArgs e)
        {
            Bitmap bmp = _camera.GetBitmap();
            var date = DateTime.Now;
            var dtFormat = date.ToString("MMddHH-mmss");
            bmp.Save(_sLogFilePath + @"\Div" + dtFormat + ".bmp");
        }

        // switch between live and blank (timer) screen
        private void btnToggleBlankOrLiveScreen_Click(object sender, EventArgs e)
        {
            WriteLog(_fUsingLiveScreen ? "Flipping to Blank" : "Flipping to Live");
            btnToggleBlankOrLiveScreen.Text = _fUsingLiveScreen ? "To Live" : "To Blank";
            _fUsingLiveScreen = !_fUsingLiveScreen;
        }

        // these are used to send discrete commands to the Arduino
        private void btnRecalLeftLess_Click(object sender, EventArgs e) { TriggerArduino("l"); }
        private void btnRecalLeftMore_Click(object sender, EventArgs e) { TriggerArduino("L"); }
        private void btnRecalRightLess_Click(object sender, EventArgs e){ TriggerArduino("r"); }
        private void btnRecalRightMore_Click(object sender, EventArgs e){ TriggerArduino("R"); }
        private void btnAllUp_Click(object sender, EventArgs e)         { TriggerArduino("U"); }
        private void button3_Click_1(object sender, EventArgs e)        { TriggerArduino("E"); } // EMP
        private void button2_Click_1(object sender, EventArgs e)        { TriggerArduino("T"); } // Turret

        // switch text colors in the bitmap (kinda useless!)
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            // Nothing
        }

        // this lets you fine-tune the % red increase to trigger the EMP (ie; 'Drones Incoming')
        private void numTrigger_ValueChanged(object sender, EventArgs e)
        {
            _cfg.ThreshHold = (float)numTrigger.Value;
            WriteConfig(_cfg);
        }

        // The Calibrate Button
        private void btnCalibrate_Click(object sender, EventArgs e)
        {
            // read the camera
            var bmp = _camera.GetBitmap();

            // Get Calibration data
            var rbgTotal = new RGBTotal();
            GetRGBInRange(bmp, ref rbgTotal);

            lblRedCount.Text = rbgTotal.R.ToString("N0");
            lblGreenCount.Text = rbgTotal.G.ToString("N0");
            lblBlueCount.Text = rbgTotal.B.ToString("N0");

            _cfg.LastCalibratedR = (int)rbgTotal.R;
            _cfg.LastCalibratedB = (int)rbgTotal.B;
            _cfg.LastCalibratedG = (int)rbgTotal.G;
            WriteConfig(_cfg);

            // draw yellow hit box
            DrawTargetRange(bmp);
            pictCamera.Image = bmp;
        }

#endregion

        #region Bitmap and drone detection code

        // determines the increase in red required to determine if the drones are incoming
        private float GetRedSpottedPercent()
        {
            return (float)_cfg.LastCalibratedR + (((float)_cfg.LastCalibratedR / 100.0F) * _cfg.ThreshHold);
        }

        private void btnTestSms_Click(object sender, EventArgs e)
        {
            if (_smsAlert == null)
                MessageBox.Show("No SMS Client is defined.");
            else
                if (!_smsAlert.RaiseAlert($"Gen2 DivGrind Test from {_smsAlert.MachineName}"))
                    WriteLog("SMS test alert failed");
        }

        // start a timer that creates screen shots
        private void btnTrace_Click(object sender, EventArgs e)
        {
            _startTraceTimer = DateTime.Now;
            btnTrace.Enabled = false;
        }

        private void numDroneDelay_ValueChanged(object sender, EventArgs e)
        {
            _elapseBetweenDrones = new TimeSpan(0,0, (int)numDroneDelay.Value);
        }

        // logic to determine if drones are coming
        bool DronesSpotted(ref RGBTotal rbgTotal)
        {
            // if there is no increase in red, then no drones
            return rbgTotal.R > GetRedSpottedPercent();
        }

        // draws the yellow rectangle 'hitbox' -
        // this is the area the code looks at for the increase in red
        // that indicates the drones are incoming
        private void DrawTargetRange(Bitmap bmp)
        {
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.FillRectangle(_brushYellow, _rectHitBox);
                g.DrawRectangle(_penYellow, _rectHitBox);
            }
        }

        // counts the number of RGB elements in pixels in the hitbox
        // skips every other pixel on the x-axis for perf
        private void GetRGBInRange(Bitmap bmp, ref RGBTotal rbgTotal)
        {
            rbgTotal.Init();
            Int32 countPixel = 0;

            for (int x = _xHitBoxStart; x < _xHitBoxEnd; x+=2)
            {
                for (int y = _yHitBoxStart; y < _yHitBoxEnd; y++)
                {
                    Color px = bmp.GetPixel(x, y);
                    countPixel++;

                    // RGB totals
                    rbgTotal.R += (Int32)px.R; 
                    rbgTotal.G += (Int32)px.G;
                    rbgTotal.B += (Int32)px.B;
                }
            }

            rbgTotal.R /= countPixel;
            rbgTotal.G /= countPixel;
            rbgTotal.B /= countPixel;
        }

        #endregion

    }
}
