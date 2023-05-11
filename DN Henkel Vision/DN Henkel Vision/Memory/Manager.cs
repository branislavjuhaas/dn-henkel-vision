using System.Collections.Generic;
using System.Linq;

namespace DN_Henkel_Vision.Memory
{
    /// <summary>
    /// This class represents all the neccessary variables and methods for managing the global memory of the application.
    /// </summary>
    internal class Manager
    {
        public static List<string> OrdersRegistry = new();

        /// <summary>
        /// This method initializes the global memory of the application.
        /// </summary>
        public static void Initialize()
        {
            Drive.Validate();

            OrdersRegistry = Drive.LoadRegistry().ToList();
        }
    }
}
