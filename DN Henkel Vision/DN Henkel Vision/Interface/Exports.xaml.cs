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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Globalization.NumberFormatting;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DN_Henkel_Vision.Interface
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Exports : Page
    {
        private string _userTime = $"{Math.Round((Export.OrdersTime(Export.Unexported.ToArray(), false) + Export.OrdersTime(Export.Unexported.ToArray(), false, true)) / 60f, 2)} hours";
        private string _totalTime = $"{Math.Round(((Export.OrdersTime(Export.Unexported.ToArray()) + Export.OrdersTime(Export.Unexported.ToArray(), true, true)) / 60f), 2)} hours";

        private DateTimeOffset _today = DateTime.UtcNow;

        public Exports()
        {
            this.InitializeComponent();

            Selected.Text = (Export.OrdersTime(Export.Unexported.ToArray(), true, Convert.ToBoolean(Category.SelectedIndex)) / 60f).ToString("0.00");
            Selected.Maximum = Math.Round(Export.OrdersTime(Export.Unexported.ToArray(), true, Convert.ToBoolean(Category.SelectedIndex)) / 60f, 2);
            Display.PlaceholderText = Selected.Text;
            Display.Text = Selected.Text;
            Display.Minimum = Math.Round(Selected.Value, 2);
        }

        public string Date(int days)
        {
            return DateTime.UtcNow.AddDays(-days).ToString("dd.MM");
        }

        private void Machine_Click(object sender, RoutedEventArgs e)
        {
            if (((ToggleButton)sender).IsChecked == false)
            {
                BackGraph.Visibility = Visibility.Collapsed;
                return;
            }

            BackGraph.Visibility = Visibility.Visible;
        }

        private void GraphGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            float space = (MathF.Pow((float)((Grid)sender).ActualWidth, 1.5f) / 3000f);

            if (space < 4f)
            {
                space = 4f;
            }

            GraphingLayout.Spacing = space;
            Legend.Spacing = 7 * (space + 4) - 2;
            GraphPanel.Width = 5f * (7f * (4f + space + 6.17f));
        }

        private void Type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FrontGraph == null) { return; }
            
            if (Type.SelectedIndex == 0)
            {
                FrontGraph.ItemsSource = Export.UserServiceGraph;
                BackGraph.ItemsSource = Export.MachServiceGraph;
                FullTime.Text = Export.GraphTime(0);
                HalfTime.Text = Export.GraphTime(1);
                return;
            }

            FrontGraph.ItemsSource = Export.UserExportsGraph;
            BackGraph.ItemsSource = Export.MachExportsGraph;
            BackGraph.ItemsSource = Export.MachExportsGraph;
            FullTime.Text = Export.GraphTime(2);
            HalfTime.Text = Export.GraphTime(3);
        }

        private void Selected_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            Display.PlaceholderText = Selected.Value.ToString();
            Display.Minimum = Selected.Value;
        }

        private void Criterium_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Selected == null) { return; }
            
            switch (Criterium.SelectedIndex)
            {
                case 0:
                    Selected.IsEnabled = false;
                    Selected.Text = (Export.OrdersTime(Export.Unexported.ToArray(), true, Convert.ToBoolean(Category.SelectedIndex)) / 60f).ToString("0.00");
                    Display.Text = Selected.Text;
                    Display.Minimum = Math.Round(Selected.Value, 2);
                    break;

                case 1:
                    Selected.IsEnabled = false;
                    Selected.Text = (Export.OrdersTime(Export.Unexported.ToArray(), false, Convert.ToBoolean(Category.SelectedIndex)) / 60f).ToString("0.00");
                    Display.Text = Selected.Text;
                    Display.Minimum = Math.Round(Selected.Value, 2);
                    break;

                case 2:
                    Selected.Text = "0";
                    Selected.Text = string.Empty;
                    Display.Text = string.Empty;
                    Selected.IsEnabled = true;
                    break;
            }
        }

        private void Category_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Selected == null) { return; }
            
            switch (Criterium.SelectedIndex)
            {
                case 0:
                    Selected.IsEnabled = false;
                    Selected.Text = (Export.OrdersTime(Export.Unexported.ToArray(), true, Convert.ToBoolean(Category.SelectedIndex)) / 60f).ToString("0.00");
                    Display.Text = Selected.Text;
                    Display.Minimum = Math.Round(Selected.Value, 2);
                    break;

                case 1:
                    Selected.IsEnabled = false;
                    Selected.Text = (Export.OrdersTime(Export.Unexported.ToArray(), false, Convert.ToBoolean(Category.SelectedIndex)) / 60f).ToString("0.00");
                    Display.Text = Selected.Text;
                    Display.Minimum = Math.Round(Selected.Value, 2);
                    break;
            }

            Selected.Maximum = Math.Round(Export.OrdersTime(Export.Unexported.ToArray(), true, Convert.ToBoolean(Category.SelectedIndex)) / 60f, 2);
        }

        private void Exporter_Click(object sender, RoutedEventArgs e)
        {
            Export.ExportFaults((float)Selected.Value, UserName.Text, ((DateTimeOffset)RegistryDate.Date).DateTime, Convert.ToBoolean(Category.SelectedIndex));
        }
    }
}
