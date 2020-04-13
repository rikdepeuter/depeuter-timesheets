using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using System.Data;
using DePeuter.Shared.Advanced;

public static class IEnumerableExtensions
{
    public static List<T> PrefixEmpty<T>(this IEnumerable<T> collection) where T : new()
    {
        return collection.Prefix(new T());
    }

    public static List<string> PrefixEmpty(this IEnumerable<string> collection)
    {
        return collection.Prefix(string.Empty);
    }

    public static List<T> Prefix<T>(this IEnumerable<T> collection, T item)
    {
        var items = new List<T>(collection);
        items.Insert(0, item);
        return items;
    }

    public static int LastIndex<T>(this IEnumerable<T> source)
    {
        var c = source.Count();
        return c == 0 ? 0 : c - 1;
    }
    public static int FirstIndex<T>(this IEnumerable<T> source)
    {
        return 0;
    }

    public static List<List<T>> SplitInto<T>(this IEnumerable<T> collection, int count)
    {
        var batches = new List<List<T>>();
        var batchSize = (int)(collection.Count() / count);

        if(batchSize == 0)
        {
            for(int i = 0; i < collection.Count(); i++)
            {
                batches.Add(collection.Skip(i).Take(1).ToList());
            }

            return batches.Where(x => x.Any()).ToList();
        }

        for(int i = 0; i < count; i++)
        {
            if(i == count - 1)
                batches.Add(collection.Skip(batchSize * (count - 1)).ToList()); // take all remaining
            else
                batches.Add(collection.Skip(batchSize * i).Take(batchSize).ToList());
        }

        return batches.Where(x => x.Any()).ToList();
    }

    public static string Join<T>(this IEnumerable<T> collection, char separator, string prefix, string affix)
    {
        return Join(collection, separator.ToString(), prefix, affix);
    }
    public static string Join<T>(this IEnumerable<T> collection, string separator, string prefix, string affix)
    {
        return string.Format("{0}{1}{2}", prefix, string.Join(separator, collection.Select(x => x == null ? string.Empty : x.ToString()).ToArray()), affix);
    }

    public static string Join<T>(this IEnumerable<T> collection, char separator)
    {
        return Join(collection, separator.ToString());
    }
    public static string Join<T>(this IEnumerable<T> collection, string separator)
    {
        return string.Join(separator, collection.Select(x => x == null ? string.Empty : x.ToString()).ToArray());
    }

    public static List<T> DistinctByEquals<T>(this IEnumerable<T> source)
    {
        return source.Distinct((t1, t2) => t1.Equals(t2));
    }

    public static List<T> Distinct<T>(this IEnumerable<T> source, Func<T, T, bool> equater)
    {
        // copy the source array 
        var result = new List<T>();

        foreach(T item in source)
        {
            if(result.All(t => !equater(item, t)))
            {
                // Doesn't exist already: Add it 
                result.Add(item);
            }
        }

        return result;
    }

    public static IOrderedEnumerable<T> OrderBy<T>(this IEnumerable<T> collection, string propertyName, OrderDirection direction)
    {
        return direction == OrderDirection.Ascending ? OrderBy(collection, propertyName) : OrderByDescending(collection, propertyName);
    }

    public static IOrderedEnumerable<T> OrderBy<T>(this IEnumerable<T> collection, string propertyName)
    {
        if(string.IsNullOrEmpty(propertyName)) return collection.OrderBy(x => x);

        var prop = TypeDescriptor.GetProperties(typeof(T)).Find(propertyName, true);
        if(prop == null)
            throw new ArgumentException("Object: " + typeof(T).FullName + " does not have the specified property: " + propertyName);
        return collection.OrderBy(x => prop.GetValue(x));

        //var itemType = typeof(T);
        //var prop = itemType.GetProperty(propertyName);
        //if(prop == null)
        //    throw new ArgumentException("Object does not have the specified property");
        //var propType = prop.PropertyType;
        //var funcType = typeof(Func<,>).MakeGenericType(itemType, propType);
        //var parameter = Expression.Parameter(itemType, "item");
        //var exp = Expression.Lambda(funcType, Expression.MakeMemberAccess(parameter, prop), parameter);
        //var @params = new object[] { collection, exp.Compile() };
        //return (IOrderedEnumerable<T>)typeof(IEnumerableExtensions).GetMethod("InvokeOrderBy", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(itemType, propType).Invoke(null, @params);
    }

