using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DePeuter.Timesheets.Database.Entities
{
    [SqlTable("jobs")]
    public class Job : EntityBase
    {
        [SqlField("vchCode")]
        public string Code { get; set; }
        [SqlField("vchClient")]
        public string Client { get; set; }
        [SqlField("vchProject")]
        public string Project { get; set; }
    }
}
