using System;
using System.Drawing;
using System.Windows.Forms;
using DirectShowLib;

namespace WebCamSettings.Forms
{
    public partial class CameraPreviewForm : Form
    {
        private CameraPreview _cameraPreview;
        private Panel _previewPanel;
        private Label _statusLabel;
        private bool _isPreviewRunning = false;
        private DsDevice _currentCamera;
        private Size _originalSize;
        private bool _handleCreated = false;

        public CameraPreviewForm()
        {
            InitializeComponent();
            InitializeUI();
            _originalSize = Size;

            // Subscribe to the handle creation event
            HandleCreated += (s, e) => _handleCreated = true;

            // Subscribe to the form closing event
            FormClosing += (s, e) =>
            {
                StopPreview();
            };

            FormClosed += (s, e) =>
            {
                DisposePreview();
            };
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            // Form settings
            Text = "Camera Preview";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(640, 480);
            MinimumSize = new Size(200, 150);
            BackColor = Color.FromArgb(30, 30, 30);
            ForeColor = Color.FromArgb(204, 204, 204);
            FormBorderStyle = FormBorderStyle.Sizable;

            // Preview panel
            _previewPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black,
                Margin = new Padding(10),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Status label
            _statusLabel = new Label
            {
                Text = "Starting preview...",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.LightGray,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };

            _previewPanel.Controls.Add(_statusLabel);
            Controls.Add(_previewPanel);

            ResumeLayout(false);
        }

        private void InitializeUI()
        {
            _cameraPreview = new CameraPreview();
        }

        public void StartPreview(DsDevice camera)
        {
            if (camera == null)
                throw new ArgumentNullException(nameof(camera));

            _currentCamera = camera;
            Text = $"Preview: {camera.Name}";

            try
            {
                // Hide the status label
                _statusLabel.Visible = false;

                // Launch the preview
                _cameraPreview.StartPreview(camera, _previewPanel);
                _isPreviewRunning = true;

                // Show the window
                Show();
                BringToFront();
                Activate();
            }
            catch (Exception ex)
            {
                _statusLabel.Text = $"Error: {ex.Message}";
                _statusLabel.Visible = true;
                _isPreviewRunning = false;

                MessageBox.Show($"Error starting preview:\n{ex.Message}",
                    "Camera Preview Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        public void StopPreview()
        {
            if (_isPreviewRunning && _cameraPreview != null)
            {
                _cameraPreview.StopPreview();
                _isPreviewRunning = false;
                _statusLabel.Text = "Preview stopped";
                _statusLabel.Visible = true;

                // Return the original window size
                Size = _originalSize;
            }
        }

        // Resize handler
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            UpdatePreviewSizeSafe();
        }

        // Client area resize handler
        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);

            if (_isPreviewRunning && _cameraPreview != null)
            {
                _cameraPreview.UpdatePreviewSize();
            }
        }

        // Safely update preview size
        private void UpdatePreviewSizeSafe()
        {
            if (_isPreviewRunning && _cameraPreview != null)
            {
                // Check if the handle has been created
                if (_handleCreated && IsHandleCreated)
                {
                    // Use BeginInvoke to update asynchronously
                    BeginInvoke(new Action(() =>
                    {
                        if (_isPreviewRunning && _cameraPreview != null)
                        {
                            _cameraPreview.UpdatePreviewSize();
                        }
                    }));
                }
                else
                {
                    // If the handle has not yet been created, update it directly
                    try
                    {
                        _cameraPreview.UpdatePreviewSize();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error updating preview size: {ex.Message}");
                    }
                }
            }
        }

        // Visibility change handler
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (Visible && _isPreviewRunning && _cameraPreview != null)
            {
                // When the window is shown, we update the video size
                if (_handleCreated && IsHandleCreated)
                {
                    BeginInvoke(new Action(() =>
                    {
                        if (_isPreviewRunning && _cameraPreview != null)
                        {
                            _cameraPreview.UpdatePreviewSize();
                        }
                    }));
                }
                else
                {
                    try
                    {
                        _cameraPreview.UpdatePreviewSize();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error updating preview on visible: {ex.Message}");
                    }
                }
            }
        }

        // Form closing handler
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            StopPreview();
            base.OnFormClosing(e);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            DisposePreview();
        }

        private void DisposePreview()
        {
            if (_cameraPreview != null)
            {
                _cameraPreview.Dispose();
                _cameraPreview = null;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                StopPreview();
                DisposePreview();
            }
            base.Dispose(disposing);
        }
    }
}