using DN_Henkel_Vision.Interface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DN_Henkel_Vision.Memory
{
    /// <summary>
    /// This class represents all the neccessary variables and methods for managing the global memory of the application.
    /// </summary>
    internal class Manager
    {
        public static List<string> OrdersRegistry = new();
        public static List<int> Users = new();
        public static List<int> Machines = new();
        public static List<int> States = new();

        public static ObservableCollection<string> VisualRegistry = new();

        public static Order Selected = new();

        public static Editor CurrentEditor;

        /// <summary>
        /// This method initializes the global memory of the application.
        /// </summary>
        public static void Initialize()
        {
            Classification.Assign();
            Drive.Validate();

            Drive.LoadRegistry();
            VisualRegistry = new(OrdersRegistry);
        }

        /// <summary>
        /// Selects the order from the global memory and loads it into the Selected variable.
        /// </summary>
        /// <param name="orderNumber">Number of the order.</param>
        public static void SelectOrder(string orderNumber)
        {
            if (Selected.OrderNumber == null) { Selected = new() { OrderNumber = orderNumber }; return; }
            
            int index = OrdersRegistry.IndexOf(Selected.OrderNumber);

            Users[index] = Selected.User;
            Machines[index] = Selected.Machine;
            States[index] = Selected.State;

            Drive.SaveFaults(Selected.OrderNumber, Selected.Faults.ToList(), Selected.ReviewFaults, Selected.PendingFaults);

            List<Fault>[] faults = Drive.LoadFaults(orderNumber);
            
            index = OrdersRegistry.IndexOf(orderNumber);

            Selected = new() {
                OrderNumber = orderNumber,
                Faults = new(faults[0]),
                ReviewFaults = faults[1],
                PendingFaults = faults[2],
                User = Users[index],
                Machine = Machines[index],
                State = States[index]
            };

            if (Selected.ReviewFaults.Count > 0)
            {
                Cache.CurrentReview = 0;
                Cache.LastPlacement = Selected.ReviewFaults[0].Placement;
                
            }
        }

        public static int CreateIndex()
        {
            //NOTE: Working just up to year 2092
            int index = (int)(DateTime.Now - new DateTime(2023, 4, 3)).TotalSeconds;

            if (index > Cache.LastIndex) { Cache.LastIndex = index; return index; }
                
            Cache.LastIndex++;
            return Cache.LastIndex;
        }
    }
}
