# Desktop Shortcut Installer for Windows Packaged Apps

[![NuGet](https://img.shields.io/nuget/v/DesktopShortcutInstaller)](https://www.nuget.org/packages/DesktopShortcutInstaller)
[![NuGet Downloads](https://img.shields.io/nuget/dt/DesktopShortcutInstaller)](https://www.nuget.org/packages/DesktopShortcutInstaller)
[![License](https://img.shields.io/github/license/yourusername/DesktopShortcutInstaller)](LICENSE)

Automatically creates desktop shortcuts for your Windows packaged applications (MSIX/AppX) using the app title and icon from `Package.appxmanifest`.

## Features

- ✅ **ZERO CODE REQUIRED**: Just install the NuGet package - that's it!
- ✅ **Fully Automatic**: Uses ModuleInitializer to run on app startup
- ✅ **Smart**: Reads app name and icon from Package.appxmanifest
- ✅ **Efficient**: Only creates the shortcut once (on first launch)
- ✅ **Safe**: Won't crash your app if shortcut creation fails
- ✅ **Clean**: Provides method to remove the shortcut if needed

## Installation

Install via NuGet Package Manager:

```bash
dotnet add package DesktopShortcutInstaller
```

Or via Package Manager Console:

```powershell
Install-Package DesktopShortcutInstaller
```

**That's it!** No code required. The desktop shortcut will be created automatically on first app launch.

### Manual Mode (Optional - for advanced scenarios)

If you want to control exactly when the shortcut is created, disable auto-initialization:

1. Add to your `.csproj` file:
```xml
<PropertyGroup>
  <DesktopShortcutInstallerAutoInit>false</DesktopShortcutInstallerAutoInit>
</PropertyGroup>
```

2. Call manually when needed:
```csharp
using DesktopShortcutInstaller;

// In your App.xaml.cs or wherever appropriate:
await ShortcutInstaller.InstallDesktopShortcutAsync();
```

## Usage

### Automatic Mode (Recommended - NO CODE!)

Just install the package. The shortcut is created automatically when your app starts.

**Nothing else needed!** 🎉

## How It Works

1. **First Launch**: When your app launches for the first time, the library:
   - Reads the app display name from `Package.appxmanifest`
   - Reads the app icon from `Package.appxmanifest`
   - Creates a shortcut on the user's desktop
   - Saves a flag so it won't create the shortcut again

2. **Subsequent Launches**: The library checks the flag and skips shortcut creation

3. **The Shortcut**: Points to your packaged app using the `shell:AppsFolder` protocol, ensuring it launches correctly

## Advanced Usage

### Manually Remove the Shortcut

If you want to provide users with an option to remove the desktop shortcut:

```csharp
using DesktopShortcutInstaller;

// Remove the desktop shortcut
ShortcutInstaller.UninstallDesktopShortcut();
```

This will:
- Delete the shortcut from the desktop
- Reset the "shortcut created" flag
- Allow the shortcut to be recreated on next app launch

### Check if Running as Packaged App

The library automatically detects if your app is running as a packaged application. If not packaged (e.g., running in development), it will silently skip shortcut creation.

## Requirements

- .NET 6.0 or later
- Windows 10 version 1809 (build 17763) or later
- App must be packaged as MSIX/AppX

## Troubleshooting

### Shortcut Not Created

1. **Check if app is packaged**: The shortcut is only created for packaged apps (MSIX/AppX)
2. **Check permissions**: Ensure the app has permission to write to the desktop
3. **Check debug output**: Error messages are written to the debug output window

### Shortcut Created Multiple Times

The library uses local app settings to track if the shortcut was created. If you:
- Clear app data
- Reinstall the app
- Call `UninstallDesktopShortcut()`

The shortcut will be created again on next launch.

### Custom Icon Not Showing

The library reads the icon from your `Package.appxmanifest`. Make sure:
- The icon file exists in your project
- The path in the manifest is correct
- The icon is included in your package

## Configuration

### Disable Auto-Initialization (Advanced)

If you want to control when the shortcut is created, you can disable auto-initialization by adding this to your `.csproj` file:

```xml
<PropertyGroup>
  <DesktopShortcutInstallerAutoInit>false</DesktopShortcutInstallerAutoInit>
</PropertyGroup>
```

Then call `InstallDesktopShortcutAsync()` manually when needed.

## What Gets Read from Package.appxmanifest

The library automatically extracts:

- **Display Name**: From `<DisplayName>` or `<uap:VisualElements DisplayName="">`
- **Application ID**: From `<Application Id="">`
- **Icon**: From `Square150x150Logo` or `Square44x44Logo` in `<uap:VisualElements>`
- **Package Family Name**: Automatically from the package

## Example Package.appxmanifest

```xml
<?xml version="1.0" encoding="utf-8"?>
<Package xmlns="http://schemas.microsoft.com/appx/manifest/foundation/windows10"
         xmlns:uap="http://schemas.microsoft.com/appx/manifest/uap/windows10">
  
  <Identity Name="MyCompany.MyApp" ... />
  
  <Properties>
    <DisplayName>My Awesome App</DisplayName>
    ...
  </Properties>
  
  <Applications>
    <Application Id="App">
      <uap:VisualElements
        DisplayName="My Awesome App"
        Square150x150Logo="Assets\Square150x150Logo.png"
        Square44x44Logo="Assets\Square44x44Logo.png">
      </uap:VisualElements>
    </Application>
  </Applications>
  
</Package>
```

The shortcut will be named "My Awesome App.lnk" and will use the icon from `Assets\Square150x150Logo.png`.

## License

MIT License - feel free to use in your commercial and open-source projects.

## Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues on GitHub.

## Support

If you encounter any issues or have questions:
1. Check the Troubleshooting section above
2. Review the debug output for error messages
3. Open an issue on GitHub with details about your setup

---

Made with ❤️ for the Windows development community