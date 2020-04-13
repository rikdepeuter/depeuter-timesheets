using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DePeuter.Shared.Database
{
    public interface IHasId
    {
        int Id { get; set; }
    }

    public interface IHasDeleted
    {
        bool Deleted { get; set; }
    }
}
