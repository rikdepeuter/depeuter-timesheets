using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Threading;

public static class DispatcherExtensions
{
    public static T Invoke<T>(this Dispatcher dispatcher, Func<T> action, DispatcherPriority priority = DispatcherPriority.Normal)
    {
        return (T)dispatcher.Invoke(priority, action);
    }
    public static void Invoke(this Dispatcher dispatcher, Action action, DispatcherPriority priority = DispatcherPriority.Normal)
    {
        dispatcher.Invoke(priority, action);
    }
}