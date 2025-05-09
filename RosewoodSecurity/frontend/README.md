# Rosewood Security - Frontend Application

This is the frontend WPF application for the Rosewood Security Key and Access Management System.

## Prerequisites

- Windows 10 or later
- [.NET 6.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) or later with WPF workload installed

## Setup

1. Clone the repository and navigate to the `frontend` directory.

2. Restore NuGet packages:

```bash
dotnet restore RosewoodSecurity.sln
```

3. Update `appsettings.json` or `appsettings.Development.json` with your backend API URL and other settings as needed.

## Build and Run

You can build and run the application using Visual Studio or the command line.

### Using Visual Studio

- Open `RosewoodSecurity.sln`.
- Set `RosewoodSecurity` as the startup project.
- Build and run the project.

### Using Command Line

```bash
dotnet build RosewoodSecurity.sln
dotnet run --project RosewoodSecurity/RosewoodSecurity.csproj
```

## Running Tests

Run all unit tests using Visual Studio Test Explorer or via command line:

```bash
dotnet test RosewoodSecurity.Tests/RosewoodSecurity.Tests.csproj
```

## Deployment Packaging

To create a self-contained deployment package:

1. Publish the application as a self-contained executable:

```bash
dotnet publish RosewoodSecurity/RosewoodSecurity.csproj -c Release -r win-x64 --self-contained true -o ./publish
```

2. The published files will be in the `publish` folder. You can distribute this folder as the application package.

3. Optionally, create an installer using tools like [WiX Toolset](https://wixtoolset.org/) or [Inno Setup](https://jrsoftware.org/isinfo.php).

## Configuration

- `appsettings.json` contains application settings such as API endpoints, security, UI preferences, and logging.
- Use `appsettings.Development.json` for development overrides.

## Notes

- Ensure the backend API is running and accessible at the configured URL.
- The application uses Material Design in XAML Toolkit for UI styling.
- Authentication supports JWT tokens with optional two-factor authentication.

## Support

For issues or questions, please contact the development team.

---

Rosewood Security © 2023
