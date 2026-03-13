using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using CameraSettingsSaver.Models;
using CameraSettingsSaver.Resources;
using DirectShowLib;

namespace CameraSettingsSaver.Utils
{
    public class CameraService
    {
        private readonly BasicLogger _logger;
        private readonly Localization _localization;

        public CameraService(BasicLogger logger, Localization localization)
        {
            _logger = logger;
            _localization = localization;
        }

        public List<CameraInfo> GetAvailableCameras()
        {
            var cameras = new List<CameraInfo>();

            try
            {
                var videoInputDevices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);

                foreach (DsDevice device in videoInputDevices)
                {
                    string monikerString = GetMonikerString(device);

                    if (string.IsNullOrEmpty(monikerString))
                    {
                        string errorMessage = string.Format(_localization.GetString("LogMessages.FailedToGetMoniker"), device.Name);
                        _logger.LogError(errorMessage);
                        continue;
                    }

                    cameras.Add(new CameraInfo
                    {
                        Name = device.Name,
                        DevicePath = device.DevicePath,
                        MonikerString = monikerString,
                        IsSelected = true
                    });
                }

                if (cameras.Count == 0)
                {
                    _logger.Log(_localization.GetString("Messages.NoCamerasFound"));
                }
                else
                {
                    string foundMessage = string.Format(_localization.GetString("LogMessages.FoundCamerasCount"), cameras.Count);
                    _logger.Log(foundMessage);
                }
            }
            catch (Exception ex)
            {
                string errorMessage = string.Format(_localization.GetString("Messages.ErrorGettingCameras"), ex.Message);
                _logger.LogError(errorMessage);
            }

            return cameras;
        }

        public CameraSettings GetCameraSettings(CameraInfo cameraInfo)
        {
            var settings = new CameraSettings
            {
                CameraName = cameraInfo.Name,
                MonikerString = cameraInfo.MonikerString,
                SaveTime = DateTime.Now,
                VideoProcAmpSettings = new Dictionary<string, VideoProcAmpSetting>(),
                CameraControlSettings = new Dictionary<string, CameraControlSetting>()
            };

            IBaseFilter filter = null;
            IFilterGraph2 filterGraph = null;

            try
            {
                var targetDevice = FindDeviceByMoniker(cameraInfo.MonikerString);
                if (targetDevice == null)
                {
                    string message = string.Format(_localization.GetString("CameraMessages.FailedToFindCamera"), cameraInfo.Name);
                    _logger.Log(message);
                    return null;
                }

                // Create a filter using Moniker
                filterGraph = new FilterGraph() as IFilterGraph2;
                int hr = filterGraph.AddSourceFilterForMoniker(
                    targetDevice.Mon,
                    null,
                    cameraInfo.Name,
                    out filter);

                if (hr < 0 || filter == null)
                {
                    string message = string.Format(_localization.GetString("CameraMessages.FailedToConnect"), cameraInfo.Name);
                    _logger.Log(message);
                    Marshal.ReleaseComObject(filterGraph);
                    return null;
                }

                // Get settings
                GetVideoProcAmpSettings(filter, settings);
                GetCameraControlSettings(filter, settings);

                string successMessage = string.Format(_localization.GetString("CameraMessages.SettingsRetrieved"),
                    cameraInfo.Name,
                    settings.VideoProcAmpSettings.Count,
                    settings.CameraControlSettings.Count);
                _logger.Log(successMessage);

                return settings;
            }
            catch (Exception ex)
            {
                string errorMessage = string.Format(_localization.GetString("CameraMessages.ErrorGettingSettings"), cameraInfo.Name, ex.Message);
                _logger.LogError(errorMessage);
                return null;
            }
            finally
            {
                ReleaseComObjects(filter, filterGraph);
            }
        }

        public OperationResult ApplyCameraSettings(CameraSettings settings)
        {
            IBaseFilter filter = null;
            IFilterGraph2 filterGraph = null;

            try
            {
                if (string.IsNullOrEmpty(settings.MonikerString))
                {
                    string message = string.Format(_localization.GetString("LogMessages.NoMonikerInSettings"), settings.CameraName);
                    _logger.Log(message);
                    return OperationResult.SettingsError;
                }

                var targetDevice = FindDeviceByMoniker(settings.MonikerString);
                if (targetDevice == null)
                {
                    string message = string.Format(_localization.GetString("LogMessages.FailedToFindCameraByMoniker"), settings.CameraName);
                    _logger.Log(message);
                    return OperationResult.CameraNotFound;
                }

                // Create a filter using Moniker
                filterGraph = new FilterGraph() as IFilterGraph2;
                int hr = filterGraph.AddSourceFilterForMoniker(
                    targetDevice.Mon,
                    null,
                    settings.CameraName,
                    out filter);

                if (hr < 0 || filter == null)
                {
                    string message = string.Format(_localization.GetString("CameraMessages.FailedToConnect"), settings.CameraName);
                    _logger.Log(message);
                    Marshal.ReleaseComObject(filterGraph);
                    return OperationResult.FailedToConnect;
                }

                // Apply settings
                bool videoSuccess = ApplyVideoProcAmpSettings(filter, settings);
                bool cameraSuccess = ApplyCameraControlSettings(filter, settings);

                return (videoSuccess && cameraSuccess) ?
                    OperationResult.Success : OperationResult.SettingsError;
            }
            catch (Exception ex)
            {
                string errorMessage = string.Format(_localization.GetString("CameraMessages.ErrorApplyingSettings"), settings.CameraName, ex.Message);
                _logger.LogError(errorMessage);
                return OperationResult.SettingsError;
            }
            finally
            {
                ReleaseComObjects(filter, filterGraph);
            }
        }

