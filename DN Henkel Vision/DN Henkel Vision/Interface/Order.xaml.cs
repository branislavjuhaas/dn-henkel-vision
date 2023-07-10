using DN_Henkel_Vision.Memory;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DN_Henkel_Vision.Interface
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Order : Page
    {
        public Order()
        {
            this.InitializeComponent();
        }

        private void Number_KeyUp  (object sender, KeyRoutedEventArgs e)
        {
            string order = Environment.Format(Number.Text.Replace(" ", ""));

            if (Manager.OrdersRegistry.Contains(order))
            {
                //CategoryChip.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 209, 94, 94));
                CategoryText.Text = "Existing";
                return;
            }

            if (Number.Text.StartsWith("20") && order.Length == 10)
            {
                //CategoryChip.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 33, 135, 33));
                CategoryText.Text = "Netstal";
                return;
            }

            if (order.StartsWith("38") && order.Length == 10)
            {
                //CategoryChip.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 33, 135, 33));
                CategoryText.Text = "Auftrag";
                return;
            }

            //CategoryChip.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 209, 94, 94));
            CategoryText.Text = "Invalid";
        }
    }
}
