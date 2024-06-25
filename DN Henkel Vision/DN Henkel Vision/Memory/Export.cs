using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.Text.RegularExpressions;
using System.IO;
using Windows.Storage.Pickers;
using Windows.Storage;
using DN_Henkel_Vision.Interface;
using Microsoft.IdentityModel.Tokens;

namespace DN_Henkel_Vision.Memory
{
    /// <summary>
    /// Class containing the environment variables and methods.
    /// </summary>
    internal class Export
    {
        private static readonly string s_header = "SZSK2ZZ69JA93CDNHENKELVISION84SAW4K7DNHENKELVISIONXNESVGSYDNHENKELVISIONFMRR4YD5DNHENKELVISION3VY0RS\r\n"
                                       + "LQZF15    N85J1DNHENKELVISIONJXG7Z5PEDNHENKELVISIONPXBXJCHRDNHENKELVISION50PUYSK7DNHENKELVISIONTU6EP\r\n"
                                       + "S1PX  6N  02W7Q3DNHENKELVISION2YCSP43WDNHENKELVISIONCFFQAQT2DNHENKELVISIONTG8W2H03DNHENKELVISION7UTJ\r\n"
                                       + "4C9Y  BL    R50EUDNHENKELVISIONCE6WZVLGDNHENKELVISION52FPJS6PDNHENKELVISION1K3U7H63DNHENKELVISIONWYA\r\n"
                                       + "QLNF  16X1  V1ALR7DNHENKELVISIONTJH0LHPSDNHENKELVISIONQN0USUBXDNHENKELVISIONK7AX8B21DNHENKELVISION4Z\r\n"
                                       + "Y3  F0LGYV4LFQ0QTGQDNHENKELVISION3QHAW00HDNHENKELVISIONEJGLJTWJDNHENKELVISION7CB9J59GDNHENKELVISIONH\r\n";

        private static readonly string s_adnetstal = "NWEWT0SCTTAEL1PB9NACDNHENKELVISIONNJCEP8XHDNHENKELVISIONW2P1R7QTDNHENKELVISIONR77KK0EBDNHENKELVISION";
        private static readonly string s_adauftrag = "AWUWF0TCRTAEG1PB9NACDNHENKELVISIONNJCEP8XHDNHENKELVISIONW2P1R7QTDNHENKELVISIONR77KK0EBDNHENKELVISION";

        /// <summary>
        /// Returns the header string for exports.
        /// </summary>
        /// <param name="netstal">Indicates whether export is for Netstal machine or not.</param>
        /// <param name="inkognito">Indicates whether to show the real project name or not.</param>
        /// <returns>The header string for exports.</returns>
        public static string Header(bool netstal = false, bool inkognito = false, string checksum = "", float exportTime = 0f)
        {
            string pseudoheader = s_header.Replace("DNHENKELVISION", "##############");

            List<int> indexes = new();
            List<char> values = new();

            int ri = Random.Shared.Next(65, 90);
            int rii = Random.Shared.Next(65, 90);

            int seed = ri * rii;

            char[] date = DateTime.Now.ToString("ddMMyy").ToCharArray();
            char[] user = Regex.Replace(System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToUpper(), "[^A-Z0-9]", "").ToCharArray();
            char[] version = Assembly.GetExecutingAssembly().GetName().Version.ToString().Replace('.', 'N').ToCharArray();
            char[] time = (exportTime / 60f).ToString("0.00").Replace(".", "").PadLeft(5, '0').ToCharArray();
            

            HeaderReplace(ref pseudoheader, ref indexes, ref values, 77, (char)ri);
            HeaderReplace(ref pseudoheader, ref indexes, ref values, 78, (char)rii);
            
            
            HeaderReplace(ref pseudoheader, ref indexes, ref values, Sequence(ref seed, 10, 605), date[0]);
            HeaderReplace(ref pseudoheader, ref indexes, ref values, Sequence(ref seed, 10, 605), date[1]);
            HeaderReplace(ref pseudoheader, ref indexes, ref values, Sequence(ref seed, 10, 605), date[2]);
            HeaderReplace(ref pseudoheader, ref indexes, ref values, Sequence(ref seed, 10, 605), date[3]);
            HeaderReplace(ref pseudoheader, ref indexes, ref values, Sequence(ref seed, 10, 605), date[4]);
            HeaderReplace(ref pseudoheader, ref indexes, ref values, Sequence(ref seed, 10, 605), date[5]);

            HeaderReplace(ref pseudoheader, ref indexes, ref values, Sequence(ref seed, 10, 605), (char)(user.Length + 65));

            foreach (char userchar in user)
            {
                HeaderReplace(ref pseudoheader, ref indexes, ref values, Sequence(ref seed, 10, 605), userchar);
            }

            HeaderReplace(ref pseudoheader, ref indexes, ref values, Sequence(ref seed, 10, 605), (char)(version.Length + 65));

            foreach (char versionchar in version)
            {
                HeaderReplace(ref pseudoheader, ref indexes, ref values, Sequence(ref seed, 10, 605), versionchar);
            }

            if (inkognito)
            {
                HeaderReplace(ref pseudoheader, ref indexes, ref values, Sequence(ref seed, 10, 605), 'I');
            }
            else
            {
                HeaderReplace(ref pseudoheader, ref indexes, ref values, Sequence(ref seed, 10, 605), 'P');
            }

            HeaderReplace(ref pseudoheader, ref indexes, ref values, Sequence(ref seed, 10, 605), time[0]);
            HeaderReplace(ref pseudoheader, ref indexes, ref values, Sequence(ref seed, 10, 605), time[1]);
            HeaderReplace(ref pseudoheader, ref indexes, ref values, Sequence(ref seed, 10, 605), time[2]);
            HeaderReplace(ref pseudoheader, ref indexes, ref values, Sequence(ref seed, 10, 605), time[3]);
            HeaderReplace(ref pseudoheader, ref indexes, ref values, Sequence(ref seed, 10, 605), time[4]);

            // Write a hash
            if (!checksum.IsNullOrEmpty())
            {
                HeaderReplace(ref pseudoheader, ref indexes, ref values, Sequence(ref seed, 10, 605), checksum[0]);
                HeaderReplace(ref pseudoheader, ref indexes, ref values, Sequence(ref seed, 10, 605), checksum[1]);
                HeaderReplace(ref pseudoheader, ref indexes, ref values, Sequence(ref seed, 10, 605), checksum[2]);
            }

            char[] pseudo = pseudoheader.Replace("##############", "DNHENKELVISION").ToCharArray();

            for (int i = 0; i < indexes.Count; i++)
            {
                pseudo[indexes[i]] = values[i];
            }

            if (netstal)
            {
                return new string(pseudo) + s_adnetstal;
            }

            return new string(pseudo) + s_adauftrag;
        }

