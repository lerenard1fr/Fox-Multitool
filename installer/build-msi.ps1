# Release v1.0.0 : portable dist/ -> MSI + ZIP dans release/.
# Usage : powershell -ExecutionPolicy Bypass -File ./installer/build-msi.ps1
# Requiert : WiX Toolset v3.11 (candle.exe + light.exe).
# Sortie : release/FoxMultitool-1.0.0.msi + release/FoxMultitool-1.0.0-portable.zip
# A joindre a la GitHub Release v1.0.0.

$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent
$version = "1.0.0"
$wixBin = "C:\Program Files (x86)\WiX Toolset v3.11\bin"

# 1. Portable frais
& powershell -ExecutionPolicy Bypass -File (Join-Path $root "publish-portable.ps1")

$dist = Join-Path $root "dist"
$release = Join-Path $root "release"
New-Item -ItemType Directory -Force -Path $release | Out-Null

# 2. MSI (WiX v3.11, per-user, sans elevation)
$candle = Join-Path $wixBin "candle.exe"
$light = Join-Path $wixBin "light.exe"
if (-not (Test-Path $candle) -or -not (Test-Path $light)) {
  Write-Host "WiX v3.11 introuvable dans $wixBin"
  exit 1
}
Push-Location $PSScriptRoot
New-Item -ItemType Directory -Force -Path (Join-Path $PSScriptRoot "obj") | Out-Null
& $candle Product.wxs "-dSourceDir=$dist" -out obj\
if ($LASTEXITCODE -ne 0) { Pop-Location; Write-Host "candle a echoue"; exit 1 }
& $light obj\Product.wixobj -ext WixUIExtension -sw1076 -out (Join-Path $release "FoxMultitool-$version.msi")
if ($LASTEXITCODE -ne 0) { Pop-Location; Write-Host "light a echoue"; exit 1 }
Pop-Location

# 3. ZIP du portable (nettoye : jamais de settings/logs/data/tools perso dedans)
Remove-Item (Join-Path $dist "settings.json") -Force -ErrorAction SilentlyContinue
Get-ChildItem $dist -Directory | Where-Object { $_.Name -in @("logs", "data", "tools") } | ForEach-Object {
  Get-ChildItem $_.FullName -Recurse -File -ErrorAction SilentlyContinue | Remove-Item -Force -ErrorAction SilentlyContinue
}
$zip = Join-Path $release "FoxMultitool-$version-portable.zip"
if (Test-Path $zip) { Remove-Item $zip -Force }
Compress-Archive -Path (Join-Path $dist "*") -DestinationPath $zip

Get-ChildItem $release | Format-Table Name, @{n="Mo";e={[math]::Round($_.Length/1MB,1)}}
Write-Host "OK : MSI + ZIP dans $release"
