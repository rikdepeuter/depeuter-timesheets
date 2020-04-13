using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace DePeuter.Shared.Wpf.Controls.Custom
{
    /// <summary>
    /// Interaction logic for OverlayLoadingPanel.xaml
    /// </summary>
    public partial class AsyncLoader : UserControl
    {
        public AsyncLoader()
        {
            InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                Visibility = Visibility.Collapsed;
            }
        }
    }
}