// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using DN_Henkel_Vision.Memory;
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
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Media.Animation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DN_Henkel_Vision.Interface
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Preview : Page
    {
        public ObservableCollection<string> Previews = new();

        public Fault Current = Manager.Selected.ReviewFaults[Cache.CurrentReview];

        /// <summary>
        /// Constructor of the Preview page.
        /// </summary>
        public Preview()
        {
            this.InitializeComponent();

            Manager.CurrentEditor.CurrentPreview = this;

            SetPreviews();
        }

        /// <summary>
        /// This void is triggered when the user changes the selected item in the Causes
        /// ComboBox. It sets the content of the Classification ComboBox to the content of the
        /// selected item in the Causes ComboBox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cause_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Cause.SelectedIndex >= 0)
            {   
                Classification.ItemsSource = DN_Henkel_Vision.Memory.Classification.Classifications[((ComboBox)sender).SelectedIndex].ToList();
            }
        }

        /// <summary>
        /// This void is triggered when the user changes the selected item in the Classification
        /// ComboBox. It sets the content of the Type ComboBox to the content of the selected item
        /// in the Classification ComboBox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Classification_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Classification.SelectedIndex >= 0)
            {
                Type.ItemsSource = DN_Henkel_Vision.Memory.Classification.Types[DN_Henkel_Vision.Memory.Classification.ClassificationsPointers[Cause.SelectedIndex][Classification.SelectedIndex]];
            }
        }

        /// <summary>
        /// This functions generattes the string in format "Current Fault/Total Faults"
        /// waiting to be reviewed by the user.
        /// </summary>
        /// <returns>The string in format "Current Fault/Total Faults"</returns>
        public string CurrentFaultLabel()
        {
            return (Cache.CurrentReview + 1).ToString() + "/" + Manager.Selected.ReviewFaults.Count.ToString();
        }

        /// <summary>
        /// This void is triggered when the user clicks on the Approve button.
        /// It approves the current fault, pushes to done faults and moves to
        /// the next one.
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Arguments of the event</param>
        private void Approve_Click(object sender, RoutedEventArgs e)
        {
            ApproveFault();
        }

        /// <summary>
        /// This void is triggered when the user clicks on the Approve button. It approves
        /// the current fault, but still shows it in the review list.
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Arguments of the event</param>
        private void Approve_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            ApproveFault(true);
        }

        private void ApproveFault(bool keep = false)
        {
            Manager.Selected.Faults.Add(PrepareFault());

            if (Manager.Selected.ReviewFaults[Cache.CurrentReview].MachineTime <= 0f)
            {
                Manager.Selected.ReviewFaults[Cache.CurrentReview].UserTime = Manager.AverageTime;
            }

            Manager.Selected.User += Manager.Selected.ReviewFaults[Cache.CurrentReview].UserTime;
            Manager.Selected.Machine += Manager.Selected.ReviewFaults[Cache.CurrentReview].MachineTime;

            //If it is not set to remove, function can return, because following code is just for removing the fault
            if (keep) { return; }

            Manager.Selected.ReviewFaults.RemoveAt(Cache.CurrentReview);

            if (Manager.Selected.ReviewFaults.Count > 0)
            {
                if (Cache.CurrentReview >= Manager.Selected.ReviewFaults.Count)
                {
                    Cache.CurrentReview = Manager.Selected.ReviewFaults.Count - 1;
                }

                Manager.CurrentEditor.FaultPreview.Navigate(typeof(Preview), null, new SuppressNavigationTransitionInfo());

                return;
            }

            Manager.CurrentEditor.Unreview();
        }

        /// <summary>
        /// This void is trigered when the placement text box loses focus. It saves the
        /// content of the text box to the cache.
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Arguments of the event</param>
        private void Placement_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Manager.Selected.IsNetstal()) { return; }

            Cache.LastPlacement = Placement.Text;
        }

        /// <summary>
        /// This void sets the content of the Previews list to the components of the
        /// faults waiting to be reviewed.
        /// </summary>
        public void SetPreviews()
        {
            Previews.Clear();
            
            foreach (Fault fault in Manager.Selected.ReviewFaults)
            {             
                Previews.Add(fault.Component);
            }

            PreviewsList.SelectedIndex = Cache.CurrentReview;
        }

        /// <summary>
        /// This void is triggered when the user changes the selected item in the Previews
        /// list. It sets the current review to the selected item and shows the preview of
        /// the fault.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PreviewsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!Count.Flyout.IsOpen) { return; }

            Cache.CurrentReview = PreviewsList.SelectedIndex;

            Manager.CurrentEditor.FaultPreview.Navigate(typeof(Preview), null, new SuppressNavigationTransitionInfo());
        }

        private Fault PrepareFault()
        {
            Fault output = new(Description.Text);

            output.Component = Component.Text;
            output.Placement = Placement.Text;

            if (Cause.SelectedValue != null) { output.Cause = Cause.SelectedValue.ToString(); }
            else { output.Cause = string.Empty; }

            if (Classification.SelectedValue != null) { output.Classification = Classification.SelectedValue.ToString(); }
            else { output.Classification = string.Empty; }

            if (Type.SelectedValue != null) { output.Type = Type.SelectedValue.ToString(); }
            else { output.Type = string.Empty; }

            output.ClassIndexes[0] = Cause.SelectedIndex;
            output.ClassIndexes[1] = Classification.SelectedIndex;
            output.ClassIndexes[2] = Type.SelectedIndex;

            output.Index = Manager.CreateIndex();

            return output;
        }

        private void Component_TextChanged(object sender, TextChangedEventArgs e)
        {
            //TODO: Upgrade method of replacing based on the index of the Felber's detection 
            if (Component.Text == Manager.Selected.ReviewFaults[Cache.CurrentReview].Component) { return; }
            if (Manager.Selected.ReviewFaults[Cache.CurrentReview].Component == string.Empty) {
                Manager.Selected.ReviewFaults[Cache.CurrentReview].Component = Component.Text;
                return; }

            Description.Text = Description.Text.Replace(Manager.Selected.ReviewFaults[Cache.CurrentReview].Component, Component.Text);

            Manager.Selected.ReviewFaults[Cache.CurrentReview].Component = Component.Text;
        }

        private void Placement_TextChanged(object sender, TextChangedEventArgs e)
        {
            int index = Placement.SelectionStart;
            int length = Placement.SelectionLength;
            
            Placement.Text = Placement.Text.ToUpper();

            Placement.SelectionStart = index;
            Placement.SelectionLength = length;
        }
    }
}
