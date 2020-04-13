using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DePeuter.Shared.Wpf.Helpers;
using DePeuter.Timesheets.Database.DTO;

namespace DePeuter.Timesheets.Controls.Timesheets
{
    /// <summary>
    /// Interaction logic for OverviewDayView.xaml
    /// </summary>
    public partial class TimesheetsOverviewDayView : UserControl
    {
        private TimesheetsOverviewDayViewModel ViewModel { get { return (TimesheetsOverviewDayViewModel) DataContext; } }

        public TimesheetsOverviewDayView()
        {
            InitializeComponent();
        }

        private void TimesheetsOverviewItemView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var row = UIHelpers.TryFindFromPoint<TimesheetsOverviewItemView>((UIElement)sender, e.GetPosition(icTimesheets));
            if(row == null)
            {
                return;
            }

            var dragData = new DataObject(typeof(TimesheetSearchResult), (TimesheetSearchResult)row.DataContext);
            DragDrop.DoDragDrop((UIElement)sender, dragData, DragDropEffects.Move);
        }

        private TimesheetSearchResult GetDraggedItem(DragEventArgs e)
        {
            var dataObj = e.Data as DataObject;
            if(dataObj == null)
            {
                return null;
            }

            return dataObj.GetData(typeof(TimesheetSearchResult)) as TimesheetSearchResult;
        }

        protected override void OnDrop(DragEventArgs e)
        {
            var item = GetDraggedItem(e);
            if(item != null)
            {
                var viewModel = ViewModel;
                try
                {
                    var previousDate = item.StartTime.Date;
                    var newDate = viewModel.Date;
                    if(previousDate == newDate)
                    {
                        return;
                    }

                    var timesheetsViewModel = viewModel.FindParent<TimesheetsViewModel>();
                    timesheetsViewModel.UpdateTimesheetTime(item.Id, newDate.AddHours(item.StartTime.Hour).AddMinutes(item.StartTime.Minute), newDate.AddHours(item.EndTime.Hour).AddMinutes(item.EndTime.Minute));

                    if(timesheetsViewModel.TimesheetDayViewModel.Date == newDate || timesheetsViewModel.TimesheetDayViewModel.Date == previousDate)
                    {
                        timesheetsViewModel.TimesheetDayViewModel.RefreshData();
                    }
                }
                catch(Exception ex)
                {
                    viewModel.HandleException(ex);
                }
            }
        }

        private readonly Brush _overlayBrush = new SolidColorBrush(Color.FromRgb(238, 243, 249));

        private void TimesheetsOverviewDayView_OnDragEnter(object sender, DragEventArgs e)
        {
            var item = GetDraggedItem(e);
            if(item != null)
            {
                var viewModel = ViewModel;
                if (item.StartTime.Date != viewModel.Date)
                {
                    Background = _overlayBrush;
                }
            }
        }

        private void TimesheetsOverviewDayView_OnDragLeave(object sender, DragEventArgs e)
        {
            Background = Brushes.Transparent;
        }

        private void TimesheetsOverviewDayView_OnMouseEnter(object sender, MouseEventArgs e)
        {
            Background = _overlayBrush;
        }

        private void TimesheetsOverviewDayView_OnMouseLeave(object sender, MouseEventArgs e)
        {
            Background = Brushes.Transparent;
        }
    }
}