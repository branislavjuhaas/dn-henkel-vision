using DN_Henkel_Vision.Memory;
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
        public static bool SetAutoTesting = true;
        public static bool DataCollection = true;

        private ElementTheme _theme = Theme;
        private int _themeIndex = ThemeIndex;
        private bool _setAutoTesting = SetAutoTesting;
        private bool _dataCollection = DataCollection;

        /// <summary>
        /// Initializes a new instance of the <see cref="Settings"/> class.
        /// </summary>
        public Settings()
        {
            this.InitializeComponent();
        }

        private void ThemeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _themeIndex = ThemeCombo.SelectedIndex;
            ThemeIndex = _themeIndex;

            switch (_themeIndex)
            {
                case 0:
                    _theme = ElementTheme.Light;
                    break;
                case 1:
                    _theme = ElementTheme.Dark;
                    break;
                case 2:
                    _theme = ElementTheme.Default;
                    break;
            }

            Theme = _theme;

            (Manager.CurrentWindow as Environment).EnvironmentalGrid.RequestedTheme = Theme;

            Memory.Lavender.SaveSettings();
        }

        private void TestingSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            _setAutoTesting = TestingSwitch.IsOn;
            SetAutoTesting = _setAutoTesting;

            Memory.Lavender.SaveSettings();
        }

        private void CollectionSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            _dataCollection = CollectionSwitch.IsOn;
            DataCollection = _dataCollection;

            Memory.Lavender.SaveSettings();
        }
    }
}
