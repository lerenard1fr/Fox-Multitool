# Dossier portable FoxMultitool : racine propre, rien a installer (sauf nmap via l'app).
# Usage : powershell -ExecutionPolicy Bypass -File ./publish-portable.ps1
# Sortie : dist/  (runtime .NET 9 inclus DANS l'exe, ~60 Mo)
#   dist/
#     FoxMultitool.exe
#     settings.json   (cree au 1er lancement : theme + langue)
#     tools/          (setup nmap telecharge par l'app ; tools/nmap/nmap.exe detecte en priorite)
#     logs/           (journal, cree au lancement)
#     data/           (exports nmap, cree au lancement)
# Zippe dist/, mets-le sur cle USB, envoie-le a un pote ou en Release GitHub.

$ErrorActionPreference = "Stop"

$out = Join-Path $PSScriptRoot "dist"
New-Item -ItemType Directory -Force -Path $out | Out-Null
$tmp = Join-Path ([System.IO.Path]::GetTempPath()) "fox-publish"

dotnet publish "$PSScriptRoot/src/Multitool/Multitool.csproj" `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:EnableCompressionInSingleFile=true `
  -o $tmp

$exe = Join-Path $tmp "FoxMultitool.exe"
if (-not (Test-Path $exe)) {
  Write-Host "ECHEC : publish sans exe"
  exit 1
}
Remove-Item (Join-Path $tmp "FoxMultitool.pdb") -Force -ErrorAction SilentlyContinue

# Racine propre : seul l'exe (+ settings/tools/logs/data crees par l'app).
# Supprime l'ancien dossier-publish si present.
$old = Join-Path $out "FoxMultitool"
if (Test-Path $old) { Remove-Item $old -Recurse -Force }
Copy-Item $exe (Join-Path $out "FoxMultitool.exe") -Force
Remove-Item $tmp -Recurse -Force

$size = [math]::Round((Get-Item (Join-Path $out "FoxMultitool.exe")).Length / 1MB, 1)
Write-Host "OK : $out/FoxMultitool.exe ($size Mo)"
Get-ChildItem $out | Format-Table Name
