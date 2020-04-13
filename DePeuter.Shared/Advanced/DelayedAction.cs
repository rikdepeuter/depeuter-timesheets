using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;

namespace DePeuter.Shared.Advanced
{
    public class DelayedAction
    {
        private static readonly object Lock = new object();
        private static readonly Dictionary<object, Dictionary<string, Timer>> Timers = new Dictionary<object, Dictionary<string, Timer>>();
        private static readonly Dictionary<object, Dictionary<string, Dispatcher>> Dispatchers = new Dictionary<object, Dictionary<string, Dispatcher>>();

        private DelayedAction()
        {
        }

        public static void Invoke(object owner, Action action, TimeSpan dueTime, string key)
        {
            Invoke(owner, action, (long)dueTime.TotalMilliseconds, key);
        }
        public static void Invoke(object owner, Action action, long dueTime, string key)
        {
            lock(Lock)
            {
                if(!Timers.ContainsKey(owner))
                {
                    Timers.Add(owner, new Dictionary<string, Timer>());
                    Dispatchers.Add(owner, new Dictionary<string, Dispatcher>());
                }

                var timers = Timers[owner];
                var dispatchers = Dispatchers[owner];

                if(!timers.ContainsKey(key))
                {
                    dispatchers.Add(key, Dispatcher.CurrentDispatcher);
                    timers.Add(key, new Timer(state =>
                    {
                        timers[key].Change(Timeout.Infinite, Timeout.Infinite);

                        dispatchers[key].Invoke(action);
                    }, null, dueTime, -1));
                }
            }

            Timers[owner][key].Change(dueTime, Timeout.Infinite);
            Dispatchers[owner][key] = Dispatcher.CurrentDispatcher;
        }
    }
}
