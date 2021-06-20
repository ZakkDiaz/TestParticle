//using ParticleLib.Models;
//using Render.Keys;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Drawing;
//using System.Runtime.InteropServices;
//using System.Threading.Tasks;
//using System.Windows.Forms;
//using System.Linq;
//using ParticleLib.Models.Entities;
//using UnityEngine;
//using Application = System.Windows.Forms.Application;

//namespace TestParticle
//{
//    public partial class Form1 : Form
//    {
//        System.Drawing.Graphics graphics;
//        ParticleEmitter particleEmitter = new ParticleEmitter();
//        ParticleSpace3D<BaseEntity<ITimesteppableLocationEntity>> particleSpace;
//        Vector3 size;
//        public Form1()
//        {
//            FormBorderStyle = FormBorderStyle.None;
//            WindowState = FormWindowState.Maximized;
//            size = new Vector3(1000, 1000, 1000); 
//            this.DoubleBuffered = true;
//            graphics = this.CreateGraphics();
//            //logic & rendering
//            Application.Idle += Application_Idle;
//            SetupKeyboardHooks();
//            particleEmitter.location = new Point(250, 200);
//            //var 3dSpace = new QuadTreeRect
//            particleSpace = new ParticleSpace3D<BaseEntity<ITimesteppableLocationEntity>>(UnityEngine.Vector3.zero, new UnityEngine.Vector3(size.x, size.y, size.z));
//            _flameImg = new Bitmap("Assets\\Images\\Spark.png");
//        }



//        private GlobalKeyboardHook _globalKeyboardHook;

//        public void SetupKeyboardHooks()
//        {
//            _globalKeyboardHook = new GlobalKeyboardHook();
//            _globalKeyboardHook.KeyboardPressed += OnKeyPressed;
//        }
//        private static Dictionary<Keys, bool> PressedKeys = new Dictionary<Keys, bool>();
//        private static object _pklock = new object();

//        private void OnKeyPressed(object sender, GlobalKeyboardHookEventArgs e)
//        {
//            if (e.KeyboardData.VirtualCode == (int)Keys.Space && e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyUp)
//            {
//                for (var i = 0; i < 1; i++)
//                {
//                    //center - new Point(size.X / 2, size.Y / 2)
//                    var at = new Vector3((int)(size.x - (size.x * (particleEmitter.location.x) / 1000f)),(int)(size.y - (size.y * (particleEmitter.location.y / 1000f))), (int)(size.z - (size.z * (particleEmitter.location.z) / 1000f)));
//                    EmitParticle(at, ThreadSafeRandom.Next(.01f, 1f, true));
//                }
//            }
//            if (e.KeyboardData.VirtualCode == (int)Keys.LControlKey && e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyUp)
//            {
//                for (var i = 0; i < 1; i++)
//                {
//                    EmitParticle(new Vector3(size.x / 2, size.y / 2, size.z / 2), 100);
//                }
//            }

//            lock(_pklock)
//            if(!PressedKeys.ContainsKey((Keys)e.KeyboardData.VirtualCode))
//                PressedKeys.Add((Keys)e.KeyboardData.VirtualCode, false);
//            PressedKeys[(Keys)e.KeyboardData.VirtualCode] = e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyDown;

//            if (e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyUp)
//                PressedKeys[(Keys)e.KeyboardData.VirtualCode] = false;
//        }

//        private void Application_Idle(object sender, EventArgs e)
//        {
//            while (IsApplicationIdle())
//            {
//                doWorldEvents();
//                doRender();
//                GC.Collect();
//            }
//        }
//        bool IsApplicationIdle()
//        {
//            NativeMessage result;
//            return PeekMessage(out result, IntPtr.Zero, (uint)0, (uint)0, (uint)0) == 0;
//        }

//        [StructLayout(LayoutKind.Sequential)]
//        public struct NativeMessage
//        {
//            public IntPtr Handle;
//            public uint Message;
//            public IntPtr WParameter;
//            public IntPtr LParameter;
//            public uint Time;
//            public Point Location;
//        }

//        [DllImport("user32.dll")]
//        public static extern int PeekMessage(out NativeMessage message, IntPtr window, uint filterMin, uint filterMax, uint remove);

//        private void doWorldEvents()
//        {
//            CheckKeyPresses();

//            ProcessEntities();
//        }

//        private void ProcessEntities()
//        {
//            var particles = ParticleEmitter.Particles;
//            var relativePoint = this.PointToClient(Cursor.Position);
//            var diff = DateTime.UtcNow.Ticks / 1000 - ms;
//            var focus = (size.x - (size.x * relativePoint.X / 1000f), size.y - (size.y * (relativePoint.Y / 1000f)));
//            particleSpace.ProcessTimestep<BaseEntity<ITimesteppableLocationEntity>>(particles, diff, focus, ((int)size.x, (int)size.y, (int)size.z));
//            //var toSplit = particleSpace.GetParticles().Where(p => p.duration <= 0).ToList();
//            //toSplit.ForEach(p => { p.isDead = true; if (p.splitCount > 0) EmitParticle(p); });
//        }

