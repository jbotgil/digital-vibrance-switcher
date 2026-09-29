# Build script for Digital Vibrance Switcher
# Run from the project root directory

$Csc = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
$Refs = @(
    "-reference:System.dll",
    "-reference:System.Core.dll",
    "-reference:System.Drawing.dll",
    "-reference:System.Windows.Forms.dll",
    "-reference:System.Runtime.Serialization.dll",
    "-reference:System.Xml.dll"
)

$Sources = @(
    "Program.cs",
    "Native\NvApi.cs",
    "Native\DdcCi.cs",
    "Core\VibranceController.cs",
    "Core\SettingsManager.cs",
    "Core\ProfileManager.cs",
    "Core\GameDetector.cs",
    "Core\HotkeyManager.cs",
    "UI\ModernTheme.cs",
    "UI\ModernCheckBox.cs",
    "UI\ModernTrackBar.cs",
    "UI\MainForm.cs",
    "UI\TrayManager.cs",
    "Properties\AssemblyInfo.cs"
)

Write-Host "Building Digital Vibrance Switcher..." -ForegroundColor Cyan

$args = @(
    "-nologo",
    "-platform:x64",
    "-target:winexe",
    "-out:DigitalVibrance.exe",
    "-win32manifest:app.manifest"
) + $Refs + $Sources

& $Csc $args

if ($LASTEXITCODE -eq 0) {
    Write-Host "`nBUILD SUCCESS" -ForegroundColor Green
    Write-Host "Output: DigitalVibrance.exe (x64, .NET Framework)" -ForegroundColor Green
} else {
    Write-Host "`nBUILD FAILED" -ForegroundColor Red
}