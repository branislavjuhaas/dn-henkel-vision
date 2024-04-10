using DN_Henkel_Vision.Memory;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
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
        public static string Language = Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride;

        private ElementTheme _theme = Theme;
        private int _themeIndex = ThemeIndex;
        private bool _setAutoTesting = SetAutoTesting;
        private bool _dataCollection = DataCollection;
        private string _userName = UserName;
        private Visibility _languageVisibility = AdminVisibility();
        private int _languageIndex = LanguageIndex();

        /// <summary>
        /// Initializes a new instance of the <see cref="Settings"/> class.
        /// </summary>
        public Settings()
        {
            this.InitializeComponent();
        }

        private static Visibility AdminVisibility()
        {
            return Manager.DevText == "Administrator" ? Visibility.Visible : Visibility.Collapsed;
        }

        private static int LanguageIndex()
        {
            int index = 0;

            switch (Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride)
            {
                case "en-US":
                    index = 0;
                    break;
                case "sk-SK":
                    index = 1;
                    break;
                case "de-DE":
                    index = 2;
                    break;
            }

            return index;
        }

        /// <summary>
        /// Handles the event when the user clicks the dropdown for the theme.
        /// </summary>
        /// <param name="sender">The object that fired the event.</param>
        /// <param name="e">The event arguments.</param>
        private void ThemeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ThemeCombo.SelectedIndex == -1) { return; }
            
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

        private void LanguageCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LanguageCombo.SelectedIndex == -1) { return; }

            _languageIndex = LanguageCombo.SelectedIndex;

            switch (_languageIndex)
            {
                case 0:
                    Language = "en-US";
                    break;
                case 1:
                    Language = "sk-SK";
                    break;
                case 2:
                    Language = "de-DE";
                    break;
                default:
                    Language = "en-US";
                    break;
            }

            Memory.Lavender.SaveSettings(true);
        }
    }
}
