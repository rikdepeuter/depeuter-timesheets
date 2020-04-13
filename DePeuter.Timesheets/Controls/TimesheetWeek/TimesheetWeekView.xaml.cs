using System;
using System.Collections.Generic;
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
using DePeuter.Timesheets.Infrastructure.Controls;

namespace DePeuter.Timesheets.Controls.TimesheetWeek
{
    /// <summary>
    /// Interaction logic for TimesheetWeekView.xaml
    /// </summary>
    public partial class TimesheetWeekView : UserControlBase
    {
        public event EventHandler ScrolledUp;
        public event EventHandler ScrolledDown;

        public TimesheetWeekView()
        {
            InitializeComponent();
        }

        private void TimesheetWeekView_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            //if(e.Delta != 0)
            //{
            //    if(e.Delta < 0)
            //    {
            //        if(ScrolledUp != null)
            //        {
            //            ScrolledUp(this, EventArgs.Empty);
            //        }
            //    }
            //    else
            //    {
            //        if(ScrolledDown != null)
            //        {
            //            ScrolledDown(this, EventArgs.Empty);
            //        }
            //    }
            //}
        }

        private void svMain_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if(e.VerticalChange != 0)
            {
                if(e.VerticalChange < 0)
                {
                    if(ScrolledUp != null)
                    {
                        ScrolledUp(this, EventArgs.Empty);
                    }
                }
                else
                {
                    if(ScrolledDown != null)
                    {
                        ScrolledDown(this, EventArgs.Empty);
                    }
                }
            }
        }

        private void TimesheetWeekView_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= TimesheetWeekView_Loaded;

            var StartHour = 7;
            var StartMinute = 0;
            var IntervalMinute = 5;
            var ItemsHeight = 15.0;

            foreach(var timesheetDayView in new[] { day1, day2, day3, day4, day5 })
            {
                timesheetDayView.Initialize(StartHour, StartMinute, IntervalMinute, ItemsHeight);
            }

            var partsPerHour = 60.0/IntervalMinute;
            var topOffset = StartHour * partsPerHour * ItemsHeight;
            svMain.ScrollToVerticalOffset(topOffset);

            svMain.ScrollChanged += svMain_ScrollChanged;
        }
    }
}
