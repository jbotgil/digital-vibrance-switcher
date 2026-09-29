<#
.SYNOPSIS
    Switch / consulta el Digital Vibrance de las GPUs NVIDIA mediante NVAPI.

.DESCRIPTION
    Usa NvAPI_PhysicalGPU_SetDigitalVibrance (nvapi64.dll del driver) para cambiar
    el Digital Vibrance de forma inmediata, sin abrir el Panel de Control.
    Lee saturación y hue actuales y los reenvía para no alterarlos.

.PARAMETER Value
    Porcentaje de Digital Vibrance (0-100). Por defecto 50.

.PARAMETER List
    Muestra el valor actual de todas las GPUs NVIDIA y no cambia nada.

.PARAMETER GPU
    Índice de la GPU a modificar (0 = primera enumerada). Por defecto 0.

.PARAMETER Min / Max
    Atajos: -Min aplica 50, -Max aplica 80 (fáciles de asignar a hotkeys).

.PARAMETER AtStartup
    Crea una tarea programada que aplica el valor al iniciar sesión.

.PARAMETER RemoveStartup
    Elimina la tarea programada creada por -AtStartup.

.EJEMPLOS
    .\DigitalVibrance.ps1
    .\DigitalVibrance.ps1 -Value 65
    .\DigitalVibrance.ps1 -Min
    .\DigitalVibrance.ps1 -Max
    .\DigitalVibrance.ps1 -List
    .\DigitalVibrance.ps1 -Value 72 -AtStartup
#>
[CmdletBinding()]
param(
    [int]$Value = 50,
    [switch]$List,
    [int]$GPU = 0,
    [switch]$Min,
    [switch]$Max,
    [switch]$AtStartup,
    [switch]$RemoveStartup
)

$ErrorActionPreference = 'Stop'
$TaskName = 'DigitalVibrance'

