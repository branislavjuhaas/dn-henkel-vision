using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DN_Henkel_Vision.Memory
{
    /// <summary>
    /// This class represents the file system of the application and provides methods for accessing the file system on the drive.
    /// </summary>
    internal class Drive
    {
        public static readonly string Folder = Windows.ApplicationModel.Package.Current.InstalledPath;

        #region File Paths

        internal static readonly string s_regdir =   $"{Folder}\\FileSystem\\Registry\\";

        internal static readonly string s_system =      $"{Folder}\\FileSystem\\System.dntf";
        internal static readonly string s_registry =    $"{Folder}\\FileSystem\\Registry\\Registry.dntf";
        
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
        public static string[] LoadRegistry()
        {
            string source = Read(s_registry);
            return source.Split('\n');
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
    }
}
