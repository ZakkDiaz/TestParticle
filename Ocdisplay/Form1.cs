using ParticleLib.Models._3D;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ocdisplay
{
    public partial class Form1 : Form
    {
        Bitmap _background;
        bool _outdatedImage = false;
        Bitmap _next;
        Random r = new Random();
        Font font = new Font("Arial", 20);
        bool _init = false;
        Octree _octree;
        int depth = 1000;

        BackgroundWorker adder;
        public Form1()
        {
            InitializeComponent();

            this.DoubleBuffered = true;
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.Paint += Form1_Paint;

            adder = new BackgroundWorker();
            adder.DoWork += Adder_DoWork;
            adder.RunWorkerAsync();
            Application.Idle += HandleApplicationIdle;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NativeMessage
        {
            public IntPtr Handle;
            public uint Message;
            public IntPtr WParameter;
            public IntPtr LParameter;
            public uint Time;
            public Point Location;
        }

        [DllImport("user32.dll")]
        public static extern int PeekMessage(out NativeMessage message, IntPtr window, uint filterMin, uint filterMax, uint remove);

        bool IsApplicationIdle()
        {
            NativeMessage result;
            return PeekMessage(out result, IntPtr.Zero, (uint)0, (uint)0, (uint)0) == 0;
        }

        void HandleApplicationIdle(object sender, EventArgs e)
        {
            while (IsApplicationIdle())
            {
                Update();
                if (_outdatedImage)
                    this.Invalidate();

            }
        }

        private void Adder_DoWork(object sender, DoWorkEventArgs e)
        {
            while(true)
            {
                if (!_init)
                {
                    Init();
                }

                for (var i = 0; i < 10; i++)
                {
                    var locationToAdd = new NodeTypeLocation3D((float)(r.NextDouble() * this.Width), (float)(r.NextDouble() * this.Height), (float)(r.NextDouble() * depth), false);
                    //var locationToAdd = new NodeTypeLocation3D((float)(r.NextDouble() *500), (float)(r.NextDouble() *100), (float)(r.NextDouble() * depth), false);
                    _octree.Add(locationToAdd);
                }
                var otSize = _octree.Size();
                var otDepth = _octree.Depth();

                Bitmap bmp = new Bitmap(this.Width, this.Height);
                using var g = Graphics.FromImage(bmp);
                _octree.Draw(g);
                g.DrawString($"Size: {otSize} Depth: {otDepth}", font, Brushes.Black, new PointF(50, 50));
                _next = bmp;
                _outdatedImage = true;
            }
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            if(!_init)
            {
                Init();
            }
            else if (_outdatedImage)
            {
                _background = new Bitmap(_next);
                e.Graphics.DrawImage(_background, Point.Empty);
            }
        }

        private void Init()
        {
            _background = new Bitmap(this.Width, this.Height);
            using var gfx = Graphics.FromImage(_background);
            gfx.Clear(Color.White);
            this.BackgroundImage = _background;
            _next = new Bitmap(_background);
            _octree = new Octree(new Point3D(), new Point3D(this.Width, this.Height, depth));
            _init = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!_init)
            {
                Init();
                return;
            }
            
            for (var i = 0; i < 7; i++)
            {
                var locationToAdd = new NodeTypeLocation3D((float)(r.NextDouble() * this.Width), (float)(r.NextDouble() * this.Height), (float)(r.NextDouble() * depth), false);
                _octree.Add(locationToAdd);
            }
            var otSize = _octree.Size();
            var otDepth = _octree.Depth();

            Bitmap bmp = new Bitmap(this.Width, this.Height);
            using var g = Graphics.FromImage(bmp);
            _octree.Draw(g);
            g.DrawString($"Size: {otSize} Depth: {otDepth}", font, Brushes.Black, new PointF(50, 50));
            _next = bmp;
            _outdatedImage = true;
            this.Invalidate();
        }
    }
}
