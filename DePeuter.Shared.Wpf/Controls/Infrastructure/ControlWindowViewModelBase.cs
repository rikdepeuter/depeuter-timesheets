using System;
using System.Windows;
using System.Windows.Input;

namespace DePeuter.Shared.Wpf.Controls.Infrastructure
{
    public abstract class ControlWindowViewModelBase : ViewModelBase
    {
        protected ControlWindowViewModelBase()
            : base()
        {
        }

        protected ControlWindowViewModelBase(ViewModelBase parent)
        {
            Parent = parent;
        }
    }
}
