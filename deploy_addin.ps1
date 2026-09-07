param(
    [switch]$KillSolidWorks
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$root = (Resolve-Path $root).Path
$projDir = Join-Path $root "src\CycloSketchAddin"
$dllPath = Join-Path $projDir "bin\Release\net48\CycloSketchAddin.dll"
$installScript = Join-Path $root "install_addin.ps1"

if (-not (Test-Path $projDir)) {
    throw "Project directory not found: $projDir"
}

if (-not (Test-Path $installScript)) {
    throw "Install script not found: $installScript"
}

function Resolve-DotnetPath {
    $x64Dotnet = Join-Path ${env:ProgramFiles} "dotnet\dotnet.exe"
    if (Test-Path $x64Dotnet) {
        return $x64Dotnet
    }

    $dotnetCmd = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($dotnetCmd -and (Test-Path $dotnetCmd.Source)) {
        return $dotnetCmd.Source
    }

    throw "dotnet.exe not found. Install .NET SDK 8.x (x64)."
}

$dotnetExe = Resolve-DotnetPath
Write-Host "Using dotnet: $dotnetExe"

$sw = Get-Process -Name "SLDWORKS" -ErrorAction SilentlyContinue
if ($sw) {
    if ($KillSolidWorks) {
        Write-Host "Stopping SolidWorks..."
        Stop-Process -Name "SLDWORKS" -Force
    } else {
        throw "SolidWorks is running and can lock the DLL. Close SolidWorks, then run again. Or run with -KillSolidWorks."
    }
}

Push-Location $projDir
try {
    Write-Host "Building addin..."
    & $dotnetExe build -c Release --nologo
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed."
    }
}
finally {
    Pop-Location
}

Write-Host "Registering addin..."
powershell -ExecutionPolicy Bypass -File $installScript -DllPath $dllPath
if ($LASTEXITCODE -ne 0) {
    throw "Registration failed."
}

Write-Host "Done. Open SolidWorks and enable 'CycloSketch Add-In' in Add-Ins."
