# XTerminal Packaging Script for BOOTH
# This script packages the Unity package into a .tgz file and creates a .zip for distribution.

$PackagePath = "Packages/jp.xeon.x-terminal"
$DistPath = "Dist"

# Ensure Dist directory exists
if (-not (Test-Path $DistPath)) {
    New-Item -ItemType Directory -Path $DistPath | Out-Null
}

# Clear previous builds
Remove-Item -Path "$DistPath/*" -Recurse -Force -ErrorAction SilentlyContinue

# Get version from package.json
$PackageJson = Get-Content (Join-Path $PackagePath "package.json") | ConvertFrom-Json
$Version = $PackageJson.version
$DisplayName = $PackageJson.displayName

Write-Host "Packaging $DisplayName v$Version..." -ForegroundColor Cyan

# Navigate to package directory and pack
Push-Location $PackagePath
try {
    # npm pack creates a .tgz file
    $PackageFile = npm pack
    if ($LASTEXITCODE -ne 0) {
        Write-Error "npm pack failed. Make sure npm is installed."
        exit $LASTEXITCODE
    }
} finally {
    Pop-Location
}

# Move the package to Dist folder
$SourceFile = Join-Path $PackagePath $PackageFile
Move-Item -Path $SourceFile -Destination $DistPath

# Copy documentation and LICENSE to Dist folder
Write-Host "Copying documentation and LICENSE..." -ForegroundColor Cyan
if (Test-Path "LICENSE.md") { Copy-Item -Path "LICENSE.md" -Destination $DistPath -Force }
Copy-Item -Path "README.md" -Destination $DistPath -Force
Copy-Item -Path "DOCS_BOOTH_INSTALL.md" -Destination $DistPath -Force
# Also copy the Japanese README if it exists in the package
if (Test-Path "$PackagePath/README.md") {
    Copy-Item -Path "$PackagePath/README.md" -Destination (Join-Path $DistPath "README_Package.md") -Force
}

# Create ZIP file
$ZipName = "${DisplayName}_v${Version}.zip"
Write-Host "Creating ZIP archive: $ZipName" -ForegroundColor Cyan

if (Test-Path $ZipName) { Remove-Item $ZipName }

# Compress the contents of the Dist folder
Compress-Archive -Path "$DistPath\*" -DestinationPath $ZipName -Force

Write-Host "Successfully packaged: $ZipName" -ForegroundColor Green
Write-Host "The ZIP file is ready for distribution at the project root." -ForegroundColor Yellow

# Open the root folder in explorer
# explorer.exe .
