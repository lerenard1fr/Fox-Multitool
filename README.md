# Fox Multitool

Multitool desktop (C# / WPF / .NET 9) — utilitaires légitimes réunis en une app.
Interface traduite en 14 langues (menu, vues, messages) + thèmes sombre/clair/système.

## Lancer (dev)
```powershell
dotnet build Multitool.sln -c Release
dotnet run --project src/Multitool
```

## Dossiers de sortie (c'est normal d'en avoir deux)
- `src/Multitool/bin/Release/` : sortie locale de `dotnet build` (dev uniquement, **nécessite .NET 9 installé**, ignoré par git). Ne pas partager.
- `dist/FoxMultitool.exe` : version **portable** à partager (runtime inclus dans l'exe, rien à installer). Produit par le script ci-dessous, ignoré par git.

## Version portable (1 exe + dossiers, rien à installer)
```powershell
powershell -ExecutionPolicy Bypass -File ./publish-portable.ps1
```
Contenu de `dist/` après publish (~60 Mo) :
```
dist/
  FoxMultitool.exe
  settings.json   (créé au 1er lancement : thème + langue)
  tools/          (setup nmap téléchargé par l'app ; tools/nmap/nmap.exe détecté en priorité)
  logs/           (journal app-AAAA-MM-JJ.log, créé au lancement)
  data/           (exports : nmap-cible-date.txt via bouton Exporter)
```
Zippe ce dossier, mets-le sur clé USB, envoie-le à un pote ou en Release GitHub : il se lance sur Windows 10/11 x64 sans rien installer, et tout (réglages, logs, exports, outils) reste dedans.
Note : Windows SmartScreen peut afficher un avertissement (exe non signé) → "Informations complémentaires" → "Exécuter quand même".

## Release v1.0.0 (MSI + ZIP)
```powershell
powershell -ExecutionPolicy Bypass -File ./installer/build-msi.ps1
```
Produit dans `release/` (ignoré par git, à joindre à la GitHub Release) :
- `FoxMultitool-1.0.0.msi` : installeur per-user, sans admin — `%LocalAppData%\Programs\FoxMultitool`, raccourcis menu Démarrer + bureau. L'app bascule ses réglages/données vers `%LocalAppData%\FoxMultitool` si le dossier d'install n'est pas inscriptible.
- `FoxMultitool-1.0.0-portable.zip` : contenu de `dist/` à dézipper et lancer.

## Modules v1.0.0 — menu style Kali + statut couleur + options
- **🔎 Information Gathering** : Base IP / Ping / Ports, Nmap Scan (`-F -sV -O -A -sn -p` + install + export `data/`), DNS / NS Lookup (`nslookup` : NS MX TXT A AAAA SOA PTR), Tracert / ARP / Netstat, IP Lookup, Amass, theHarvester, **Host Discovery** (ping sweep CIDR natif), **Dorking** (requêtes prêtes → navigateur)
- **🛡️ Vulnerability Analysis** : Nessus, Nikto
- **🌍 Web Application Analysis** : Burp Suite, sqlmap, Gobuster, ffuf, **URL Crawler** (découverte de liens + export), **Website Cloner** (miroir léger → `data/`)
- **🔑 Password Attacks** : John the Ripper, Hashcat, Hydra
- **📡 Wireless Attacks** : Aircrack-ng
- **👃 Sniffing & Spoofing** : Wireshark, Bettercap, Responder, Netcat (ncat)
- **💥 Exploitation Tools** : Metasploit
- **🕵️ Social Engineering** : Maltego, Sherlock
- **🔎 Lookup** : **Email** (format + MX + Gravatar), **Phone** (libphonenumber hors-ligne : validité, région, opérateur), **Instagram** (ouverture profil), **Wallet** (solde BTC via blockstream.info), **Metadata** (lecture + suppression EXIF/IPTC/XMP)
- **💬 Discord** : envoi via webhook (le vôtre), infos webhook, décodage d'ID (date de création : user, voc, serveur...), infos d'invitation — sans token
- **🖥️ Système** : infos OS/machine, top process RAM, nettoyage %TEMP%
- **⚙️ Générateurs** : mot de passe, GUID, Base64, SHA256/MD5
- **⚙️ Options** : thème System (défaut, suit Windows) / Dark / Light + langue (14 langues, défaut English), sauvegardé dans `settings.json` à côté de l'exe
- **🧰 All Tools** : vue d'ensemble des 19 outils sécu — détection 🟢/🔴, install via winget ou page officielle, lancement. Audit de vos propres systèmes uniquement.

## Structure
```
src/Multitool/
  MainWindow.xaml (+.cs)  # navigation Expanders
  Views/                 # NetworkView, NmapScanView, DnsLookupView, NetworkExtraView, SystemView, OsintView, GeneratorView, SettingsView, SecurityView, DiscordView, CrawlerView, HostDiscoveryView, DorkingView, EmailView, PhoneView, InstagramView, WalletView, MetadataView, ClonerView
  Services/              # NetworkTools, NmapTools, DnsTools, NetExtraTools, SystemTools, OsintTools, GeneratorTools, ToolInstaller, ToolStatus, SecurityTools, DiscordTools, LookupTools, PhoneTools, MetaTools, AppSettings, AppPaths, Logger, Lang, ThemeManager
  Themes/                # Dark.xaml, Light.xaml
publish-portable.ps1     # produit dist/FoxMultitool/ (dossier portable)
```

## Prérequis outils externes
- `nmap` optionnel : install auto via l'app (winget ou setup dans `tools/`) ou https://nmap.org/download.html
- `nslookup`, `tracert`, `arp`, `ipconfig`, `netstat`, `netsh` inclus Windows

## Antivirus / SmartScreen (sans certificat payant)
- L'exe et le MSI sont **auto-signés** (`CN=FoxMultitool`, script `installer/selfsign.ps1`, intégré à `build-msi.ps1`) : identité stable + preuve d'intégrité, rebuild automatique à chaque release.
- Limite honnête : SmartScreen/Defender avertiront quand même (pas de réputation). La vraie solution gratuite : soumettre les 2 fichiers à Microsoft (compte gratuit) sur https://www.microsoft.com/en-us/wdsi/filesubmission → "Software developer" → whitelist Defender. La réputation SmartScreen monte avec les téléchargements.

## Notes
Outils défensifs / admin sys uniquement. Pas de fonction offensive.
