using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Windowing;
using Microsoft.UI;
using WinRT.Interop;
using DN_Henkel_Vision.Memory;
using System.ComponentModel;

namespace DN_Henkel_Vision.Interface
{
    /// <summary>
    /// The splash screen of the application.
    /// </summary>
    public sealed partial class Splash : Window
    {
        public int StartupWidth = 480;
        public int StartupHeight = 360;

        private Window environment;

        private static Window s_loader;

        /// <summary>
        /// Initializes a new instance of the <see cref="Splash"/> class.
        /// </summary>
        public Splash()
        {           
            s_loader = this;
            
            IntPtr hWnd = WindowNative.GetWindowHandle(this);

            DisplayArea area = DisplayArea.GetFromWindowId(Win32Interop.GetWindowIdFromWindow(hWnd), DisplayAreaFallback.Nearest);

            this.InitializeComponent();
            OverlappedPresenter presenter = GetAppWindowAndPresenter();
            presenter.IsMaximizable = false;
            presenter.IsMinimizable = false;
            presenter.IsResizable = false;
            presenter.SetBorderAndTitleBar(true, false);
            this.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32((area.WorkArea.Width - StartupWidth) / 2, (area.WorkArea.Height - StartupHeight) / 2, StartupWidth, StartupHeight));
        }

        /// <summary>
        /// Gets the app window and presenter.
        /// </summary>
        /// <returns>The overlapped presenter.</returns>
        private OverlappedPresenter GetAppWindowAndPresenter()
        {
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WindowId myWndId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            Microsoft.UI.Windowing.AppWindow _apw = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(myWndId);
            return _apw.Presenter as OverlappedPresenter;
        }

        /// <summary>
        /// Event handler for Grid.Loaded event.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The RoutedEventArgs object that contains the event data.</param>
        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;

            worker.DoWork += Worker_DoWork;

            worker.RunWorkerAsync();
        }

        /// <summary>
        /// Executes the background work.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">An instance of DoWorkEventArgs containing event data.</param>
        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            LoadApplication();

            s_loader.DispatcherQueue.TryEnqueue(() =>
            {
                Environmentate();
            });
        }

        /// <summary>
        /// Creates a new Environment, activates it, closes the current window, and sets the CurrentWindow to the new Environment.
        /// </summary>
        public void Environmentate()
        {
            environment = new Environment();
            environment.Activate();
            this.Close();
            Manager.CurrentWindow = environment;
        }

        /// <summary>
        /// Loads the application by initializing the Manager and Felber.Felber classes.
        /// </summary>
        private void LoadApplication()
        {
            Manager.Initialize();
            Felber.Felber.Initialize();
        }
    }
}
