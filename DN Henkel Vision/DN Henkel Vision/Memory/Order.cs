using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DN_Henkel_Vision.Memory
{
    public class Order
    {
        #region Export Constants

        public const int Complete = 0;
        public const int Exported = 1;
        public const int Partial = 2;

        #endregion

        public string OrderNumber;
        public string Faults;
        
        public int OrderTotalTime;
        public int ExportModifiedTime;

        public int State = Complete;

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
