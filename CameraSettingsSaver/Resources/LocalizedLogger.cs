using System;

namespace CameraSettingsSaver.Resources
{
    public class LocalizedLogger
    {
        private readonly BasicLogger _logger;
        private Localization _localization;

        public LocalizedLogger(BasicLogger logger, Localization localization)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        }

        public void UpdateLocalization(Localization newLocalization)
        {
            if (newLocalization != null)
            {
                _localization = newLocalization;
            }
        }

        // Basic logging methods with localization
        public void Log(string key, params object[] args)
        {
            string message = _localization.GetString(key, args);
            _logger.Log(message);
        }

        public void LogError(string key, params object[] args)
        {
            string message = _localization.GetString(key, args);
            _logger.LogError(message);
        }

        public void LogWarning(string key, params object[] args)
        {
            string message = _localization.GetString(key, args);
            _logger.LogWarning(message);
        }

        // Methods for direct logging (without localization)
        public void LogMessage(string message)
        {
            _logger.Log(message);
        }

        public void LogErrorMessage(string message)
        {
            _logger.LogError(message);
        }

        public void LogWarningMessage(string message)
        {
            _logger.LogWarning(message);
        }

        // Delegate methods to BasicLogger
        public void EnableConsoleOutput()
        {
            _logger.EnableConsoleOutput();
        }

        public string GetAllLogs()
        {
            return _logger.GetAllLogs();
        }

        public void ClearLogs()
        {
            _logger.ClearLogs();
        }
    }
}