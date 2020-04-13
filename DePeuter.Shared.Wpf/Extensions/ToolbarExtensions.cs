using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

public static class ToolbarExtensions
{
    public static void HideOverflowToggleButton(this ToolBar toolBar)
    {
        if(toolBar == null)
        {
            return;
        }

        var overflowGrid = toolBar.Template.FindName("OverflowGrid", toolBar) as FrameworkElement;
        if(overflowGrid != null)
        {
            overflowGrid.Visibility = Visibility.Collapsed;
        }

        var mainPanelBorder = toolBar.Template.FindName("MainPanelBorder", toolBar) as FrameworkElement;
        if(mainPanelBorder != null)
        {
            mainPanelBorder.Margin = new Thickness();
        }
    }
}