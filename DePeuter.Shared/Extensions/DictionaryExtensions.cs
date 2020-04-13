using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class DictionaryExtensions
{
    public static void Set<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
    {
        if (dictionary.ContainsKey(key))
            dictionary[key] = value;
        else
            dictionary.Add(key, value);
    }

    public static TValue Get<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
    {
        if (dictionary.ContainsKey(key))
            return dictionary[key];
    
        return default(TValue);
    }

    public static TKey TryGetKeyByValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TValue value)
    {
        return dictionary.Where(x => x.Value.Equals(value)).Select(x => x.Key).SingleOrDefault();
    }
    public static TKey GetKeyByValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TValue value)
    {
        return dictionary.Where(x => x.Value.Equals(value)).Select(x => x.Key).Single();
    }
    public static TKey[] GetKeysByValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TValue value)
    {
        return dictionary.Where(x => x.Value.Equals(value)).Select(x => x.Key).Distinct().ToArray();
    }

    public static void Remove<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, Func<KeyValuePair<TKey, TValue>, Boolean> expression)
    {
        var toRemove = dictionary.Where(expression).Select(x => x.Key).ToArray();
        foreach (var item in toRemove)
        {
            dictionary.Remove(item);
        }
    }

    public static void AddRange<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, Dictionary<TKey, TValue> otherDictionary)
    {
        foreach (var x in otherDictionary)
        {
            dictionary.Add(x.Key, x.Value);
        }
    }
}