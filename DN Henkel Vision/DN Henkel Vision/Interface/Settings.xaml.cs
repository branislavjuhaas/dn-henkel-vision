using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using System.Reflection;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DN_Henkel_Vision.Interface
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Settings : Page
    {
        public string Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public static ElementTheme Theme = ElementTheme.Default;
        public static int ThemeIndex = 2;
        public static string UserName = "JuhasB";
        public static bool SetAutoTesting = true;
        public static bool DataCollection = true;

        private ElementTheme _theme = Theme;
        private int _themeIndex = ThemeIndex;
        private string _userName = UserName;
        private bool _setAutoTesting = SetAutoTesting;
        private bool _dataCollection = DataCollection;

        public Settings()
        {
            this.InitializeComponent();
        }
    }
}
