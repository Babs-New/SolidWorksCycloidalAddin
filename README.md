# CycloSketch Add-In (SolidWorks)

Add-In SolidWorks (C#) pour generer les sketches de base d'un reducteur cycloidal.

## Statut

- V1: base Add-In + commande + generation sketches dans la piece active.
- UI native: PropertyManagerPage (3 onglets) avec fallback automatique vers formulaire WinForms.
- Architecture modulaire prete pour enrichissement (images, preview, presets, i18n).

## Arborescence

- src/CycloSketchAddin/Addin
- src/CycloSketchAddin/Core
- src/CycloSketchAddin/SolidWorks
- src/CycloSketchAddin/UI

## Prerequis

- SolidWorks 2025 installe
- .NET Framework 4.8 Developer Pack
- References COM SolidWorks (`SolidWorks.Interop.sldworks`, `SolidWorks.Interop.swconst`)

## Fonctionnalites V1

- Generate sketches (separes et nommes):
  - 00_SK_REFERENCE_AXES
  - 01_SK_CYCLO_DISC_PROFILE
  - 02_SK_CENTER_BORE
  - 03_SK_OUTPUT_HOLES_DISC
  - 04_SK_RING_PINS_REFERENCE
  - 05_SK_OUTPUT_PINS_DISC
- Validation geometrique minimale
- Regle de liaison Fusion-like optionnelle:
  - D_output_pin = D_around_hole - 2 * eccentric

## Build

1. Ouvrir `src/CycloSketchAddin/CycloSketchAddin.csproj` dans Visual Studio.
2. Verifier les references SolidWorks Interop.
3. Build en `Release x64`.
4. Enregistrer la DLL COM (admin):
  - `powershell -ExecutionPolicy Bypass -File .\install_addin.ps1 -DllPath "<chemin-vers>\CycloSketchAddin.dll"`
5. Activer l'Add-In dans SolidWorks.

## Utilisation dans SolidWorks

1. Ouvrir une piece (.sldprt).
2. Lancer la commande `Create Cyclo Reducer` depuis le menu/toolbar CycloSketch.
3. Remplir les champs de la PropertyManagerPage (Necessary, Optional, Detailed setting).
4. Cliquer OK pour generer les sketches.

## Sketches generes

- 00_SK_REFERENCE_AXES
- 01_SK_CYCLO_DISC_PROFILE
- 02_SK_CENTER_BORE
- 03_SK_OUTPUT_HOLES_DISC
- 04_SK_RING_PINS_REFERENCE
- 05_SK_OUTPUT_PINS_DISC

## Note

Cette base privilegie la stabilite geometrie/sketch. La PropertyManagerPage utilise un bridge COM tolerant; si la UI native ne s'ouvre pas sur une machine, l'Add-In bascule automatiquement vers la mini UI WinForms.
