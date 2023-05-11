// Copyright(c) DN Foundation and Branislav Juhás.
// Trade secret of DN Foundation.

using Microsoft.UI.Xaml;

namespace DN_Henkel_Vision
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Environment : Window
    {
        public Environment()
        {
            this.InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(ApplicationTitleBar);
        }
    }
}
