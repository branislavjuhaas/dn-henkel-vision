using DN_Henkel_Vision.Memory;
using DN_Henkel_Vision.Interface;
using Microsoft.UI.Xaml;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;

// Copyright(c) DN Foundation and Branislav Juhás.
// Trade secret of DN Foundation.

namespace DN_Henkel_Vision
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            Manager.Initialize();
            this.InitializeComponent();
            Felber.Felber.Initialize();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            environment = new Interface.Environment();
            environment.Activate();
        }

        /// <summary>
        /// Initializes the environment of the application. This variable stores the main application's window.
        /// </summary>
        private Window environment;
    }
}
