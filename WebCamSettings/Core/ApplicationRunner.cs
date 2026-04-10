using CameraSettingsSaver.Resources;
using CameraSettingsSaver.Utils;
using CameraSettingsSaver.Core;
using WebCamSettings.Resources;
using System;

namespace WebCamSettings.Core
{
    public class ApplicationRunner
    {
        private const string CriticalLogFileName = "critical.log";
        private readonly StartupMode _startupMode;
        private readonly ArgumentParser _argumentParser;
        private readonly ConsoleManager _consoleManager;
        private readonly SettingsApplier _settingsApplier;
        private MessageDisplayer _messageDisplayer;

        private Localization _localization;
        private BasicLogger _logger;
        private bool _isConsoleStart = false;

        public ApplicationRunner()
        {
            _startupMode = new StartupMode();
            _argumentParser = new ArgumentParser(_startupMode);
            _consoleManager = new ConsoleManager();
            _settingsApplier = new SettingsApplier();
        }

        public void Run(string[] args)
        {
            if (args.Length > 0)
            {
                _argumentParser.Parse(args);
            }

            var mode = DetermineApplicationMode();

            switch (mode)
            {
                case ApplicationMode.NoInterface:
                    RunNoInterfaceMode();
                    break;
                case ApplicationMode.Console:
                    RunConsoleMode();
                    break;
                case ApplicationMode.Help:
                    RunHelpMode();
                    break;
                default:
                    RunGuiMode();
                    break;
            }
        }

        private ApplicationMode DetermineApplicationMode()
        {
            if (_startupMode.IsShowHelp) return ApplicationMode.Help;
            if (_startupMode.IsConsole) return ApplicationMode.Console;
            if (_startupMode.IsNoInterface) return ApplicationMode.NoInterface;
            return ApplicationMode.Gui;
        }

        private void RunNoInterfaceMode()
        {
            InitializeServices();
            _messageDisplayer.LogStartupMessages(_startupMode);

            var success = _settingsApplier.ApplySettingsInBackground(
                _startupMode, _localization, _logger);

            Environment.Exit(success ? 0 : 1);
        }

        private void RunConsoleMode()
        {
            if (!_consoleManager.Initialize())
            {
                _startupMode.CriticalStartupExceptions.Add("Cannot start console mode");
                // Create a temporary logger to display the error
                var tempLogger = new BasicLogger(CriticalLogFileName);
                var tempDisplayer = new MessageDisplayer(tempLogger);
                tempDisplayer.ShowCriticalErrors(_startupMode, false);
                Environment.Exit(1);
                return;
            }

            _isConsoleStart = true;
            InitializeServices();
            _messageDisplayer.ShowStartupMessages(_startupMode, _isConsoleStart);

            if (_startupMode.IsShowHelp)
            {
                ShowHelp();
                return;
            }

            ApplySettingsInConsoleMode();
        }

        private void RunGuiMode()
        {
            InitializeServices();
            _messageDisplayer.ShowStartupMessages(_startupMode, _isConsoleStart);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm(_localization, _startupMode.Profile, _startupMode.LogFile));
        }

        private void RunHelpMode()
        {
            _consoleManager.Initialize();
            _isConsoleStart = true;
            InitializeServices();
            ShowHelp();
        }

        private void InitializeServices()
        {
            try
            {
                _localization = new Localization();
                _localization.SetLanguage(_startupMode.Language);

                if (_localization.Warnings != null)
                {
                    _startupMode.StartupExceptions.AddRange(_localization.Warnings);
                }
            }
            catch (LocalizationErrorLoadMainLanguage ex)
            {
                _startupMode.CriticalStartupExceptions.Add(ex.Message);
            }

            // Create a logger depending on the mode
            if (_startupMode.IsConsole || _startupMode.IsNoInterface || _startupMode.IsShowHelp)
            {
                // Console mode - using BasicLogger
                _logger = new BasicLogger(_startupMode.LogFile);
                if (_startupMode.IsConsole || _startupMode.IsShowHelp)
                {
                    _logger.EnableConsoleOutput();
                }
            }
            else
            {
                // GUI mode - using FormsLogger
                _logger = new FormsLogger(_startupMode.LogFile);
            }

            // Create a MessageDisplayer with a logger and localization
            _messageDisplayer = new MessageDisplayer(_logger, _localization);
        }

        private void ApplySettingsInConsoleMode()
        {
            try
            {
                _consoleManager.ShowHeader(_startupMode, _localization, _logger);

                var success = _settingsApplier.ApplySettingsWithConsoleOutput(
                    _startupMode, _localization, _logger);

                _consoleManager.ShowFooter(_logger);
                _consoleManager.WaitForExit(_isConsoleStart);
                Environment.Exit(success ? 0 : 1);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error: {ex.Message}");
                _consoleManager.ShowFooter(_logger);
                _consoleManager.WaitForExit(_isConsoleStart);
                Environment.Exit(2);
            }
            finally
            {
                _consoleManager.Cleanup();
            }
        }

        private void ShowHelp()
        {
            if (_logger == null || _localization == null) return;

            var help = new CameraSettingsSaver.Resources.Help();
            string helpText = help.GetConsoleHelp(_localization);

            // In console mode, output directly
            if (_isConsoleStart)
            {
                Console.WriteLine(helpText);
            }
            else
            {
                // Or log
                _logger.Log(helpText);
            }

            Environment.Exit(0);
        }
    }
}