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

        readonly LogQueue       _logQueue;
        readonly string         _sLogFilePath;
        const string            _dateTemplate = "yyyy MMM dd, HH:mm:ss";
        string                  _logUri;
        string                  _gatewayIp = null;

        readonly int            _xHitBoxStart = 200,    // this is the hit box rectangle 
                                _yHitBoxStart = 200, 
                                _xHitBoxEnd = 460, 
                                _yHitBoxEnd = 270; 

        Thread                  _threadWorker = null;
        Thread                  _threadLog = null;
        Thread                  _threadPinger = null;

        SerialPort              _sComPort = null;
        const int               _ComPortSpeed = 9600;

        bool                    _fKillThreads = false;
        System.Timers.Timer     _skillTimer = null;

        RGBTotal                _calibrationData;
        bool                    _fUsingLiveScreen = true;
        float                   _triggerPercent = 50F;
        TimeSpan                _elapseBetweenDrones = new TimeSpan(0, 0, 9);       // cooldown before we look for drones after detected
        TimeSpan                _longestTimeBetweenDrones = new TimeSpan(0, 0, 31); // longest time we can go without seeing a drone, used to send out an emergency EMP

        string                  _machineName;
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
            // URL to the Azure Function
            _logUri = "https://divgrind.azurewebsites.net/api/DivGrindLog?verb=u";

            // if no log upload UI is set, then don't attempt to upload the data
            if (string.IsNullOrEmpty(_logUri))
                return;

            // title for the log collection
            // by default this is the machine name or whatever was passed in on the command-line using -n
            var title = _machineName;

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

                wc.UploadString(_logUri, sb.ToString());
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
                Thread.Sleep(200);
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
                    p.StartInfo.Arguments = "-h 2 -d ubisoft.com";
                    p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden | ProcessWindowStyle.Minimized;
                    p.Start();
                    string output = p.StandardOutput.ReadToEnd();
                    p.WaitForExit();

                    // simplified (lazy) IP address
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

                // we have a gateway IP address
                Ping pingSender = new Ping();
                PingReply replyGateway = pingSender.Send(_gatewayIp, 1000);
                WriteLog("Ping: " + _gatewayIp + " " + replyGateway.Status.ToString() + "  " + replyGateway.RoundtripTime + "ms");

                PingReply replyRemote = pingSender.Send("ubisoft.com", 10000);
                WriteLog("Ping: ubisoft.com " + replyRemote.Status.ToString() + " " + replyRemote.RoundtripTime + "ms");

                Thread.Sleep(30000);
            }
        }

        // a thread function that uploads log data to Azure
        private void UploadLogThreadFunc()
        {
            while (!_fKillThreads)
            {
                UploadLogs();
                Thread.Sleep(60000); // delay 60 secs
            }
        }

        #endregion

        #region UI Elements

        // Code to read command-line args
        // -n "machinename" -c "connectionstring" -f "from sms #"  -t "to sms #" -u "log app URI"
        bool GetCmdLineArgs(ref string machineName,         // -n
                            ref string azureCommsString,    // -c
                            ref string azureSmsFrom,        // -f
                            ref string azureSmsTo,          // -t
                            ref string logUri)              // -u 
        {
            bool success = true;

            string[] args = Environment.GetCommandLineArgs();
            if (args.Length == 0) return false;

            try
            {
                for (int i = 1; i < args.Length; i++)
                {
                    if (args[i].ToLower().StartsWith("-n") == true)
                    {
                        machineName = args[i + 1];
                        txtName.Text = machineName;
                    }

                    if (args[i].ToLower().StartsWith("-c") == true)
                    {
                        azureCommsString = args[i + 1];
                    }

                    if (args[i].ToLower().StartsWith("-f") == true)
                    {
                        azureSmsFrom = args[i + 1];
                    }

                    if (args[i].ToLower().StartsWith("-t") == true)
                    {
                        azureSmsTo = args[i + 1];
                    }

                    if (args[i].ToLower().StartsWith("-u") == true)
                    {
                        logUri = args[i + 1];
                    }
                }
            }
            catch (Exception)
            {
                success = false;
            }

            return success;
        }

        // The main entry point
        public Form1()
        {
            InitializeComponent();

            // logs go in user's profile folder (eg; c:\users\mikehow)
            _sLogFilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            // get the date this binary was last created
            string strpath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            System.IO.FileInfo fi = new System.IO.FileInfo(strpath);
            string buildDate = fi.LastWriteTime.ToString("yyMMdd:HHmm");

            // machine name
            string machine = Dns.GetHostName();

            // add info to title of the tools
            Text = $"DivGrind [{buildDate}] on {machine}";

            numTrigger.Value = (decimal)_triggerPercent;

            _logQueue = new LogQueue(50);
            
            _machineName = Dns.GetHostName();

            string machineName = "";
            string azureConnection = "";
            string azureSmsFrom = "";
            string azureSmsTo = "";
            string logUri = "";

            bool ok = GetCmdLineArgs(ref machineName, ref azureConnection, ref azureSmsFrom, ref azureSmsTo, ref logUri);
            if (ok)
            {
                // set machine name
                if (string.IsNullOrEmpty(machineName) == false) _machineName = machineName;

                // if the three args are available for SMS, then create an SmsAlert object
                if (string.IsNullOrEmpty(azureConnection) == false &&
                    string.IsNullOrEmpty(azureSmsFrom) == false &&
                    string.IsNullOrEmpty(azureSmsTo) == false) 
                {
                    _smsAlert = new SmsAlert(machineName, azureConnection, azureSmsFrom, azureSmsTo) {
                        BlockLateNightSms = true
                    };

                    txtSmsEnabled.Text = "Yes";
                    btnTestSms.Enabled = true;
                }

                if (string.IsNullOrEmpty(logUri) == false)
                    _logUri = logUri;
            }
        }

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

            // start the two extra worker threads
            _fKillThreads = false;
            _threadWorker = new Thread(WorkerThreadFunc);
            _threadWorker.Start();

            _threadLog = new Thread(UploadLogThreadFunc);
            _threadLog.Start();

            _threadPinger = new Thread(PingerThread);
            _threadPinger.Start();
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

        private void cmbCamera_SelectedIndexChanged(object sender, EventArgs e)
        {
            int cameraIndex = cmbCamera.SelectedIndex;
            UsbCamera.VideoFormat[] formats = UsbCamera.GetVideoFormat(cameraIndex);

            cmbCameraFormat.Items.Clear();
            for (int i = 0; i < formats.Length; i++)
            {
                string f = "Resolution: " + formats[i].Caps.InputSize.ToString() + ", bits/sec: " + formats[i].Caps.MinBitsPerSecond;
                cmbCameraFormat.Items.Add(f);
            }
        }

        private void cmbCameraFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            // create usb camera object with selected resolution and start.
            int camera = cmbCamera.SelectedIndex;
            UsbCamera.VideoFormat[] formats = UsbCamera.GetVideoFormat(camera);
            
            var selectFormat = formats[cmbCameraFormat.SelectedIndex];
            if (selectFormat.Size.Width != 640 && selectFormat.Size.Height != 480)
                MessageBox.Show("Warning! Only 640x480 has been tested","Warning");

            if (selectFormat.Caps.MinBitsPerSecond == 0)
                MessageBox.Show("Warning! Selected format streams no data, choose another format", "Warning");

            _camera = new UsbCamera(camera, selectFormat);
            _camera.Start();

            foreach (string p in SerialPort.GetPortNames())
                cmbComPorts.Items.Add(p);
        }

        // COM port selected
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnTestComPort.Enabled = true;
            
            string sPort = cmbComPorts.SelectedItem.ToString();
            try
            {
                _sComPort = new SerialPort(sPort, _ComPortSpeed);
                _sComPort.Open();
            } catch (Exception ex)
            {
                WriteLog($"EXCEPTION: Unable to open COM port, error is {ex.Message}");
            }
        }

        // test the COM port
        private void btnTestComPort_Click(object sender, EventArgs e)
        {
            cmbComPorts.Items[cmbComPorts.SelectedIndex].ToString();
            TriggerArduino("V");
        }

        // send an EMP
        private void button3_Click_1(object sender, EventArgs e)
        {
            TriggerArduino("E");
        }

        // deploy Turret
        private void button2_Click_1(object sender, EventArgs e)
        {
            TriggerArduino("T");
        }

        // switch text colors in the bitmap (kinda useless!)
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            // Nothing
        }

        // this gives the code a chance to kill the main worker thread gracefully
        private void Form1_Closing(object sender, FormClosingEventArgs e)
        {
            KillSkillTimer();
            _fKillThreads = true;

            Thread.Sleep(400);
            e.Cancel = false;

            if (_sComPort != null) { 
                _sComPort.Close();
                _sComPort = null;
            }
        }

        // this lets you fine-tune the % red increase to trigger the EMP (ie; 'Drones Incoming')
        private void numTrigger_ValueChanged(object sender, EventArgs e)
        {
            _triggerPercent = (float)numTrigger.Value;
        }

        // The Calibrate Button
        private void btnCalibrate_Click(object sender, EventArgs e)
        {
            // read the camera
            var bmp = _camera.GetBitmap();

            _calibrationData.Init();

            // Get Calibration data
            var rbgTotal = new RGBTotal();
            GetRGBInRange(bmp, ref rbgTotal);

            lblRedCount.Text = rbgTotal.R.ToString("N0");
            lblGreenCount.Text = rbgTotal.G.ToString("N0");
            lblBlueCount.Text = rbgTotal.B.ToString("N0");

            _calibrationData.R = rbgTotal.R;
            _calibrationData.B = rbgTotal.B;
            _calibrationData.G = rbgTotal.G;

            // draw yellow hit box
            DrawTargetRange(bmp);
            pictCamera.Image = bmp;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string[] devices = UsbCamera.FindDevices();
            if (devices.Length == 0)
            {
                MessageBox.Show(this,"Uh oh! No Camera!");
                return; // no camera.
            }

            numDroneDelay.Text = _elapseBetweenDrones.TotalSeconds.ToString();

            foreach (string d in devices)
                cmbCamera.Items.Add(d);
        }

