using DN_Henkel_Vision.Memory;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using Windows.System;
using Windows.UI.Core;

namespace DN_Henkel_Vision.Interface
{
    /// <summary>
    /// Exports page of the application.
    /// </summary>
    public sealed partial class Exports : Page
    {
        private string _userTime = $"{Math.Round((Export.OrdersTime(Export.Unexported.ToArray(), false) + Export.OrdersTime(Export.Unexported.ToArray(), false, true)) / 60f, 2)} hours";
        private string _totalTime = $"{Math.Round(((Export.OrdersTime(Export.Unexported.ToArray()) + Export.OrdersTime(Export.Unexported.ToArray(), true, true)) / 60f), 2)} hours";

        private DateTimeOffset _today = DateTime.Now;

        /// <summary>
        /// Initializes a new instance of the <see cref="Exports"/> class.
        /// </summary>
        public Exports()
        {
            this.InitializeComponent();

            Selected.Text = (Export.OrdersTime(Export.Unexported.ToArray(), true, Convert.ToBoolean(Category.SelectedIndex)) / 60f).ToString("0.00");
            Selected.Maximum = Math.Round(Export.OrdersTime(Export.Unexported.ToArray(), true, Convert.ToBoolean(Category.SelectedIndex)) / 60f, 2);
            Display.PlaceholderText = Selected.Text;
            Display.Text = Selected.Text;
            Display.Minimum = Math.Round(Selected.Value, 2);
            Manager.Selected.OrderNumber = null;
        }

        /// <summary>
        /// Returns the date calculated from subtracting days from the current date.
        /// </summary>
        /// <param name="days">Number of days to subtract from the current date.</param>
        /// <returns>A string that represents the date in the format 'dd.MM'.</returns>
        public string Date(int days)
        {
            return DateTime.Now.AddDays(-days).ToString("dd.MM");
        }

        /// <summary>
        /// Event handler for the click event on the Machine toggle button.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private void Machine_Click(object sender, RoutedEventArgs e)
        {
            if (((ToggleButton)sender).IsChecked == false)
            {
                BackGraph.Visibility = Visibility.Collapsed;
                return;
            }

            BackGraph.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Handles the event when the size of the GraphGrid changes.
        /// </summary>
        /// <param name="sender">The object that fired the event.</param>
        /// <param name="e">The event arguments.</param>
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

        /// <summary>
        /// Event handler for when the selection of a combobox item changes. Updates the FrontGraph and BackGraph elements and FullTime and HalfTime TextBlocks to reflect the current selection.
        /// </summary>
        /// <param name="sender">The sender object of the event.</param>
        /// <param name="e">The SelectionChangedEventArgs associated with the event.</param>
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

        /// <summary>
        /// Event handler for when the selected number changes in the NumberBox control.
        /// </summary>
        /// <param name="sender">The NumberBox sending the event.</param>
        /// <param name="args">Event arguments containing the new and old selected value.</param>
        private void Selected_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            Display.PlaceholderText = Selected.Value.ToString();
            Display.Minimum = Selected.Value;

            Exporter.IsEnabled = Selected.Value > 0f;
        }

        /// <summary>
        /// Handle selection changes in the Criterium ComboBox.
        /// </summary>
        /// <param name="sender">The ComboBox that raised the event.</param>
        /// <param name="e">The event arguments.</param>
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

        /// <summary>
        /// Handles the selection changed event for the Category control.
        /// Calculates and displays the selected criteria value.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
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

        /// <summary>
        /// Event handler for the Exporter button click event. Saves exports with given input. 
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event data.</param>
        private void Exporter_Click(object sender, RoutedEventArgs e)
        {
            Drive.ExportsSave((float)Selected.Value, UserName.Text, ((DateTimeOffset)RegistryDate.Date).DateTime, Convert.ToBoolean(Category.SelectedIndex), IsShift());
        }

        /// <summary>
        /// Check if exporting is possible.
        /// </summary>
        /// <returns>True if export is possible, false otherwise.</returns>
        public bool CanExport()
        {
            return Selected.Value > 0f;
        }

        /// <summary>
        /// Returns a boolean indicating whether the Shift key is currently pressed.
        /// </summary>
        /// <returns>A boolean value indicating whether the Shift key is currently pressed.</returns>
        private bool IsShift()
        {
            CoreVirtualKeyStates states = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift);

            return (states & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
        }
    }
}
