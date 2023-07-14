using DN_Henkel_Vision.Memory;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace DN_Henkel_Vision.Interface
{
    /// <summary>
    /// Content of the dialog for creating a new order.
    /// </summary>
    public sealed partial class Order : Page
    {
        /// <summary>
        /// Initializes a new instance of the Order class.
        /// </summary>
        public Order()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Handles the KeyUp event of the Number control. 
        /// Validates the entered order and sets the proper category text and button state.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">Key event data that describes the key that raised the event.</param> 
        private void Number_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            string order = Environment.Format(Number.Text.Replace(" ", ""));

            ContentDialog dialog = this.Parent as ContentDialog;

            if (string.IsNullOrEmpty(order))
            {
                CategoryText.Text = "Invalid";
                dialog.IsPrimaryButtonEnabled = false;
                return;
            }

            switch (order.Substring(0, 2))
            {
                case "10":
                    if (order.Length == 9)
                    {
                        CategoryText.Text = "Feauf";
                        dialog.IsPrimaryButtonEnabled = true;
                        dialog.PrimaryButtonText = "Create";
                        return;
                    }
                    break;
                case "20":
                    if (order.Length == 10)
                    {
                        CategoryText.Text = "Netstal";
                        dialog.IsPrimaryButtonEnabled = true;
                        dialog.PrimaryButtonText = "Create";
                        return;
                    }
                    break;
                case "38":
                    if (order.Length == 10)
                    {
                        CategoryText.Text = "Auftrag";
                        dialog.IsPrimaryButtonEnabled = true;
                        dialog.PrimaryButtonText = "Create";
                        return;
                    }
                    break;
                default:
                    break;
            }

            if (Manager.OrdersRegistry.Contains(order))
            {
                CategoryText.Text = "Existing";
                dialog.IsPrimaryButtonEnabled = true;
                dialog.PrimaryButtonText = "Select";
                return;
            }

            CategoryText.Text = "Invalid";
            dialog.IsPrimaryButtonEnabled = false;
        }
    }
}
