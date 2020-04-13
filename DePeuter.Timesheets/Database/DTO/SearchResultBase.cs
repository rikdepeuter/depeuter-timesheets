using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DePeuter.Timesheets.Database.DTO
{
    public abstract class SearchResultBase
    {
        [SqlField("intId")]
        public int Id { get; set; }
        [SqlCustomField("dteCreatedAt")]
        public DateTime CreatedAt { get; set; }
    }
}
