using CameraSettingsSaver.Models;
using CameraSettingsSaver.Resources;
using CameraSettingsSaver.Utils;
using DirectShowLib;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Collections.Generic;
using System.IO;
using System;

namespace CameraSettingsSaver.Core
{
    public class SettingsApplier
    {
        public bool ApplySettingsWithConsoleOutput(
            StartupMode startupMode,
            Localization localization,
            BasicLogger logger)
        {
            return ApplySettingsCommon(startupMode, localization, logger, true);
        }

        public bool ApplySettingsInBackground(
            StartupMode startupMode,
            Localization localization,
            BasicLogger logger)
        {
            return ApplySettingsCommon(startupMode, localization, logger, false);
        }

        private bool ApplySettingsCommon(
            StartupMode startupMode,
            Localization localization,
            BasicLogger logger,
            bool showConsoleOutput)
        {
            if (!File.Exists(startupMode.Profile))
            {
                string errorMessage = string.Format(localization.GetString("Messages.FileNotFound"), startupMode.Profile);
                LogOrShowError(logger, showConsoleOutput, errorMessage);
                return false;
            }

            logger.Log(localization.GetString("Console.LoadingConfig"));

            string json = File.ReadAllText(startupMode.Profile);
            var settingsList = JsonSerializer.Deserialize<CameraSettings[]>(json);

            if (settingsList == null || settingsList.Length == 0)
            {
                string errorMessage = localization.GetString("Console.NoValidSettings");
                LogOrShowError(logger, showConsoleOutput, errorMessage);
                return false;
            }

            logger.Log(string.Format(localization.GetString("Console.FoundSettings"), settingsList.Length));
            logger.Log(localization.GetString("Console.ScanningCameras"));

            DsDevice[] videoInputDevices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            logger.Log(string.Format(localization.GetString("Console.FoundConnectedCameras"), videoInputDevices.Length));

            logger.Log(localization.GetString("Console.ApplyingSettings"));

            int appliedCount = 0;
            int failedCount = 0;

            foreach (var cameraSettings in settingsList)
            {
                var result = ApplyCameraSettings(cameraSettings, videoInputDevices,
                    localization, logger, showConsoleOutput);

                if (result) appliedCount++;
                else failedCount++;
            }

            LogApplicationSummary(logger, settingsList.Length, appliedCount, failedCount, localization);
            return appliedCount > 0;
        }

        private void LogOrShowError(BasicLogger logger, bool showConsoleOutput, string message)
        {
            // BasicLogger doesn't have ShowError, we always log it as an error
            logger.LogError(message);
        }

        private bool ApplyCameraSettings(
            CameraSettings settings,
            DsDevice[] devices,
            Localization localization,
            BasicLogger logger,
            bool showConsoleOutput)
        {
            logger.Log(string.Format(localization.GetString("Console.ProcessingCamera"), settings.CameraName));

            DsDevice targetDevice = FindDeviceByMonikerOrName(devices, settings.MonikerString, settings.CameraName);

            if (targetDevice == null)
            {
                logger.Log(localization.GetString("Console.CameraNotFound"));
                return false;
            }

            logger.Log(string.Format(localization.GetString("Console.CameraFound"), targetDevice.Name));

            int videoSettings = settings.VideoProcAmpSettings?.Count ?? 0;
            int cameraSettingsCount = settings.CameraControlSettings?.Count ?? 0;

            logger.Log(string.Format(localization.GetString("Console.SettingsToApply"), videoSettings, cameraSettingsCount));

            bool success = ApplySettingsToDevice(targetDevice, settings, localization, logger, showConsoleOutput);

            if (success)
                logger.Log(localization.GetString("Console.AllSettingsApplied"));
            else
                logger.Log(localization.GetString("Console.SomeSettingsFailed"));

            return success;
        }

        private void LogApplicationSummary(BasicLogger logger, int totalSettings, int appliedCount, int failedCount, Localization localization)
        {
            logger.Log(localization.GetString("Console.Summary"));
            logger.Log(string.Format(localization.GetString("Console.TotalInConfig"), totalSettings));
            logger.Log(string.Format(localization.GetString("Console.SuccessfullyApplied"), appliedCount));
            logger.Log(string.Format(localization.GetString("Console.Failed"), failedCount));

            if (appliedCount > 0)
            {
                logger.Log(string.Format(localization.GetString("Console.OperationCompleted"), appliedCount));
            }
            else
            {
                logger.Log(localization.GetString("Console.NoSettingsApplied"));
            }
        }

        private DsDevice FindDeviceByMonikerOrName(DsDevice[] devices, string monikerString, string cameraName)
        {
            foreach (DsDevice device in devices)
            {
                string deviceMoniker = GetMonikerString(device);
                if (deviceMoniker == monikerString || device.Name == cameraName)
                {
                    return device;
                }
            }
            return null;
        }

        private string GetMonikerString(DsDevice device)
        {
            if (device?.Mon == null) return null;

            string displayName;
            device.Mon.GetDisplayName(null, null, out displayName);
            return displayName;
        }

