using System;
using System.IO;

namespace DN_Henkel_Vision.Memory
{
    /// <summary>
    /// Provides methods for setting up the file system if it does not exist.
    /// </summary>
    internal class Setup : Drive
    {        
        /// <summary>
        /// This method creates the file system.
        /// </summary>
        public static void CreateFileSystem()
        {
            Directory.CreateDirectory(s_regdir);
            Directory.CreateDirectory(s_orders);
            
            Write(s_registry, "");

            Write(s_exports, DateTime.Now.ToString("ddMMyyyy"));
            Write(s_settings, "211");

            Write(s_system, string.Empty);
        }
    }
}