//        //private void CheckKeyPresses()
//        //{
//        //    if (PressedKeys.ContainsKey(Keys.W) && PressedKeys[Keys.W] == true)
//        //        particleEmitter.SetLocation((particleEmitter.location.x), (particleEmitter.location.y - 1 * stepSize * 500));
//        //    if (PressedKeys.ContainsKey(Keys.S) && PressedKeys[Keys.S] == true)
//        //        particleEmitter.SetLocation((particleEmitter.location.x), (particleEmitter.location.y + 1 * stepSize * 500));
//        //    if (PressedKeys.ContainsKey(Keys.A) && PressedKeys[Keys.A] == true)
//        //        particleEmitter.SetLocation((particleEmitter.location.x - 1 * stepSize * 500), (particleEmitter.location.y));
//        //    if (PressedKeys.ContainsKey(Keys.D) && PressedKeys[Keys.D] == true)
//        //        particleEmitter.SetLocation((particleEmitter.location.x + 1 * stepSize * 500), (particleEmitter.location.y));
//        //}

//        float stepSize = .001f;
//        //private void EmitParticle(ParticleEntity from)
//        //{
//        //    particleEmitter.EmitParticle(ref particleSpace, new PointF((float)(from.dimenisons[0].pos + (ThreadSafeRandom.Next_s() - .5)), (float)(from.dimenisons[1].pos + (ThreadSafeRandom.Next_s() - .5))), new PointF((float)(ThreadSafeRandom.Next_s() - .5), 0), new PointF(from.dimenisons[0].vel, from.dimenisons[1].vel), size, from.splitCount - 1, true, stepSize);
//        //    //particleEmitter.EmitParticle(ref particleSpace, new PointF((float)(from.dimenisons[0].pos + (ThreadSafeRandom.Next_s() - .5)), (float)(from.dimenisons[1].pos + (ThreadSafeRandom.Next_s() - .5))), new PointF(0, (float)(ThreadSafeRandom.Next_s() - .5)), new PointF(from.dimenisons[0].vel, from.dimenisons[1].vel), size, from.splitCount - 1, true, stepSize);
//        //}
//        private void EmitParticle(Vector3 at, float particleSize)
//        {
//            particleEmitter.EmitParticle(ref particleSpace, at, Vector3.zero, PointF.Empty, size, 0, true, stepSize, particleSize);
//        }

//        long ms = DateTime.UtcNow.Ticks/1000;
//        Bitmap _flameImg;
//        public void doRender()
//        {
//            BufferedGraphicsContext context = new BufferedGraphicsContext();

//            lock (graphics)
//            {
//                BufferedGraphics buffer = context.Allocate(graphics, new Rectangle(0, 0, this.Width, this.Height));
//                buffer.Graphics.Clear(System.Drawing.Color.White);
//                var diff = DateTime.UtcNow.Ticks / 1000 - ms;
//                ms = DateTime.UtcNow.Ticks / 1000;
//                var particles = ParticleEmitter.Particles;
//                buffer.Graphics.DrawString("\tparticles:" + particles.Count + "\tms:" + diff, new Font(FontFamily.GenericSansSerif, 12, FontStyle.Regular), Brushes.Black, new PointF(10, 10));

//                var parts = particles.ToList();
//                foreach (var particle in parts)
//                {
//                    //buffer.Graphics.FillEllipse(Brushes.Black, new Rectangle((int)particle.dimenisons[0].pos, (int)particle.dimenisons[1].pos, ((int)(particle.mass * 100)) / 10, ((int)(particle.mass * 100)) / 10));
//                      buffer.Graphics.DrawImage(_flameImg, new RectangleF(1000*(size.x - (particle.Rect.X))/size.x, 1000*(size.y - (particle.Rect.Y))/size.y, particle.Rect.Width * 10, particle.Rect.Height * 10));
//                }

//                //var rectangles = particleSpace.GetRects();
//                //foreach (var rect in rectangles)
//                //{
//                //    //buffer.Graphics.DrawRectangle(Pens.Black, new Rectangle((int)rect.x, (int)rect.y, (int)(rect. - rect.x, (int)(rect.Height - rect.y)));
//                //}

//                buffer.Graphics.DrawEllipse(Pens.Black, (particleEmitter.location.X) - 5, (particleEmitter.location.Y) - 5, 10, 10);

//                buffer.Render();
//                buffer.Dispose();

//            }
//        }
//    }
//}
