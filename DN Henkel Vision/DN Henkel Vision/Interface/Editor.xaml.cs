// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using DN_Henkel_Vision.Felber;
using DN_Henkel_Vision.Memory;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DN_Henkel_Vision.Interface
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Editor : Page
    {
        public Action Analyze;

        private bool _locked;
        private bool _reviewing;

        public Preview CurrentPreview;

        /// <summary>
        /// Constructor of the Editor page.
        /// </summary>
        public Editor()
        {
            this.InitializeComponent();
            if (Manager.Selected.IsNetstal())
            {
                Analyze = NetstalAnalyse;
            }
            else
            {
                Analyze = OrderAnalyze;
            }
        }

        /// <summary>
        /// This void sets the content of the Tact TextBlock to the content of the clicked item in the CauseList
        /// and sets the visibility of the CauseList to Collapsed and locks the output.
        /// </summary>
        private void CauseList_ItemClick(object sender, ItemClickEventArgs e)
        {
            Tact.Content = e.ClickedItem.ToString();
            Tact.Flyout.Hide();

            if (_locked) { return; }
            Lock();
        }

        /// <summary>
        /// This void triggers the analysis of the fault input when changed.
        /// </summary>
        private void FaultInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            Analyze();
        }

        private void FaultInput_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                FaultPush();
            }
            else if (e.Key == VirtualKey.Tab && Tact.Content.ToString() != "Cause" && !_locked)
            {
                Lock();
                e.Handled = true;
            }
        }

        /// <summary>
        /// This void unassigns the Netstal placement from the fault input.
        /// </summary>
        private void NetstalPlacement_Click(object sender, RoutedEventArgs e)
        {
            UnassignNetstalPlacement();
        }

        /// <summary>
        /// This void analyzes the fault input for a Netstal placement.
        /// </summary>
        private void NetstalAnalyse()
        {            
            if (FaultInput.SelectionStart < 2 || NetstalPlacement.Visibility == Visibility.Visible) { return; }

            int cursor = FaultInput.SelectionStart - 2;

            string placement = FaultInput.Text.Substring(cursor, 2);

            if (Fault.IsValidNetstalPlacement(placement))
                AssignNetstalPlacement(placement, cursor);
        }

        /// <summary>
        /// This void analyzes the fault input for an order.
        /// </summary>
        private void OrderAnalyze()
        {
            if (_locked || FaultInput.Text.Length == 0) { return; }

            Felber.Felber.EnqueueAnalyze(FaultInput.Text);
        }

        /// <summary>
        /// This void assigns the Netstal placement to the fault input.
        /// </summary>
        private void AssignNetstalPlacement(string placement, int cursor)
        {
            NetstalPlacement.Content = placement.ToUpper();
            FaultInput.Text = FaultInput.Text.Remove(cursor, 2);
            FaultInput.SelectionStart = cursor;
            FaultInput.Margin = new Thickness(82, 0, 180, 0);
            NetstalPlacement.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// This void unassigns the Netstal placement from the fault input and
        /// sets the margin of the fault input to its default value.
        /// </summary>
        private void UnassignNetstalPlacement()
        {
            NetstalPlacement.Visibility = Visibility.Collapsed;
            FaultInput.Margin = new Thickness(20, 0, 180, 0);
        }

        private void PushFault_Click(object sender, RoutedEventArgs e)
        {
            FaultPush();
        }

        private void EditorPage_Loaded(object sender, RoutedEventArgs e)
        {
            Manager.CurrentEditor = this;
        }

        public void Felber_UpdateCause(string cause)
        {
            if (_locked) { return; }

            Tact.Content = cause;
            Tact.FontStyle = Windows.UI.Text.FontStyle.Italic;
        }

        public void Felber_UpdateFault(Fault fault, string order)
        {
            if (order != Manager.Selected.OrderNumber) { return; }

            Manager.Selected.PendingFaults.RemoveAt(0);
            Manager.Selected.ReviewFaults.Add(fault);

            if (_reviewing)
            {
                CurrentPreview.Count.Content = CurrentPreview.CurrentFaultLabel();

                CurrentPreview.SetPreviews();
                
                return;
            }

            Cache.CurrentReview = 0;
            Cache.PreviewFault = Manager.Selected.ReviewFaults[0];

            DataRing.Visibility = Visibility.Collapsed;
            FaultPreview.Navigate(typeof(Preview), null, new SuppressNavigationTransitionInfo());

            _reviewing = true;
        }

        public void Lock()
        {
            _locked = true;
            Tact.FontStyle = Windows.UI.Text.FontStyle.Normal;
            Tact.Foreground = new SolidColorBrush((Color)Application.Current.Resources["TextFillColorPrimary"]);
        }

        public void Unlock()
        {
            _locked = false;
            Tact.Foreground = new SolidColorBrush((Color)Application.Current.Resources["TextFillColorSecondary"]);
        }

        public void ResetEditor()
        {
            FaultInput.Text = string.Empty;            

            Tact.Content = "Cause";
            Unlock();

            NetstalPlacement.Content = string.Empty;
            UnassignNetstalPlacement();
        }

        public void FaultPush()
        {
            if (!_reviewing)
            {
                NoDataText.Visibility = Visibility.Collapsed;
                DataRing.Visibility = Visibility.Visible;
            }

            Manager.Selected.PendingFaults.Add(new Fault(FaultInput.Text, Tact.Content.ToString()));

            Felber.Felber.Requeue(Manager.Selected.PendingFaults[0], Manager.Selected.OrderNumber);

            ResetEditor();
        }
    }
}
