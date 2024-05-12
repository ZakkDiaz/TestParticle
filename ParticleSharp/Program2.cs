//using Silk.NET.Windowing;
//using Silk.NET.OpenGL;
//using ParticleSharp.Models.Entities;
//using System;
//using ParticleSharp.Models;
//using System.Numerics;
//using ParticleLib.Models._3D;
//using V256d = System.Runtime.Intrinsics.Vector256<double>;
//using BarnesHut;
//using System.Runtime.Intrinsics;
//using System.Runtime.InteropServices;
//using static System.Windows.Forms.VisualStyles.VisualStyleElement;
//using System.Globalization;
//using Window = Silk.NET.Windowing.Window;

//unsafe class Program
//{
//    private static IWindow window;
//    private static GL gl;

//    private static int x = 500;
//    private static int y = 500;
//    private static int z = 500;
//    private static int w = 500;
//    private static int h = 500;


//    static unsafe void Main(string[] args)
//    {
//        var options = WindowOptions.Default;
//        options.Size = new Silk.NET.Maths.Vector2D<int>(w, h);
//        options.Title = "Particle Renderer";

//        window = Window.Create(options);

//        window.Load += OnLoad;
//        window.Update += OnUpdate;
//        window.Render += OnRender;

//        window.Run();
//    }

//    private static List<ParticleEntity> particles = new List<ParticleEntity>();


//    private static Matrix4x4 projectionMatrix;
//    private static Matrix4x4 viewMatrix;
//    private static uint vbo; // Vertex Buffer Object
//    private static uint vao; // Vertex Array Object
//    private  static void OnLoad()
//    {
//        gl = GL.GetApi(window);
//        SetupShader();

//        // Define the orthographic projection matrix
//        float aspectRatio = w / h;
//        projectionMatrix = Matrix4x4.CreateOrthographicOffCenter(-x * aspectRatio, x * aspectRatio, -y, y, -z, z);
//        viewMatrix = Matrix4x4.Identity; // No camera movement, identity matrix

//        // Set up VAO and VBO without specific particle data
//        vao = gl.GenVertexArray();
//        gl.BindVertexArray(vao);

//        vbo = gl.GenBuffer();
//        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);

//        // Since we don't know the particle count yet, allocate no initial data
//        gl.BufferData(BufferTargetARB.ArrayBuffer, 0, null, BufferUsageARB.DynamicDraw);

//        // Set up vertex attribute
//        gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), null);
//        gl.EnableVertexAttribArray(0);

//        // Unbind the VBO and VAO to prevent accidental modification
//        gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
//        gl.BindVertexArray(0);

//        // Call InitSystem with the aligned memory
//        Net60_NBody_AVX_9_3b.InitSystem(out _m, out _p, out _v);
//    }

//    static double[] _m;
//    static V256d[] _p;
//    static V256d[] _v;

//    private unsafe static void OnUpdate(double deltaTime)
//    {

//        Net60_NBody_AVX_9_3b.Advance(10000, deltaTime, _m, _p, _v);

//        UpdateParticlesFromSimulation();

//        if (particles != null && particles.Count > 0)
//        {
//            float[] vertexData = new float[particles.Count * 3];
//            for (int i = 0; i < particles.Count; i++)
//            {
//                int index = i * 3;
//                vertexData[index] = particles[i].pos().X;
//                vertexData[index + 1] = particles[i].pos().Y;
//                vertexData[index + 2] = particles[i].pos().Z;
//            }

//            // Update the buffer with the new vertex data
//            gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);

//            // Pin the vertices array in memory and pass a pointer to gl.BufferData
//            fixed (float* vertexPtr = vertexData)
//            {
//                gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(vertexData.Length * sizeof(float)), vertexPtr, BufferUsageARB.DynamicDraw);
//            }
//            gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
//        }
//    }
//    // Convert simulation data to ParticleEntity
//    private static void UpdateParticlesFromSimulation()
//    {
//        particles.Clear();

