﻿using System;
using System.Drawing;

namespace WindowsFormsAppCamera
{
    internal class Chart
    {
        public Bitmap               Bmp { get; }

        private const int           _minDimension = 10;

        private readonly int        _x, _y, _scaling;
        private readonly SolidBrush _background;
        private readonly Pen        _pen;
        private readonly Pen        _5secPen;
        private readonly Pen        _penCalibration;
        private readonly Rectangle  _rect;
        private readonly int        _5secsMarker;

        public Chart(int x, int y, Color col, int loopDelay)
        {
            if (x < _minDimension || y < _minDimension)
                throw new ArgumentException("x or y dimension is too small");

            _x = x;
            _y = y;
            _scaling = 255 / _y; // calculate line scaling

            Bmp = new Bitmap(_x, _y);
            _background = new SolidBrush(Color.Black);
            _pen = new Pen(col);
            _5secPen = new Pen(Color.LightGray);
            _penCalibration = new Pen(Color.LightGray) {DashStyle = System.Drawing.Drawing2D.DashStyle.Dot};
            _rect = new Rectangle(0, 0, _x, _y);

            _5secsMarker = 5 // seconds  
                           * (1000 / loopDelay);
        }

        public void Draw(byte[] arr, byte b, byte? calibration)
        {
            using (Graphics g = Graphics.FromImage(Bmp))
            {
                g.FillRectangle(_background, _rect);

                // shift the array and add the new item to the array
                Array.Copy(arr, 0, arr, 1, _x - 1);
                arr[0] = b;

                for (int i = 0; i < _x; i++)
                {
                    byte data = arr[i];
                    int top = (255 - data) / _scaling;
                    g.DrawLine(_pen, _x-i, _y - 1, _x-i, top);

                    // place the 5sec marker
                    if (i % _5secsMarker == 0)
                        g.DrawLine(_5secPen, _x - i, _y, _x - i, _y-5);
                }

                if (calibration != null)
                {
                    int top = (int)(255 - calibration) / _scaling;
                    g.DrawLine(_penCalibration,
                               0,
                               top,
                               _x,
                               top);
                }
            }
        }
    }
}
