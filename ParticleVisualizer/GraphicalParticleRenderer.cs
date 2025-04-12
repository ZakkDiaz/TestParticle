using ParticleLib.Modern.Models;
using ParticleLib.Modern.Rendering;
using System.Drawing.Drawing2D;
using System.Numerics;

namespace ParticleVisualizer
{
    /// <summary>
    /// A Windows Forms-based renderer for particle simulations.
    /// </summary>
    public class GraphicalParticleRenderer : IParticleRenderer
    {
        private readonly PictureBox _canvas;
        private readonly List<RenderableParticle> _particles = new();
        private readonly List<RenderableNode> _nodes = new();
        private readonly Font _font = new("Arial", 8);
        private readonly Brush _textBrush = Brushes.White;
        private readonly Pen _nodePen = new(Color.FromArgb(40, 40, 40), 1);
        private readonly Pen _nodeWithParticlesPen = new(Color.FromArgb(60, 100, 60), 1);
        private int _framesRendered = 0;
        private DateTime _lastFrameTime = DateTime.UtcNow;
        private readonly List<double> _frameRates = new();
        
        // Camera settings
        private float _scale = 0.1f;
        private Vector2 _panOffset = new(0, 0);
        private Vector3 _cameraPosition = new(500, 500, 500); // Center of the world
        private float _rotationX = 0;
        private float _rotationY = 0;
        
        /// <summary>
        /// Initializes a new instance of the GraphicalParticleRenderer class.
        /// </summary>
        public GraphicalParticleRenderer(PictureBox canvas)
        {
            _canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
            
            // Set up the canvas
            _canvas.Paint += Canvas_Paint;
            
            // Set up mouse interaction for camera control
            _canvas.MouseWheel += Canvas_MouseWheel;
            _canvas.MouseDown += Canvas_MouseDown;
            _canvas.MouseMove += Canvas_MouseMove;
            _canvas.MouseUp += Canvas_MouseUp;
        }
        
        private bool _isDragging = false;
        private bool _isPanning = false;
        private Point _lastMousePosition;
        
