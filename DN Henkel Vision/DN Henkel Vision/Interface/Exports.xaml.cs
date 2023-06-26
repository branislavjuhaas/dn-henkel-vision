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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DN_Henkel_Vision.Interface
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Exports : Page
    {
        public List<int> FrontHeights = new();
        public List<int> BackHeights = new();
        public Exports()
        {
            for (int i = 0; i < 36; i++)
            {
                FrontHeights.Add(Random.Shared.Next(0,79));
                BackHeights.Add(Random.Shared.Next(0,79) + FrontHeights[i]);
            }
            
            this.InitializeComponent();
        }

        public string Date(int days)
        {
            return DateTime.UtcNow.AddDays(-days).ToString("dd.MM");
        }

        private void Machine_Click(object sender, RoutedEventArgs e)
        {

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
    }
}
