using ParticleLib.Modern.Models;
using ParticleLib.Modern.Rendering;
using System.Diagnostics;
using System.Numerics;

namespace ModernParticleDemo;

public partial class ParticleForm : Form
{
    // Particle simulation components
    private Octree _octree;
    private ParticlePhysics _particlePhysics;
    private IParticleRenderer _renderer;
    
    // UI and rendering
    private BufferedGraphics _buffer;
    private BufferedGraphicsContext _context;
    private readonly Stopwatch _frameTimer = new Stopwatch();
    private readonly Stopwatch _performanceTimer = new Stopwatch();
    private Point3D _emitterPosition;
    private float _particleSize = 1.0f;
    private int _frameCount = 0;
    private float _fps = 0;
    private int _particleCount = 0;
    private bool _isRunning = true;
    private Bitmap _particleImage;
    
    // Constants
    private const float BOUNDS_SIZE = 100.0f;
    private const int MAX_PARTICLES_PER_CLICK = 100;
    private const float EMITTER_MOVE_SPEED = 5.0f;
    
    // Key tracking
    private readonly HashSet<Keys> _pressedKeys = new HashSet<Keys>();
    private readonly object _keyLock = new object();

    public ParticleForm()
    {
        // Form setup
        Text = "Modern Particle Demo";
        ClientSize = new Size(1024, 768);
        DoubleBuffered = true;
        
        // Initialize the octree with bounds
        var min = new Point3D(-BOUNDS_SIZE, -BOUNDS_SIZE, -BOUNDS_SIZE);
        var max = new Point3D(BOUNDS_SIZE, BOUNDS_SIZE, BOUNDS_SIZE);
        var bounds = new AAABBB(min, max);
        _octree = new Octree(min, max);
        
        // Initialize particle physics
        _particlePhysics = new ParticlePhysics(_octree);
        _particlePhysics.BoundaryCondition = BoundaryConditionType.Reflective;
        _particlePhysics.SnapshotInterval = 1; // Take snapshot every frame
        
        // Set initial emitter position
        _emitterPosition = new Point3D(0, 0, 0);
        
        // Load particle image
        try
        {
            string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Images", "Spark.png");
            if (File.Exists(imagePath))
            {
                _particleImage = new Bitmap(imagePath);
            }
            else
            {
                // Create a fallback image if the file doesn't exist
                _particleImage = new Bitmap(16, 16);
                using (var g = Graphics.FromImage(_particleImage))
                {
                    g.Clear(Color.Yellow);
                    g.DrawEllipse(Pens.Orange, 0, 0, 15, 15);
                    g.FillEllipse(Brushes.Orange, 2, 2, 12, 12);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading particle image: {ex.Message}");
            // Create a fallback image
            _particleImage = new Bitmap(16, 16);
            using (var g = Graphics.FromImage(_particleImage))
            {
                g.Clear(Color.Yellow);
                g.DrawEllipse(Pens.Orange, 0, 0, 15, 15);
                g.FillEllipse(Brushes.Orange, 2, 2, 12, 12);
            }
        }
        
        // Set up event handlers
        Paint += ParticleForm_Paint;
        KeyDown += ParticleForm_KeyDown;
        KeyUp += ParticleForm_KeyUp;
        MouseClick += ParticleForm_MouseClick;
        
        // Start the animation timer
        var timer = new System.Windows.Forms.Timer();
        timer.Interval = 16; // ~60 FPS
        timer.Tick += Timer_Tick;
        timer.Start();
        
        // Start performance timer
        _frameTimer.Start();
        _performanceTimer.Start();
    }
    
    private void Timer_Tick(object sender, EventArgs e)
    {
        if (!_isRunning)
            return;
            
        // Update particle physics
        _particlePhysics.Update(0.016f); // 16ms frame time
        
        // Handle keyboard input
        ProcessInput();
        
        // Update FPS counter every second
        _frameCount++;
        if (_performanceTimer.ElapsedMilliseconds > 1000)
        {
            _fps = _frameCount / (_performanceTimer.ElapsedMilliseconds / 1000.0f);
            _frameCount = 0;
            _performanceTimer.Restart();
            
            // Update particle count
            _particleCount = _particlePhysics.GetAllParticles().Count;
        }
        
        // Trigger repaint
        Invalidate();
    }
    
    private void ProcessInput()
    {
        lock (_keyLock)
        {
            // Move emitter based on pressed keys
            if (_pressedKeys.Contains(Keys.W))
                _emitterPosition = new Point3D(_emitterPosition.X, _emitterPosition.Y + EMITTER_MOVE_SPEED, _emitterPosition.Z);
                
            if (_pressedKeys.Contains(Keys.S))
                _emitterPosition = new Point3D(_emitterPosition.X, _emitterPosition.Y - EMITTER_MOVE_SPEED, _emitterPosition.Z);
                
            if (_pressedKeys.Contains(Keys.A))
                _emitterPosition = new Point3D(_emitterPosition.X - EMITTER_MOVE_SPEED, _emitterPosition.Y, _emitterPosition.Z);
                
            if (_pressedKeys.Contains(Keys.D))
                _emitterPosition = new Point3D(_emitterPosition.X + EMITTER_MOVE_SPEED, _emitterPosition.Y, _emitterPosition.Z);
                
            if (_pressedKeys.Contains(Keys.Q))
                _emitterPosition = new Point3D(_emitterPosition.X, _emitterPosition.Y, _emitterPosition.Z - EMITTER_MOVE_SPEED);
                
            if (_pressedKeys.Contains(Keys.E))
                _emitterPosition = new Point3D(_emitterPosition.X, _emitterPosition.Y, _emitterPosition.Z + EMITTER_MOVE_SPEED);
                
            // Adjust particle size
            if (_pressedKeys.Contains(Keys.Add) || _pressedKeys.Contains(Keys.Oemplus))
                _particleSize = Math.Min(5.0f, _particleSize + 0.1f);
                
            if (_pressedKeys.Contains(Keys.Subtract) || _pressedKeys.Contains(Keys.OemMinus))
                _particleSize = Math.Max(0.1f, _particleSize - 0.1f);
                
            // Pause/resume simulation
            if (_pressedKeys.Contains(Keys.P))
            {
                _isRunning = !_isRunning;
                _pressedKeys.Remove(Keys.P); // Consume the key press
            }
            
            // Clear all particles
            if (_pressedKeys.Contains(Keys.C))
            {
                ClearParticles();
                _pressedKeys.Remove(Keys.C); // Consume the key press
            }
        }
    }
    
    private void ClearParticles()
    {
        // Recreate the octree and physics
        var min = new Point3D(-BOUNDS_SIZE, -BOUNDS_SIZE, -BOUNDS_SIZE);
        var max = new Point3D(BOUNDS_SIZE, BOUNDS_SIZE, BOUNDS_SIZE);
        var bounds = new AAABBB(min, max);
        _octree = new Octree(min, max);
        _particlePhysics = new ParticlePhysics(_octree);
        _particlePhysics.BoundaryCondition = BoundaryConditionType.Reflective;
        _particlePhysics.SnapshotInterval = 1;
    }
    
    private void AddParticles(Point3D position, int count)
    {
        try
        {
            // Create a batch of particles to add
            var particleBatch = new List<(Point3D position, Vector3 velocity, float mass)>();
            
            for (int i = 0; i < count; i++)
            {
                // Add some randomness to position
                float offsetX = (float)Random.Shared.NextDouble() * 2 - 1;
                float offsetY = (float)Random.Shared.NextDouble() * 2 - 1;
                float offsetZ = (float)Random.Shared.NextDouble() * 2 - 1;
                
                var pos = new Point3D(
                    position.X + offsetX * 2,
                    position.Y + offsetY * 2,
                    position.Z + offsetZ * 2
                );
                
                // Random velocity
                var vel = new Vector3(
                    (float)Random.Shared.NextDouble() * 10 - 5,
                    (float)Random.Shared.NextDouble() * 10 - 5,
                    (float)Random.Shared.NextDouble() * 10 - 5
                );
                
                // Random mass between 0.5 and 2.0
                float mass = (float)Random.Shared.NextDouble() * 1.5f + 0.5f;
                
                particleBatch.Add((pos, vel, mass));
            }
            
            // Add particles in batch
            _particlePhysics.AddParticles(particleBatch);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error adding particles: {ex.Message}");
        }
    }
    
    private void ParticleForm_Paint(object sender, PaintEventArgs e)
    {
        // Calculate the frame time
        float frameTime = _frameTimer.ElapsedMilliseconds / 1000.0f;
        _frameTimer.Restart();
        
        // Create or recreate the buffer if needed
        if (_buffer == null || _context == null)
        {
            _context = BufferedGraphicsManager.Current;
            _context.MaximumBuffer = new Size(Width + 1, Height + 1);
            _buffer = _context.Allocate(e.Graphics, ClientRectangle);
        }
        
        // Clear the background
        _buffer.Graphics.Clear(Color.Black);
        
        // Get the latest snapshot
        var snapshot = _particlePhysics.LatestSnapshot;
        
        // Draw the particles
        foreach (var particle in snapshot.Particles)
        {
            // Convert 3D coordinates to 2D screen coordinates
            float screenX = ClientSize.Width / 2 + particle.Position.X * 3;
            float screenY = ClientSize.Height / 2 - particle.Position.Y * 3;
            
            // Scale by mass
            float size = particle.Mass * 10 * _particleSize;
            
            // Draw the particle
            _buffer.Graphics.DrawImage(
                _particleImage,
                screenX - size / 2,
                screenY - size / 2,
                size,
                size
            );
        }
        
        // Draw the octree nodes (optional)
        foreach (var node in snapshot.Nodes)
        {
            // Only draw nodes that have particles or are at a low depth
            if (node.HasParticles || node.Depth < 3)
            {
                // Convert 3D bounding box to 2D rectangle
                float centerX = ClientSize.Width / 2 + (node.BoundingBox.Min.X + node.BoundingBox.Max.X) / 2 * 3;
                float centerY = ClientSize.Height / 2 - (node.BoundingBox.Min.Y + node.BoundingBox.Max.Y) / 2 * 3;
                float width = (node.BoundingBox.Max.X - node.BoundingBox.Min.X) * 3;
                float height = (node.BoundingBox.Max.Y - node.BoundingBox.Min.Y) * 3;
                
                // Draw the node
                var pen = node.HasParticles ? Pens.Green : Pens.Gray;
                _buffer.Graphics.DrawRectangle(
                    pen,
                    centerX - width / 2,
                    centerY - height / 2,
                    width,
                    height
                );
            }
        }
        
        // Draw the emitter
        float emitterX = ClientSize.Width / 2 + _emitterPosition.X * 3;
        float emitterY = ClientSize.Height / 2 - _emitterPosition.Y * 3;
        _buffer.Graphics.FillEllipse(Brushes.Red, emitterX - 5, emitterY - 5, 10, 10);
        
        // Draw stats
        _buffer.Graphics.DrawString(
            $"FPS: {_fps:F1} | Particles: {_particleCount} | Emitter: ({_emitterPosition.X:F1}, {_emitterPosition.Y:F1}, {_emitterPosition.Z:F1}) | Size: {_particleSize:F1}",
            SystemFonts.DefaultFont,
            Brushes.White,
            10,
            10
        );
        
        // Draw controls help
        _buffer.Graphics.DrawString(
            "Controls: WASD/QE - Move Emitter | +/- - Adjust Size | Space - Add Particles | P - Pause | C - Clear",
            SystemFonts.DefaultFont,
            Brushes.White,
            10,
            30
        );
        
        // Render the buffer to the screen
        _buffer.Render(e.Graphics);
    }
    
    private void ParticleForm_KeyDown(object sender, KeyEventArgs e)
    {
        lock (_keyLock)
        {
            _pressedKeys.Add(e.KeyCode);
            
            // Handle space key to add particles
            if (e.KeyCode == Keys.Space)
            {
                AddParticles(_emitterPosition, MAX_PARTICLES_PER_CLICK);
                _pressedKeys.Remove(Keys.Space); // Consume the key press
            }
        }
    }
    
    private void ParticleForm_KeyUp(object sender, KeyEventArgs e)
    {
        lock (_keyLock)
        {
            _pressedKeys.Remove(e.KeyCode);
        }
    }
    
    private void ParticleForm_MouseClick(object sender, MouseEventArgs e)
    {
        // Convert screen coordinates to world coordinates
        float worldX = (e.X - ClientSize.Width / 2) / 3.0f;
        float worldY = -(e.Y - ClientSize.Height / 2) / 3.0f;
        
        // Create a new emitter position
        var position = new Point3D(worldX, worldY, _emitterPosition.Z);
        
        // Add particles at click position
        AddParticles(position, MAX_PARTICLES_PER_CLICK / 2);
    }
    
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _buffer?.Dispose();
            _particleImage?.Dispose();
        }
        base.Dispose(disposing);
    }
}
