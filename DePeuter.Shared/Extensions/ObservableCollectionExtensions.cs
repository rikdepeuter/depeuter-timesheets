using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

public static class ObservableCollectionExtensions
{
    public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
    {
        if(items == null)
        {
            throw new ArgumentNullException("items");
        }

        foreach(var item in items)
        {
            collection.Add(item);
        }
    }
}