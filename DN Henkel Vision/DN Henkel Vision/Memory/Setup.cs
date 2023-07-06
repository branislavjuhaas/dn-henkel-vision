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
            
            Write(s_registry, "38 630 144\t0\t0\t0\t0");

            Write(s_exports, "26062023\n100\t60\t100\t40\n500\t120\t500\t140");

            Write(CreateFaultsPath("38 630 144"), "Preview:\nPending:");
            //Write(CreateFaultsPath("38 621 846"), "Preview:\nPending:");
            //Write(CreateFaultsPath("38 621 848"), "Preview:\nPending:");
            //Write(CreateFaultsPath("2023  0001"), "Preview:\nPending:");

            Write(s_system, string.Empty);
        }
    }
}
