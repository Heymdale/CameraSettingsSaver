using CameraSettingsSaver.Resources;
using CameraSettingsWindowsService.Models;

namespace CameraSettingsWindowsService.Core
{
    public class ArgumentParser
    {
        private readonly Localization _localization;
        private readonly string _baseDirectory;
        private readonly string _rootDirectory;

        public List<string> Warnings { get; } = new List<string>();
        public List<string> Errors { get; } = new List<string>();

        public ArgumentParser(Localization localization)
        {
            _localization = localization;
            _baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _rootDirectory = Path.GetFullPath(Path.Combine(_baseDirectory, ".."));
        }

        public ServiceSettings Parse(string[] args, bool isServiceMode)
        {
            // Create default settings
            var settings = new ServiceSettings();
            settings.IsServiceMode = isServiceMode;

            string? rawProfilePath = null;
            string? rawLogPath = null;

            // Check console mode (flag or non-service mode)
            settings.IsConsoleMode = args.Contains("--console") || args.Contains("/console") || !isServiceMode;

            // If this is service mode and there are no arguments, we simply return the default settings.
            if (isServiceMode && args.Length == 0)
            {
                settings.ProfilePath = ResolvePath(settings.ProfilePath);
                return settings;
            }

            // If this is console mode and there are no arguments, we show help
            if (settings.IsConsoleMode && args.Length == 0)
            {
                settings.IsShowHelp = true;
                settings.ProfilePath = ResolvePath(settings.ProfilePath);
                return settings;
            }

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];

                switch (arg)
                {
                    case "-p":
                    case "--profile":
                    case "/p":
                    case "/profile":
                        if (i + 1 < args.Length && !args[i + 1].StartsWith("/") &&
                            !args[i + 1].StartsWith("-") && !args[i + 1].StartsWith("--"))
                        {
                            rawProfilePath = args[++i];
                        }
                        else
                        {
                            Errors.Add(_localization.GetString("Messages.ErrorNoConfigFile"));
                        }
                        break;

                    case "-l":
                    case "--log":
                    case "/l":
                    case "/log":
                        if (i + 1 < args.Length && !args[i + 1].StartsWith("/") &&
                            !args[i + 1].StartsWith("-") && !args[i + 1].StartsWith("--"))
                        {
                            rawLogPath = args[++i];
                        }
                        else
                        {
                            // If /l is specified without a file name, use the default name
                            rawLogPath = "service.log";
                        }
                        break;

                    case "-i":
                    case "--interval":
                    case "/i":
                    case "/interval":
                        if (i + 1 < args.Length && int.TryParse(args[i + 1], out int interval))
                        {
                            if (interval > 0)
                            {
                                settings.IntervalSeconds = interval;
                                i++;
                            }
                            else
                            {
                                Warnings.Add(_localization.GetString("Service.Warnings.IntervalMustBePositive"));
                            }
                        }
                        else
                        {
                            Warnings.Add(_localization.GetString("Service.Warnings.InvalidInterval"));
                        }
                        break;

                    case "-lang":
                    case "--language":
                    case "/lang":
                    case "/language":
                        if (i + 1 < args.Length)
                        {
                            HandleLanguageArgument(args[i + 1], settings);
                            i++;
                        }
                        else
                        {
                            Warnings.Add("WARNING: No language specified. Using English.");
                            settings.Language = Language.en;
                        }
                        break;

                    case "--console":
                    case "/console":
                        // Just skip it, the flag is already set
                        break;

                    case "-?":
                    case "-h":
                    case "--help":
                    case "/?":
                    case "/h":
                    case "/help":
                        settings.IsShowHelp = true;
                        break;

                    // Unknown argument
                    default:
                        if (arg.StartsWith("/") || arg.StartsWith("-"))
                        {
                            Errors.Add(string.Format(_localization.GetString("Messages.WarningUnknownParameter"), arg));
                        }
                        break;
                }
            }

            // If help is requested, return the settings without further processing
            if (settings.IsShowHelp)
            {
                settings.ProfilePath = ResolvePath(settings.ProfilePath);
                return settings;
            }

            // Resolve paths after parsing, if they were specified
            if (rawProfilePath != null)
            {
                settings.ProfilePath = ResolvePath(rawProfilePath);
            }
            else
            {
                // If the profile is not specified, we use the default path
                settings.ProfilePath = ResolvePath(settings.ProfilePath);
            }

            if (rawLogPath != null)
            {
                settings.LogFile = ResolvePath(rawLogPath);
            }

            return settings;
        }

        private void HandleLanguageArgument(string langArg, ServiceSettings settings)
        {
            string lang = langArg.ToLower();

            // We try to parse it as an Enum
            if (Enum.TryParse<Language>(lang, true, out Language language))
            {
                settings.Language = language;
            }
            else
            {
                Warnings.Add(string.Format(_localization.GetString("Service.Warnings.UnknownLanguage"), langArg));
                settings.Language = Language.en;
            }
        }

        private string ResolvePath(string path)
        {
            // Absolute path - return as is
            if (Path.IsPathRooted(path))
            {
                return path;
            }

            // Relative path or file name - resolve relative to the root folder (one level above the exe)
            return Path.Combine(_rootDirectory, path);
        }

        public void ShowHelp(ServiceSettings settings)
        {
            var help = new Help();
            string helpText = help.GetServiceHelp(_localization);
            Console.WriteLine(helpText);
        }

        public void ShowWarningsAndErrors()
        {
            foreach (var warning in Warnings)
            {
                Console.WriteLine($"[WARNING] {warning}");
            }
            foreach (var error in Errors)
            {
                Console.WriteLine($"[ERROR] {error}");
            }
        }
    }
}