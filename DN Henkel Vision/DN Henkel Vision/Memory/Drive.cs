using System;
using System.Collections.Generic;
using System.IO;
using Windows.Storage.Pickers;
using Windows.Storage;
using DN_Henkel_Vision.Interface;
using Microsoft.UI.Xaml;
using System.Globalization;

namespace DN_Henkel_Vision.Memory
{
    /// <summary>
    /// This class represents the file system of the application and provides methods for accessing the file system on the drive.
    /// </summary>
    internal class Drive
    {
        public static readonly string Folder = Windows.ApplicationModel.Package.Current.InstalledPath;
        public static readonly string Data = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "\\DN Henkel Vision";
        public static readonly string Developer = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "\\DN Henkel Vision\\Dev";

        #region File Paths

        internal static readonly string s_regdir =      $"{Data}\\Files\\Registry\\";

        internal static readonly string s_system =      $"{Data}\\Files\\System.dntf";
        internal static readonly string s_settings =    $"{Data}\\Files\\Settings.dntf";
        internal static readonly string s_orders =      $"{Data}\\Files\\Orders\\";
        internal static readonly string s_registry =    $"{Data}\\Files\\Registry\\Registry.dntf";
        internal static readonly string s_exports =     $"{Data}\\Files\\Registry\\Exports.dntf";
        internal static readonly string s_trainees =    $"{Data}\\Files\\Trainee\\";
        internal static readonly string s_language =    $"{Data}\\Files\\Language.dnlf";

        internal static readonly string s_devlog =   $"{Developer}\\Logs\\{DateTime.Now.ToString("ddMMyyHHmm")}.txt";
        
        #endregion

        /// <summary>
        /// This method validates the file system by checking for the existence
        /// of the system file and creates the file system if it does not exist.
        /// </summary>
        public static void Validate()
        {
            if (File.Exists(s_system))
            {
                return;
            }

            Setup.CreateFileSystem();
        }

        public static void Devset()
        {
            if (!Manager.Developer) return;
            
            if (File.Exists(s_devlog)) return;

            if (!Directory.Exists(Developer)) Directory.CreateDirectory(Developer);
            if (!Directory.Exists($"{Developer}\\Logs")) Directory.CreateDirectory($"{Developer}\\Logs");
            Write(s_devlog, "[03.04.23 16:32:21:127] DN Henkel Vision Developer Log \r\n\r\n");
            Log("Application started successfully in developer mode.");
            Log("Application started from location: " + Folder);
        }
        
        /// <summary>
        /// This method loads the registry from the file system.
        /// </summary>
        public static void LoadRegistry()
        {
            Manager.OrdersRegistry.Clear();
            Manager.Users.Clear();
            Manager.Machines.Clear();
            Manager.Exports.Clear();
            Manager.Contents.Clear();
            
            string source = Read(s_registry);

            foreach (string order in source.Split('\n'))
            {
                if (string.IsNullOrEmpty(order)) { continue; }

                string[] parameters = order.Split('\t');
                Manager.OrdersRegistry.Add(parameters[0]);
                Manager.Users.Add(Int32.Parse(parameters[1]));
                Manager.Machines.Add(Int32.Parse(parameters[2]));
                Manager.Exports.Add(Int32.Parse(parameters[3]));
                Manager.Contents.Add(Int32.Parse(parameters[4]));

                if (Int32.Parse(parameters[3]) >= Int32.Parse(parameters[4])) { continue; }

                Export.Unexported.Add(parameters[0]);
            }
        }

        /// <summary>
        /// Saves the data in OrdersRegistry to the registry.
        /// </summary>
        public static void SaveRegistry()
        {
            string output = "";
            for (int i = 0; i < Manager.OrdersRegistry.Count; i++)
            {
                output += $"{Manager.OrdersRegistry[i]}\t{Manager.Users[i]}\t{Manager.Machines[i]}\t{Manager.Exports[i]}\t{Manager.Contents[i]}\n";
            }
            Write(s_registry, output);
        }

        /// <summary>
        /// Saves the list of faults, previews, and pending with given orderNumber to file system.
        /// </summary>
        /// <param name="orderNumber">The unique identifier of the order.</param>
        /// <param name="faults">The list of faults to be saved.</param>
        /// <param name="previews">The list of fault previews to be saved.</param>
        /// <param name="pending">The list of pending faults to be saved.</param>
        public static void SaveFaults(string orderNumber, List<Fault> faults, List<Fault> previews, List<Fault> pending)
        {
            string output = "";

            foreach (Fault fault in faults)
            {
                output += $"{fault.Index}\t{fault}\n";
            }
            output += "Preview:\n";
            foreach (Fault fault in previews)
            {
                output += $"{fault.Index}\t{fault}\n";
            }
            output += "Pending:\n";
            foreach (Fault fault in pending)
            {
                output += $"{fault.Index}\t{fault}\n";
            }

            Write(CreateFaultsPath(orderNumber), output);
        }

