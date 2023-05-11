using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DN_Henkel_Vision.Memory
{
    internal class Manager
    {
        public static List<string> OrdersRegistry = new();

        public static void Initialize()
        {
            OrdersRegistry = Drive.LoadRegistry().ToList();
        }
    }
}
