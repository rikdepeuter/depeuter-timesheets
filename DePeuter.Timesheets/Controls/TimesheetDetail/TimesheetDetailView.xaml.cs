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
using DePeuter.Timesheets.Database.DTO;
using DePeuter.Timesheets.Database.Entities;
using DePeuter.Timesheets.Infrastructure.Controls;

namespace DePeuter.Timesheets.Controls.TimesheetDetail
{
    /// <summary>
    /// Interaction logic for TimesheetDetailView.xaml
    /// </summary>
    public partial class TimesheetDetailView : UserControlBase
    {
        private TimesheetDetailViewModel ViewModel { get { return (TimesheetDetailViewModel)DataContext; } }

        public TimesheetDetailView()
        {
            InitializeComponent();

            HandleDataContextChangedAsLoaded = true;

            DataContextChanged += TimesheetDetailView_DataContextChanged;
        }

        private void TimesheetDetailView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext != null)
            {
                tbTaskNumber.Focus();
            }
        }

        private void Toolbar_Loaded(object sender, RoutedEventArgs e)
        {
            ((ToolBar) sender).HideOverflowToggleButton();
        }

        private void lbRecentItems_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var lb = (ListBox) sender;
            var timesheet = lb.SelectedItem as TimesheetDetailViewModel.TimesheetQuickSearchItem;
            if (timesheet != null)
            {
                ViewModel.TaskNumber = timesheet.TaskNumber;
                ViewModel.Description = timesheet.Description;
            }
        }
    }
}
