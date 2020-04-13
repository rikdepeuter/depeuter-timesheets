using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

public static class DataGridViewExtensions
{
    public static T SelectedItem<T>(this DataGridView dgv) where T : class
    {
        if (dgv.SelectedRows.Count == 0) return null;

        return dgv.SelectedRows[0].DataBoundItem as T;
    }

    public static T[] SelectedItems<T>(this DataGridView dgv) where T : class
    {
        return dgv.SelectedRows.Cast<DataGridViewRow>().Select(x => x.DataBoundItem as T).Where(x => x != null).ToArray();
    }

    public static T Item<T>(this DataGridView dgv) where T : class
    {
        if(dgv.Rows.Count == 0) return null;

        return dgv.Rows[0].DataBoundItem as T;
    }

    public static T[] Items<T>(this DataGridView dgv) where T : class
    {
        return dgv.Rows.Cast<DataGridViewRow>().Select(x => x.DataBoundItem as T).Where(x => x != null).ToArray();
    }

    public static bool SelectItem<T>(this DataGridView dgv, Func<T, bool> predicate) where T : class
    {
        dgv.ClearSelection();
        for(int i = 0; i < dgv.Rows.Count; i++)
        {
            var rowItem = dgv.Rows[i].DataBoundItem as T;
            if(predicate(rowItem))
            {
                dgv.Rows[i].Selected = true;
                try
                {
                    dgv.FirstDisplayedScrollingRowIndex = i;
                }
                catch { }
                return true;
            }
        }
        return false;
    }

    public static bool SelectItems<T>(this DataGridView dgv, Func<T, bool> predicate) where T : class
    {
        dgv.ClearSelection();
        var res = false;
        for(int i = 0; i < dgv.Rows.Count; i++)
        {
            var rowItem = dgv.Rows[i].DataBoundItem as T;
            if(predicate(rowItem))
            {
                dgv.Rows[i].Selected = true;
                res = true;
                try
                {
                    dgv.FirstDisplayedScrollingRowIndex = i;
                }
                catch { }
            }
        }
        return res;
    }

    public static IEnumerable<T> Select<T>(this DataGridView dgv)
    {
        foreach(DataGridViewRow row in dgv.Rows)
        {
            if(row.DataBoundItem is T)
                yield return (T)row.DataBoundItem;
        }
    }

    public static IEnumerable<T> Selected<T>(this DataGridView dgv)
    {
        foreach(DataGridViewRow row in dgv.SelectedRows)
        {
            if(row.DataBoundItem is T)
                yield return (T)row.DataBoundItem;
        }
    }
}