        public static string[] DecodeHeader(string header)
        {
            string series = "Auftrag";
            string date = string.Empty;
            string user = string.Empty;
            string version = string.Empty;
            string time = "N/A";
            string checksum = string.Empty;

            // if the seventh line is equal to the netstal header then the export is for netstal
            if (header.Split('\r')[6].Contains(s_adnetstal)) { series = "Netstal"; }

            // Remove the last line
            header = header.Substring(0, header.LastIndexOf('\r'));

            // Convert header to char array
            char[] headerArray = header.Replace("DNHENKELVISION", "##############").ToCharArray();

            int ri = HeaderRead(ref headerArray, 77);
            int rii = HeaderRead(ref headerArray, 78);

            int seed = ri * rii;

            // Get date
            for (int i = 0; i < 6; i++)
            {
                date += HeaderRead(ref headerArray, Sequence(ref seed, 10, 605));
            }

            // Convert date which is now in format ddMMyy to dd. mm. yyyy
            date = date.Substring(0, 2).TrimStart('0') + ". " + date.Substring(2, 2).TrimStart('0') + ". " + "20" + date.Substring(4, 2);

            // Get username
            int userlength = (int)HeaderRead(ref headerArray, Sequence(ref seed, 10, 605)) - 65;

            for (int i = 0; i < userlength; i++)
            {
                user += HeaderRead(ref headerArray, Sequence(ref seed, 10, 605));
            }

            // Get version
            int versionLength = (int)HeaderRead(ref headerArray, Sequence(ref seed, 10, 605)) - 65;
            for (int i = 0; i < versionLength; i++)
            {
                version += HeaderRead(ref headerArray, Sequence(ref seed, 10, 605));
            }

            version = version.Replace('N', '.');

            // Get mode
            series = HeaderRead(ref headerArray, Sequence(ref seed, 10, 605)) == 'I' ? series + " Inkognito" : series;

            // If the build of the version can be read and it is >= 2007 then read the time (build are digits after the last dot in the version
            if (int.TryParse(version.Split('.').Last(), out int build) && build >= 2027)
            {
                time = string.Empty;
                for (int i = 0; i < 5; i++)
                {
                    time += HeaderRead(ref headerArray, Sequence(ref seed, 10, 605));
                }
                // Conver time string from 01234 to 012.34 and then remove all paddin zeros from the beginning
                time = time.Insert(3, ".").TrimStart('0') + " " + Windows.ApplicationModel.Resources.ResourceLoader.GetStringForReference(new Uri("ms-resource:S_Hours"));

                if (time.StartsWith("."))
                {
                    time = "0" + time;
                }

                // Get checksum
                for (int i = 0; i < 3; i++)
                {
                    checksum += HeaderRead(ref headerArray, Sequence(ref seed, 10, 605));
                }
            }

            return new string[] { series, date, user, version, time, checksum };
        }

