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
            
            Write(s_registry, "38 655 127\n38 621 846\n38 621 848\n38 630 095");
        }
    }
}
