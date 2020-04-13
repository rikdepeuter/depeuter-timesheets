using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace System.Timers
{
    public class Timer2 : Timer
    {
        public event EventHandler OnStart;
        public event EventHandler OnStop;

        public void RaiseOnStart(EventArgs e)
        {
            if (OnStart != null)
                OnStart(this, e);
        }
        public void RaiseOnStop(EventArgs e)
        {
            if(OnStop != null)
                OnStop(this, e);
        }

        public bool StopBeforeStart { get; set; }
        public bool TickOnce { get; set; }

        public Timer2()
            : base()
        {
            Elapsed += Timer2_Elapsed;
        }

        public Timer2(double interval)
            : base(interval)
        {
            Elapsed += Timer2_Elapsed;
        }

        void Timer2_Elapsed(object sender, ElapsedEventArgs e)
        {
            if(TickOnce)
                Stop();
        }

        public new void Start()
        {
            base.Stop();

            RaiseOnStart(EventArgs.Empty);

            base.Start();
        }

        public new void Stop()
        {
            RaiseOnStop(EventArgs.Empty);

            base.Stop();
        }

        public void Reset()
        {
            base.Stop();
            base.Start();
        }
    }
}
