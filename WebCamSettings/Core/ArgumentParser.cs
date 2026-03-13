using CameraSettingsSaver.Resources;
using CameraSettingsSaver.Utils;

namespace WebCamSettings.Core
{
    public class ArgumentParser
    {
        private readonly StartupMode _startupMode;

        public ArgumentParser(StartupMode startupMode)
        {
            _startupMode = startupMode;
        }

        public void Parse(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i].ToLower();

                switch (arg)
                {
                    case "-c":
                    case "--console":
                    case "/c":
                    case "/console":
                        _startupMode.IsConsole = true;
                        break;

                    case "--ni":
                    case "--no-interface":
                    case "/ni":
                    case "/no-interface":
                        _startupMode.IsNoInterface = true;
                        break;

                    case "-p":
                    case "--profile":
                    case "/p":
                    case "/profile":
                        if (i + 1 < args.Length)
                        {
                            _startupMode.Profile = args[i + 1];
                            i++;
                        }
                        else
                        {
                            _startupMode.CriticalStartupExceptions.Add("Error: No config file specified");
                        }
                        break;

                    case "-l":
                    case "--log":
                    case "/l":
                    case "/log":
                        if (i + 1 < args.Length && (!args[i + 1].StartsWith('/') && !args[i + 1].StartsWith('-')))
                        {
                            _startupMode.LogFile = args[i + 1];
                            i++;
                        }
                        else
                        {
                            _startupMode.LogFile = $"camera_settings_{DateTime.Now:yyyyMMdd_HHmmss}.log";
                        }
                        break;

                    case "--language":
                    case "--lang":
                    case "/language":
                    case "/lang":
                        if (i + 1 < args.Length)
                        {
                            HandleLanguageArgument(args[i + 1]);
                            i++;
                        }
                        else
                        {
                            _startupMode.StartupExceptions.Add("WARNING: No language specified. Using English.");
                            _startupMode.Language = Language.en;
                        }
                        break;

                    case "-?":
                    case "-h":
                    case "--help":
                    case "/?":
                    case "/h":
                    case "/help":
                        _startupMode.IsShowHelp = true;
                        break;

                    default:
                        _startupMode.CriticalStartupExceptions.Add($"Error: Unknown parameter '{args[i]}'");
                        break;
                }
            }
        }

        private void HandleLanguageArgument(string langArg)
        {
            string lang = langArg.ToLower();
            if (Enum.TryParse<Language>(lang, true, out Language language))
            {
                _startupMode.Language = language;
            }
            else
            {
                _startupMode.StartupExceptions.Add($"WARNING: Unsupported language '{langArg}'. Using English.");
                _startupMode.Language = Language.en;
            }
        }
    }
}