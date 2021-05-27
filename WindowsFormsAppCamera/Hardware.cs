using System;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;

namespace WindowsFormsAppCamera
{
    public partial class Form1
    {
        private void PopulateVideoFormatCombo(int cameraIndex)
        {
            UsbCamera.VideoFormat[] formats = UsbCamera.GetVideoFormat(cameraIndex);

            cmbCameraFormat.Items.Clear();
            foreach (var t in formats)
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
            var selectFormat = GetCameraVideoFormat(camera);

            if (selectFormat.Size.Width != 640 && selectFormat.Size.Height != 480)
                MessageBox.Show("Warning! Only 640x480 has been tested", "Warning");

            if (selectFormat.Caps.MinBitsPerSecond == 0)
                MessageBox.Show("Warning! Selected format streams no data, choose another format", "Warning");

            _camera = new UsbCamera(camera, selectFormat);
            _camera.Start();

            cmbComPorts.Items.Clear();
            foreach (string p in SerialPort.GetPortNames())
                cmbComPorts.Items.Add(p);

            cmbComPorts.SelectedItem = _cfg.ComPort;
        }

        // open the COM port
        // returns false on failure, true on success
        private bool OpenComPort(string comport)
        {
            if (_sComPort != null)
                return false;

            var fOk = true;

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
                fOk = false;
            }

            return fOk;
        }

        private string GetArduinoCodeVersion()
        {
            string ret;

            try
            {
                // now check there's an Arduino at the end of the COM port
                // '+' gets the version info from the DivGrind software
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

            return String.IsNullOrEmpty(ret) ? "" : ret;
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
