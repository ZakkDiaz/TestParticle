using ParticleLib.Models;
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
using UnityEngine;

namespace Ocdisplay
{
    public partial class Form1 : Form
    {
        Bitmap _background;
        bool _outdatedImage = false;
        Bitmap _next;
        Font font = new Font("Arial", 20);
        bool _init = false;
        ParticleSpace3D _octree;
        ParticleEmitter particleEmitter = new ParticleEmitter();

        int depth = 1000;
        bool _mouseDown = false;
        BackgroundWorker adder;
        BackgroundWorker drawer;
        BackgroundWorker physics;
        public Form1()
        {
            InitializeComponent();

            this.DoubleBuffered = true;
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.Paint += Form1_Paint;
            this.MouseDown += Form1_MouseDown;
            this.MouseUp += Form1_MouseUp;

            adder = new BackgroundWorker();
            adder.DoWork += Adder_DoWork;
            adder.RunWorkerAsync();
            drawer = new BackgroundWorker();
            drawer.DoWork += Drawer_DoWork;
            drawer.RunWorkerAsync();
            physics = new BackgroundWorker();
            physics.DoWork += Physics_DoWork;
            physics.RunWorkerAsync();
            //Application.Idle += HandleApplicationIdle;
        }

        private int physicsInterval = 10000000;
        private int physicsCount = 0;
        private void Physics_DoWork(object sender, DoWorkEventArgs e)
        {
            physicsCount = 0;
            while (true)
            {
                physicsCount++;
                if (physicsCount == physicsInterval)
                {
                    _octree.ProcessTimestep(1, Vector3.zero, Vector3.zero);
                    //var collections = _octree.GetCollections();
                    //Parallel.ForEach(collections, (collection) =>
                    //{
                    //    collection.SumPhysics();
                    //});
                    physicsCount = 0;
                }
            }
        }

        bool doDraw = false;
        private void Drawer_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!_init)
            {
                System.Threading.Thread.Sleep(10);
            }
            while(true)
            {
                System.Threading.Thread.Sleep(10);
                //if (doDraw)
                {
                    Draw();
                    doDraw = false;
                }
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            _mouseDown = false;
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            _mouseDown = true;
        }

        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            var location = new Vector3(e.X, e.Y, 250);
            particleEmitter.EmitParticle(ref _octree, location, Vector3.zero, Vector3.zero, new UnityEngine.Bounds());


            Bitmap bmp = new Bitmap(this.Width, this.Height);
            using var g = System.Drawing.Graphics.FromImage(bmp);
            //_octree.Draw(g);
            _next = bmp;
            _outdatedImage = true;
            this.Invalidate();
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

        bool _outdatedImagedReceived = false;
        //void HandleApplicationIdle(object sender, EventArgs e)
        //{
        //    while (IsApplicationIdle())
        //    {
        //        Update();
        //        if (_outdatedImage)
        //        {
        //            this.Invalidate();
        //            _outdatedImage = false;
        //            _outdatedImagedReceived = true;
        //        }
        //    }
        //}

        private int renderInterval = 10000000;
        private int renderCount = 0;
        private int radius = 25;
        private unsafe void Adder_DoWork(object sender, DoWorkEventArgs e)
        {
            if (!_init)
            {
                Init();
            }

            var otSize = _octree.to - _octree.from;
            var otCenter = _octree.from + otSize / 2;
            while (true)
            {
                renderCount++;
                if (renderCount == renderInterval)
                {
                    if (_mouseDown)
                        try
                        {

                            var cusrorPos = System.Windows.Forms.Cursor.Position;
                            //System.Threading.Thread.Sleep(1);
                            //System.Threading.Thread.Sleep(10);
                            for (var i = 0; i < 1; i++)
                            {
                                var cX = cusrorPos.X + (float)(ThreadSafeRandom.Next_s() * 2 * radius) - radius;
                                var cY = cusrorPos.Y + (float)(ThreadSafeRandom.Next_s() * 2 * radius) - radius;
                                //var locationToAdd = new NodeTypeLocation3D((float)(r.NextDouble() * this.Width), (float)(r.NextDouble() * this.Height), (float)(r.NextDouble() * depth), false);
                                //var locationToAdd = new NodeTypeLocation3D(cX, cY, 500, false);

                                //var locationToAdd = new NodeTypeLocation3D((float)(r.NextDouble() + 10), (float)(r.NextDouble() + 10), (float)(r.NextDouble() + 10), false);
                                var location = new Vector3(cX, cY, 500);
                                particleEmitter.EmitParticle(ref _octree, location, Vector3.zero, Vector3.zero, new UnityEngine.Bounds(otCenter, otSize));
                            }
                            //Draw();
                            doDraw = true;

                        }
                        catch (Exception ex)
                        {

                        }
                    renderCount = 0;
                }
            }
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            if(!_init)
            {
                Init();
            }
            else if (_outdatedImagedReceived)
            {
                _background = new Bitmap(_next);
                this.BackgroundImage = _background;
                //e.Graphics.DrawImage(_background, Point.Empty);
                _outdatedImagedReceived = false;
            }
        }

        private void Init()
        {
            _background = new Bitmap(this.Width, this.Height);
            using var gfx = System.Drawing.Graphics.FromImage(_background);
            gfx.Clear(System.Drawing.Color.White);
            this.BackgroundImage = _background;
            _next = new Bitmap(_background);
            _octree = new ParticleSpace3D(new Vector3(0, 0, 0), new Vector3(this.Width, this.Height, depth));
            _init = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!_init)
            {
                Init();
                return;
            }
            
            for (var i = 0; i < 1; i++)
            {
                //var locationToAdd = new NodeTypeLocation3D((float)(r.NextDouble() * this.Width), (float)(r.NextDouble() * this.Height), (float)(r.NextDouble() * depth), false);
                //_octree.Add(locationToAdd);
            }

            //Draw();

          
        }

        private void Draw()
        {
            //var otSize = _octree.Size();
            //var otDepth = _octree.Depth();

            Bitmap bmp = new Bitmap(this.Width, this.Height);
            using var g = System.Drawing.Graphics.FromImage(bmp);
            //_octree.Draw(g);
            //g.DrawString($"Size: {otSize} Depth: {otDepth}", font, Brushes.Black, new PointF(50, 50));
            foreach (var itm in _octree.GetParticles())
            {
                g.FillEllipse(Brushes.Black, new Rectangle((int)itm.Location.x, (int)itm.Location.y, 10, 10));
                //System.Windows.Forms.Application.DoEvents();
            }
            //foreach (var bound in _octree.GetBoxCloud())
            //{
            //    g.DrawRectangle(Pens.YellowGreen, new Rectangle((int)bound.From.X, (int)bound.From.Y, (int)(bound.To.X - bound.From.X), (int)(bound.To.Y - bound.From.Y)));
            //    Application.DoEvents();
            //}
            _next = bmp;
            _outdatedImagedReceived = true;
            this.Invalidate();
        }
    }
}
