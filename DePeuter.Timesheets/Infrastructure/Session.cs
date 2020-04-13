using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using DePeuter.Timesheets.Database.Entities;

namespace DePeuter.Timesheets.Infrastructure
{
    public static class Session
    {
        public static string Username
        {
            get { return WindowsIdentity.GetCurrent().Name; }
        }

        public static void Clear()
        {
            //_jobs = null;
        }

        //private static Job[] _jobs;
        //public static Job[] Jobs
        //{
        //    get
        //    {
        //        if(_jobs == null)
        //        {
        //            using(var context = new TimesheetsContext())
        //            {
        //                _jobs = context.GetJobs().ToArray();
        //            }
        //        }
        //        return _jobs;
        //    }
        //} 
    }
}
