using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DePeuter.Shared.Database;

namespace DePeuter.Timesheets.Database.Entities
{
    [SqlSchema("timesheets"), SqlPrimaryKey("intId")]
    public abstract class EntityBase : IHasId
    {
        [SqlField("intId")]
        public int Id { get; set; }

        [SqlField("dteCreatedAt")]
        public DateTime CreatedAt { get; set; }

        public bool IsNew { get { return Id <= 0; } }

        protected EntityBase()
        {
            CreatedAt = DateTime.Now;
        }
    }
}
