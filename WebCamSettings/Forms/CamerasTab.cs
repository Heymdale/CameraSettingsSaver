using CameraSettingsSaver.Resources;
using CameraSettingsSaver.Models;
using static WebCamSettings.MainForm;

namespace WebCamSettings.Forms
{
    public class CamerasTab : TabPage
    {
        private MainForm _mainForm;
        private Localization _localization;
        private BasicLogger _logger;

        public Label InstructionLabel { get; private set; }
        public ListView CamerasListView { get; private set; }
        public Button PreviewButton { get; private set; }
        public Button SettingsButton { get; private set; }

        public event EventHandler<ListViewItem> CameraSelected;
        public event EventHandler<CameraInfo> PreviewRequested;
        public event EventHandler<CameraInfo> SettingsRequested;

        public CamerasTab(MainForm mainForm, Localization localization, BasicLogger logger)
        {
            _mainForm = mainForm;
            _localization = localization;
            _logger = logger;

            InitializeComponent();
            ApplyColors();
        }

        private void InitializeComponent()
        {
            this.BackColor = _mainForm.SecondaryBackgroundColor;

            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
                Padding = new Padding(10)
            };
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // Instruction Label
            InstructionLabel = new Label
            {
                Text = _localization.GetString("Labels.SelectCameras"),
                Font = new Font(_mainForm.DefaultFont.FontFamily, 12),
                ForeColor = _mainForm.AccentColor,
                Margin = new Padding(10),
                AutoSize = true
            };

            // ListView
            CamerasListView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                CheckBoxes = true,
                BorderStyle = BorderStyle.FixedSingle
            };

            CamerasListView.Columns.Add("Camera Name", 400);
            CamerasListView.Columns.Add("Device Path", 450);
            CamerasListView.SelectedIndexChanged += OnCameraSelected;

            // Action Buttons Panel
            var buttonPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                Dock = DockStyle.Fill,
                Padding = new Padding(10, 10, 10, 0)
            };

            PreviewButton = CreateStyledButton(_localization.GetString("Buttons.Preview"));
            PreviewButton.Click += OnPreviewButtonClick;
            PreviewButton.Enabled = false;

            SettingsButton = CreateStyledButton(_localization.GetString("Buttons.Settings"));
            SettingsButton.Click += OnSettingsButtonClick;
            SettingsButton.Enabled = false;

            buttonPanel.Controls.Add(PreviewButton);
            buttonPanel.Controls.Add(SettingsButton);

            table.Controls.Add(InstructionLabel, 0, 0);
            table.Controls.Add(CamerasListView, 0, 1);
            table.Controls.Add(buttonPanel, 0, 2);

            this.Controls.Add(table);
        }

        private Button CreateStyledButton(string text)
        {
            return new Button
            {
                Text = text,
                BackColor = _mainForm.AccentColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = {
                    BorderColor = Color.FromArgb(85, 85, 85),
                    BorderSize = 1
                },
                Margin = new Padding(2, 5, 2, 5),
                Padding = new Padding(12, 4, 12, 4),
                Cursor = Cursors.Hand,
                Font = _mainForm.DefaultFont,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                TextAlign = ContentAlignment.MiddleCenter,
                MinimumSize = new Size(0, 30)
            };
        }

        private void ApplyColors()
        {
            CamerasListView.BackColor = _mainForm.SecondaryBackgroundColor;
            CamerasListView.ForeColor = _mainForm.TextColor;
        }

        private void OnCameraSelected(object sender, EventArgs e)
        {
            bool hasSelection = CamerasListView.SelectedItems.Count > 0;
            PreviewButton.Enabled = hasSelection;
            SettingsButton.Enabled = hasSelection;

            if (hasSelection && CameraSelected != null)
            {
                CameraSelected(this, CamerasListView.SelectedItems[0]);
            }
        }

        private void OnPreviewButtonClick(object sender, EventArgs e)
        {
            var selectedItem = CamerasListView.SelectedItems[0];
            var cameraInfo = selectedItem.Tag as CameraInfo;

            if (cameraInfo != null && PreviewRequested != null)
            {
                PreviewRequested(this, cameraInfo);
            }
        }

        private void OnSettingsButtonClick(object sender, EventArgs e)
        {
            var selectedItem = CamerasListView.SelectedItems[0];
            var cameraInfo = selectedItem.Tag as CameraInfo;

            if (cameraInfo != null && SettingsRequested != null)
            {
                SettingsRequested(this, cameraInfo);
            }
        }

        public void UpdateLanguage()
        {
            this.Text = _localization.GetString("Tabs.Cameras");
            InstructionLabel.Text = _localization.GetString("Labels.SelectCameras");
            PreviewButton.Text = _localization.GetString("Buttons.Preview");
            SettingsButton.Text = _localization.GetString("Buttons.Settings");
        }
    }
}