    public static IOrderedEnumerable<T> OrderByDescending<T>(this IEnumerable<T> collection, string propertyName)
    {
        if(string.IsNullOrEmpty(propertyName)) return collection.OrderByDescending(x => x);

        var prop = TypeDescriptor.GetProperties(typeof(T)).Find(propertyName, true);
        if(prop == null)
            throw new ArgumentException("Object: " + typeof(T).FullName + " does not have the specified property: " + propertyName);
        return collection.OrderByDescending(x => prop.GetValue(x));

        //var itemType = typeof(T);
        //var prop = itemType.GetProperty(propertyName);
        //if(prop == null)
        //    throw new ArgumentException("Object does not have the specified property");
        //var propType = prop.PropertyType;
        //var funcType = typeof(Func<,>).MakeGenericType(itemType, propType);
        //var parameter = Expression.Parameter(itemType, "item");
        //var exp = Expression.Lambda(funcType, Expression.MakeMemberAccess(parameter, prop), parameter);
        //var @params = new object[] { collection, exp.Compile() };
        //return (IOrderedEnumerable<T>)typeof(IEnumerableExtensions).GetMethod("InvokeOrderByDescending", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(itemType, propType).Invoke(null, @params);
    }

    //private static IOrderedEnumerable<T> InvokeOrderBy<T, U>(IEnumerable<T> collection, Func<T, U> f)
    //{
    //    return Enumerable.OrderBy(collection, f);
    //}
    //private static IOrderedEnumerable<T> InvokeOrderByDescending<T, U>(IEnumerable<T> collection, Func<T, U> f)
    //{
    //    return Enumerable.OrderByDescending(collection, f);
    //}

    public static DataTable ToDataTable<T>(this IEnumerable<T> collection)
    {
        var type = typeof(T);
        var properties = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance).Where(x => x.CanRead && !x.HasCustomAttribute<IgnoreAttribute>()).ToArray();

        var dt = new DataTable();

        foreach(var prop in properties)
        {
            dt.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
        }

        foreach(var item in collection)
        {
            dt.Rows.Add(properties.Select(x => x.GetValue(item, null) ?? DBNull.Value).ToArray());
        }

