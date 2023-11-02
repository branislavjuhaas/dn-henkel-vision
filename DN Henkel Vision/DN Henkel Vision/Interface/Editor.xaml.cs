using DN_Henkel_Vision.Memory;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.Calls;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;

namespace DN_Henkel_Vision.Interface
{
    /// <summary>
    /// Editor page of the application.
    /// </summary>
    public sealed partial class Editor : Page
    {
        public Action Analyze;

        private bool _locked;
        private bool _reviewing;

        private bool _counting;

        private DateTime _begin;

        public Preview CurrentPreview;

        private Grid LastTapped;

        /// <summary>
        /// Constructor of the Editor page.
        /// </summary>
        public Editor()
        {
            this.InitializeComponent();
            Manager.CurrentEditor = this;
            if (Manager.Selected.IsNetstal())
            {
                Analyze = NetstalAnalyse;
            }
            else
            {
                Analyze = OrderAnalyze;
            }
            if (Manager.Selected.ReviewFaults.Count > 0)
            {
                DataRing.Visibility = Visibility.Collapsed;
                NoDataText.Visibility = Visibility.Collapsed;
                FaultPreview.Navigate(typeof(Preview), null, new SuppressNavigationTransitionInfo());
                _reviewing = true;
            }
        }

        /// <summary>
        /// This void sets the content of the Tact TextBlock to the content of the clicked item in the CauseList
        /// and sets the visibility of the CauseList to Collapsed and locks the output.
        /// </summary>
        private void CauseList_ItemClick(object sender, ItemClickEventArgs e)
        {
            //Tact.Content = e.ClickedItem.ToString();
            Tact.Content = ((e.ClickedItem as Grid).Children[0] as TextBlock).Text;
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

        /// <summary>
        /// This void triggers the analysis of the fault input when the user presses key.
        /// If the user presses Enter, the fault input will be pushed to the fault list.
        /// If the user presses Tab, the fault input will be locked, so the user can't
        /// edit it anymore until it is unlocked.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="e">Arguments of the event.</param>
        private void FaultInput_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {               
                FaultPush();
                return;
            }

            // Check if there is no slash in the fault input and if yes, add one
            if (((int)e.Key) == 220 && !FaultInput.Text.Contains("\\"))
            {
                int index = FaultInput.SelectionStart; 
                FaultInput.Text = FaultInput.Text.Insert(index, "\\");
                FaultInput.SelectionStart = index;
            }

            // Check if the character right after the cursor is slash and if yes, move the cursor one character to the right
            if (e.Key == VirtualKey.Space && FaultInput.SelectionStart < FaultInput.Text.Length && FaultInput.Text[FaultInput.SelectionStart] == '\\' && !IsShift())
            {
                FaultInput.SelectionStart++;
            }

            if (e.Key == VirtualKey.Tab && Tact.Content.ToString() != Windows.ApplicationModel.Resources.ResourceLoader.GetStringForReference(new Uri("ms-resource:B_Cause/Content")) && !_locked)
            {
                Lock();
                e.Handled = true;
            }

            if (_counting) { return; }

            _begin = DateTime.Now;

            _counting = true;
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
            NetstalPlacement.Content = string.Empty;
            FaultInput.Margin = new Thickness(20, 0, 180, 0);
        }

        /// <summary>
        /// This void is trigerred when the user clicks on the fault push button.
        /// It pushes the fault input to the fault list.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="e">Arguments of the event.</param>
        private void PushFault_Click(object sender, RoutedEventArgs e)
        {
            FaultPush();
        }

        /// <summary>
        /// This void updates the AI predicted cause of the fault input in the
        /// Tact button after Felber has analyzed the fault input.
        /// </summary>
        /// <param name="cause">Cause predicted by Felber.</param>
        public void Felber_UpdateCause(string cause)
        {
            if (_locked) { return; }

            Tact.Content = LocalCause(cause);
            Tact.FontStyle = Windows.UI.Text.FontStyle.Italic;
        }

