using System;
using System.Collections.Generic;

namespace CameraSettingsSaver.Models
{
    public class CameraSettings
    {
        public string CameraName { get; set; }
        public string MonikerString { get; set; }
        public DateTime SaveTime { get; set; }
        public Dictionary<string, VideoProcAmpSetting> VideoProcAmpSettings { get; set; }
        public Dictionary<string, CameraControlSetting> CameraControlSettings { get; set; }
    }

    public class VideoProcAmpSetting
    {
        public int Value { get; set; }
        public int Flags { get; set; }
    }

    public class CameraControlSetting
    {
        public int Value { get; set; }
        public int Flags { get; set; }
    }
}