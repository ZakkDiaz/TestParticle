using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using ParticleSharp.Models.Entities;
using System;
using ParticleSharp.Models;
using System.Numerics;
using ParticleLib.Models._3D;
using Silk.NET.Input;

class Program
{
    private static Vector2? lastRightMousePosition;
    private static bool isRotating = false;
    private static Vector2? lastMousePosition;
    private static bool isPanning = false;
    private static IInputContext inputContext;
    private static IWindow window;
    private static IMouse mouse;
    private static GL gl;
    private static float zoomLevel = 1.0f; // Initial zoom level


    private static int x = 1250000;
    private static int y = 1250000;
    private static int z = 1250000;
    private static int w = 2560;
    private static int h = 1440;

    private static ParticleSpace3D pspace = new ParticleSpace3D(new Vector3(-x, -y, -z), new Vector3(x, y, z));
    private static ParticleEmitter emitter = new ParticleEmitter();

    static void Main(string[] args)
    {
        var options = WindowOptions.Default;
        options.Size = new Silk.NET.Maths.Vector2D<int>(w, h);
        options.Title = "Particle Renderer";

        window = Window.Create(options);

        window.Load += OnLoad;
        window.Update += OnUpdate;
        window.Render += OnRender;

        window.Run();
    }

    private static List<ParticleEntity> particles = new List<ParticleEntity>();

    private static void UpdateProjectionMatrix()
    {
        float aspectRatio = (float)w / h;
        projectionMatrix = Matrix4x4.CreateOrthographicOffCenter(
            -x * aspectRatio * zoomLevel, x * aspectRatio * zoomLevel,
            -y * zoomLevel, y * zoomLevel,
            -z * zoomLevel, z * zoomLevel);
    }

    private static void OnMouseDown(IMouse mouse, MouseButton button)
    {
        if (button == MouseButton.Left)
        {
            isPanning = true;
            lastMousePosition = mouse.Position;
        }
        else if (button == MouseButton.Right)
        {
            isRotating = true;
            lastRightMousePosition = mouse.Position;
        }
    }

    private static void OnMouseUp(IMouse mouse, MouseButton button)
    {
        if (button == MouseButton.Left)
        {
            isPanning = false;
            lastMousePosition = null;
        }
        else if (button == MouseButton.Right)
        {
            isRotating = false;
            lastRightMousePosition = null;
        }
    }

    private static void OnMouseMove(IMouse mouse, Vector2 position)
    {
        if (isPanning && lastMousePosition.HasValue)
        {
            Vector2 delta = position - lastMousePosition.Value;
            float panSpeed = zoomLevel * 250;

            // Adjust pan direction based on the current view matrix rotation
            var rightDirection = new Vector3(viewMatrix.M11, viewMatrix.M21, viewMatrix.M31); // Right direction
            var upDirection = new Vector3(viewMatrix.M12, viewMatrix.M22, viewMatrix.M32); // Up direction

            var panTranslation = (rightDirection * delta.X + upDirection * -delta.Y) * panSpeed;

            // Apply translation to the view matrix
            viewMatrix = Matrix4x4.CreateTranslation(panTranslation) * viewMatrix;

            lastMousePosition = position;
        }
        else if (isRotating && lastRightMousePosition.HasValue)
        {
            Vector2 delta = position - lastRightMousePosition.Value;
            float rotationSpeed = 0.01f; // Adjust this value to control rotation speed

            // Apply rotation based on mouse movement
            viewMatrix = Matrix4x4.CreateRotationY(delta.X * rotationSpeed) * viewMatrix;
            viewMatrix = Matrix4x4.CreateRotationX(-delta.Y * rotationSpeed) * viewMatrix;

            lastRightMousePosition = position;
        }
    }


