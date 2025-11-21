# GenerateCertificates.ps1
param(
    [string]$CertFolder = "..\certificates",
    [string]$CertFileName = "localhost.pfx",
    [string]$CertPassword = "YourPassword"
)

$openssl = "openssl"
if (-not (Get-Command $openssl -ErrorAction SilentlyContinue)) {
    Write-Error "OpenSSL not found. Install OpenSSL and ensure it's in your PATH."
    exit 1
}

# Create folder if missing
$CertFolderFull = Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) $CertFolder
if (-not (Test-Path $CertFolderFull)) {
    New-Item -ItemType Directory -Path $CertFolderFull | Out-Null
    Write-Host "Created certificates folder: $CertFolderFull"
}

# Temp paths
$tmpKey = Join-Path $CertFolderFull "localhost.key"
$tmpCrt = Join-Path $CertFolderFull "localhost.crt"
$pfxPath = Join-Path $CertFolderFull $CertFileName

# Step 1: Generate key and certificate
Write-Host "Generating private key and certificate..."
& $openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout $tmpKey -out $tmpCrt -subj "/CN=localhost"

# Step 2: Convert to PFX (Windows syntax)
Write-Host "Converting to PFX..."
& $openssl pkcs12 -export -out $pfxPath -inkey $tmpKey -in $tmpCrt -password "pass:$CertPassword"

Write-Host "PFX generated at: $pfxPath"

# Remove temp files
Remove-Item -Path $tmpKey, $tmpCrt -Force
Write-Host "Temporary key and crt removed."
Write-Host "Certificate generation complete."
