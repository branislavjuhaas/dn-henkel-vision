using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.Storage;
using System.Threading.Tasks;
using Windows.UI.Shell;

namespace DN_Henkel_Vision.Memory
{
    /// <summary>
    /// This class represents the file system of the application and provides methods for accessing the file system on the drive.
    /// </summary>
    internal class Drive
    {
        public static readonly string Folder = Windows.ApplicationModel.Package.Current.InstalledPath;

        #region File Paths

        internal static readonly string s_regdir =   $"{Folder}\\Files\\Registry\\";

        internal static readonly string s_system =      $"{Folder}\\Files\\System.dntf";
        internal static readonly string s_orders =      $"{Folder}\\Files\\Orders\\";
        internal static readonly string s_registry =    $"{Folder}\\Files\\Registry\\Registry.dntf";
        internal static readonly string s_exports =     $"{Folder}\\Files\\Registry\\Exports.dntf";
        
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
                if (order == string.Empty) { continue; }

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

        public static void SaveRegistry()
        {
            string output = "";
            for (int i = 0; i < Manager.OrdersRegistry.Count; i++)
            {
                output += $"{Manager.OrdersRegistry[i]}\t{Manager.Users[i]}\t{Manager.Machines[i]}\t{Manager.Exports[i]}\t{Manager.Contents[i]}\n";
            }
            Write(s_registry, output);
        }

        public static List<Fault>[] LoadFaults(string orderNumber)
        {
            List<Fault>[] output = new List<Fault>[3];
            
            List<Fault> faults = new();
            List<Fault> previews = new();
            List<Fault> pending = new();

            string source = Read(CreateFaultsPath(orderNumber));

            string header = "Normal";

            foreach (string line in source.Split('\n'))
            {
                if (line == "Preview:") { header = "Preview"; continue; }
                if (line == "Pending:") { header = "Pending"; continue; }
                if (line == "") { continue; }
                string[] parts = line.Split('\t');

                Fault fault = new(parts[3]);

                fault.Index = uint.Parse(parts[0]);
                fault.Component = parts[1];
                fault.Placement = parts[2];
                fault.Cause = parts[4];
                fault.Classification = parts[5];
                fault.Type = parts[6];
                fault.ClassIndexes[0] = int.Parse(parts[7]);
                fault.ClassIndexes[1] = int.Parse(parts[8]);
                fault.ClassIndexes[2] = int.Parse(parts[9]);

                if (header == "Normal") { faults.Add(fault); continue; }
                if (header == "Preview") { previews.Add(fault); continue; }
                
                pending.Add(fault);
            }

            output[0] = faults;
            output[1] = previews;
            output[2] = pending;

            return output;
        }

        public static void SaveFaults(string orderNumber, List<Fault> faults, List<Fault> previews, List<Fault> pending)
        {
            string output = "";

            foreach (Fault fault in faults)
            {
                output += $"{fault.Index}\t{fault.ToString()}\n";
            }
            output += "Preview:\n";
            foreach (Fault fault in previews)
            {
                output += $"{fault.Index}\t{fault.ToString()}\n";
            }
            output += "Pending:\n";
            foreach (Fault fault in pending)
            {
                output += $"{fault.Index}\t{fault.ToString()}\n";
            }

            Write(CreateFaultsPath(orderNumber), output);
        }

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

        public static void LoadExportHistory()
        {
            string[] source = Read(s_exports).Split('\n');

            int[] datea = new int[3];

            datea[0] = Int32.Parse(source[0].Substring(0,2));
            datea[1] = Int32.Parse(source[0].Substring(2,2));
            datea[2] = Int32.Parse(source[0].Substring(4));

            DateTime date = new DateTime(datea[2], datea[1], datea[0]);

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

        public static async void ExportsSave(float time, string username, DateTime date, bool netstal = false, bool inkognito = false)
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
                filetext = filetext.Substring(0, 22) + " - Inkognito" + filetext.Substring(22);
            }
            
            FileSavePicker savePicker = new Windows.Storage.Pickers.FileSavePicker();

            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(Manager.CurrentWindow);

            // Initialize the file picker with the window handle (HWND).
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hWnd);

            // Set options for your file picker
            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add(filetext, new List<string>() { filetype });

            // Open the picker for the user to pick a file
            StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                if (!inkognito)
                {
                    float user = time;

                    if (user > (Export.OrdersTime(Export.Unexported.ToArray(), false) + Export.OrdersTime(Export.Unexported.ToArray(), false, true)) / 60f)
                    {
                        user = (Export.OrdersTime(Export.Unexported.ToArray(), false) + Export.OrdersTime(Export.Unexported.ToArray(), false, true)) / 60f;
                    }

                    float mach = time - user;

                    Export.UpdateExportValues(user, mach);
                }

                string content = await Export.ExportFaults(time, username, date, netstal, inkognito);

                // Prevent updates to the remote version of the file until we finish making changes and call CompleteUpdatesAsync.
                CachedFileManager.DeferUpdates(file);

                using (var stream = await file.OpenStreamForWriteAsync())
                {
                    using (var tw = new StreamWriter(stream))
                    {
                        tw.Write(content);
                    }
                }
            }
        }
    }
}