        private void Canvas_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isDragging = true;
                _lastMousePosition = e.Location;
            }
            else if (e.Button == MouseButtons.Right)
            {
                _isPanning = true;
                _lastMousePosition = e.Location;
            }
        }
        
        private void Canvas_MouseMove(object? sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                int deltaX = e.X - _lastMousePosition.X;
                int deltaY = e.Y - _lastMousePosition.Y;
                
                _rotationY += deltaX * 0.01f;
                _rotationX += deltaY * 0.01f;
                
                _lastMousePosition = e.Location;
                _canvas.Invalidate();
            }
            else if (_isPanning)
            {
                int deltaX = e.X - _lastMousePosition.X;
                int deltaY = e.Y - _lastMousePosition.Y;
                
                // Update pan offset
                _panOffset.X += deltaX;
                _panOffset.Y += deltaY;
                
                _lastMousePosition = e.Location;
                _canvas.Invalidate();
            }
        }
        
        private void Canvas_MouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isDragging = false;
            }
            else if (e.Button == MouseButtons.Right)
            {
                _isPanning = false;
            }
        }
        
        private void Canvas_MouseWheel(object? sender, MouseEventArgs e)
        {
            // Adjust scale based on mouse wheel
            _scale *= e.Delta > 0 ? 1.1f : 0.9f;
            _canvas.Invalidate();
        }
        
        /// <summary>
        /// Begins a new rendering frame.
        /// </summary>
        public void BeginFrame()
        {
            _particles.Clear();
            _nodes.Clear();
        }
        
        /// <summary>
        /// Renders a particle at the specified position with the given velocity.
        /// </summary>
        public void RenderParticle(Point3D position, Vector3 velocity, Vector4? color = null)
        {
            Color particleColor = Color.White;
            
            // Determine color based on velocity
            if (color.HasValue)
            {
                particleColor = Color.FromArgb(
                    (int)(color.Value.W * 255),
                    (int)(color.Value.X * 255),
                    (int)(color.Value.Y * 255),
                    (int)(color.Value.Z * 255)
                );
            }
            else
            {
                // Color based on velocity
                float speed = velocity.Length();
                if (speed > 0)
                {
                    // Normalize velocity and use it for color
                    Vector3 normalizedVelocity = Vector3.Normalize(velocity);
                    
                    // Map velocity direction to color (RGB)
                    particleColor = Color.FromArgb(
                        255,
                        (int)(Math.Abs(normalizedVelocity.X) * 255),
                        (int)(Math.Abs(normalizedVelocity.Y) * 255),
                        (int)(Math.Abs(normalizedVelocity.Z) * 255)
                    );
                }
            }
            
            _particles.Add(new RenderableParticle(position, velocity, particleColor));
        }
        
        /// <summary>
        /// Renders an octree node's bounding box.
        /// </summary>
        public void RenderOctreeNode(AAABBB bounds, int depth, bool hasParticles)
        {
            // Only render nodes up to a certain depth to avoid clutter
            if (depth <= 5)
            {
                _nodes.Add(new RenderableNode(bounds, depth, hasParticles));
            }
        }
        
        /// <summary>
        /// Ends the current rendering frame.
        /// </summary>
        public void EndFrame()
        {
            _framesRendered++;
            
            // Calculate frame rate
            DateTime now = DateTime.UtcNow;
            double frameTime = (now - _lastFrameTime).TotalSeconds;
            _lastFrameTime = now;
            
            if (frameTime > 0)
            {
                _frameRates.Add(1.0 / frameTime);
                
                // Keep only the last 60 frame rates for averaging
                if (_frameRates.Count > 60)
                {
                    _frameRates.RemoveAt(0);
                }
            }
            
            // Trigger a repaint
            _canvas.Invalidate();
        }
        
        private void Canvas_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Color.Black);
            
            // Enable anti-aliasing
            g.SmoothingMode = SmoothingMode.AntiAlias;
            
            // Set up transformation matrix for 3D projection
            Matrix3x2 viewMatrix = CreateViewMatrix();
            g.Transform = new Matrix(viewMatrix.M11, viewMatrix.M12, viewMatrix.M21, viewMatrix.M22, viewMatrix.M31, viewMatrix.M32);
            
            // Draw octree nodes
            foreach (var node in _nodes)
            {
                DrawNode(g, node);
            }
            
            // Draw particles
            foreach (var particle in _particles)
            {
                DrawParticle(g, particle);
            }
            
            // Reset transformation for UI elements
            g.ResetTransform();
            
            // Draw stats
            DrawStats(g);
        }
        
        private Matrix3x2 CreateViewMatrix()
        {
            // Create rotation matrices
            float cosX = (float)Math.Cos(_rotationX);
            float sinX = (float)Math.Sin(_rotationX);
            float cosY = (float)Math.Cos(_rotationY);
            float sinY = (float)Math.Sin(_rotationY);
            
            // Create view matrix with rotation, translation, and scaling
            Matrix3x2 rotationX = new(
                1, 0,
                0, cosX,
                0, sinX
            );
            
            Matrix3x2 rotationY = new(
                cosY, 0,
                0, 1,
                -sinY, 0
            );
            
            // Combine transformations
            Matrix3x2 rotation = rotationX * rotationY;
            
            // Create translation matrix to center the view
            Matrix3x2 translation = Matrix3x2.CreateTranslation(
                _canvas.Width / 2 + _panOffset.X,
                _canvas.Height / 2 + _panOffset.Y
            );
            
            // Create scaling matrix
            Matrix3x2 scaling = Matrix3x2.CreateScale(_scale);
            
            // Combine all transformations
            return rotation * scaling * translation;
        }
        
        private void DrawNode(Graphics g, RenderableNode node)
        {
            var bounds = node.Bounds;
            var min = bounds.Min;
            var max = bounds.Max;
            
            // Use different pen based on whether the node has particles
            Pen pen = node.HasParticles ? _nodeWithParticlesPen : _nodePen;
            
            // Draw the bounding box as a wireframe cube
            // Front face
            g.DrawLine(pen, Project(min.X, min.Y, min.Z), Project(max.X, min.Y, min.Z));
            g.DrawLine(pen, Project(max.X, min.Y, min.Z), Project(max.X, max.Y, min.Z));
            g.DrawLine(pen, Project(max.X, max.Y, min.Z), Project(min.X, max.Y, min.Z));
            g.DrawLine(pen, Project(min.X, max.Y, min.Z), Project(min.X, min.Y, min.Z));
            
            // Back face
            g.DrawLine(pen, Project(min.X, min.Y, max.Z), Project(max.X, min.Y, max.Z));
            g.DrawLine(pen, Project(max.X, min.Y, max.Z), Project(max.X, max.Y, max.Z));
            g.DrawLine(pen, Project(max.X, max.Y, max.Z), Project(min.X, max.Y, max.Z));
            g.DrawLine(pen, Project(min.X, max.Y, max.Z), Project(min.X, min.Y, max.Z));
            
            // Connecting edges
            g.DrawLine(pen, Project(min.X, min.Y, min.Z), Project(min.X, min.Y, max.Z));
            g.DrawLine(pen, Project(max.X, min.Y, min.Z), Project(max.X, min.Y, max.Z));
            g.DrawLine(pen, Project(max.X, max.Y, min.Z), Project(max.X, max.Y, max.Z));
            g.DrawLine(pen, Project(min.X, max.Y, min.Z), Project(min.X, max.Y, max.Z));
        }
        
        private void DrawParticle(Graphics g, RenderableParticle particle)
        {
            // Project the particle position to screen space
            PointF screenPos = Project(particle.Position.X, particle.Position.Y, particle.Position.Z);
            
            // Draw the particle as a small circle
            float size = 3f;
            using var brush = new SolidBrush(particle.Color);
            g.FillEllipse(brush, screenPos.X - size / 2, screenPos.Y - size / 2, size, size);
            
            // Draw velocity vector if significant
            if (particle.Velocity.Length() > 1f)
            {
                // Scale velocity for visualization
                Vector3 scaledVelocity = Vector3.Normalize(particle.Velocity) * 10f;
                
                // Calculate end point of velocity vector
                PointF endPoint = Project(
                    particle.Position.X + scaledVelocity.X,
                    particle.Position.Y + scaledVelocity.Y,
                    particle.Position.Z + scaledVelocity.Z
                );
                
                // Draw the velocity vector
                using var pen = new Pen(particle.Color, 1);
                g.DrawLine(pen, screenPos, endPoint);
            }
        }
        
        private void DrawStats(Graphics g)
        {
            // Draw statistics in the top-left corner
            string stats = $"Particles: {_particles.Count}\n" +
                          $"Octree Nodes: {_nodes.Count}\n" +
                          $"FPS: {(_frameRates.Count > 0 ? _frameRates.Average() : 0):F1}\n" +
                          $"Scale: {_scale:F2}x\n" +
                          $"Rotation: ({_rotationX:F2}, {_rotationY:F2})\n" +
                          $"Pan: ({_panOffset.X:F0}, {_panOffset.Y:F0})";
            
            // Draw with shadow for better visibility
            g.DrawString(stats, _font, Brushes.Black, 11, 11);
            g.DrawString(stats, _font, _textBrush, 10, 10);
        }
        
        private PointF Project(float x, float y, float z)
        {
            // Apply rotation around X axis
            float cosX = (float)Math.Cos(_rotationX);
            float sinX = (float)Math.Sin(_rotationX);
            float y2 = y * cosX - z * sinX;
            float z2 = y * sinX + z * cosX;
            
            // Apply rotation around Y axis
            float cosY = (float)Math.Cos(_rotationY);
            float sinY = (float)Math.Sin(_rotationY);
            float x2 = x * cosY + z2 * sinY;
            float z3 = -x * sinY + z2 * cosY;
            
            // Simple perspective projection
            float perspective = 1000.0f;
            float factor = perspective / (perspective + z3);
            
            // Apply perspective scaling
            float px = x2 * factor;
            float py = y2 * factor;
            
            return new PointF(px, py);
        }
        
        /// <summary>
        /// Resets the view to its default state.
        /// </summary>
        public void ResetView()
        {
            // Reset camera settings
            _scale = 0.1f;
            _panOffset = new Vector2(0, 0);
            _rotationX = 0;
            _rotationY = 0;
            
            // Trigger a repaint
            _canvas.Invalidate();
        }
        
        private class RenderableParticle
        {
            public Point3D Position { get; }
            public Vector3 Velocity { get; }
            public Color Color { get; }
            
            public RenderableParticle(Point3D position, Vector3 velocity, Color color)
            {
                Position = position;
                Velocity = velocity;
                Color = color;
            }
        }
        
        private class RenderableNode
        {
            public AAABBB Bounds { get; }
            public int Depth { get; }
            public bool HasParticles { get; }
            
            public RenderableNode(AAABBB bounds, int depth, bool hasParticles)
            {
                Bounds = bounds;
                Depth = depth;
                HasParticles = hasParticles;
            }
        }
    }
}
