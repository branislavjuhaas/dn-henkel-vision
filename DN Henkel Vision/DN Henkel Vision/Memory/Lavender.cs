using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DN_Henkel_Vision.Interface;
using Microsoft.Data.Sqlite;
using Microsoft.Win32;
using Windows.Storage;
using Microsoft.UI.Xaml;
using System.Globalization;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace DN_Henkel_Vision.Memory
{
    public static class Lavender
    {
        private static string s_local = "";
        private static SqliteConnection Lavenderbase;

        public static float Time = 0;

        public static void Invalidate()
        {
            Lavenderbase?.Close();
            Lavenderbase = null;
        }

        public static SqliteConnection GetConnection()
        {
            if (Lavenderbase == null)
            {
                s_local = Path.Combine(ApplicationData.Current.LocalFolder.Path, "DN Henkel Vision Data.db");
                Lavenderbase = new SqliteConnection($"Filename={s_local}");
                Lavenderbase.Open();

                string[] tables = new string[] {
                    "CREATE TABLE IF NOT EXISTS orders (id INTEGER PRIMARY KEY, number INTEGER)",
                    "CREATE TABLE IF NOT EXISTS faults (id INTEGER PRIMARY KEY, number INTEGER, component TEXT, placement TEXT, description TEXT, cause TEXT, classification TEXT, type TEXT, user FLOAT, machine FLOAT, status TEXT)",
                    "CREATE TABLE IF NOT EXISTS users (id INTEGER PRIMARY KEY, username TEXT, fullname TEXT, password TEXT, role TEXT)",
                    "CREATE TABLE IF NOT EXISTS graph (id INTEGER PRIMARY KEY, datafrom DATE, userservice TIME, machineservice TIME, userexports TIME, machineexports TIME)",
                    "CREATE TABLE IF NOT EXISTS settings (id INTEGER PRIMARY KEY, name TEXT, value TEXT)"
                };

                for (int i = 0; i < tables.Length; i++)
                {
                    SqliteCommand createTable = new SqliteCommand(tables[i], Lavenderbase);
                    createTable.ExecuteReader();
                }
            }

            return Lavenderbase;
        }

        public static void Validate()
        {
            GetConnection();
        }

        public static List<string> LoadRegistry()
        {
            List<string> registry = new List<string>();

            using (SqliteConnection Lavenderbase = GetConnection())
            {
                Lavenderbase.Open();
                SqliteCommand selectCommand = new SqliteCommand("SELECT * from orders", Lavenderbase);
                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    registry.Add(query.GetString(1));
                }

                Lavenderbase.Close();
            }

            return registry;
        }

        public static List<Fault> LoadFaults(string orderNumber)
        {
            List<Fault> faults = new List<Fault>();

            using (SqliteConnection Lavenderbase = GetConnection())
            {
                Lavenderbase.Open();
                SqliteCommand selectCommand = new SqliteCommand($"SELECT * from faults WHERE number='{orderNumber.Replace(" ", "")}'", Lavenderbase);
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
    
        public static int AppendFault(Fault fault, string orderNumber)
        {
            int id = 0;

            using (SqliteConnection Lavenderbase = GetConnection())
            {
                Lavenderbase.Open();

                SqliteCommand insertCommand = new SqliteCommand($"INSERT INTO faults (number, component, placement, description, cause, classification, type, time, status, registrant) VALUES ('{orderNumber.Replace(" ", "")}', '{fault.Component}', '{fault.Placement}', '{fault.Description}', '{fault.ClassIndexes[0]}', '{fault.ClassIndexes[1]}', '{fault.ClassIndexes[2]}', '{fault.UserTime + fault.MachineTime}', 'complete', 'Branislav Juhás')", Lavenderbase);
                insertCommand.ExecuteNonQuery();

                SqliteCommand selectCommand = new SqliteCommand("SELECT * FROM faults WHERE id = (SELECT MAX(id) FROM faults)", Lavenderbase);
                SqliteDataReader query = selectCommand.ExecuteReader();

                query.Read();
                id = query.GetInt32(0);
                Lavenderbase.Close();
            }

            return id;
        }

        public static void DeleteFault(uint index)
        {
            using (SqliteConnection Lavenderbase = GetConnection())
            {
                Lavenderbase.Open();
                SqliteCommand deleteCommand = new SqliteCommand($"DELETE FROM faults WHERE id='{index}'", Lavenderbase);
                deleteCommand.ExecuteReader();
                Lavenderbase.Close();
            }
        }

        public static string LoadExports(string user, string date, bool netstal, bool inkognito, int count = -1)
        {
            // Declare a lists used to store the exports and the times.
            List<string> exports = new List<string>();
            List<float> times = new List<float>();

            // Open the database connection.
            using (SqliteConnection Lavenderbase = GetConnection())
            {
                Lavenderbase.Open();

                // Create a command to select all the exports from the database and execute it.
                SqliteCommand selectCommand = new SqliteCommand("SELECT * from faults WHERE status='complete'", Lavenderbase);
                SqliteDataReader query = selectCommand.ExecuteReader();

                // Read all the exports and add them to the list with the times.
                while (query.Read())
                {
                    // Get the order number and remove the spaces if Auftrag.
                    string ordernumber = query.GetString(2).Replace(" ", "");
                    if (ordernumber.StartsWith("38")) { ordernumber = ordernumber.Remove(0, 2); }

                    // Add to the lists.
                    exports.Add($"{ordernumber}\t{query.GetString(4)}\t{query.GetString(5)}\t{query.GetString(4)}\t{Memory.Classification.OriginalCauses[query.GetInt32(6)]}\t{Memory.Classification.OriginalClassifications[query.GetInt32(6)][query.GetInt32(7)]}\t{Memory.Classification.OriginalTypes[Memory.Classification.ClassificationsPointers[query.GetInt32(6)][query.GetInt32(7)]][query.GetInt32(8)]}\t{user}\t{date}");
                    times.Add(query.GetFloat(9));
                }
            }

            // If the count is -1, return all the exports.
            if (count == -1) { return Export.Header(netstal, inkognito) + "\r\n" + string.Join("\n", exports); }

            // Make a sum of all the times
            float sum = times.Sum();

            // Now chose the count exports which's sum of times is closest to the sum of all the times divided by the count.
            float averageTime = sum / count;
            exports = exports.OrderBy(s => Math.Abs(times[exports.IndexOf(s)] - averageTime)).Take(count).ToList();

            return Export.Header(netstal, inkognito) + "\r\n" + string.Join("\n", exports);
        }

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
                Time = query.GetFloat(0);
            }   
        }

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

        public static void CreateOrder(string order)
        {
            // Open the database connection.
            using (SqliteConnection Lavenderbase = GetConnection())
            {
                Lavenderbase.Open();

                // Create a command to select all the exports from the database and execute it.
                SqliteCommand insertCommand = new SqliteCommand($"INSERT INTO orders (number) VALUES ('{order.Replace(" ", "")}')", Lavenderbase);
                insertCommand.ExecuteNonQuery();

                Lavenderbase.Close();
            }
        }

        public static void LoadSettings()
        {
            // Load the local settings of the application.
            ApplicationDataContainer settings = Windows.Storage.ApplicationData.Current.LocalSettings;

            Settings.ThemeIndex = settings.Values["theme"] == null ? 2 : (int)settings.Values["theme"];
            Settings.SetAutoTesting = settings.Values["testing"] == null ? false : (bool)settings.Values["testing"];
            Settings.DataCollection = settings.Values["collection"] == null ? false : (bool)settings.Values["collection"];

            switch (Settings.ThemeIndex)
            {
                case 0: Settings.Theme = ElementTheme.Light; break;
                case 1: Settings.Theme = ElementTheme.Dark; break;
                default: Settings.Theme = ElementTheme.Default; break;
            }
        }

        public static void SaveSettings()
        {
            // Load the local settings of the application.
            ApplicationDataContainer settings = Windows.Storage.ApplicationData.Current.LocalSettings;

            settings.Values["theme"] = Settings.ThemeIndex;
            settings.Values["testing"] = Settings.SetAutoTesting;
            settings.Values["collection"] = Settings.DataCollection;
        }

        public static string LoadLanguage()
        {
            // Load the local settings of the application.
            ApplicationDataContainer settings = Windows.Storage.ApplicationData.Current.LocalSettings;

            // Return the language if it is set, otherwise return the current language.
            return settings.Values["language"] == null ? CultureInfo.CurrentUICulture.Name : (string)settings.Values["language"];
        }

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
    }
}
