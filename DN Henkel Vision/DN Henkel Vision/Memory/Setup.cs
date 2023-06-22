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
            
            Write(s_registry, "38 655 127\t1000\t1000\t1\n38 621 846\t1000\t1000\t1\n38 621 848\t1000\t1000\t1\n2023  0001\t1000\t1000\t1");

            Write(CreateFaultsPath("38 655 127"), "Preview:\nPending:");
            Write(CreateFaultsPath("38 621 846"), "Preview:\nPending:");
            Write(CreateFaultsPath("38 621 848"), "Preview:\nPending:");
            Write(CreateFaultsPath("2023  0001"), "Preview:\nPending:");
        }
    }
}
