using System;
using System.Windows.Forms;
using System.IO.Ports;

namespace WindowsFormsAppCamera
{
    public partial class Form1 : Form
    {
        private void PopulateVideoFormatCombo(int cameraIndex)
        {
            UsbCamera.VideoFormat[] formats = UsbCamera.GetVideoFormat(cameraIndex);

            cmbCameraFormat.Items.Clear();
            for (int i = 0; i < formats.Length; i++)
            {
                string f = "Resolution: " + formats[i].Caps.InputSize.ToString() + ", bits/sec: " + formats[i].Caps.MinBitsPerSecond;
                cmbCameraFormat.Items.Add(f);
            }
        }

        // when the camera changes, populate the possible video modes
        private void cmbCamera_SelectedIndexChanged(object sender, EventArgs e)
        {
            int cameraIndex = _cfg.Camera = cmbCamera.SelectedIndex;
            PopulateVideoFormatCombo(cameraIndex);
        }

        // retusn the selected video format from the camera
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
        private void openComPort(string comport)
        {
            if (_sComPort != null)
                return;

            WriteLog($"Attempting to open COM port {comport}");
            try
            {
                _sComPort = new SerialPort(comport, _ComPortSpeed)
                {
                    WriteTimeout = 500 // 500 msec timeout
                };
                _sComPort.Open();
            }
            catch (Exception ex)
            {
                WriteLog($"EXCEPTION: Unable to open COM port, error is {ex.Message}");
            }

            WriteConfig(_cfg);
        }

        // COM port selected
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnTestComPort.Enabled = true;

            string sPort = cmbComPorts.SelectedItem.ToString();
            openComPort(sPort);
        }

        // test the COM port
        private void btnTestComPort_Click(object sender, EventArgs e)
        {
            TriggerArduino("V");
        }
    }
}
