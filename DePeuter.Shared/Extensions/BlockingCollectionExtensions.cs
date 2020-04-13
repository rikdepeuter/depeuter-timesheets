using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

public static class BlockingCollectionExtensions
{
    public static void AddRange<T>(this BlockingCollection<T> blockingCollection, IEnumerable<T> collection)
    {
        foreach(var item in collection)
        {
            blockingCollection.Add(item);
        }
    }

    public static void AddRange<T>(this BlockingCollection<T> blockingCollection, IEnumerable<T> collection, CancellationToken cancellationToken)
    {
        foreach(var item in collection)
        {
            blockingCollection.Add(item, cancellationToken);
        }
    }
}