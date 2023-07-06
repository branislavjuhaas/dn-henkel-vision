// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

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
using Windows.Web.AtomPub;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DN_Henkel_Vision.Interface
{
    /// <summary>
    /// The main environment window of the application.
    /// </summary>
    public sealed partial class Environment : Window
    {
        private static string _selectedOrder = "";
        
        /// <summary>
        /// Constructor of the main application's window.
        /// </summary>
        public Environment()
        {
            this.InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(ApplicationTitleBar);
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
                    suitableItems.Add("No results found");
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

            OrdersPanel_Select(senderButton.Content.ToString());
        }

        /// <summary>
        /// Visually reselects the order in the orders panel and edit environmental conditions for the new selection.
        /// </summary>
        private void NavigationSearch_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            string selection = (string)args.ChosenSuggestion;

            if (selection == null)
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
        }
        
        /// <summary>
        /// This function formats blank order number into a formated selection;
        /// </summary>
        /// <param name="input">input string</param>
        /// <returns>Formatted string in (net)'xxxx  xxxx' and (ord)'xx xxx xxx'</returns>
        private static string Format(string input)
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

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            Manager.UpdateRegistry();
            OrdersPanel_Select(string.Empty);
            Workspace.Navigate(typeof(Exports));
        }

        private void Environment_Closed(object sender, WindowEventArgs args)
        {
            Drive.SaveRegistry();
        }
    }
}