        /// <summary>
        /// Create file path for faults.
        /// </summary>
        /// <param name="orderNumber">The order number.</param>
        /// <returns>File path for faults</returns>
        internal static string CreateFaultsPath(string orderNumber)
        {
            return $"{s_orders}{orderNumber.Replace(" ", string.Empty)}.dnff";
        }

        /// <summary>
        /// This method writes to the specific file in the file system.
        /// </summary>
        /// <param name="file">The path to the file to save.</param>
        /// <param name="value">The value to save to the file.</param>
        public static void Write(string file, string value)
        {
            File.WriteAllText(file, value);
        }

        public static void Append(string file, string value)
        {
            File.AppendAllText(file, value);
        }

        /// <summary>
        /// This method reads the specific file from the file system using the
        /// safe method of reading the file (if the file does not exist, it will
        /// be created).
        /// </summary>
        /// <param name="file">The path to the file to read.</param>
        public static string Read(string file)
        {
            Check(file);
            return File.ReadAllText(file);
        }

        /// <summary>
        /// This method checks if the file exists in the file system and if not,
        /// it creates the file.
        /// </summary>
        /// <param name="file">The path to the file to check.</param>
        public static void Check(string file)
        {
            if (!File.Exists(file))
            {
                File.Create(file);
            }
        }

        /// <summary>
        /// Loads export history from file.
        /// </summary>
        public static void LoadExportHistory()
        {
            string[] source = Read(s_exports).Split('\n');

            int[] datea = new int[3];

            datea[0] = Int32.Parse(source[0][0..2]);
            datea[1] = Int32.Parse(source[0][2..4]);
            datea[2] = Int32.Parse(source[0][4..]);

            DateTime date = new(datea[2], datea[1], datea[0]);

            int offset = (int)(DateTime.Now - date).TotalDays;

            int x = 1;
            for (int i = Export.GraphicalCount - offset - 1; i >= 0; i--)
            {
                if (x >= source.Length) { continue; }

                if (source[x] == "\n") { x++; continue; }

                string[] parts = source[x].Split("\t");

                Export.UserService[i] = (float)Int32.Parse(parts[0]) / 60f;
                Export.MachService[i] = (float)Int32.Parse(parts[1]) / 60f;

                Export.UserExports[i] = (float)Int32.Parse(parts[2]) / 60f;
                Export.MachExports[i] = (float)Int32.Parse(parts[3]) / 60f;

                x++;
            }
        }

        /// <summary>
        /// Saves the export history and appends the service user and export times to a file.
        /// </summary>
        public static void SaveExportHistory()
        {
            string output = DateTime.Now.ToString("ddMMyyyy");

            if (Cache.LastDate != DateTime.Now.Date)
            {
                int offset = (int)(DateTime.Now - Cache.LastDate).TotalDays;

                Cache.LastDate = DateTime.Now.Date;

                for (int i = 0; i < Export.GraphicalCount; i++)
                {
                    if (i + offset < Export.GraphicalCount)
                    {
                        Export.UserService[i] = Export.UserService[i + offset];
                        Export.MachService[i] = Export.MachService[i + offset];
                        Export.UserExports[i] = Export.UserExports[i + offset];
                        Export.MachExports[i] = Export.MachExports[i + offset];
                        continue;
                    }

                    Export.UserService[i] = 0f;
                    Export.MachService[i] = 0f;
                    Export.UserExports[i] = 0f;
                    Export.MachExports[i] = 0f;
                }
            }

            for (int i = Export.GraphicalCount - 1; i >= 0; i--)
            {
                output += "\n" + Math.Ceiling(Export.UserService[i] * 60f).ToString();
                output += "\t" + Math.Ceiling(Export.MachService[i] * 60f).ToString();
                output += "\t" + Math.Ceiling(Export.UserExports[i] * 60f).ToString();
                output += "\t" + Math.Ceiling(Export.MachExports[i] * 60f).ToString();
            }

            Write(s_exports, output);
        }

