using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DePeuter.Shared.Database;

namespace DePeuter.Timesheets.Infrastructure.ViewModel
{
    public class ItemSelectedEventArgs<T>
        where T : class, IHasId
    {
        public T SelectedItem { get; private set; }

        public ItemSelectedEventArgs(T item)
        {
            SelectedItem = item;
        }
    }
    public abstract class PickerControlViewModelBase<T> : ControlViewModelBase
        where T : class, IHasId
    {
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set
            {
                _isReadOnly = value;
                OnPropertyChanged();
            }
        }

        public bool SupportAdd
        {
            get { return _supportAdd; }
            set
            {
                _supportAdd = value;
                OnPropertyChanged();
            }
        }

        public event EventHandler<ItemSelectedEventArgs<T>> ItemSelected;

        public bool HasValue { get { return SelectedItem != null; } }

        private int? _overrideId;
        public int? OverrideId
        {
            get { return _overrideId; }
            set
            {
                _overrideId = value;
                OnPropertyChanged();

                IsReadOnly = _overrideId != null;
                if(_overrideId != null)
                {
                    SelectedId = _overrideId;
                }
            }
        }

        public int? SelectedId
        {
            get
            {
                return SelectedItem != null ? SelectedItem.Id : (int?)null;
            }
            set
            {
                SelectedItem = value != null && value != 0 ? GetItemById(value.Value) : null;
            }
        }

        private T _selectedItem;
        private bool _isReadOnly;
        private bool _supportAdd;

        public T SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (_selectedItem == null && _selectedItem == value)
                {
                    return;
                }

                _selectedItem = value;
                OnPropertyChanged();

                OnItemSelected();

                if(ItemSelected != null)
                {
                    ItemSelected(this, new ItemSelectedEventArgs<T>(value));
                }
            }
        }

        protected PickerControlViewModelBase(ViewModelBase parent)
            : base(parent)
        {
        }

        protected virtual void OnItemSelected()
        {
        }

        protected abstract T GetItemById(int id);

        protected abstract string DisplayEntity(T entity);

        public string EntityDescription
        {
            get
            {
                var type = typeof(T);
                var attr = type.GetCustomAttribute<DescriptionAttribute>(true);
                if(attr != null)
                {
                    return attr.Description;
                }

                return type.Name;
            }
        }
    }
}
