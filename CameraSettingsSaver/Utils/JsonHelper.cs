using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using CameraSettingsSaver.Models;

namespace CameraSettingsSaver.Utils
{
    public static class JsonHelper
    {
        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public static string SerializeSettings(List<CameraSettings> settings)
        {
            return JsonSerializer.Serialize(settings, _options);
        }

        public static List<CameraSettings> DeserializeSettings(string json)
        {
            return JsonSerializer.Deserialize<List<CameraSettings>>(json, _options);
        }

        public static void SaveToFile(string filePath, List<CameraSettings> settings)
        {
            string json = SerializeSettings(settings);
            File.WriteAllText(filePath, json);
        }

        public static List<CameraSettings> LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            string json = File.ReadAllText(filePath);
            return DeserializeSettings(json);
        }

        public static bool ValidateSettings(List<CameraSettings> settings)
        {
            if (settings == null) return false;

            foreach (var setting in settings)
            {
                if (string.IsNullOrEmpty(setting.MonikerString))
                {
                    return false;
                }
            }

            return true;
        }
    }
}