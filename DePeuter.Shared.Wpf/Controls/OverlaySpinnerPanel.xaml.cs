using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DePeuter.Shared.Wpf.Controls.Custom
{
    /// <summary>
    /// Interaction logic for OverlaySpinnerPanel.xaml
    /// </summary>
    public partial class OverlaySpinnerPanel : UserControl
    {
        public double SpinnerHeight
        {
            get { return vSpinner.Height; }
            set { vSpinner.Height = value; }
        }

        public OverlaySpinnerPanel()
        {
            InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                Visibility = Visibility.Collapsed;
            }
        }
    }
}
