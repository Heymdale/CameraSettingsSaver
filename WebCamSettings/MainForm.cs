using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CameraSettingsSaver.Models;
using CameraSettingsSaver.Utils;
using CameraSettingsSaver.Resources;
using WebCamSettings.Forms;
using WebCamSettings.Resources;

namespace WebCamSettings
{
    public partial class MainForm : Form
    {
        // Color scheme
        public Color BackgroundColor { get; } = Color.FromArgb(30, 30, 30);
        public Color SecondaryBackgroundColor { get; } = Color.FromArgb(37, 37, 37);
        public Color TextColor { get; } = Color.FromArgb(204, 204, 204);
        public Color AccentColor { get; } = Color.FromArgb(155, 39, 204);
        public Color WarningColor { get; } = Color.FromArgb(245, 124, 0);
        public Color SuccessColor { get; } = Color.FromArgb(56, 142, 60);
        public Color LogTextColor { get; } = Color.Lime;

        // Fonts
        public Font DefaultFont { get; } = new Font("Segoe UI", 12);
        public Font MonospaceFont { get; } = new Font("Consolas", 11);

        // Tabs
        private TabControl _tabControl;
        private CamerasTab _camerasTab;
        private GuideTab _guideTab;
        private LogsTab _logsTab;
        private ServiceTab _serviceTab;

        // UI Controls
        private GroupBox _configFileGroupBox;
        private TextBox _configFilePathTextBox;
        private Button _browseConfigFileButton;
        private Button _refreshButton;
        private Button _saveSettingsButton;
        private Button _applySettingsButton;
        private RadioButton _englishRadioButton;
        private RadioButton _russianRadioButton;

        // Services
        private readonly Localization _localization;
        private readonly string _defaultConfName;
        private readonly FormsLogger _formsLogger;
        private readonly LocalizedLogger _logger;
        private readonly CameraService _cameraService;
        private readonly CameraSettingsSaver.Resources.Help _help;
        private CameraPreviewForm _previewForm;

        // Data
        private List<CameraInfo> _cameras = new List<CameraInfo>();

        public MainForm(Localization localization, string defaultConfName, string? logFile = null)
        {
            _localization = localization;
            _defaultConfName = defaultConfName;
            _formsLogger = new FormsLogger(logFile);
            _logger = new LocalizedLogger(_formsLogger, localization);
            _cameraService = new CameraService(_formsLogger, localization);
            _help = new();

            InitializeComponent();
            InitializeUI();
            LoadCameras();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            UpdateLanguageUI();
            _formsLogger.SetLogsTextBox(_logsTab.LogsTextBox);
        }

        private void InitializeUI()
        {
            this.Controls.Clear();

            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(900, 700);
            this.BackColor = BackgroundColor;
            this.ForeColor = TextColor;
            this.Font = DefaultFont;

            var mainTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                RowCount = 4,
                ColumnCount = 1
            };
            mainTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            mainTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // Language selection
            var languagePanel = CreateLanguagePanel();

            // Tab Control
            _tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Appearance = TabAppearance.FlatButtons,
                Margin = new Padding(0, 10, 0, 10)
            };

            // Create tabs
            InitializeTabs();

            // Config File Group
            _configFileGroupBox = CreateConfigFileGroupBox();

            // Action buttons
            var actionButtonsPanel = CreateActionButtonsPanel();

            // Add controls to main table
            mainTable.Controls.Add(languagePanel, 0, 0);
            mainTable.Controls.Add(_tabControl, 0, 1);
            mainTable.Controls.Add(_configFileGroupBox, 0, 2);
            mainTable.Controls.Add(actionButtonsPanel, 0, 3);

