using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

public static class Convert2
{
    public static string ToString(object value, IFormatProvider provider = null)
    {
        if(value is DBNull || value == null) return null;

        return provider == null ? Convert.ToString(value) : Convert.ToString(value, provider);
    }

    public static bool? ToNullableBoolean(object value, IFormatProvider provider = null)
    {
        if(value is DBNull || value == null || (value is string && value.ToString() == string.Empty)) return null;

        return provider == null ? Convert.ToBoolean(value) : Convert.ToBoolean(value, provider);
    }

    public static int? ToNullableInteger(object value, IFormatProvider provider = null)
    {
        if(value is DBNull || value == null || (value is string && value.ToString() == string.Empty)) return null;
        if(!Math2.IsInteger(value.ToString())) return null;

        return provider == null ? Convert.ToInt32(value) : Convert.ToInt32(value, provider);
    }

    public static double? ToNullableDouble(object value, IFormatProvider provider = null)
    {
        if(value is DBNull || value == null || (value is string && value.ToString() == string.Empty)) return null;

        return provider == null ? Convert.ToDouble(value) : Convert.ToDouble(value, provider);
    }

    public static decimal? ToNullableDecimal(object value, IFormatProvider provider = null)
    {
        if(value is DBNull || value == null || (value is string && value.ToString() == string.Empty)) return null;

        return provider == null ? Convert.ToDecimal(value) : Convert.ToDecimal(value, provider);
    }

    public static DateTime? ToNullableDateTime(object value, IFormatProvider provider = null)
    {
        if(value is DBNull || value == null || (value is string && value.ToString() == string.Empty)) return null;
        
        return provider == null ? Convert.ToDateTime(value) : Convert.ToDateTime(value, provider);
    }

    public static string ToNullableInteger(object value, string format, IFormatProvider provider = null)
    {
        var res = ToNullableInteger(value, provider);
        if(res == null) return string.Empty;
        return provider == null ? res.Value.ToString(format) : res.Value.ToString(format, provider);
    }

    public static string ToNullableDouble(object value, string format, IFormatProvider provider = null)
    {
        var res = ToNullableDouble(value, provider);
        if(res == null) return string.Empty;
        return provider == null ? res.Value.ToString(format) : res.Value.ToString(format, provider);
    }

    public static string ToNullableDecimal(object value, string format, IFormatProvider provider = null)
    {
        var res = ToNullableDecimal(value, provider);
        if(res == null) return string.Empty;
        return provider == null ? res.Value.ToString(format) : res.Value.ToString(format, provider);
    }

    public static DateTime? ToNullableDateTimeExact(object value, string format, IFormatProvider provider = null)
    {
        if(value is DBNull || value == null || (value is string && value.ToString() == string.Empty)) return null;

        return DateTime.ParseExact(value.ToString(), format, provider ?? CultureInfo.CurrentCulture);
    }

    public static string ToNullableDateTime(object value, string format, IFormatProvider provider = null)
    {
        var res = ToNullableDateTime(value, provider);
        if(res == null) return string.Empty;
        return provider == null ? res.Value.ToString(format) : res.Value.ToString(format, provider);
    }

    public static object ChangeType(object value, Type conversionType)
    {
        if (value == null) return null;

        conversionType = Nullable.GetUnderlyingType(conversionType) ?? conversionType;

        try
        {
            return Convert.ChangeType(value, conversionType);
        }
        catch(FormatException ex)
        {
            throw new FormatException(string.Format("{0} with value ({1})", ex.Message, value), ex);
        }
    }

    public static object ChangeType(object value, Type conversionType, IFormatProvider provider)
    {
        if(value == null) return null;

        conversionType = Nullable.GetUnderlyingType(conversionType) ?? conversionType;

        try
        {
            return Convert.ChangeType(value, conversionType, provider);
        }
        catch(FormatException ex)
        {
            throw new FormatException(string.Format("{0} with value ({1})", ex.Message, value), ex);
        }
    }
}