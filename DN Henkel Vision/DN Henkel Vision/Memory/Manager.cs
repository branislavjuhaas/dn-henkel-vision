using DN_Henkel_Vision.Interface;
using Microsoft.UI.Xaml;
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
        public static List<int> Exports = new();
        public static List<int> Contents = new();

        public static ObservableCollection<string> VisualRegistry = new();

        public static Order Selected = new();

        public static Editor CurrentEditor;
        public static Window CurrentWindow;

        public static readonly float Addition = 0.17f;
        public static readonly float AverageTime = 1f;

        public static readonly float AverageLength = 33.09f; // 33.09144927536232 : average of 13 800 samples length

        /// <summary>
        /// This method initializes the global memory of the application.
        /// </summary>
        public static void Initialize()
        {
            Classification.Assign();
            Drive.Validate();

            Drive.LoadRegistry();
            VisualRegistry = new(OrdersRegistry);
            Drive.LoadExportHistory();
            Export.Evaluate();
        }

        /// <summary>
        /// Selects the order from the global memory and loads it into the Selected variable.
        /// </summary>
        /// <param name="orderNumber">Number of the order.</param>
        public static void SelectOrder(string orderNumber)
        {
            if (!string.IsNullOrEmpty(Selected.OrderNumber))
            {
                UpdateRegistry();

                Drive.SaveFaults(Selected.OrderNumber, Selected.Faults.ToList(), Selected.ReviewFaults, Selected.PendingFaults);
            }

            List<Fault>[] faults = Drive.LoadFaults(orderNumber);
            
            int index = OrdersRegistry.IndexOf(orderNumber);

            Selected = new() {
                OrderNumber = orderNumber,
                Faults = new(faults[0]),
                ReviewFaults = faults[1],
                PendingFaults = faults[2],
                User = Users[index],
                Machine = Machines[index],
                Exports = Exports[index]
            };

            if (Selected.ReviewFaults.Count > 0)
            {
                Cache.CurrentReview = 0;
                Cache.LastPlacement = Selected.ReviewFaults[0].Placement;
                
            }
        }

        public static void UpdateRegistry()
        {
            if (string.IsNullOrEmpty(Selected.OrderNumber)) { return; }
            
            int index = OrdersRegistry.IndexOf(Selected.OrderNumber);

            Users[index] = (int)Math.Ceiling(Selected.User);
            Machines[index] = (int)Math.Ceiling(Selected.Machine);
            Exports[index] = Selected.Exports;
            Contents[index] = Selected.Faults.Count;

            if (!Export.Unexported.Contains(Selected.OrderNumber) && Exports[index] < Contents[index])
            {
                Export.Unexported.Add(Selected.OrderNumber);
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

        public static void CreateOrder(string order)
        {
            OrdersRegistry.Add(order);
            VisualRegistry.Add(order);
            Users.Add(0);
            Machines.Add(0);
            Exports.Add(0);
            Contents.Add(0);

            Drive.SaveFaults(order, new(), new(), new());
        }
    }
}
