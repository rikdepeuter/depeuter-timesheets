using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DePeuter.Timesheets.Constants;
using DePeuter.Timesheets.Infrastructure.Controls;
using Microsoft.Win32;

namespace DePeuter.Timesheets.Controls.Timesheets
{
    /// <summary>
    /// Interaction logic for TimesheetsView.xaml
    /// </summary>
    public partial class TimesheetsView : UserControlBase
    {
        private TimesheetsViewModel _viewModel;
        private TimesheetsViewModel ViewModel
        {
            get
            {
                if (_viewModel == null)
                {
                    _viewModel = (TimesheetsViewModel)DataContext;    
                }
                return _viewModel;
            }
        }

        private DateTime _currentDay = DateTime.Today;
        private readonly Timer _dayTimer;

        private bool _weekendIsVisibleOverride;

        public TimesheetsView()
        {
            InitializeComponent();

            _dayTimer = new Timer(Callback);
            _dayTimer.Change(0, 0);

            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
        }

        void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            ChangeDay();
        }

        private void Callback(object sender)
        {
            _dayTimer.Change(DateTime.Today.AddDays(1) - DateTime.Now, TimeSpan.Zero);
            
            Dispatcher.Invoke(ChangeDay);
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            timesheetDayView1.Initialize(CalendarConstants.StartHour, CalendarConstants.StartMinute, CalendarConstants.IntervalMinute);
        }

        private void ChangeDay()
        {
            var vm = ViewModel;

            var today = DateTime.Today;
            if(_currentDay != today)
            {
                _currentDay = today;

                vm.CalendarSelectedDate = _currentDay;
                timesheetDayView1.ScrollToStart();
            }

            var calendar = vm.TimesheetsCalendarViewModel;
            if(!calendar.WeekendIsVisible && (_currentDay.DayOfWeek == DayOfWeek.Saturday || _currentDay.DayOfWeek == DayOfWeek.Sunday))
            {
                _weekendIsVisibleOverride = true;
                calendar.WeekendIsVisible = _weekendIsVisibleOverride;
            }
            else if(calendar.WeekendIsVisible && _weekendIsVisibleOverride && _currentDay.DayOfWeek != DayOfWeek.Saturday && _currentDay.DayOfWeek != DayOfWeek.Sunday)
            {
                _weekendIsVisibleOverride = false;
                calendar.WeekendIsVisible = _weekendIsVisibleOverride;
            }    
        }

        private void TimesheetsCalendarOverview_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var vm = ViewModel;

            vm.IncrementWeeks(e.Delta < 0 ? 1 : -1);
        }
        
        private void DataGrid_OnCopyingRowClipboardContent(object sender, DataGridRowClipboardEventArgs e)
        {
            e.ClipboardRowContent.Clear();
            e.ClipboardRowContent.Add(new DataGridClipboardCellContent(e.Item, ((DataGrid)sender).Columns[e.StartColumnDisplayIndex], "Abc-hello"));
        }

        private void TimesheetsView_OnLayoutUpdated(object sender, EventArgs e)
        {
            ChangeDay();
        }

        private void Toolbar_Loaded(object sender, RoutedEventArgs e)
        {
            ((ToolBar)sender).HideOverflowToggleButton();
        }
    }
}