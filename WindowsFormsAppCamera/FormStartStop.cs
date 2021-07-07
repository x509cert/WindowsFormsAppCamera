using System;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.IO;
using System.IO.Ports;
using System.Diagnostics;
using System.Drawing;

namespace WindowsFormsAppCamera
{
    public partial class Form1 : Form
    {
        // The main entry point
        public Form1()
        {
            InitializeComponent();
        }

        // writes version info to the status bar
        private void SetStatusBar()
        {
            // Status bar info

            // get the date this binary was last modified
            var strpath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var fi = new FileInfo(strpath);
            var buildDate = fi.LastWriteTime.ToString("dd MMMM yyyy, hh: mm tt");

            var isDebug = "";
#if DEBUG
            isDebug = "[DBG] ";
#endif

            // add info to title of the tool
            var machine = Dns.GetHostName();
            var codeVersion = $"{isDebug}[{buildDate}] [{machine}] ";
            var arduinoVerion = "?";

            if (_sComPort != null && _sComPort.IsOpen)
                arduinoVerion = "[Arduino:" + GetArduinoCodeVersion() + "]";

            lblVersionInfo.Text = $"{codeVersion} {arduinoVerion}";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Trace.TraceInformation("Form_Load");

            // get config
            try
            {
                _cfg = new Config();
                _cfg = ReadConfig();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Application.Exit();
            }

            string[] devices = UsbCamera.FindDevices();
            if (devices.Length == 0)
            {
                MessageBox.Show("FATAL: No Camera.");
                return; // no camera.
            }

            foreach (string d in devices)
                cmbCamera.Items.Add(d);

            // GET A WHOLE BUNCH OF DEFAULTS
            //  1) Set defaults
            //  2) Read from Config

            // logs go in user's profile folder (eg; c:\users\frodob)
            _sLogFilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            _logQueue = new LogQueue(50);

            txtName.Text = _cfg.MachineName;

            var threshold = (decimal)_cfg.ThreshHold;
            if (threshold < numTrigger.Minimum || threshold > numTrigger.Maximum)
                numTrigger.Value = 0;
            else
                numTrigger.Value = threshold;

            numDroneDelay.Text = _elapseBetweenDrones.TotalSeconds.ToString(); // TODO get from config

            // COM ports
            cmbComPorts.Items.Clear();
            foreach (string p in SerialPort.GetPortNames())
                cmbComPorts.Items.Add(p);

            cmbComPorts.SelectedItem = _cfg.ComPort;
            OpenComPort(_cfg.ComPort);

            // camera details
            cmbCamera.SelectedIndex = _cfg.Camera;
            cmbCameraFormat.SelectedIndex = _cfg.VideoMode;

            // if the three args are available for SMS, then create an SmsAlert object
            if (string.IsNullOrEmpty(_cfg.AzureConnection) == false &&
                string.IsNullOrEmpty(_cfg.FromNumber) == false &&
                string.IsNullOrEmpty(_cfg.ToNumber) == false)
            {
                _smsAlert = new SmsAlert(_cfg.MachineName, _cfg.AzureConnection, _cfg.FromNumber, _cfg.ToNumber) {
                    BlockLateNightSms = true
                };

                txtSmsEnabled.Text = "Yes";
                btnTestSms.Enabled = true;
            }

            // if there's a -run argument then start the DivGrind running
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length == 2 && args[1].ToLower().StartsWith("-run"))
            {
                Trace.TraceInformation("Autostart");

                StartAllThreads();

                btnStart.Enabled = false;
                cmbCamera.Enabled = false;
                cmbCameraFormat.Enabled = false;
                txtName.Enabled = false;

                // keep this set to true so if the wrong COM port is selected it can be changed
                cmbComPorts.Enabled = true;         
            }

            radLBLongPress.Checked = _bLBLongPress;
            radLBShortPress.Checked = !_bLBLongPress;

            radRBLongPress.Checked = _bRBLongPress;
            radRBShortPress.Checked = !_bRBLongPress;

            // used for the sliding RGB charts
            _arrR = new byte[pictR.Width];
            _chartR = new Chart(pictR.Width, pictR.Height, Color.Red, _loopDelay);

            _arrG = new byte[pictG.Width];
            _chartG = new Chart(pictG.Width, pictG.Height, Color.Green, _loopDelay);

            _arrB = new byte[pictB.Width];
            _chartB = new Chart(pictB.Width, pictB.Height, Color.Blue, _loopDelay);

            SetStatusBar();

            // send a message to the Arduino
            // to indicate the DivGrind is alive
            // this stays enabled until the tool is killed.
            SetHeartbeat();
        }

        // this gives the code a chance to kill the worker threads gracefully
        private void Form1_Closing(object sender, FormClosingEventArgs e)
        {
            Trace.TraceInformation("Form_Closing");

            KillSkillTimer();
            StopHeartbeat();
            _fKillThreads = true;

            Thread.Sleep(400);
            e.Cancel = false;

            if (_sComPort != null)
            {
                _sComPort.Close();
                _sComPort = null;
            }
        }

        // this is a pause function that accommodates for thread abandonment
        private void SpinDelay(int secs)
        {
            Trace.TraceInformation($"SpinDelay -> {secs}s");

            for (int i = 0; i < secs; i++)
            {
                if (_fKillThreads)
                    break;

                Thread.Sleep(1000);
            }
        }
    }
}


