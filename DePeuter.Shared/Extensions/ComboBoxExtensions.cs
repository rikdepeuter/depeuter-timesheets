using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Linq.Expressions;

public static class ComboBoxExtensions
{
    public static void SetDatasource<T>(this ComboBox sender, IEnumerable<T> collection, Expression<Func<T, string>> displayMember, Expression<Func<T, object>> valueMember)
    {
        sender.DataSource = collection.ToList();
        if (displayMember != null)
            sender.DisplayMember = displayMember.GetMember();
        if (valueMember != null)
            sender.ValueMember = valueMember.GetMember();
    }

    public static T GetSelectedValue<T>(this ComboBox sender)
    {
        object selectedValue = null;
        try
        {
            selectedValue = sender.SelectedValue;
        }
        catch { }

        if(selectedValue == null) return default(T);

        return (T)selectedValue;
    }
}