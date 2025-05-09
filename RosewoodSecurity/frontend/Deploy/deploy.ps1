param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$OutputPath = "C:\Program Files\RosewoodSecurity",
    [switch]$CreateInstaller = $false
)

# Stop on any error
$ErrorActionPreference = "Stop"

Write-Host "Starting Rosewood Security deployment process..." -ForegroundColor Green

# Ensure we're in the right directory
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location (Split-Path -Parent $scriptPath)

# Clean previous builds
if (Test-Path $OutputPath) {
    Write-Host "Cleaning previous build..." -ForegroundColor Yellow
    Remove-Item -Path $OutputPath -Recurse -Force
}

# Create output directory if it doesn't exist
if (-not (Test-Path $OutputPath)) {
    New-Item -ItemType Directory -Path $OutputPath | Out-Null
}

# Restore dependencies
Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore RosewoodSecurity.sln

# Run tests
Write-Host "Running tests..." -ForegroundColor Yellow
dotnet test RosewoodSecurity.Tests/RosewoodSecurity.Tests.csproj --configuration $Configuration

if ($LASTEXITCODE -ne 0) {
    Write-Host "Tests failed! Aborting deployment." -ForegroundColor Red
    exit 1
}

# Publish application
Write-Host "Publishing application..." -ForegroundColor Yellow
dotnet publish RosewoodSecurity/RosewoodSecurity.csproj `
    --configuration $Configuration `
    --runtime $Runtime `
    --self-contained true `
    --output $OutputPath `
    -p:PublishSingleFile=true `
    -p:PublishTrimmed=true `
    -p:IncludeNativeLibrariesForSelfExtract=true

# Copy additional files
Write-Host "Copying additional files..." -ForegroundColor Yellow
Copy-Item "RosewoodSecurity/appsettings.json" -Destination $OutputPath
Copy-Item "README.md" -Destination $OutputPath

# Create version info file
$version = (Get-Item "$OutputPath\RosewoodSecurity.exe").VersionInfo.FileVersion
@"
Rosewood Security
Version: $version
Deployment Date: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
Configuration: $Configuration
Runtime: $Runtime
"@ | Out-File "$OutputPath\version.txt"

Write-Host "Deployment completed successfully!" -ForegroundColor Green
Write-Host "Application installed to: $OutputPath" -ForegroundColor Green

# Launch the application
$exePath = Join-Path $OutputPath "RosewoodSecurity.exe"
if (Test-Path $exePath) {
    Write-Host "Launching application..." -ForegroundColor Green
    Start-Process $exePath
}
