using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

public static class IListExtensions
{
    public static string GetCommonString(this List<string> values)
    {
        var index = 0;

        while(true)
        {
            index++;

            if(index > values[0].Length)
                return values[0].Substring(0, index - 1);

            var shortest = values[0].Substring(0, index);

            if(values.Skip(1).Any(x => !x.StartsWith(shortest)))
            {
                if(index == 1)
                    return string.Empty;

                return values[0].Substring(0, index - 1);
            }
        }
    }

    public static BindingList<T> ToBindingList<T>(this IList<T> collection)
    {
        if(collection ==null) return new BindingList<T>();
        return new BindingList<T>(collection);
    }

    //public static bool[] RemoveRange<T>(this IList<T> collection, params T[] items)
    //{
    //    return items.Select(collection.Remove).ToArray();
    //}

    public static List<bool> RemoveRange<T>(this IList<T> collection, IEnumerable<T> items)
    {
        return items.Select(collection.Remove).ToList();
    }
}