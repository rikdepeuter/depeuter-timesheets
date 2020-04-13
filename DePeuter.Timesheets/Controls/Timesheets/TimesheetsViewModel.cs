using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using DePeuter.Timesheets.Constants;
using DePeuter.Timesheets.Controls.TimesheetDay;
using DePeuter.Timesheets.Controls.TimesheetDetail;
using DePeuter.Timesheets.Controls.TimesheetsDump;
using DePeuter.Timesheets.Database.Contexts;
using DePeuter.Timesheets.Database.DTO;
using DePeuter.Timesheets.Database.Entities;
using DePeuter.Timesheets.Infrastructure;
using DePeuter.Timesheets.Infrastructure.ViewModel;
using DePeuter.Timesheets.Properties;
using DePeuter.Timesheets.Utils;
using Microsoft.Win32;

namespace DePeuter.Timesheets.Controls.Timesheets
{
    public delegate IList<Timesheet> GetTimesheetsHandler(DateTime startDate, DateTime endDate);

    public class TimesheetsViewModel : ControlViewModelBase, ITabViewModel, IShortcutKey
    {
        public string HeaderText { get { return "Timesheets"; } }

        public bool CalendarMode
        {
            get { return _calendarMode; }
            set
            {
                _calendarMode = value;
                OnPropertyChanged();

                SearchTimesheets();
            }
        }

        public DateTime CalendarSelectedDate
        {
            get { return _calendarSelectedDate; }
            set
            {
                _calendarSelectedDate = value.Date;
                OnPropertyChanged();

                TimesheetDayViewModel = new TimesheetDayViewModel(this, value, GetDayItems);
                TimesheetDayViewModel.TimePeriodChanged += TimesheetDayViewModel_TimePeriodChanged;
                TimesheetDayViewModel.ContextMenuCommandClicked += TimesheetDayViewModel_ContextMenuCommandClicked;
                TimesheetDayViewModel.RefreshData();

                if(TimesheetsCalendarViewModel.TimesheetsWeeks != null)
                {
                    foreach(var week in TimesheetsCalendarViewModel.TimesheetsWeeks)
                    {
                        foreach(var day in week.Days)
                        {
                            day.RefreshProperties();
                        }
                    }
                }
            }
        }

        public DateTime CalendarStartDate
        {
            get { return _calendarStartDate; }
            set
            {
                _calendarStartDate = value;
                OnPropertyChanged();
                OnPropertyChanged("DateRange");
            }
        }

        public DateTime CalendarEndDate
        {
            get { return CalendarStartDate.AddWeeks(TimesheetsCalendarViewModel.WeekCount).AddDays(-1); }
        }

        public string DateRange
        {
            get
            {
                return CalendarStartDate.GetDateRangeDescriptionUntil(CalendarEndDate);
            }
        }

        public TimesheetDayViewModel TimesheetDayViewModel
        {
            get { return _timesheetDayViewModel; }
            set
            {
                _timesheetDayViewModel = value;
                OnPropertyChanged();
            }
        }

        public TimesheetDetailViewModel TimesheetDetailViewModel
        {
            get { return _timesheetDetailViewModel; }
            set
            {
                _timesheetDetailViewModel = value;
                OnPropertyChanged();
            }
        }

        public string FilterTimesheets
        {
            get { return _filterTimesheets; }
            set
            {
                _filterTimesheets = value;
                OnPropertyChanged();

                SearchTimesheets();
            }
        }

        public TimesheetsCalendarViewModel TimesheetsCalendarViewModel
        {
            get { return _timesheetsCalendarViewModel; }
            set
            {
                _timesheetsCalendarViewModel = value;
                OnPropertyChanged();
            }
        }

        public List<TimesheetSearchResult> TimesheetsFiltered
        {
            get { return _timesheetsFiltered; }
            set
            {
                _timesheetsFiltered = value;
                OnPropertyChanged();
            }
        }

        public TimesheetsDumpViewModel TimesheetsDumpViewModel
        {
            get { return _timesheetsDumpViewModel; }
            set
            {
                _timesheetsDumpViewModel = value;
                OnPropertyChanged();

                //TimesheetsCalendarViewModel.SingleWeek = value != null;
            }
        }

        private readonly List<TimesheetSearchResult> _data = new List<TimesheetSearchResult>();
        private readonly List<SessionSwitch> _sessionSwitches = new List<SessionSwitch>();

