using DN_Henkel_Vision.Interface;
using Microsoft.Data.Sqlite;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace DN_Henkel_Vision.Memory
{
    /// <summary>
    /// Class for the authentification of the user.
    /// </summary>
    internal class Authentification : Lavender
    {
        public static string User = string.Empty;
        public static string Name = string.Empty;
        public static string Role = string.Empty;

        /// <summary>
        /// Authenticate the user using a dialog if needed.
        /// </summary>
        /// <param name="cantoken">If the user can be authenticated using a token.</param>
        /// <param name="canclose">If the user can close the dialog.</param>
        /// <returns>True if the user is authenticated, false otherwise.</returns>
        public static async Task<bool> Auth(bool cantoken = true, bool canclose = false)
        {
            // If there is no user, set the user to guest.
            if (!AnyUser()) { User = "Guest"; Name = "Guest"; Role = "Guest"; return true; }
            
            // If there is a token, check if it is valid. If it is, return true.
            if (cantoken && ValidToken()) { return true; }

            // Generate a dialog to ask for the username and password.
            ContentDialog loginDialog = new()
            {
                XamlRoot = Manager.CurrentWindow.Content.XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = "Authentication",
                PrimaryButtonText = "Login",
                SecondaryButtonText = "Close",
                DefaultButton = ContentDialogButton.Primary,
                IsSecondaryButtonEnabled = canclose,
                RequestedTheme = (Manager.CurrentWindow.Content as Grid).RequestedTheme,
                Content = new Login()
            };

            // Disable escape key for the dialog.
            loginDialog.Closing += (ContentDialog sender, ContentDialogClosingEventArgs args) =>
            {
                if (args.Result == ContentDialogResult.None && !canclose) { args.Cancel = true; }
            };

            // Create a custom handler for the login button.
            loginDialog.PrimaryButtonClick += (ContentDialog sender, ContentDialogButtonClickEventArgs args) =>
            {
                // Get the username and password from the dialog.
                string username = (loginDialog.Content as Login).Username.Text;
                string password = (loginDialog.Content as Login).Password.Password;

                // Check if the username and password are valid.
                if (!Validate(username, password))
                {
                    // If not, show an error message.
                    (loginDialog.Content as Login).Error.Visibility = Visibility.Visible;
                    args.Cancel = true;
                }

                // If the the username should be remembered, save it.
                if ((loginDialog.Content as Login).Remember.IsChecked == false) { return; }

                CreateToken(username);
            };

            // Show the dialog.
            ContentDialogResult result = await loginDialog.ShowAsync();

            // If the user clicked the close button, return false.
            if (result == ContentDialogResult.Secondary) { return false; }

            return true;
        }

        /// <summary>
        /// Set the Overlay background for content Dialog
        /// </summary>
        /// <param name="subTree">Content Dialog reference</param>
        public static void SetContentDialogOverlay(UIElement subTree)
        {

            var hostparent = VisualTreeHelper.GetParent(subTree);
            var rect = FindVisualChild<Rectangle>(hostparent);
            rect.Fill = Application.Current.Resources["AcrylicInAppFillColorDefaultBrush"] as AcrylicBrush;
            rect.Opacity = 1;

        }

        /// <summary>
        /// Find the child element from UIContainer
        /// </summary>
        /// <typeparam name="T"> Type</typeparam>
        /// <param name="depObj"> Dependency Reference </param>
        /// <returns></returns>
        public static T FindVisualChild<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        return (T)child;
                    }

                    T childItem = FindVisualChild<T>(child);
                    if (childItem != null) return childItem;
                }
            }
            return null;
        }

        /// <summary>
        /// Check if the token is valid.
        /// </summary>
        /// <returns>True if the token is valid, false otherwise.</returns>
        private static bool ValidToken()
        {
            // Load the local settings of the application.
            ApplicationDataContainer settings = Windows.Storage.ApplicationData.Current.LocalSettings;

            // Load the auth token if not null.
            // Auth token is composited of the username, the token and the expiration date.
            // The token is generated as a SHA512 hash of the id, username, password, and the expiration date.

            Windows.Storage.ApplicationDataCompositeValue savedauth = settings.Values["auth"] as Windows.Storage.ApplicationDataCompositeValue;
            if (savedauth == null) { return false; }

            // Get the expiration date. by parsing the string.
            DateTime expiration = DateTime.Parse(savedauth["expiration"] as string ?? "2023-04-03");

            // If the token is expired, return from the if statement.
            if (expiration < DateTime.Now) { return false; }

            // Get the username.
            string username = savedauth["username"] as string ?? string.Empty;
            
            // Set the username for the case of the auth token being invalid.
            User = username;

            // Get the password.
            string token = savedauth["token"] as string ?? string.Empty;

            // If the username or the password is empty, return from the if statement.
            if (username == string.Empty || token == string.Empty) { return false; }

            // Get the user information from the database.
            using (SqliteConnection Lavenderbase = GetConnection())
            {
                Lavenderbase.Open();

                // Create a command to select all the exports from the database and execute it.
                SqliteCommand selectCommand = new SqliteCommand($"SELECT * from users WHERE username='{username}'", Lavenderbase);
                SqliteDataReader query = selectCommand.ExecuteReader();

                // Read all the exports and update the time.
                query.Read();
                if (!query.HasRows) { Lavenderbase.Close(); return false; }

                // Get the user information.
                string id = query.GetString(0);
                string user = query.GetString(1);
                string fullname = query.GetString(2);
                string password = query.GetString(3);
                string role = query.GetString(4);

                // Generate the token.
                string gentoken = GenerateToken(id, user, password, role, expiration);

                // If the token is valid, return true.
                if (token != gentoken) { Lavenderbase.Close(); return false; }

                User = user;
                Name = fullname;
                Settings.UserName = Abreviate(fullname);
                Role = role;

                Lavenderbase.Close();
            }

            return true;
        }

        /// <summary>
        /// Create a token for the user.
        /// </summary>
        /// <param name="username">The username of the user.</param>
        private static void CreateToken(string username)
        {
            // Load the local settings of the application.
            ApplicationDataContainer settings = Windows.Storage.ApplicationData.Current.LocalSettings;

            // Set the expiration date to the random date from 1 to 31 days.
            DateTime expiration = DateTime.Now.AddDays(new Random().Next(1, 31));

            // Get the user information from the database.

            using (SqliteConnection Lavenderbase = GetConnection())
            {
                Lavenderbase.Open();

                // Create a command to select all the coresponding users from the database and execute it.
                SqliteCommand selectCommand = new SqliteCommand($"SELECT * from users WHERE username='{username}'", Lavenderbase);
                SqliteDataReader query = selectCommand.ExecuteReader();

                query.Read();
                if (!query.HasRows) { Lavenderbase.Close(); return; }

                // Get the user information.
                string id = query.GetString(0);
                string user = query.GetString(1);
                string password = query.GetString(3);
                string role = query.GetString(4);

                // Generate the token.
                string token = GenerateToken(id, user, password, role, expiration);

                // Save the token to the local settings.

                Windows.Storage.ApplicationDataCompositeValue savedauth = new Windows.Storage.ApplicationDataCompositeValue
                {
                    ["username"] = user,
                    ["token"] = token,
                    ["expiration"] = expiration.ToString("yyyy-MM-dd")
                };

                settings.Values["auth"] = savedauth;

                Lavenderbase.Close();
            }
        }

        /// <summary>
        /// Generate a token for the user.
        /// </summary>
        /// <param name="id">The id of the user.</param>
        /// <param name="username">The username of the user.</param>
        /// <param name="password">The password of the user.</param>
        /// <param name="role">The role of the user.</param>
        /// <param name="expiration">The expiration date of the token.</param>
        /// <returns>The generated token.</returns>
        private static string GenerateToken(string id, string username, string password, string role, DateTime expiration)
        {
            // Generate the token.
            string token = id + username + password + role + expiration.ToString("yyyy-MM-dd");

            // Create a SHA512 hash of the token.
            SHA512 sha512 = SHA512.Create();
            byte[] hash = sha512.ComputeHash(Encoding.UTF8.GetBytes(token));

            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Abreviate the name of the user.
        /// </summary>
        /// <param name="text">The name of the user.</param>
        /// <returns>The abreviated name of the user.</returns>
        private static string Abreviate(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder(capacity: normalizedString.Length);

            for (int i = 0; i < normalizedString.Length; i++)
            {
                char c = normalizedString[i];
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            string international = stringBuilder.ToString().Normalize(NormalizationForm.FormC);

            return international.Split(' ').Last() + international.Split(' ').First().First().ToString();
        }

        /// <summary>
        /// Validate the username and password.
        /// </summary>
        /// <param name="username">The username to validate.</param>
        /// <param name="password">The password to validate.</param>
        /// <returns>True if the username and password are valid, false otherwise.</returns>
        private static bool Validate(string username, string password)
        {
            // Open the database connection.
            using (SqliteConnection Lavenderbase = GetConnection())
            {
                Lavenderbase.Open();

                // Create a command to select all the exports from the database and execute it.
                SqliteCommand selectCommand = new SqliteCommand($"SELECT * from users WHERE username='{username}'", Lavenderbase);
                SqliteDataReader query = selectCommand.ExecuteReader();

                // Read all the exports and update the time.
                query.Read();
                if (!query.HasRows) { Lavenderbase.Close(); return false; }

                // Get the user information.
                string user = query.GetString(1);
                string fullname = query.GetString(2);
                string pass = query.GetString(3);
                string role = query.GetString(4);
                
                SHA512 sha512 = SHA512.Create();
                byte[] hash = sha512.ComputeHash(Encoding.UTF8.GetBytes(password));

                if (Convert.ToBase64String(hash) != pass) { Lavenderbase.Close(); return false; }
                    
                User = user;
                Name = fullname;
                Settings.UserName = Abreviate(fullname);
                Role = role;

                Lavenderbase.Close();
            }

            return true;
        }

        /// <summary>
        /// Check if there is any user in the database.
        /// </summary>
        /// <returns>True if there is any user, false otherwise.</returns>
        private static bool AnyUser()
        {
            // Open the database connection.
            using (SqliteConnection Lavenderbase = GetConnection())
            {
                // If the count of the users is 0, return false.
                Lavenderbase.Open();

                // Create a command to select all the exports from the database and execute it.
                SqliteCommand selectCommand = new SqliteCommand($"SELECT * from users", Lavenderbase);
                SqliteDataReader query = selectCommand.ExecuteReader();

                query.Read();
                if (!query.HasRows) { Lavenderbase.Close(); return false; }

                Lavenderbase.Close();
                return true;
            }
        }
    }
}
