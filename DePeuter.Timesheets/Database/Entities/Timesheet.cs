using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DePeuter.Shared.Database;

namespace DePeuter.Timesheets.Database.Entities
{
    [SqlTable("timesheets")]
    public class Timesheet : EntityBase, IHasDeleted
    {
        [SqlField("vchUsername")]
        public string Username { get; set; }
        //[SqlField("intJobId")]
        //public int? JobId { get; set; }
        [SqlField("dteStartTime")]
        public DateTime StartTime { get; set; }
        [SqlField("dteEndTime")]
        public DateTime EndTime { get; set; }
        [SqlField("vchDescription")]
        public string Description { get; set; }
        [SqlField("intType")]
        public int Type { get; set; }
        [SqlField("vchTaskNumber")]
        public string TaskNumber { get; set; }

        [SqlField("bDeleted")]
        public bool Deleted { get; set; }
    }
}
