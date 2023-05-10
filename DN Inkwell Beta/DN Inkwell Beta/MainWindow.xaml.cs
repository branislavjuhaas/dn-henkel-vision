// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DN_Inkwell_Beta
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public ObservableCollection<Note> Notes = new ObservableCollection<Note>();
        
        public MainWindow()
        {
            this.InitializeComponent();

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(ApplicationTitleBar);

            Notes.Add(new Note("Hello World"));
            Notes.Add(new Note("Hello World"));
            Notes.Add(new Note("Hello World"));

            Notes[0].Tags.Add("# Hello");
            Notes[0].Tags.Add("# Hello");
            Notes[0].Tags.Add("# Hello");
            
            Notes[1].Tags.Add("#Ahooj");
            Notes[1].Tags.Add("#Ahooj");
            Notes[1].Tags.Add("#Ahooj");
        }

        private void StackPanel_Loaded(object sender, RoutedEventArgs e)
        {
            //Calendarqe.MaxDate = DateTime.Now;
        }
    }

    public class Note
    {
        public List<string> Tags;
        public String Text;
        public float Rating;

        public Note(string text)
        {
            Tags = new List<string>();
            Text = text;
        }
    }
}