    private static Matrix4x4 projectionMatrix;
    private static Matrix4x4 viewMatrix;
    private static uint vbo; // Vertex Buffer Object
    private static uint vao; // Vertex Array Object
    private unsafe static void OnLoad()
    {
        inputContext = window.CreateInput();
        mouse = inputContext.Mice.FirstOrDefault(); // Use the first available mouse
        if (mouse != null)
        {
            mouse.Scroll += OnMouseScroll; // Scroll event handler
            mouse.MouseDown += OnMouseDown; // Mouse down event handler
            mouse.MouseUp += OnMouseUp; // Mouse up event handler
            mouse.MouseMove += OnMouseMove; // Mouse move event handler
        }
        gl = GL.GetApi(window);
        SetupShader();

        // Define the orthographic projection matrix
        float aspectRatio = w / h;
        projectionMatrix = Matrix4x4.CreateOrthographicOffCenter(-x * aspectRatio, x * aspectRatio, -y, y, -z, z);
        viewMatrix = Matrix4x4.Identity; // No camera movement, identity matrix

        // Set up VAO and VBO without specific particle data
        vao = gl.GenVertexArray();
        gl.BindVertexArray(vao);

        vbo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);

        // Since we don't know the particle count yet, allocate no initial data
        gl.BufferData(BufferTargetARB.ArrayBuffer, 0, null, BufferUsageARB.DynamicDraw);

