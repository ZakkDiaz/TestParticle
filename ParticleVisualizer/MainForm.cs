using ParticleLib.Modern.Examples;
using ParticleLib.Modern.Models;
using System.Numerics;

namespace ParticleVisualizer
{
    public partial class MainForm : Form
    {
        private readonly SimulationExample _simulation;
        private readonly GraphicalParticleRenderer _renderer;
        private readonly System.Windows.Forms.Timer _renderTimer = new();
        private readonly System.Windows.Forms.Timer _statsTimer = new();
        
        // UI elements
        private PictureBox _canvas;
        private Panel _controlPanel;
        private Label _statsLabel;
        private Button _addParticlesButton;
        private Button _resetViewButton;
        private ComboBox _boundaryConditionComboBox;
        private TrackBar _particleCountTrackBar;
        private Label _particleCountLabel;
        
        public MainForm()
        {
            InitializeComponent();
            
            // Create the renderer
            _renderer = new GraphicalParticleRenderer(_canvas);
            
            // Create the simulation with 120 physics updates per second
            _simulation = new SimulationExample(_renderer, 120);
            
            // Set up the rendering timer (30 FPS)
            _renderTimer.Interval = 33; // ~30 FPS
            _renderTimer.Tick += (s, e) => _simulation.Render();
            
            // Set up the stats timer (1 update per second)
            _statsTimer.Interval = 1000;
            _statsTimer.Tick += UpdateStats;
            
            // Set up UI event handlers
            _addParticlesButton.Click += AddParticlesButton_Click;
            _boundaryConditionComboBox.SelectedIndexChanged += BoundaryConditionComboBox_SelectedIndexChanged;
            _particleCountTrackBar.ValueChanged += ParticleCountTrackBar_ValueChanged;
            _resetViewButton.Click += ResetViewButton_Click;
            
            // Start the simulation
            _simulation.Start();
            _renderTimer.Start();
            _statsTimer.Start();
        }
        
        private void InitializeComponent()
        {
            // Create the main form
            this.Text = "Particle Physics Visualizer";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            
            // Create the canvas
            _canvas = new PictureBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black
            };
            
            // Create the control panel
            _controlPanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 200,
                BackColor = Color.FromArgb(30, 30, 30),
                Padding = new Padding(10)
            };
            
            // Create stats label
            _statsLabel = new Label
            {
                Dock = DockStyle.Top,
                Height = 100,
                ForeColor = Color.White,
                Text = "Simulation Statistics:",
                Font = new Font("Consolas", 9)
            };
            
            // Create boundary condition selector
            var boundaryLabel = new Label
            {
                Text = "Boundary Condition:",
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                Height = 20
            };
            
            _boundaryConditionComboBox = new ComboBox
            {
                Dock = DockStyle.Top,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _boundaryConditionComboBox.Items.AddRange(new object[]
            {
                "Periodic (Wrap)",
                "Reflective (Bounce)",
                "Open (Remove)"
            });
            _boundaryConditionComboBox.SelectedIndex = 0;
            
            // Create particle count slider
            var particleCountLabel = new Label
            {
                Text = "New Particle Count:",
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                Height = 20,
                Margin = new Padding(0, 10, 0, 0)
            };
            
            _particleCountLabel = new Label
            {
                Text = "1,000",
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                Height = 20
            };
            
            _particleCountTrackBar = new TrackBar
            {
                Dock = DockStyle.Top,
                Minimum = 100,
                Maximum = 10000,
                Value = 1000,
                TickFrequency = 1000,
                LargeChange = 1000,
                SmallChange = 100
            };
            
            // Create add particles button
            _addParticlesButton = new Button
            {
                Text = "Add Particles",
                Dock = DockStyle.Top,
                Height = 30,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(60, 60, 60),
                Margin = new Padding(0, 10, 0, 0)
            };
            
            // Create reset view button
            _resetViewButton = new Button
            {
                Text = "Reset View",
                Dock = DockStyle.Top,
                Height = 30,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(60, 60, 60),
                Margin = new Padding(0, 10, 0, 0)
            };
            
            // Add controls to the panel
            _controlPanel.Controls.Add(_resetViewButton);
            _controlPanel.Controls.Add(_addParticlesButton);
            _controlPanel.Controls.Add(_particleCountLabel);
            _controlPanel.Controls.Add(_particleCountTrackBar);
            _controlPanel.Controls.Add(particleCountLabel);
            _controlPanel.Controls.Add(_boundaryConditionComboBox);
            _controlPanel.Controls.Add(boundaryLabel);
            _controlPanel.Controls.Add(_statsLabel);
            
            // Add controls to the form
            this.Controls.Add(_canvas);
            this.Controls.Add(_controlPanel);
            
            // Handle form closing
            this.FormClosing += MainForm_FormClosing;
        }
        
        private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            // Stop the simulation when the form is closing
            _simulation.Stop();
            _renderTimer.Stop();
            _statsTimer.Stop();
        }
        
        private void UpdateStats(object? sender, EventArgs e)
        {
            // Get the current state of the simulation
            var snapshot = _simulation.LatestSnapshot;
            
            // Update the stats label
            _statsLabel.Text = $"Simulation Statistics:\n" +
                              $"Particles: {snapshot.Particles.Count:N0}\n" +
                              $"Octree Nodes: {snapshot.Nodes.Count:N0}\n" +
                              $"Max Depth: {(snapshot.Nodes.Count > 0 ? snapshot.Nodes.Max(n => n.Depth) : 0)}\n" +
                              $"Memory: {GC.GetTotalMemory(false) / 1024 / 1024:N1} MB";
        }
        
        private void AddParticlesButton_Click(object? sender, EventArgs e)
        {
            // Add particles based on the slider value
            _simulation.AddRandomParticles(_particleCountTrackBar.Value);
        }
        
        private void ResetViewButton_Click(object? sender, EventArgs e)
        {
            // Reset the view
            _renderer.ResetView();
        }
        
        private void BoundaryConditionComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            // Update the boundary condition based on the selected index
            switch (_boundaryConditionComboBox.SelectedIndex)
            {
                case 0:
                    _simulation.BoundaryCondition = BoundaryConditionType.Periodic;
                    break;
                case 1:
                    _simulation.BoundaryCondition = BoundaryConditionType.Reflective;
                    break;
                case 2:
                    _simulation.BoundaryCondition = BoundaryConditionType.Open;
                    break;
            }
        }
        
        private void ParticleCountTrackBar_ValueChanged(object? sender, EventArgs e)
        {
            // Update the particle count label
            _particleCountLabel.Text = $"{_particleCountTrackBar.Value:N0}";
        }
    }
}