        /// <summary>
        /// Saves export file for given time, username and date. 
        /// </summary>
        /// <param name="time">Time to export</param>
        /// <param name="username">Username of the person exporting</param>
        /// <param name="date">Date of the export</param>
        /// <param name="netstal">Whether Netstal comapny data is being exported or not</param>
        /// <param name="inkognito">Whether the export is anonymous</param>
        public static async void ExportsSave(string username, DateTime date, bool netstal = false, bool inkognito = false)
        {
            string filetype = ".dnfa";
            string filetext = "DN Auftrag Export File (*.dnfa)";

            if (netstal)
            {
                filetype = ".dnfn";
                filetext = "DN Netstal Export File (*.dnfn)";
            }

            if (inkognito)
            {
                filetext = string.Concat(filetext.AsSpan(0, 22), " - Inkognito", filetext.AsSpan(22));
            }
            
            FileSavePicker savePicker = new();

            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(Manager.CurrentWindow);

            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hWnd);

            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            savePicker.FileTypeChoices.Add(filetext, new List<string>() { filetype });
            savePicker.SuggestedFileName = $"DN Export {DateTime.Now.ToString("ddMMyyHHmm")}{filetype}";

            StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                if (!inkognito)
                {
                    //Export.UpdateExportValues(user, mach);
                }

                //string content = await Export.ExportFaults(time, username, date, netstal, inkognito);

                string content = Lavender.LoadExports(username, date.ToString("yyyy-MM-dd"), netstal, inkognito);

                CachedFileManager.DeferUpdates(file);

                using var stream = await file.OpenStreamForWriteAsync();
                using var tw = new StreamWriter(stream);
                tw.Write(content);
            }
        }

        /// <summary>
        /// Loads the settings from the registry.
        /// </summary>
        public static void LoadSettings()
        {
            string source = Read(s_settings);

            char theme = source[0];
            char testing = source[1];
            char collection = source[2];
            string username = source[3..];

            switch (theme)
            {
                case '0':
                    Settings.Theme = ElementTheme.Light;
                    Settings.ThemeIndex = 0;
                    break;
                case '1':
                    Settings.Theme = ElementTheme.Dark;
                    Settings.ThemeIndex = 1;
                    break;
                case '2':
                    Settings.Theme = ElementTheme.Default;
                    Settings.ThemeIndex = 2;
                    break;
            }

            switch (testing)
            {
                case '0':
                    Settings.SetAutoTesting = false;
                    break;
                case '1':
                    Settings.SetAutoTesting = true;
                    break;
            }

            switch (collection)
            {
                case '0':
                    Settings.DataCollection = false;
                    break;
                case '1':
                    Settings.DataCollection = true;
                    break;
            }

            Settings.UserName = username;
        }

        /// <summary>
        /// Saves the application settings to file.
        /// </summary>
        public static void SaveSettings()
        {
            string output = "";

            output += Settings.ThemeIndex.ToString();

            if (Settings.SetAutoTesting)
            {
                output += "1";
            }
            else
            {
                output += "0";
            }

            if (Settings.DataCollection)
            {
                output += "1";
            }
            else
            {
                output += "0";
            }

            output += Settings.UserName;

            Write(s_settings, output);
        }

        /// <summary>
        /// Gets the safe element theme
        /// </summary>
        /// <returns>The element theme</returns>
        public static ElementTheme SafeTheme()
        {
            if (!File.Exists(s_settings)) return ElementTheme.Default;

            string source = Read(s_settings);

            char theme = source[0];

            switch (theme)
            {
                case '0': return ElementTheme.Light;
                case '1': return ElementTheme.Dark;
                default: return ElementTheme.Default;
            }
        }

        public static string SafeLanguage()
        {
            if (!File.Exists(s_language)) return CultureInfo.CurrentUICulture.Name;

            return Read(s_language);
        }

        public static void WriteTrainees()
        {
            string correct = string.Empty;
            string incorrect = string.Empty;
            string today = DateTime.Now.ToString("ddMMyyHHmm");

            foreach (Trainee trainee in Trainer.Correct)
            {
                correct += trainee.ToString() + "\n";
            }

            foreach (Trainee trainee in Trainer.Incorrect)
            {
                incorrect += trainee.ToString() + "\n";
            }

            if (correct != string.Empty)
            {
                Write($"{s_trainees}C{today}.dntf", correct);
            }
            if (incorrect != string.Empty)
            {
                Write($"{s_trainees}N{today}.dntf", incorrect);
            }
        }

        public static void Log(string log)
        {
            if (!Manager.Developer) return;

            Append(s_devlog, $"[{DateTime.Now.ToString("dd.MM.yy HH:mm:ss:fff")}] {log}\r\n");
        }
    }
}