            this.Controls.Add(mainTable);
            UpdateLanguageUI();
        }

        private FlowLayoutPanel CreateLanguagePanel()
        {
            var panel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 10)
            };

            _russianRadioButton = new RadioButton
            {
                Text = _localization.GetString("Language.Russian"),
                AutoSize = true,
                ForeColor = TextColor,
                Checked = (_localization.CurrentLanguage == Language.ru)
            };
            _russianRadioButton.CheckedChanged += LanguageRadioButton_CheckedChanged;

            _englishRadioButton = new RadioButton
            {
                Text = _localization.GetString("Language.English"),
                AutoSize = true,
                ForeColor = TextColor,
                Checked = (_localization.CurrentLanguage == Language.en)
            };
            _englishRadioButton.CheckedChanged += LanguageRadioButton_CheckedChanged;

            var languageLabel = new Label
            {
                Text = _localization.GetString("Labels.Language"),
                AutoSize = true,
                ForeColor = AccentColor,
                Font = new Font(this.Font, FontStyle.Bold)
            };

            panel.Controls.Add(_russianRadioButton);
            panel.Controls.Add(_englishRadioButton);
            panel.Controls.Add(languageLabel);

            return panel;
        }

        private void InitializeTabs()
        {
            _camerasTab = new CamerasTab(this, _localization, _formsLogger);
            _camerasTab.CameraSelected += OnCameraSelected;
            _camerasTab.PreviewRequested += OnPreviewRequested;
            _camerasTab.SettingsRequested += OnSettingsRequested;

            _guideTab = new GuideTab(this, _localization, _help);
            _logsTab = new LogsTab(this, _localization);
            _logsTab.ClearLogsRequested += OnClearLogsRequested;
            _logsTab.CopyLogsRequested += OnCopyLogsRequested;
            _logsTab.SaveLogsRequested += OnSaveLogsRequested;

            _serviceTab = new ServiceTab(this, _localization, _formsLogger);

            _tabControl.TabPages.Add(_camerasTab);
            _tabControl.TabPages.Add(_guideTab);
            _tabControl.TabPages.Add(_logsTab);
            _tabControl.TabPages.Add(_serviceTab);
        }

        private GroupBox CreateConfigFileGroupBox()
        {
            var groupBox = new GroupBox
            {
                Text = _localization.GetString("GroupBoxes.SettingsFile"),
                Dock = DockStyle.Fill,
                ForeColor = AccentColor,
                BackColor = SecondaryBackgroundColor,
                Height = 100,
                FlatStyle = FlatStyle.Flat
            };

            groupBox.Paint += (s, e) =>
            {
                e.Graphics.DrawRectangle(new Pen(AccentColor, 3),
                    new Rectangle(0, 0, groupBox.Width - 1, groupBox.Height - 1));
            };

            var configTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(10, 20, 10, 20)
            };

            configTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
            configTable.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            _configFilePathTextBox = new TextBox
            {
                Text = _defaultConfName,
                Dock = DockStyle.Fill,
                BackColor = SecondaryBackgroundColor,
                ForeColor = TextColor,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 0, 10, 0)
            };

            _browseConfigFileButton = CreateStyledButton(_localization.GetString("Buttons.Browse"),
                Color.FromArgb(62, 62, 62));
            _browseConfigFileButton.AutoSize = true;
            _browseConfigFileButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            _browseConfigFileButton.Click += BrowseConfigFileButton_Click;
            _browseConfigFileButton.Margin = new Padding(0);
            _browseConfigFileButton.MinimumSize = new Size(100, 30);

            configTable.Controls.Add(_configFilePathTextBox, 0, 0);
            configTable.Controls.Add(_browseConfigFileButton, 1, 0);

            groupBox.Controls.Add(configTable);
            return groupBox;
        }

        private FlowLayoutPanel CreateActionButtonsPanel()
        {
            var panel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true,
                Dock = DockStyle.Fill
            };

            _refreshButton = CreateStyledButton(_localization.GetString("Buttons.RefreshList"), WarningColor);
            _refreshButton.Click += RefreshButton_Click;

            _saveSettingsButton = CreateStyledButton(_localization.GetString("Buttons.SaveSettings"), SuccessColor);
            _saveSettingsButton.Click += SaveSettingsButton_Click;

            _applySettingsButton = CreateStyledButton(_localization.GetString("Buttons.ApplySettings"), AccentColor);
            _applySettingsButton.Click += ApplySettingsButton_Click;

            panel.Controls.Add(_applySettingsButton);
            panel.Controls.Add(_saveSettingsButton);
            panel.Controls.Add(_refreshButton);

            return panel;
        }

        private Button CreateStyledButton(string text, Color backColor)
        {
            return new Button
            {
                Text = text,
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = {
                    BorderColor = Color.FromArgb(85, 85, 85),
                    BorderSize = 1
                },
                Margin = new Padding(2, 5, 2, 5),
                Padding = new Padding(12, 4, 12, 4),
                Cursor = Cursors.Hand,
                Font = DefaultFont,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                TextAlign = ContentAlignment.MiddleCenter,
                MinimumSize = new Size(0, 30)
            };
        }

        #region Event Handlers

        private void LanguageRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is RadioButton radioButton && radioButton.Checked)
            {
                bool isEnglish = radioButton == _englishRadioButton;
                _localization.SetLanguage(isEnglish ? Language.en : Language.ru);
                UpdateLanguageUI();

                // Update LocalizedLogger with the new localization
                _logger.UpdateLocalization(_localization);
            }
        }

        private void OnCameraSelected(object sender, ListViewItem e)
        {
            // You can add additional logic when selecting a camera
        }

        private void OnPreviewRequested(object sender, CameraInfo cameraInfo)
        {
            StartCameraPreview(cameraInfo);
        }

        private void OnSettingsRequested(object sender, CameraInfo cameraInfo)
        {
            OpenCameraSettings(cameraInfo);
        }

        private void OnClearLogsRequested(object sender, EventArgs e)
        {
            _formsLogger.ClearLogs();
        }

        private void OnCopyLogsRequested(object sender, EventArgs e)
        {
            _formsLogger.CopyLogsToClipboard();
        }

        private void OnSaveLogsRequested(object sender, string logsText)
        {
            SaveLogsToFile(logsText);
        }

        #endregion

        #region Camera Operations

        private void LoadCameras()
        {
            _cameras = _cameraService.GetAvailableCameras();
            UpdateCamerasListView();
        }

        private void UpdateCamerasListView()
        {
            _camerasTab.CamerasListView.Items.Clear();

            foreach (var camera in _cameras)
            {
                var item = new ListViewItem(camera.Name);
                item.SubItems.Add(TruncateDevicePath(camera.DevicePath));
                item.Tag = camera;
                item.Checked = camera.IsSelected;
                _camerasTab.CamerasListView.Items.Add(item);
            }
        }

        private void StartCameraPreview(CameraInfo cameraInfo)
        {
            try
            {
                ClosePreviewForm();

                var targetDevice = _cameraService.FindDeviceForPreview(cameraInfo);
                if (targetDevice == null)
                {
                    string errorMessage = _localization.GetString("LogMessages.CameraNotFoundForPreview");
                    _formsLogger.ShowMessageBox(errorMessage,
                        _localization.GetString("Common.Error"),
                        MessageBoxIcon.Error);
                    return;
                }

                _previewForm = new CameraPreviewForm();
                _previewForm.FormClosed += (s, e) =>
                {
                    _previewForm?.Dispose();
                    _previewForm = null;
                };

                _previewForm.StartPreview(targetDevice);
                _logger.Log("LogMessages.PreviewStarted", cameraInfo.Name);
            }
            catch (Exception ex)
            {
                string errorMessage = _localization.GetString("LogMessages.PreviewError", ex.Message);
                _formsLogger.ShowMessageBox(errorMessage,
                    _localization.GetString("Common.Error"),
                    MessageBoxIcon.Error);
                _logger.LogError("LogMessages.MonikerError", ex.Message);

                _previewForm?.Dispose();
                _previewForm = null;
            }
        }

        private void OpenCameraSettings(CameraInfo cameraInfo)
        {
            try
            {
                var targetDevice = _cameraService.FindDeviceForSettings(cameraInfo);
                if (targetDevice == null)
                {
                    string errorMessage = _localization.GetString("LogMessages.CameraNotFoundForSettings");
                    _formsLogger.ShowMessageBox(errorMessage,
                        _localization.GetString("Common.Error"),
                        MessageBoxIcon.Error);
                    return;
                }

                CameraSettingsDialog.ShowSettingsDialog(targetDevice);
                _logger.Log("LogMessages.SettingsDialogOpened", cameraInfo.Name);
            }
            catch (Exception ex)
            {
                string errorMessage = _localization.GetString("LogMessages.SettingsDialogError", ex.Message);
                _formsLogger.ShowMessageBox(errorMessage,
                    _localization.GetString("Common.Error"),
                    MessageBoxIcon.Error);
                _logger.LogError("LogMessages.MonikerError", ex.Message);
            }
        }

        #endregion

        #region Button Click Handlers

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            ClosePreviewForm();
            LoadCameras();

            _camerasTab.PreviewButton.Enabled = false;
            _camerasTab.SettingsButton.Enabled = false;
        }

        private void SaveSettingsButton_Click(object sender, EventArgs e)
        {
            var selectedCameras = GetSelectedCameras();
            if (selectedCameras.Count == 0)
            {
                string warningMessage = _localization.GetString("Messages.SelectAtLeastOneCamera");
                _formsLogger.ShowMessageBox(warningMessage,
                    _localization.GetString("Common.Warning"),
                    MessageBoxIcon.Warning);
                return;
            }

            var allSettings = new List<CameraSettings>();
            foreach (var camera in selectedCameras)
            {
                var settings = _cameraService.GetCameraSettings(camera);
                if (settings != null)
                {
                    allSettings.Add(settings);
                }
            }

            if (allSettings.Count > 0)
            {
                SaveSettingsToFile(allSettings);
            }
            else
            {
                string warningMessage = _localization.GetString("Messages.FailedToGetSettings");
                _formsLogger.ShowMessageBox(warningMessage,
                    _localization.GetString("Common.Warning"),
                    MessageBoxIcon.Warning);
            }
        }

        private void ApplySettingsButton_Click(object sender, EventArgs e)
        {
            var selectedCameras = GetSelectedCameras();
            if (selectedCameras.Count == 0)
            {
                string warningMessage = _localization.GetString("Messages.SelectAtLeastOneCamera");
                _formsLogger.ShowMessageBox(warningMessage,
                    _localization.GetString("Common.Warning"),
                    MessageBoxIcon.Warning);
                return;
            }

            var loadedSettings = LoadSettingsFromFile();
            if (loadedSettings == null || loadedSettings.Count == 0)
            {
                return;
            }

            ApplySettingsToCameras(selectedCameras, loadedSettings);
        }

        private void BrowseConfigFileButton_Click(object sender, EventArgs e)
        {
            using (var openDialog = new OpenFileDialog())
            {
                openDialog.Filter = _localization.GetString("FileDialogs.JsonFilter");
                openDialog.DefaultExt = ".json";
                openDialog.FileName = _defaultConfName;

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    _configFilePathTextBox.Text = openDialog.FileName;
                }
            }
        }

        #endregion

        #region Helper Methods

        private List<CameraInfo> GetSelectedCameras()
        {
            var selected = new List<CameraInfo>();
            foreach (ListViewItem item in _camerasTab.CamerasListView.Items)
            {
                if (item.Checked)
                {
                    selected.Add((CameraInfo)item.Tag);
                }
            }
            return selected;
        }

        private void SaveSettingsToFile(List<CameraSettings> settings)
        {
            try
            {
                string filePath = _configFilePathTextBox.Text;
                JsonHelper.SaveToFile(filePath, settings);

                string successMessage = _localization.GetString("Messages.SettingsSaveSuccess", filePath);
                _formsLogger.ShowMessageBox(successMessage,
                    _localization.GetString("Common.Success"),
                    MessageBoxIcon.Information);
                _logger.Log("Messages.SettingsSaveSuccess", filePath);
            }
            catch (Exception ex)
            {
                string errorMessage = _localization.GetString("Messages.SettingsSaveError", ex.Message);
                _formsLogger.ShowMessageBox(errorMessage,
                    _localization.GetString("Common.Error"),
                    MessageBoxIcon.Error);
                _logger.LogError("LogMessages.Error", ex.Message);
            }
        }

        private List<CameraSettings> LoadSettingsFromFile()
        {
            try
            {
                string filePath = _configFilePathTextBox.Text;
                var settings = JsonHelper.LoadFromFile(filePath);

                if (!JsonHelper.ValidateSettings(settings))
                {
                    string errorMessage = _localization.GetString("Messages.SettingsLoadError",
                        "Some camera settings do not contain Moniker. This file is not compatible with this version.");
                    _formsLogger.ShowMessageBox(errorMessage,
                        _localization.GetString("Common.Error"),
                        MessageBoxIcon.Error);
                    return null;
                }

                _logger.Log("Messages.SettingsLoaded", filePath, settings.Count);
                return settings;
            }
            catch (FileNotFoundException ex)
            {
                string errorMessage = _localization.GetString("Messages.FileNotFound", ex.Message);
                _formsLogger.ShowMessageBox(errorMessage,
                    _localization.GetString("Common.Error"),
                    MessageBoxIcon.Error);
                return null;
            }
            catch (Exception ex)
            {
                string errorMessage = _localization.GetString("Messages.SettingsLoadError", ex.Message);
                _formsLogger.ShowMessageBox(errorMessage,
                    _localization.GetString("Common.Error"),
                    MessageBoxIcon.Error);
                _logger.LogError("LogMessages.Error", ex.Message);
                return null;
            }
        }

        private void ApplySettingsToCameras(List<CameraInfo> cameras, List<CameraSettings> allSettings)
        {
            int appliedCount = 0;
            int failedCount = 0;

            foreach (var camera in cameras)
            {
                var cameraSettings = allSettings.FirstOrDefault(s => s.MonikerString == camera.MonikerString);
                if (cameraSettings != null)
                {
                    _logger.Log("CameraMessages.ApplyingSettings", camera.Name);
                    var result = _cameraService.ApplyCameraSettings(cameraSettings);

                    if (result == OperationResult.Success)
                    {
                        appliedCount++;
                        _logger.Log("CameraMessages.SettingsAppliedSuccess", camera.Name);
                    }
                    else
                    {
                        failedCount++;
                        _logger.Log("CameraMessages.SettingsAppliedFailed", camera.Name);
                    }
                }
                else
                {
                    _logger.Log("LogMessages.SettingsNotFoundForMoniker", camera.Name);
                    failedCount++;
                }
            }

            string infoMessage = _localization.GetString("Messages.SettingsAppliedResult", appliedCount, failedCount);
            _formsLogger.ShowMessageBox(infoMessage,
                _localization.GetString("Common.Information"),
                MessageBoxIcon.Information);
        }

        private void SaveLogsToFile(string logsText)
        {
            using (var saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = _localization.GetString("FileDialogs.TextFilter");
                saveDialog.DefaultExt = ".txt";
                saveDialog.FileName = string.Format(_localization.GetString("FileDialogs.DefaultLogName"),
                    DateTime.Now.ToString("yyyyMMdd_HHmmss"));

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        File.WriteAllText(saveDialog.FileName, logsText);
                        _logger.Log("LogMessages.LogsSavedToFile", saveDialog.FileName);

                        string successMessage = _localization.GetString("Messages.LogsSaved", saveDialog.FileName);
                        _formsLogger.ShowMessageBox(successMessage,
                            _localization.GetString("Common.Success"),
                            MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("LogMessages.Error", ex.Message);

                        string errorMessage = _localization.GetString("Messages.LogsSaveError", ex.Message);
                        _formsLogger.ShowMessageBox(errorMessage,
                            _localization.GetString("Common.Error"),
                            MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ClosePreviewForm()
        {
            if (_previewForm != null)
            {
                _previewForm.StopPreview();
                _previewForm?.Close();
                _previewForm?.Dispose();
                _previewForm = null;
            }
        }

        private string TruncateDevicePath(string devicePath, int maxLength = 60)
        {
            if (string.IsNullOrEmpty(devicePath)) return "N/A";
            if (devicePath.Length <= maxLength) return devicePath;
            return devicePath.Substring(0, maxLength) + "...";
        }

        private void UpdateLanguageUI()
        {
            this.Text = _localization.GetString("Common.AppTitle");
            _camerasTab.UpdateLanguage();
            _guideTab.UpdateLanguage();
            _logsTab.UpdateLanguage();
            _serviceTab.UpdateLanguage();

            _browseConfigFileButton.Text = _localization.GetString("Buttons.Browse");
            _refreshButton.Text = _localization.GetString("Buttons.RefreshList");
            _saveSettingsButton.Text = _localization.GetString("Buttons.SaveSettings");
            _applySettingsButton.Text = _localization.GetString("Buttons.ApplySettings");
            _configFileGroupBox.Text = _localization.GetString("GroupBoxes.SettingsFile");
            _englishRadioButton.Text = _localization.GetString("Language.English");
            _russianRadioButton.Text = _localization.GetString("Language.Russian");
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ClosePreviewForm();

                if (components != null)
                {
                    components.Dispose();
                    components = null;
                }

                _camerasTab?.Dispose();
                _guideTab?.Dispose();
                _logsTab?.Dispose();
                _serviceTab?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}