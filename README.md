# CycloSketch Add-In for SolidWorks

Add-in SolidWorks pour generer des esquisses de reducteur cycloidal a partir de parametres.

Ce README est centre sur l'installation, afin qu'un utilisateur puisse deployer l'add-in et l'activer dans SolidWorks.

## Prerequis obligatoires

- Windows 10/11 x64
- SolidWorks 2025 x64
- .NET SDK 8.x
- .NET Framework 4.8 Developer Pack (inclut RegAsm)
- PowerShell 5.1+
- Droits Administrateur Windows (necessaire pour l'enregistrement COM)

## Verifier les prerequis

Dans PowerShell:

```powershell
dotnet --info
```

La commande doit repondre avec un SDK 8.x installe.

Verifier RegAsm:

```powershell
Test-Path "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe"
```

La commande doit retourner `True`.

## Installation rapide (recommandee)

1. Fermer SolidWorks.
2. Ouvrir PowerShell en Administrateur.
3. Aller a la racine du projet.
4. Lancer le script de deploiement.

```powershell
Set-Location G:\SolidWorksCycloidalAddin
powershell -ExecutionPolicy Bypass -File .\deploy_addin.ps1 -KillSolidWorks
```

Ce script fait automatiquement:

- Build en `Release`
- Enregistrement de `CycloSketchAddin.dll` via `RegAsm`

## Activation dans SolidWorks

1. Ouvrir SolidWorks.
2. Aller dans `Tools > Add-Ins`.
3. Cocher `CycloSketch Add-In`.
4. Cocher aussi `Start Up` si vous voulez le chargement automatique au demarrage.

## Installation manuelle (alternative)

Si vous ne souhaitez pas utiliser `deploy_addin.ps1`:

1. Build du projet:

```powershell
Set-Location G:\SolidWorksCycloidalAddin\src\CycloSketchAddin
dotnet build -c Release --nologo
```

2. Enregistrement COM de la DLL (PowerShell Admin):

```powershell
Set-Location G:\SolidWorksCycloidalAddin
powershell -ExecutionPolicy Bypass -File .\install_addin.ps1 -DllPath ".\src\CycloSketchAddin\bin\Release\net48\CycloSketchAddin.dll"
```

## Mise a jour de l'add-in

Apres modification du code:

```powershell
Set-Location G:\SolidWorksCycloidalAddin
powershell -ExecutionPolicy Bypass -File .\deploy_addin.ps1 -KillSolidWorks
```

## Desinstallation

```powershell
Set-Location G:\SolidWorksCycloidalAddin
powershell -ExecutionPolicy Bypass -File .\install_addin.ps1 -DllPath ".\src\CycloSketchAddin\bin\Release\net48\CycloSketchAddin.dll" -Unregister
```

## Utilisation

1. Ouvrir une piece (`.sldprt`).
2. Lancer la commande `Create Cyclo Reducer`.
3. Renseigner les onglets de parametres.
4. Cliquer `Generate`.

## Depannage

### L'add-in n'apparait pas dans la liste Add-Ins

1. Verifier que PowerShell a ete lance en Administrateur.
2. Relancer l'installation manuelle (`install_addin.ps1`).
3. Redemarrer SolidWorks.

### Echec de build avec DLL verrouillee (MSB3021 / MSB3027)

SolidWorks verrouille la DLL.

1. Fermer SolidWorks.
2. Refaire le deploiement avec `-KillSolidWorks`.

### RegAsm introuvable

Installer le `.NET Framework 4.8 Developer Pack`, puis relancer la commande d'installation.

## Arborescence utile

- `src/CycloSketchAddin/CycloSketchAddin.csproj`
- `src/CycloSketchAddin/Addin/SwAddin.cs`
- `src/CycloSketchAddin/Addin/CommandManagerService.cs`
- `install_addin.ps1`
- `deploy_addin.ps1`
