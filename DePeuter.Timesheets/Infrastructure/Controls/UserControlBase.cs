using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using DePeuter.Timesheets.Infrastructure.ViewModel;

namespace DePeuter.Timesheets.Infrastructure.Controls
{
    public abstract class UserControlBase : UserControl
    {
        public bool HandleDataContextChangedAsLoaded { get; set; }

        protected UserControlBase()
        {
            DataContextChanged += ViewModelUserControl_DataContextChanged;
            Loaded += view_Loaded;
        }

        private void ViewModelUserControl_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            var vm = DataContext as ViewModelBase;
            if(vm != null && HandleDataContextChangedAsLoaded)
            {
                try
                {
                    vm.RaiseLoaded(sender, null);
                }
                catch(Exception ex)
                {
                    HandleException(ex);
                }
            }
            //var view = Content as UserControl;
            //if(view != null)
            //{
            //    view.Loaded += view_Loaded;
            //}
        }

        protected void HandleException(Exception ex)
        {
            ((ViewModelBase)DataContext).HandleException(ex);
        }

        private void view_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var vm = DataContext as ViewModelBase;
            if (vm != null)
            {
                //var view = (UserControl) sender;
                //view.Loaded -= view_Loaded;
                Loaded -= view_Loaded;

                try
                {
                    vm.RaiseLoaded(sender, e);
                }
                catch (Exception ex)
                {
                    HandleException(ex);
                }
            }
        }
    }
}