using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace DePeuter.Timesheets.Database.Entities
{
    [SqlTable("session_switches")]
    public class SessionSwitch : EntityBase
    {
        [SqlField("vchUsername")]
        public string Username { get; set; }
        [SqlField("vchReason")]
        public string Reason { get; set; }

        public SessionSwitchReason ReasonEnum { get { return Reason.ToEnum<SessionSwitchReason>(); } }
    }
}
