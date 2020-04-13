using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DePeuter.Shared.Wpf.Controls.Custom
{
    public class ToolBar2: ToolBar
    {
        public bool ShowOverflow { get; set; }
        public ToolBar2()
        {

            if(!ShowOverflow)
            {
                Loaded += ToolBar_Loaded;
            }
        }
        private void ToolBar_Loaded(object sender, RoutedEventArgs e)
        {
            var toolBar = (ToolBar)sender;
            var overflowGrid = toolBar.Template.FindName("OverflowGrid", toolBar) as FrameworkElement;
            if (overflowGrid != null)
            {
                overflowGrid.Visibility = Visibility.Collapsed;
            }
            var mainPanelBorder = toolBar.Template.FindName("MainPanelBorder", toolBar) as FrameworkElement;
            if (mainPanelBorder != null)
            {
                mainPanelBorder.Margin = new Thickness();
            }
        }
    }
}
