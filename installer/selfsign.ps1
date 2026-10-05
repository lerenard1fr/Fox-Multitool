# Auto-signature gratuite (certificat auto-genere, sans autorite payante).
# Usage : powershell -ExecutionPolicy Bypass -File ./installer/selfsign.ps1 -Files "chemin\a.exe","chemin\b.msi"
# Effet : identite stable (CN=FoxMultitool) + preuve d'integrite (fichier non modifie depuis signature).
# Limite HONNETE : SmartScreen/Defender continueront d'avertir tant que le fichier n'a pas de reputation.
#   La vraie solution gratuite : soumettre les fichiers a Microsoft (compte dev gratuit) :
#   https://www.microsoft.com/en-us/wdsi/filesubmission  -> "Software developer" -> whitelist Defender.
# Le certificat vit dans Cert:\CurrentUser\My (nom "FoxMultitool Self-Signed"), rien a sauvegarder.

param([string[]]$Files = @())
$ErrorActionPreference = "Stop"

$cert = Get-ChildItem Cert:\CurrentUser\My -CodeSigningCert -ErrorAction SilentlyContinue |
  Where-Object { $_.FriendlyName -eq "FoxMultitool Self-Signed" } | Select-Object -First 1
if (-not $cert) {
  $cert = New-SelfSignedCertificate -Type CodeSigningCert -Subject "CN=FoxMultitool" `
    -FriendlyName "FoxMultitool Self-Signed" -KeyAlgorithm RSA -KeyLength 2048 `
    -NotAfter (Get-Date).AddYears(5) -CertStoreLocation Cert:\CurrentUser\My `
    -KeyUsage DigitalSignature -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3")
  Write-Host "Certificat cree : $($cert.Thumbprint)"
} else {
  Write-Host "Certificat existant : $($cert.Thumbprint)"
}

# Le cert auto-genere doit etre racine de confiance (magasin Root de l'utilisateur,
# sans admin) sinon la signature reste "UnknownError". Sans effet sur les autres PC.
$inRoot = Get-ChildItem Cert:\CurrentUser\Root -ErrorAction SilentlyContinue |
  Where-Object { $_.Thumbprint -eq $cert.Thumbprint } | Select-Object -First 1
if (-not $inRoot) {
  $tmpCer = Join-Path ([System.IO.Path]::GetTempPath()) "fox-selfsigned.cer"
  Export-Certificate -Cert $cert -FilePath $tmpCer -Force | Out-Null
  Import-Certificate -FilePath $tmpCer -CertStoreLocation Cert:\CurrentUser\Root | Out-Null
  Remove-Item $tmpCer -Force
  Write-Host "Certificat ajoute aux racines de confiance (utilisateur local uniquement)."
}

foreach ($f in $Files) {
  if (-not (Test-Path $f)) { Write-Host "Introuvable : $f"; continue }
  $sig = Set-AuthenticodeSignature -FilePath $f -Certificate $cert `
    -TimestampServer "http://timestamp.digicert.com" -HashAlgorithm SHA256
  if ($sig.Status -ne "Valid") {
    Write-Host "Timestamp echoue, signature sans horodatage : $f"
    $sig = Set-AuthenticodeSignature -FilePath $f -Certificate $cert -HashAlgorithm SHA256
  }
  Write-Host "$($sig.Status) : $f"
}
