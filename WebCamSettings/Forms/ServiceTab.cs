using CameraSettingsSaver.Resources;
using WebCamSettings.Core;
using WebCamSettings.Models;
using WebCamSettings.Resources;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace WebCamSettings.Forms
{
    public class ServiceTab : TabPage
    {
        private readonly MainForm _mainForm;
        private readonly Localization _localization;
        private readonly FormsLogger _logger;
        private readonly ServiceManager _serviceManager;

        // UI Controls
        private GroupBox _serviceStatusGroupBox;
        private GroupBox _serviceParamsGroupBox;
        private GroupBox _serviceControlGroupBox;

        private Button _installButton;
        private Button _uninstallButton;
        private Button _startButton;
        private Button _stopButton;
        private Button _refreshStatusButton;

        private TextBox _profilePathTextBox;
        private Button _browseProfileButton;
        private ComboBox _languageComboBox;
        private NumericUpDown _intervalNumericUpDown;
        private TextBox _logPathTextBox;
        private Button _browseLogButton;
        private CheckBox _useLogCheckBox;

        private Label _statusLabel;
        private Label _serviceNameLabel;
        private Label _servicePathLabel;
        private Panel _statusPanel;

        private Label _adminWarningLabel;
        private Button _restartAsAdminButton;

        private Panel _mainPanel;
        private TableLayoutPanel _mainTable;

        public ServiceTab(MainForm mainForm, Localization localization, FormsLogger logger)
        {
            _mainForm = mainForm;
            _localization = localization;
            _logger = logger;
            _serviceManager = new ServiceManager(logger, localization);
            _serviceManager.StatusChanged += OnServiceStatusChanged;

            InitializeComponent();
            UpdateUIForAdminRights();
            _serviceManager.StartStatusMonitoring();

            this.Resize += ServiceTab_Resize;
        }

        private void InitializeComponent()
        {
            Text = _localization.GetString("Tabs.Service");
            BackColor = _mainForm.SecondaryBackgroundColor;
            Padding = new Padding(0);

            _mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = _mainForm.SecondaryBackgroundColor
            };

            _mainTable = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Width = this.ClientSize.Width - 25,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(10),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = _mainForm.SecondaryBackgroundColor
            };

            _mainTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _mainTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _mainTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _mainTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var adminPanel = CreateAdminWarningPanel();
            _serviceStatusGroupBox = CreateStatusGroupBox();
            _serviceParamsGroupBox = CreateParametersGroupBox();
            _serviceControlGroupBox = CreateControlGroupBox();

            adminPanel.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            _serviceStatusGroupBox.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            _serviceParamsGroupBox.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            _serviceControlGroupBox.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;

            _mainTable.Controls.Add(adminPanel, 0, 0);
            _mainTable.Controls.Add(_serviceStatusGroupBox, 0, 1);
            _mainTable.Controls.Add(_serviceParamsGroupBox, 0, 2);
            _mainTable.Controls.Add(_serviceControlGroupBox, 0, 3);

            _mainPanel.Controls.Add(_mainTable);
            Controls.Add(_mainPanel);
        }

        private void ServiceTab_Resize(object sender, EventArgs e)
        {
            if (_mainTable != null)
            {
                _mainTable.Width = this.ClientSize.Width - 25;
            }
        }

        private Panel CreateAdminWarningPanel()
        {
            var panel = new Panel
            {
                Height = 50,
                Margin = new Padding(0, 0, 0, 10),
                BackColor = _mainForm.SecondaryBackgroundColor
            };

            _adminWarningLabel = new Label
            {
                Text = _localization.GetString("Messages.AdminRequired"),
                ForeColor = _mainForm.WarningColor,
                Font = new Font(_mainForm.DefaultFont, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(5, 15),
                MaximumSize = new Size(700, 40),
                BackColor = _mainForm.SecondaryBackgroundColor
            };

            _restartAsAdminButton = new Button
            {
                Text = _localization.GetString("Buttons.RestartAsAdmin"),
                BackColor = _mainForm.WarningColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                MaximumSize = new Size(280, 35),
                MinimumSize = new Size(200, 30),
                Location = new Point(panel.Width - 230, 10),
                Cursor = Cursors.Hand,
                FlatAppearance = {
                    BorderColor = Color.FromArgb(85, 85, 85),
                    BorderSize = 1
                }
            };

            _restartAsAdminButton.Click += RestartAsAdminButton_Click;

            panel.Resize += (s, e) =>
            {
                _restartAsAdminButton.Location = new Point(panel.Width - _restartAsAdminButton.Width - 10, 10);
            };

            panel.Controls.Add(_restartAsAdminButton);
            panel.Controls.Add(_adminWarningLabel);

            return panel;
        }

        private GroupBox CreateStatusGroupBox()
        {
            var groupBox = new GroupBox
            {
                Text = _localization.GetString("GroupBoxes.ServiceStatus"),
                ForeColor = _mainForm.AccentColor,
                BackColor = _mainForm.SecondaryBackgroundColor,
                Height = 130,
                FlatStyle = FlatStyle.Flat,
                Padding = new Padding(10),
                Margin = new Padding(0, 0, 0, 10)
            };

            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(5),
                BackColor = _mainForm.SecondaryBackgroundColor
            };

            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            table.Controls.Add(new Label
            {
                Text = _localization.GetString("Labels.ServiceName"),
                ForeColor = _mainForm.TextColor,
                Font = new Font(_mainForm.DefaultFont, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight,
                Dock = DockStyle.Fill,
                BackColor = _mainForm.SecondaryBackgroundColor
            }, 0, 0);

            _serviceNameLabel = new Label
            {
                Text = _serviceManager.ServiceName,
                ForeColor = _mainForm.TextColor,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                AutoEllipsis = true,
                BackColor = _mainForm.SecondaryBackgroundColor
            };
            table.Controls.Add(_serviceNameLabel, 1, 0);

            table.Controls.Add(new Label
            {
                Text = _localization.GetString("Labels.ServicePath"),
                ForeColor = _mainForm.TextColor,
                Font = new Font(_mainForm.DefaultFont, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight,
                Dock = DockStyle.Fill,
                BackColor = _mainForm.SecondaryBackgroundColor
            }, 0, 1);

            _servicePathLabel = new Label
            {
                Text = _serviceManager.ServicePath,
                ForeColor = _mainForm.TextColor,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                AutoEllipsis = true,
                BackColor = _mainForm.SecondaryBackgroundColor
            };
            table.Controls.Add(_servicePathLabel, 1, 1);

            var statusFlowPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                Dock = DockStyle.Fill,
                WrapContents = false,
                BackColor = _mainForm.SecondaryBackgroundColor
            };

            _statusPanel = new Panel
            {
                Size = new Size(20, 20),
                Margin = new Padding(0, 5, 10, 0),
                BackColor = GetStatusColor(_serviceManager.CurrentStatus)
            };

            _statusLabel = new Label
            {
                Text = GetStatusText(_serviceManager.CurrentStatus),
                ForeColor = _mainForm.TextColor,
                Font = new Font(_mainForm.DefaultFont, FontStyle.Bold),
                AutoSize = true,
                BackColor = _mainForm.SecondaryBackgroundColor
            };

            statusFlowPanel.Controls.Add(_statusPanel);
            statusFlowPanel.Controls.Add(_statusLabel);

            table.Controls.Add(new Label
            {
                Text = _localization.GetString("Labels.ServiceStatus"),
                ForeColor = _mainForm.TextColor,
                Font = new Font(_mainForm.DefaultFont, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight,
                Dock = DockStyle.Fill,
                BackColor = _mainForm.SecondaryBackgroundColor
            }, 0, 2);

            table.Controls.Add(statusFlowPanel, 1, 2);

            groupBox.Controls.Add(table);
            return groupBox;
        }

        private GroupBox CreateParametersGroupBox()
        {
            var groupBox = new GroupBox
            {
                Text = _localization.GetString("GroupBoxes.ServiceParameters"),
                ForeColor = _mainForm.AccentColor,
                BackColor = _mainForm.SecondaryBackgroundColor,
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlatStyle = FlatStyle.Flat,
                Padding = new Padding(10),
                Margin = new Padding(0, 0, 0, 10)
            };

            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 3,
                Padding = new Padding(5),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = _mainForm.SecondaryBackgroundColor
            };

            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));

            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // ===== ROW 0: Profile Path =====
            table.Controls.Add(new Label
            {
                Text = _localization.GetString("Labels.ProfilePath"),
                ForeColor = _mainForm.TextColor,
                TextAlign = ContentAlignment.MiddleRight,
                Dock = DockStyle.Fill,
                BackColor = _mainForm.SecondaryBackgroundColor,
                AutoSize = true,
                Padding = new Padding(0, 2, 0, 2)
            }, 0, 0);

            var panelProfile = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = _mainForm.SecondaryBackgroundColor,
                Height = 40
            };

            _profilePathTextBox = new TextBox
            {
                Text = "web_conf.json",
                Location = new Point(0, 7),
                Width = panelProfile.Width - 5,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
                BackColor = _mainForm.SecondaryBackgroundColor,
                ForeColor = _mainForm.TextColor,
                BorderStyle = BorderStyle.FixedSingle,
                Height = 28
            };

            panelProfile.Resize += (s, e) =>
            {
                _profilePathTextBox.Width = panelProfile.Width - 5;
                _profilePathTextBox.Location = new Point(0, (panelProfile.Height - _profilePathTextBox.Height) / 2);
            };

            panelProfile.Controls.Add(_profilePathTextBox);
            table.Controls.Add(panelProfile, 1, 0);
            table.Controls.Add(new Panel(), 2, 0);

            var panelBrowseProfile = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = _mainForm.SecondaryBackgroundColor,
                Height = 40
            };
            
            _browseProfileButton = new Button
            {
                Text = _localization.GetString("Buttons.Browse"),
                BackColor = Color.FromArgb(62, 62, 62),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(90, 35),
                Location = new Point(5, 2),
                Anchor = AnchorStyles.None,
                Cursor = Cursors.Hand,
                FlatAppearance = {
                    BorderColor = Color.FromArgb(85, 85, 85),
                    BorderSize = 1
                }
            };

            panelBrowseProfile.Resize += (s, e) =>
            {
                _browseProfileButton.Location = new Point(
                    (panelBrowseProfile.Width - _browseProfileButton.Width) / 2,
                    (panelBrowseProfile.Height - _browseProfileButton.Height) / 2
                );
            };

            _browseProfileButton.Click += BrowseProfileButton_Click;
            panelBrowseProfile.Controls.Add(_browseProfileButton);
            table.Controls.Add(panelBrowseProfile, 3, 0);

            // ===== ROW 1: Language + Interval =====
            table.Controls.Add(new Label
            {
                Text = _localization.GetString("Labels.Language"),
                ForeColor = _mainForm.TextColor,
                TextAlign = ContentAlignment.MiddleRight,
                Dock = DockStyle.Fill,
                BackColor = _mainForm.SecondaryBackgroundColor,
                AutoSize = true,
                Padding = new Padding(0, 2, 0, 2)
            }, 0, 1);

            var panelLanguage = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = _mainForm.SecondaryBackgroundColor,
                Height = 40
            };

            _languageComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(0, 7),
                Width = panelLanguage.Width - 5,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
                BackColor = _mainForm.SecondaryBackgroundColor,
                ForeColor = _mainForm.TextColor,
                Height = 28
            };
            _languageComboBox.Items.AddRange(new object[] {
                _localization.GetString("Language.English"),
                _localization.GetString("Language.Russian")
            });
            _languageComboBox.SelectedIndex = _localization.CurrentLanguage == Language.en ? 0 : 1;

            panelLanguage.Resize += (s, e) =>
            {
                _languageComboBox.Width = panelLanguage.Width - 5;
                _languageComboBox.Location = new Point(0, (panelLanguage.Height - _languageComboBox.Height) / 2);
            };

            panelLanguage.Controls.Add(_languageComboBox);
            table.Controls.Add(panelLanguage, 1, 1);

            var lblInterval = new Label
            {
                Text = _localization.GetString("Labels.IntervalSeconds"),
                ForeColor = _mainForm.TextColor,
                TextAlign = ContentAlignment.MiddleRight,
                Dock = DockStyle.Fill,
                BackColor = _mainForm.SecondaryBackgroundColor,
                AutoSize = true,
                Padding = new Padding(0, 2, 0, 2)
            };
            table.Controls.Add(lblInterval, 2, 1);

            var panelInterval = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = _mainForm.SecondaryBackgroundColor,
                Height = 40
            };

            _intervalNumericUpDown = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 3600,
                Value = 20,
                Location = new Point(0, 7),
                Width = panelInterval.Width - 5,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
                BackColor = _mainForm.SecondaryBackgroundColor,
                ForeColor = _mainForm.TextColor,
                TextAlign = HorizontalAlignment.Center,
                Height = 28
            };

            panelInterval.Resize += (s, e) =>
            {
                _intervalNumericUpDown.Width = panelInterval.Width - 5;
                _intervalNumericUpDown.Location = new Point(0, (panelInterval.Height - _intervalNumericUpDown.Height) / 2);
            };

            panelInterval.Controls.Add(_intervalNumericUpDown);
            table.Controls.Add(panelInterval, 3, 1);

            // ===== ROW 2: Log File =====
            var panelCheckBox = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = _mainForm.SecondaryBackgroundColor
            };

            _useLogCheckBox = new CheckBox
            {
                Text = _localization.GetString("Labels.UseLogFile"),
                ForeColor = _mainForm.TextColor,
                Checked = false,
                AutoSize = true,
                BackColor = _mainForm.SecondaryBackgroundColor,
                UseVisualStyleBackColor = true,
                TextAlign = ContentAlignment.MiddleLeft,
                MaximumSize = new Size(110, 0),
                Padding = new Padding(0, 2, 0, 2),
            };

            panelCheckBox.Resize += (s, e) =>
            {
                _useLogCheckBox.Location = new Point(
                    0,
                    (panelCheckBox.Height - _useLogCheckBox.Height) / 2
                );
            };

            _useLogCheckBox.CheckedChanged += UseLogCheckBox_CheckedChanged;
            panelCheckBox.Controls.Add(_useLogCheckBox);
            table.Controls.Add(panelCheckBox, 0, 2);

            var panelLog = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = _mainForm.SecondaryBackgroundColor,
                Height = 40
            };

            _logPathTextBox = new TextBox
            {
                Text = "",
                Location = new Point(0, 7),
                Width = panelLog.Width - 5,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
                BackColor = _mainForm.SecondaryBackgroundColor,
                ForeColor = _mainForm.TextColor,
                BorderStyle = BorderStyle.FixedSingle,
                Enabled = false,
                Height = 28
            };

            panelLog.Resize += (s, e) =>
            {
                _logPathTextBox.Width = panelLog.Width - 5;
                _logPathTextBox.Location = new Point(0, (panelLog.Height - _logPathTextBox.Height) / 2);
            };

            panelLog.Controls.Add(_logPathTextBox);
            table.Controls.Add(panelLog, 1, 2);
            table.Controls.Add(new Panel(), 2, 2);

            var panelBrowseLog = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = _mainForm.SecondaryBackgroundColor,
                Height = 40
            };

            _browseLogButton = new Button
            {
                Text = _localization.GetString("Buttons.Browse"),
                BackColor = Color.FromArgb(62, 62, 62),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(90, 35),
                Location = new Point(5, 2),
                Anchor = AnchorStyles.None,
                Cursor = Cursors.Hand,
                Enabled = false,
                FlatAppearance = {
                    BorderColor = Color.FromArgb(85, 85, 85),
                    BorderSize = 1
                }
            };

            panelBrowseLog.Resize += (s, e) =>
            {
                _browseLogButton.Location = new Point(
                    (panelBrowseLog.Width - _browseLogButton.Width) / 2,
                    (panelBrowseLog.Height - _browseLogButton.Height) / 2
                );
            };

            _browseLogButton.Click += BrowseLogButton_Click;
            panelBrowseLog.Controls.Add(_browseLogButton);
            table.Controls.Add(panelBrowseLog, 3, 2);

            groupBox.Controls.Add(table);
            return groupBox;
        }

        private GroupBox CreateControlGroupBox()
        {
            var groupBox = new GroupBox
            {
                Text = _localization.GetString("GroupBoxes.ServiceControl"),
                ForeColor = _mainForm.AccentColor,
                BackColor = _mainForm.SecondaryBackgroundColor,
                Height = 140,
                FlatStyle = FlatStyle.Flat,
                Padding = new Padding(10),
                Margin = new Padding(0, 0, 0, 10)
            };

            var flowPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(5),
                BackColor = _mainForm.SecondaryBackgroundColor
            };

            _installButton = CreateServiceButton(_localization.GetString("Buttons.InstallService"), _mainForm.SuccessColor);
            _installButton.Click += InstallButton_Click;

            _uninstallButton = CreateServiceButton(_localization.GetString("Buttons.UninstallService"), _mainForm.WarningColor);
            _uninstallButton.Click += UninstallButton_Click;
            _uninstallButton.Enabled = false;

            _startButton = CreateServiceButton(_localization.GetString("Buttons.StartService"), _mainForm.AccentColor);
            _startButton.Click += StartButton_Click;
            _startButton.Enabled = false;

            _stopButton = CreateServiceButton(_localization.GetString("Buttons.StopService"), _mainForm.WarningColor);
            _stopButton.Click += StopButton_Click;
            _stopButton.Enabled = false;

            _refreshStatusButton = CreateServiceButton(_localization.GetString("Buttons.RefreshStatus"), _mainForm.TextColor);
            _refreshStatusButton.Click += RefreshStatusButton_Click;

            foreach (Button btn in new[] { _installButton, _uninstallButton, _startButton, _stopButton, _refreshStatusButton })
            {
                btn.MinimumSize = new Size(120, 40);
                btn.MaximumSize = new Size(200, 40);
                flowPanel.Controls.Add(btn);
            }

            groupBox.Controls.Add(flowPanel);
            return groupBox;
        }

        private Button CreateServiceButton(string text, Color backColor)
        {
            return new Button
            {
                Text = text,
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Height = 40,
                Width = 150,
                Margin = new Padding(5),
                Cursor = Cursors.Hand,
                Font = _mainForm.DefaultFont,
                FlatAppearance = {
                    BorderColor = Color.FromArgb(85, 85, 85),
                    BorderSize = 1
                },
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
        }

        private Color GetStatusColor(ServiceStatus status)
        {
            return status switch
            {
                ServiceStatus.Running => _mainForm.SuccessColor,
                ServiceStatus.Stopped => _mainForm.WarningColor,
                ServiceStatus.Starting => Color.Orange,
                ServiceStatus.Stopping => Color.Orange,
                ServiceStatus.NotFound => Color.Gray,
                _ => Color.Gray
            };
        }

        private string GetStatusText(ServiceStatus status)
        {
            string key = status switch
            {
                ServiceStatus.Running => "ServiceStatus.Running",
                ServiceStatus.Stopped => "ServiceStatus.Stopped",
                ServiceStatus.Starting => "ServiceStatus.Starting",
                ServiceStatus.Stopping => "ServiceStatus.Stopping",
                ServiceStatus.NotFound => "ServiceStatus.NotFound",
                _ => "ServiceStatus.Unknown"
            };
            return _localization.GetString(key);
        }

        private void UpdateUIForServiceStatus(ServiceStatus status)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<ServiceStatus>(UpdateUIForServiceStatus), status);
                return;
            }

            bool isInstalled = status != ServiceStatus.NotFound;
            bool isRunning = status == ServiceStatus.Running;
            bool isStopped = status == ServiceStatus.Stopped;
            bool isTransitioning = status == ServiceStatus.Starting || status == ServiceStatus.Stopping;

            _installButton.Enabled = !isInstalled;
            _uninstallButton.Enabled = isInstalled && !isTransitioning;
            _startButton.Enabled = isInstalled && isStopped;
            _stopButton.Enabled = isInstalled && isRunning;

            _statusPanel.BackColor = GetStatusColor(status);
            _statusLabel.Text = GetStatusText(status);
        }

        private void UpdateUIForAdminRights()
        {
            bool isAdmin = _serviceManager.IsRunningAsAdministrator();
            _adminWarningLabel.Parent.Visible = !isAdmin;

            _installButton.Enabled = isAdmin && _installButton.Enabled;
            _uninstallButton.Enabled = isAdmin && _uninstallButton.Enabled;
            _startButton.Enabled = isAdmin && _startButton.Enabled;
            _stopButton.Enabled = isAdmin && _stopButton.Enabled;
            _profilePathTextBox.Enabled = isAdmin;
            _browseProfileButton.Enabled = isAdmin;
            _languageComboBox.Enabled = isAdmin;
            _intervalNumericUpDown.Enabled = isAdmin;
            _useLogCheckBox.Enabled = isAdmin;
            _logPathTextBox.Enabled = isAdmin && _useLogCheckBox.Checked;
            _browseLogButton.Enabled = isAdmin && _useLogCheckBox.Checked;
        }

        #region Event Handlers

        private void OnServiceStatusChanged(object sender, ServiceStatus status)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => UpdateUIForServiceStatus(status)));
                }
                else
                {
                    UpdateUIForServiceStatus(status);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{_localization.GetString("LogMessages.Error")}: {ex.Message}");
            }
        }

        private void RestartAsAdminButton_Click(object sender, EventArgs e)
        {
            try
            {
                _serviceManager.RestartAsAdministrator();
            }
            catch (Exception ex)
            {
                _logger.LogError($"{_localization.GetString("LogMessages.Error")}: {ex.Message}");
                _logger.ShowMessageBox(
                    $"{_localization.GetString("LogMessages.Error")}: {ex.Message}",
                    _localization.GetString("Common.Error"),
                    MessageBoxIcon.Error
                );
            }
        }

        private void UseLogCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                _logPathTextBox.Enabled = _useLogCheckBox.Checked;
                _browseLogButton.Enabled = _useLogCheckBox.Checked;

                if (_useLogCheckBox.Checked)
                {
                    _logger.Log($"{_localization.GetString("Labels.UseLogFile")}: enabled");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{_localization.GetString("LogMessages.Error")}: {ex.Message}");
            }
        }

        private void BrowseProfileButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var openDialog = new OpenFileDialog())
                {
                    openDialog.Filter = _localization.GetString("FileDialogs.JsonFilter");
                    openDialog.DefaultExt = ".json";
                    openDialog.FileName = _profilePathTextBox.Text;

                    if (openDialog.ShowDialog() == DialogResult.OK)
                    {
                        _profilePathTextBox.Text = openDialog.FileName;
                        _logger.Log($"{_localization.GetString("Labels.ProfilePath")}: {openDialog.FileName}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{_localization.GetString("LogMessages.Error")}: {ex.Message}");
                _logger.ShowMessageBox(
                    $"{_localization.GetString("LogMessages.Error")}: {ex.Message}",
                    _localization.GetString("Common.Error"),
                    MessageBoxIcon.Error
                );
            }
        }

        private void BrowseLogButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = _localization.GetString("FileDialogs.LogFilter");
                    saveDialog.DefaultExt = ".log";
                    saveDialog.FileName = "service.log";

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        _logPathTextBox.Text = saveDialog.FileName;
                        _logger.Log($"{_localization.GetString("Labels.UseLogFile")}: {saveDialog.FileName}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{_localization.GetString("LogMessages.Error")}: {ex.Message}");
                _logger.ShowMessageBox(
                    $"{_localization.GetString("LogMessages.Error")}: {ex.Message}",
                    _localization.GetString("Common.Error"),
                    MessageBoxIcon.Error
                );
            }
        }

        private void InstallButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_serviceManager.IsRunningAsAdministrator())
                {
                    string warning = _localization.GetString("Messages.AdminRequired");
                    _logger.LogWarning(warning);
                    _logger.ShowMessageBox(
                        warning,
                        _localization.GetString("Common.Warning"),
                        MessageBoxIcon.Warning
                    );
                    return;
                }

                if (!File.Exists(_profilePathTextBox.Text))
                {
                    string warning = string.Format(
                        _localization.GetString("Messages.FileNotFound"),
                        _profilePathTextBox.Text
                    );
                    _logger.LogWarning(warning);
                    _logger.ShowMessageBox(
                        warning,
                        _localization.GetString("Common.Warning"),
                        MessageBoxIcon.Warning
                    );
                    return;
                }

                var language = _languageComboBox.SelectedIndex == 0 ? Language.en : Language.ru;

                // Important: we pass the flag and path separately
                bool useLogFile = _useLogCheckBox.Checked;
                string? logPath = useLogFile ? _logPathTextBox.Text : null;

                // If the checkbox is enabled but the path is empty, we pass null, but with the useLogFile = true flag
                // ServiceManager will automatically add /log without a path

                bool success = _serviceManager.InstallService(
                    _profilePathTextBox.Text,
                    language,
                    (int)_intervalNumericUpDown.Value,
                    useLogFile,  // Pass a separate flag
                    logPath      // Path may be null or empty
                );

                if (success)
                {
                    _logger.Log(string.Format(
                        _localization.GetString("LogMessages.ServiceInstalled"),
                        _serviceManager.ServiceName
                    ));
                }
                else
                {
                    _logger.LogError($"{_localization.GetString("LogMessages.Error")}: Failed to install service");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{_localization.GetString("LogMessages.Error")}: {ex.Message}");
                _logger.ShowMessageBox(
                    $"{_localization.GetString("LogMessages.Error")}: {ex.Message}",
                    _localization.GetString("Common.Error"),
                    MessageBoxIcon.Error
                );
            }
        }

        private void UninstallButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_serviceManager.IsRunningAsAdministrator())
                {
                    string warning = _localization.GetString("Messages.AdminRequired");
                    _logger.LogWarning(warning);
                    _logger.ShowMessageBox(
                        warning,
                        _localization.GetString("Common.Warning"),
                        MessageBoxIcon.Warning
                    );
                    return;
                }

                var result = MessageBox.Show(
                    _localization.GetString("Messages.ConfirmUninstall"),
                    _localization.GetString("Common.Confirmation"),
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.Yes)
                {
                    bool success = _serviceManager.UninstallService();
                    if (success)
                    {
                        _logger.Log(string.Format(
                            _localization.GetString("LogMessages.ServiceUninstalled"),
                            _serviceManager.ServiceName
                        ));
                    }
                    else
                    {
                        _logger.LogError($"{_localization.GetString("LogMessages.Error")}: Failed to uninstall service");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{_localization.GetString("LogMessages.Error")}: {ex.Message}");
                _logger.ShowMessageBox(
                    $"{_localization.GetString("LogMessages.Error")}: {ex.Message}",
                    _localization.GetString("Common.Error"),
                    MessageBoxIcon.Error
                );
            }
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_serviceManager.IsRunningAsAdministrator())
                {
                    string warning = _localization.GetString("Messages.AdminRequired");
                    _logger.LogWarning(warning);
                    _logger.ShowMessageBox(
                        warning,
                        _localization.GetString("Common.Warning"),
                        MessageBoxIcon.Warning
                    );
                    return;
                }

                bool success = _serviceManager.StartService();
                if (success)
                {
                    _logger.Log(string.Format(
                        _localization.GetString("LogMessages.ServiceStarted"),
                        _serviceManager.ServiceName
                    ));
                }
                else
                {
                    _logger.LogError($"{_localization.GetString("LogMessages.Error")}: Failed to start service");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{_localization.GetString("LogMessages.Error")}: {ex.Message}");
                _logger.ShowMessageBox(
                    $"{_localization.GetString("LogMessages.Error")}: {ex.Message}",
                    _localization.GetString("Common.Error"),
                    MessageBoxIcon.Error
                );
            }
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_serviceManager.IsRunningAsAdministrator())
                {
                    string warning = _localization.GetString("Messages.AdminRequired");
                    _logger.LogWarning(warning);
                    _logger.ShowMessageBox(
                        warning,
                        _localization.GetString("Common.Warning"),
                        MessageBoxIcon.Warning
                    );
                    return;
                }

                bool success = _serviceManager.StopService();
                if (success)
                {
                    _logger.Log(string.Format(
                        _localization.GetString("LogMessages.ServiceStopped"),
                        _serviceManager.ServiceName
                    ));
                }
                else
                {
                    _logger.LogError($"{_localization.GetString("LogMessages.Error")}: Failed to stop service");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{_localization.GetString("LogMessages.Error")}: {ex.Message}");
                _logger.ShowMessageBox(
                    $"{_localization.GetString("LogMessages.Error")}: {ex.Message}",
                    _localization.GetString("Common.Error"),
                    MessageBoxIcon.Error
                );
            }
        }

        private void RefreshStatusButton_Click(object sender, EventArgs e)
        {
            try
            {
                var status = _serviceManager.GetServiceStatus();
                UpdateUIForServiceStatus(status);
                _logger.Log($"{_localization.GetString("Buttons.RefreshStatus")}: {GetStatusText(status)}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{_localization.GetString("LogMessages.Error")}: {ex.Message}");
            }
        }

        #endregion

        public void UpdateLanguage()
        {
            try
            {
                Text = _localization.GetString("Tabs.Service");

                _adminWarningLabel.Text = _localization.GetString("Messages.AdminRequired");
                _restartAsAdminButton.Text = _localization.GetString("Buttons.RestartAsAdmin");

                _serviceStatusGroupBox.Text = _localization.GetString("GroupBoxes.ServiceStatus");
                _serviceParamsGroupBox.Text = _localization.GetString("GroupBoxes.ServiceParameters");
                _serviceControlGroupBox.Text = _localization.GetString("GroupBoxes.ServiceControl");

                _installButton.Text = _localization.GetString("Buttons.InstallService");
                _uninstallButton.Text = _localization.GetString("Buttons.UninstallService");
                _startButton.Text = _localization.GetString("Buttons.StartService");
                _stopButton.Text = _localization.GetString("Buttons.StopService");
                _refreshStatusButton.Text = _localization.GetString("Buttons.RefreshStatus");
                _browseProfileButton.Text = _localization.GetString("Buttons.Browse");
                _browseLogButton.Text = _localization.GetString("Buttons.Browse");
                _useLogCheckBox.Text = _localization.GetString("Labels.UseLogFile");

                _languageComboBox.Items.Clear();
                _languageComboBox.Items.AddRange(new object[] {
                    _localization.GetString("Language.English"),
                    _localization.GetString("Language.Russian")
                });
                _languageComboBox.SelectedIndex = _localization.CurrentLanguage == Language.en ? 0 : 1;

                _statusLabel.Text = GetStatusText(_serviceManager.CurrentStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{_localization.GetString("LogMessages.Error")}: {ex.Message}");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    _serviceManager?.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"{_localization.GetString("LogMessages.Error")}: {ex.Message}");
                }
            }
            base.Dispose(disposing);
        }
    }
}