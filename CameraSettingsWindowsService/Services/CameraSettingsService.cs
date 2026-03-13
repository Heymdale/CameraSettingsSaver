using CameraSettingsSaver.Core;
using CameraSettingsSaver.Models;
using CameraSettingsSaver.Resources;
using CameraSettingsSaver.Utils;
using CameraSettingsWindowsService.Models;
using CameraSettingsWindowsService.Core;
using Microsoft.Extensions.Hosting;

namespace CameraSettingsWindowsService.Services
{
    public class CameraSettingsService : BackgroundService
    {
        private readonly ServiceSettings _settings;
        private readonly Localization _localization;
        private readonly BasicLogger _basicLogger;
        private readonly LocalizedLogger _logger;
        private readonly SettingsApplier _settingsApplier;
        private Timer? _timer;
        private int _executionCount = 0;
        private readonly object _lockObject = new object();

        public CameraSettingsService(
            ServiceSettings settings,
            Localization localization,
            BasicLogger basicLogger)
        {
            _settings = settings;
            _localization = localization;
            _basicLogger = basicLogger;
            _logger = new LocalizedLogger(basicLogger, localization);
            _settingsApplier = new SettingsApplier();

            _localization.SetLanguage(_settings.Language);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogMessage(_localization.lineSeparator);
            _logger.Log("Service.Starting");

            _logger.LogMessage(string.Format(_localization.GetString("Service.ProfilePath"),
                _settings.ProfilePath));
            _logger.LogMessage(string.Format(_localization.GetString("Service.Interval"),
                _settings.IntervalSeconds));

            _logger.LogMessage(string.Format(_localization.GetString("Service.LogFile"),
                _settings.LogFile ?? _localization.GetString("Console.NoLogging")));

            _logger.LogMessage(string.Format(_localization.GetString("Service.Language"),
                _settings.Language));
            _logger.LogMessage(_localization.lineSeparator);

            // Check for file existence on startup
            if (!File.Exists(_settings.ProfilePath))
            {
                _logger.LogWarning("Service.ProfileNotFound", _settings.ProfilePath);
            }

            // Create a directory for logs, if necessary
            if (_settings.LogFile != null)
            {
                string? logDirectory = Path.GetDirectoryName(_settings.LogFile);
                if (!string.IsNullOrEmpty(logDirectory) && !Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }
            }

            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(_settings.IntervalSeconds));

            return Task.CompletedTask;
        }

        private void DoWork(object? state)
        {
            lock (_lockObject)
            {
                _executionCount++;
                var now = DateTime.Now;

                try
                {
                    _logger.LogMessage("");
                    _logger.LogMessage(_localization.lineSeparator);
                    _logger.LogMessage(string.Format(_localization.GetString("Service.ExecutionStarted"),
                        _executionCount, now.ToString("yyyy-MM-dd HH:mm:ss")));
                    _logger.LogMessage(_localization.lineSeparator);

                    // Check the existence of the file before each execution
                    if (!File.Exists(_settings.ProfilePath))
                    {
                        _logger.LogError("Service.ProfileNotFound", _settings.ProfilePath);
                        _logger.LogMessage(_localization.lineSeparator);
                        _logger.LogMessage(string.Format(_localization.GetString("Service.ExecutionCompleted"),
                            _executionCount, "FAILED - PROFILE NOT FOUND"));
                        _logger.LogMessage(_localization.lineSeparator);
                        return;
                    }

                    var startupMode = new StartupMode
                    {
                        Profile = _settings.ProfilePath,
                        LogFile = _settings.LogFile,
                        Language = _settings.Language
                    };

                    bool success = _settingsApplier.ApplySettingsInBackground(
                        startupMode,
                        _localization,
                        _basicLogger);

                    _logger.LogMessage(_localization.lineSeparator);
                    _logger.LogMessage(string.Format(_localization.GetString("Service.ExecutionCompleted"),
                        _executionCount, success ? "SUCCESS" : "FAILED"));
                    _logger.LogMessage(_localization.lineSeparator);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Service.ExecutionError", ex.Message);
                    _logger.LogError("Service.StackTrace", ex.StackTrace ?? "No stack trace");
                }
            }
        }

        public override Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogMessage(_localization.lineSeparator);
            _logger.Log("Service.Stopping");
            _logger.LogMessage(string.Format(_localization.GetString("Service.TotalExecutions"), _executionCount));
            _logger.LogMessage(_localization.lineSeparator);

            _timer?.Change(Timeout.Infinite, 0);
            _timer?.Dispose();

            return base.StopAsync(stoppingToken);
        }
    }
}