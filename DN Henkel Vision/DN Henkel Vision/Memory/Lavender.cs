using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DN_Henkel_Vision.Interface;
using Microsoft.Data.Sqlite;
using Windows.Storage;
using Microsoft.UI.Xaml;
using System.Globalization;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DN_Henkel_Vision.Memory
{
    /// <summary>
    /// The Lavender class storing all information about the database.
    /// </summary>
    internal class Lavender
    {
        private static string s_local = string.Empty;
        private static FileSystemWatcher s_watcher;
        private static SqliteConnection Lavenderbase;

        public static float Time = 0;

        /// <summary>
        /// Function to destroy the database connection.
        /// </summary>
        public static void Invalidate()
        {
            Lavenderbase?.Close();
            Lavenderbase = null;
        }

        /// <summary>
        /// Function to get the database connection.
        /// </summary>
        public static SqliteConnection GetConnection()
        {
            if (Lavenderbase == null)
            {
                s_local = Path.Combine(ApplicationData.Current.LocalFolder.Path, "DN Henkel Vision Data.db");
                Lavenderbase = new SqliteConnection($"Filename={s_local}");
                Lavenderbase.Open();

                string[] tables = new string[] {
                    "CREATE TABLE IF NOT EXISTS orders (id INTEGER PRIMARY KEY, number INTEGER)",
                    "CREATE TABLE IF NOT EXISTS faults (id INTEGER PRIMARY KEY, written TIMESTAMP, number INTEGER, component TEXT, placement TEXT, description TEXT, cause TEXT, classification TEXT, type TEXT, time FLOAT, status TEXT, registrant TEXT)",
                    "CREATE TABLE IF NOT EXISTS users (id INTEGER PRIMARY KEY, username TEXT, fullname TEXT, password TEXT, role TEXT)"
                };

                for (int i = 0; i < tables.Length; i++)
                {
                    SqliteCommand createTable = new SqliteCommand(tables[i], Lavenderbase);
                    createTable.ExecuteReader();
                }
            }

            return Lavenderbase;
        }

        /// <summary>
        /// Function to check if the database is valid.
        /// </summary>
        public static void Validate()
        {
            GetConnection();
        }

        /// <summary>
        /// Loads the registry from the database.
        /// </summary>
        /// <returns>A list of registry entries.</returns>
        public static List<string>[] LoadRegistries()
        {
            List<string> registry = new List<string>();
            List<DateTime> lastDates = new List<DateTime>();

            using (SqliteConnection Lavenderbase = GetConnection())
            {
                Lavenderbase.Open();
                // get unique order numbers from the database table faults and get the last datetime of that number sorted by the datetime from the most recent to the oldest
                SqliteCommand selectCommand = new SqliteCommand("SELECT DISTINCT number, MAX(written) FROM faults GROUP BY number ORDER BY MAX(written) DESC", Lavenderbase);
                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    registry.Add(Interface.Environment.Format(query.GetInt32(0).ToString()));
                    lastDates.Add(query.GetDateTime(1));
                }

                Lavenderbase.Close();
            }

            if (registry.Count <= 9) { return new List<string>[] { registry, registry }; }

            int count = 0;
            int weeks = 0;

            while (count < 9)
            {
                count = 0;
                // count is a number of lastDates that are in a past weeks weeks
                foreach (DateTime date in lastDates)
                {
                    DateTime calcdate = DateTime.Now.AddDays(-7 * weeks);
                    if (date > DateTime.Now.AddDays(-7 * weeks))
                    {
                        count++;
                        // skip the rest of the current loop cycle
                        continue;
                    }

                    break;
                }

                weeks++;
            }

            // round the count to the nearest multiple of 3
            count = (int)Math.Ceiling((double)count / 3) * 3;

            // return count registry entries from the most recent to the oldest and the complete registry
            return new List<string>[] { registry.GetRange(0, count), registry };
        }

        /// <summary>
        /// Loads faults associated with a specific order number.
        /// </summary>
        /// <param name="orderNumber">The order number.</param>
        /// <returns>A list of faults.</returns>
        public static List<Fault> LoadFaults(string orderNumber)
        {
            List<Fault> faults = new List<Fault>();

            using (SqliteConnection Lavenderbase = GetConnection())
            {
                Lavenderbase.Open();
                // Create a command to select all the faults from the database for the order number which do not have the status deleted and execute it.
                SqliteCommand selectCommand = new SqliteCommand($"SELECT * from faults WHERE number='{orderNumber.Replace(" ", string.Empty)}' AND status!='deleted'", Lavenderbase);
                SqliteDataReader query = selectCommand.ExecuteReader();

                bool btet = query.HasRows;

                while (query.Read())
                {
                    Fault fault = new(query.GetString(5))
                    {
                        Index = uint.Parse(query.GetString(0)),
                        Component = query.GetString(3),
                        Placement = query.GetString(4)
                    };

                    fault.ClassIndexes[0] = query.GetInt32(6);
                    fault.ClassIndexes[1] = query.GetInt32(7);
                    fault.ClassIndexes[2] = query.GetInt32(8);

                    fault.Cause = Memory.Classification.LocalCauses[fault.ClassIndexes[0]];
                    fault.Classification = Memory.Classification.LocalClassifications[fault.ClassIndexes[0]][fault.ClassIndexes[1]];
                    fault.Type = Memory.Classification.LocalTypes[Memory.Classification.ClassificationsPointers[fault.ClassIndexes[0]][fault.ClassIndexes[1]]][fault.ClassIndexes[2]];
                
                    faults.Add(fault);
                }

                Lavenderbase.Close();
            }

            return faults.Reverse<Fault>().ToList();
        }
    
        /// <summary>
        /// Appends a fault to the database.
        /// </summary>
        /// <param name="fault">The fault to append.</param>
        /// <param name="orderNumber">The associated order number.</param>
        /// <returns>The ID of the appended fault.</returns>
        public static int AppendFault(Fault fault, string orderNumber)
        {
            int id = 0;

            using (SqliteConnection Lavenderbase = GetConnection())
            {
                Lavenderbase.Open();

                SqliteCommand insertCommand = new SqliteCommand($"INSERT INTO faults (written, number, component, placement, description, cause, classification, type, time, status, registrant) VALUES ('{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}', '{orderNumber.Replace(" ", string.Empty)}', '{fault.Component}', '{fault.Placement}', '{fault.Description}', '{fault.ClassIndexes[0]}', '{fault.ClassIndexes[1]}', '{fault.ClassIndexes[2]}', '{fault.UserTime + fault.MachineTime}', 'complete', '{Settings.UserName}')", Lavenderbase);
                insertCommand.ExecuteNonQuery();

                SqliteCommand selectCommand = new SqliteCommand("SELECT * FROM faults WHERE id = (SELECT MAX(id) FROM faults)", Lavenderbase);
                SqliteDataReader query = selectCommand.ExecuteReader();

                query.Read();
                id = query.GetInt32(0);
                Lavenderbase.Close();
            }

            return id;
        }

        /// <summary>
        /// Deletes a fault from the database.
        /// </summary>
        /// <param name="index">The ID of the fault to delete.</param>
        public static void DeleteFault(uint index)
        {
            using (SqliteConnection Lavenderbase = GetConnection())
            {
                Lavenderbase.Open();
                // Change the status of the fault to deleted.
                SqliteCommand updateCommand = new SqliteCommand($"UPDATE faults SET status='deleted' WHERE id='{index}'", Lavenderbase);
                updateCommand.ExecuteNonQuery();
                Lavenderbase.Close();
            }
        }

        /// <summary>
        /// Loads exports from the database.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="date">The date.</param>
        /// <param name="netstal">A flag indicating whether to include Netstal data.</param>
        /// <param name="inkognito">A flag indicating whether to include Inkognito data.</param>
        /// <returns>A string representing the exports.</returns>
        public static string LoadExports(string user, string date, bool netstal, bool inkognito)
        {
            // Declare a lists used to store the exports and the times.
            List<string> exports = new List<string>();
            List<float> times = new List<float>();

            float exportTime = GetExportTime(netstal);

            // Open the database connection.
            using (SqliteConnection Lavenderbase = GetConnection())
            {
                Lavenderbase.Open();

                string beginning = "38";
                if (netstal) { beginning = "20"; }

                // Create a command to select all the exports from the database where the first two digits of number are equal to beginning
                SqliteCommand selectCommand = new SqliteCommand("SELECT * from faults WHERE status='complete' AND number LIKE '" + beginning + "%'", Lavenderbase);
                SqliteDataReader query = selectCommand.ExecuteReader();

                // Read all the exports and add them to the list with the times.
                while (query.Read())
                {
                    // Get the order number and remove the spaces if Auftrag.
                    string ordernumber = query.GetString(2).Replace(" ", string.Empty);
                    if (ordernumber.StartsWith("38")) { ordernumber = ordernumber.Remove(0, 2); }

                    // Add to the lists.
                    exports.Add($"{ordernumber}\t{query.GetString(4)}\t{query.GetString(5)}\t{query.GetString(3)}\t{Memory.Classification.OriginalCauses[query.GetInt32(6)]}\t{Memory.Classification.OriginalClassifications[query.GetInt32(6)][query.GetInt32(7)]}\t{Memory.Classification.OriginalTypes[Memory.Classification.ClassificationsPointers[query.GetInt32(6)][query.GetInt32(7)]][query.GetInt32(8)]}\t{user}\t{date}");
                    times.Add(query.GetFloat(9));
                }

                // Replace the status of the exports to exported.
                if (!inkognito)
                {
                    SqliteCommand updateCommand = new SqliteCommand("UPDATE faults SET status='exported' WHERE status='complete' AND number LIKE '" + beginning + "%'", Lavenderbase);
                    updateCommand.ExecuteNonQuery();
                }

                // Close the database connection.
                Lavenderbase.Close();
            }

            string joined = string.Join("\r\n", exports);
            

            // Return the exports.
            return Export.Header(netstal, inkognito, Export.Checksum(joined), exportTime) + "\r\n" + joined;
        }

        /// <summary>
        /// Evaluates the total time of all complete faults.
        /// </summary>
        public static void EvaluateTime()
        {
            // Open the database connection.
            using (SqliteConnection Lavenderbase = GetConnection())
            {
                Lavenderbase.Open();

                // Create a command to select all the exports from the database and execute it.
                SqliteCommand selectCommand = new SqliteCommand("SELECT SUM(time) from faults WHERE status='complete'", Lavenderbase);
                SqliteDataReader query = selectCommand.ExecuteReader();

                // Read all the exports and update the time.
                query.Read();

                if (!query.HasRows) { return; }
                if (query.IsDBNull(0)) { return; }

                Time = query.GetFloat(0);
            }   
        }

        public static float GetExportTime(bool netstal)
        {
            string beginning = "38";
            if (netstal) { beginning = "20"; }

            using (SqliteConnection Lavenderbase = GetConnection())
            {
                Lavenderbase.Open();

                // Create a command to select all the exports from the database and execute it.
                SqliteCommand selectCommand = new SqliteCommand("SELECT SUM(time) from faults WHERE status='complete' AND number LIKE '" + beginning + "%'", Lavenderbase);
                SqliteDataReader query = selectCommand.ExecuteReader();

                // Read all the exports and update the time.
                query.Read();

                if (!query.HasRows) { return 0f; }
                if (query.IsDBNull(0)) { return 0f ; }

                return query.GetFloat(0);
            }
        }

        /// <summary>
        /// Evaluates the graph data.
        /// </summary>
        /// <returns>A list of float values representing the graph data.</returns>
        public static List<float> EvaluateGraph()
        {
            List<float> output = new List<float>();

            List<string> dates = new List<string>();

            // Generate the list of the last 36 days. as strings.
            for (int i = 0; i < 36; i++)
            {
                dates.Add(DateTime.Now.AddDays(-i).ToString("yyyy-MM-dd"));
            }

            // Open the database connection.
            using (SqliteConnection Lavenderbase = GetConnection())
            {
                Lavenderbase.Open();

                // Iterate through the dates and get the sum of times for each day.
                foreach (string date in dates)
                {
                    // Create a command to select all the exports from the database and execute it.
                    SqliteCommand selectCommand = new SqliteCommand($"SELECT SUM(time) from faults WHERE DATE(written)='{date}'", Lavenderbase);
                    SqliteDataReader query = selectCommand.ExecuteReader();

                    // Read all the exports and update the time.
                    query.Read();
                    if (query.IsDBNull(0)) { output.Add(0); }
                    else { output.Add(query.GetFloat(0)); }
                }

                Lavenderbase.Close();
            }

            output.Reverse();

            return output;
        }

        /// <summary>
        /// Loads the application settings.
        /// </summary>
        public static void LoadSettings()
        {
            // Load the local settings of the application.
            ApplicationDataContainer settings = Windows.Storage.ApplicationData.Current.LocalSettings;

            Settings.ThemeIndex = settings.Values["theme"] == null ? 2 : (int)settings.Values["theme"];
            Settings.SetAutoTesting = settings.Values["testing"] == null ? false : (bool)settings.Values["testing"];
            Settings.DataCollection = settings.Values["collection"] == null ? false : (bool)settings.Values["collection"];
            Settings.UserName = settings.Values["user"] == null ? string.Empty : (string)settings.Values["user"];

            switch (Settings.ThemeIndex)
            {
                case 0: Settings.Theme = ElementTheme.Light; break;
                case 1: Settings.Theme = ElementTheme.Dark; break;
                default: Settings.Theme = ElementTheme.Default; break;
            }
        }

        /// <summary>
        /// Saves the application settings.
        /// </summary>
        public static void SaveSettings(bool language = false)
        {
            // Load the local settings of the application.
            ApplicationDataContainer settings = Windows.Storage.ApplicationData.Current.LocalSettings;

            settings.Values["theme"] = Settings.ThemeIndex;
            settings.Values["testing"] = Settings.SetAutoTesting;
            settings.Values["collection"] = Settings.DataCollection;
            settings.Values["user"] = Settings.UserName;

            if (language)
            {
                settings.Values["language"] = Settings.Language;
            }
        }

        /// <summary>
        /// Loads the application language.
        /// </summary>
        /// <returns>The application language.</returns>
        public static string LoadLanguage()
        {
            // Load the local settings of the application.
            ApplicationDataContainer settings = Windows.Storage.ApplicationData.Current.LocalSettings;

            // Return the language if it is set, otherwise return the current language.
            return settings.Values["language"] == null ? CultureInfo.CurrentUICulture.Name : (string)settings.Values["language"];
        }

        /// <summary>
        /// Loads the application theme.
        /// </summary>
        /// <returns>The application theme.</returns>
        public static ElementTheme LoadTheme()
        {
            // Load the local settings of the application.
            ApplicationDataContainer settings = Windows.Storage.ApplicationData.Current.LocalSettings;

            // Return the theme if it is set, otherwise return the default theme.
            int theme = settings.Values["theme"] == null ? 2 : (int)settings.Values["theme"];

            switch (theme)
            {
                case 0: return ElementTheme.Light;
                case 1: return ElementTheme.Dark;
                default: return ElementTheme.Default;
            }
        }

        public static void CreateWatcher()
        {
            string directory = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Messages");

            // Create the directory if it does not exist.
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Create a file system watcher for the directory.
            s_watcher = new FileSystemWatcher(directory)
            {
                NotifyFilter = NotifyFilters.LastWrite,
            };

            // Add the event handler for the changed event.
            s_watcher.Changed += MessageReceived;
            s_watcher.EnableRaisingEvents = true;
        }

        private static void MessageReceived(object sender, FileSystemEventArgs e)
        {
            // Open the file and read the contents.
            using (StreamReader reader = new StreamReader(e.FullPath))
            {
                string message = reader.ReadToEnd();

                // Parse the message and append it to the database.
                string[] parts = message.Split('\t');
                // If the first part is equal to "FOCUS", then focus the window.
                if (parts[0] == "FOCUS")
                {
                    // Focus the window.
                    WindowHelper.BringProcessToFront(Process.GetCurrentProcess());
                }
                else if (parts[0] == "OPEN")
                {
                    // Open the exported file (parts[1]) in the default application.
                    if (parts[1] == string.Empty || (!parts[1].EndsWith(".dnfa") && !parts[1].EndsWith(".dnfn"))) { return; }

                    Manager.LaunchingFile = parts[1];

                    // Focus the window.
                    WindowHelper.BringProcessToFront(Process.GetCurrentProcess());

                    // Open the file.
                    Manager.CurrentWindow.DispatcherQueue.TryEnqueue(() => { (Manager.CurrentWindow as Interface.Environment).Workspace.Navigate(typeof(Explorer)); });
                }
            }

            // Delete the file
            File.Delete(e.FullPath);
        }

        public static void SendMessage(string message)
        {
            string directory = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Messages");

            // Create the directory if it does not exist.
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Create a file and write the message to it.
            using (StreamWriter writer = new StreamWriter(Path.Combine(directory, $"{Guid.NewGuid()}.dnw")))
            {
                writer.Write(message);
            }
        }
    }

    public static class WindowHelper
    {
        [DllImport("USER32.DLL")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        public static void BringProcessToFront(Process process)
        {
            IntPtr handle = process.MainWindowHandle;
            if (IsIconic(handle))
            {
                ShowWindow(handle, SW_RESTORE);
            }
            SetForegroundWindow(handle);
        }

        const int SW_RESTORE = 9;

        [DllImport("User32.dll")]
        private static extern bool IsIconic(IntPtr handle);

        [DllImport("User32.dll")]
        private static extern bool ShowWindow(IntPtr handle, int nCmdShow);
    }
}
