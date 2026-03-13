using CameraSettingsSaver.Resources;

namespace WebCamSettings.Forms
{
    public class GuideTab : TabPage
    {
        private MainForm _mainForm;
        private Localization _localization;
        private CameraSettingsSaver.Resources.Help _help;

        public GroupBox GuideGroupBox { get; private set; }
        public TextBox GuideTextBox { get; private set; }

        public GuideTab(MainForm mainForm, Localization localization, CameraSettingsSaver.Resources.Help help)
        {
            _mainForm = mainForm;
            _localization = localization;
            _help = help;

            InitializeComponent();
            ApplyColors();
        }

        private void InitializeComponent()
        {
            this.BackColor = _mainForm.SecondaryBackgroundColor;

            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 1,
                ColumnCount = 1,
                Padding = new Padding(10)
            };
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Group Box
            GuideGroupBox = new GroupBox
            {
                Text = _localization.GetString("GroupBoxes.UserGuide"),
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat
            };
            GuideGroupBox.Paint += OnGroupBoxPaint;

            // Text Box
            GuideTextBox = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BorderStyle = BorderStyle.None,
                Dock = DockStyle.Fill,
                Margin = new Padding(10),
                Font = new Font("Segoe UI", 11)
            };

            GuideGroupBox.Controls.Add(GuideTextBox);
            table.Controls.Add(GuideGroupBox, 0, 0);

            this.Controls.Add(table);
        }

        private void ApplyColors()
        {
            GuideGroupBox.BackColor = _mainForm.SecondaryBackgroundColor;
            GuideGroupBox.ForeColor = _mainForm.AccentColor;
            GuideTextBox.BackColor = _mainForm.BackgroundColor;
            GuideTextBox.ForeColor = _mainForm.TextColor;
        }

        private void OnGroupBoxPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(
                new Pen(_mainForm.AccentColor, 3),
                new Rectangle(0, 0, GuideGroupBox.Width - 1, GuideGroupBox.Height - 1)
            );
        }

        public void UpdateLanguage()
        {
            this.Text = _localization.GetString("Tabs.UserGuide");
            GuideGroupBox.Text = _localization.GetString("GroupBoxes.UserGuide");
            GuideTextBox.Text = _help.GetUserGuide(_localization);
        }
    }
}