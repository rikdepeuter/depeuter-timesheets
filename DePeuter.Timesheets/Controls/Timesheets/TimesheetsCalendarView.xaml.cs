using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
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

namespace DePeuter.Timesheets.Controls.Timesheets
{
    /// <summary>
    /// Interaction logic for TimesheetsCalendarView.xaml
    /// </summary>
    public partial class TimesheetsCalendarView : UserControl
    {
        private TimesheetsCalendarViewModel ViewModel
        {
            get
            {
                return (TimesheetsCalendarViewModel)DataContext;
            }
        }

        public TimesheetsCalendarView()
        {
            InitializeComponent();

            DataContextChanged+=TimesheetsCalendarView_DataContextChanged;
        }

        void TimesheetsCalendarView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;

            SetWeekendVisible(ViewModel.WeekendIsVisible);
        }

        void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "WeekendIsVisible")
            {
                SetWeekendVisible(ViewModel.WeekendIsVisible);
            }
        }

        private void SetWeekendVisible(bool visible)
        {
            cdFridaySaturday.Width = visible ? new GridLength(1) : new GridLength(0);
            cdSaturday.Width = visible ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
            cdSaturdaySunday.Width = visible ? new GridLength(1) : new GridLength(0);
            cdSunday.Width = visible ? new GridLength(1, GridUnitType.Star) : new GridLength(0);

            foreach(var weekOverview in gSingleWeek.Children.OfType<TimesheetsOverviewWeekView>())
            {
                weekOverview.SetWeekendVisible(visible);
            }
            foreach(var weekOverview in gWeeks.Children.OfType<TimesheetsOverviewWeekView>())
            {
                weekOverview.SetWeekendVisible(visible);
            }
        }

        private void TimesheetsCalendarView_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if(!e.Handled)
            {
                e.Handled = true;
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = MouseWheelEvent;
                eventArg.Source = sender;
                var parent = (UIElement)((dynamic)sender).Parent.Parent;
                parent.RaiseEvent(eventArg);
            }
        }
    }
}
