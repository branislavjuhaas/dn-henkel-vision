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

        public static ObservableCollection<string> VisualRegistry = new();

        public static Order Selected = new();

        public static Editor CurrentEditor;
        public static Window CurrentWindow;

        public static readonly float Addition = 0.17f;
        public static readonly float AverageTime = 1f;

        public static readonly float AverageLength = 33.09f; // 33.09144927536232 : average of 13 800 samples length

        public static string DevText = "";

        public static string LaunchingFile = string.Empty;

        /// <summary>
        /// This method initializes the global memory of the application.
        /// </summary>
        public static void Initialize()
        {
            Classification.Assign(Windows.ApplicationModel.Resources.ResourceLoader.GetStringForReference(new Uri("ms-resource:S_Language")));
            Lavender.Validate();

            Memory.Lavender.LoadSettings();
            OrdersRegistry = Lavender.LoadRegistry();
            VisualRegistry = new(OrdersRegistry);
            Lavender.EvaluateTime();
            Lavender.CreateWatcher();
        }

        /// <summary>
        /// Selects the order from the global memory and loads it into the Selected variable.
        /// </summary>
        /// <param name="orderNumber">Number of the order.</param>
        public static void SelectOrder(string orderNumber)
        {           
            int index = OrdersRegistry.IndexOf(orderNumber);

            Selected = new() {
                OrderNumber = orderNumber,
                Faults = new(Lavender.LoadFaults(orderNumber)),
            };

            if (Selected.ReviewFaults.Count > 0)
            {
                Cache.CurrentReview = 0;
                Cache.LastPlacement = Selected.ReviewFaults[0].Placement;
                
            }

            Cache.LastPlacement = string.Empty;
        }

        /// <summary>
        /// Creates and adds an order to the global memory.
        /// </summary>
        /// <param name="order">Number of the order to be created and added.</param>
        public static void CreateOrder(string order)
        {
            OrdersRegistry.Add(order);
            VisualRegistry.Add(order);

            Lavender.CreateOrder(order);
        }
    }
}
