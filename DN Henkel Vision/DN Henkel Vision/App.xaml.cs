using DN_Henkel_Vision.Memory;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using System;
using System.Threading;
using Windows.ApplicationModel.Activation;
using Windows.System;
using Windows.UI.Core;
using Windows.Foundation;
using Microsoft.Windows.AppLifecycle;

namespace DN_Henkel_Vision
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App
    {
        private static readonly Mutex mutex = new(true, "{b2a44102-131d-4742-95a6-f3e80c85d275}");
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = Memory.Lavender.LoadLanguage();

            this.InitializeComponent();
            if (!mutex.WaitOne(TimeSpan.Zero, true))
            {
                System.Environment.Exit(0);
            }
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            AppActivationArguments activatedEventArgs = Microsoft.Windows.AppLifecycle.AppInstance.GetCurrent().GetActivatedEventArgs();
            if (activatedEventArgs.Kind == Microsoft.Windows.AppLifecycle.ExtendedActivationKind.File)
            {
                Manager.LaunchingFile = (activatedEventArgs.Data as Windows.ApplicationModel.Activation.IFileActivatedEventArgs).Files[0].Path;
            }
            
            splash = new Interface.Splash();
            splash.Activate();
        }

        /// <summary>
        /// Returns a boolean indicating whether the Shift key is currently pressed.
        /// </summary>
        /// <returns>A boolean value indicating whether the Shift key is currently pressed.</returns>
        private static bool IsShift()
        {
            CoreVirtualKeyStates states = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift);

            return (states & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
        }   

        /// <summary>
        /// Initializes the splash screen of the application. This variable stores the main application's splash screen window.
        /// </summary>
        private Window splash;
    }
}
