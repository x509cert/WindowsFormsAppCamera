﻿using System;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.IO;
using System.IO.Ports;

namespace WindowsFormsAppCamera
{
    public partial class Form1 : Form
    {
        // The main entry point
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string[] devices = UsbCamera.FindDevices();
            if (devices.Length == 0)
            {
                MessageBox.Show(this, "Uh oh! No Camera!");
                return; // no camera.
            }

            foreach (string d in devices)
                cmbCamera.Items.Add(d);

            // GET A WHOLE BUNCH OF DEFAULTS
            // Format here is:
            //  1) Set defaults
            //  2) Read from Config
            //  3) Override with any command-line args

            // logs go in user's profile folder (eg; c:\users\mikehow)
            _sLogFilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            // get the date this binary was last modified
            string strpath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            FileInfo fi = new FileInfo(strpath);
            string buildDate = fi.LastWriteTime.ToString("yyMMdd:HHmm");

            _logQueue = new LogQueue(50);

            string isDebug = "";
#if DEBUG
            isDebug = "[DBG] ";
#endif

            // add info to title of the tool
            string machine = Dns.GetHostName();
            Text = $"DivGrind {isDebug}[{buildDate}] on {machine}";

            // get config TODO - add error checking
            _cfg = new Config();
            _cfg = ReadConfig();

            txtName.Text = _cfg.MachineName;

            // set the RGB values - these are the last saved settings - updated when Calibrate is pressed
            lblRedCount.Text = _cfg.LastCalibratedR.ToString();
            lblGreenCount.Text = _cfg.LastCalibratedG.ToString();
            lblBlueCount.Text = _cfg.LastCalibratedB.ToString();

            numTrigger.Value = (decimal)_cfg.ThreshHold;
            numDroneDelay.Text = _elapseBetweenDrones.TotalSeconds.ToString(); // TODO get from config

            // COM ports
            foreach (string p in SerialPort.GetPortNames())
                cmbComPorts.Items.Add(p);

            cmbComPorts.Text = _cfg.ComPort;

            // camera
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
            if (args.Length == 2 && args[1].ToLower().StartsWith("-run") == true)
            {
                StartAllThreads();
                btnStart.Enabled = false;
                cmbCamera.Enabled = false;
                cmbCameraFormat.Enabled = false;
                cmbComPorts.Enabled = false;
                txtName.Enabled = false;
            }
        }

        // this gives the code a chance to kill the main worker thread gracefully
        private void Form1_Closing(object sender, FormClosingEventArgs e)
        {
            KillSkillTimer();
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
            for (int i = 0; i < secs; i++)
            {
                if (_fKillThreads == true)
                    break;

                Thread.Sleep(1000);
            }
        }
    }
}