        public void IncrementWeeks(int weekCount)
        {
            var startDate = CalendarStartDate.AddWeeks(weekCount);
            var endDate = startDate.AddWeeks(TimesheetsCalendarViewModel.WeekCount).AddDays(-1);

            var maxEndDate = DateTime.Today.ToNextDay(DayOfWeek.Sunday);
            if(endDate <= maxEndDate)
            {
                CalendarStartDate = startDate;
                SearchTimesheets();
            }
        }

        public ICommand GoToTodayCommand { get { return NewCommand(GoToTodayCommand_Execute); } }
        private void GoToTodayCommand_Execute(object obj)
        {
            CalendarSelectedDate = DateTime.Today;
            CalendarStartDate = CalendarSelectedDate.ToPreviousDay(DayOfWeek.Monday).AddWeeks(-(TimesheetsCalendarViewModel.WeekCount - 1));
            SearchTimesheets();
        }

        public ICommand IncrementWeekCommand { get { return NewCommand(IncrementWeekCommand_Execute); } }
        private void IncrementWeekCommand_Execute(object obj)
        {
            IncrementWeeks(int.Parse(obj.ToString()));
        }

        public ICommand ToggleTimesheetsDumpCommand { get { return NewCommand(ToggleTimesheetsDumpCommand_Execute); } }
        private void ToggleTimesheetsDumpCommand_Execute(object obj)
        {
            ToggleTimesheetsDump();
        }

        public ICommand CompressCommand { get { return NewCommand(CompressCommand_Execute); } }
        private void CompressCommand_Execute(object obj)
        {
            TimesheetDayViewModel.CompressDay();
        }

        public ICommand SwapLastTimesheetsCommand { get { return NewCommand(SwapLastTimesheetsCommand_Execute); } }
        private void SwapLastTimesheetsCommand_Execute(object obj)
        {
            TimesheetDayViewModel.SwapLastTimesheets();
        }

        private void ToggleTimesheetsDump()
        {
            if(TimesheetsDumpViewModel == null)
            {
                //if(!CalendarMode)
                //{
                //  CalendarMode = true;
                //}

                TimesheetsDumpViewModel = new TimesheetsDumpViewModel(this);
                var window = new Window()
                {
                    Content = new TimesheetsDumpView()
                    {
                        DataContext = TimesheetsDumpViewModel
                    },
                    //SizeToContent = SizeToContent.WidthAndHeight,
                    Width = 900,
                    Height = 500,
                    Topmost = true,
                    Icon = Resources.servicenow_icon16.ToBitmapImage(),
                    Title = "ServiceNow overview"
                };
                window.Show();
                window.Closed += (s, e) => TimesheetsDumpViewModel = null;

                //TimesheetsDumpViewModel = new TimesheetsDumpViewModel(this);
                //CalendarStartDate = CalendarStartDate.AddWeeks(2);
                SearchTimesheets();
            }
            //else
            //{
            //    TimesheetsDumpViewModel = null;
            //    CalendarStartDate = CalendarStartDate.AddWeeks(-2);
            //    SearchTimesheets();
            //}
        }

        public ICommand ToggleCalendarModeCommand { get { return NewCommand(ToggleCalendarModeCommand_Execute); } }
        private void ToggleCalendarModeCommand_Execute(object obj)
        {
            CalendarMode = !CalendarMode;
            //if(!CalendarMode && TimesheetsDumpViewModel != null)
            //{
            //    ToggleTimesheetsDump();
            //}
        }

        private DateTime _calendarSelectedDate;
        private TimesheetDayViewModel _timesheetDayViewModel;
        private TimesheetDetailViewModel _timesheetDetailViewModel;
        private List<TimesheetSearchResult> _timesheetsFiltered;
        private TimesheetsDumpViewModel _timesheetsDumpViewModel;
        private string _filterTimesheets;
        private bool _calendarMode = true;
        private DateTime _calendarStartDate;
        private TimesheetsCalendarViewModel _timesheetsCalendarViewModel;

        public TimesheetsViewModel(ViewModelBase parent)
            : base(parent)
        {
            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;

            TimesheetsCalendarViewModel = new TimesheetsCalendarViewModel(this);

            TimesheetsContext.My.TimesheetSaved += TimesheetSaved;
        }

        void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            Logger.DebugFormat("SessionSwitch: {0}", e.Reason);

