using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DePeuter.Shared.Wpf.Internal
{
    internal class Global
    {
        internal static void InvokeOnGuiThread(Action action)
        {
            InvokeOnGuiThread(() =>
            {
                action();
                return 0;
            });
        }

        internal static T InvokeOnGuiThread<T>(Func<T> action)
        {
            if(Application.Current != null)
            {
                return Application.Current.Dispatcher.Invoke(action);
            }
            
            //TODO winforms gui thread?
            
            return action();
        }
    }
}