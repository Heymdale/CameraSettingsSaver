using System.Diagnostics;
using System.Security.Principal;
using Microsoft.Win32;
using CameraSettingsSaver.Resources;
using WebCamSettings.Models;

namespace WebCamSettings.Core
{
    public class ServiceManager : IDisposable
    {
        private readonly string _serviceName = "CameraSettingsService";
        private readonly string _serviceDisplayName = "Camera Settings Service";
        private readonly BasicLogger _logger;
        private readonly Localization _localization;
        private readonly string _servicePath;
        private System.Threading.Timer _statusTimer;
        private ServiceStatus _currentStatus = ServiceStatus.Unknown;

        public event EventHandler<ServiceStatus> StatusChanged;

        public ServiceManager(BasicLogger logger, Localization localization)
        {
            _logger = logger;
            _localization = localization;

            string appRoot = Path.GetDirectoryName(Application.ExecutablePath);
            _servicePath = Path.Combine(appRoot, "bin", "CameraSettingsService.exe");
        }

        public string ServiceName => _serviceName;
        public string ServicePath => _servicePath;
        public ServiceStatus CurrentStatus => _currentStatus;

        public bool IsRunningAsAdministrator()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        public void RestartAsAdministrator()
        {
            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = true,
                WorkingDirectory = Environment.CurrentDirectory,
                FileName = Application.ExecutablePath,
                Verb = "runas"
            };

            try
            {
                Process.Start(startInfo);
                Application.Exit();
            }
            catch (Exception ex)
            {
                _logger.LogError($"{_localization.GetString("LogMessages.Error")}: {ex.Message}");
            }
        }

        public bool IsServiceInstalled()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(
                    @"SYSTEM\CurrentControlSet\Services\" + _serviceName))
                {
                    return key != null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{_localization.GetString("LogMessages.Error")}: {ex.Message}");
                return false;
            }
        }

        public ServiceStatus GetServiceStatus()
        {
            if (!IsServiceInstalled())
                return ServiceStatus.NotFound;

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "sc",
                    Arguments = $"query {_serviceName}",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (Process process = Process.Start(startInfo))
                {
                    process.WaitForExit();
                    string output = process.StandardOutput.ReadToEnd();

                    if (output.Contains("RUNNING"))
                        return ServiceStatus.Running;
                    else if (output.Contains("STOPPED"))
                        return ServiceStatus.Stopped;
                    else if (output.Contains("START_PENDING"))
                        return ServiceStatus.Starting;
                    else if (output.Contains("STOP_PENDING"))
                        return ServiceStatus.Stopping;
                    else
                        return ServiceStatus.Unknown;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{_localization.GetString("LogMessages.Error")}: {ex.Message}");
                return ServiceStatus.Unknown;
            }
        }

        public bool InstallService(string profilePath, Language language, int intervalSeconds, bool useLogFile, string? logPath = null)
        {
            try
            {
                if (!File.Exists(_servicePath))
                {
                    _logger.LogError($"Service executable not found: {_servicePath}");
                    return false;
                }

                var arguments = new List<string>
        {
            $"/p \\\"{profilePath}\\\"",
            $"/lang {language.ToString().ToLower()}",
            $"/interval {intervalSeconds}"
        };

                // If logging is enabled
                if (useLogFile)
                {
                    if (string.IsNullOrWhiteSpace(logPath))
                    {
                        // If the path is not specified, simply add /log (without value)
                        arguments.Add("/log");
                        _logger.Log("Adding /log parameter without path (service will use default)");
                    }
                    else
                    {
                        // If the path is specified, add /log with the path
                        arguments.Add($"/log \\\"{logPath}\\\"");
                        _logger.Log($"Adding /log parameter with path: {logPath}");
                    }
                }

                string args = string.Join(" ", arguments);
                string binPath = $"\\\"{_servicePath}\\\" {args}";

                _logger.Log($"Installing service with binPath: {binPath}");

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "sc",
                    Arguments = $"create {_serviceName} binPath= \"{binPath}\" start= auto DisplayName= \"{_serviceDisplayName}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (Process process = Process.Start(startInfo))
                {
                    process.WaitForExit();
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    if (!string.IsNullOrEmpty(output))
                        _logger.Log($"SC output: {output}");

                    if (!string.IsNullOrEmpty(error))
                        _logger.LogError($"SC error: {error}");

                    if (process.ExitCode == 0)
                    {
                        _logger.Log($"Service {_serviceName} installed successfully");
                        UpdateStatus();
                        return true;
                    }
                    else
                    {
                        _logger.LogError($"Failed to install service. Exit code: {process.ExitCode}");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error installing service: {ex.Message}");
                return false;
            }
        }

        public bool UninstallService()
        {
            try
            {
                if (GetServiceStatus() == ServiceStatus.Running)
                {
                    StopService();
                    Thread.Sleep(2000);
                }

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "sc",
                    Arguments = $"delete {_serviceName}",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (Process process = Process.Start(startInfo))
                {
                    process.WaitForExit();

                    if (process.ExitCode == 0)
                    {
                        _logger.Log($"{_localization.GetString("LogMessages.ServiceUninstalled")}: {_serviceName}");
                        UpdateStatus();
                        return true;
                    }
                    else
                    {
                        string error = process.StandardError.ReadToEnd();
                        _logger.LogError($"{_localization.GetString("LogMessages.Error")}: Failed to uninstall service: {error}");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{_localization.GetString("LogMessages.Error")}: {ex.Message}");
                return false;
            }
        }

        public bool StartService()
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "sc",
                    Arguments = $"start {_serviceName}",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (Process process = Process.Start(startInfo))
                {
                    process.WaitForExit();

                    if (process.ExitCode == 0)
                    {
                        _logger.Log($"{_localization.GetString("LogMessages.ServiceStarted")}: {_serviceName}");

                        Thread.Sleep(1000);
                        UpdateStatus();
                        return true;
                    }
                    else
                    {
                        string error = process.StandardError.ReadToEnd();
                        _logger.LogError($"{_localization.GetString("LogMessages.Error")}: Failed to start service: {error}");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{_localization.GetString("LogMessages.Error")}: {ex.Message}");
                return false;
            }
        }

        public bool StopService()
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "sc",
                    Arguments = $"stop {_serviceName}",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (Process process = Process.Start(startInfo))
                {
                    process.WaitForExit();

                    if (process.ExitCode == 0)
                    {
                        _logger.Log($"{_localization.GetString("LogMessages.ServiceStopped")}: {_serviceName}");

                        Thread.Sleep(1000);
                        UpdateStatus();
                        return true;
                    }
                    else
                    {
                        string error = process.StandardError.ReadToEnd();
                        _logger.LogError($"{_localization.GetString("LogMessages.Error")}: Failed to stop service: {error}");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{_localization.GetString("LogMessages.Error")}: {ex.Message}");
                return false;
            }
        }

        public void StartStatusMonitoring()
        {
            _statusTimer?.Dispose();
            _statusTimer = new System.Threading.Timer(CheckStatusCallback, null, 0, 2000);
        }

        public void StopStatusMonitoring()
        {
            _statusTimer?.Dispose();
            _statusTimer = null;
        }

        private void CheckStatusCallback(object state)
        {
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            try
            {
                var newStatus = GetServiceStatus();
                if (_currentStatus != newStatus)
                {
                    _currentStatus = newStatus;
                    StatusChanged?.Invoke(this, _currentStatus);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{_localization.GetString("LogMessages.Error")}: {ex.Message}");
            }
        }

        public void Dispose()
        {
            StopStatusMonitoring();
            _statusTimer?.Dispose();
        }
    }
}