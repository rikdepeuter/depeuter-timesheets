using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

public static class ListBoxExtensions
{
    public static T SelectedItem<T>(this ListBox lb) where T : class
    {
        return lb.SelectedItem as T;
    }

    public static void SelectItem<T>(this ListBox lb, Func<T, bool> predicate) where T : class
    {
        for(int i = 0; i < lb.Items.Count; i++)
        {
            var rowItem = lb.Items[i] as T;
            if(predicate(rowItem))
            {
                lb.SelectedIndex = i;
                return;
            }
        }
    }
}