using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class LongExtensions
{
    public static long? DefaultAsNull(this long value)
    {
        return value == default(long) ? (long?)null : value;
    }
}