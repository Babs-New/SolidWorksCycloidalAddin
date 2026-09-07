# CycloSketch Add-In for SolidWorks

SolidWorks add-in to generate cycloidal reducer sketches from parameters.

This README is focused on installation, so any user can deploy the add-in and enable it in SolidWorks.

## Required prerequisites

- Windows 10/11 x64
- SolidWorks x64
- .NET SDK 8.x
- .NET Framework 4.8 Developer Pack (includes RegAsm)
- PowerShell 5.1+
- Windows Administrator rights (required for COM registration)

## Verify prerequisites

In PowerShell:

```powershell
dotnet --info
```

The command should report an installed 8.x SDK.

Check RegAsm:

```powershell
Test-Path "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe"
```

The command should return `True`.

## Quick install (recommended)

1. Close SolidWorks.
2. Open PowerShell as Administrator.
3. Go to the project root (folder containing `deploy_addin.ps1`).
4. Run the deployment script.

```powershell
$RepoRoot = "C:\path\to\SolidWorksCycloidalAddin"
Set-Location $RepoRoot
powershell -ExecutionPolicy Bypass -File .\deploy_addin.ps1 -KillSolidWorks
```

This script automatically:

- Builds in `Release`
- Registers `CycloSketchAddin.dll` using `RegAsm`

## Enable in SolidWorks

1. Open SolidWorks.
2. Go to `Tools > Add-Ins`.
3. Enable `CycloSketch Add-In`.
4. Also enable `Start Up` if you want automatic loading at launch.

## Manual install (alternative)

If you do not want to use `deploy_addin.ps1`:

1. Build the project:

```powershell
$RepoRoot = "C:\path\to\SolidWorksCycloidalAddin"
Set-Location (Join-Path $RepoRoot "src\CycloSketchAddin")
dotnet build -c Release --nologo
```

2. Register the DLL for COM (PowerShell as Administrator):

```powershell
$RepoRoot = "C:\path\to\SolidWorksCycloidalAddin"
Set-Location $RepoRoot
powershell -ExecutionPolicy Bypass -File .\install_addin.ps1 -DllPath (Join-Path $RepoRoot "src\CycloSketchAddin\bin\Release\net48\CycloSketchAddin.dll")
```

## Update the add-in

After code changes:

```powershell
$RepoRoot = "C:\path\to\SolidWorksCycloidalAddin"
Set-Location $RepoRoot
powershell -ExecutionPolicy Bypass -File .\deploy_addin.ps1 -KillSolidWorks
```

## Uninstall

```powershell
$RepoRoot = "C:\path\to\SolidWorksCycloidalAddin"
Set-Location $RepoRoot
powershell -ExecutionPolicy Bypass -File .\install_addin.ps1 -DllPath (Join-Path $RepoRoot "src\CycloSketchAddin\bin\Release\net48\CycloSketchAddin.dll") -Unregister
```

## Usage

1. Open a Part document (`.sldprt`).
2. Run the `Create Cyclo Reducer` command.
3. Fill in the parameter tabs.
4. Click `Generate`.

## Troubleshooting

### Add-in does not appear in the Add-Ins list

1. Make sure PowerShell was launched as Administrator.
2. Re-run manual installation (`install_addin.ps1`).
3. Restart SolidWorks.

### Build fails with locked DLL (MSB3021 / MSB3027)

SolidWorks is locking the DLL.

1. Close SolidWorks.
2. Re-run deployment with `-KillSolidWorks`.

### RegAsm not found

Install the `.NET Framework 4.8 Developer Pack`, then run the install command again.

## Useful project paths

- `src/CycloSketchAddin/CycloSketchAddin.csproj`
- `src/CycloSketchAddin/Addin/SwAddin.cs`
- `src/CycloSketchAddin/Addin/CommandManagerService.cs`
- `install_addin.ps1`
- `deploy_addin.ps1`
