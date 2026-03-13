cd %~dp0..\bin\
sc create CameraSettingsService binPath= "%CD%\CameraSettingsService.exe" start= auto
sc start CameraSettingsService
pause