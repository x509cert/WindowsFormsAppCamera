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

namespace WindowsFormsAppCamera
{
    public partial class Form1 : Form
    {
        UsbCamera               _camera;
        readonly Queue<Int64>   _redCalibrationData = null;
        Int64                   _redCalibrationAvg;
        readonly Int64          _redTriggerPercent = 33;
        readonly int            _xHitBoxStart = 200, 
                                _yHitBoxStart = 200, 
                                _xHitBoxEnd = 460, 
                                _yHitBoxEnd = 270; // this is the hit box rectangle
        Thread                  _thread = null;
        bool                    _fKillThread = false;
        System.Timers.Timer     _skillTimer = null;
        readonly string         _sLogFilePath;
        bool                    _fUsingLiveScreen = true;
        readonly TimeSpan       _elapseBetweenDrones = new TimeSpan(0, 0, 9);
        Brush                   _colorInfo = Brushes.AliceBlue;

        public Form1()
        {
            InitializeComponent();

            _redCalibrationData = new Queue<Int64>();
            _sLogFilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            this.Text = "Sneaky's DivGrind [Last Built " + GetBuildDate() + "]";
        }

        private string GetBuildDate()
        {
            string strpath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            System.IO.FileInfo fi = new System.IO.FileInfo(strpath);
            return fi.LastWriteTime.ToString();
        }

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
                }
            }
            catch (Exception)
            {
                // keep on chugging
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (_redCalibrationData.Count > 0)
                _redCalibrationData.Dequeue();

            Int64 totalRed = 0L;
            int count = 0;
            foreach (Int64 red in _redCalibrationData)
            {
                if (red > 0)
                {
                    count++;
                    totalRed += red;
                }
            }

            if (count > 0)
                _redCalibrationAvg = totalRed / count;
            else
                _redCalibrationAvg = 0;

            lblRedAvg.Text = _redCalibrationAvg.ToString();
        }

        bool DronesSpotted(int totalR)
        {
            long spotted = _redCalibrationAvg + ((_redCalibrationAvg / 100) * _redTriggerPercent);
            return totalR > spotted;
        }

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
                _skillTimer.Stop();
        }

        // starts a new thread that does the core work, 
        // this is so we don't use the UI thread for the work which would make the UI sluggish
        private void WorkerThreadFunc()
        {
            DateTime dtDronesStart = DateTime.Now;

            bool fDronesIncoming = false;
            Int64 _showDroneText = 0;
            var sb = new StringBuilder(24);

            WriteLog("Worker thread start");

            SetSkillTimer();

            while (!_fKillThread)
            {
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
                    gd.SmoothingMode = SmoothingMode.HighSpeed;

                    // Write amount of red in the bmp
                    GetRGBAInRange(bmp, out Int32 totalR, out Int32 totalG, out Int32 totalB, out Int32 totalA);
                    int percentAbove = (int)(((float)totalR / (float)_redCalibrationAvg) * 100);
                    var rectRed = new Rectangle(0, bmp.Height - 24, bmp.Width, 24);

                    sb.Clear();
                    sb.Append("Red: ");
                    sb.Append(totalR.ToString("N0"));
                    sb.Append("(");
                    sb.Append(percentAbove);
                    sb.Append("%)");
                    gd.DrawString(sb.ToString(), new Font("Tahoma", 14), _colorInfo, rectRed);

                    // Write elapsed time to next drone check
                    Rectangle rectCheck = new Rectangle(0, bmp.Height - 56, bmp.Width, 24);
                    gd.DrawString(droneCooldown, new Font("Tahoma", 14), _colorInfo, rectCheck);

                    // if drones spotted and not on drone-check-coodown then trigger the Arduino to hold EMP pulse
                    // start the countdown for displaying the "incoming text"
                    if (!fDronesIncoming && DronesSpotted(totalR))
                    {
                        WriteLog("Drones detected");
                        TriggerArduino("E");

                        dtDronesStart = DateTime.Now;
                        fDronesIncoming = true;
                        _showDroneText = 12; // display the drone text for 12 frames
                    }

                    // display the "Incoming text" - this is written the the image
                    if (_showDroneText > 0)
                    {
                        Rectangle rectDrone = new Rectangle(200, bmp.Height - 100, bmp.Width, 100);
                        gd.DrawString("Drones Incoming", new Font("Tahoma", 30), Brushes.Firebrick, rectDrone);

                        _showDroneText--;
                    }

                    // write the camera image + text etc to the UI
                    DrawTargetRange(bmp);
                    pictCamera.Image = bmp;
                }
                else
                {
                    Bitmap bmp = new Bitmap(640, 480);
                    Graphics gr = Graphics.FromImage(bmp);
                    gr.FillRectangle(Brushes.Black, 0, 0, 640, 480);
                    pictCamera.Image = bmp;
                }

                Thread.Sleep(250);
            }

            KillSkillTimer();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = false;
            btnStop.Enabled = true;

            cmbCamera.Enabled = false;
            cmbCameraFormat.Enabled = false;
            cmbComPorts.Enabled = false;

            btnTestComPort.Enabled = false;

            _fKillThread = false;
            _thread = new Thread(WorkerThreadFunc);
            _thread.Start();
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
            _fKillThread = true;
            _skillTimer.Stop();
        }

        private void btnEraseCalibrate_Click(object sender, EventArgs e)
        {
            _redCalibrationAvg = 0L;
            _redCalibrationData.Clear();
            lblRedCount.Text = "0";
            lblRedAvg.Text = "0";
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
            // create usb camera and start.
            int camera = cmbCamera.SelectedIndex;
            UsbCamera.VideoFormat[] formats = UsbCamera.GetVideoFormat(camera);
            _camera = new UsbCamera(camera, formats[cmbCameraFormat.SelectedIndex]);
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

        private void Form1_Load(object sender, EventArgs e)
        {
            string[] devices = UsbCamera.FindDevices();
            if (devices.Length == 0)
            {
                MessageBox.Show("No Camera");
                return; // no camera.
            }

            foreach (string d in devices)
            {
                cmbCamera.Items.Add(d);
            }
        }

        // counts the number of RBGA elements in pixels in the hitbox
        // TODO: Will this work if do every other pixel?
        private void GetRGBAInRange(Bitmap bmp,
                                out Int32 totalR,
                                out Int32 totalG,
                                out Int32 totalB,
                                out Int32 totalA)
        {
            totalR = totalG = totalB = totalA = 0;
            for (int x = _xHitBoxStart; x < _xHitBoxEnd; x++)
            {
                for (int y = _yHitBoxStart; y < _yHitBoxEnd; y++)
                {
                    Color px = bmp.GetPixel(x, y);

                    totalR += (Int32)px.R;
                    totalG += (Int32)px.G;
                    totalB += (Int32)px.B;
                    totalA += (Int32)px.A;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var bmp = _camera.GetBitmap();

            // Get Calibration data
            GetRGBAInRange(bmp, out Int32 totalR, out Int32 totalG, out Int32 totalB, out Int32 totalA);
            lblRedCount.Text = totalR.ToString("N0");
            _redCalibrationData.Enqueue(totalR);

            // draw yellow hit box
            DrawTargetRange(bmp);
            pictCamera.Image = bmp;

            // count reds in the queue
            Int64 totalRed = 0L;
            int count = 0;
            foreach (Int64 red in _redCalibrationData)
            {
                if (red > 0)
                {
                    count++;
                    totalRed += red;
                }
            }

            _redCalibrationAvg = totalRed / count;
            lblRedAvg.Text = _redCalibrationAvg.ToString("N0");
        }
    }
}
