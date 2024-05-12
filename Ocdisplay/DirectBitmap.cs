using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace Ocdisplay
{
    public class DirectBitmap : IDisposable
    {
        public Bitmap Bitmap { get; private set; }
        public byte[] Bits { get; private set; }
        public bool Disposed { get; private set; }
        public int Height { get; private set; }
        public int Width { get; private set; }

        protected GCHandle BitsHandle { get; private set; }

        public DirectBitmap(int width, int height)
        {
            Width = width;
            Height = height;
            Bits = new byte[width * height * 4];
            BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
            Bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppPArgb, BitsHandle.AddrOfPinnedObject());
        }

        public void SetPixels(byte[] pixels)
        {
            for(var i = 0; i <  pixels.Length; i++)
            {
                if(Bits.Length > i)
                    Bits[i] = pixels[i];
            }
        }

        public void SetPixel(int x, int y, Color color)
        {
            int index = (x + (y * Width)) * 4;
            Bits[index + 0] = color.B;
            Bits[index + 1] = color.G;
            Bits[index + 2] = color.R;
            Bits[index + 3] = color.A;
        }

        public Color GetPixel(int x, int y)
        {
            int index = (x + (y * Width)) * 4;
            byte b = Bits[index + 0];
            byte g = Bits[index + 1];
            byte r = Bits[index + 2];
            byte a = Bits[index + 3];
            return Color.FromArgb(a, r, g, b);
        }

        public void Dispose()
        {
            if (Disposed) return;
            Disposed = true;
            Bitmap.Dispose();
            BitsHandle.Free();
        }
    }
}
