﻿using System;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;

namespace WindowsFormsAppCamera
{
    public partial class Form1
    {
        private void PopulateVideoFormatCombo(int cameraIndex)
        {
            UsbCamera.VideoFormat[] formats = UsbCamera.GetVideoFormat(cameraIndex);

            cmbCameraFormat.Items.Clear();
            foreach (UsbCamera.VideoFormat t in formats)
            {
                string f = "Resolution: " + t.Caps.InputSize.ToString() + ", bits/sec: " + t.Caps.MinBitsPerSecond;
                cmbCameraFormat.Items.Add(f);
            }
        }

        // when the camera changes, populate the possible video modes
        private void cmbCamera_SelectedIndexChanged(object sender, EventArgs e)
        {
            int cameraIndex = _cfg.Camera = cmbCamera.SelectedIndex;
            PopulateVideoFormatCombo(cameraIndex);
        }

        // return the selected video format from the camera
        private UsbCamera.VideoFormat GetCameraVideoFormat(int camera)
        {
            // create usb camera object with selected resolution and start.
            UsbCamera.VideoFormat[] formats = UsbCamera.GetVideoFormat(camera);
            _cfg.VideoMode = cmbCameraFormat.SelectedIndex;
            return formats[cmbCameraFormat.SelectedIndex];
        }

        // when the camera video mode changes, select the video mode
        private void cmbCameraFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            int camera = cmbCamera.SelectedIndex;
            UsbCamera.VideoFormat selectFormat = GetCameraVideoFormat(camera);

#if DEBUG
            if (selectFormat.Size.Width != 640 && selectFormat.Size.Height != 480)
                MessageBox.Show("Warning! Only 640x480 has been tested", "Warning");

            if (selectFormat.Caps.MinBitsPerSecond == 0)
                MessageBox.Show("Warning! Selected format streams no data, choose another format", "Warning");
#endif

            _camera = new UsbCamera(camera, selectFormat);
            _camera?.Start();

            cmbComPorts.Items.Clear();
            foreach (string p in SerialPort.GetPortNames())
                cmbComPorts.Items.Add(p);

            cmbComPorts.SelectedItem = _cfg.ComPort;
        }

        // open the COM port
        private void OpenComPort(string comport)
        {
            // don't try opening an already open COM port
            if (_sComPort != null)
                return;

            WriteLog($"Attempting to open COM port {comport}");
            try
            {
                _sComPort = new SerialPort(comport, ComPortSpeed)
                {
                    WriteTimeout = 500 // 500 msec timeout
                };
                _sComPort.Open();

                WriteConfig(_cfg);
            }
            catch (Exception ex)
            {
                WriteLog($"EXCEPTION: Unable to open COM port, error is {ex.Message}");
            }
        }

        private string GetArduinoCodeVersion()
        {
            string ret;

            try
            {
                // now check there's an Arduino at the end of the COM port
                // '+' gets the version info froCm the DivGrind software
                _sComPort.Write("+");
                Thread.Sleep(100);

                ret = _sComPort.ReadExisting();
                Thread.Sleep(100);
            }
            catch (Exception ex)
            {
                WriteLog($"ERROR: {ex.Message}");
                ret = "";
            }

            return string.IsNullOrEmpty(ret) ? "" : ret;
        }

        // when the Arduino starts up, it has default RB/LB offsets to 0/0
        // this code resets the offsets and then writes the offsets stored in the config file
        private void WriteOffsetsToArduino()
        {
            WriteLog("Resetting LB/RB offsets to zero.");
            TriggerArduino("X");    // resets LB/RB to zero
            Thread.Sleep(50);

            WriteLog("Writing new offsets to LB");
            for (int i = 0; i < Math.Abs(_cfg.LBOffset); i++)
            {
                TriggerArduino(_cfg.LBOffset < 0 ? "l" : "L");
                Thread.Sleep(50);
            }

            WriteLog("Writing new offsets to RB");
            for (int i = 0; i < Math.Abs(_cfg.RBOffset); i++)
            {
                TriggerArduino(_cfg.RBOffset < 0 ? "r" : "R");
                Thread.Sleep(50);
            }
        }

        // COM port selected
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnTestComPort.Enabled = true;

            string sPort = cmbComPorts.SelectedItem.ToString();
            OpenComPort(sPort);
        }

        // test the COM port
        private void btnTestComPort_Click(object sender, EventArgs e)
        {
            TriggerArduino("V");
        }
    }
}
