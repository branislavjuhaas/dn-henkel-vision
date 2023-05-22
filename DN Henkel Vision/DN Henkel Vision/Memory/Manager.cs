using DN_Henkel_Vision.Interface;
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

        /// <summary>
        /// This method initializes the global memory of the application.
        /// </summary>
        public static void Initialize()
        {
            Drive.Validate();

            OrdersRegistry = Drive.LoadRegistry().ToList<string>();
            VisualRegistry = new(OrdersRegistry);
        }

        /// <summary>
        /// Selects the order from the global memory and loads it into the Selected variable.
        /// </summary>
        /// <param name="orderNumber">Number of the order.</param>
        public static void SelectOrder(string orderNumber)
        {
            // TODO: make this method load the order from the file system\
            Selected = new Order() { OrderNumber = orderNumber };
        }
    }
}
