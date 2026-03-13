using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace CameraSettingsSaver.Resources
{
    public class LocalizationErrorLoadMainLanguage : Exception
    {
        public LocalizationErrorLoadMainLanguage()
        {
        }

        public LocalizationErrorLoadMainLanguage(string message)
            : base(message)
        {
        }
    }

    public enum Language
    {
        en,
        ru
    }

    public class Localization
    {
        private Dictionary<Language, Dictionary<string, string>> _resources;
        private Language _currentLanguage = Language.en;
        private List<string> _warnings = new List<string>();
        public string lineSeparator = "=".PadRight(50, '=');

        public List<string> Warnings { get { return _warnings; } }

        public Localization()
        {
            _resources = new();
            LoadLanguage(Language.en);
            foreach(Language lang in Enum.GetValues(typeof(Language)))
            {
                if (lang != Language.en) { LoadLanguage(lang); }
            }
        }

        public void SetLanguage(Language language)
        {
                _currentLanguage = language;
        }

        public Language CurrentLanguage => _currentLanguage;

        public bool IsEnglish => _currentLanguage == Language.en;

        private void LoadLanguage(Language language)
        {
            try
            {
                // 1. Get the path to the folder where the CameraSettingsSaver.dll file is physically located
                string dllPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                // 2. Create a path to resources located one level higher (in the configuration root)
                // This will work for both the EXE in the root and the service in the bin,
                // since the DLL always "knows" where it is.
                string rootPath = Path.GetFullPath(Path.Combine(dllPath, ".."));

                string filePath = Path.Combine(rootPath, "Resources", "locales", $"{language}.json");



                string json = File.ReadAllText(filePath);
                var resourceDict = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(json);

                // Flatten the nested dictionary
                var flattenedDict = new Dictionary<string, string>();
                foreach (var category in resourceDict)
                {
                    foreach (var item in category.Value)
                    {
                        flattenedDict[$"{category.Key}.{item.Key}"] = item.Value;
                    }
                }

                _resources[language] = flattenedDict;
            }
            catch (Exception ex)
            {
                if (language != Language.en)
                {
                    _resources[language] = _resources[Language.en];
                    _warnings.Add($"Error loading language {language}, will be used {Language.en}.\n" +
                        $"Error message: {ex.Message}");
                }
                else throw new LocalizationErrorLoadMainLanguage($"Error loading default language 'en': {ex.Message}");
            }
        }

        public string GetString(string key, params object[] args)
        {
            if (_resources.ContainsKey(_currentLanguage) &&
                _resources[_currentLanguage].ContainsKey(key))
            {
                string value = _resources[_currentLanguage][key];

                if (args != null && args.Length > 0)
                {
                    try
                    {
                        return string.Format(value, args);
                    }
                    catch (Exception)
                    {
                        return value;
                    }
                }

                return value;
            }

            // Fallback to English if key not found
            if (_currentLanguage != Language.en &&
                _resources.ContainsKey(Language.en) &&
                _resources[Language.en].ContainsKey(key))
            {
                string value = _resources[Language.en][key];

                if (args != null && args.Length > 0)
                {
                    try
                    {
                        return string.Format(value, args);
                    }
                    catch (Exception)
                    {
                        return value;
                    }
                }

                return value;
            }

            return $"[{key}]";
        }
    }
}