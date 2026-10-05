# Fox Multitool

Multitool desktop (C# / WPF / .NET 9) — utilitaires légitimes réunis en une app.

## Lancer
```powershell
dotnet build Multitool.sln -c Release
dotnet run --project src/Multitool
```
Exe : `src/Multitool/bin/Release/net9.0-windows/FoxMultitool.exe`

## Modules v0.4.0 — menu déroulant + statut couleur
- **🌐 Réseau** (Expander)
  - Base IP / Ping / Ports : IP locale/publique, Ping, DNS lookup, scan ports natif
  - Nmap Scan (`nmap`) : `-F`, `-sV`, `-O`, `-A`, `-sn`, `-p` + boutons Installer via winget / Télécharger setup.exe (nmap-7.991) + carte statut 🟢 installé / 🔴 manquant
  - DNS / NS Lookup (`nslookup`) : NS, MX, TXT, A, AAAA, SOA, PTR
  - Tracert / ARP / Netstat (`tracert`, `arp -a`, `ipconfig /all`, `netstat -ano`, `netsh wlan show profiles`)
- **🖥️ Système** : infos OS/machine, top process RAM, nettoyage %TEMP%
- **🔍 OSINT** : IP lookup via ip-api.com
- **⚙️ Générateurs** : mot de passe, GUID, Base64, SHA256/MD5

## Structure
```
src/Multitool/
  MainWindow.xaml (+.cs)  # navigation Expanders
  Views/                 # NetworkView, NmapScanView, DnsLookupView, NetworkExtraView, SystemView, OsintView, GeneratorView
  Services/              # NetworkTools, NmapTools, DnsTools, NetExtraTools, SystemTools, OsintTools, GeneratorTools, ToolInstaller
```

## Prérequis outils externes
- `nmap` optionnel : install auto via l'app ou https://nmap.org/download.html
- `nslookup`, `tracert`, `arp`, `ipconfig`, `netstat`, `netsh` inclus Windows

## Notes
Outils défensifs / admin sys uniquement. Pas de fonction offensive.
