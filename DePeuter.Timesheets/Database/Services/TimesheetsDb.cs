using System;
using System.Collections.Generic;
using System.Linq;
using DePeuter.Timesheets.Database.DTO;
using DePeuter.Timesheets.Database.Entities;

namespace DePeuter.Timesheets.Database.Services
{
    public class TimesheetsDb : DbBase 
    {
        //public List<Job> GetJobs()
        //{
        //    return GetEntities<Job>("vchCode ASC", null, null);
        //}

        //public bool JobExists(string code)
        //{
        //    return ExistsByWhere<Job>(NewQueryObject("vchCode = @code", "code", code));
        //}

        public List<Timesheet> GetTimesheets(string username, DateTime? startDate, DateTime? endDate, bool activeOnly)
        {
            var query = NewQueryObject();

            query.AppendLine("vchUsername = @username");
            query.AddParameter("username", username);

            if (activeOnly)
            {
                query.AppendLine("AND bDeleted = 0");
            }

            if(startDate != null)
            {
                query.AppendLine("AND dteStartTime >= @startDate");
                query.AddParameter("startDate", startDate.Value.Date);
            }

            if(endDate != null)
            {
                query.AppendLine("AND dteEndTime < @endDate");
                query.AddParameter("endDate", endDate.Value.Date.AddDays(1));
            }

            return GetEntitiesByWhere<Timesheet>(query);
        }

        public List<TimesheetSearchResult> SearchTimesheets(string username, DateTime? startDate, DateTime? endDate, bool activeOnly)
        {
            var query = NewQueryObject();

            query.AppendLine("SELECT t.intId, t.dteCreatedAt, t.vchUsername, t.dteStartTime, t.dteEndTime, t.intType");
            query.AppendLine(", coalesce(t.vchTaskNumber, j.vchProject) as vchTaskNumber");
            query.AppendLine(", t.vchDescription");
            query.AppendLine("FROM timesheets.timesheets t");
            query.AppendLine("LEFT JOIN timesheets.jobs j ON j.intid = t.intjobid");
            query.AppendLine("WHERE t.vchUsername = @username");
            query.AddParameter("username", username);

            if(activeOnly)
            {
                query.AppendLine("AND t.bDeleted = 0");
            }

            //if(!string.IsNullOrEmpty(filter))
            //{
            //    var data = filter.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            //    query.AppendLine("AND (");
            //    var fieldNames = new[] { "t.vchDescription", "t.vchTaskNumber" };
            //    for(var i = 0; i < data.Length; i++)
            //    {
            //        if(i > 0)
            //        {
            //            query.Append(" OR ");
            //        }
            //        query.Append(fieldNames.Select(fieldName => string.Format("{0} like '%'+@f{1}+'%'", fieldName, i)).Join(" OR "));
            //        query.AddParameter("f" + i, data[i]);
            //    }
            //    query.AppendLine(")");
            //}

            if(startDate != null)
            {
                query.AppendLine("AND t.dteStartTime >= @startDate");
                query.AddParameter("startDate", startDate.Value.Date);
            }

            if(endDate != null)
            {
                query.AppendLine("AND t.dteEndTime < @endDate");
                query.AddParameter("endDate", endDate.Value.Date.AddDays(1));
            }

            //if (types != null && types.Any())
            //{
            //    query.AppendLine(string.Format("AND t.intType IN ({0})", types.Select(x => (int)x).Join(",")));
            //}

            query.AppendLine("ORDER BY t.dteStartTime ASC");

            return GetEntities<TimesheetSearchResult>(query).Select(x => x.CalculateWeekNumber()).ToList();
        }

        public void UpdateTimesheet(int id, DateTime startDate, DateTime endDate)
        {
            var query = NewQueryObject();

            query.Append("UPDATE timesheets.timesheets SET dteStartTime = @startDate, dteEndTime = @endDate WHERE intId = @id");
            query.AddParameters("id", id, "startDate", startDate, "endDate", endDate);

            ExecuteNonQuery(query);
        }

        public List<SessionSwitch> GetSessionSwitches(string username, DateTime? startDate, DateTime? endDate)
        {
            var query = NewQueryObject();

            query.AppendLine("vchUsername = @username");
            query.AddParameter("username", username);

            if(startDate != null)
            {
                query.AppendLine("AND dteCreatedAt >= @startDate");
                query.AddParameter("startDate", startDate.Value.Date);
            }

            if(endDate != null)
            {
                query.AppendLine("AND dteCreatedAt < @endDate");
                query.AddParameter("endDate", endDate.Value.Date.AddDays(1));
            }

            return GetEntitiesByWhere<SessionSwitch>(query);
        }
    }
}