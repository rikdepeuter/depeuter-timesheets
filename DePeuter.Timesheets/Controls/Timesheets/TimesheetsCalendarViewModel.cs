using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DePeuter.Timesheets.Infrastructure.ViewModel;

namespace DePeuter.Timesheets.Controls.Timesheets
{
    public class TimesheetsCalendarViewModel : ControlViewModelBase
    {
        public List<TimesheetsOverviewWeekViewModel> TimesheetsWeeks
        {
            get { return _timesheetsWeeks; }
            set
            {
                _timesheetsWeeks = value;
                OnPropertyChanged();
            }
        }

        public bool WeekendIsVisible
        {
            get { return _weekendIsVisible; }
            set
            {
                _weekendIsVisible = value;
                OnPropertyChanged();
            }
        }

        public bool SingleWeek
        {
            get { return _singleWeek; }
            set
            {
                _singleWeek = value;
                OnPropertyChanged();
            }
        }

        public int WeekCount
        {
            get { return SingleWeek ? 1 : 3; }
        }

        private List<TimesheetsOverviewWeekViewModel> _timesheetsWeeks;
        private bool _weekendIsVisible;
        private bool _singleWeek;
        
        public TimesheetsCalendarViewModel(ViewModelBase parent)
            : base(parent)
        {
        }
    }
}
