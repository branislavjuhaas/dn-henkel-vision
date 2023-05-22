using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DN_Henkel_Vision.Memory
{
    public class Order
    {
        public string OrderNumber;
        public string Faults;

        /// <summary>
        /// This Bool checks if the order is a Netstal order.
        /// </summary>
        /// <returns>The type of the order</returns>
        public bool IsNetstal()
        {
            return OrderNumber.StartsWith("20");
        }
    }
}
