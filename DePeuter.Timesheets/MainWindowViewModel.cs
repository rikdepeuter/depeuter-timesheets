using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using DePeuter.Timesheets.Constants;
using DePeuter.Timesheets.Controls.ExceptionInfo;
using DePeuter.Timesheets.Controls.Timesheets;
using DePeuter.Timesheets.Infrastructure.ViewModel;
using DePeuter.Timesheets.Utils;

namespace DePeuter.Timesheets
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<ITabViewModel> Tabs { get; private set; }

        public ITabViewModel SelectedTab
        {
            get { return _selectedTab; }
            private set
            {
                _selectedTab = value;
                OnPropertyChanged();
            }
        }

        public int SelectedTabIndex
        {
            get { return _selectedTabIndex; }
            set
            {
                if(value >= Tabs.Count || _selectedTabIndex == value || value < 0)
                {
                    return;
                }

                _selectedTabIndex = value;
                OnPropertyChanged();

                var selectedTab = Tabs[value];
                _selectedTabsHistory.Remove(selectedTab);
                _selectedTabsHistory.Add(selectedTab);
                SelectedTab = selectedTab;
            }
        }

        public ExceptionViewModel ExceptionViewModel
        {
            get { return _exceptionViewModel; }
            set
            {
                _exceptionViewModel = value; 
                OnPropertyChanged();
            }
        }

        private readonly List<ITabViewModel> _selectedTabsHistory = new List<ITabViewModel>();
        private int _selectedTabIndex = -1;
        private ITabViewModel _selectedTab;
        private ExceptionViewModel _exceptionViewModel;

        public MainWindowViewModel()
        {
            Tabs = new ObservableCollection<ITabViewModel>();
        }

        protected override void Loaded(object sender, RoutedEventArgs e)
        {
            OpenTab(typeof(TimesheetsViewModel));
        }

        private void OpenTab(Type viewModelType)
        {
            var tab = FindTab(viewModelType);
            if(tab != null)
            {
                SelectTab(tab);
                return;
            }

            var vm = (ITabViewModel)Activator.CreateInstance(viewModelType, new object[] { this });
            AddTab(vm);
        }
        private void AddTab(ITabViewModel vm)
        {
            Tabs.Add(vm);
            SelectedTabIndex = Tabs.Count - 1;
        }
        //private void RemoveTab(ITabViewModel vm)
        //{
        //    _selectedTabsHistory.Remove(vm);
        //    SelectedTabIndex = Tabs.IndexOf(_selectedTabsHistory.Last());
        //}
        private void SelectTab(ITabViewModel vm)
        {
            SelectedTabIndex = Tabs.IndexOf(vm);
        }
        private ITabViewModel FindTab(Type type)
        {
            return Tabs.FirstOrDefault(x => x.GetType() == type);
        }

        public ICommand OpenViewModelCommand { get { return NewCommand(OpenViewModelCommand_Execute); } }
        private void OpenViewModelCommand_Execute(object obj)
        {
            var viewModelName = (string)obj;

            var vmType = ViewModelUtils.GetViewModelType(viewModelName);
            OpenTab(vmType);
        }

        public override void HandleException(Exception ex)
        {
            Logger.Error(ex.Message, ex);
            ExceptionViewModel = new ExceptionViewModel(this, ex);
            ExceptionViewModel.Closed += ExceptionViewModel_Closed;
        }

        void ExceptionViewModel_Closed(object sender, EventArgs e)
        {
            ExceptionViewModel = null;
        }

        public void HandleShortcutKey(ProcessShortcutKeyEventArgs e)
        {
            var tab = Tabs[SelectedTabIndex];

            var handler = tab as IShortcutKey;
            if (handler != null)
            {
                handler.ProcessShortcutKey(e);
            }
        }
    }
}