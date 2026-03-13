using CameraSettingsSaver.Resources;

namespace CameraSettingsSaver.Utils
{
    public class StartupMode
    {
        private bool _isConsole = false;
        private bool _isNoInterface = false;
        private bool _isShowHelp = false;

        private static readonly string _modeConflictError =
            "Console mode and no-interface mode are mutually exclusive. " +
            "Console mode takes precedence, and the program will run in console mode.";

        private static readonly string _showHelpConflictError =
            "Show help set program mode to \"console\"" + _modeConflictError;

        public Language Language { get; set; } = Language.en;
        public string Profile { get; set; } = "web_conf.json";
        public string? LogFile { get; set; } = null;
        public List<string> StartupExceptions { get; } = [];
        public List<string> CriticalStartupExceptions { get; } = [];

        public bool IsConsole
        {
            get => _isConsole;
            set
            {
                if (value)
                {
                    if (_isNoInterface)
                    {
                        StartupExceptions.Add(_modeConflictError);
                        _isNoInterface = false;
                    }
                    _isConsole = true;
                }
                else
                {
                    _isConsole = false;
                }
            }
        }

        public bool IsNoInterface
        {
            get => _isNoInterface;
            set
            {
                if (value)
                {
                    if (_isConsole)
                    {
                        StartupExceptions.Add(_modeConflictError);
                        _isNoInterface = false;
                    }
                    else
                    {
                        _isNoInterface = true;
                    }
                }
                else
                {
                    _isNoInterface = false;
                }
            }
        }

        public bool IsShowHelp
        {
            get => _isShowHelp;
            set
            {
                if (value)
                {
                    if (_isNoInterface)
                    {
                        StartupExceptions.Add(_showHelpConflictError);
                        _isNoInterface = false;
                    }
                    _isConsole = true;
                    _isShowHelp = true;
                }
                else
                {
                    _isShowHelp = false;
                }
            }
        }

        public bool HasCriticalErrors => CriticalStartupExceptions.Count > 0;
        public bool HasWarnings => StartupExceptions.Count > 0;
    }
}