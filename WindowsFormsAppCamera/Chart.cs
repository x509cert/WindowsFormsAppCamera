using System;
using System.Drawing;

namespace WindowsFormsAppCamera
{
    class Chart
    {
        readonly private Bitmap _bmp; 
        public Bitmap Bmp => _bmp;

        readonly private int _x, _y, _per;
        readonly private SolidBrush _background;
        readonly private Pen _pen;
        readonly private Rectangle _rect;

        public Chart(int x, int y, Color col)
        {
            _x = x;
            _y = y;
            _per = 255 / _y;

            _bmp = new Bitmap(_x, _y);
            _background = new SolidBrush(Color.Black);
            _pen = new Pen(col);
            _rect = new Rectangle(0, 0, _x, _y);
        }

        public void Draw(byte[] arr, byte b)
        {
            using (Graphics g = Graphics.FromImage(_bmp))
            {
                g.FillRectangle(_background, _rect);

                // shift the array and add the new item
                Array.Copy(arr, 0, arr, 1, _x - 1);
                arr[0] = b;

                for (int i = 0; i < _x; i++)
                {
                    byte v = arr[i];
                    int top = (255 - v) / _per;
                    g.DrawLine(_pen, _x-i, _y - 1, _x-i, top);
                }
            }
        }
    }
}