        private static string LocalCause(string cause)
        {
            int index = Array.FindIndex(Memory.Classification.Causes, c => c == cause);

            if (index == -1) return cause;

            return Memory.Classification.LocalCauses[index];
        }

        private static string GlobalCause(string cause)
        {
            int index = Array.FindIndex(Memory.Classification.LocalCauses, c => c == cause);

            if (index == -1) return cause;

            return Memory.Classification.Causes[index];
        }

        /// <summary>
        /// This void pushes the fault input to the fault list and if there is
        /// no fault previewed, it previews the first fault in the fault list.
        /// Otherwise it just updates the count and the flyout of the fault preview.
        /// </summary>
        /// <param name="fault">Fault analyzed by Felber.</param>
        /// <param name="order">Order number of the fault, so it can be checked if
        /// the fault is still relevant.</param>
        public void Felber_UpdateFault(Fault fault, string order)
        {
            if (order != Manager.Selected.OrderNumber) { return; }

            Manager.Selected.ReviewFaults.Add(fault);

            if (_reviewing)
            {
                CurrentPreview.Count.Content = CurrentPreview.CurrentFaultLabel();

                CurrentPreview.SetPreviews();
                
                return;
            }

            Cache.CurrentReview = 0;
            Cache.CurrentReview = 0;

            DataRing.Visibility = Visibility.Collapsed;
            FaultPreview.Navigate(typeof(Preview), null, new SuppressNavigationTransitionInfo());

            _reviewing = true;
        }

        /// <summary>
        /// This void locks the cause of the fault input, so it can't be predicted by Felber.
        /// </summary>
        public void Lock()
        {
            _locked = true;
            Tact.FontStyle = Windows.UI.Text.FontStyle.Normal;
            Tact.Foreground = new SolidColorBrush((Color)Application.Current.Resources["TextFillColorPrimary"]);
        }

        /// <summary>
        /// This void unlocks the cause of the fault input, so it can be predicted by Felber.
        /// </summary>
        public void Unlock()
        {
            _locked = false;
            Tact.Foreground = new SolidColorBrush((Color)Application.Current.Resources["TextFillColorSecondary"]);
            Tact.FontStyle = Windows.UI.Text.FontStyle.Normal;
        }

        /// <summary>
        /// This void resets the fault input, tact button and Netstal placement in case
        /// the current order is Netstal, so the editor could be used for a new fault.
        /// </summary>
        public void ResetEditor()
        {
            FaultInput.Text = string.Empty;            

            Tact.Content = Windows.ApplicationModel.Resources.ResourceLoader.GetStringForReference(new Uri("ms-resource:B_Cause/Content"));
            Unlock();

            UnassignNetstalPlacement();
        }

        /// <summary>
        /// This void pushes the fault input to the Felber, so it could be analyzed.
        /// </summary>
        public void FaultPush()
        {
            if (string.IsNullOrEmpty(FaultInput.Text)) { return; }
            
            string placement = string.Empty;

            if (NetstalPlacement.Content != null)
            {
                placement = NetstalPlacement.Content.ToString();
            }

            _counting = false;

            float user = (float)(DateTime.Now - _begin).TotalMinutes + Manager.Addition;

            Fault input = new(FaultInput.Text, GlobalCause(Tact.Content.ToString())) { Placement = placement, UserTime = user };

            Manager.Selected.PendingFaults.Add(input);

            if (!Felber.Felber.Classifier.IsBusy)
            {
                Felber.Felber.Requeue();
            }

            if (!_reviewing)
            {
                NoDataText.Visibility = Visibility.Collapsed;
                DataRing.Visibility = Visibility.Visible;
            }

            ResetEditor();
        }

