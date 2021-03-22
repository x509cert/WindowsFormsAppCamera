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
using Azure.Storage;

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
            private int _max;

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
        UsbCamera _camera;
        RGBTotal                _calibrationAvg;
        LogQueue                _logQueue;
        float                   _triggerPercent = 65F;
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
        readonly TimeSpan       _longestTimeBetweenDrones = new TimeSpan(0, 0, 22); // longest time we can go without seeing a drone, used to send out an emergency EMP
        Brush                   _colorInfo = Brushes.Black;

        #endregion

        private string GetBuildDate()
        {
            string strpath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            System.IO.FileInfo fi = new System.IO.FileInfo(strpath);
            return fi.LastWriteTime.ToString();
        }

        #region Logging
        private void WriteLog(string s)
        {
            DateTime dt = DateTime.Now;
            var dts = dt.ToString("yyyy MMM dd, HH:mm:ss");

            if (s.Length == 1)
                s += ": Comms to Arduino";

            string entry = dts + ", " + s;
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
        #endregion

        #region Arduino Interface

        // Send command to the Arduino over the COM port
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
            catch (Exception)
            {
                WriteLog("EXCEPTION: Serial port not open");
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

        // a new thread that uploads log data to Azure
        private void UploadLogThreadFunc()
        {
            while (!_fKillThreads)
            {

                Thread.Sleep(20000); // delay 20secs
            }
        }

        // starts a new thread that does the core work, 
        // this is so we don't use the UI thread for the work which would make the UI sluggish
        private void WorkerThreadFunc()
        {
            var dtDronesStart = DateTime.Now;
            DateTime dtLastDroneSpotted = DateTime.Now;

            bool fDronesIncoming = false;
            Int64 _showDroneText = 0;
            var sb = new StringBuilder(24);

            WriteLog("Worker thread start");

            SetSkillTimer();

            while (!_fKillThreads)
            {
                // need to check that drones have been spotted and if not, throw out the EMP and deploy the turret
                TimeSpan tSpan = DateTime.Now - dtLastDroneSpotted;
                if (tSpan > _longestTimeBetweenDrones)
                {
                    WriteLog("Last drone seen: " + tSpan.TotalSeconds + "s ago");
                    TriggerArduino("E");
                    TriggerArduino("T");

                    dtLastDroneSpotted = DateTime.Now;
                }

                if (_fUsingLiveScreen)
                {
                    string droneCooldown = "Drone check: Ready";

                    // this stops the code from checking for drones constantly
                    if (fDronesIncoming)
                    {
                        TimeSpan elapsedTime = DateTime.Now - dtDronesStart;
                        if (elapsedTime > _elapseBetweenDrones)
                        {
                            WriteLog("Drone scan timeout completed");
                            fDronesIncoming = false;
                        }

                        Int32 elapsed = (Int32)(_elapseBetweenDrones.TotalSeconds - elapsedTime.TotalSeconds);
                        droneCooldown = "Drone check: " + elapsed.ToString("N0") + "s";
                    }

                    // get the image from the camera
                    var bmp = _camera.GetBitmap();
                    Graphics gd = Graphics.FromImage(bmp);

                    // draw a rectangle for the text
                    gd.FillRectangle(Brushes.DarkBlue, 2, 480-98, 640/3, 480-2);
                    gd.SmoothingMode = SmoothingMode.HighSpeed;

                    // Write amount of red in the bmp
                    RGBTotal rbgTotal = new RGBTotal() ;
                    GetRGBInRange(bmp, ref rbgTotal);

                    const int X = 4;

                    int percentChange = (int)(((float)rbgTotal.R / (float)_calibrationAvg.R) * 100);
                    string r = $"R: {rbgTotal.R:N0} ({percentChange}%)";
                    gd.DrawString(r, new Font("Tahoma", 14), _colorInfo, new Rectangle(X, bmp.Height - 70, bmp.Width, 24));

                    percentChange = (int)(((float)rbgTotal.G / (float)_calibrationAvg.G) * 100);
                    string g = $"G: {rbgTotal.G:N0} ({percentChange}%)";
                    gd.DrawString(g, new Font("Tahoma", 14), _colorInfo, new Rectangle(X, bmp.Height - 48, bmp.Width, 24));

                    percentChange = (int)(((float)rbgTotal.B / (float)_calibrationAvg.B) * 100);
                    string b = $"B: {rbgTotal.B:N0} ({percentChange}%)";
                    gd.DrawString(b, new Font("Tahoma", 14), _colorInfo, new Rectangle(X, bmp.Height - 24, bmp.Width, 24));

                    // Write elapsed time to next drone check
                    gd.DrawString(droneCooldown, new Font("Tahoma", 14), _colorInfo, new Rectangle(X, bmp.Height - 100, bmp.Width, 24));

                    // if drones spotted and not on drone-check-cooldown then trigger the Arduino to hold EMP pulse
                    // start the countdown for displaying the "incoming text"
                    if (!fDronesIncoming && DronesSpotted(ref rbgTotal))
                    {
                        dtLastDroneSpotted = DateTime.Now;

                        WriteLog("Drones detected");
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
                    // when running no camera mode, uses a black screen
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
        public Form1()
        {
            InitializeComponent();

            _sLogFilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            this.Text = "Sneaky's DivGrind [Last Built " + GetBuildDate() + "]";

            numTrigger.Value = (decimal)_triggerPercent;
        }
        private void btnStart_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = false;
            btnStop.Enabled = true;

            cmbCamera.Enabled = false;
            cmbCameraFormat.Enabled = false;
            cmbComPorts.Enabled = false;

            btnTestComPort.Enabled = false;

            // start the threads
            _fKillThreads = false;
            _threadWorker = new Thread(WorkerThreadFunc);
            _threadWorker.Start();

            _threadLog = new Thread(UploadLogThreadFunc);
            _threadLog.Start();
        }

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

        private void btnSaveBmp_Click(object sender, EventArgs e)
        {
            Bitmap bmp = _camera.GetBitmap();
            var date1 = DateTime.Now;
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var dtFormat = date1.ToString("MMddTHH-mmss");
            bmp.Save(folder + @"\Div" + dtFormat + ".bmp");
        }

        private void btnToggleBlankOrLiveScreen_Click(object sender, EventArgs e)
        {
            WriteLog(_fUsingLiveScreen ? "Flipping to Blank" : "Flipping to Live");
            btnToggleBlankOrLiveScreen.Text = (_fUsingLiveScreen) ? "Flip to Live" : "Flip to Blank";
            _fUsingLiveScreen = !_fUsingLiveScreen;
        }

        private void btnRecalLeftLess_Click(object sender, EventArgs e)
        {
            TriggerArduino("l");
        }

        private void btnRecalLeftMore_Click(object sender, EventArgs e)
        {
            TriggerArduino("L");
        }

        private void btnRecalRightLess_Click(object sender, EventArgs e)
        {
            TriggerArduino("r");
        }

        private void btnRecalRightMore_Click(object sender, EventArgs e)
        {
            TriggerArduino("R");
        }

        private void btnAllUp_Click(object sender, EventArgs e)
        {
            TriggerArduino("U");
        }

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

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnTestComPort.Enabled = true;
        }

        private void btnTestComPort_Click(object sender, EventArgs e)
        {
            cmbComPorts.Items[cmbComPorts.SelectedIndex].ToString();
            TriggerArduino("V");
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            TriggerArduino("E");
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            TriggerArduino("T");
        }

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

        private void numTrigger_ValueChanged(object sender, EventArgs e)
        {
            _triggerPercent = (float)numTrigger.Value;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // read the camera
            var bmp = _camera.GetBitmap();

            _calibrationAvg.Init();

            // Get Calibration data
            var rbgTotal = new RGBTotal();
            GetRGBInRange(bmp, ref rbgTotal);

            lblRedCount.Text = rbgTotal.R.ToString("N0");
            lblGreenCount.Text = rbgTotal.G.ToString("N0");
            lblBlueCount.Text = rbgTotal.B.ToString("N0");

            _calibrationAvg.R = rbgTotal.R;
            _calibrationAvg.B = rbgTotal.B;
            _calibrationAvg.G = rbgTotal.G;

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
        // logic to determine if drones are coming - need to use floats owing to small numbers (0..255)
        bool DronesSpotted(ref RGBTotal rbgTotal)
        {
            // if there is no increase in red, then no drones
            float spottedRed = (float)_calibrationAvg.R + (((float)_calibrationAvg.R / 100.0F) * _triggerPercent);
            if (rbgTotal.R <= spottedRed)
                return false;

            return true;

            // if there is also an increase in blue and green, then it's an EMP flash
            //float spottedGreen = (float)_calibrationAvg.G + (((float)_calibrationAvg.G / 100.0F) * _triggerPercent);
            //float spottedBlue = (float)_calibrationAvg.B + (((float)_calibrationAvg.B / 100.0F) * _triggerPercent);

            //return rbgTotal.G < spottedGreen || rbgTotal.B < spottedBlue;
        }

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
