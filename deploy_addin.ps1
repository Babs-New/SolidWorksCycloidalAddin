param(
    [switch]$KillSolidWorks
)

$ErrorActionPreference = "Stop"

$root = "g:\SolidWorksCycloidalAddin"
$projDir = Join-Path $root "src\CycloSketchAddin"
$dllPath = Join-Path $projDir "bin\Release\net48\CycloSketchAddin.dll"
$installScript = Join-Path $root "install_addin.ps1"

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
    dotnet build -c Release --nologo
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
