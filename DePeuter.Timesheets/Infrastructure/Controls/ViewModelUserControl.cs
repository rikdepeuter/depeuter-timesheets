using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using DePeuter.Timesheets.Infrastructure.ViewModel;
using DePeuter.Timesheets.Utils;

namespace DePeuter.Timesheets.Infrastructure.Controls
{
    public sealed class ViewModelUserControl : UserControl
    {
        public ViewModelUserControl()
        {
            DataContextChanged += ViewModelUserControl_DataContextChanged;
            //Loaded += ViewModelUserControl_Loaded;
        }

        void ViewModelUserControl_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            var vm = DataContext as ViewModelBase;
            if(vm != null)
            {
                var viewType = ViewModelUtils.GetViewType(vm.GetType());
                var view = (UserControl)Activator.CreateInstance(viewType);
                view.DataContext = vm;

                if (!(view is UserControlBase))
                {
                    view.Loaded += view_Loaded;
                }
                
                Content = view;
            }
        }

        private void HandleException(Exception ex)
        {
            ((ViewModelBase)DataContext).HandleException(ex);
        }

        void view_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var vm = DataContext as ViewModelBase;
            if(vm != null)
            {
                var view = (UserControl)sender;
                view.Loaded -= view_Loaded;

                try
                {
                    vm.RaiseLoaded(sender, e);
                }
                catch(Exception ex)
                {
                    HandleException(ex);
                }
            }
        }
    }
}