#endregion

        #region Bitmap and drone detection code

        // determines the increase in red required to determine if the drones are incoming
        private float GetRedSpottedPercent()
        {
            return (float)_calibrationData.R + (((float)_calibrationData.R / 100.0F) * _triggerPercent);
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

        // logic to determine if drones are coming - need to use floats owing to small numbers (0..255)
        bool DronesSpotted(ref RGBTotal rbgTotal)
        {
            // if there is no increase in red, then no drones
            float spottedRed = GetRedSpottedPercent();
            if (rbgTotal.R <= spottedRed)
                return false;

            return true;

            // if there is also an increase in blue and green, then it's an EMP flash
            //float spottedGreen = (float)_calibrationAvg.G + (((float)_calibrationAvg.G / 100.0F) * _triggerPercent);
            //float spottedBlue = (float)_calibrationAvg.B + (((float)_calibrationAvg.B / 100.0F) * _triggerPercent);

            //return rbgTotal.G < spottedGreen || rbgTotal.B < spottedBlue;
        }

        // draws the yellow rectangle 'hitbox' -
        // this is the area the code looks at for the increase in red
        // that indicates the drones are incoming
        private void DrawTargetRange(Bitmap bmp)
        {
            using (Graphics g = Graphics.FromImage(bmp))
            {
                int width = _xHitBoxEnd - _xHitBoxStart;
                int height = _yHitBoxEnd - _yHitBoxStart;
                Rectangle rectTarget = new Rectangle(_xHitBoxStart, _yHitBoxStart, width, height);

                g.FillRectangle(_brushYellow, rectTarget);
                g.DrawRectangle(_penYellow, rectTarget);
            }
        }
        // counts the number of RBGA elements in pixels in the hitbox
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
