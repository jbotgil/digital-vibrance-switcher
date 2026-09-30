@echo off
echo Building Digital Vibrance Switcher...
echo.

set CSC="C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
set REFS=-reference:System.dll -reference:System.Core.dll -reference:System.Drawing.dll -reference:System.Windows.Forms.dll -reference:System.Runtime.Serialization.dll -reference:System.Xml.dll

%CSC% -nologo -platform:x64 -target:winexe -out:DigitalVibrance.exe -win32manifest:app.manifest %REFS% ^
    Program.cs ^
    Native\NvApi.cs ^
    Native\DdcCi.cs ^
    Core\VibranceController.cs ^
    Core\SettingsManager.cs ^
    Core\ProfileManager.cs ^
    Core\GameDetector.cs ^
    Core\InstalledAppScanner.cs ^
    Core\HotkeyManager.cs ^
    UI\Theme.cs ^
    UI\ToggleSwitch.cs ^
    UI\SliderBar.cs ^
    UI\AppPickerDialog.cs ^
    UI\LoadingDialog.cs ^
    UI\MainForm.cs ^
    UI\TrayManager.cs ^
    Properties\AssemblyInfo.cs

if %ERRORLEVEL% EQU 0 (
    echo.
    echo BUILD SUCCESS
) else (
    echo.
    echo BUILD FAILED
)

pause