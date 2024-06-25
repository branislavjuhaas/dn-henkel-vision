using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DN_Henkel_Vision.Memory
{
    /// <summary>
    /// Class for storing all information about an order.
    /// </summary>
    public class Order
    {
        #region Export Constants

        #endregion

        public string OrderNumber;

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
