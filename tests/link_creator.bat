@echo off
setlocal enabledelayedexpansion

rem === КОНФИГУРАЦИЯ ===
set "batDir=%~dp0"
set "exePath=%batDir%..\WebCamSettings.exe"
set "workDir=%batDir%.."

rem === СОЗДАНИЕ ЯРЛЫКОВ ===
call :CreateShortcut "1-1.11,3.1-3.2,3.4.lnk"
call :CreateShortcut "2.2.8.lnk" "-c /p web_conf1.json"
call :CreateShortcut "2.3.1.lnk" "/no-interface /language ru"
call :CreateShortcut "2.3.2.lnk" "/ni /lang ru /log ni_test.log /p web_conf1.json"
call :CreateShortcut "2.3.4.lnk" "-c /ni"
call :CreateShortcut "3.3_with_admin_rights.lnk" "--log C://test.log"
echo.
echo ✓ Все ярлыки успешно созданы в папке: %batDir%
pause
exit /b

rem ========================================================
rem Подпрограмма: Создание ярлыка
rem Аргументы:
rem   %1 - Имя ярлыка (с расширением .lnk)
rem   %2 - Параметры командной строки
rem ========================================================
:CreateShortcut
set "lnkName=%~1"
set "lnkArgs=%~2"
set "lnkPath=%batDir%%lnkName%"

rem Создаём временный VBS-скрипт
set "vbs=%temp%\create_lnk_%RANDOM%.vbs"

>"%vbs%" echo Set oWS = WScript.CreateObject("WScript.Shell")
>>"%vbs%" echo Set oLink = oWS.CreateShortcut("%lnkPath%")
>>"%vbs%" echo oLink.TargetPath = "%exePath%"
>>"%vbs%" echo oLink.Arguments = "%lnkArgs%"
>>"%vbs%" echo oLink.WorkingDirectory = "%workDir%"
>>"%vbs%" echo oLink.IconLocation = "%exePath%,0"
>>"%vbs%" echo oLink.Save

cscript //nologo "%vbs%" >nul 2>&1
del "%vbs%"

echo   • Создан: %lnkName%
if defined lnkDesc echo       Описание: %lnkDesc%
echo       Параметры: %lnkArgs%
exit /b