# ---------------------------------------------------------------- NVAPI P/Invoke
if (-not ('Nvapi' -as [type])) {
    Add-Type -Namespace 'DvNative' -Name 'Nvapi' -MemberDefinition @'
[DllImport("nvapi64.dll", CallingConvention = CallingConvention.Cdecl)]
public static extern int NvAPI_Initialize();

[DllImport("nvapi64.dll", CallingConvention = CallingConvention.Cdecl)]
public static extern int NvAPI_Unload();

[DllImport("nvapi64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nvapi_gpu_count")]
public static extern int NvAPI_EnumPhysicalGpus_count();

[DllImport("nvapi64.dll", CallingConvention = CallingConvention.Cdecl)]
public static extern int NvAPI_EnumPhysicalGPUs([In, Out] System.IntPtr[] handles);

[DllImport("nvapi64.dll", CallingConvention = CallingConvention.Cdecl)]
public static extern int NvAPI_PhysicalGPU_GetDigitalVibrance(
    System.IntPtr hGpu,
    out int brightness,
    out int saturation,
    out int hue);

[DllImport("nvapi64.dll", CallingConvention = CallingConvention.Cdecl)]
public static extern int NvAPI_PhysicalGPU_SetDigitalVibrance(
    System.IntPtr hGpu,
    int brightness,
    int saturation,
    int hue);

[DllImport("nvapi64.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
public static extern int NvAPI_PhysicalGPU_GetAdapterName(
    System.IntPtr hGpu,
    [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPStr)]
    out string name);

[DllImport("nvapi64.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
public static extern int NvAPI_GetErrorString(int rc,
    [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPStr)]
    out string msg);
'@
}

function Get-NvapiMessage {
    param([int]$Code)
    $msg = $null
    [void][DvNative.Nvapi]::NvAPI_GetErrorString($Code, [ref]$msg)
    if ($msg) { return $msg } else { return "NVAPI error 0x$('{0:X8}' -f $Code)" }
}

function Assert-Nvapi {
    param([int]$Code, [string]$What)
    if ($Code -ne 0) { throw "$What -> $(Get-NvapiMessage $Code)" }
}

function Get-NvidiaGpu {
    $rc = [DvNative.Nvapi]::NvAPI_Initialize()
    Assert-Nvapi $rc 'NvAPI_Initialize'
    $count = [DvNative.Nvapi]::NvAPI_EnumPhysicalGpus_count()
    if ($count -le 0) { throw 'No se encontraron GPUs NVIDIA.' }
    $handles = New-Object 'System.IntPtr[]' $count
    $rc = [DvNative.Nvapi]::NvAPI_EnumPhysicalGPUs($handles)
    Assert-Nvapi $rc 'NvAPI_EnumPhysicalGPUs'
    $list = @()
    for ($i = 0; $i -lt $handles.Length; $i++) {
        $name = $null
        [void][DvNative.Nvapi]::NvAPI_PhysicalGPU_GetAdapterName($handles[$i], [ref]$name)
        $list += [pscustomobject]@{ Index = $i; Handle = $handles[$i]; Name = $name }
    }
    return $list
}

function Get-DigitalVibrance {
    param([IntPtr]$Handle)
    $b = 0; $s = 0; $h = 0
    $rc = [DvNative.Nvapi]::NvAPI_PhysicalGPU_GetDigitalVibrance($Handle, [ref]$b, [ref]$s, [ref]$h)
    Assert-Nvapi $rc 'GetDigitalVibrance'
    [pscustomobject]@{ DigitalVibrance = $b; Saturation = $s; Hue = $h }
}

function Set-DigitalVibrance {
    param([IntPtr]$Handle, [int]$Brightness)
    $cur = Get-DigitalVibrance -Handle $Handle
    $rc = [DvNative.Nvapi]::NvAPI_PhysicalGPU_SetDigitalVibrance(
        $Handle, $Brightness, $cur.Saturation, $cur.Hue)
    Assert-Nvapi $rc 'SetDigitalVibrance'
    return (Get-DigitalVibrance -Handle $Handle)
}

# ---------------------------------------------------------------- helpers
function Get-TaskCommand {
    $psExe = Join-Path $PSHOME 'powershell.exe'
    if (-not (Test-Path $psExe)) { $psExe = 'powershell.exe' }
    $script = Join-Path $PSScriptRoot 'DigitalVibrance.ps1'
    return "`"$psExe`" -NoProfile -WindowStyle Hidden -ExecutionPolicy Bypass -File `"$script`" -Value"
}

# ---------------------------------------------------------------- main
if ($RemoveStartup) {
    Unregister-ScheduledTask -TaskName $TaskName -Confirm:$false -ErrorAction SilentlyContinue
    Write-Host "Tarea '$TaskName' eliminada." -ForegroundColor Yellow
    return
}

if ($List) {
    $gpus = Get-NvidiaGpu
    Write-Host ''
    Write-Host '  GPU                        DigitalVibrance   Saturation   Hue' -ForegroundColor Cyan
    Write-Host '  ' + ('-' * 62)
    foreach ($g in $gpus) {
        $dv = Get-DigitalVibrance -Handle $g.Handle
        Write-Host ('  {0,-26} {1,10} {2,14} {3,7}' -f $g.Name, $dv.DigitalVibrance, $dv.Saturation, $dv.Hue)
    }
    Write-Host ''
    return
}

if ($Min) { $Value = 50 }
if ($Max) { $Value = 80 }

if ($Value -lt 0 -or $Value -gt 100) { throw "Valor fuera de rango: $Value (use 0-100)." }

$gpus = Get-NvidiaGpu
if ($GPU -lt 0 -or $GPU -ge $gpus.Count) { throw "Índice de GPU inválido: $GPU (0..$($gpus.Count - 1))." }
$g = $gpus[$GPU]

$before = Get-DigitalVibrance -Handle $g.Handle
$after = Set-DigitalVibrance -Handle $g.Handle -Brightness $Value

Write-Host ("{0}  DigitalVibrance: {1} -> {2}   [saturación {3}, hue {4}]" -f `
    $g.Name.Trim(), $before.DigitalVibrance, $after.DigitalVibrance, $after.Saturation, $after.Hue) -ForegroundColor Green

if ($AtStartup) {
    $arg = "$(Get-TaskCommand) $Value"
    $action = New-ScheduledTaskAction -Execute 'powershell.exe' -Argument $arg
    $trigger = New-ScheduledTaskTrigger -AtLogOn
    $principal = New-ScheduledTaskPrincipal -UserId $env:USERNAME -LogonType Interactive -RunLevel Limited
    Register-ScheduledTask -TaskName $TaskName -Action $action -Trigger $trigger -Principal $principal `
        -Force -Description "Aplica Digital Vibrance $Value% al iniciar sesion" | Out-Null
    Write-Host "Tarea programada creada: DigitalVibrance = $Value% al iniciar sesion." -ForegroundColor Cyan
}
