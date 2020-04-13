using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DePeuter.Timesheets.Infrastructure.ViewModel
{
    public abstract class ControlViewModelBase : ViewModelBase
    {
        protected ControlViewModelBase(ViewModelBase parent)
            : base(parent)
        {
        }
    }
}
