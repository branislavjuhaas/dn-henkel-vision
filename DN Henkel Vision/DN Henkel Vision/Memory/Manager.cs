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

        public static bool Developer = false;
        public static string DevText = "Preview";

        /// <summary>
        /// This method initializes the global memory of the application.
        /// </summary>
        public static void Initialize()
        {
            Classification.Assign(Windows.ApplicationModel.Resources.ResourceLoader.GetStringForReference(new Uri("ms-resource:S_Language")));
            Drive.Validate();
            Drive.Log("Drive validated successfully.");

            Drive.LoadSettings();
            Drive.Log("Settings loaded successfully.");
            Drive.LoadRegistry();
            Drive.Log("Registry loaded successfully.");
            VisualRegistry = new(OrdersRegistry);
            Drive.LoadExportHistory();
            Drive.Log("Export history loaded successfully.");
            Export.Evaluate();
            Drive.Log("Export history evaluated successfully.");
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

            Cache.LastPlacement = string.Empty;
        }

        /// <summary>
        /// Updates the OrdersRegistry with the Selected order data, along with the Users, Machines,
        /// Exports, and Contents lists with updated values. Also updates the Export data for a graphical display.
        /// </summary>
        public static void UpdateRegistry()
        {
            if (string.IsNullOrEmpty(Selected.OrderNumber)) { return; }
            
            int index = OrdersRegistry.IndexOf(Selected.OrderNumber);

            int oldusers = Users[index];
            int oldmachs = Machines[index];

            Users[index] = (int)Math.Ceiling(Selected.User);
            Machines[index] = (int)Math.Ceiling(Selected.Machine);
            Exports[index] = Selected.Exports;
            Contents[index] = Selected.Faults.Count;

            if (!Export.Unexported.Contains(Selected.OrderNumber) && Exports[index] < Contents[index])
            {
                Export.Unexported.Add(Selected.OrderNumber);
            }

            if (Cache.LastDate != DateTime.Now.Date)
            {
                int offset = (int)(DateTime.Now - Cache.LastDate).TotalDays;

                Cache.LastDate = DateTime.Now.Date;

                for (int i = 0; i < Export.GraphicalCount; i++)
                {
                    if (i + offset < Export.GraphicalCount)
                    {
                        Export.UserService[i] = Export.UserService[i + offset];
                        Export.MachService[i] = Export.MachService[i + offset];
                        Export.UserExports[i] = Export.UserExports[i + offset];
                        Export.MachExports[i] = Export.MachExports[i + offset];
                        continue;
                    }

                    Export.UserService[i] = 0f;
                    Export.MachService[i] = 0f;
                    Export.UserExports[i] = 0f;
                    Export.MachExports[i] = 0f;
                }
            }

            Export.UserService[Export.GraphicalCount - 1] += (float)(Users[index] - oldusers) / 60f;
            Export.MachService[Export.GraphicalCount - 1] += (float)(Machines[index] - oldmachs) / 60f;

            if (oldusers != Users[index] || oldmachs != Machines[index]) { Export.ChangedData = true; }
        }

        /// <summary>
        /// This method creates and returns an unique index using the current time and date.
        /// NOTE: Working just up to year 2159
        /// </summary>
        /// <returns>An unsigned integer representing the unique index.</returns>
        public static uint CreateIndex()
        {
            uint index = (uint)(DateTime.Now - new DateTime(2023, 4, 3)).TotalSeconds;

            if (index > Cache.LastIndex) { Cache.LastIndex = index; return index; }
                
            Cache.LastIndex++;
            return Cache.LastIndex;
        }

        /// <summary>
        /// Creates and adds an order to the global memory.
        /// </summary>
        /// <param name="order">Number of the order to be created and added.</param>
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
