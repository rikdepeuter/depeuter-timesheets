using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

public static class DataTableExtensions2
{
    public static IEnumerable<T> Select<T>(this DataTable dt, Func<DataRow, T> selector)
    {
        return dt.Select().Select(selector);
    }

    public static IEnumerable<T> SelectMany<T>(this DataTable dt, Func<DataRow, IEnumerable<T>> selector)
    {
        return dt.Select().SelectMany(selector);
    }

    public static IEnumerable<DataRow> Where(this DataTable dt, Func<DataRow, bool> predicate)
    {
        return dt.Select().Where(predicate);
    }

    public static DataRow FirstOrDefault(this DataTable dt)
    {
        if(dt.Rows == null || dt.Rows.Count == 0)
            return null;
        return dt.Rows[0];
    }

    public static DataRow First(this DataTable dt)
    {
        if(dt.Rows == null || dt.Rows.Count == 0)
            throw new InvalidOperationException("DataTable contains no rows");
        return dt.Rows[0];
    }

    public static DataRow LastOrDefault(this DataTable dt)
    {
        if(dt.Rows == null || dt.Rows.Count == 0)
            return null;
        return dt.Rows[dt.Rows.Count - 1];
    }

    public static DataRow Last(this DataTable dt)
    {
        if(dt.Rows == null || dt.Rows.Count == 0)
            throw new InvalidOperationException("DataTable contains no rows");
        return dt.Rows[dt.Rows.Count - 1];
    }

    public static DataRow SingleOrDefault(this DataTable dt)
    {
        if(dt.Rows != null && dt.Rows.Count > 1)
            throw new InvalidOperationException("DataTable contains more than one row");
        if(dt.Rows == null || dt.Rows.Count == 0)
            return null;
        return dt.Rows[0];
    }

    public static DataRow Single(this DataTable dt)
    {
        if(dt.Rows == null || dt.Rows.Count == 0)
            throw new InvalidOperationException("DataTable contains no rows");
        if(dt.Rows != null && dt.Rows.Count > 1)
            throw new InvalidOperationException("DataTable contains more than one row");
        return dt.Rows[0];
    }

    public static bool Any(this DataTable dt)
    {
        return dt.Rows != null && dt.Rows.Count > 0;
    }

    public static DataTable ReplaceDbNullByNull(this DataTable dt)
    {
        for (var r = 0; r < dt.Rows.Count; r++)
        {
            for (var c = 0; c < dt.Columns.Count; c++)
            {
                if (dt.Rows[r][c] is DBNull)
                {
                    dt.Rows[r][c] = null;
                }
            }
        }

        return dt;
    }
}