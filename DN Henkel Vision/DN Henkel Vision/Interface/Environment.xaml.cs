using DN_Henkel_Vision.Memory;
using Microsoft.UI.Windowing;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Numerics;

namespace DN_Henkel_Vision.Interface
{
    /// <summary>
    /// The main environment window of the application.
    /// </summary>
    public sealed partial class Environment : Window
    {
        private static string _selectedOrder = "";

        private static string s_hours = Windows.ApplicationModel.Resources.ResourceLoader.GetStringForReference(new Uri("ms-resource:S_Hours"));
        private string _time = $"0.00 {s_hours}";
        private string _revenue = "0€";
      
        /// <summary>
        /// Constructor of the main application's window.
        /// </summary>
        public Environment()
        {
            UpdateTimebar(true);
            this.InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(ApplicationTitleBar);

            Timegrid.Translation += new Vector3(0, 0, 32);
        }

        /// <summary>
        /// Sets the completion source for the navigation search bar.
        /// </summary>
        #pragma warning disable CA1822 // Mark members as static
        private void NavigationSearch_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        #pragma warning restore CA1822 // Mark members as static
        {
            // Since selecting an item will also change the text,
            // only listen to changes caused by user entering text.
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                List<string> suitableItems = new();
                string[] splitText = Format(sender.Text).Split(" ");
                foreach (string order in Manager.OrdersRegistry)
                {
                    bool found = splitText.All((key) =>
                    {
                        return order.Contains(key);
                    });
                    if (found)
                    {
                        suitableItems.Add(order);
                    }
                }
                if (suitableItems.Count == 0)
                {
                    suitableItems.Add(Windows.ApplicationModel.Resources.ResourceLoader.GetStringForReference(new Uri("ms-resource:S_Results")));
                }
                sender.ItemsSource = suitableItems;
            }

        }

        /// <summary>
        /// Visually reselects the order in the orders panel and
        /// edit environmental conditions for the new selection.
        /// </summary>
        private void OrderToggle_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton senderButton = sender as ToggleButton;

            UpdateTimebar();

