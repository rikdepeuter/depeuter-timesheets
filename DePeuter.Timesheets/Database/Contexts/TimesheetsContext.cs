using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DePeuter.Timesheets.Database.DTO;
using DePeuter.Timesheets.Database.Entities;
using DePeuter.Timesheets.Database.Services;

namespace DePeuter.Timesheets.Database.Contexts
{
    public class TimesheetsContext
    {
        public static readonly TimesheetsContext My = new TimesheetsContext();

        public event EntitySavedHandler<Timesheet> TimesheetSaved;
        public event EntityDeletedHandler<Timesheet> TimesheetDeleted;

        private readonly List<SessionSwitch> _sessionSwitches = new List<SessionSwitch>();

        private TimesheetsDb NewDb()
        {
            return new TimesheetsDb();
        }

        public int Save(Timesheet entity)
        {
            using (var db = NewDb())
            {
                db.Save(entity);
            }

            if (TimesheetSaved != null)
            {
                TimesheetSaved(entity);
            }

            return entity.Id;
        }

        public void Delete(Timesheet entity)
        {
            using (var db = NewDb())
            {
                db.Delete(entity);
            }

            if (TimesheetDeleted != null)
            {
                TimesheetDeleted(entity);
            }
        }

        public Timesheet GetTimesheet(int id)
        {
            using (var db = NewDb())
            {
                return db.GetEntityById<Timesheet>(id);
            }
        }

        public List<Timesheet> GetTimesheets(string username, DateTime? startDate = null, DateTime? endDate = null, bool activeOnly = true)
        {
            using(var db = NewDb())
            {
                return db.GetTimesheets(username, startDate, endDate, activeOnly);
            }
        }

        public List<TimesheetSearchResult> SearchTimesheets(string username, DateTime? startDate = null, DateTime? endDate = null, bool activeOnly = true)
        {
            using(var db = NewDb())
            {
                return db.SearchTimesheets(username, startDate, endDate, activeOnly);
            }
        }

        public void UpdateTimesheet(int id, DateTime startDate, DateTime endDate)
        {
            using(var db = NewDb())
            {
                db.UpdateTimesheet(id, startDate, endDate);
            }
        }

        public void CompressDay(Timesheet[] items, DateTime? dayStartTime)
        {
            using (var db = NewDb())
            {
                db.StartTransaction();

                var previousItem = items.First();

                if (dayStartTime != null && dayStartTime.Value < previousItem.StartTime)
                {
                    MoveToStartTime(db, previousItem, dayStartTime.Value);
                }

                foreach(var item in items.Skip(1))
                {
                    if(previousItem.EndTime != item.StartTime)
                    {
                        MoveToStartTime(db, item, previousItem.EndTime);
                    }

                    previousItem = item;
                }

                db.CommitTransaction();
            }
        }

        private void MoveToStartTime(TimesheetsDb db, Timesheet item, DateTime startTime)
        {
            var duration = item.EndTime - item.StartTime;
            item.StartTime = startTime;
            item.EndTime = item.StartTime + duration;

            db.UpdateTimesheet(item.Id, item.StartTime, item.EndTime);
        }

        public void SwapTimesheets(Timesheet item1, Timesheet item2)
        {
            var duration1 = item1.EndTime - item1.StartTime;
            var duration2 = item2.EndTime - item2.StartTime;

            item2.StartTime = item1.StartTime;
            item2.EndTime = item2.StartTime + duration2;

            item1.StartTime = item2.EndTime;
            item1.EndTime = item1.StartTime + duration1;

            using (var db = NewDb())
            {
                db.StartTransaction();

                db.UpdateTimesheet(item1.Id, item1.StartTime, item1.EndTime);
                db.UpdateTimesheet(item2.Id, item2.StartTime, item2.EndTime);

                db.CommitTransaction();
            }
        }

        public void InitSessionSwitches(string username)
        {
            using(var db = NewDb())
            {
                var items = db.GetSessionSwitches(username, null, null);
                _sessionSwitches.AddRange(items);
            }
        }

        public void InsertSessionSwitch(SessionSwitch sessionSwitch)
        {
            using (var db = NewDb())
            {
                db.Insert(sessionSwitch);
            }

            _sessionSwitches.Add(sessionSwitch);
        }

        public List<SessionSwitch> GetSessionSwitches(string username, DateTime? startDate, DateTime? endDate)
        {
            var q = _sessionSwitches.Where(x => x.Username == username);

            if(startDate != null)
            {
                q = q.Where(x => x.CreatedAt >= startDate.Value.Date);
            }

            if(endDate != null)
            {
                q = q.Where(x => x.CreatedAt < endDate.Value.Date.AddDays(1));
            }

            return q.ToList();
        }
    }

    public delegate void EntitySavedHandler<in T>(T entity) where T: class;
    public delegate void EntityDeletedHandler<in T>(T entity) where T : class;
}
