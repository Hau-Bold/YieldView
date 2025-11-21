# MoveArtifactsToRelease.ps1
# Copies Linux tarball, Windows MSI, and MSI .wixpdb to a release folder structure
# Safe to call from MSBuild, will not break build if files are missing

param(
    [string]$TarballPath = "$PSScriptRoot\..\..\publish\linux\YieldView-Ubuntu.tar.gz",
    [string]$MsiPath = "$PSScriptRoot\..\..\YieldView.API.Installer\bin\Debug\YieldView.API.Installer.msi",
    [string]$WixPdbPath = "$PSScriptRoot\..\..\YieldView.API.Installer\bin\Debug\YieldView.API.Installer.wixpdb",
    [string]$ReleaseFolder = "$PSScriptRoot\..\Release"
)

try {
    Write-Host "Release folder: $ReleaseFolder"

    # Ensure release folder exists
    if (Test-Path $ReleaseFolder) {
        Write-Host "Cleaning existing release folder..."
        try { Remove-Item -Path "$ReleaseFolder\*" -Recurse -Force -ErrorAction Stop } catch {
            Write-Warning "Could not fully clean release folder: $_"
        }
    } else {
        New-Item -ItemType Directory -Path $ReleaseFolder | Out-Null
        Write-Host "Created release folder: $ReleaseFolder"
    }

    # Create subfolders
    $WindowsFolder = Join-Path $ReleaseFolder "Windows"
    $LinuxFolder   = Join-Path $ReleaseFolder "Linux"

    foreach ($folder in @($WindowsFolder, $LinuxFolder)) {
        if (-not (Test-Path $folder)) {
            New-Item -ItemType Directory -Path $folder | Out-Null
            Write-Host "Created folder: $folder"
        }
    }

    # Copy Linux tarball
    if (Test-Path $TarballPath) {
        Copy-Item $TarballPath -Destination $LinuxFolder -Force
        Write-Host "Copied Linux tarball to $LinuxFolder"
    } else {
        Write-Warning "Tarball not found: $TarballPath"
    }

    # Copy Windows MSI
    if (Test-Path $MsiPath) {
        Copy-Item $MsiPath -Destination $WindowsFolder -Force
        Write-Host "Copied Windows MSI to $WindowsFolder"
    } else {
        Write-Warning "MSI not found: $MsiPath"
    }

    # Copy MSI .wixpdb
    if (Test-Path $WixPdbPath) {
        Copy-Item $WixPdbPath -Destination $WindowsFolder -Force
        Write-Host "Copied MSI .wixpdb to $WindowsFolder"
    } else {
        Write-Warning "MSI .wixpdb not found: $WixPdbPath"
    }

    Write-Host "Artifacts move to release folder complete."
} catch {
    Write-Warning "An unexpected error occurred: $_"
} finally {
    exit 0  # Prevents MSBuild from treating warnings as failures
}