        // Set up vertex attribute
        gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), null);
        gl.EnableVertexAttribArray(0);

        // Unbind the VBO and VAO to prevent accidental modification
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        gl.BindVertexArray(0);
    }
    private static void OnMouseScroll(IMouse mouse, ScrollWheel scrollWheel)
    {
        zoomLevel *= scrollWheel.Y > 0 ? 0.9f : 1.1f; // Adjust zoom factor
        UpdateProjectionMatrix();
    }


    private static int minParticleCount = 25;
    private unsafe static void OnUpdate(double deltaTime)
    {
        var update = pspace.ProcessTimestep((float)deltaTime, Vector3.Zero, new AAABBB(new Point3D(pspace.from.X, pspace.from.Y, pspace.from.Z), new Point3D(pspace.to.X, pspace.to.Y, pspace.to.Z)));
        if (update)
        {
            update = false;
            particles = pspace.GetParticles();

        }
        var numDif = minParticleCount - particles.Count();
        for (var i = 0; i < numDif; i++)
        {
            GetRandomSphereLocationAndVelocity(Vector3.Zero, 2000, out Vector3 location, out Vector3 velocity);
            var mass = ThreadSafeRandom.NextWeightedParticleSize();
            emitter.EmitParticle(
                ref pspace,
                location,
                Vector3.Zero,
                (velocity * 10) / (mass * mass),
                new AAABBB(new Point3D(pspace.from.X, pspace.from.Y, pspace.from.Z), new Point3D(pspace.to.X, pspace.to.Y, pspace.to.Z)),
                particleSize: mass
            );
        }
        particles = pspace.GetParticles();
        if (particles != null && particles.Count > 0)
        {
            float[] vertexData = new float[particles.Count * 3];
            for (int i = 0; i < particles.Count; i++)
            {
                int index = i * 3;
                vertexData[index] = particles[i].pos().X;
                vertexData[index + 1] = particles[i].pos().Y;
                vertexData[index + 2] = particles[i].pos().Z;
            }

            // Update the buffer with the new vertex data
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);

            // Pin the vertices array in memory and pass a pointer to gl.BufferData
            fixed (float* vertexPtr = vertexData)
            {
                gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(vertexData.Length * sizeof(float)), vertexPtr, BufferUsageARB.DynamicDraw);
            }
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        }
    }


    public static void GetRandomSphereLocationAndVelocity(Vector3 center, float radius, out Vector3 location, out Vector3 velocity)
    {
        // Generate random azimuthal and polar angles
        float phi = ThreadSafeRandom.Next(0, 2 * MathF.PI); // Azimuthal angle
        float theta = ThreadSafeRandom.Next(0, MathF.PI); // Polar angle

        // Calculate the position on the sphere
        float sinTheta = MathF.Sin(theta);
        float x = center.X + radius * sinTheta * MathF.Cos(phi);
        float y = center.Y + radius * sinTheta * MathF.Sin(phi);
        float z = center.Z + radius * MathF.Cos(theta);

        location = new Vector3(x, y, z);

        // Calculate the tangential velocity (perpendicular to the radius)
        float vx = sinTheta * -MathF.Sin(phi);
        float vy = sinTheta * MathF.Cos(phi);
        float vz = MathF.Cos(theta);

        velocity = new Vector3(vx, vy, vz);

        // Optionally scale the velocity for a realistic speed
        velocity *= 1.0f; // Adjust this scalar value as needed for desired speed
    }


    private static void OnRender(double deltaTime)
    {
        gl.Clear((uint)ClearBufferMask.ColorBufferBit);
        UpdateProjectionMatrix(); // Update the projection matrix before rendering

        if (particles != null && particles.Count > 0)
        {
            gl.UseProgram(shaderProgram);

            // Get uniform locations
            int projectionLocation = gl.GetUniformLocation(shaderProgram, "projection");
            int viewLocation = gl.GetUniformLocation(shaderProgram, "view");

            // Manually convert Matrix4x4 to a float array
            float[] projectionArray = MatrixToArray(projectionMatrix);
            float[] viewArray = MatrixToArray(viewMatrix);

            // Upload the matrices to the shader
            unsafe
            {
                fixed (float* projPtr = projectionArray)
                {
                    gl.UniformMatrix4(projectionLocation, 1, false, projPtr);
                }
                fixed (float* viewPtr = viewArray)
                {
                    gl.UniformMatrix4(viewLocation, 1, false, viewPtr);
                }
            }

            gl.BindVertexArray(vao);
            gl.DrawArrays((GLEnum)PrimitiveType.Points, 0, (uint)particles.Count);
            gl.BindVertexArray(0);
        }
    }


    private static float[] MatrixToArray(Matrix4x4 matrix)
    {
        return new float[]
        {
        matrix.M11, matrix.M12, matrix.M13, matrix.M14,
        matrix.M21, matrix.M22, matrix.M23, matrix.M24,
        matrix.M31, matrix.M32, matrix.M33, matrix.M34,
        matrix.M41, matrix.M42, matrix.M43, matrix.M44
        };
    }


    private static void RenderParticles()
    {
        gl.UseProgram(shaderProgram);
        gl.BindVertexArray(vao);  // Bind the VAO which holds the state
        gl.DrawArrays(PrimitiveType.Points, 0, 1); // Draw one point
        gl.BindVertexArray(0); // Unbind the VAO to prevent accidental modification
    }

    private static uint shaderProgram;
    private static void SetupShader()
    {
        var vertexShaderSource = @"
        #version 330 core
        layout (location = 0) in vec3 aPosition;

        uniform mat4 projection;
        uniform mat4 view;

        void main()
        {
            gl_Position = projection * view * vec4(aPosition, 1.0);
        }";
        var fragmentShaderSource = @"
        #version 330 core
        out vec4 FragColor;
        void main()
        {
            FragColor = vec4(1.0, 0.5, 0.2, 1.0);
        }";

        var vertexShader = gl.CreateShader(ShaderType.VertexShader);
        gl.ShaderSource(vertexShader, vertexShaderSource);
        gl.CompileShader(vertexShader);
        var fragmentShader = gl.CreateShader(ShaderType.FragmentShader);
        gl.ShaderSource(fragmentShader, fragmentShaderSource);
        gl.CompileShader(fragmentShader);

        shaderProgram = gl.CreateProgram();
        gl.AttachShader(shaderProgram, vertexShader);
        gl.AttachShader(shaderProgram, fragmentShader);
        gl.LinkProgram(shaderProgram);

        gl.DetachShader(shaderProgram, vertexShader);
        gl.DetachShader(shaderProgram, fragmentShader);
        gl.DeleteShader(vertexShader);
        gl.DeleteShader(fragmentShader);
    }
}
