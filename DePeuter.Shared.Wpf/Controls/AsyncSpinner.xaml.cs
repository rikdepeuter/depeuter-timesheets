using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace DePeuter.Shared.Wpf.Controls.Custom
{
    /// <summary>
    /// Interaction logic for OverlaySpinnerPanel.xaml
    /// </summary>
    public partial class AsyncSpinner : UserControl
    {
        public double SpinnerHeight
        {
            get { return vSpinner.Height; }
            set { vSpinner.Height = value; }
        }

        public AsyncSpinner()
        {
            InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                Visibility = Visibility.Collapsed;
            }
        }
    }
}
