using CameraSettingsSaver.Resources;

namespace CameraSettingsWindowsService.Models
{
    public class ServiceSettings
    {
        public string ProfilePath { get; set; } = "web_conf.json";
        public int IntervalSeconds { get; set; } = 20;
        public string? LogFile { get; set; }
        public Language Language { get; set; } = Language.en;
        public bool IsConsoleMode { get; set; }
        public bool IsServiceMode { get; set; }
        public bool IsShowHelp { get; set; }
    }
}