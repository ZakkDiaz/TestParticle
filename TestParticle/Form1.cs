using ParticleLib.Models;
using Render.Keys;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;

namespace TestParticle
{
    public partial class Form1 : Form
    {
        System.Drawing.Graphics graphics;
        BackgroundWorker _renderWorker;
        ThreadSafeRandom r;
        //List<ParticleLib.Models.Particle> particles = new List<ParticleLib.Models.Particle>();
        ParticleEmitter particleEmitter = new ParticleEmitter();
        ParticleSpace2D particleSpace;
        Point size;
        public Form1()
        {
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            r = new ThreadSafeRandom();
            size = new Point(1000, 1000); 
            this.DoubleBuffered = true;
            graphics = this.CreateGraphics();
            //logic & rendering
            Application.Idle += Application_Idle;
            SetupKeyboardHooks();
            particleEmitter.location = new Point(250, 200);
            particleSpace = new ParticleSpace2D(PointF.Empty, new PointF(size.X, size.Y));
            _flameImg = new Bitmap("Assets\\Images\\Spark.png");
        }



        private GlobalKeyboardHook _globalKeyboardHook;

        public void SetupKeyboardHooks()
        {
            _globalKeyboardHook = new GlobalKeyboardHook();
            _globalKeyboardHook.KeyboardPressed += OnKeyPressed;
        }
        private static Dictionary<Keys, bool> PressedKeys = new Dictionary<Keys, bool>();
        private static object _pklock = new object();

