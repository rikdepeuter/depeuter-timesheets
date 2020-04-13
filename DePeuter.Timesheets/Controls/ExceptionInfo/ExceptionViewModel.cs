using System;
using System.Windows.Input;
using DePeuter.Timesheets.Infrastructure.ViewModel;

namespace DePeuter.Timesheets.Controls.ExceptionInfo
{
    public class ExceptionViewModel : ControlViewModelBase
    {
        public event EventHandler Closed;

        public string Message { get; set; }
        public string Summary { get; set; }

        public ExceptionViewModel(ViewModelBase parent, System.Exception exception)
            : base(parent)
        {
            Message = exception.Message;
            Summary = exception.ToString();
        }

        public ICommand CloseCommand { get { return NewCommand(CloseCommand_Execute); } }
        private void CloseCommand_Execute(object obj)
        {
            if (Closed != null)
            {
                Closed(this, EventArgs.Empty);
            }
        }
    }
}
