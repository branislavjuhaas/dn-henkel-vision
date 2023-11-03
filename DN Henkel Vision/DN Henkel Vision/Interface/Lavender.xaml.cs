using DN_Henkel_Vision.Memory;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.System;
using Windows.UI.Core;

namespace DN_Henkel_Vision.Interface
{
    /// <summary>
    /// Exports page of the application.
    /// </summary>
    public sealed partial class Lavender : Page
    {
        private static string _hours = Windows.ApplicationModel.Resources.ResourceLoader.GetStringForReference(new Uri("ms-resource:S_Hours"));

        private List<int> _graphings = new();

        public int _full = 2;
        public int _half = 1;

        private readonly DateTimeOffset _today = DateTime.Now;

        /// <summary>
        /// Initializes a new instance of the <see cref="Exports"/> class.
        /// </summary>
        public Lavender()
        {
            EvaluateGraph();
            this.InitializeComponent();
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
        /// Event handler for the Exporter button click event. Saves exports with given input. 
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event data.</param>
        public void Exporter_Click(object sender, RoutedEventArgs e)
        {
            Drive.ExportsSave("Branislav Juhás", ((DateTimeOffset)RegistryDate.Date).DateTime, Convert.ToBoolean(Category.SelectedIndex), IsShift());
        }

        /// <summary>
        /// Returns a boolean indicating whether the Shift key is currently pressed.
        /// </summary>
        /// <returns>A boolean value indicating whether the Shift key is currently pressed.</returns>
        private static bool IsShift()
        {
            CoreVirtualKeyStates states = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift);

            return (states & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
        }

        public void EvaluateGraph()
        {
            List<float> services = Memory.Lavender.EvaluateGraph();

            if (services.Max() > 120f)
            {
                //ServiceMaximal = (int)((float)Math.Ceiling(ServiceSum().Max() / 2f) * 2f);
                //ServiceHalf = ServiceMaximal / 2;

                _full = (int)(Math.Ceiling(services.Max() / 120f)) * 2;
                _half = _full / 2;
            }
            else
            {
                _full = 2;
                _half = 1;
            }

            float multiplier = 158f / (float)_full;

            _graphings.Clear();

            for (int i = 0; i < services.Count; i++)
            {
                _graphings.Add((int)(services[i] * multiplier / 60f));
            }
        }

        public string GraphTime(int scenario)
        {
            return scenario switch
            {
                0 => _half.ToString() + "h",
                1 => _full.ToString() + "h",
                _ => String.Empty,
            };
        }
    }
}
