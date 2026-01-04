// Example App.xaml.cs for WinUI 3 Application
// NO CODE CHANGES NEEDED! Just install the DesktopShortcutInstaller NuGet package
// and the desktop shortcut will be created automatically on first launch.

using Microsoft.UI.Xaml;

namespace ExampleWinUI3App
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window m_window;

        /// <summary>
        /// Initializes the singleton application object.
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            // ============================================
            // DESKTOP SHORTCUT INSTALLER
            // ============================================
            // NO CODE NEEDED! The DesktopShortcutInstaller
            // NuGet package automatically creates a desktop
            // shortcut on first launch using ModuleInitializer.
            // 
            // Just install the package and you're done! 🎉
            // ============================================

            // If you want manual control (advanced):
            // 1. Set <DesktopShortcutInstallerAutoInit>false</DesktopShortcutInstallerAutoInit> in .csproj
            // 2. Uncomment the following lines:
            //
            // using DesktopShortcutInstaller;
            // await ShortcutInstaller.InstallDesktopShortcutAsync();

            m_window = new MainWindow();
            m_window.Activate();
        }
    }
}
