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

namespace DePeuter.Timesheets.Controls.JobPicker
{
    /// <summary>
    /// Interaction logic for JobPickerView.xaml
    /// </summary>
    public partial class JobPickerView : UserControl
    {
        public JobPickerView()
        {
            InitializeComponent();
            Focusable = true;
        }

        private void JobPickerView_GotFocus(object sender, RoutedEventArgs e)
        {
            tbFilter.Focus();
            e.Handled = true;
        }

        private void tbFilter_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            var vm = (JobPickerViewModel)DataContext;
            if((e.Key == Key.Enter || e.Key == Key.Tab) && vm.Items != null && vm.Items.Any())
            {
                vm.SelectItemCommand.Execute(vm.Items.First());

                var tRequest = new TraversalRequest(FocusNavigationDirection.Next);
                var keyboardFocus = Keyboard.FocusedElement as UIElement;

                if(keyboardFocus != null)
                {
                    keyboardFocus.MoveFocus(tRequest);
                }

                e.Handled = true;
            }
        }
    }
}
