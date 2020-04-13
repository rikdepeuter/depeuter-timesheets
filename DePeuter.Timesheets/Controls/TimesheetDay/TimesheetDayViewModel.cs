using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using DePeuter.Shared;
using DePeuter.Timesheets.Constants;
using DePeuter.Timesheets.Controls.Timesheets;
using DePeuter.Timesheets.Database.Contexts;
using DePeuter.Timesheets.Database.Entities;
using DePeuter.Timesheets.Infrastructure.ViewModel;

namespace DePeuter.Timesheets.Controls.TimesheetDay
{
    public class TimesheetDayViewModel : ControlViewModelBase
    {
        public new TimesheetsViewModel Parent { get { return (TimesheetsViewModel) base.Parent; } set { base.Parent = value; } }

        public event EventHandler RequestUpdateItems;
        public event EventHandler<TimePeriodChangingEventArgs> TimePeriodChanged;
        public event EventHandler<TimePeriodChangingEventArgs> RequestUpdateNewItem;
        public event EventHandler<ContextMenuCommandEventArgs> ContextMenuCommandClicked;

        public DateTime Date { get; private set; }
        public Timesheet[] Items { get; private set; }

        public bool SupportAdd { get { return true; } }

        private readonly GetTimesheetsHandler _getTimesheets;

        public TimesheetDayViewModel(ViewModelBase parent, DateTime date, GetTimesheetsHandler getTimesheets)
            : base(parent)
        {
            if(getTimesheets == null)
            {
                throw new ArgumentNullException("getTimesheets");
            }

            _getTimesheets = getTimesheets;

            Date = date.Date;

            TimesheetsContext.My.TimesheetDeleted += OnTimesheetDeleted;
        }

        public ICommand NewContextMenuCommand(string commandName)
        {
            return NewCommand(obj =>
            {
                var item = (Timesheet) obj;
                RaiseContextMenuCommandClicked(this, new ContextMenuCommandEventArgs(commandName, item));
            });
        }
        public void RaiseContextMenuCommandClicked(object sender, ContextMenuCommandEventArgs e)
        {
            if (ContextMenuCommandClicked != null)
            {
                ContextMenuCommandClicked(sender, e);
            }
        }

        public ICommand FillCommand { get { return NewCommand(FillCommand_Execute);} }
        private void FillCommand_Execute(object obj)
        {
            var item = (Timesheet) obj;

            var previousItem = Items.LastOrDefault(x => x.Id != item.Id && x.EndTime <= item.StartTime);
            var nextItem = Items.FirstOrDefault(x => x.Id != item.Id && x.StartTime >= item.EndTime);

            var dayStartTime = Parent.GetDayStartTime(Date);
            var dayEndTime = Parent.GetDayEndTime(Date);

            DateTime? startTime, endTime;
            if (dayStartTime == null && dayEndTime == null)
            {
                startTime = Date.AddHours(CalendarConstants.WorkDayStartHour);
                endTime = startTime.Value.AddHours((double) CalendarConstants.HoursWorkWeek/CalendarConstants.DaysWorkWeek);
            }
            else 
            {
                startTime = previousItem != null ? previousItem.EndTime : dayStartTime;
                endTime = nextItem != null ? nextItem.StartTime : (Date == DateTime.Today ? DateTime.Now.RoundByMinute(CalendarConstants.IntervalMinute) : dayEndTime);
            }

            //startTime = (previousItem != null ? previousItem.EndTime : dayStartTime) ?? Date.AddHours(CalendarConstants.WorkDayStartHour);
            //endTime = (nextItem != null ? nextItem.StartTime : (Date == DateTime.Today ? DateTime.Now.RoundByMinute(CalendarConstants.IntervalMinute) : dayEndTime)) ?? startTime.AddHours((double)CalendarConstants.HoursWorkWeek/CalendarConstants.DaysWorkWeek);

            if (startTime != null && endTime != null)
            {
                item.StartTime = startTime.Value;
                item.EndTime = endTime.Value;

                Parent.UpdateTimesheetTime(item.Id, item.StartTime, item.EndTime);

                UpdateItem(item);    
            }
        }

        protected override void Loaded(object sender, RoutedEventArgs e)
        {
            if(Items == null)
            {
                RefreshData();
            }
        }

        public void RefreshData()
        {
            var items = _getTimesheets(Date, Date);
            SetItems(items);
        }

        public void UpdateItem(Timesheet item)
        {
            SetItems(Items.Where(x => x.Id != item.Id).Concat(new[] { item }.Where(x => !x.IsNew)));
        }
        private void OnTimesheetDeleted(Timesheet item)
        {
            SetItems(Items.Where(x => x.Id != item.Id));
        }

        public void SetItems(IEnumerable<Timesheet> items)
        {
            Items = (items ?? new List<Timesheet>()).OrderBy(x => x.StartTime).ToArray();

            UpdateItems();
        }

        public void CompressDay()
        {
            var items = Items;
            if(!items.Any())
            {
                return;
            }
            
            try
            {
                var dayStartTime = Parent.GetDayStartTime(Date);
                if (dayStartTime != null)
                {
                    dayStartTime = dayStartTime.Value.RoundDownByMinute(CalendarConstants.IntervalMinute);
                }

                TimesheetsContext.My.CompressDay(items, dayStartTime);
            }
            finally
            {
                SetItems(items);
            }
        }

        public void SwapLastTimesheets()
        {
            if(Items.Length < 2)
            {
                return;
            }

            var item1 = Items[Items.Length - 2];
            var item2 = Items[Items.Length - 1];

            try
            {
                TimesheetsContext.My.SwapTimesheets(item1, item2);
            }
            finally
            {
                SetItems(Items);
            }
        }

        private void UpdateItems()
        {
            if(RequestUpdateItems == null)
            {
                throw new InvalidOperationException("RequestUpdateItems is null");
            }

            RequestUpdateItems(this, EventArgs.Empty);
        }

        public void RaiseTimePeriodChanged(object sender, TimePeriodChangingEventArgs e)
        {
            if(TimePeriodChanged != null)
            {
                TimePeriodChanged(sender, e);
            }
        }

        public void RaiseRequestUpdateNewItem(object sender, TimePeriodChangingEventArgs e)
        {
            if(RequestUpdateNewItem != null)
            {
                RequestUpdateNewItem(sender, e);
            }
        }
    }

    public class Timeslot
    {
        public bool IsSelected { get; set; }
        public bool IsCurrent { get; set; }
        public bool IsInUse { get; set; }
        public bool IsIllegal { get; set; }
        public string Tooltip { get; set; }

        public int Index { get; private set; }
        public DateTime Start { get; private set; }
        public DateTime End { get; private set; }

        public string DisplayHour { get; private set; }
        public string DisplayMinute { get; private set; }

        public Timeslot(int index, DateTime start, int timeslotMinutes = 15)
        {
            Index = index;
            Start = start;
            End = start.AddMinutes(timeslotMinutes);

            if(start.Minute == 0 || index == 0)
            {
                DisplayHour = start.Hour.ToString();
            }
            DisplayMinute = start.Minute.ToString("00");
        }
    }
}
