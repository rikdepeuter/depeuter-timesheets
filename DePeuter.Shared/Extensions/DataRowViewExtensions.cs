using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

public static class DataRowViewExtensions
{
    public static T GetValue<T>(this DataRowView dr, string column)
    {
        if(dr == null) throw new NullReferenceException("DataRowView is null");

        var value = dr[column];
        if(value == null || value is DBNull) return default(T);

        try
        {
            return (T)value;
        }
        catch(Exception ex)
        {
            throw new InvalidCastException(string.Format("Failed to cast column name {3} with value '{0}' from type '{1}' to type '{2}'", value, value.GetType().Name, typeof(T).Name, column), ex);
        }
    }
    public static T GetValue<T>(this DataRowView dr, int index)
    {
        if(dr == null) throw new NullReferenceException("DataRowView is null");

        var value = dr[index];
        if(value == null || value is DBNull) return default(T);

        try
        {
            return (T)value;
        }
        catch(Exception ex)
        {
            throw new InvalidCastException(string.Format("Failed to cast column index {3} with value '{0}' from type '{1}' to type '{2}'", value, value.GetType().Name, typeof(T).Name, index), ex);
        }
    }

    public static int? GetInteger(this DataRowView dr, string column)
    {
        var value = dr[column];
        if(value == null || value is DBNull) return null;

        return Convert.ToInt32(value);
    }
    public static int? GetInteger(this DataRowView dr, int index)
    {
        var value = dr[index];
        if(value == null || value is DBNull) return null;

        return Convert.ToInt32(value);
    }

    public static double? GetDouble(this DataRowView dr, string column)
    {
        var value = dr[column];
        if(value == null || value is DBNull) return null;

        return Convert.ToDouble(value);
    }
    public static double? GetDouble(this DataRowView dr, int index)
    {
        var value = dr[index];
        if(value == null || value is DBNull) return null;

        return Convert.ToDouble(value);
    }

    public static decimal? GetDecimal(this DataRowView dr, string column)
    {
        var value = dr[column];
        if(value == null || value is DBNull) return null;

        return Convert.ToDecimal(value);
    }
    public static decimal? GetDecimal(this DataRowView dr, int index)
    {
        var value = dr[index];
        if(value == null || value is DBNull) return null;

        return Convert.ToDecimal(value);
    }

    public static string GetString(this DataRowView dr, string column)
    {
        var value = dr[column];
        if(value == null || value is DBNull) return null;

        return value.ToString();
    }
    public static string GetString(this DataRowView dr, int index)
    {
        var value = dr[index];
        if(value == null || value is DBNull) return null;

        return value.ToString();
    }

    public static DateTime? GetDateTime(this DataRowView dr, string column, IFormatProvider provider = null)
    {
        var value = dr[column];
        if(value == null || value is DBNull) return null;

        return provider == null ? Convert.ToDateTime(value) : Convert.ToDateTime(value, provider);
    }
    public static DateTime? GetDateTime(this DataRowView dr, int index, IFormatProvider provider = null)
    {
        var value = dr[index];
        if(value == null || value is DBNull) return null;

        return provider == null ? Convert.ToDateTime(value) : Convert.ToDateTime(value, provider);
    }
    public static string GetDateTime(this DataRowView dr, string column, string format, IFormatProvider provider = null)
    {
        var res = GetDateTime(dr, column, provider);
        if(res == null) return string.Empty;
        return provider == null ? res.Value.ToString(format) : res.Value.ToString(format, provider);
    }
    public static string GetDateTime(this DataRowView dr, int index, string format, IFormatProvider provider = null)
    {
        var res = GetDateTime(dr, index, provider);
        if(res == null) return string.Empty;
        return provider == null ? res.Value.ToString(format) : res.Value.ToString(format, provider);
    }

    public static bool? GetBoolean(this DataRowView dr, string column)
    {
        var value = dr[column];
        if(value == null || value is DBNull) return null;

        return Convert.ToBoolean(value);
    }
    public static bool? GetBoolean(this DataRowView dr, int index)
    {
        var value = dr[index];
        if(value == null || value is DBNull) return null;

        return Convert.ToBoolean(value);
    }
}