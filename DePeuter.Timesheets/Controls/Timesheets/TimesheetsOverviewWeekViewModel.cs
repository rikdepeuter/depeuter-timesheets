using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DePeuter.Timesheets.Constants;
using DePeuter.Timesheets.Database.DTO;
using DePeuter.Timesheets.Database.Entities;
using DePeuter.Timesheets.Infrastructure.ViewModel;

namespace DePeuter.Timesheets.Controls.Timesheets
{
    public class TimesheetsOverviewWeekViewModel : ControlViewModelBase
    {
        public int WeekNumber { get; private set; }
        public DateTime DateStart { get; private set; }
        public DateTime DateEnd { get; private set; }

        public List<TimesheetsOverviewDayViewModel> Days { get; set; }

        public double TotalMinutes
        {
            get
            {
                if(Days == null)
                {
                    return 0;
                }
                return Days.Sum(x => x.TotalMinutes);
            }
        }

        public double DeltaMinutes
        {
            get
            {
                if(Days == null)
                {
                    return 0;
                }

                var hoursPerDay = (double)CalendarConstants.HoursWorkWeek/CalendarConstants.DaysWorkWeek;
                var workdDays = Days.Where(x => x.Date <= DateTime.Today).ToArray();
                var daysWorkWeek = workdDays.Length < CalendarConstants.DaysWorkWeek ? workdDays.Length : CalendarConstants.DaysWorkWeek;
                var expectedTotalHoursToWork = daysWorkWeek*hoursPerDay;

                var totalMinutesUntilToday = workdDays.Sum(x => x.TotalMinutes);
                return totalMinutesUntilToday - (expectedTotalHoursToWork * 60);
            }
        }

        public TimesheetsOverviewWeekViewModel(TimesheetsCalendarViewModel parent, DateTime date, List<TimesheetSearchResult> timesheets, List<SessionSwitch> sessionSwitches)
            : base(parent)
        {
            WeekNumber = date.ToWeekNumber();
            DateStart = date.ToPreviousDay(DayOfWeek.Monday);
            DateEnd = date.ToNextDay(DayOfWeek.Sunday);

            Days = new List<TimesheetsOverviewDayViewModel>();
            for(var i = 0; i < 7; i++)
            {
                var dayDate = DateStart.AddDays(i);

                var day = new TimesheetsOverviewDayViewModel(this, dayDate);
                Days.Add(day);

                day.Timesheets.AddRange(timesheets.Where(x => x.StartTime.Date == day.Date).OrderBy(x => x.StartTime));
            }
        }
    }
}