        /// <summary>
        /// Replaces character at given index with given character in the provided pseudoheader string. 
        /// Adds the replaced index and value to their respective lists. 
        /// </summary>
        /// <param name="pseudoheader">The string to replace a character in.</param>
        /// <param name="replacedindex">The list of replaced indices.</param>
        /// <param name="replacedvalue">The list of replacement characters.</param>
        /// <param name="index">The index of the character to replace.</param>
        /// <param name="replacement">The character to replace with.</param>
        public static void HeaderReplace( ref string pseudoheader, ref List<int> replacedindex, ref List<char> replacedvalue, int index, char replacement )
        {
            while (pseudoheader.ToCharArray()[index] == '#' || pseudoheader.ToCharArray()[index] == ' ' || pseudoheader.ToCharArray()[index] == '$' || pseudoheader.ToCharArray()[index] == '\r' || pseudoheader.ToCharArray()[index] == '\n') 
            {
                index += 2; 
                if (index >= pseudoheader.Length) { index = 0; }
            }

            replacedindex.Add(index);
            replacedvalue.Add(replacement);

            char[] pseudo = pseudoheader.ToCharArray();
            pseudo[index] = '$';
            pseudoheader = new string(pseudo);
        }

        public static char HeaderRead( ref char[] pseudoheader, int index )
        {
            while (pseudoheader[index] == '#' || pseudoheader[index] == ' ' || pseudoheader[index] == '$' || pseudoheader[index] == '\r' || pseudoheader[index] == '\n')
            {
                index += 2;
                if (index >= pseudoheader.Length) { index = 0; }
            }

            char value = pseudoheader[index];
            pseudoheader[index] = '$';
            return value;
        }

        public static string Checksum(string checker)
        {
            if (checker.IsNullOrEmpty()) { return string.Empty; }

            char[] chars = checker.ToCharArray();

            int checksum = 0;

            for (int i = 0; i < chars.Length; i++)
            {
                checksum += chars[i] ^ i;
                if (checksum > 15625) { checksum = checksum % 15626; }
            }

            string checks = "";

            for (int i = 0; i < 3; i++)
            {
                checks += (char)(65 + checksum % 26);
                checksum = checksum / 26;
            }

            return checks;
        }

        /// <summary>
        /// Generates a random integer value within a specified range and updates the seed.
        /// </summary>
        /// <param name="seed">The seed value to update.</param>
        /// <param name="min">The minimum value of the range, defaults to the minimum integer value.</param>
        /// <param name="max">The maximum value of the range, defaults to the maximum integer value.</param>
        /// <returns>A random integer value within the specified range.</returns>
        public static int Sequence(ref int seed, int min = int.MinValue, int max = int.MaxValue)
        {
            seed = (seed * 1103515245 + 12345) & int.MaxValue;

            return (seed % (max - min + 1)) + min;
        }

        /// <summary>
        /// Saves export file for given time, username and date. 
        /// </summary>
        /// <param name="time">Time to export</param>
        /// <param name="username">Username of the person exporting</param>
        /// <param name="date">Date of the export</param>
        /// <param name="netstal">Whether Netstal comapny data is being exported or not</param>
        /// <param name="inkognito">Whether the export is anonymous</param>
        public static async void Save(string username, DateTime date, bool netstal = false, bool inkognito = false)
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
                string content = Lavender.LoadExports(username, date.ToString("yyyy-MM-dd"), netstal, inkognito);

                CachedFileManager.DeferUpdates(file);

                using var stream = await file.OpenStreamForWriteAsync();
                using var tw = new StreamWriter(stream);
                tw.Write(content);
            }
        }

        /// <summary>
        /// Returns a list of faults for given time, username and date.
        /// </summary>
        /// <param name="file">The file to load the faults from.</param>
        /// <returns>A list of faults for given time, username and date.</returns>
        public static List<Exportite> GetExport(string file)
        {
            // Create a list of faults
            List<Exportite> faults = new List<Exportite>();

            // Load file from disk
            string[] lines = File.ReadAllLines(file);

            // Remove the first 7 lines
            lines = lines.Skip(7).ToArray();

            // Loop through each line
            foreach (string line in lines)
            {
                string[] splits = line.Split('\t');

                // If the line is not empty
                if (splits.Length == 0) { continue; }

                string order = splits[0].Length == 6 ? Interface.Environment.Format("38" + splits[0]) : Interface.Environment.Format(splits[0]);

                // Split the order number into its parts usin

                string[] appendand = { order, splits[1], splits[2], splits[7] };

                Exportite exportite = new Exportite(appendand);

                faults.Add(exportite);
            }

            return faults;
        }

        public static string[] GetHeader(string file)
        {
            string[] lines = File.ReadAllLines(file);

            string[] decoded = DecodeHeader(string.Join("\r\n", lines.Take(7)));

            if (Checksum(string.Join("\r\n", lines.Skip(7))).Equals(decoded[5]))
            {
                decoded[5] = Windows.ApplicationModel.Resources.ResourceLoader.GetStringForReference(new Uri("ms-resource:T_Verified/Text"));
            }
            else
            {
                decoded[5] = Windows.ApplicationModel.Resources.ResourceLoader.GetStringForReference(new Uri("ms-resource:T_Forged/Text"));
            }

            return decoded;
        }
    }
}
