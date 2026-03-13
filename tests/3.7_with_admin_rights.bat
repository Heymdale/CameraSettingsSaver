cd %~dp0..
move /Y ./web_conf.json c://web_conf.json
cd bin
sc create CameraSettingsService binPath= "%CD%\CameraSettingsService.exe -p c://web_conf.json /lang ru --log c://test.log" start= auto
sc start CameraSettingsService
echo.
echo press key to return web_conf.json and delete test.log
pause
move /Y c://web_conf.json ../web_conf.json 
del "c://test.log"
pause