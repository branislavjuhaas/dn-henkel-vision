using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DN_Henkel_Vision.Memory
{
    public class Order
    {
        #region Export Constants

        #endregion

        public string OrderNumber;

        public float User;
        public float Machine;

        public float OrderTotalTime;
        public float ExportModifiedTime;

        public ObservableCollection<Fault> Faults = new();
        public List<Fault> Loader = new();
        public List<Fault> PendingFaults = new();
        public List<Fault> ReviewFaults = new();

        public int Exports;

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
