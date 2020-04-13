using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DePeuter.Timesheets.Infrastructure.ViewModel;

namespace DePeuter.Timesheets.Infrastructure.Entities
{
    public class IsCheckedItem : ViewModelBase
    {
        private bool _isChecked;
        public string Display { get; set; }
        public object Tag { get; set; }

        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                var valueChanged = value != _isChecked;
                _isChecked = value;
                OnPropertyChanged();
                if (valueChanged && IsCheckedChanged != null)
                {
                    IsCheckedChanged(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler IsCheckedChanged;

        public IsCheckedItem(string display, bool isChecked = false, object tag = null)
        {
            Display = display;
            IsChecked = isChecked;
            Tag = tag;
        }
    }
}