            OrdersPanel_Select(senderButton.Content.ToString());
        }

        /// <summary>
        /// Visually reselects the order in the orders panel and edit environmental conditions for the new selection.
        /// </summary>
        private void NavigationSearch_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            string selection = (string)args.ChosenSuggestion;

            if (string.IsNullOrEmpty(selection))
            {
                List<string> suitableItems = new();
                string[] splitText = Format(sender.Text).Split(" ");
                foreach (string order in Manager.OrdersRegistry)
                {
                    bool found = splitText.All((key) =>
                    {
                        return order.Contains(key);
                    });
                    if (found)
                    {
                        suitableItems.Add(order);
                    }
                }
                if (suitableItems.Count == 0)
                {
                    return;
                }
                selection = suitableItems[0];
            }
            
            OrdersPanel_Select(selection);
            sender.Text = "";
        }

        /// <summary>
        /// Visually sets the first element in the orders panel as selected order.
        /// </summary>
        private void OrdersPanel_Loaded(object sender, RoutedEventArgs e)
        {
            if (Manager.OrdersRegistry.Count == 0)
            {
                Workspace.Navigate(typeof(Welcome));
                return;
            }
            
            OrdersPanel_Select(Manager.OrdersRegistry[0]);
        }

        /// <summary>
        /// Selects the order in the visual orders panel and deselects the old one.
        /// </summary>
        /// <param name="selection">The string with order that should be selected</param>
        private void OrdersPanel_Select(string selection)
        {           
            int selectedOrderIndex = Manager.OrdersRegistry.IndexOf(_selectedOrder);

            // If there is already selected order, deselect it.
            if (selectedOrderIndex != -1)
            {
                ((ToggleButton)OrdersPanel.GetOrCreateElement(selectedOrderIndex)).IsChecked = false;
            }

            // Check if the selection exist and if not, set the selected
            // order index to -1 (no order selected) and return.
            if (!Manager.OrdersRegistry.Contains(selection))
            {
                _selectedOrder = "";
                return;
            }

            int newOrderIndex = Manager.OrdersRegistry.IndexOf(selection);

            ((ToggleButton)OrdersPanel.GetOrCreateElement(newOrderIndex)).IsChecked = true;

            _selectedOrder = selection;

            ReselectOrder(selection);
        }

        /// <summary>
        /// Reselects the order and edit environmental conditions for the new selection.
        /// </summary>
        /// <param name="orderNumber">Number of the new order, that is goinng to be selected</param>
        public void ReselectOrder(string orderNumber)
        {
            if ( (orderNumber == Manager.Selected.OrderNumber && orderNumber != _selectedOrder) || !Manager.OrdersRegistry.Contains(orderNumber))
            {
                return;
            }

            if (Manager.CurrentEditor != null)
            {
                Manager.CurrentEditor.FaultsList.ItemsSource = null;
            }

            Manager.SelectOrder(orderNumber);
            Workspace.Navigate(typeof(Editor));
            Exporter.Visibility = Visibility.Collapsed;
        }
        
        /// <summary>
        /// This function formats blank order number into a formated selection;
        /// </summary>
        /// <param name="input">input string</param>
        /// <returns>Formatted string in (net)'xxxx  xxxx' and (ord)'xx xxx xxx'</returns>
        public static string Format(string input)
        {
            string output = input;

            try
            {
                if (input.StartsWith("20"))
                {
                    output = input.Insert(4, "  ");
                }
                else if (input.StartsWith("38"))
                {
                    output = input.Insert(2, " ");
                    output = output.Insert(6, " ");
                }
            }
            catch { }

            return output;
        }

        /// <summary>
        /// Handles the click event of the Export button.
        /// Saves faults, initiates data evaluation and navigates to the Exports page.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The event args.</param>
        private void Export_Click(object sender, RoutedEventArgs e)
        {
            OrdersPanel_Select(string.Empty);
            Workspace.Navigate(typeof(Lavender));
            Exporter.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Handles the event when the Environment window is closed.
        /// Saves any changes made to the selected order before closing the application.
        /// </summary>
        private void Environment_Closed(object sender, WindowEventArgs args)
        {

        }

        /// <summary>
        /// Event handler for the Create button. Displays a dialog to create a new order and saves it.
        /// </summary>
        /// <param name="sender">The object which raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private async void Create_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog orderDialog = new()
            {
                XamlRoot = Manager.CurrentWindow.Content.XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = Windows.ApplicationModel.Resources.ResourceLoader.GetStringForReference(new Uri("ms-resource:T_CreateOrder/Text")),
                PrimaryButtonText = Windows.ApplicationModel.Resources.ResourceLoader.GetStringForReference(new Uri("ms-resource:B_Create/Content")),
                CloseButtonText = Windows.ApplicationModel.Resources.ResourceLoader.GetStringForReference(new Uri("ms-resource:B_Cancel/Content")),
                DefaultButton = ContentDialogButton.Primary,
                IsPrimaryButtonEnabled = false,
                RequestedTheme = (Manager.CurrentWindow.Content as Grid).RequestedTheme,
                Content = new Order()
            };

            ContentDialogResult result = await orderDialog.ShowAsync();

            if (result != ContentDialogResult.Primary) { return; }
            
            string chip = ((Order)orderDialog.Content).CategoryText.Text;

            if (chip == Windows.ApplicationModel.Resources.ResourceLoader.GetStringForReference(new Uri("ms-resource:T_Invalid/Text"))) { return; }

            if (chip == Windows.ApplicationModel.Resources.ResourceLoader.GetStringForReference(new Uri("ms-resource:T_Existing/Text")))
            {
                OrdersPanel_Select(Format(((Order)orderDialog.Content).Number.Text));
                return;
            }

            Manager.CreateOrder(Format(((Order)orderDialog.Content).Number.Text));
        }

        /// <summary>
        /// Handles the click event of the settings button and navigates to the settings page.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event data.</param>
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            OrdersPanel_Select(string.Empty);
            Workspace.Navigate(typeof(Settings));
            Exporter.Visibility = Visibility.Collapsed;
        }

        private void Environment_Loaded(object sender, RoutedEventArgs e)
        {
            var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(windowHandle);
            AppWindow appWindow = AppWindow.GetFromWindowId(windowId);
            OverlappedPresenter presenter = (OverlappedPresenter)appWindow.Presenter;
            presenter.Maximize();
            Authentification.Auth();
        }

        public void UpdateTimebar(bool evaluateOnly = false)
        {
            _time = $"{MathF.Round(Memory.Lavender.Time / 60f, 2)} {s_hours}";
            _revenue = (MathF.Round(Memory.Lavender.Time / 60f, 2) * 4.2f).ToString("0.00") + "€";

            if (evaluateOnly) { return; }

            (Timepanel.Children[1] as TextBlock).Text = _time;
            (Timepanel.Children[3] as TextBlock).Text = _revenue;
        }

        private void Exporter_Click(object sender, RoutedEventArgs e)
        {
            (Workspace.Content as Lavender).Exporter_Click(sender, e);
        }
    }
}
