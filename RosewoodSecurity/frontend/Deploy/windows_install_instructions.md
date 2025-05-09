# Windows Installation Instructions for Rosewood Security

## Prerequisites

1. Install the following on your Windows machine:
   - [.NET 6.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
   - [PowerShell 7](https://github.com/PowerShell/PowerShell/releases)
   - [Inno Setup](https://jrsoftware.org/isdl.php) (if you want to create an installer)

## Installation Steps

1. Copy the entire `RosewoodSecurity` folder to your Windows machine.

2. Open PowerShell as Administrator and navigate to the deployment folder:
   ```powershell
   cd path\to\RosewoodSecurity\frontend\Deploy
   ```

3. If this is the first time running PowerShell scripts, you may need to allow script execution:
   ```powershell
   Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
   ```

4. Run the deployment script:
   ```powershell
   .\deploy.ps1
   ```
   This will:
   - Build the application
   - Run all tests
   - Publish to D:\RosewoodSecurity
   - Create a version info file
   - Launch the application

5. The application will be installed to D:\RosewoodSecurity and automatically launched.

## Creating an Installer (Optional)

If you want to create a distributable installer:

1. Make sure Inno Setup is installed.
2. Run the deployment script with the CreateInstaller parameter:
   ```powershell
   .\deploy.ps1 -CreateInstaller
   ```
3. The installer will be created in the Deploy\Installers folder.

## Troubleshooting

1. If you get a security warning when running the script:
   ```powershell
   Unblock-File .\deploy.ps1
   ```

2. If the D: drive doesn't exist, modify the OutputPath in deploy.ps1:
   ```powershell
   .\deploy.ps1 -OutputPath "C:\RosewoodSecurity"
   ```

3. If you encounter any .NET SDK errors, verify that .NET 6.0 SDK is installed:
   ```powershell
   dotnet --list-sdks
   ```

For additional help, refer to the README.md in the project root directory.