        /// <summary>
        /// Clears the FaultPreview content and sets NoDataText visibility to true, indicating unreviewed state.
        /// </summary>
        public void Unreview()
        {
            Manager.CurrentEditor.FaultPreview.Content = null;
            Manager.CurrentEditor.NoDataText.Visibility = Visibility.Visible;
            _reviewing = false;
        }

        /// <summary>
        /// Event handler for the Edit button click.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (LastTapped == null) return;

            int index = FaultsList.GetElementIndex(LastTapped);
            Fault fault = Manager.Selected.Faults[index].Clone();

            Manager.Selected.Faults.RemoveAt(index);
            Manager.Selected.ReviewFaults.Insert(0, fault);

            Cache.CurrentReview = 0;

            DataRing.Visibility = Visibility.Collapsed;
            NoDataText.Visibility = Visibility.Collapsed;
            FaultPreview.Navigate(typeof(Preview), null, new SuppressNavigationTransitionInfo());
        }

        /// <summary>
        /// Deletes the selected fault from the list of faults.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (LastTapped == null) return;

            int index = FaultsList.GetElementIndex(LastTapped);

            Lavender.DeleteFault(Manager.Selected.Faults[index].Index);
            Manager.Selected.Faults.RemoveAt(index);
        }

        /// <summary>
        /// Provides logic for handling the right-tapped event for a fault control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void Fault_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            // Set the last-tapped element to the tapped grid element.
            LastTapped = sender as Grid;
        }

        private void CauseList_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            int index = 0;

            switch (e.Key)
            {
                case VirtualKey.Number0:
                    index = 0; break;
                case VirtualKey.NumberPad0:
                    index = 0; break;
                case VirtualKey.Number1:
                    index = 1; break;
                case VirtualKey.NumberPad1:
                    index = 1; break;
                case VirtualKey.Number2:
                    index = 2; break;
                case VirtualKey.NumberPad2:
                    index = 2; break;
                case VirtualKey.Number3:
                    index = 3; break;
                case VirtualKey.NumberPad3:
                    index = 3; break;
                case VirtualKey.Number4:
                    index = 4; break;
                case VirtualKey.NumberPad4:
                    index = 4; break;
                case VirtualKey.Number5:
                    index = 5; break;
                case VirtualKey.NumberPad5:
                    index = 5; break;
                case VirtualKey.A:
                    index = 6; break;
                case VirtualKey.E:
                    index = 7; break;
                case VirtualKey.B:
                    index = 8; break;
                case VirtualKey.S:
                    index = 9; break;
                case VirtualKey.T:
                    index = 10; break;
                case VirtualKey.L:
                    index = 11; break;
                case VirtualKey.R:
                    index = 12; break;
                case VirtualKey.C:
                    index = 13; break;
                case VirtualKey.W:
                    index = 14; break;
                case VirtualKey.P:
                    index = 15; break;
                case VirtualKey.O:
                    index = 16; break;
                default: return;
            }

            e.Handled = true;

            Tact.Content = Memory.Classification.LocalCauses[index];
            Tact.Flyout.Hide();

            if (_locked) { return; }
            Lock();
        }

        public List<ListViewItem> Causes()
        {
            string[] shortcuts = new string[] { "0", "1", "2", "3", "4", "5", "A", "E", "B", "S", "T", "L", "R", "C", "W", "P", "O" };

            List<ListViewItem> causes = new();

            for (int i = 0; i < Memory.Classification.LocalCauses.Length; i++)
            {
                Grid grid = new();

                TextBlock text = new()
                {
                    Text = Memory.Classification.LocalCauses[i],
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Left
                };

                TextBlock shortcut = new()
                {
                    Text = shortcuts[i],
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Foreground = new SolidColorBrush((Color)Application.Current.Resources["TextFillColorSecondary"])
            };

                grid.Children.Add(text);
                grid.Children.Add(shortcut);

                ListViewItem item = new()
                {
                    Content = grid
                };

                causes.Add(item);
            }

            return causes;
        }
    }
}
