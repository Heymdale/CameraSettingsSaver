using System;
using System.Windows.Forms;
using CameraSettingsSaver.Resources;

namespace WebCamSettings.Resources
{
    public class FormsLogger : BasicLogger
    {
        private TextBox _logsTextBox;
        private bool _isTextBoxReady = false;

        public FormsLogger(string? logFile)
            : base(logFile)
        {
            this.OnLogMessage += HandleLogMessage;
        }

        public void SetLogsTextBox(TextBox logsTextBox)
        {
            _logsTextBox = logsTextBox;
            if (logsTextBox != null)
            {
                _logsTextBox.HandleCreated += (s, e) =>
                {
                    _isTextBoxReady = true;
                    UpdateTextBoxWithAllLogs();
                };

                if (logsTextBox.IsHandleCreated)
                {
                    _isTextBoxReady = true;
                    UpdateTextBoxWithAllLogs();
                }
            }
        }

        private void UpdateTextBoxWithAllLogs()
        {
            if (_logsTextBox != null && _isTextBoxReady && !_logsTextBox.IsDisposed)
            {
                try
                {
                    if (_logsTextBox.InvokeRequired)
                    {
                        _logsTextBox.Invoke((MethodInvoker)delegate
                        {
                            SafeSetTextBoxText(GetAllLogs());
                        });
                    }
                    else
                    {
                        SafeSetTextBoxText(GetAllLogs());
                    }
                }
                catch (InvalidOperationException)
                {
                    _isTextBoxReady = false;
                }
            }
        }

        private void HandleLogMessage(string logMessage)
        {
            UpdateTextBox(logMessage);
        }

        private void UpdateTextBox(string logMessage)
        {
            if (_logsTextBox != null && _isTextBoxReady && !_logsTextBox.IsDisposed)
            {
                try
                {
                    if (_logsTextBox.InvokeRequired)
                    {
                        _logsTextBox.BeginInvoke((MethodInvoker)delegate
                        {
                            SafeAppendTextBox(logMessage);
                        });
                    }
                    else
                    {
                        SafeAppendTextBox(logMessage);
                    }
                }
                catch (InvalidOperationException)
                {
                    _isTextBoxReady = false;
                }
            }
        }

        private void SafeSetTextBoxText(string text)
        {
            if (_logsTextBox != null && !_logsTextBox.IsDisposed && _logsTextBox.IsHandleCreated)
            {
                _logsTextBox.Text = text;
                _logsTextBox.SelectionStart = _logsTextBox.Text.Length;
                _logsTextBox.ScrollToCaret();
            }
        }

        private void SafeAppendTextBox(string text)
        {
            if (_logsTextBox != null && !_logsTextBox.IsDisposed && _logsTextBox.IsHandleCreated)
            {
                _logsTextBox.AppendText(text + Environment.NewLine);
                _logsTextBox.SelectionStart = _logsTextBox.Text.Length;
                _logsTextBox.ScrollToCaret();
            }
        }

        // GUI-specific methods
        public void ShowMessageBox(string message, string caption, MessageBoxIcon icon)
        {
            MessageBox.Show(message, caption, MessageBoxButtons.OK, icon);
        }

        public void CopyLogsToClipboard()
        {
            string logs = GetAllLogs();
            if (!string.IsNullOrEmpty(logs))
            {
                try
                {
                    Clipboard.SetText(logs);
                }
                catch
                {
                    // Ignore clipboard errors
                }
            }
        }
    }
}