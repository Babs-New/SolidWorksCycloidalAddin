param(
    [Parameter(Mandatory = $true)]
    [string]$DllPath,

    [string]$RegAsmPath = "",

    [switch]$Unregister
)

$ErrorActionPreference = "Stop"

function Resolve-RegAsmPath {
    param([string]$Preferred)

    if ($Preferred -and (Test-Path $Preferred)) {
        return (Resolve-Path $Preferred).Path
    }

    $candidates = @(
        "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe",
        "$env:WINDIR\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe"
    )

    foreach ($c in $candidates) {
        if (Test-Path $c) { return $c }
    }

    throw "RegAsm.exe not found. Install .NET Framework Developer Pack 4.8 or provide -RegAsmPath."
}

if (-not (Test-Path $DllPath)) {
    throw "DLL not found: $DllPath"
}

$dllFull = (Resolve-Path $DllPath).Path
$regasm = Resolve-RegAsmPath -Preferred $RegAsmPath

Write-Host "Using RegAsm: $regasm"
Write-Host "Target DLL: $dllFull"

if ($Unregister) {
    & $regasm $dllFull /unregister
    if ($LASTEXITCODE -ne 0) {
        throw "RegAsm unregister failed with exit code $LASTEXITCODE"
    }
    Write-Host "Unregistered successfully."
} else {
    & $regasm $dllFull /codebase
    if ($LASTEXITCODE -ne 0) {
        throw "RegAsm register failed with exit code $LASTEXITCODE"
    }
    Write-Host "Registered successfully."
}

Write-Host "Done. Open SolidWorks and verify Add-Ins list."
