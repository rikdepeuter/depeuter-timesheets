using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DePeuter.Shared.Wpf.DependencyProperties
{
    public static class Commands
    {
        public static readonly DependencyProperty DataGridDoubleClickProperty = DependencyProperty.RegisterAttached("DataGridDoubleClickCommand", typeof(ICommand), typeof(Commands), new PropertyMetadata(new PropertyChangedCallback(AttachOrRemoveDataGridDoubleClickEvent)));

        public static ICommand GetDataGridDoubleClickCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(DataGridDoubleClickProperty);
        }

        public static void SetDataGridDoubleClickCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(DataGridDoubleClickProperty, value);
        }

        public static void AttachOrRemoveDataGridDoubleClickEvent(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var dataGrid = obj as DataGrid;
            if(dataGrid != null)
            {
                //var cmd = (ICommand)args.NewValue;

                if(args.OldValue == null && args.NewValue != null)
                {
                    dataGrid.MouseDoubleClick += ExecuteDataGridDoubleClick;
                }
                else if(args.OldValue != null && args.NewValue == null)
                {
                    dataGrid.MouseDoubleClick -= ExecuteDataGridDoubleClick;
                }
            }
        }

        private static void ExecuteDataGridDoubleClick(object sender, MouseButtonEventArgs args)
        {
            var obj = (DependencyObject)sender;
            var cmd = (ICommand)obj.GetValue(DataGridDoubleClickProperty);
            if(cmd != null)
            {
                if(cmd.CanExecute(obj))
                {
                    cmd.Execute(obj);
                }
            }
        }
    }
}