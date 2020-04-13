using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class IntExtensions
{
    public static string Repeat(this int amount, string value)
    {
        var res = string.Empty;
        for (int i = 0; i < amount; i++)
        {
            res += value;
        }
        return res;
    }

    public static int? DefaultAsNull(this int value)
    {
        return value == default(int) ? (int?)null : value;
    }

    public static bool IsEven(this int value)
    {
        return value%2 == 0;
    }
    public static bool IsUneven(this int value)
    {
        return value%2 == 1;
    }
}