using DN_Henkel_Vision.Memory;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Reflection;
using Windows.ApplicationModel.Activation;

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
        public static string UserName = string.Empty;

        private ElementTheme _theme = Theme;
        private int _themeIndex = ThemeIndex;
        private bool _setAutoTesting = SetAutoTesting;
        private bool _dataCollection = DataCollection;
        private string _userName = UserName;

        /// <summary>
        /// Initializes a new instance of the <see cref="Settings"/> class.
        /// </summary>
        public Settings()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Handles the event when the user clicks the dropdown for the theme.
        /// </summary>
        /// <param name="sender">The object that fired the event.</param>
        /// <param name="e">The event arguments.</param>
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

        /// <summary>
        /// Handles the event when the user clicks the toggle for the testing redirection.
        /// </summary>
        /// <param name="sender">The object that fired the event.</param>
        /// <param name="e">The event arguments.</param>
        private void TestingSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            _setAutoTesting = TestingSwitch.IsOn;
            SetAutoTesting = _setAutoTesting;

            Memory.Lavender.SaveSettings();
        }

        /// <summary>
        /// Handles the event when the user clicks the toggle for the data collection.
        /// </summary>
        /// <param name="sender">The object that fired the event.</param>
        /// <param name="e">The event arguments.</param>
        private void CollectionSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            _dataCollection = CollectionSwitch.IsOn;
            DataCollection = _dataCollection;

            Memory.Lavender.SaveSettings();
        }

        /// <summary>
        /// Handles the event when the user edits the registrant text box.
        /// </summary>
        /// <param name="sender">The object that fired the event.</param>
        /// <param name="e">The event arguments.</param>
        private void UserText_TextChanged(object sender, TextChangedEventArgs e)
        {
            _userName = UserText.Text;
            UserName = _userName;

            Memory.Lavender.SaveSettings();
        }

        /// <summary>
        /// Checks if the registrant text can be edited.
        /// </summary>
        /// <returns>A boolean value indicating whether the registrant text can be edited.</returns>
        private bool UserEnabled()
        {
            if (Authentification.User == string.Empty || Authentification.User == "Guest")
            {
                return true;
            }

            return false;
        }
    }
}
