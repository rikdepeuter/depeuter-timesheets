using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DePeuter.Timesheets.Controls.TimesheetDay;
using DePeuter.Timesheets.Controls.Timesheets;
using DePeuter.Timesheets.Infrastructure.ViewModel;

namespace DePeuter.Timesheets.Controls.TimesheetWeek
{
    public class TimesheetWeekViewModel : ControlViewModelBase
    {
        public int WeekNumber { get; set; }
        public TimesheetDayViewModel Day1 { get; set; }
        public TimesheetDayViewModel Day2 { get; set; }
        public TimesheetDayViewModel Day3 { get; set; }
        public TimesheetDayViewModel Day4 { get; set; }
        public TimesheetDayViewModel Day5 { get; set; }

        private readonly GetTimesheetsHandler _getTimesheets;

        public TimesheetWeekViewModel(ViewModelBase parent, DateTime day1, GetTimesheetsHandler getTimesheets)
            : base(parent)
        {
            _getTimesheets = getTimesheets;

            WeekNumber = day1.ToWeekNumber();

            Day1 = new TimesheetDayViewModel(this, day1, getTimesheets);
            Day2 = new TimesheetDayViewModel(this, day1.AddDays(1), getTimesheets);
            Day3 = new TimesheetDayViewModel(this, day1.AddDays(2), getTimesheets);
            Day4 = new TimesheetDayViewModel(this, day1.AddDays(3), getTimesheets);
            Day5 = new TimesheetDayViewModel(this, day1.AddDays(4), getTimesheets);
        }

        protected override void Loaded(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        public void Refresh()
        {
            var result = _getTimesheets(Day1.Date, Day5.Date);
            Day1.SetItems(result.Where(x => x.StartTime.Date == Day1.Date));
            Day2.SetItems(result.Where(x => x.StartTime.Date == Day2.Date));
            Day3.SetItems(result.Where(x => x.StartTime.Date == Day3.Date));
            Day4.SetItems(result.Where(x => x.StartTime.Date == Day4.Date));
            Day5.SetItems(result.Where(x => x.StartTime.Date == Day5.Date));
            //LoadAsync(() => _getCalendarItems(Day1.Date, Day5.Date), (result) =>
            //{
            //    Day1.SetItems(result.Where(x => x.From.Date == Day1.Date));
            //    Day2.SetItems(result.Where(x => x.From.Date == Day2.Date));
            //    Day3.SetItems(result.Where(x => x.From.Date == Day3.Date));
            //    Day4.SetItems(result.Where(x => x.From.Date == Day4.Date));
            //    Day5.SetItems(result.Where(x => x.From.Date == Day5.Date));
            //}, ex =>
            //{
            //    //ignore
            //});
        }
    }
}
