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
    /// Interaction logic for OverlayLoadingPanel.xaml
    /// </summary>
    public partial class OverlayLoadingPanel : UserControl
    {
        public OverlayLoadingPanel()
        {
            InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                Visibility = Visibility.Collapsed;
            }
        }
    }
}