//        for (int i = 0; i < _p.Length; i++)
//        {
//            // Read position from _p
//            double x = _p[i].GetElement(0);
//            double y = _p[i].GetElement(1);
//            double z = _p[i].GetElement(2);

//            // Create a new particle entity
//            var particle = new ParticleEntity() { Location = new Vector3((float)x, (float)y, (float)z) };
//            particle.ParticleInit(1, (float)_m[i], (float)x, (float)y, (float)z);



//            // Optionally read velocity and update particle velocity

//            // Add to particles list
//            particles.Add(particle);
//        }
//    }

//    private static void OnRender(double deltaTime)
//    {
//        gl.Clear((uint)ClearBufferMask.ColorBufferBit);

//        if (particles != null && particles.Count > 0)
//        {
//            gl.UseProgram(shaderProgram);

//            // Get uniform locations
//            int projectionLocation = gl.GetUniformLocation(shaderProgram, "projection");
//            int viewLocation = gl.GetUniformLocation(shaderProgram, "view");

//            // Manually convert Matrix4x4 to a float array
//            float[] projectionArray = MatrixToArray(projectionMatrix);
//            float[] viewArray = MatrixToArray(viewMatrix);

//            // Upload the matrices to the shader
//            unsafe
//            {
//                fixed (float* projPtr = projectionArray)
//                {
//                    gl.UniformMatrix4(projectionLocation, 1, false, projPtr);
//                }
//                fixed (float* viewPtr = viewArray)
//                {
//                    gl.UniformMatrix4(viewLocation, 1, false, viewPtr);
//                }
//            }

//            gl.BindVertexArray(vao);
//            gl.DrawArrays((GLEnum)PrimitiveType.Points, 0, (uint)particles.Count);
//            gl.BindVertexArray(0);
//        }
//    }

//    private static float[] MatrixToArray(Matrix4x4 matrix)
//    {
//        return new float[]
//        {
//        matrix.M11, matrix.M12, matrix.M13, matrix.M14,
//        matrix.M21, matrix.M22, matrix.M23, matrix.M24,
//        matrix.M31, matrix.M32, matrix.M33, matrix.M34,
//        matrix.M41, matrix.M42, matrix.M43, matrix.M44
//        };
//    }


//    private static void RenderParticles()
//    {
//        gl.UseProgram(shaderProgram);
//        gl.BindVertexArray(vao);  // Bind the VAO which holds the state
//        gl.DrawArrays(PrimitiveType.Points, 0, 1); // Draw one point
//        gl.BindVertexArray(0); // Unbind the VAO to prevent accidental modification
//    }

//    private static uint shaderProgram;
//    private static void SetupShader()
//    {
//        var vertexShaderSource = @"
//        #version 330 core
//        layout (location = 0) in vec3 aPosition;

//        uniform mat4 projection;
//        uniform mat4 view;

//        void main()
//        {
//            gl_Position = projection * view * vec4(aPosition, 1.0);
//        }";
//        var fragmentShaderSource = @"
//        #version 330 core
//        out vec4 FragColor;
//        void main()
//        {
//            FragColor = vec4(1.0, 0.5, 0.2, 1.0);
//        }";

//        var vertexShader = gl.CreateShader(ShaderType.VertexShader);
//        gl.ShaderSource(vertexShader, vertexShaderSource);
//        gl.CompileShader(vertexShader);
//        var fragmentShader = gl.CreateShader(ShaderType.FragmentShader);
//        gl.ShaderSource(fragmentShader, fragmentShaderSource);
//        gl.CompileShader(fragmentShader);

//        shaderProgram = gl.CreateProgram();
//        gl.AttachShader(shaderProgram, vertexShader);
//        gl.AttachShader(shaderProgram, fragmentShader);
//        gl.LinkProgram(shaderProgram);

//        gl.DetachShader(shaderProgram, vertexShader);
//        gl.DetachShader(shaderProgram, fragmentShader);
//        gl.DeleteShader(vertexShader);
//        gl.DeleteShader(fragmentShader);
//    }
//}
