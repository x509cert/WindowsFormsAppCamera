﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Net;
using System.Threading;
using System.Windows.Forms;

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
            // get the date this binary was last modified
            string strpath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            FileInfo fi = new FileInfo(strpath);
            string buildDate = fi.LastWriteTime.ToString("dd MMMM yyyy, hh: mm tt");

#if DEBUG
            const string isDebug = "[DBG] ";
#else 
            const string isDebug = "";
#endif

            // add info to title of the tool
            string machine = Dns.GetHostName();
            string codeVersion = $"{isDebug}[{buildDate}] [{machine}] ";
            string arduinoVerion = "?";

            if (_sComPort?.IsOpen == true)
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
                Application.Exit(); // no camera. We need to bail!
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

            decimal threshold = (decimal)_cfg.ThreshHold;
            numTrigger.Value = threshold < numTrigger.Minimum || threshold > numTrigger.Maximum ? numTrigger.Minimum : threshold;

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
            if (!string.IsNullOrEmpty(_cfg.AzureConnection) &&
                !string.IsNullOrEmpty(_cfg.FromNumber) &&
                !string.IsNullOrEmpty(_cfg.ToNumber))
            {
                _smsAlert = new SmsAlert(_cfg.MachineName, _cfg.AzureConnection, _cfg.FromNumber, _cfg.ToNumber) {
                    BlockLateNightSms = true
                };

                chkSmsAlerts.Enabled = true;
                chkSmsAlerts.Checked = true;
                btnTestSms.Enabled = true;
            }
            else
            {
                chkSmsAlerts.Enabled = false;
                chkSmsAlerts.Checked = false;
                btnTestSms.Enabled = false; 
            }

            // if there's a -run argument then start the DivGrind running
            string[] args = Environment.GetCommandLineArgs();
            bool autoStart = args.Length == 2 && args[1].StartsWith("-run", StringComparison.OrdinalIgnoreCase);
            if (autoStart)
            {
                Trace.TraceInformation("Autostart");

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
            UpdateToolTipLbRbData();
            WriteOffsetsToArduino();

            // send a message to the Arduino
            // to indicate the DivGrind is alive
            // this stays enabled until the tool is killed.
            SetHeartbeat();

            _timedList = new TimedList();

            // finally start all the main worker threads
            if (autoStart)
                StartAllThreads();
        }

        // this gives the code a chance to kill the worker threads gracefully
        private void Form1_Closing(object sender, FormClosingEventArgs e)
        {
            Trace.TraceInformation("Form_Closing");

            KillSkillTimer();
            StopHeartbeat();
            _fKillThreads = true;

            Thread.Sleep(300);
            e.Cancel = false;

            _sComPort?.Close();
            _sComPort = null;
        }

        // this is a pause function that accommodates for thread abandonment
        private void SpinDelay(int secs)
        {
            Trace.TraceInformation($"SpinDelay -> {secs}s");

            for (int i = 0; i < secs; i++)
            {
                if (_fKillThreads)
                    break;

                Thread.Sleep(999);
            }
        }
    }
}


