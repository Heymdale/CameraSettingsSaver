using CameraSettingsSaver.Resources;
using CameraSettingsSaver.Utils;
using System;
using System.Windows.Forms;

namespace WebCamSettings.Core
{
    public class MessageDisplayer
    {
        private readonly BasicLogger _basicLogger;
        private readonly LocalizedLogger _localizedLogger;
        private readonly Localization _localization;
        private readonly bool _useLocalizedMessages;

        public MessageDisplayer(BasicLogger basicLogger, Localization localization = null)
        {
            _basicLogger = basicLogger ?? throw new ArgumentNullException(nameof(basicLogger));
            _localization = localization;
            _useLocalizedMessages = localization != null;

            if (_useLocalizedMessages)
            {
                _localizedLogger = new LocalizedLogger(basicLogger, localization);
            }
        }

        public void ShowStartupMessages(StartupMode startupMode, bool isConsoleStart)
        {
            ShowStartupWarnings(startupMode, isConsoleStart);
            ShowCriticalErrors(startupMode, isConsoleStart);
        }

        public void LogStartupMessages(StartupMode startupMode)
        {
            if (startupMode.StartupExceptions.Count > 0)
            {
                string warningMessage = string.Join("\n", startupMode.StartupExceptions);
                if (_useLocalizedMessages)
                {
                    // For localized warnings (if any)
                    _basicLogger.Log(warningMessage);
                }
                else
                {
                    _basicLogger.Log(warningMessage);
                }
            }

            if (startupMode.CriticalStartupExceptions.Count > 0)
            {
                string errorMessage = string.Join("\n", startupMode.CriticalStartupExceptions);
                if (_useLocalizedMessages)
                {
                    // For localized errors (if any)
                    _basicLogger.LogError(errorMessage);
                }
                else
                {
                    _basicLogger.LogError(errorMessage);
                }
            }
        }

        public void ShowStartupWarnings(StartupMode startupMode, bool isConsoleStart)
        {
            if (startupMode.StartupExceptions.Count > 0)
            {
                WriteToConsoleOrMessageBox(string.Join("\n", startupMode.StartupExceptions),
                    isConsoleStart, false);
            }
        }

        public void ShowCriticalErrors(StartupMode startupMode, bool isConsoleStart = false)
        {
            if (startupMode.CriticalStartupExceptions.Count > 0)
            {
                WriteToConsoleOrMessageBox(string.Join("\n", startupMode.CriticalStartupExceptions),
                    isConsoleStart, true);
            }
        }

        private void WriteToConsoleOrMessageBox(string message, bool isConsoleStart, bool isCritical)
        {
            if (isConsoleStart)
            {
                if (isCritical)
                {
                    // We also log critical errors
                    _basicLogger.LogError(message);
                }
                else
                {
                    _basicLogger.Log(message);
                }
            }
            else
            {
                // GUI mode
                MessageBoxIcon icon = isCritical ? MessageBoxIcon.Error : MessageBoxIcon.Warning;
                string caption = GetLocalizedCaption(isCritical ? "Common.Error" : "Common.Warning");

                MessageBox.Show(message, caption, MessageBoxButtons.OK, icon);

                // Also log
                if (isCritical)
                {
                    _basicLogger.LogError(message);
                }
                else
                {
                    _basicLogger.Log(message);
                }
            }

            if (isCritical)
            {
                Environment.Exit(1);
            }
        }

        private string GetLocalizedCaption(string captionKey)
        {
            if (_useLocalizedMessages)
            {
                try
                {
                    return _localization.GetString(captionKey);
                }
                catch
                {
                    return "Camera Settings Saver";
                }
            }
            return "Camera Settings Saver";
        }

        // Methods for working with localized messages
        public void ShowLocalizedWarning(string messageKey, params object[] args)
        {
            if (_useLocalizedMessages)
            {
                string message = _localization.GetString(messageKey, args);
                _localizedLogger.Log(message);
                ShowMessageBox(message, "Common.Warning", MessageBoxIcon.Warning);
            }
        }

        public void ShowLocalizedError(string messageKey, params object[] args)
        {
            if (_useLocalizedMessages)
            {
                string message = _localization.GetString(messageKey, args);
                _localizedLogger.LogError(message);
                ShowMessageBox(message, "Common.Error", MessageBoxIcon.Error);
            }
        }

        public void ShowLocalizedInformation(string messageKey, params object[] args)
        {
            if (_useLocalizedMessages)
            {
                string message = _localization.GetString(messageKey, args);
                _localizedLogger.Log(message);
                ShowMessageBox(message, "Common.Information", MessageBoxIcon.Information);
            }
        }

        private void ShowMessageBox(string message, string captionKey, MessageBoxIcon icon)
        {
            string caption = _useLocalizedMessages
                ? _localization.GetString(captionKey)
                : "Camera Settings Saver";

            MessageBox.Show(message, caption, MessageBoxButtons.OK, icon);
        }
    }
}