using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using DePeuter.Timesheets.Database.Entities;
using DePeuter.Timesheets.Database.Services;
using DePeuter.Timesheets.Infrastructure;
using DePeuter.Timesheets.Infrastructure.ViewModel;

namespace DePeuter.Timesheets.Controls.JobPicker
{
    public class JobPickerViewModel : PickerControlViewModelBase<Job>
    {
        public event EventHandler AddingItem;

        public ObservableCollection<Job> Items
        {
            get { return _items; }
            set
            {
                _items = value; 
                OnPropertyChanged();
            }
        }

        public string Filter
        {
            get { return _filter; }
            set
            {
                _filter = value;
                OnPropertyChanged();

                //if(string.IsNullOrEmpty(value))
                //{
                //    Items = null;
                //}
                //else
                //{
                //    var items = Session.Jobs.Where(x => x.Id == (SelectedId ?? 0)).Concat(Session.Jobs.Where(x => string.IsNullOrEmpty(Filter) || x.Code.ToLower().Contains(Filter.ToLower()) || (x.Project ?? string.Empty).ToLower().Contains(Filter.ToLower()) || (x.Client ?? string.Empty).ToLower().Contains(Filter.ToLower()))).OrderByDescending(x => x.Code).ToList();
                //    if(SupportAdd)
                //    {
                //        items.Add(new Job() { Code = string.Format("<New {0}>", EntityDescription) });
                //    }

                //    Items = items.ToObservableCollection();
                //}
                //SelectedItem = null;
            }
        }

        public ICommand SelectItemCommand { get { return NewCommand(SelectItemCommand_Execute); } }
        private void SelectItemCommand_Execute(object obj)
        {
            var item = (Job)obj;
            if(item != null && item.IsNew)
            {
                if(AddingItem != null)
                {
                    AddingItem(this, EventArgs.Empty);
                }
                //else
                //{
                //    var vm = new CategoryDetailViewModel(this, 0);
                //    vm.Entity.Name = Filter;
                //    vm.Saved += CategoryDetailViewModel_Saved;
                //    vm.Show();
                //}

                Filter = null;
                return;
            }

            SelectedItem = item;
        }

        //void CategoryDetailViewModel_Saved(object sender, EventArgs e)
        //{
        //    SelectedId = ((CategoryDetailViewModel)sender).Entity.Id;
        //}

        private string _filter;
        private ObservableCollection<Job> _items;

        public JobPickerViewModel(ViewModelBase parent)
            : base(parent)
        {
        }

        protected override Job GetItemById(int id)
        {
            using(var context = new TimesheetsDb())
            {
                return context.GetEntityById<Job>(id);
            }
        }

        protected override void OnItemSelected()
        {
            if (SelectedItem != null)
            {
                _filter = SelectedItem.Code;
                OnPropertyChanged("Filter");

                Items = null;
            }
            else
            {
                Filter = null;
            }
        }

        protected override string DisplayEntity(Job entity)
        {
            //if (entity.IsNew)
            //{
            //    return string.Format("<New {0}>", EntityDescription);
            //}
            return entity.Code;
        }
    }
}
