; SAYA64 Extreme - Inno Setup Script
; v2.1.0

#define MyAppName "SAYA64 Extreme"
#define MyAppVersion "2.1.0"
#define MyAppPublisher "SAYA64 Project"
#define MyAppURL "https://github.com/saya64extreme"
#define MyAppExeName "SAYA64Extreme.exe"
#define MySrcDir "D:\NEXTCLOUD\Windows_app\SAYA64 Extreme"

[Setup]
AppId={{A7E3F4B2-9C1D-4E6F-8A2B-5D3C7F1E9B4A}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} v{#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir={#MySrcDir}\installer\output
OutputBaseFilename=SAYA64Extreme_v{#MyAppVersion}_Setup
SetupIconFile={#MySrcDir}\installer\app.ico
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
UninstallDisplayIcon={app}\{#MyAppExeName}
VersionInfoVersion={#MyAppVersion}.0
VersionInfoProductName={#MyAppName}
LicenseFile={#MySrcDir}\installer\LICENSE.txt

[Languages]
Name: "japanese"; MessagesFile: "compiler:Languages\Japanese.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#MySrcDir}\src\SAYA64Extreme\bin\Release\net8.0-windows\win-x64\publish\SAYA64Extreme.exe"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent runascurrentuser
