using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System.Runtime.InteropServices;
using Windows.ApplicationModel.Core;
using Microsoft.UI.Windowing;
using Microsoft.UI;
using Windows.Graphics;
using WinRT.Interop;
using Windows.UI.WindowManagement;
using System.Drawing;
using DN_Henkel_Vision.Memory;
using System.ComponentModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DN_Henkel_Vision.Interface
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Splash : Window
    {
        public int StartupWidth = 480;
        public int StartupHeight = 360;

        private Window environment;

        private static Window s_loader;
        
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

        private OverlappedPresenter GetAppWindowAndPresenter()
        {
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WindowId myWndId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            Microsoft.UI.Windowing.AppWindow _apw = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(myWndId);
            return _apw.Presenter as OverlappedPresenter;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;

            worker.DoWork += Worker_DoWork;

            worker.RunWorkerAsync();
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            LoadApplication();

            s_loader.DispatcherQueue.TryEnqueue(() =>
            {
                Environmentate();
            });
        }

        public void Environmentate()
        {
            environment = new Environment();
            environment.Activate();
            this.Close();
            Manager.CurrentWindow = environment;
        }

        private void LoadApplication()
        {
            Manager.Initialize();
            Felber.Felber.Initialize();
        }
    }
}
