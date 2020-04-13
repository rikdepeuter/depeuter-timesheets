using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

public static class DoubleExtensions
{
    public static string ToInvariantString(this double value)
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }
    public static string ToInvariantString(this double? value)
    {
        if (value == null) return null;
        return value.Value.ToString(CultureInfo.InvariantCulture);
    }

    public static string ToInvariantString(this double value, string format)
    {
        return value.ToString(format, CultureInfo.InvariantCulture);
    }
    public static string ToInvariantString(this double? value, string format)
    {
        if (value == null) return null;
        return value.Value.ToString(format, CultureInfo.InvariantCulture);
    }

    public static double? DefaultAsNull(this double value)
    {
        return value == default(double) ? (double?)null : value;
    }

    public static bool IsEqualTo(this double value, double other)
    {
        return Math.Abs(value - other) < double.Epsilon;
    }
    public static bool IsEqualTo(this double value, double other, double tolerance)
    {
        return Math.Abs(value - other) < tolerance;
    }
    public static bool IsEqualToWithPrecision(this double value, double other, int precision)
    {
        return IsEqualTo(value, other, Math2.PrecisionToTolerance(precision));
    }

    public static string FormatCurrency(this double value, char currencySymbol = '$')
    {
        if(value < 0)
        {
            return string.Format("-{0}{1:0.00}", currencySymbol, value * -1);
        }
        return string.Format("{0}{1:0.00}", currencySymbol, value);
    }
}