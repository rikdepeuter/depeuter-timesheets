using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Specialized;

public static class StringCollectionExtensions
{
    public static IEnumerable<string> AsEnumerable(this StringCollection collection)
    {
        foreach(var value in collection)
        {
            yield return value;
        }
    }

    public static IEnumerable<string> Select(this StringCollection collection)
    {
        foreach(var value in collection)
        {
            yield return value;
        }
    }

    public static StringCollection Copy(this StringCollection collection)
    {
        var res = new StringCollection();
        foreach(var value in collection)
        {
            res.Add(value);
        }
        return res;
    }

    public static IEnumerable<T> SelectMany<T>(this StringCollection collection, Func<string, IEnumerable<T>> selector)
    {
        return collection.Select().SelectMany(selector);
    }

    public static IEnumerable<string> Where(this StringCollection collection, Func<string,bool> predicate)
    {
        return collection.Select().Where(predicate);
    }

    public static string FirstOrDefault(this StringCollection collection)
    {
        if(collection.Count == 0)
            return null;
        return collection[0];
    }

    public static string First(this StringCollection collection)
    {
        if(collection.Count == 0)
            throw new InvalidOperationException("StringCollection contains no elements");
        return collection[0];
    }

    public static string LastOrDefault(this StringCollection collection)
    {
        if(collection.Count == 0)
            return null;
        return collection[collection.Count - 1];
    }

    public static string Last(this StringCollection collection)
    {
        if(collection.Count == 0)
            throw new InvalidOperationException("StringCollection contains no elements");
        return collection[collection.Count - 1];
    }

    public static string SingleOrDefault(this StringCollection collection)
    {
        if(collection.Count > 1)
            throw new InvalidOperationException("StringCollection contains more than one element");
        if(collection.Count == 0)
            return null;
        return collection[0];
    }

    public static string Single(this StringCollection collection)
    {
        if(collection.Count == 0)
            throw new InvalidOperationException("StringCollection contains no elements");
        if(collection.Count > 1)
            throw new InvalidOperationException("StringCollection contains more than one element");
        return collection[0];
    }

    public static bool Any(this StringCollection collection)
    {
        return collection.Count > 0;
    }
}