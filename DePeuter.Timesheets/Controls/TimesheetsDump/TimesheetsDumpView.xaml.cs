using System;
using System.Collections;
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

namespace DePeuter.Timesheets.Controls.TimesheetsDump
{
    /// <summary>
    /// Interaction logic for TimesheetsDumpView.xaml
    /// </summary>
    public partial class TimesheetsDumpView : UserControl
    {
        public TimesheetsDumpView()
        {
            InitializeComponent();
        }

        private void Control_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var grid = sender as DataGrid;
            if(grid == null)
            {
                return;
            }

            var item = grid.CurrentCell.Item as TimesheetsDumpViewModel.TimesheetWeekOverview;
            if(item == null)
            {
                return;
            }

            string clipboard = null;
            var tb = grid.CurrentCell.Column.GetCellContent(item) as TextBlock;
            if (tb != null)
            {
                clipboard = tb.Text;
            }
            else
            {
                var boundColumn = grid.CurrentCell.Column as DataGridBoundColumn;
                if(boundColumn == null)
                {
                    return;
                }

                var binding = boundColumn.Binding as Binding;
                if(binding == null)
                {
                    return;
                }

                var propertyName = binding.Path.Path;
                var parts = propertyName.TrimEnd(']').Split('[');

                propertyName = parts[0];

                var pi = item.GetType().GetProperty(propertyName);
                if(pi != null)
                {
                    var value = pi.GetValue(item, null);
                    if(value != null)
                    {
                        if(parts.Length > 1)
                        {
                            var index = int.Parse(parts[1]);
                            value = ((IEnumerable)value).Cast<object>().Skip(index).Take(1).Single();
                        }

                        clipboard = value.ToString();
                    }
                }
            }

            if (clipboard != null)
            {
                Clipboard.SetText(clipboard);
            }
        }
    }
}
