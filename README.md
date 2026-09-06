# CycloSketch Add-In for SolidWorks

Open-source SolidWorks add-in to generate clean, parameter-driven cycloidal reducer sketches.

This project focuses first on robust sketch generation (not automatic 3D bodies), so users can extrude, assemble, and manufacture with their own workflow (3D printing, CNC, laser, etc.).

## Features

- Tabbed UI with image previews and parameter validation
- Two-disc cycloidal mode:
- Disc A center offset: +e
- Disc B center offset: -e
- Disc B phase: 180deg
- Optional center bores, around holes, and output pins
- Fusion-like linking option:
- `D_output_pin = D_around_hole - 2 * eccentric`

## Generated Sketch Names

- `SK_CYCLO_DISC_PROFILE_A`
- `SK_CYCLO_DISC_PROFILE_B_180DEG`
- `SK_RING_PINS_REFERENCE`
- `SK_CENTER_BORE_A`
- `SK_CENTER_BORE_B_180DEG`
- `SK_OUTPUT_HOLES_DISC_A`
- `SK_OUTPUT_HOLES_DISC_B_180DEG`
- `SK_OUTPUT_PINS_DISC`
- `SK_REFERENCE_AXES`

## Requirements

- Windows
- SolidWorks 2025
- .NET SDK 8.x
- .NET Framework 4.8 Developer Pack
- Visual Studio 2022 (recommended)

## Repository Layout

- `src/CycloSketchAddin/Addin`
- `src/CycloSketchAddin/Core`
- `src/CycloSketchAddin/SolidWorks`
- `src/CycloSketchAddin/UI`
- `src/CycloSketchAddin/assets`

## Quick Start (Community Install)

1. Clone or download this repository.
2. Open `src/CycloSketchAddin/CycloSketchAddin.csproj` in Visual Studio.
3. Build in `Release`.
4. Close SolidWorks completely.
5. Open PowerShell as Administrator.
6. Run:

```powershell
powershell -ExecutionPolicy Bypass -File .\deploy_addin.ps1
```

7. Open SolidWorks.
8. Go to `Tools > Add-Ins`.
9. Enable `CycloSketch Add-In` (and Startup if desired).

## Manual Install (Alternative)

1. Build the project:

```powershell
Set-Location .\src\CycloSketchAddin
dotnet build -c Release --nologo
```

2. Register the add-in DLL (Admin PowerShell):

```powershell
powershell -ExecutionPolicy Bypass -File .\install_addin.ps1 -DllPath ".\src\CycloSketchAddin\bin\Release\net48\CycloSketchAddin.dll"
```

## Using the Add-In

1. Open a Part document (`.sldprt`).
2. Run `Create Cyclo Reducer` from the CycloSketch command UI.
3. Fill `Necessary param`, `optionary param`, and `Detailed setting`.
4. Click `Generate`.

## UI Preview Images

Place preview PNG files in:

- `src/CycloSketchAddin/assets`

Supported names:

- `cyclo_Discription_Image_nec.png`
- `cyclo_Discription_Image_opt.png`
- `preview_necessary_params.png`
- `preview_optional_params.png`
- `preview_detailed_settings.png`

These files are copied to output automatically during build.

## Troubleshooting

### Add-in not visible in SolidWorks Add-Ins list

1. Run PowerShell as Administrator.
2. Re-run registration with `install_addin.ps1`.
3. Restart SolidWorks.

### Build fails with MSB3021/MSB3027 (locked DLL)

SolidWorks is still running and locking the DLL.

1. Close SolidWorks.
2. Rebuild.
3. Redeploy.

Or force close from deploy script:

```powershell
powershell -ExecutionPolicy Bypass -File .\deploy_addin.ps1 -KillSolidWorks
```

### Images still do not appear

1. Verify files exist in `src/CycloSketchAddin/assets`.
2. Rebuild in `Release`.
3. Confirm copied files exist in `src/CycloSketchAddin/bin/Release/net48/assets`.
4. Redeploy and restart SolidWorks.

## Development Notes

- Primary add-in entry: `src/CycloSketchAddin/Addin/SwAddin.cs`
- Sketch generation logic: `src/CycloSketchAddin/SolidWorks/SketchGenerator.cs`
- Geometry and validation: `src/CycloSketchAddin/Core`
- UI: `src/CycloSketchAddin/UI/CycloPropertyManagerPage.cs`

## Contributing

Contributions are welcome.

1. Fork the repository.
2. Create a feature branch.
3. Add focused commits with clear messages.
4. Open a pull request with screenshots and test notes.

Recommended PR checklist:

- Build succeeds in `Release`
- Add-in registers and loads in SolidWorks
- Sketch naming remains stable
- UI remains usable at 100% and 125% display scale

## Roadmap

- Preset save/load
- Language toggle (EN/FR)
- DXF export helper
- Advanced PropertyManagerPage parity
- Optional automated 3D feature generation

## License

Open-source community project.

Recommended: MIT License (add a `LICENSE` file in repository root if not present).
