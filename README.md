# Fox Multitool

Multitool desktop (C# / WPF / .NET 9) — utilitaires légitimes réunis en une app.

## Lancer
```powershell
dotnet build Multitool.sln -c Release
dotnet run --project src/Multitool
```
Exe : `src/Multitool/bin/Release/net9.0-windows/FoxMultitool.exe`

## Modules v0.2.0
- **Réseau** : IP locale/publique, Ping, DNS lookup, scan ports communs natif
- **Nmap / DNS** : utilise les outils connus `nmap` et `nslookup`
  - Nmap : `-F`, `-sV`, `-O`, `-A`, `-sn`, `-p` (si nmap installé)
  - Boutons **Installer via winget** (`winget install Insecure.Nmap`), **Télécharger setup.exe** (nmap-7.991 depuis nmap.org vers %TEMP% + lancement auto), **Ouvrir nmap.org**
  - DNS : NS, MX, TXT, A, PTR via `nslookup`
- **Système** : infos OS/machine, top process RAM, nettoyage %TEMP%
- **OSINT / Lookup** : IP lookup via ip-api.com (pays, ville, ISP, AS)
- **Générateurs** : mot de passe, GUID, Base64 encode/decode, SHA256/MD5

## Structure
```
src/Multitool/
  MainWindow.xaml (+.cs)  # navigation latérale
  Views/                 # NetworkView, NmapView, SystemView, OsintView, GeneratorView
  Services/              # NetworkTools, NmapTools, DnsTools, SystemTools, OsintTools, GeneratorTools
```

## Prérequis outils externes
- `nmap` optionnel : https://nmap.org/download.html
- `nslookup` inclus dans Windows

## Notes
Outils défensifs / admin sys uniquement. Pas de fonction offensive.
