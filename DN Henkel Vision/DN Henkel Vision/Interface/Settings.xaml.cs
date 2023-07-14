using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Reflection;

namespace DN_Henkel_Vision.Interface
{
    /// <summary>
    /// The settings page and static settings variables of the application.
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

        /// <summary>
        /// Initializes a new instance of the <see cref="Settings"/> class.
        /// </summary>
        public Settings()
        {
            this.InitializeComponent();
        }
    }
}
