using System;
using System.Drawing;

namespace WindowsFormsAppCamera
{
    class Chart
    {
        private readonly Bitmap _bmp; 
        public Bitmap Bmp => _bmp;

        private readonly int _x, _y, _scaling;
        private readonly SolidBrush _background;
        private readonly Pen _pen;
        private readonly Pen _penCalibration;
        private readonly Rectangle _rect;

        public Chart(int x, int y, Color col)
        {
            _x = x;
            _y = y;
            _scaling = 255 / _y; // calculate line scaling

            _bmp = new Bitmap(_x, _y);
            _background = new SolidBrush(Color.Black);
            _pen = new Pen(col);
            _penCalibration = new Pen(Color.LightGray) {DashStyle = System.Drawing.Drawing2D.DashStyle.Dot};
            _rect = new Rectangle(0, 0, _x, _y);
        }

        public void Draw(byte[] arr, byte b, byte? calibration)
        {
            using (var g = Graphics.FromImage(_bmp))
            {
                g.FillRectangle(_background, _rect);

                // shift the array and add the new item to the array
                Array.Copy(arr, 0, arr, 1, _x - 1);
                arr[0] = b;

                for (int i = 0; i < _x; i++)
                {
                    var v = arr[i];
                    var top = (255 - v) / _scaling;
                    g.DrawLine(_pen, _x-i, _y - 1, _x-i, top);
                }

                if (calibration != null)
                {
                    var top = (int)(255 - calibration) / _scaling;
                    g.DrawLine(_penCalibration, 0, top, _x, top);
                }
            }
        }
    }
}
