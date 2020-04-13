using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

public static class DataColumnCollectionExtensions
{
    public static IEnumerable<DataColumn> AsEnumerable(this DataColumnCollection collection)
    {
        return collection.Cast<DataColumn>();
    }
}