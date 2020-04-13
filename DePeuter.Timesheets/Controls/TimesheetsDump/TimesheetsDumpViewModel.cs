using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using DePeuter.Shared.Wpf.Controls.Infrastructure;
using DePeuter.Timesheets.Constants;
using DePeuter.Timesheets.Controls.Timesheets;
using DePeuter.Timesheets.Utils;

namespace DePeuter.Timesheets.Controls.TimesheetsDump
{
    public class TimesheetsDumpViewModel : ControlViewModelBase
    {
        public WeekOverview CurrentWeek
        {
            get { return _currentWeek; }
            set
            {
                _currentWeek = value;
                OnPropertyChanged();
                OnPropertyChanged("DateRange");
            }
        }

        public DateTime CurrentDate
        {
            get { return _currentDate; }
            set
            {
                _currentDate = value;
                OnPropertyChanged();
            }
        }

        public string DateRange
        {
            get
            {
                if (CurrentWeek == null)
                {
                    return null;
                }
                return CurrentWeek.DateStart.GetDateRangeDescriptionUntil(CurrentWeek.DateEnd);
            }
        }

        //public ICommand ExportToServiceNowTimePortalCommand { get { return NewCommand(ExportToServiceNowTimePortalCommand_Execute); } }

        private WeekOverview _currentWeek;
        private DateTime _currentDate;

        public TimesheetsDumpViewModel(ViewModelBase parent)
            : base(parent)
        {
        }

        public void SetCurrentWeek(TimesheetsOverviewWeekViewModel timesheetsWeek)
        {
            CurrentDate = timesheetsWeek.DateStart;
            CurrentWeek = new WeekOverview(this, timesheetsWeek);
        }

        //private void ExportToServiceNowTimePortalCommand_Execute(object obj)
        //{
        //    var selfMainWindowHandle = Process.GetCurrentProcess().MainWindowHandle;
        //    var windowHandle = Messaging.GetForegroundWindow();
        //    while (windowHandle == selfMainWindowHandle)
        //    {
        //        Thread.Sleep(100);
        //        windowHandle = Messaging.GetForegroundWindow();
        //    }
            
        //    Thread.Sleep(1000);

        //    Messaging.SendMessage(windowHandle, 0X000C, 0, "aa\ttab\nnewline");
        //    //KeyCollection.FromString("aa\ttab\nnewline").PressBackground(windowHandle);
        //}

        public class WeekOverview : ControlViewModelBase
        {
            public DateTime DateStart { get; private set; }
            public DateTime DateEnd { get; private set; }
            public List<TimesheetWeekOverview> Timesheets { get; set; }

            public WeekOverview(TimesheetsDumpViewModel parent, TimesheetsOverviewWeekViewModel week)
                : base(parent)
            {
                DateStart = week.DateStart;
                DateEnd = week.DateEnd;
                Timesheets = new List<TimesheetWeekOverview>();

                var timesheets = week.Days.SelectMany(x => x.Timesheets).Where(x => x.Type == (int)TimesheetType.Normal).ToList();

                var descriptionTimesheets = timesheets.Where(x => x.TaskNumber == null).ToArray();
                foreach(var description in descriptionTimesheets.Select(x => x.Description).Distinct().OrderBy(x => x))
                {
                    var timesheets2 = descriptionTimesheets.Where(x => x.Description == description).ToArray();

                    var dayTotalHours = timesheets2.Select(x => x.StartTime.DayOfWeek).Distinct().ToDictionary(x => x, x => timesheets2.Where(t => t.StartTime.DayOfWeek == x).Sum(t => Math.Round(t.TotalMinutes/60, 2)).DefaultAsNull());

                    Timesheets.Add(new TimesheetWeekOverview(this, null, description, dayTotalHours));
                }

                var taskNumberTimesheets = timesheets.Where(x => x.TaskNumber != null).ToArray();
                var taskNumbers = taskNumberTimesheets.Select(x => x.TaskNumber).Distinct().OrderBy(x => x).ToList();
                var defaultTaskNumbers = TimesheetsRegistry.GetDefaultTaskNumbers();
                taskNumbers.RemoveRange(defaultTaskNumbers);
                taskNumbers.InsertRange(0, defaultTaskNumbers);

                foreach(var taskNumber in taskNumbers)
                {
                    var timesheets2 = taskNumberTimesheets.Where(x => x.TaskNumber == taskNumber).ToArray();

                    if (timesheets2.Any())
                    {
                        var dayTotalHours = timesheets2.Select(x => x.StartTime.DayOfWeek).Distinct().ToDictionary(x => x, x => timesheets2.Where(t => t.StartTime.DayOfWeek == x).Sum(t => Math.Round(t.TotalMinutes/60, 2)).DefaultAsNull());

                        Timesheets.Add(new TimesheetWeekOverview(this, taskNumber, timesheets2.Select(x => x.Description).Distinct().Join(", ").WithMaxLength(70, "..."), dayTotalHours));    
                    }
                }
            }
        }

        public class TimesheetWeekOverview : ControlViewModelBase
        {
            public string TaskNumber { get; set; }
            public string Description { get; set; }
            public double?[] DayTotalHours { get; set; }
            public double? TotalHours { get; set; }

            //private readonly Dictionary<string, bool> _clickedProperties = new Dictionary<string, bool>();

            //public bool ClickedJobCode { get { return _clickedProperties.Get("JobCode"); } }
            //public bool ClickedStartTime { get { return _clickedProperties.Get("StartTime"); } }
            //public bool ClickedEndTime { get { return _clickedProperties.Get("EndTime"); } }
            //public bool ClickedDescription { get { return _clickedProperties.Get("Description"); } }

            public TimesheetWeekOverview(ViewModelBase parent, string taskNumber, string description, Dictionary<DayOfWeek, double?> dayTotalHours)
                : base(parent)
            {
                TaskNumber = taskNumber;
                Description = description;
                DayTotalHours = new[]
                {
                    dayTotalHours.Get(DayOfWeek.Monday),
                    dayTotalHours.Get(DayOfWeek.Tuesday),
                    dayTotalHours.Get(DayOfWeek.Wednesday),
                    dayTotalHours.Get(DayOfWeek.Thursday),
                    dayTotalHours.Get(DayOfWeek.Friday),
                    dayTotalHours.Get(DayOfWeek.Saturday),
                    dayTotalHours.Get(DayOfWeek.Sunday)
                };
                TotalHours = dayTotalHours.Sum(x => x.Value);
            }

            //public ICommand CopyToClipboardCommand { get { return NewCommand(CopyToClipboardCommand_Execute); } }
            //private void CopyToClipboardCommand_Execute(object obj)
            //{
            //    var propertyName = obj.ToString();

            //    var type = Item.GetType();
            //    var pi = type.GetProperty(propertyName);
            //    var value = pi.GetValue(Item, null);

            //    var time = value as DateTime?;
            //    if(time != null)
            //    {
            //        value = string.Format("{0:H:mm}", time);
            //    }

            //    Clipboard.SetText(value.ToString().Trim('\r').Trim('\n'));

            //    _clickedProperties.Set(propertyName, true);

            //    OnPropertyChanged("Clicked" + propertyName);
            //}
        }
    }
}