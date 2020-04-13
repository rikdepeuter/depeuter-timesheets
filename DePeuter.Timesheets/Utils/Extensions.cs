using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DePeuter.Timesheets.Utils
{
    public static class Extensions
    {
        public static string GetDateRangeDescriptionUntil(this DateTime dateStart, DateTime dateEnd)
        {
            if(dateStart.Year != dateEnd.Year)
            {
                return string.Format("{0:dd} {0:MMM} {0:yyyy} - {1:dd} {1:MMM} {1:yyyy}", dateStart, dateEnd);
            }

            if(dateStart.Month != dateEnd.Month)
            {
                return string.Format("{0:dd} {0:MMM} - {1:dd} {1:MMM} {1:yyyy}", dateStart, dateEnd);
            }

            return string.Format("{0:dd} - {1:dd} {0:MMM} {0:yyyy}", dateStart, dateEnd);
        }
    }
}