        return dt;
    }

    public static IEnumerable<T> AreNull<T>(this IEnumerable<T> collection)
    {
        return collection.Where(x => x == null);
    }
    public static IEnumerable<T> AreNotNull<T>(this IEnumerable<T> collection)
    {
        return collection.Where(x => x != null);
    }

    public static BindingList<T> ToBindingList<T>(this IEnumerable<T> collection)
    {
        if(collection == null) return new BindingList<T>();
        return new BindingList<T>(collection.ToList());
    }

    public static SortableBindingList<T> ToSortableBindingList<T>(this IEnumerable<T> collection) where T : class
    {
        if(collection == null) return new SortableBindingList<T>();
        return new SortableBindingList<T>(collection.ToList());
    }

    public static void Remove<T>(this List<T> collection, Func<T, bool> predicate)
    {
        var count = collection.Count;

        for(var i = 0; i < count; i++)
        {
            if(predicate(collection[i]))
            {
                collection.RemoveAt(i);
                i--;
                count--;
            }
        }
    }

    public static T SingleOrDefault<T>(this IEnumerable<T> collection, string errorMessageFormat, params object[] errorMessageArgs)
    {
        if(collection == null)
        {
            throw new NullReferenceException("Sequence is null");
        }

        var enumerable = collection.ToArray();
        if(enumerable.Length > 1)
            throw new CustomInvalidOperationException("Sequence contains more than 1 element: " + string.Format(errorMessageFormat, errorMessageArgs));

        return enumerable.SingleOrDefault();
    }

    public static T Single<T>(this IEnumerable<T> collection, string errorMessageFormat, params object[] errorMessageArgs)
    {
        if(collection == null)
        {
            throw new NullReferenceException("Sequence is null");
        }

        var enumerable = collection.ToArray();
        if(enumerable.Length == 0)
            throw new CustomInvalidOperationException("Sequence contains no elements: " + string.Format(errorMessageFormat, errorMessageArgs));
        if(enumerable.Length > 1)
            throw new CustomInvalidOperationException("Sequence contains more than 1 element: " + string.Format(errorMessageFormat, errorMessageArgs));

        return enumerable.Single();
    }

    public static T SingleOrDefault<T>(this IEnumerable<T> collection, Func<T, bool> predicate, string errorMessageFormat, params object[] errorMessageArgs)
    {
        if(collection == null)
        {
            throw new NullReferenceException("Sequence is null");
        }

        var enumerable = collection.Where(predicate).ToArray();
        if(enumerable.Length > 1)
            throw new CustomInvalidOperationException("Sequence contains more than 1 element: " + string.Format(errorMessageFormat, errorMessageArgs));

        return enumerable.SingleOrDefault();
    }

    public static T Single<T>(this IEnumerable<T> collection, Func<T, bool> predicate, string errorMessageFormat, params object[] errorMessageArgs)
    {
        if(collection == null)
        {
            throw new NullReferenceException("Sequence is null");
        }

        var enumerable = collection.Where(predicate).ToArray();
        if(enumerable.Length == 0)
            throw new CustomInvalidOperationException("Sequence contains no elements: " + string.Format(errorMessageFormat, errorMessageArgs));
        if(enumerable.Length > 1)
            throw new CustomInvalidOperationException("Sequence contains more than 1 element: " + string.Format(errorMessageFormat, errorMessageArgs));

        return enumerable.Single();
    }

    public static T MaxOrDefault<T>(this IEnumerable<T> collection)
    {
        if(collection == null)
        {
            throw new NullReferenceException("Sequence is null");
        }


        try
        {
            return collection.Max();
        }
        catch(InvalidOperationException ex)
        {
            if(ex.Message == "Sequence contains no elements")
                return default(T);
            throw;
        }
    }
    public static TResult MaxOrDefault<T, TResult>(this IEnumerable<T> collection, Func<T, TResult> selector)
    {
        if(collection == null)
        {
            throw new NullReferenceException("Sequence is null");
        }

        try
        {
            return collection.Max(selector);
        }
        catch(InvalidOperationException ex)
        {
            if(ex.Message == "Sequence contains no elements")
                return default(TResult);
            throw;
        }
    }

    public static T MinOrDefault<T>(this IEnumerable<T> collection)
    {
        if(collection == null)
        {
            throw new NullReferenceException("Sequence is null");
        }

        try
        {
            return collection.Max();
        }
        catch(InvalidOperationException ex)
        {
            if(ex.Message == "Sequence contains no elements")
                return default(T);
            throw;
        }
    }
    public static TResult MinOrDefault<T, TResult>(this IEnumerable<T> collection, Func<T, TResult> selector)
    {
        if(collection == null)
        {
            throw new NullReferenceException("Sequence is null");
        }

        try
        {
            return collection.Max(selector);
        }
        catch(InvalidOperationException ex)
        {
            if(ex.Message == "Sequence contains no elements")
                return default(TResult);
            throw;
        }
    }

    public static StringCollection ToStringCollection(this IEnumerable<string> collection)
    {
        var res = new StringCollection();
        foreach(var item in collection)
        {
            res.Add(item);
        }
        return res;
    }

    public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> collection)
    {
        if(collection == null) return new ObservableCollection<T>();
        return new ObservableCollection<T>(collection);
    }

    public static Queue<T> ToQueue<T>(this IEnumerable<T> collection)
    {
        if(collection == null) return new Queue<T>();
        return new Queue<T>(collection);
    }

    public static IEnumerable<T> OrderByStringWithEqualLength<T>(this IEnumerable<T> collection, Func<T, string> keyselector)
    {
        if(collection == null) return null;
        if(!collection.Any()) return collection;

        var maxLength = collection.Select(x => (keyselector(x) ?? string.Empty).Length).Max();
        if(maxLength == 0) return collection;

        return collection.OrderBy(x => (keyselector(x) ?? string.Empty).Prefix(" ", maxLength - (keyselector(x) ?? string.Empty).Length));
    }

    public static IEnumerable<T> OrderByDescendingStringWithEqualLength<T>(this IEnumerable<T> collection, Func<T, string> keyselector)
    {
        if(collection == null) return null;
        if(!collection.Any()) return collection;

        var maxLength = collection.Select(x => (keyselector(x) ?? string.Empty).Length).Max();
        if(maxLength == 0) return collection;

        return collection.OrderByDescending(x => (keyselector(x) ?? string.Empty).Prefix(" ", maxLength - (keyselector(x) ?? string.Empty).Length));
    }

    public static void ForEach<T>(this IEnumerable<T> collection, Action<T, int> action)
    {
        if(collection == null) return;
        var index = -1;
        foreach(var item in collection)
        {
            index++;
            action(item, index);
        }
    }

    public static IEnumerable<TResult> ForEach<T, TResult>(this IEnumerable<T> collection, Func<T, int, TResult> action)
    {
        var index = -1;
        if(collection != null)
        {
            foreach(var item in collection)
            {
                index++;
                yield return action(item, index);
            }
        }
    }
}