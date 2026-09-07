# CycloSketch Add-In for SolidWorks

SolidWorks add-in to generate cycloidal reducer sketches from parameters.

This README is focused on installation, so any user can deploy the add-in and enable it in SolidWorks.

## Required prerequisites

- Windows 10/11 x64
- SolidWorks x64 ([official page](https://www.solidworks.com/))
- .NET SDK 8.x ([download](https://dotnet.microsoft.com/en-us/download/dotnet/8.0))
- .NET Framework 4.8 Developer Pack (includes RegAsm) ([download](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net48))
- PowerShell 5.1+ ([Windows PowerShell 5.1 info](https://learn.microsoft.com/en-us/powershell/scripting/windows-powershell/wmf-overview))
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
Set-Location <your_repo_folder> ## Ex: "C:\path\to\SolidWorksCycloidalAddin"
powershell -ExecutionPolicy Bypass -File .\deploy_addin.ps1 -KillSolidWorks
```

This script automatically:

- Builds in `Release`
- Registers `CycloSketchAddin.dll` using `RegAsm`

## Execution command (short version)

Use this command whenever you want to build and register the add-in in one step:

```powershell
Set-Location <your_repo_folder>
powershell -ExecutionPolicy Bypass -File .\deploy_addin.ps1 -KillSolidWorks
```

Example:

```powershell
Set-Location D:\SolidWorksCycloidalAddin
powershell -ExecutionPolicy Bypass -File .\deploy_addin.ps1 -KillSolidWorks
```

## Enable in SolidWorks

1. Open SolidWorks.
2. Go to `Tools > Add-Ins`.
3. Enable `CycloSketch Add-In`.
4. Also enable `Start Up` if you want automatic loading at launch.

## Manual install (alternative)

If you do not want to use `deploy_addin.ps1`:

1. Build the project:

```powershell
$RepoRoot = "<your_repo_folder>"
Set-Location (Join-Path $RepoRoot "src\CycloSketchAddin")
dotnet build -c Release --nologo
```

2. Register the DLL for COM (PowerShell as Administrator):

```powershell
$RepoRoot = "<your_repo_folder>"
Set-Location $RepoRoot
powershell -ExecutionPolicy Bypass -File .\install_addin.ps1 -DllPath (Join-Path $RepoRoot "src\CycloSketchAddin\bin\Release\net48\CycloSketchAddin.dll")
```

## Update the add-in

After code changes:

```powershell
Set-Location <your_repo_folder>
powershell -ExecutionPolicy Bypass -File .\deploy_addin.ps1 -KillSolidWorks
```

## Uninstall

```powershell
$RepoRoot = "<your_repo_folder>"
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

### `dotnet --info` shows `Architecture: x86`

If `dotnet --info` reports x86, but x64 is installed, use x64 dotnet explicitly.

1. Check x64 SDK list:

```powershell
& "C:\Program Files\dotnet\dotnet.exe" --list-sdks
```

2. Check x64 runtime info:

```powershell
& "C:\Program Files\dotnet\dotnet.exe" --info
```

3. Temporary fix in current shell:

```powershell
$env:Path = "C:\Program Files\dotnet;$env:Path"
where.exe dotnet
dotnet --info
```

Expected: `C:\Program Files\dotnet\dotnet.exe` appears first and `Architecture: x64`.

Note: `deploy_addin.ps1` already prefers x64 dotnet automatically when available.

### Add-in button appears disabled or shows unexpected tooltip

This is often a stale SolidWorks CommandManager cache or a command ID collision.

1. Close SolidWorks.
2. Re-run deployment:

```powershell
$RepoRoot = "<your_repo_folder>"
Set-Location $RepoRoot
powershell -ExecutionPolicy Bypass -File .\deploy_addin.ps1 -KillSolidWorks
```

3. Start SolidWorks and re-enable the add-in in `Tools > Add-Ins`.

## Useful project paths

- `src/CycloSketchAddin/CycloSketchAddin.csproj`
- `src/CycloSketchAddin/Addin/SwAddin.cs`
- `src/CycloSketchAddin/Addin/CommandManagerService.cs`
- `install_addin.ps1`
- `deploy_addin.ps1`
