using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsAppCamera
{
    public partial class Form1 : Form
    {
        UsbCamera _camera;
        Queue<Int64> _redCalibrationData = null;
        Int64 _redCalibrationAvg;
        int xStart=40, yStart=20, xEnd=300, yEnd=200;

        public Form1()
        {
            InitializeComponent();
            _redCalibrationData = new Queue<Int64>();
        }

        private void btnEraseCalibrate_Click(object sender, EventArgs e)
        {
            _redCalibrationAvg = 0l;
            _redCalibrationData.Clear();
            lblRedCount.Text = "0";
            lblRedAvg.Text = "0";
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            string[] devices = UsbCamera.FindDevices();
            if (devices.Length == 0)
            {
                MessageBox.Show("No Camera");
                return; // no camera.
            }

            // check format
            int cameraIndex = 1;
            UsbCamera.VideoFormat[] formats = UsbCamera.GetVideoFormat(cameraIndex);
            //for (int i = 0; i < formats.Length; i++) Console.WriteLine("{0}:{1}", i, formats[i]);

            // create usb camera and start.
            _camera = new UsbCamera(cameraIndex, formats[0]);
            _camera.Start();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // get image.
            // Immediately after starting the USB camera,
            // GetBitmap() fails because image buffer is not prepared yet.
            var bmp = _camera.GetBitmap();
            pictureBox1.Image = bmp;

            using (Graphics g = Graphics.FromImage(bmp))
            {
                Rectangle rect = new Rectangle(xStart, yStart, xEnd, yEnd);

                Color customColor = Color.FromArgb(99, Color.Yellow);
                SolidBrush brush = new SolidBrush(customColor); 
                g.FillRectangle(brush, rect);

                Pen pen = new Pen(Color.FromKnownColor(KnownColor.Yellow));
                g.DrawRectangle(pen, rect);
            }


            //bmp.Save(@"c:\users\mikehow\foo.bmp");

            lblHeight.Text = bmp.Height.ToString();
            lblWidth.Text = bmp.Width.ToString();

            // Get Calibration data
            Int32 totalR = 0;
            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    Color px = bmp.GetPixel(x, y);
                    totalR += (Int32)px.R;
                }
            }

            lblRedCount.Text = totalR.ToString();
            _redCalibrationData.Enqueue(totalR);

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
            lblRedAvg.Text = _redCalibrationAvg.ToString();
        }
    }
}
