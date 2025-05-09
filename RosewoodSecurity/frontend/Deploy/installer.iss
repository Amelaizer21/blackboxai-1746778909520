#define MyAppName "Rosewood Security"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Rosewood Security"
#define MyAppURL "https://www.rosewoodsecurity.com"
#define MyAppExeName "RosewoodSecurity.exe"

[Setup]
AppId={{8B8E5D5A-7B57-4225-8BBC-41E3E937B8EB}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
LicenseFile=..\LICENSE
OutputDir=.\Installers
OutputBaseFilename=RosewoodSecurity-Setup
Compression=lzma
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "..\publish\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\README.md"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{app}"

[Code]
function InitializeSetup(): Boolean;
begin
  Result := True;
  // Check for .NET 6.0 Runtime
  if not IsDotNetInstalled(net60, 0) then
  begin
    MsgBox('.NET 6.0 Runtime is required to run this application. Please install it first.', mbInformation, MB_OK);
    Result := False;
  end;
end;

[CustomMessages]
DotNetFrameworkNeeded=This application requires .NET 6.0 Runtime. Please install it and run this setup again.
