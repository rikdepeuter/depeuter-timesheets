using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Threading;

public static class ControlExtensions
{
    public static T OnThread<T>(this Control control, Func<T> action)
    {
        return (T)control.Invoke(action);
    }

    public static void OnThread(this Control control, Action action)
    {
        control.Invoke(action);
    }
}