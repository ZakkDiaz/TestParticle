using ParticleLib.Models;
using ParticleLib.Models._3D;
using ParticleLib.Models.Entities;
using System.Numerics;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows.Forms;
using Point = SharpDX.Point;
using Vector3 = SharpDX.Vector3;
using SharpDX.D3DCompiler;
using System.Linq;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace Ocdisplay
{
    public partial class Form1 : Form
    {
        private SharpDX.Direct3D11.Device device;
        private SwapChain swapChain;
        private RenderTargetView renderTargetView;
        private System.Windows.Forms.Timer renderTimer;

        bool _init = false;
        ParticleSpace3D _octree;
        ParticleEmitter particleEmitter = new ParticleEmitter();

        int depth = 10;
        int width = 10;
        int height = 10;
        bool _mouseDown = false;
        bool _mouseUp = false;
        BackgroundWorker adder;
        BackgroundWorker physics;

        private SharpDX.Direct3D11.Buffer vertexBuffer;
        private Buffer indexBuffer;
        private VertexShader vertexShader;
        private PixelShader pixelShader;
        private InputLayout inputLayout;

        private AAABBB Bounds;
        public Form1()
        {
            InitializeComponent();

            this.DoubleBuffered = true;
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.MouseDown += Form1_MouseDown;
            this.MouseUp += Form1_MouseUp;

            System.Threading.Thread.Sleep(1000);

            Init();




            adder = new BackgroundWorker();
            adder.DoWork += Adder_DoWork;
            adder.RunWorkerAsync();
            physics = new BackgroundWorker();
            physics.DoWork += Physics_DoWork;
            physics.RunWorkerAsync();

        }

        private void Physics_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                if (_octree != null)
                {
                    _octree.ProcessTimestep(.001f, Vector3.Zero, Vector3.Zero);
                }
                System.Threading.Thread.Sleep(100);
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            _mouseDown = false;
            _mouseUp = true;
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            _mouseDown = true;
            _mouseUp = false;
        }

        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            var location = new Vector3(e.X, e.Y, 250);
            particleEmitter.EmitParticle(ref _octree, location, Vector3.Zero, Vector3.Zero, Bounds);
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


        private int radius = 25;
        private unsafe void Adder_DoWork(object sender, DoWorkEventArgs e)
        {
            var otSize = _octree.to - _octree.from;
            var otCenter = _octree.from + otSize / 2;
            while (true)
            {
                if (_mouseUp)
                    try
                    {
                        _mouseUp = false;

                        var cusrorPos = System.Windows.Forms.Cursor.Position;
                        for (var i = 0; i < 1; i++)
                        {
                            var cX = cusrorPos.X + (float)(ThreadSafeRandom.Next_s() * 2 * radius) - radius;
                            var cY = cusrorPos.Y + (float)(ThreadSafeRandom.Next_s() * 2 * radius) - radius;
                            var location = new Vector3(cX, cY, 500);
                            particleEmitter.EmitParticle(ref _octree, location, Vector3.Zero, Vector3.Zero, Bounds);
                        }
                        InitializeParticles();
                    }
                    catch (Exception ex)
                    {

                    }
                System.Threading.Thread.Sleep(1);
            }
        }

        private void Init()
        {
            _octree = new ParticleSpace3D(new Vector3(0, 0, 0), new Vector3(width, this.height, depth));
            InitializeDirectX();
            Bounds = new AAABBB(new Point3D(0, 0, 0), new Point3D(width, height, depth));
            _init = true;
        }

        private void InitializeDirectX()
        {
            var desc = new SwapChainDescription()
            {
                BufferCount = 1,
                ModeDescription = new ModeDescription(width, height, new Rational(60, 1), Format.R8G8B8A8_UNorm),
                IsWindowed = true,
                OutputHandle = Handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };

            SharpDX.Direct3D11.Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.None, desc, out device, out swapChain);

            using (var backBuffer = swapChain.GetBackBuffer<SharpDX.Direct3D11.Texture2D>(0))
            {
                renderTargetView = new RenderTargetView(device, backBuffer);
            }
            // Set a viewport that covers the entire window
            device.ImmediateContext.Rasterizer.SetViewport(new Viewport(0, 0, this.Width, this.Height));
            matrixBuffer = new Buffer(device, Utilities.SizeOf<Matrix>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

            // Orthographic projection to cover 10x10 units area
            var projection = Matrix.OrthoOffCenterLH(-5, 5, -5, 5, -10, 10);
            UpdateProjectionBuffer(projection);

            InitializeGraphics();
            InitializeParticles();

            SetupTimer();

        }
        private Buffer matrixBuffer;

        private void UpdateMatrixBuffer()
        {
            // Create transformation matrices
            var worldMatrix = Matrix.Identity;  // No transformation
            var viewMatrix = Matrix.LookAtLH(new Vector3(0, 0, -5), new Vector3(0, 0, 0), Vector3.UnitY);
            var projectionMatrix = Matrix.PerspectiveFovLH((float)Math.PI / 4, this.Width / (float)this.Height, 0.1f, 100.0f);

            var worldViewProjection = worldMatrix * viewMatrix * projectionMatrix;
            worldViewProjection.Transpose(); // HLSL expects column-major matrices

            // Update the buffer
            device.ImmediateContext.UpdateSubresource(ref worldViewProjection, matrixBuffer);

            // Bind the buffer to the vertex shader
            device.ImmediateContext.VertexShader.SetConstantBuffer(0, matrixBuffer);
        }


        private void UpdateProjectionBuffer(Matrix projection)
        {
            projection.Transpose(); // Transpose for HLSL
            var buffer = new Buffer(device, Utilities.SizeOf<Matrix>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            device.ImmediateContext.VertexShader.SetConstantBuffer(0, buffer);
            device.ImmediateContext.UpdateSubresource(ref projection, buffer);
        }
        private void SetupTimer()
        {
            renderTimer = new System.Windows.Forms.Timer();
            renderTimer.Interval = 16; // Roughly 60 FPS
            renderTimer.Tick += (sender, args) => RenderFrame();
            renderTimer.Start();
        }

        protected override void OnClosed(EventArgs e)
        {
            renderTargetView.Dispose();
            swapChain.Dispose();
            device.Dispose();
            base.OnClosed(e);
        }



        private List<ParticleEntity> _drawParticles;
        private void InitializeParticles()
        {
            // Assuming you have some data structure to store your particles
            _drawParticles = _octree.GetParticles();
            UpdateVertexBuffer();
            UpdateIndexBuffer();
        }

        private void UpdateVertexBuffer()
        {
            // Define vertices for a cube centered at the origin
            Vertex[] vertices = new Vertex[]
            {
                // Front face
                new Vertex(new Vector3(-0.5f, -0.5f, 0.5f), Color4.White),  // Vertex 0
                new Vertex(new Vector3(0.5f, -0.5f, 0.5f), Color4.White),   // Vertex 1
                new Vertex(new Vector3(-0.5f, 0.5f, 0.5f), Color4.White),   // Vertex 2
                new Vertex(new Vector3(0.5f, 0.5f, 0.5f), Color4.White),    // Vertex 3

                // Back face
                new Vertex(new Vector3(-0.5f, -0.5f, -0.5f), Color4.White), // Vertex 4
                new Vertex(new Vector3(0.5f, -0.5f, -0.5f), Color4.White),  // Vertex 5
                new Vertex(new Vector3(-0.5f, 0.5f, -0.5f), Color4.White),  // Vertex 6
                new Vertex(new Vector3(0.5f, 0.5f, -0.5f), Color4.White),   // Vertex 7
            };

            vertexBuffer?.Dispose(); // Dispose previous buffer if exists
            vertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, vertices);
            vertexCount = vertices.Length;
        }

        private void UpdateIndexBuffer()
        {
            // Define indices for the cube (each triplet represents a triangle)
            ushort[] indices = new ushort[]
            {
        // Front face
        0, 1, 2, 2, 1, 3,
        // Back face
        4, 6, 5, 5, 6, 7,
        // Left face
        4, 2, 6, 4, 0, 2,
        // Right face
        1, 5, 3, 3, 5, 7,
        // Top face
        2, 3, 6, 6, 3, 7,
        // Bottom face
        4, 5, 0, 0, 5, 1
            };

            indexBuffer?.Dispose(); // Dispose previous buffer if exists
            indexBuffer = Buffer.Create(device, BindFlags.IndexBuffer, indices);
            indexCount = indices.Length;
        }


        private void InitializeGraphics()
        {
            var shaderFlags = ShaderFlags.EnableStrictness;
#if DEBUG
            shaderFlags |= ShaderFlags.Debug;
#endif

            // Load and compile the vertex and pixel shaders
            var vertexShaderByteCode = ShaderBytecode.CompileFromFile("shader.hlsl", "VS", "vs_5_0", shaderFlags);
            if (vertexShaderByteCode.Message != null)
            {
                Console.WriteLine(vertexShaderByteCode.Message);
            }
            var pixelShaderByteCode = ShaderBytecode.CompileFromFile("shader.hlsl", "PS", "ps_5_0", shaderFlags);
            if(pixelShaderByteCode.Message != null)
            {
                Console.WriteLine(pixelShaderByteCode.Message);
            }
            vertexShader = new VertexShader(device, vertexShaderByteCode);
            pixelShader = new PixelShader(device, pixelShaderByteCode);

            // Create input layout
            inputLayout = new InputLayout(device, vertexShaderByteCode, Vertex.InputElements);
        }

        private int vertexCount;
        private int indexCount;
        private void RenderFrame()
        {
            UpdateMatrixBuffer();
            // Ensure the device context is correctly configured
            device.ImmediateContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBuffer, Utilities.SizeOf<Vertex>(), 0));
            device.ImmediateContext.InputAssembler.SetIndexBuffer(indexBuffer, Format.R16_UInt, 0);
            device.ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            device.ImmediateContext.InputAssembler.InputLayout = inputLayout;

            // Set the shaders
            device.ImmediateContext.VertexShader.Set(vertexShader);
            device.ImmediateContext.PixelShader.Set(pixelShader);

            // Clear the render target to a solid color
            device.ImmediateContext.ClearRenderTargetView(renderTargetView, new RawColor4(0, 0, 0, 1));  // Black background

            // Draw the indexed vertices in the vertex buffer
            device.ImmediateContext.DrawIndexed(indexCount, 0, 0);

            // Present the back buffer to the screen
            swapChain.Present(0, PresentFlags.None);
        }

    }
}
