using System;
using System.Collections.Generic;
using System.Linq;
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
using DePeuter.Timesheets.Controls.Timesheets;

namespace DePeuter.Timesheets.Controls.TimesheetDay
{
    /// <summary>
    /// Interaction logic for CurrentTimeControl.xaml
    /// </summary>
    public partial class CurrentTimeControl : UserControl
    {
        private readonly TimesheetDayView _dayView;

        public CurrentTimeControl()
        {
            InitializeComponent();
        }

        public CurrentTimeControl(TimesheetDayView dayView)
            : this()
        {
            _dayView = dayView;
        }

        private int GetRemainingSecondsUntilNextMinute()
        {
            return 60 - DateTime.Now.Second;
        }

        private void Callback(object sender)
        {
            var timer = (Timer)sender;
            timer.Change(TimeSpan.FromSeconds(GetRemainingSecondsUntilNextMinute()), TimeSpan.Zero);

            Dispatcher.Invoke(RefreshTime);
        }

        public void RefreshTime()
        {
            var now = DateTime.Now;
            var nowMinutes = now.Hour*60 + now.Minute;
            var intervalsPerHour = 60.0/_dayView.IntervalMinute;
            var itemsHeightPerHour = _dayView.ItemsHeight.Value * intervalsPerHour;
            var itemsHeightPerMinute = itemsHeightPerHour/60;

            Margin = new Thickness(0, nowMinutes*itemsHeightPerMinute, 0, 0);
            tbTime.Text = now.ToString("H:mm");

            Visibility = _dayView.ViewModel.Date == DateTime.Today ? Visibility.Visible : Visibility.Collapsed;
        }

        private void CurrentTimeControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            var timer = new Timer(Callback);
            timer.Change(0, 0);
        }

        private void NewTimesheetUntilNow_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _dayView.SetNewItemFromStartUntilNow();
            }
            catch(Exception ex)
            {
                _dayView.ViewModel.HandleException(ex);
            }
        }

        private void FinishLastTimesheet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _dayView.UpdateLastTimesheetToNow();
            }
            catch(Exception ex)
            {
                _dayView.ViewModel.HandleException(ex);
            }
        }

        private void CompressDay_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _dayView.ViewModel.CompressDay();
            }
            catch(Exception ex)
            {
                _dayView.ViewModel.HandleException(ex);
            }
        }

        private void SwapLastTimesheets_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _dayView.ViewModel.SwapLastTimesheets();
            }
            catch(Exception ex)
            {
                _dayView.ViewModel.HandleException(ex);
            }
        }
    }
}
