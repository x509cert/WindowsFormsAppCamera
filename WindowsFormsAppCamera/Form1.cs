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
        private struct RgbTotal
        {
            public long R { get; set; }
            public long G { get; set; }
            public long B { get; set; }

            public void Init()
            {
                Trace.TraceInformation("RGBTotal::Init()");
                R = G = B = 0L;
            }
        }

        // keeps track of log entries for uploading to Azure
        private sealed class LogQueue : Queue<string>
        {
            private const int Max = 20;
            private readonly int _max;

            public LogQueue(int max = Max)
            {
                _max = max;
            }

            public void Enqueue2(string s)
            {
                Trace.TraceInformation($"LogQueue::Enqueue2, len = {Count}");

                if (s.Length == 0)
                    return;

                // if we have reached the max size of the queue, then remove the oldest item
                if (Count > _max)
                {
                    Trace.TraceInformation($"LogQueue::Enqueue2, dropping old data. len = {Count}, max= {_max}");
                    Dequeue();
                }

                Enqueue(s);
            }
        }
#endregion

        #region Class member variables
        private UsbCamera       _camera;

        private Config          _cfg;

        private LogQueue        _logQueue;
        private string          _sLogFilePath;
        private const string    DateTemplate = "MMM dd, HH:mm:ss";
        private const string    DateTemplateShort = "HH:mm:ss";
        private string          _gatewayIp;

        // Drone hitbox 
        private
        const int               XDroneHitBoxStart = 200,     
                                YDroneHitBoxStart = 200,
                                XDroneHitBoxEnd = 460,
                                YDroneHitBoxEnd = 270,
                                WidthDroneHitBox = XDroneHitBoxEnd - XDroneHitBoxStart,
                                HeightDroneHitBox = YDroneHitBoxEnd - YDroneHitBoxStart;
        private 
        readonly Rectangle      _rectDroneHitBox = new Rectangle(XDroneHitBoxStart, YDroneHitBoxStart, WidthDroneHitBox, HeightDroneHitBox);

        private Thread          _threadWorker;
        private Thread          _threadLog;
        private Thread          _threadPinger;

        private SerialPort      _sComPort;
        private const int       ComPortSpeed = 9600;

        private
        bool                    _bLBLongPress=false, 
                                _bRBLongPress=true;

        private bool            _fKillThreads;

        private System.Timers.Timer     
                                _skillTimer, _heartbeatTimer;

        private const int       _loopDelay = 200; // 200msec
        private const int       _threadStartDelay = 250;

        private bool            _fUsingLiveScreen = true;
        private TimeSpan        _elapseBetweenDrones = new TimeSpan(0, 0, 9);       // cooldown before we look for drones after detected

        private readonly TimeSpan       
                                _longestTimeBetweenDrones = new TimeSpan(0, 0, 31); // longest time we can go without seeing a drone, used to send out an emergency EMP

        private int             _heartBeatSent;
        private const int       MaxIncomingFrames = 10;

        private SmsAlert        _smsAlert;

        private readonly Brush  _colorInfo = Brushes.AliceBlue;
        private readonly Pen    _penHitBox = new Pen(Color.FromKnownColor(KnownColor.White));

        // this is for dumping a trace of the screenshots for 20secs - approx 100 images
        private DateTime        _startTraceTimer;

        private readonly TimeSpan       
                                _maxTraceTime = new TimeSpan(0, 0, 20);

        // RBG sliding chart and data
        private Chart           _chartR, _chartG, _chartB;
        private byte[]          _arrR, _arrG, _arrB;

        // shared memory for comms to the camera app
        private MMIo            _mmio;

        #endregion

        #region Logs
        // writes log data to a local log file
        private void WriteLog(string s)
        {
            Trace.TraceInformation($"WriteLog -> {s}");

            DateTime dt = DateTime.Now;
            var dts = dt.ToString(DateTemplateShort);

            // Arduino messages are only one 8-bit char long, so add a little more context
            if (s.Length == 1)
            {
                var ch = s[0];
                s = "    Msg to Arduino: ";

                switch (ch)
                {
                    case 'H': s += "Heartbeat";             break;
                    case 'V': s += "Verify comms";          break;
                    case '+': s += "Ger version";           break;

                    case 'E': s += "EMP";                   break;
                    case 'T': s += "Turret";                break;
                    case 'U': s += "LB/RB up";              break;

                    case 'R': s += "Inc RB sweep (+1)";     break;
                    case 'r': s += "Dec RB sweep (-1)";     break;
                    case 'L': s += "Inc LB sweep (+1)";     break;
                    case 'l': s += "Dec LB sweep (-1)";     break;
                    case 'X': s += "Reset LB/RB offsets";   break;

                    case '0': s += "Set LB long press";     break;
                    case '1': s += "Set LB short press";    break;
                    case '2': s += "Set RB long press";     break;
                    case '3': s += "Set RB short press";    break;

                    case '4': s += "Set LB to no press";    break;
                    case '5': s += "Set RB to no press";    break;

                    case '8': s += "Turn off LB no press";  break;
                    case '9': s += "Turn off RB no press";  break;

                    case '~': s += "0x timer offset";       break;
                    case '!': s += "1x timer offset";       break;
                    case '@': s += "2x timer offset";       break;
                    case '#': s += "3x timer offset";       break;
                    case '$': s += "4x timer offset";       break;
                    case '%': s += "5x timer offset";       break;
                    case '^': s += "6x timer offset";       break;
                    case '&': s += "7x timer offset";       break;
                    case '*': s += "8x timer offset";       break;

                    default: s += "!!Unknown command!!";    break;
                }    
            }

            // text for log file entry
            string entry = dts + " " + s;
            
            // log filename
            string sLogFile = $"{_sLogFilePath}\\DivGrind-{dt:yyyyMMMdd}.log";

            // log to file
            try
            {
                _logQueue.Enqueue2(entry);

                using (StreamWriter w = File.AppendText(sLogFile))
                {
                    w.WriteLine(entry);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceWarning($"EXCEPTION: {ex.Message}");
                // keep on chugging, yes, I know it's bad form to swallow exceptions
            }
        }

        // uploads the last log N-entries to Azure every few secs
        private void UploadLogs()
        {
            Trace.TraceInformation("UploadLogs()");

            // if no log upload UI is set, then don't attempt to upload the data
            if (string.IsNullOrEmpty(_cfg.LogUri))
                return;

            Trace.Indent();
            Trace.TraceInformation("Building log data series to send to Azure");

            // title for the log collection
            // by default this is the machine name or whatever was passed in on the command-line using -n
            var title = _cfg.MachineName;

            // build the packet of data that goes to Azure
            var sb = new StringBuilder(512);
            const string delim = "|";

            sb.Append(title);
            sb.Append(delim);

            var curTimeZone = TimeZoneInfo.Local.BaseUtcOffset.Hours;

            sb.Append("Last update ").Append(DateTime.Now.ToString(DateTemplate)).Append(" (UTC").Append(curTimeZone).Append("), using ");
            sb.Append(_fUsingLiveScreen ? "camera." : "timer.");
            sb.Append(delim);

            // loop through each log entry, add to the structure to send to Azure and remove from the queue
            Trace.TraceInformation("Writing each entry");
            while (_logQueue.Count > 0)
            {
                sb.Append(_logQueue.Dequeue());
                sb.Append(delim);
            }

            // push up to an Azure Function
            Trace.TraceInformation("Sending logs");
            try
            {
                WebClient wc = new WebClient();
                wc.Headers.Add("user-agent", "DivGrind C# Client");
                wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                wc.UploadString(_cfg.LogUri, sb.ToString());
                wc.Dispose();
            } catch (Exception ex)
            {
                WriteLog($"EXCEPTION: Error uploading to Azure {ex.Message}.");
                Trace.TraceWarning($"EXCEPTION: Error uploading to Azure {ex.Message}.");
            }

            Trace.Unindent();
            Trace.TraceInformation("Logs sent");
        }
        #endregion

        #region Arduino Interface

        // Send command to the Arduino over the COM port (ie; USB port)
        // The USB port is treated as a COM port
        // R - increase RB sweep (+/- 5)
        // r - decrease RB sweep (+/- 5)
        // L - increase LB sweep (+/- 5)
        // l - decrease LB sweep (+/- 5)
        // X - all trigger sweeps reset to 0
        // E - deploy EMP
        // T - deploy Turret
        // U - all triggers up
        // V - verify comms
        // H - heartbeat - notifies the Arduino that this code is alive
        // + - get Arduino code version
        // 0 - Set LB to short press (default)
        // 1 - set LB to long press
        // 2 - set RB to short press
        // 3 - set RB to long press (default)
        // 4 - set LB to no press
        // 5 - set RB to no press
        // 8 - turns off LB no press
        // 9 - turns off RB no press
        private void TriggerArduino(string msg)
        {
            Trace.TraceInformation($"TriggerArduino() -> {msg}");

            WriteLog(msg);

            if (_sComPort == null)
            {
                WriteLog("FATAL: COM Port is not set.");
                return;
            }       

            if (!_sComPort.IsOpen)
            { 
                WriteLog("FATAL: COM Port is not open.");
                return;
            }       

            try
            {
                _sComPort.Write(msg);
                Thread.Sleep(50);
            }
            catch (Exception ex)
            {
                WriteLog($"EXCEPTION: Arduino COM error {ex.Message}.");
            }
        }

        private void DeploySkill(Object source, ElapsedEventArgs e)
        {
            Trace.TraceInformation("DeploySkill");

            WriteLog("Deploy turret");
            TriggerArduino("T");

            // if not use the camera, then pop the EMP, too
            if (!_fUsingLiveScreen)
            {
                WriteLog("Deploy EMP");
                TriggerArduino("E");
            }
        }

        private void SendHeartbeat(Object source, ElapsedEventArgs e)
        {
            Trace.TraceInformation("Heartbeat sent");

            WriteLog("Heartbeat sent");
            TriggerArduino("H");

            // timer to display a heart on the screen for 6 frames
            _heartBeatSent = 6;
        }
        #endregion

        #region Secondary Thread Functions
        // Press the turret ever 15secs
        // if the screen is blank, then hit the EMP too
        private void SetSkillTimer()
        {
            Trace.TraceInformation("SetSkillTimer");

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
            Trace.TraceInformation("KillSkillTimer");

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
            Trace.TraceInformation("SetHeartBeat");

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
            Trace.TraceInformation("StopHeartbeat");
            if (_heartbeatTimer != null)
            {
                _heartbeatTimer.Stop();
                _heartbeatTimer = null;
            }
        }

        // Code to ping the local gateway and ubisoft every 30secs
        private void PingerThreadFunc()
        {
            bool fPingProcessFailed = false;

            Trace.TraceInformation("PingerThreadFunc start");
            Thread.Sleep(_threadStartDelay);

            while (!_fKillThreads)
            {
                Trace.TraceInformation("PingerThreadFunc main loop");

                // if there's no gateway IP address, then use tracert to get it
                // unless we have been here before and it failed - so don't keep trying!
                if (!fPingProcessFailed && String.IsNullOrEmpty(_gatewayIp))
                {
                    Trace.TraceInformation("PingerThreadFunc -> getting trace route etc and setting up");

                    try
                    {
                        Process p = new Process
                        {
                            StartInfo =
                            {
                                UseShellExecute = false,
                                RedirectStandardOutput = true,
                                FileName = "tracert",
                                CreateNoWindow = true,
                                Arguments = "-h 2 -d ubisoft.com",
                                WindowStyle = ProcessWindowStyle.Hidden
                            }
                        };

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
                    } catch (Exception ex)
                    {
                        Trace.TraceWarning($"ERROR: Creating tracert etc {ex.Message}");
                        WriteLog($"ERROR: ping {ex.Message}");

                        fPingProcessFailed = true;
                    }
                } 
                else 
                {
                    try
                    {
                        Trace.TraceInformation($"PingerThreadFunc -> ping {_gatewayIp}");

                        // we have a gateway IP address
                        Ping pingSender = new Ping();
                        PingReply replyGateway = pingSender.Send(_gatewayIp, 1000);
                        WriteLog(replyGateway == null
                            ? $"Ping: {_gatewayIp} failed, and returned NULL"
                            : $"Ping: {_gatewayIp}  {replyGateway.Status} {replyGateway.RoundtripTime}ms");

                        Trace.TraceInformation("PingerThreadFunc -> ping ubisoft");
                        PingReply replyRemote = pingSender.Send("ubisoft.com", 10000);
                        WriteLog(replyRemote == null
                            ? "Ping: ubisoft.com failed, and returned NULL"
                            : $"Ping: ubisoft.com {replyRemote.Status} {replyRemote.RoundtripTime}ms");
                    } 
                    catch (Exception ex)
                    {
                        Trace.TraceWarning($"ERROR: ping {ex.Message}");
                        WriteLog($"ERROR: ping {ex.Message}");
                    }
                }

                SpinDelay(30);
            }
        }

        // a thread function that uploads log data to Azure
        private void UploadLogThreadFunc()
        {
            Trace.TraceInformation("UploadThreadFunc");
            Thread.Sleep(_threadStartDelay);

            while (!_fKillThreads)
            {
                UploadLogs();
                SpinDelay(60);
            }
        }

        // starts all the worker threads
        private void StartAllThreads()
        {
            Trace.TraceInformation("StartAllThreads");

            _fKillThreads = false;

            _threadWorker = new Thread(WorkerThreadFunc);
            _threadWorker.Start();

            _threadLog = new Thread(UploadLogThreadFunc);
            _threadLog.Start();

            _threadPinger = new Thread(PingerThreadFunc);
            _threadPinger.Start();
        }

        #endregion

        #region UI Elements

        private void btnTestSms_Click(object sender, EventArgs e)
        {
            if (_smsAlert == null)
                MessageBox.Show("No SMS Client is defined.");
            else
                if (!_smsAlert.RaiseAlert($"Gen2 DivGrind Test from {_smsAlert.MachineName} [Time:{DateTime.Now}]"))
                    WriteLog("SMS test alert failed");
        }

        // start a timer that creates screen shots
        private void btnTrace_Click(object sender, EventArgs e)
        {
            _startTraceTimer = DateTime.Now;
            btnTrace.Enabled = false;
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
            // If the code is alive, but not actively engaged, it'll tell the Arduino it's alive
            // otherwise the Arduino will go into failsafe mode
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

        // this sets the RB and LB offset tooltip text
        private void UpdateToolTipLbRbData()
        {
            var ttt = $"LB Offset: {_cfg.LBOffset}\nRB Offset: {_cfg.RBOffset}";
            tpTooltip.SetToolTip(btnRecalLeftLess, ttt);
            tpTooltip.SetToolTip(btnRecalLeftMore, ttt);
            tpTooltip.SetToolTip(btnRecalRightLess, ttt);
            tpTooltip.SetToolTip(btnRecalRightMore, ttt);

            WriteLog("Updating RB/LB offsets");
            
            WriteConfig(_cfg);
        }

        // these are used to send discrete commands to the Arduino
        private void btnRecalLeftLess_Click(object sender, EventArgs e) { TriggerArduino("l"); _cfg.LBOffset--; UpdateToolTipLbRbData(); } // Reduce offset on RB
        private void btnRecalLeftMore_Click(object sender, EventArgs e) { TriggerArduino("L"); _cfg.LBOffset++; UpdateToolTipLbRbData(); } // Add more offset to LB
        private void btnRecalRightLess_Click(object sender, EventArgs e){ TriggerArduino("r"); _cfg.RBOffset--; UpdateToolTipLbRbData(); } // Reduce offset on RB
        private void btnRecalRightMore_Click(object sender, EventArgs e){ TriggerArduino("R"); _cfg.RBOffset++; UpdateToolTipLbRbData(); } // Add more offset to RB

        private void btnAllUp_Click(object sender, EventArgs e)         { TriggerArduino("U"); } // raise all servos
        private void button3_Click_1(object sender, EventArgs e)        { TriggerArduino("E"); } // EMP
        private void button2_Click_1(object sender, EventArgs e)        { TriggerArduino("T"); } // Turret
        private void btnResetOffsets_Click(object sender, EventArgs e)  { TriggerArduino("X"); } // Resets the LB/RB offset adjustments
        private void lblVersionInfo_Click(object sender, EventArgs e)   { SetStatusBar(); }

        // set LB/RB long/short press
        private void radLBLongPress_CheckedChanged(object sender, EventArgs e)
        {
            _bLBLongPress = true;
            TriggerArduino("0");
            TriggerArduino("8"); // turns off NO Press LB
        }

        private void radLBShortPress_CheckedChanged(object sender, EventArgs e)
        {
            _bLBLongPress = false;
            TriggerArduino("1");
            TriggerArduino("8"); // turns off NO Press LB
        }

        private void radRBLongPress_CheckedChanged(object sender, EventArgs e)
        {
            _bRBLongPress = true;
            TriggerArduino("2");
            TriggerArduino("9"); // turns off NO Press RB
        }

        private void rabRBShortPress_CheckedChanged(object sender, EventArgs e)
        {
            _bRBLongPress = false;
            TriggerArduino("3");
            TriggerArduino("9"); // turns off NO Press RB
        }

        private void radLBNoPress_CheckedChanged(object sender, EventArgs e) => TriggerArduino("4");
        private void radRBNoPress_CheckedChanged(object sender, EventArgs e) => TriggerArduino("5");

        // does nothing, sets no state - other code reads this UI element directly
        private void txtSmsEnabled_Click(object sender, EventArgs e) {}

        private void numLongDelayOffset_ValueChanged(object sender, EventArgs e)
        {
            switch (numLongDelayOffset.Value)
            {
                case 0: TriggerArduino("~"); break;
                case 1: TriggerArduino("!"); break;
                case 2: TriggerArduino("@"); break;
                case 3: TriggerArduino("#"); break;
                case 4: TriggerArduino("$"); break;
                case 5: TriggerArduino("%"); break;
                case 6: TriggerArduino("^"); break;
                case 7: TriggerArduino("&"); break;
                case 8: TriggerArduino("*"); break;
            }
        }

        // when the name of the agent changes, update the memory-mapped data so it can be read by the camera app
        private void txtName_TextChanged(object sender, EventArgs e)
        {
            _mmio?.Write(txtName.Text);
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

            // Get RGB calibration data from the hitbox
            var rbgTotal = new RgbTotal();
            GetRgbInRange(bmp, ref rbgTotal);

            _cfg.LastCalibratedR = (int)rbgTotal.R;
            _cfg.LastCalibratedB = (int)rbgTotal.B;
            _cfg.LastCalibratedG = (int)rbgTotal.G;
            WriteConfig(_cfg);

            // draw hit box
            DrawTargetRange(bmp);
            pictCamera.Image = bmp;
        }

        #endregion

        #region Bitmap and drone detection code

        // determines the increase in red required to determine if the drones are incoming
        private float GetRedSpottedPercent() => _cfg.LastCalibratedR + ((_cfg.LastCalibratedR / 100.0F) * _cfg.ThreshHold);
        private void label8_Click(object sender, EventArgs e) {}
        private void numDroneDelay_ValueChanged(object sender, EventArgs e) => _elapseBetweenDrones = new TimeSpan(0, 0, (int)numDroneDelay.Value);

        // logic to determine if drones are coming
        // if there is no increase in red, then no drones
        private bool DronesSpotted(ref RgbTotal rbgTotal) => rbgTotal.R > GetRedSpottedPercent();

        // draws the yellow rectangle 'hitbox' -
        // this is the area the code looks at for the increase in red
        // that indicates the drones are incoming
        private void DrawTargetRange(Bitmap bmp)
        {
            Trace.TraceInformation("DrawTargetRange (hitbox)");
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.DrawRectangle(_penHitBox, _rectDroneHitBox);
            }
        }

        // counts the number of RGB elements in pixels in the hitbox
        // skips every other pixel on the x-axis for perf
        private void GetRgbInRange(Bitmap bmp, ref RgbTotal rbgTotal)
        {
            Trace.TraceInformation("GetRGBInRange");

            rbgTotal.Init();
            Int32 countPixel = 0;

            for (int x = XDroneHitBoxStart; x < XDroneHitBoxEnd; x+=2)
            {
                for (int y = YDroneHitBoxStart; y < YDroneHitBoxEnd; y++)
                {
                    Color px = bmp.GetPixel(x, y);
                    countPixel++;

                    // RGB totals
                    rbgTotal.R += px.R; 
                    rbgTotal.G += px.G;
                    rbgTotal.B += px.B;
                }
            }

            rbgTotal.R /= countPixel;
            rbgTotal.G /= countPixel;
            rbgTotal.B /= countPixel;
        }

        // overload
        // counts the number of RGB elements in pixels in the hitbox
        // skips every other pixel on the x-axis for perf
        private void GetRgbInRange(Bitmap bmp, int xRect, int yRect, int width, int height, ref RgbTotal rbgTotal, ref Color mainColor)
        {
            Trace.TraceInformation("GetRGBInRange (overload)");

            rbgTotal.Init();
            Int32 countPixel = 0;

            Dictionary<Color, int> dictColor = new Dictionary<Color, int>();

            for (int x = xRect; x < xRect + width; x += 2)
            {
                for (int y = yRect; y < yRect + height; y++)
                {
                    Color px = bmp.GetPixel(x, y);
                    countPixel++;

                    // RGB totals
                    rbgTotal.R += px.R;
                    rbgTotal.G += px.G;
                    rbgTotal.B += px.B;

                    // this keeps track of the count of the closest colors per pixel
                    Color col = RgbToClosest.GetClosestColorFromRgb(px.R, px.G, px.B);
                    if (dictColor.ContainsKey(col))
                    {
                        if (dictColor.TryGetValue(col, out int count))
                            dictColor[col] = ++count;
                    }
                    else
                    {
                        dictColor.Add(col, 1);
                    }
                }
            }

            rbgTotal.R /= countPixel;
            rbgTotal.G /= countPixel;
            rbgTotal.B /= countPixel;

            // get the highest color count
            Color highestColor = Color.Transparent;
            const int highestCount = -1;
            foreach (var d in dictColor)
            {
                if (d.Value > highestCount)
                {
                    highestColor = d.Key;
                }
            }

            mainColor = highestColor;
        }

        #endregion

    }
}
