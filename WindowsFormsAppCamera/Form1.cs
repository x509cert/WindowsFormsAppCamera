using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using System.Net;

using Azure;
using Azure.Communication;
using Azure.Communication.Sms;

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

        // sends SMS alerts if drones not seen - usually indicates agent death or delta
        class SmsAlert
        {
            private string _machineName, _connectionString, _smsFrom, _smsTo;
            private SmsClient _smsClient;

            public string MachineName { get => _machineName; set => _machineName = value; }
            public string ConnectionString { get => _connectionString; set => _connectionString = value; }
            public string SmsFrom { get => _smsFrom; set => _smsFrom = value; }
            public string SmsTo { get => _smsTo; set => _smsTo = value; }

            public SmsAlert() { }
            public SmsAlert(string machineName, string connectionString, string smsFrom, string smsTo)
            {
                MachineName = machineName;
                ConnectionString = connectionString;
                SmsFrom = smsFrom;
                SmsTo = smsTo;

                _smsClient = new SmsClient(ConnectionString);
            }

            // send a message to the recipient
            public bool RaiseAlert(string msg)
            {
                if (_smsClient == null)
                    return false;

                SmsSendResult sendResult = _smsClient.Send(
                    from: SmsFrom,
                    to: SmsTo,
                    message: msg
                );

                MessageBox.Show(sendResult.Successful ? "Message sent" : sendResult.ErrorMessage);

                return sendResult.Successful;
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
        RGBTotal                _calibrationData;
        readonly LogQueue       _logQueue;
        float                   _triggerPercent = 55F;
        readonly int            _xHitBoxStart = 200, 
                                _yHitBoxStart = 200, 
                                _xHitBoxEnd = 460, 
                                _yHitBoxEnd = 270; // this is the hit box rectangle
        Thread                  _threadWorker = null;
        Thread                  _threadLog = null;
        bool                    _fKillThreads = false;
        System.Timers.Timer     _skillTimer = null;
        readonly string         _sLogFilePath;
        bool                    _fUsingLiveScreen = true;
        readonly TimeSpan       _elapseBetweenDrones = new TimeSpan(0, 0, 9);       // cooldown before we look for drones after detected
        readonly TimeSpan       _longestTimeBetweenDrones = new TimeSpan(0, 0, 27); // longest time we can go without seeing a drone, used to send out an emergency EMP
        Brush                   _colorInfo = Brushes.AliceBlue;
        const string            _dateTemplate = "yyyy MMM dd, HH:mm:ss";
        string                  _machineName;
        SmsAlert                _smsAlert = null;
        DateTime                _lastSmsMessageSent;    // Keep track of when the last SMS alert was sent
        readonly TimeSpan       _lastSmsElapseBetweenMessages = new TimeSpan(0, 30, 0); // wait 30mins between SMS messages

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
            // URL to the Azure Function TODO: Pull URL from commandline
            var uri = "https://divgrind.azurewebsites.net/api/DivGrindLog?verb=u";

            // title for the log collection
            // by default this is the machine name or whatever was passed in on the command-line using -n
            var title = _machineName;

            // build the packet of data that goes to Azure
            var sb = new StringBuilder(512);
            var delim = "|";

            sb.Append(title);
            sb.Append(delim);
            sb.Append("Last update (UTC): " + DateTime.UtcNow.ToString(_dateTemplate));
            sb.Append(delim);

            // loop through each log entry, add to the structure to send to Azure and remove from the queue
            while (_logQueue.Count > 0)
            {
                sb.Append(_logQueue.Dequeue());
                sb.Append(delim);
            }

            // push up to Azure TODO: Make the URI a cmd-line argument
            try
            {
                WebClient wc = new WebClient();
                wc.Headers.Add("user-agent", "DivGrind C# Client");
                wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";

                _ = wc.UploadString(uri, sb.ToString());
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

            try
            {
                string s = cmbComPorts.SelectedItem.ToString();
                SerialPort port = new SerialPort(s, 9600);
                port.Open();
                port.Write(msg);
                Thread.Sleep(200);
                port.Close();
            }
            catch (Exception ex)
            {
                WriteLog($"EXCEPTION: Arduino COM port not open {ex.Message}.");
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

        #region Thread Functions
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

        // a thread function that uploads log data to Azure
        private void UploadLogThreadFunc()
        {
            while (!_fKillThreads)
            {
                UploadLogs();
                Thread.Sleep(55000); // delay 55 secs, no real reason for this number!
            }
        }

        // a new thread function that does the core work, 
        // this is so we don't use the UI thread for the work which would make the UI sluggish
        private void WorkerThreadFunc()
        {
            var dtDronesStart = DateTime.Now;
            DateTime dtLastDroneSpotted = DateTime.Now;

            bool fDronesIncoming = false;
            Int64 _showDroneText = 0;

            WriteLog("Worker thread start");

            SetSkillTimer();

            while (!_fKillThreads)
            {
                // need to check that if drones have not been spotted for a while then
                // throw out the EMP and deploy the turret
                // this is an emergency measure
                // sends an SMS alert if one is configured
                TimeSpan tSpan = DateTime.Now - dtLastDroneSpotted;
                if (tSpan > _longestTimeBetweenDrones)
                {
                    WriteLog("Last drone seen: " + tSpan.TotalSeconds.ToString("N2") + "s ago");
                    TriggerArduino("E");
                    TriggerArduino("T");

                    dtLastDroneSpotted = DateTime.Now;

                    if (_smsAlert != null)
                        if (!_smsAlert.RaiseAlert("Drones not detected"))
                            WriteLog("SMS alert failed");
                }

                // using camera
                if (_fUsingLiveScreen)
                {
                    string droneCooldown = "Drone check: Ready";

                    // this stops the code from checking for drones constantly right after drones are spotted and EMP sent out
                    if (fDronesIncoming)
                    {
                        TimeSpan elapsedTime = DateTime.Now - dtDronesStart;
                        if (elapsedTime > _elapseBetweenDrones)
                        {
                            WriteLog("Ready for next drone scan");
                            fDronesIncoming = false;
                        }

                        Int32 elapsed = (Int32)(_elapseBetweenDrones.TotalSeconds - elapsedTime.TotalSeconds);
                        droneCooldown = "Drone check: " + elapsed.ToString("N0") + "s";
                    }

                    // get the image from the camera
                    var bmp = _camera.GetBitmap();
                    Graphics gd = Graphics.FromImage(bmp);

                    // define a rectangle for the text
                    gd.FillRectangle(Brushes.DarkBlue, 2, 480-98, 640/3, 480-2);
                    gd.SmoothingMode = SmoothingMode.HighSpeed;

                    // Write amount of red/green/blue in the bmp
                    RGBTotal rbgTotal = new RGBTotal() ;
                    GetRGBInRange(bmp, ref rbgTotal);

                    const int X = 4;

                    float redSpottedValue = GetRedSpottedPercent();

                    // calcluate current RGB as discrete values and percentages and write into the bmp
                    int percentChange = (int)(rbgTotal.R / (float)_calibrationData.R * 100);
                    string wouldTrigger = redSpottedValue < percentChange ? " *" : "";
                    string r = $"R: {rbgTotal.R:N0} ({percentChange}%) {wouldTrigger}";
                    gd.DrawString(r, new Font("Tahoma", 14), _colorInfo, new Rectangle(X, bmp.Height - 70, bmp.Width, 24));

                    percentChange = (int)(rbgTotal.G / (float)_calibrationData.G * 100);
                    wouldTrigger = redSpottedValue < percentChange ? " *" : "";
                    string g = $"G: {rbgTotal.G:N0} ({percentChange}%) {wouldTrigger}";
                    gd.DrawString(g, new Font("Tahoma", 14), _colorInfo, new Rectangle(X, bmp.Height - 48, bmp.Width, 24));

                    percentChange = (int)(rbgTotal.B / (float)_calibrationData.B * 100);
                    wouldTrigger = redSpottedValue < percentChange ? " *" : "";
                    string b = $"B: {rbgTotal.B:N0} ({percentChange}%) {wouldTrigger}";
                    gd.DrawString(b, new Font("Tahoma", 14), _colorInfo, new Rectangle(X, bmp.Height - 24, bmp.Width, 24));

                    // Write elapsed time to next drone check
                    gd.DrawString(droneCooldown, new Font("Tahoma", 14), _colorInfo, new Rectangle(X, bmp.Height - 100, bmp.Width, 24));

                    // if drones spotted and not on drone-check-cooldown then trigger the Arduino to hold EMP pulse
                    // start the countdown for displaying the "incoming" text
                    if (!fDronesIncoming && DronesSpotted(ref rbgTotal))
                    {
                        dtLastDroneSpotted = DateTime.Now;

                        // send out the EMP
                        WriteLog("Drones detected -> EMP");
                        TriggerArduino("E");

                        dtDronesStart = DateTime.Now;
                        fDronesIncoming = true;
                        _showDroneText = 12; // display the drone text for 12 frames
                    }

                    // display the "Incoming text" - this is written the the image
                    if (_showDroneText > 0)
                    {
                        Rectangle rectDrone = new Rectangle(180, bmp.Height - 100, bmp.Width, 100);
                        gd.DrawString("Drones Incoming", new Font("Tahoma", 30), Brushes.Firebrick, rectDrone);

                        _showDroneText--;
                    }

                    // write the camera image + text etc to the UI
                    DrawTargetRange(bmp);
                    pictCamera.Image = bmp;
                }
                else
                {
                    // when running in no camera mode (ie; timer), uses a black screen
                    Bitmap bmp = new Bitmap(640, 480);
                    Graphics gr = Graphics.FromImage(bmp);
                    gr.FillRectangle(Brushes.Black, 0, 0, 640, 480);
                    pictCamera.Image = bmp;
                }

                Thread.Sleep(210);
            }

            KillSkillTimer();
        }
        #endregion

        #region UI Elements

        // Code to read command-line args
        // -n "machinename" -c "connectionstring" -f "from sms #"  -t "to sms #"
        bool GetCmdLineArgs(ref string machineName,         // -n
                            ref string azureCommsString,    // -c
                            ref string azureSmsFrom,        // -f
                            ref string azureSmsTo)          // -t
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
                }
            }
            catch (Exception)
            {
                success = false;
            }

            return success;
        }

        // WInForm version of main()
        public Form1()
        {
            InitializeComponent();

            // logs go in user's profile folder (eg; c:\users\mikehow)
            _sLogFilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            // get the date this binary was last created
            string strpath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            System.IO.FileInfo fi = new System.IO.FileInfo(strpath);
            string buildDate = fi.LastWriteTime.ToString();

            // machine name
            string machine = Dns.GetHostName();

            // add info to title of the tools
            Text = $"DivGrind [Last Built {buildDate}] on {machine}";

            numTrigger.Value = (decimal)_triggerPercent;

            _logQueue = new LogQueue(50);
            
            _machineName = Dns.GetHostName();

            string machineName = "";
            string azureConnection = "";
            string azureSmsFrom = "";
            string azureSmsTo = "";

            bool ok = GetCmdLineArgs(ref machineName, ref azureConnection, ref azureSmsFrom, ref azureSmsTo);
            if (ok)
            {
                // set machine name
                if (string.IsNullOrEmpty(machineName) == false) _machineName = machineName;

                // if the three args are available for SMS, then create an SmsAlert object
                if (string.IsNullOrEmpty(azureConnection) == false &&
                    string.IsNullOrEmpty(azureSmsFrom) == false &&
                    string.IsNullOrEmpty(azureSmsTo) == false)
                {
                    _smsAlert = new SmsAlert(machineName, azureConnection, azureSmsFrom, azureSmsTo);
                    txtSmsEnabled.Text = "Yes";
                }
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
            var date1 = DateTime.Now;
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var dtFormat = date1.ToString("MMddTHH-mmss");
            bmp.Save(folder + @"\Div" + dtFormat + ".bmp");
        }

        // switch between live and blank (timer) screen
        private void btnToggleBlankOrLiveScreen_Click(object sender, EventArgs e)
        {
            WriteLog(_fUsingLiveScreen ? "Flipping to Blank" : "Flipping to Live");
            btnToggleBlankOrLiveScreen.Text = (_fUsingLiveScreen) ? "Flip to Live" : "Flip to Blank";
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

            _camera = new UsbCamera(camera, selectFormat);
            _camera.Start();

            foreach (string p in SerialPort.GetPortNames())
                cmbComPorts.Items.Add(p);
        }

        // COM port selected
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnTestComPort.Enabled = true;
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
            _colorInfo = (_colorInfo == Brushes.AliceBlue) ? Brushes.Black : Brushes.AliceBlue;
        }

        // this gives the code a chance to kill the main worker thread gracefully
        private void Form1_Closing(object sender, FormClosingEventArgs e)
        {
            KillSkillTimer();
            _fKillThreads = true;

            Thread.Sleep(400);
            e.Cancel = false;
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
                _smsAlert.RaiseAlert($"Gen2 DivGrind Test from {_smsAlert.MachineName}");
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

                Color customColor = Color.FromArgb(99, Color.Yellow);
                SolidBrush brushYellow = new SolidBrush(customColor);
                g.FillRectangle(brushYellow, rectTarget);

                Pen penYellow = new Pen(Color.FromKnownColor(KnownColor.Yellow));
                g.DrawRectangle(penYellow, rectTarget);
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

                    rbgTotal.R += (Int32)px.R; 
                    rbgTotal.G += (Int32)px.G;
                    rbgTotal.B += (Int32)px.B;
                    countPixel++;
                }
            }

            rbgTotal.R /= countPixel;
            rbgTotal.G /= countPixel;
            rbgTotal.B /= countPixel;
        }

#endregion

    }
}
