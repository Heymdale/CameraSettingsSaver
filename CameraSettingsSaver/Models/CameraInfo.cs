using System;

namespace CameraSettingsSaver.Models
{
    public class CameraInfo
    {
        public string Name { get; set; }
        public string DevicePath { get; set; }
        public string MonikerString { get; set; }
        public bool IsSelected { get; set; }
    }
}