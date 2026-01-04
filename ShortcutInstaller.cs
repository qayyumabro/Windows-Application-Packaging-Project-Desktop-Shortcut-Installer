using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.ApplicationModel;
using Windows.Storage;

namespace DesktopShortcutInstaller
{
    /// <summary>
    /// Automatically creates desktop shortcuts for packaged Windows applications.
    /// NO CODE REQUIRED - this runs automatically when your app starts!
    /// </summary>
    public static class ShortcutInstaller
    {
        private const string SHORTCUT_CREATED_KEY = "DesktopShortcutCreated";

        /// <summary>
        /// Module initializer - runs automatically when the assembly is loaded.
        /// This creates the desktop shortcut without requiring any code from the user!
        /// </summary>
        [ModuleInitializer]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA2255:The 'ModuleInitializer' attribute should not be used in libraries", Justification = "This is intentional - the library needs to auto-initialize")]
        public static void Initialize()
        {
            // Run on a background thread to avoid blocking app startup
            Task.Run(async () =>
            {
                await InstallDesktopShortcutAsync();
            });
        }

        /// <summary>
        /// Creates a desktop shortcut for the current packaged application if not already created.
        /// This is called automatically by the ModuleInitializer - no manual call needed!
        /// You can also call this manually if you disabled auto-initialization.
        /// </summary>
        public static async Task InstallDesktopShortcutAsync()
        {
            try
            {
                // Check if we're running as a packaged app
                if (!IsPackagedApp())
                {
                    System.Diagnostics.Debug.WriteLine("Not running as a packaged app. Shortcut creation skipped.");
                    return;
                }

                // Check if shortcut was already created
                var localSettings = ApplicationData.Current.LocalSettings;
                if (localSettings.Values.ContainsKey(SHORTCUT_CREATED_KEY))
                {
                    System.Diagnostics.Debug.WriteLine("Desktop shortcut already created.");
                    return;
                }

                // Get app info from manifest
                var appInfo = await GetAppInfoFromManifestAsync();
                if (appInfo == null)
                {
                    System.Diagnostics.Debug.WriteLine("Failed to read app info from manifest.");
                    return;
                }

                // Create the shortcut
                bool success = CreateDesktopShortcut(appInfo);

                if (success)
                {
                    // Mark as created
                    localSettings.Values[SHORTCUT_CREATED_KEY] = true;
                    System.Diagnostics.Debug.WriteLine($"Desktop shortcut created successfully: {appInfo.DisplayName}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating desktop shortcut: {ex.Message}");
                // Don't throw - we don't want to crash the app if shortcut creation fails
            }
        }

        /// <summary>
        /// Removes the desktop shortcut if it exists and resets the creation flag.
        /// </summary>
        public static void UninstallDesktopShortcut()
        {
            try
            {
                if (!IsPackagedApp()) return;

                var appInfo = GetAppInfoFromManifestAsync().GetAwaiter().GetResult();
                if (appInfo == null) return;

                string shortcutPath = GetShortcutPath(appInfo.DisplayName);
                if (File.Exists(shortcutPath))
                {
                    File.Delete(shortcutPath);
                }

                // Reset the flag
                var localSettings = ApplicationData.Current.LocalSettings;
                localSettings.Values.Remove(SHORTCUT_CREATED_KEY);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error removing desktop shortcut: {ex.Message}");
            }
        }

        private static bool IsPackagedApp()
        {
            try
            {
                _ = Package.Current;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static async Task<AppInfo?> GetAppInfoFromManifestAsync()
        {
            try
            {
                var package = Package.Current;
                var installedLocation = package.InstalledLocation;

                // Read the AppxManifest.xml file
                var manifestFile = await installedLocation.GetFileAsync("AppxManifest.xml");
                var manifestText = await FileIO.ReadTextAsync(manifestFile);

                // Parse the XML
                var doc = XDocument.Parse(manifestText);
                XNamespace ns = "http://schemas.microsoft.com/appx/manifest/foundation/windows10";
                XNamespace uapNs = "http://schemas.microsoft.com/appx/manifest/uap/windows10";

                // Get display name - try element first, then attribute
                string displayName;
                var displayNameElement = doc.Descendants(ns + "DisplayName").FirstOrDefault();
                if (displayNameElement != null)
                {
                    displayName = displayNameElement.Value;
                }
                else
                {
                    // Try to get from VisualElements attribute
                    var visualElementsForName = doc.Descendants(uapNs + "VisualElements").FirstOrDefault();
                    displayName = visualElementsForName?.Attribute("DisplayName")?.Value ?? package.DisplayName;
                }

                // Get application ID
                var applicationElement = doc.Descendants(ns + "Application").FirstOrDefault();
                string appId = applicationElement?.Attribute("Id")?.Value ?? "App";

                // Get logo path
                var visualElements = doc.Descendants(uapNs + "VisualElements").FirstOrDefault();
                string? logoPath = visualElements?.Attribute("Square150x150Logo")?.Value
                                ?? visualElements?.Attribute("Square44x44Logo")?.Value;

                // Get the full path to the logo
                string? iconPath = null;
                if (!string.IsNullOrEmpty(logoPath))
                {
                    try
                    {
                        var logoFile = await installedLocation.GetFileAsync(logoPath);
                        iconPath = logoFile.Path;
                    }
                    catch
                    {
                        // Logo file not found, will use default
                    }
                }

                return new AppInfo
                {
                    DisplayName = displayName,
                    ApplicationId = appId,
                    PackageFamilyName = package.Id.FamilyName,
                    IconPath = iconPath
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error reading manifest: {ex.Message}");
                return null;
            }
        }

        private static string GetShortcutPath(string appName)
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            return Path.Combine(desktopPath, $"{appName}.lnk");
        }

        private static bool CreateDesktopShortcut(AppInfo appInfo)
        {
            try
            {
                string shortcutPath = GetShortcutPath(appInfo.DisplayName);

                // Don't overwrite if it already exists
                if (File.Exists(shortcutPath))
                {
                    return true;
                }

                // Create the shell link
                IShellLink link = (IShellLink)new ShellLink();

                // Set the target to the app using shell:AppsFolder
                string target = $"shell:AppsFolder\\{appInfo.PackageFamilyName}!{appInfo.ApplicationId}";
                link.SetPath("explorer.exe");
                link.SetArguments(target);

                // Set description
                link.SetDescription(appInfo.DisplayName);

                // Set icon if available
                if (!string.IsNullOrEmpty(appInfo.IconPath) && File.Exists(appInfo.IconPath))
                {
                    link.SetIconLocation(appInfo.IconPath, 0);
                }

                // Save the shortcut
                IPersistFile file = (IPersistFile)link;
                file.Save(shortcutPath, false);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating shortcut: {ex.Message}");
                return false;
            }
        }

        private class AppInfo
        {
            public string DisplayName { get; set; } = string.Empty;
            public string ApplicationId { get; set; } = string.Empty;
            public string PackageFamilyName { get; set; } = string.Empty;
            public string? IconPath { get; set; }
        }

        #region COM Interop for Shell Link

        [ComImport]
        [Guid("00021401-0000-0000-C000-000000000046")]
        private class ShellLink
        {
        }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("000214F9-0000-0000-C000-000000000046")]
        private interface IShellLink
        {
            void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, IntPtr pfd, int fFlags);
            void GetIDList(out IntPtr ppidl);
            void SetIDList(IntPtr pidl);
            void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
            void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
            void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
            void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
            void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
            void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
            void GetHotkey(out short pwHotkey);
            void SetHotkey(short wHotkey);
            void GetShowCmd(out int piShowCmd);
            void SetShowCmd(int iShowCmd);
            void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
            void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
            void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);
            void Resolve(IntPtr hwnd, int fFlags);
            void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
        }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("0000010b-0000-0000-C000-000000000046")]
        private interface IPersistFile
        {
            void GetClassID(out Guid pClassID);
            void IsDirty();
            void Load([In, MarshalAs(UnmanagedType.LPWStr)] string pszFileName, uint dwMode);
            void Save([In, MarshalAs(UnmanagedType.LPWStr)] string pszFileName, [In, MarshalAs(UnmanagedType.Bool)] bool fRemember);
            void SaveCompleted([In, MarshalAs(UnmanagedType.LPWStr)] string pszFileName);
            void GetCurFile([In, MarshalAs(UnmanagedType.LPWStr)] string ppszFileName);
        }

        #endregion
    }
}