        public DsDevice FindDeviceByMoniker(string monikerString)
        {
            var devices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            return devices.FirstOrDefault(device => GetMonikerString(device) == monikerString);
        }

        public DsDevice FindDeviceForPreview(CameraInfo cameraInfo)
        {
            return FindDeviceByMoniker(cameraInfo.MonikerString);
        }

        public DsDevice FindDeviceForSettings(CameraInfo cameraInfo)
        {
            return FindDeviceByMoniker(cameraInfo.MonikerString);
        }

        private string GetMonikerString(DsDevice device)
        {
            try
            {
                if (device?.Mon == null) return null;

                string displayName;
                device.Mon.GetDisplayName(null, null, out displayName);
                return displayName;
            }
            catch (Exception ex)
            {
                string errorMessage = string.Format(_localization.GetString("LogMessages.MonikerError"), ex.Message);
                _logger.LogError(errorMessage);
                return null;
            }
        }

        private void GetVideoProcAmpSettings(IBaseFilter filter, CameraSettings settings)
        {
            IAMVideoProcAmp videoProcAmp = filter as IAMVideoProcAmp;
            if (videoProcAmp == null) return;

            foreach (VideoProcAmpProperty property in Enum.GetValues(typeof(VideoProcAmpProperty)))
            {
                try
                {
                    int hrGet = videoProcAmp.Get(property, out int value, out VideoProcAmpFlags flags);
                    if (hrGet == 0) // S_OK
                    {
                        settings.VideoProcAmpSettings[property.ToString()] = new VideoProcAmpSetting
                        {
                            Value = value,
                            Flags = (int)flags
                        };
                    }
                }
                catch
                {
                    // Skip unsupported properties
                }
            }
        }

        private void GetCameraControlSettings(IBaseFilter filter, CameraSettings settings)
        {
            IAMCameraControl cameraControl = filter as IAMCameraControl;
            if (cameraControl == null) return;

            foreach (CameraControlProperty property in Enum.GetValues(typeof(CameraControlProperty)))
            {
                try
                {
                    int hrGet = cameraControl.Get(property, out int value, out CameraControlFlags flags);
                    if (hrGet == 0) // S_OK
                    {
                        settings.CameraControlSettings[property.ToString()] = new CameraControlSetting
                        {
                            Value = value,
                            Flags = (int)flags
                        };
                    }
                }
                catch
                {
                    // Skip unsupported properties
                }
            }
        }

        private bool ApplyVideoProcAmpSettings(IBaseFilter filter, CameraSettings settings)
        {
            IAMVideoProcAmp videoProcAmp = filter as IAMVideoProcAmp;
            if (videoProcAmp == null || settings.VideoProcAmpSettings == null)
                return true;

            bool success = true;

            foreach (var kvp in settings.VideoProcAmpSettings)
            {
                try
                {
                    if (Enum.TryParse<VideoProcAmpProperty>(kvp.Key, out VideoProcAmpProperty property))
                    {
                        int hrSet = videoProcAmp.Set(property, kvp.Value.Value, (VideoProcAmpFlags)kvp.Value.Flags);
                        if (hrSet == 0)
                        {
                            string message = string.Format(_localization.GetString("PropertyMessages.AppliedVideoProcAmp"), property, kvp.Value.Value);
                            _logger.Log(message);
                        }
                        else
                        {
                            string message = string.Format(_localization.GetString("PropertyMessages.ErrorApplyingVideoProcAmp"), property, hrSet);
                            _logger.LogError(message);
                            success = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    string message = string.Format(_localization.GetString("PropertyMessages.ErrorApplyingVideoProcAmpEx"), kvp.Key, ex.Message);
                    _logger.LogError(message);
                    success = false;
                }
            }

            return success;
        }

        private bool ApplyCameraControlSettings(IBaseFilter filter, CameraSettings settings)
        {
            IAMCameraControl cameraControl = filter as IAMCameraControl;
            if (cameraControl == null || settings.CameraControlSettings == null)
                return true;

            bool success = true;

            foreach (var kvp in settings.CameraControlSettings)
            {
                try
                {
                    if (Enum.TryParse<CameraControlProperty>(kvp.Key, out CameraControlProperty property))
                    {
                        int hrSet = cameraControl.Set(property, kvp.Value.Value, (CameraControlFlags)kvp.Value.Flags);
                        if (hrSet == 0)
                        {
                            string message = string.Format(_localization.GetString("PropertyMessages.AppliedCameraControl"), property, kvp.Value.Value);
                            _logger.Log(message);
                        }
                        else
                        {
                            string message = string.Format(_localization.GetString("PropertyMessages.ErrorApplyingCameraControl"), property, hrSet);
                            _logger.LogError(message);
                            success = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    string message = string.Format(_localization.GetString("PropertyMessages.ErrorApplyingCameraControlEx"), kvp.Key, ex.Message);
                    _logger.LogError(message);
                    success = false;
                }
            }

            return success;
        }

        private void ReleaseComObjects(params object[] comObjects)
        {
            foreach (var obj in comObjects)
            {
                if (obj != null && Marshal.IsComObject(obj))
                {
                    Marshal.ReleaseComObject(obj);
                }
            }
        }
    }
}