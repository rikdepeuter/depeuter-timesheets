using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DePeuter.Timesheets.Database.Entities;

namespace DePeuter.Timesheets.Utils
{
    public static class DtoExtensions
    {
        public static bool HasTime(this Timesheet item)
        {
            return (item.EndTime - item.StartTime).TotalMinutes > 0;
        }

        public static bool ContainsTime(this Timesheet item, DateTime value)
        {
            return item.StartTime <= value && item.EndTime >= value;
        }

        public static bool IsWithinTimeRange(this Timesheet item, DateTime rangeStart, DateTime rangeEnd)
        {
            return item.StartTime >= rangeStart && item.EndTime <= rangeEnd;
        }

        public static bool IntersectsWithTimeRange(this Timesheet item, DateTime rangeStart, DateTime rangeEnd)
        {
            return IsWithinTimeRange(item, rangeStart, rangeEnd) || (item.StartTime <= rangeStart && item.EndTime > rangeStart) || (item.StartTime < rangeEnd && item.EndTime >= rangeEnd);
        }
    }
}