            try
            {
                var sessionSwitch = new SessionSwitch()
                {
                    Username = Session.Username,
                    Reason = e.Reason.ToString()
                };

                TimesheetsContext.My.InsertSessionSwitch(sessionSwitch);
                _sessionSwitches.Add(sessionSwitch);
            }
            catch(Exception ex)
            {
                HandleException(ex);
            }
        }

        protected override void Loaded(object sender, RoutedEventArgs e)
        {
            SystemEvents_SessionSwitch(this, new SessionSwitchEventArgs(SessionSwitchReason.SessionLogon));

            CalendarStartDate = DateTime.Today.ToPreviousDay(DayOfWeek.Monday).AddWeeks(-(TimesheetsCalendarViewModel.WeekCount - 1));

            Refresh();
        }

        public void Refresh()
        {
            RefreshDateRange(CalendarStartDate, CalendarStartDate.AddWeeks(TimesheetsCalendarViewModel.WeekCount), onComplete: () =>
            {
                if(TimesheetDayViewModel != null)
                {
                    TimesheetDayViewModel.RefreshData();
                }
                else
                {
                    CalendarSelectedDate = DateTime.Today;
                }
            });
        }

        public void RefreshDateRange(DateTime startDate, DateTime endDate, Action onComplete = null)
        {
            var isInitializing = !_data.Any();

            LoadAsync(() =>
            {
                if(isInitializing)
                {
                    TimesheetsContext.My.InitSessionSwitches(Session.Username);

                }

                var timesheets = TimesheetsContext.My.SearchTimesheets(Session.Username, isInitializing ? null : (DateTime?)startDate, isInitializing ? null : (DateTime?)endDate);
                var sessionSwitches = TimesheetsContext.My.GetSessionSwitches(Session.Username, isInitializing ? null : (DateTime?)startDate, isInitializing ? null : (DateTime?)endDate);

                return new { timesheets, startDate, endDate, sessionSwitches };
            }, result =>
            {
                _data.RemoveRange(_data.Where(x => x.StartTime.Date >= result.startDate && x.EndTime.Date <= result.endDate).ToArray());
                _data.AddRange(result.timesheets);

                _sessionSwitches.RemoveRange(_sessionSwitches.Where(x => x.CreatedAt.Date >= result.startDate && x.CreatedAt.Date <= result.endDate).ToArray());
                _sessionSwitches.AddRange(result.sessionSwitches);

                SearchTimesheets();

                if(onComplete != null)
                {
                    onComplete();
                }
            });
        }

        public void SearchTimesheets()
        {
            var predicates = new List<Func<TimesheetSearchResult, bool>>();

            if(!string.IsNullOrEmpty(FilterTimesheets))
            {
                var data = FilterTimesheets.ToLower().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                for(var i = 0; i < data.Length; i++)
                {
                    var dataItem = data[i];
                    predicates.Add(x => (x.TaskNumber ?? string.Empty).ToLower().Contains(dataItem) || (x.Description ?? string.Empty).ToLower().Contains(dataItem));
                }
            }

            if(CalendarMode)
            {
                var startDate = CalendarStartDate;
                var endDate = CalendarEndDate;

                var timesheets = _data.Where(x => x.StartTime.Date >= startDate && x.EndTime.Date <= endDate).Where(predicates).ToList();

                var weeks = new List<TimesheetsOverviewWeekViewModel>();

                for(var i = 0; i < TimesheetsCalendarViewModel.WeekCount; i++)
                {
                    weeks.Add(new TimesheetsOverviewWeekViewModel(TimesheetsCalendarViewModel, CalendarStartDate.AddWeeks(i), timesheets, _sessionSwitches));
                }

                TimesheetsCalendarViewModel.TimesheetsWeeks = weeks;

                if(TimesheetsDumpViewModel != null)
                {
                    TimesheetsDumpViewModel.SetCurrentWeek(weeks.Last());
                }
            }
            else
            {
                TimesheetsFiltered = _data.Where(predicates).OrderByDescending(x => x.StartTime.Date).ThenBy(x => x.StartTime).ToList();
            }
        }

        public IList<Timesheet> GetDayItems(DateTime startDate, DateTime endDate)
        {
            return TimesheetsContext.My.GetTimesheets(Session.Username, startDate, endDate);
        }

