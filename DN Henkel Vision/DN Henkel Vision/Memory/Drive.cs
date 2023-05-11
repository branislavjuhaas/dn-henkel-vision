using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DN_Henkel_Vision.Memory
{
    internal class Drive
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
        public static readonly string Folder = Windows.ApplicationModel.Package.Current.InstalledPath;

        internal static readonly string s_system =      $"{Folder}\\FileSystem\\System.dntf";
        internal static readonly string s_registry =    $"{Folder}\\FileSystem\\Registry\\Registry.dntf";
        
        /// <summary>
        /// This method validates
        /// </summary>
        public static void Validate()
        {
            if (File.Exists(s_system))
            {
                return;
            }

            Setup.CreateFileSystem();
        }
        
        public static string[] LoadRegistry()
        {
            return new string[0];
        }
    }
}