        private void OnKeyPressed(object sender, GlobalKeyboardHookEventArgs e)
        {
            if (e.KeyboardData.VirtualCode == (int)Keys.Space && e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyUp)
            {
                for (var i = 0; i < 1; i++)
                {
                    //center - new Point(size.X / 2, size.Y / 2)
                    var at = new Point((int)(size.X - (size.X * (particleEmitter.location.X) / 1000f)),(int)(size.Y - (size.X * (particleEmitter.location.Y / 1000f))));
                    EmitParticle(at, ThreadSafeRandom.Next(.01f, 1f, true));
                }
            }
            if (e.KeyboardData.VirtualCode == (int)Keys.LControlKey && e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyUp)
            {
                for (var i = 0; i < 1; i++)
                {
                    EmitParticle(new Point(size.X / 2, size.Y / 2), 100);
                }
            }
            //if (e.KeyboardData.VirtualCode == (int)Keys.A && e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyDown)
            //{
            //    if (!PressedKeys.ContainsKey(Keys.A))
            //        PressedKeys.Add(Keys.A, true);
            //}
            //if (e.KeyboardData.VirtualCode == (int)Keys.W && e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyDown)
            //{
            //}
            //if (e.KeyboardData.VirtualCode == (int)Keys.D && e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyDown)
            //{
            //}
            //if (e.KeyboardData.VirtualCode == (int)Keys.S && e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyDown)
            //{
            //}
            lock(_pklock)
            if(!PressedKeys.ContainsKey((Keys)e.KeyboardData.VirtualCode))
                PressedKeys.Add((Keys)e.KeyboardData.VirtualCode, false);
            PressedKeys[(Keys)e.KeyboardData.VirtualCode] = e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyDown;

            if (e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyUp)
                PressedKeys[(Keys)e.KeyboardData.VirtualCode] = false;
            
            //if (e.KeyboardData.VirtualCode == (int)Keys.LControlKey && e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyDown)
            //{
            //    particleSpace.ProcessSubspace();
            //}
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            while (IsApplicationIdle())
            {
                doWorldEvents();
                doRender();
                GC.Collect();
            }
        }
        bool IsApplicationIdle()
        {
            NativeMessage result;
            return PeekMessage(out result, IntPtr.Zero, (uint)0, (uint)0, (uint)0) == 0;
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

        private void doWorldEvents()
        {
            
            if (PressedKeys.ContainsKey(Keys.W) && PressedKeys[Keys.W] == true)
                particleEmitter.SetLocation((particleEmitter.location.X), (particleEmitter.location.Y - 1 * stepSize * 500));
            if (PressedKeys.ContainsKey(Keys.S) && PressedKeys[Keys.S] == true)
                particleEmitter.SetLocation((particleEmitter.location.X), (particleEmitter.location.Y + 1 * stepSize * 500));
            if (PressedKeys.ContainsKey(Keys.A) && PressedKeys[Keys.A] == true)
                particleEmitter.SetLocation((particleEmitter.location.X - 1 * stepSize * 500), (particleEmitter.location.Y));
            if (PressedKeys.ContainsKey(Keys.D) && PressedKeys[Keys.D] == true)
                particleEmitter.SetLocation((particleEmitter.location.X + 1 * stepSize * 500), (particleEmitter.location.Y));



            //if (particles.Count == 0)
            //    EmitParticle();

            var relativePoint = this.PointToClient(Cursor.Position);
            var particles = particleSpace.GetParticles();
            var diff = DateTime.UtcNow.Ticks / 1000 - ms;
            //Parallel.ForEach(particles, particle =>
            //{
            //    var wasDead = particle.duration <= 0;
            //    //particle.ProcessTimestep(diff, relativePoint);
            //    //particleSpace.UpdateParticle(particle, wasDead);
            //});
            particleSpace.ProcessTimestep(diff, (size.X - (size.X*(relativePoint.X)/1000f) , size.Y - (size.X * (relativePoint.Y / 1000f))), size);
            //particles.RemoveAll(p => p.isDead);
            var toSplit = particleSpace.GetParticles().Where(p => p.duration <= 0).ToList();
            toSplit.ForEach(p => { p.isDead = true; if (p.splitCount > 0) EmitParticle(p); });
        }
        float stepSize = .001f;
        private void EmitParticle(Particle from)
        {
            var emittedParticle = particleEmitter.EmitParticle(ref particleSpace, new PointF((float)(from.dimenisons[0].pos + (ThreadSafeRandom.Next_s() - .5)), (float)(from.dimenisons[1].pos + (ThreadSafeRandom.Next_s() - .5))), new PointF((float)(ThreadSafeRandom.Next_s() - .5), 0), new PointF(from.dimenisons[0].vel, from.dimenisons[1].vel), size, from.splitCount - 1, true, stepSize);
            var emittedParticle2 = particleEmitter.EmitParticle(ref particleSpace, new PointF((float)(from.dimenisons[0].pos + (ThreadSafeRandom.Next_s() - .5)), (float)(from.dimenisons[1].pos + (ThreadSafeRandom.Next_s() - .5))), new PointF(0, (float)(ThreadSafeRandom.Next_s() - .5)), new PointF(from.dimenisons[0].vel, from.dimenisons[1].vel), size, from.splitCount - 1, true, stepSize);

            //if (emittedParticle != null)
            //    particleSpace.AddParticle(emittedParticle);
            //if (emittedParticle2 != null)
            //    particleSpace.AddParticle(emittedParticle2);
        }

        private void EmitParticle()
        {
            var relativePoint = this.PointToClient(Cursor.Position);
            var emittedParticle = particleEmitter.EmitParticle(ref particleSpace, relativePoint, Point.Empty, Point.Empty, size, 0, stepSize: stepSize, particleSize: ThreadSafeRandom.Next(.01f, .5f, true));
            //if (emittedParticle != null)
            //    particleSpace.AddParticle(emittedParticle);
        }
        private void EmitParticle(Point at, float particleSize)
        {
            var emittedParticle = particleEmitter.EmitParticle(ref particleSpace, at, Point.Empty, Point.Empty, size, 0, true, stepSize, particleSize);
        }

        long ms = DateTime.UtcNow.Ticks/1000;
        Bitmap _flameImg;
        public void doRender()
        {
            BufferedGraphicsContext context = new BufferedGraphicsContext();

            lock (graphics)
            {
                BufferedGraphics buffer = context.Allocate(graphics, new Rectangle(0, 0, this.Width, this.Height));
                buffer.Graphics.Clear(System.Drawing.Color.White);
                var diff = DateTime.UtcNow.Ticks / 1000 - ms;
                ms = DateTime.UtcNow.Ticks / 1000;
                var particles = particleSpace.GetParticles();
                buffer.Graphics.DrawString("\tparticles:" + particles.Count + "\tms:" + diff, new Font(FontFamily.GenericSansSerif, 12, FontStyle.Regular), Brushes.Black, new PointF(10, 10));

                var parts = particles.ToList();
                foreach (var particle in parts)
                {
                    if(particle.mass > 0)
                    //buffer.Graphics.FillEllipse(Brushes.Black, new Rectangle((int)particle.dimenisons[0].pos, (int)particle.dimenisons[1].pos, ((int)(particle.mass * 100)) / 10, ((int)(particle.mass * 100)) / 10));
                      buffer.Graphics.DrawImage(_flameImg, new RectangleF(1000*(size.X - (particle.Rect.X))/size.X, 1000*(size.Y - (particle.Rect.Y))/size.Y, particle.Rect.Width * 10, particle.Rect.Height * 10));
                }

                var rectangles = particleSpace.GetRects(true);
                //var parts = particleSpace.GetParticles();
                //foreach (var particle in parts)
                //{
                //    buffer.Graphics.FillEllipse(Brushes.Black, new Rectangle((int)particle.dimenisons[0].pos, (int)particle.dimenisons[1].pos, ((int)(particle.mass * 100)) / 10, ((int)(particle.mass * 100)) / 10));
                //    //buffer.Graphics.DrawImage(_flameImg, new Rectangle((int)particle.dimenisons[0].pos, (int)particle.dimenisons[1].pos, ((int)(particle.mass * 100)) / 10, ((int)(particle.mass * 100)) / 10));
                //}
                foreach (var rect in rectangles)
                {
                    buffer.Graphics.DrawRectangle(Pens.Black, new Rectangle((int)rect.X, (int)rect.Y, (int)(rect.Width - rect.X), (int)(rect.Height - rect.Y)));
                }

                buffer.Graphics.DrawEllipse(Pens.Black, (particleEmitter.location.X) - 5, (particleEmitter.location.Y) - 5, 10, 10);

                buffer.Render();
                buffer.Dispose();

            }
        }
    }
}
