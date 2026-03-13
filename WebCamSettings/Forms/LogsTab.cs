using CameraSettingsSaver.Resources;

namespace WebCamSettings.Forms
{
    public class LogsTab : TabPage
    {
        private MainForm _mainForm;
        private Localization _localization;

        public GroupBox LogsGroupBox { get; private set; }
        public TextBox LogsTextBox { get; private set; }
        public Button ClearLogsButton { get; private set; }
        public Button CopyLogsButton { get; private set; }
        public Button SaveLogsButton { get; private set; }

        public event EventHandler ClearLogsRequested;
        public event EventHandler CopyLogsRequested;
        public event EventHandler<string> SaveLogsRequested;

        public LogsTab(MainForm mainForm, Localization localization)
        {
            _mainForm = mainForm;
            _localization = localization;

            InitializeComponent();
            ApplyColors();
        }

        private void InitializeComponent()
        {
            this.BackColor = _mainForm.SecondaryBackgroundColor;

            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                Padding = new Padding(10)
            };
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // Group Box
            LogsGroupBox = new GroupBox
            {
                Text = _localization.GetString("GroupBoxes.ApplicationLogs"),
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat
            };
            LogsGroupBox.Paint += OnGroupBoxPaint;

            // Text Box
            LogsTextBox = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                BorderStyle = BorderStyle.None,
                Font = _mainForm.MonospaceFont,
                WordWrap = true,
                Dock = DockStyle.Fill,
                Margin = new Padding(10)
            };

            LogsGroupBox.Controls.Add(LogsTextBox);
            table.Controls.Add(LogsGroupBox, 0, 0);

            // Buttons Panel
            var buttonPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true,
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 10, 0, 0)
            };

            SaveLogsButton = CreateStyledButton(_localization.GetString("Buttons.SaveLogs"));
            SaveLogsButton.Click += OnSaveLogsClick;

            CopyLogsButton = CreateStyledButton(_localization.GetString("Buttons.CopyLogs"));
            CopyLogsButton.Click += OnCopyLogsClick;

            ClearLogsButton = CreateStyledButton(_localization.GetString("Buttons.ClearLogs"));
            ClearLogsButton.Click += OnClearLogsClick;

            buttonPanel.Controls.Add(SaveLogsButton);
            buttonPanel.Controls.Add(CopyLogsButton);
            buttonPanel.Controls.Add(ClearLogsButton);
            table.Controls.Add(buttonPanel, 0, 1);

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
            LogsGroupBox.BackColor = _mainForm.SecondaryBackgroundColor;
            LogsGroupBox.ForeColor = _mainForm.AccentColor;
            LogsTextBox.BackColor = _mainForm.BackgroundColor;
            LogsTextBox.ForeColor = _mainForm.LogTextColor;
        }

        private void OnGroupBoxPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(
                new Pen(_mainForm.AccentColor, 3),
                new Rectangle(0, 0, LogsGroupBox.Width - 1, LogsGroupBox.Height - 1)
            );
        }

        private void OnClearLogsClick(object sender, EventArgs e)
        {
            ClearLogsRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OnCopyLogsClick(object sender, EventArgs e)
        {
            CopyLogsRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OnSaveLogsClick(object sender, EventArgs e)
        {
            SaveLogsRequested?.Invoke(this, LogsTextBox.Text);
        }

        public void UpdateLanguage()
        {
            this.Text = _localization.GetString("Tabs.Logs");
            LogsGroupBox.Text = _localization.GetString("GroupBoxes.ApplicationLogs");
            ClearLogsButton.Text = _localization.GetString("Buttons.ClearLogs");
            CopyLogsButton.Text = _localization.GetString("Buttons.CopyLogs");
            SaveLogsButton.Text = _localization.GetString("Buttons.SaveLogs");
        }
    }
}