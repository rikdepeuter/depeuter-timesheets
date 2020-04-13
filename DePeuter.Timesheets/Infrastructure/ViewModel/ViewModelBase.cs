using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;

namespace DePeuter.Timesheets.Infrastructure.ViewModel
{
    public abstract class ViewModelBase : Shared.Wpf.Controls.Infrastructure.ViewModelBase
    {
        protected ViewModelBase()
            : base()
        {
        }
        protected ViewModelBase(ViewModelBase parent)
            : base(parent)
        {
        }
        
        public override void HandleException(Exception ex)
        {
            if (Parent != null)
            {
                Parent.HandleException(ex);
            }
            else
            {
                Logger.Error(ex.Message, ex);
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}