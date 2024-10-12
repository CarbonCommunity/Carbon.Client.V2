@echo off

set ROOT=%cd%
set TMP=%ROOT%\tmp
set BIEX=https://github.com/BepInEx/BepInEx/releases/download/v6.0.0-pre.2/BepInEx-Unity.IL2CPP-win-x64-6.0.0-pre.2.zip

echo Downloading BepInEx from %BIEX%...
powershell -Command "(New-Object Net.WebClient).DownloadFile('%BIEX%', '%TMP%\bepinex.zip')"

echo Extracting BepInEx..
powershell -Command "Expand-Archive '%TMP%\bepinex.zip' -DestinationPath '%ROOT%\ext'" -Force