        private bool ApplySettingsToDevice(
            DsDevice device,
            CameraSettings settings,
            Localization localization,
            BasicLogger logger,
            bool showConsoleOutput)
        {
            IBaseFilter filter = null;
            IFilterGraph2 filterGraph = null;

            try
            {
                filterGraph = new FilterGraph() as IFilterGraph2;

                Guid filterGuid = typeof(IBaseFilter).GUID;
                object filterObj;
                device.Mon.BindToObject(null, null, ref filterGuid, out filterObj);
                filter = filterObj as IBaseFilter;

                if (filter == null)
                {
                    logger.LogError(localization.GetString("Messages.FailedCreateFilter"));
                    return false;
                }

                filterGraph.AddFilter(filter, device.Name);

                IAMVideoProcAmp videoProcAmp = filter as IAMVideoProcAmp;
                IAMCameraControl cameraControl = filter as IAMCameraControl;

                int appliedSettings = 0;
                int failedSettings = 0;

                if (settings.VideoProcAmpSettings != null)
                {
                    ApplyVideoProcAmpSettings(videoProcAmp, settings.VideoProcAmpSettings,
                        localization, logger, showConsoleOutput, ref appliedSettings, ref failedSettings);
                }

                if (settings.CameraControlSettings != null)
                {
                    ApplyCameraControlSettings(cameraControl, settings.CameraControlSettings,
                        localization, logger, showConsoleOutput, ref appliedSettings, ref failedSettings);
                }

                if (showConsoleOutput)
                {
                    logger.Log(string.Format(localization.GetString("Console.ResultSummary"), appliedSettings, failedSettings));
                }

                return failedSettings == 0;
            }
            catch (Exception ex)
            {
                logger.LogError(string.Format(localization.GetString("Console.ErrorApplyingSettings"), ex.Message));
                return false;
            }
            finally
            {
                if (filter != null) Marshal.ReleaseComObject(filter);
                if (filterGraph != null) Marshal.ReleaseComObject(filterGraph);
            }
        }

        private void ApplyVideoProcAmpSettings(
            IAMVideoProcAmp videoProcAmp,
            Dictionary<string, VideoProcAmpSetting> settings,
            Localization localization,
            BasicLogger logger,
            bool showConsoleOutput,
            ref int appliedSettings,
            ref int failedSettings)
        {
            if (videoProcAmp != null)
            {
                if (showConsoleOutput)
                    logger.Log(localization.GetString("Console.ApplyingVideoProcAmp"));

                foreach (var kvp in settings)
                {
                    ApplyVideoProcAmpProperty(videoProcAmp, kvp.Key, kvp.Value,
                        localization, logger, showConsoleOutput, ref appliedSettings, ref failedSettings);
                }
            }
            else if (settings.Count > 0)
            {
                if (showConsoleOutput)
                    logger.Log(string.Format(localization.GetString("Console.WarningInterfaceNotSupported"), "VideoProcAmp"));
                failedSettings += settings.Count;
            }
        }

        private void ApplyVideoProcAmpProperty(
            IAMVideoProcAmp videoProcAmp,
            string propertyName,
            VideoProcAmpSetting setting,
            Localization localization,
            BasicLogger logger,
            bool showConsoleOutput,
            ref int appliedSettings,
            ref int failedSettings)
        {
            VideoProcAmpProperty property = (VideoProcAmpProperty)Enum.Parse(typeof(VideoProcAmpProperty), propertyName);
            int hr = videoProcAmp.Set(property, setting.Value, (VideoProcAmpFlags)setting.Flags);

            if (hr == 0)
            {
                if (showConsoleOutput)
                    logger.Log(string.Format(localization.GetString("Console.AppliedProperty"), property, setting.Value));
                appliedSettings++;
            }
            else
            {
                if (showConsoleOutput)
                    logger.LogError(string.Format(localization.GetString("Console.ErrorProperty"), property, hr));
                failedSettings++;
            }
        }

        private void ApplyCameraControlSettings(
            IAMCameraControl cameraControl,
            Dictionary<string, CameraControlSetting> settings,
            Localization localization,
            BasicLogger logger,
            bool showConsoleOutput,
            ref int appliedSettings,
            ref int failedSettings)
        {
            if (cameraControl != null)
            {
                if (showConsoleOutput)
                    logger.Log(localization.GetString("Console.ApplyingCameraControl"));

                foreach (var kvp in settings)
                {
                    ApplyCameraControlProperty(cameraControl, kvp.Key, kvp.Value,
                        localization, logger, showConsoleOutput, ref appliedSettings, ref failedSettings);
                }
            }
            else if (settings.Count > 0)
            {
                if (showConsoleOutput)
                    logger.Log(string.Format(localization.GetString("Console.WarningInterfaceNotSupported"), "CameraControl"));
                failedSettings += settings.Count;
            }
        }

        private void ApplyCameraControlProperty(
            IAMCameraControl cameraControl,
            string propertyName,
            CameraControlSetting setting,
            Localization localization,
            BasicLogger logger,
            bool showConsoleOutput,
            ref int appliedSettings,
            ref int failedSettings)
        {
            CameraControlProperty property = (CameraControlProperty)Enum.Parse(typeof(CameraControlProperty), propertyName);
            int hr = cameraControl.Set(property, setting.Value, (CameraControlFlags)setting.Flags);

            if (hr == 0)
            {
                if (showConsoleOutput)
                    logger.Log(string.Format(localization.GetString("Console.AppliedProperty"), property, setting.Value));
                appliedSettings++;
            }
            else
            {
                if (showConsoleOutput)
                    logger.LogError(string.Format(localization.GetString("Console.ErrorProperty"), property, hr));
                failedSettings++;
            }
        }
    }
}