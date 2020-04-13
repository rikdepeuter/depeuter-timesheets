using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DePeuter.Shared
{
    public static class StopwatchExtensions
    {
        public static void WaitUntil(this Stopwatch sw, TimeSpan timeout)
        {
            WaitUntil(sw, (long)timeout.TotalMilliseconds);
        }

        public static void WaitUntil(this Stopwatch sw, long timeoutMs)
        {
            var remainingMs = (int)(timeoutMs - sw.ElapsedMilliseconds);
            if(remainingMs > 0)
            {
                Thread.Sleep(remainingMs);
            }
        }
    }
}