        public DateTime? GetDayStartTime(DateTime date)
        {
            return _sessionSwitches.Where(x => x.CreatedAt.Date == date.Date && (x.Reason == SessionSwitchReason.SessionLogon.ToString() || x.Reason == SessionSwitchReason.SessionUnlock.ToString())).OrderBy(x => x.CreatedAt).Select(x => (DateTime?)x.CreatedAt).FirstOrDefault();
        }
        public DateTime? GetDayEndTime(DateTime date)
        {
            return _sessionSwitches.Where(x => x.CreatedAt.Date == date.Date && (x.Reason == SessionSwitchReason.SessionLogoff.ToString() || x.Reason == SessionSwitchReason.SessionLock.ToString())).OrderByDescending(x => x.CreatedAt).Select(x => (DateTime?)x.CreatedAt).FirstOrDefault();
        }
        public SessionSwitch[] GetSessionSwitches(DateTime date)
        {
            return _sessionSwitches.Where(x => x.CreatedAt.Date == date.Date).ToArray();
        }

        public void EditTimesheet(TimesheetSearchResult item)
        {
            if(item.StartTime.Date != CalendarSelectedDate)
            {
                CalendarSelectedDate = item.StartTime.Date;
            }

            EditTimesheet(item.Id);
        }

        private void TimesheetSaved(Timesheet entity)
        {
            TimesheetDayViewModel.UpdateItem(entity);

            var date = entity.StartTime.Date;
            RefreshDateRange(date, date);
        }

        private void EditTimesheet(int id, Timesheet entity = null)
        {
            TimesheetDetailViewModel = new TimesheetDetailViewModel(this, id, entity, _data);
            TimesheetDetailViewModel.RequestClose += (s, e) =>
            {
                if(TimesheetDetailViewModel.Entity.IsNew)
                {
                    TimesheetDayViewModel.UpdateItem(TimesheetDetailViewModel.Entity);
                }
                TimesheetDetailViewModel = null;
            };
        }

