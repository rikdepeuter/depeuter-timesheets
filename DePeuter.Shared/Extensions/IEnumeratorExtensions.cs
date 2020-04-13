using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

public static class IEnumeratorExtensions
{
    public static List<T> ToList<T>(this IEnumerator numerator)
    {
        var res = new List<T>();
        numerator.Reset();
        while (numerator.MoveNext())
        {
            res.Add((T)numerator.Current);
        }
        
        return res;
    }

    public static IEnumerable<T> Cast<T>(this IEnumerator numerator)
    {
        numerator.Reset();
        while (numerator.MoveNext())
        {
            yield return (T)numerator.Current;
        }
    }
}