using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

public static class DataGridViewColumnCollectionExtensions
{
    public static DataGridViewColumn First(this DataGridViewColumnCollection columns)
    {
        if(columns.Count == 0)
            throw new InvalidOperationException("DataGridViewColumnCollection contains no columns");
        return columns[0];
    }
    public static DataGridViewColumn FirstOrDefault(this DataGridViewColumnCollection columns)
    {
        if (columns.Count == 0)
            return null;
        return columns[0];
    }

    public static DataGridViewColumn Last(this DataGridViewColumnCollection columns)
    {
        if(columns.Count == 0)
            throw new InvalidOperationException("DataGridViewColumnCollection contains no columns");
        return columns[columns.Count - 1];
    }
    public static DataGridViewColumn LastOrDefault(this DataGridViewColumnCollection columns)
    {
        if(columns.Count == 0)
            return null;
        return columns[columns.Count - 1];
    }

    public static int LastIndex(this DataGridViewColumnCollection columns)
    {
        if(columns.Count == 0)
            return -1;
        return columns.Count - 1;
    }
}