        public bool DeleteTimesheet(Timesheet item)
        {
            if(MessageBox.Show("Are you sure you want to delete?", "Timesheet", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return false;
            }

            TimesheetsContext.My.Delete(item);

            var date = item.StartTime.Date;
            RefreshDateRange(date, date);

            return true;
        }

        private void TimesheetDayViewModel_TimePeriodChanged(object sender, TimePeriodChangingEventArgs e)
        {
            try
            {
                if(e.Item.IsNew)
                {
                    if(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    {
                        var entity = new Timesheet()
                        {
                            Username = Session.Username,
                            Type = (int)TimesheetType.Break,
                            StartTime = e.Start,
                            EndTime = e.End
                        };

                        TimesheetsContext.My.Save(entity);
                    }
                    else
                    {
                        EditTimesheet(e.Item.Id, e.Item);
                    }
                }
                else
                {
                    UpdateTimesheetTime(e.Item.Id, e.Start, e.End);
                }
            }
            catch(Exception ex)
            {
                HandleException(ex);
            }
        }

        public void UpdateTimesheetTime(int id, DateTime startTime, DateTime endTime)
        {
            TimesheetsContext.My.UpdateTimesheet(id, startTime, endTime);

            var item = _data.Single(x => x.Id == id);
            item.StartTime = startTime;
            item.EndTime = endTime;

            SearchTimesheets();
        }

        public void FlattenDay(TimesheetsOverviewDayViewModel dayItem)
        {
            //using(var db = new TimesheetsContext())
            //{
            //    var timesheets = db.GetTimesheets(Session.Username, dayItem.Date, dayItem.Date);

            //    //add break item of 30m if missing ONLY if it's 1 timesheet item around lunch
            //    if(timesheets.All(x => x.Type != (int)TimesheetType.Break))
            //    {
            //        //TODO check if only 1 timesheet is between 12am and 2 pm, if yes then add break so that no other timesheets get cut by accident
            //        var lunchStart = dayItem.Date.AddHours(12);
            //        var lunchEnd = dayItem.Date.AddHours(14);
            //        var timesheetsAroundLunch = timesheets.Where(x => x.IntersectsWithTimeRange(lunchStart, lunchEnd)).ToArray();
            //        if(timesheetsAroundLunch.Length == 1)
            //        {
            //            timesheets.Add(new Timesheet()
            //            {
            //                Username = Session.Username,
            //                Type = (int)TimesheetType.Break,
            //                StartTime = dayItem.Date.AddHours(12),
            //                EndTime = dayItem.Date.AddHours(12).AddMinutes(30)
            //            });
            //        }
            //    }

            //    //split long timesheets
            //    foreach(var timesheet in timesheets.OrderBy(x => x.StartTime).ToArray())
            //    {
            //        var activeItem = timesheets.OrderBy(x => x.StartTime).FirstOrDefault(x => x.StartTime < timesheet.StartTime && x.EndTime > timesheet.StartTime);
            //        if(activeItem != null)
            //        {
            //            if(activeItem.EndTime > timesheet.EndTime)
            //            {
            //                timesheets.Add(new Timesheet()
            //                {
            //                    Username = Session.Username,
            //                    StartTime = timesheet.EndTime,
            //                    EndTime = activeItem.EndTime,
            //                    Type = activeItem.Type,
            //                    //JobId = activeItem.JobId,
            //                    Description = activeItem.Description
            //                });
            //            }

            //            activeItem.EndTime = timesheet.StartTime;
            //        }
            //    }

            //    //cluster timesheets together
            //    var flatList = new List<Timesheet>();

            //    foreach(var timesheet in timesheets.OrderBy(x => x.StartTime))
            //    {
            //        var item = flatList.SingleOrDefault(x => x.Type == timesheet.Type && x.Description == timesheet.Description);
            //        if(item == null)
            //        {
            //            flatList.Add(timesheet);
            //        }
            //        else
            //        {
            //            var minutes = (timesheet.EndTime - timesheet.StartTime).TotalMinutes;
            //            item.EndTime = item.EndTime.AddMinutes(minutes);

            //            foreach(var item2 in flatList.Where(x => x.StartTime > item.StartTime))
            //            {
            //                item2.StartTime = item2.StartTime.AddMinutes(minutes);
            //                item2.EndTime = item2.EndTime.AddMinutes(minutes);
            //            }
            //        }
            //    }

            //    foreach(var item in flatList)
            //    {
            //        db.Save(item);
            //    }

            //    var deleteItems = timesheets.Where(x => !x.IsNew && flatList.All(y => y.Id != x.Id)).ToArray();
            //    foreach(var item in deleteItems)
            //    {
            //        db.Delete(item);
            //    }
            //}

            //RefreshDateRange(dayItem.Date, dayItem.Date);

            //if(dayItem.IsSelected)
            //{
            //    TimesheetDayViewModel.RefreshData();
            //}
        }

        public ICommand CalendarDayDoubleClickCommand { get { return NewCommand(CalendarDayDoubleClickCommand_Execute); } }
        private void CalendarDayDoubleClickCommand_Execute(object obj)
        {
            var item = (TimesheetsOverviewDayViewModel)obj;

            if(TimesheetDetailViewModel == null)
            {
                CalendarSelectedDate = item.Date;
            }
        }

        public ICommand TimesheetDoubleClickCommand { get { return NewCommand(TimesheetDoubleClickCommand_Execute); } }
        private void TimesheetDoubleClickCommand_Execute(object obj)
        {
            var item = (TimesheetSearchResult)obj;

            if(TimesheetDetailViewModel != null)
            {
                //TimesheetDetailViewModel.JobPickerViewModel.SelectedId = item.JobId;
                TimesheetDetailViewModel.SelectedType = item.Type;
                if(string.IsNullOrEmpty(TimesheetDetailViewModel.TaskNumber))
                {
                    TimesheetDetailViewModel.TaskNumber = item.TaskNumber;
                }
                TimesheetDetailViewModel.Description = item.Description;
            }
            else
            {
                EditTimesheet(item);
            }
        }

        void TimesheetDayViewModel_ContextMenuCommandClicked(object sender, ContextMenuCommandEventArgs e)
        {
            switch(e.CommandName)
            {
                case "Edit":
                    {
                        EditTimesheet(e.Timesheet.Id);
                        break;
                    }
                case "Delete":
                    {
                        DeleteTimesheet(e.Timesheet);
                        break;
                    }
            }
        }

        public void ProcessShortcutKey(ProcessShortcutKeyEventArgs e)
        {
            if (e.Key == ShortcutKey.Refresh)
            {
                Refresh();
                return;
            }

            if(TimesheetDetailViewModel != null)
            {
                TimesheetDetailViewModel.ProcessShortcutKey(e);
            }

            if(e.Handled)
            {
                return;
            }

            //switch (e.Key)
            //{
            //    case ShortcutKey.Save:
            //        break;
            //}
        }
    }
}