using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DePeuter.Timesheets.Database.Entities
{
    [SqlTable("configuration")]
    public class Configuration : EntityBase
    {
        [SqlField("vchKey")]
        public string Key { get; set; }
        [SqlField("vchValue")]
        public string Value { get; set; }
    }
}
