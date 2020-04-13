using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DePeuter.Timesheets.Controls.Timesheets
{
    /// <summary>
    /// Interaction logic for TimesheetsOverviewWeekView.xaml
    /// </summary>
    public partial class TimesheetsOverviewWeekView : UserControl
    {
        public TimesheetsOverviewWeekView()
        {
            InitializeComponent();

            DataContextChanged += TimesheetsOverviewWeekView_DataContextChanged;
        }

        void TimesheetsOverviewWeekView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            var viewModel = (TimesheetsOverviewWeekViewModel)DataContext;
            SetWeekendVisible(((TimesheetsCalendarViewModel)viewModel.Parent).WeekendIsVisible);    
        }

        public void SetWeekendVisible(bool visible)
        {
            cdFridaySaturday.Width = visible ? new GridLength(1) : new GridLength(0);
            cdSaturday.Width = visible ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
            cdSaturdaySunday.Width = visible ? new GridLength(1) : new GridLength(0);
            cdSunday.Width = visible ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
        }
    }
}
