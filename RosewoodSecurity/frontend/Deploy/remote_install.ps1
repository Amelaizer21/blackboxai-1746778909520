# This script should be run on your Windows 11 machine to install Rosewood Security

# Ensure running as administrator
if (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Warning "Please run this script as Administrator"
    Exit
}

# Check for .NET 6.0 SDK
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host "Installing .NET 6.0 SDK..." -ForegroundColor Yellow
    $dotnetUrl = "https://download.visualstudio.microsoft.com/download/pr/c505a449-9ecf-4352-8629-56216f521616/bd96242dc40c0d14385c0006c97b556f/dotnet-sdk-6.0.417-win-x64.exe"
    $dotnetInstaller = "$env:TEMP\dotnet-sdk-6.0.417-win-x64.exe"
    Invoke-WebRequest -Uri $dotnetUrl -OutFile $dotnetInstaller
    Start-Process -Wait -FilePath $dotnetInstaller -ArgumentList "/quiet /norestart"
    Remove-Item $dotnetInstaller
}

# Set the installation directory
$installDir = "C:\Program Files\RosewoodSecurity"

# Create the installation directory if it doesn't exist
if (-not (Test-Path $installDir)) {
    New-Item -ItemType Directory -Path $installDir | Out-Null
}

# Set working directory to the script location
Set-Location $PSScriptRoot

# Run the deployment script
Write-Host "Running deployment script..." -ForegroundColor Green
.\deploy.ps1 -OutputPath $installDir

# Create desktop shortcut
$WshShell = New-Object -comObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut("$env:USERPROFILE\Desktop\Rosewood Security.lnk")
$Shortcut.TargetPath = "$installDir\RosewoodSecurity.exe"
$Shortcut.Save()

Write-Host "Installation completed successfully!" -ForegroundColor Green
Write-Host "The application has been installed to: $installDir" -ForegroundColor Green
Write-Host "A desktop shortcut has been created." -ForegroundColor Green

# Launch the application
$launchApp = Read-Host "Would you like to launch Rosewood Security now? (Y/N)"
if ($launchApp -eq 'Y' -or $launchApp -eq 'y') {
    Start-Process "$installDir\RosewoodSecurity.exe"
}
