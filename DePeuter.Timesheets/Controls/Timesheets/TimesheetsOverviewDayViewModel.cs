using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using DePeuter.Timesheets.Constants;
using DePeuter.Timesheets.Database.DTO;
using DePeuter.Timesheets.Infrastructure.ViewModel;

namespace DePeuter.Timesheets.Controls.Timesheets
{
    public class TimesheetsOverviewDayViewModel : ControlViewModelBase
    {
        public DateTime Date { get; private set; }

        public DateTime? StartTime { get; private set; }
        public DateTime? EndTime { get; private set; }

        public List<TimesheetSearchResult> Timesheets { get; set; }

        public bool IsToday { get { return Date == DateTime.Today; } }
        public bool IsSelected { get { return TimesheetsViewModel.CalendarSelectedDate == Date; } }
        public double TotalMinutes
        {
            get
            {
                var totalMinutes = Timesheets.Where(x => x.Type != (int)TimesheetType.Break).Select(x => x.EndTime - x.StartTime).Sum(x => x.TotalMinutes);
                return totalMinutes;
            }
        }

        private TimesheetsViewModel TimesheetsViewModel { get { return FindParent<TimesheetsViewModel>(); } }

        public TimesheetsOverviewDayViewModel(TimesheetsOverviewWeekViewModel parent, DateTime date)
            : base(parent)
        {
            Parent = parent;
            Date = date;

            Timesheets = new List<TimesheetSearchResult>();

            RefreshStartEndTimes();
        }

        private void RefreshStartEndTimes()
        {
            var vm = TimesheetsViewModel;
            StartTime = vm.GetDayStartTime(Date);
            EndTime = vm.GetDayEndTime(Date);
        }
        public void RefreshProperties()
        {
            OnPropertyChanged("IsSelected");
            OnPropertyChanged("IsToday");
            OnPropertyChanged("TotalMinutes");
            RefreshStartEndTimes();
        }

        public ICommand FlattenCommand { get { return NewCommand(FlattenCommand_Execute); } }
        private void FlattenCommand_Execute(object obj)
        {
            TimesheetsViewModel.FlattenDay(this);
        }

        public ICommand TimesheetEditCommand { get { return NewCommand(TimesheetEditCommand_Execute); } }
        private void TimesheetEditCommand_Execute(object obj)
        {
            var item = (TimesheetSearchResult)obj;

            TimesheetsViewModel.EditTimesheet(item);
        }
    }
}
