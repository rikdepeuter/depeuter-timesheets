using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DePeuter.Timesheets.Constants;
using DePeuter.Timesheets.Controls.Timesheets;
using DePeuter.Timesheets.Database.Contexts;
using DePeuter.Timesheets.Database.DTO;
using DePeuter.Timesheets.Database.Entities;
using DePeuter.Timesheets.Infrastructure;
using DePeuter.Timesheets.Infrastructure.ViewModel;
using DePeuter.Timesheets.Utils;

namespace DePeuter.Timesheets.Controls.TimesheetDetail
{
    public class TimesheetDetailViewModel : ControlViewModelBase, IShortcutKey
    {
        public event EventHandler RequestClose;
        public event EventHandler Deleted;

        public Timesheet Entity
        {
            get { return _entity; }
            private set
            {
                _entity = value;
                OnPropertyChanged();
            }
        }

        public string TaskNumber
        {
            get { return _taskNumber; }
            set
            {
                _taskNumber = value;
                OnPropertyChanged();
                SearchTimesheets(filterTaskNumber: true);
            }
        }

        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                OnPropertyChanged();
                SearchTimesheets(filterDescription: true);
            }
        }

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        public List<string> TaskNumbers
        {
            get { return _taskNumbers; }
            set
            {
                _taskNumbers = value;
                OnPropertyChanged();
            }
        }

        //public JobPickerViewModel JobPickerViewModel
        //{
        //    get { return _jobPickerViewModel; }
        //    set
        //    {
        //        _jobPickerViewModel = value;
        //        OnPropertyChanged();
        //    }
        //}

        public bool IsBreak
        {
            get { return SelectedType == (int)TimesheetType.Break; }
            set
            {
                SelectedType = (int)(value ? TimesheetType.Break : TimesheetType.Normal);
                OnPropertyChanged();

                if(SelectedType == (int)TimesheetType.Break)
                {
                    //JobPickerViewModel.SelectedId = null;
                    Description = null;
                }
            }
        }

        public int SelectedType
        {
            get { return _selectedType; }
            set
            {
                _selectedType = value;
                OnPropertyChanged();
                OnPropertyChanged("IsBreak");
            }
        }

        public List<MetaDataItem> Types
        {
            get { return _types; }
            set
            {
                _types = value;
                OnPropertyChanged();
            }
        }

        public List<TimesheetQuickSearchItem> TimesheetsFiltered
        {
            get { return _timesheetsFiltered; }
            set
            {
                _timesheetsFiltered = value;
                OnPropertyChanged();
            }
        }

        //public string FilterTimesheets
        //{
        //    get { return _filterTimesheets; }
        //    set
        //    {
        //        _filterTimesheets = value;
        //        OnPropertyChanged();

        //        SearchTimesheets();
        //    }
        //}

        private readonly int _id;
        private Timesheet _entity;
        private string _description;
        private string _errorMessage;
        //private JobPickerViewModel _jobPickerViewModel;
        private List<MetaDataItem> _types = new List<MetaDataItem>()
        {
            new MetaDataItem((int)TimesheetType.Normal, TimesheetType.Normal.ToString()),
            //new MetaDataItem((int)TimesheetType.Active, TimesheetType.Active.ToString()),
            new MetaDataItem((int)TimesheetType.Break, TimesheetType.Break.ToString()),
        };

        private int _selectedType;
        private string _taskNumber;
        private List<string> _taskNumbers;
        private List<TimesheetQuickSearchItem> _timesheetsFiltered;
        //private string _filterTimesheets;

        private readonly List<TimesheetSearchResult> _data;

        public TimesheetDetailViewModel(ViewModelBase parent, int id, Timesheet entity, List<TimesheetSearchResult> data)
            : base(parent)
        {
            _id = id;
            Entity = entity;
            _data = data;
            //JobPickerViewModel = new JobPickerViewModel(this);
        }

        protected override void Loaded(object sender, RoutedEventArgs e)
        {
            if(Entity == null)
            {
                Entity = TimesheetsContext.My.GetTimesheet(_id);
            }

            SelectedType = Entity.Type;
            //JobPickerViewModel.SelectedId = Entity.JobId;
            TaskNumber = Entity.TaskNumber;
            Description = Entity.Description;

            TaskNumbers = TimesheetsRegistry.GetDefaultTaskNumbers();

            if(Entity.IsNew)
            {
                SearchTimesheets(true, true);
            }
        }

        private void Close()
        {
            RequestClose(this, EventArgs.Empty);
        }

        public ICommand SaveCommand { get { return NewCommand(SaveCommand_Execute); } }
        private void SaveCommand_Execute(object obj)
        {
            ErrorMessage = null;

            try
            {
                if(((TimesheetType)SelectedType) != TimesheetType.Break)
                {
                    if(string.IsNullOrEmpty(Description))
                    {
                        throw new ValidationException("Please enter a description.");
                    }
                }

                Entity.Username = Session.Username;

                Entity.Type = SelectedType;
                //Entity.JobId = JobPickerViewModel.SelectedId;
                Entity.TaskNumber = (TaskNumber ?? string.Empty).Trim().EmptyAsNull();
                Entity.Description = (Description ?? string.Empty).Trim().EmptyAsNull();

                TimesheetsContext.My.Save(Entity);

                Close();
            }
            catch(Exception ex)
            {
                Logger.Error(ex.Message, ex);
                ErrorMessage = ex.Message;
            }
        }

        public ICommand CancelCommand { get { return NewCommand(CancelCommand_Execute); } }
        private void CancelCommand_Execute(object obj)
        {
            //TODO check if there are any changes

            Close();
        }

        public ICommand DeleteCommand { get { return NewCommand(DeleteCommand_Execute); } }
        private void DeleteCommand_Execute(object obj)
        {
            var parent = FindParent<TimesheetsViewModel>();
            if(parent.DeleteTimesheet(Entity))
            {
                Close();
            }
        }

        public ICommand UseDescriptionCommand { get { return NewCommand(UseDescriptionCommand_Execute); } }
        private void UseDescriptionCommand_Execute(object obj)
        {
            var timesheet = (TimesheetQuickSearchItem)obj;
            Description = timesheet.Description;
        }

        //public ICommand AddJobCommand { get { return NewCommand(AddJobCommand_Execute); } }
        //private void AddJobCommand_Execute(object obj)
        //{
        //    //var data = InputBox.GetValue("Job code, client, project:", characterCasing: CharacterCasing.Upper);
        //    var data = InputBox.GetValue("Job code, project:", characterCasing: CharacterCasing.Upper);
        //    if(string.IsNullOrEmpty(data))
        //    {
        //        return;
        //    }

        //    var parts = data.Split(',').Select(x => x.Trim()).ToArray();

        //    using (var db = new TimesheetsContext())
        //    {
        //        var jobCode = parts[0];
        //        //var client = parts.Skip(1).FirstOrDefault();
        //        //var project = parts.Skip(2).FirstOrDefault();
        //        var project = parts.Skip(1).FirstOrDefault();

        //        if (db.JobExists(jobCode))
        //        {
        //            throw new ValidationException("Job code already in use.");
        //        }

        //        var job = new Job()
        //        {
        //            Code = jobCode,
        //            //Client = client,
        //            Project = project
        //        };
        //        db.Insert(job);

        //        Session.Clear();

        //        //JobPickerViewModel.SelectedId = job.Id;
        //    }
        //}

        private void SearchTimesheets(bool filterTaskNumber = false, bool filterDescription = false)
        {
            LoadAsync(() =>
            {
                var q = _data.AsQueryable();

                q = q.Where(x => x.Type == (int)TimesheetType.Normal);

                if(filterTaskNumber && !string.IsNullOrWhiteSpace(TaskNumber))
                {
                    var filter = TaskNumber.Trim().ToLower();
                    q = q.Where(x => (x.TaskNumber ?? string.Empty).ToLower().Contains(filter));
                }

                if(filterDescription && !string.IsNullOrWhiteSpace(Description))
                {
                    var filter = Description.Trim().ToLower();
                    q = q.Where(x => (x.Description ?? string.Empty).ToLower().Contains(filter));
                }

                //if (!string.IsNullOrWhiteSpace(FilterTimesheets))
                //{
                //    var filter = FilterTimesheets.Trim().ToLower();
                //    q = q.Where(x => (x.TaskNumber ?? string.Empty).Contains(filter) || (x.Description ?? string.Empty).Contains(filter));
                //}

                q = q.OrderByDescending(x => x.StartTime);

                var result = new Dictionary<string, TimesheetQuickSearchItem>();
                var maxRecords = 50;

                foreach(var x in q)
                {
                    if(result.Count == maxRecords)
                    {
                        break;
                    }

                    var key = string.Format("${0}|{1}$", x.TaskNumber, x.Description).Trim().ToLower();
                    if(result.ContainsKey(key))
                    {
                        continue;
                    }

                    result.Add(key, new TimesheetQuickSearchItem() { StartTime = x.StartTime, TaskNumber = x.TaskNumber, Description = x.Description });
                }

                return result.Values.ToList();

                //using(var context = new TimesheetsContext())
                //{
                //    var timesheets = context.SearchTimesheets(Session.Username, take: 10, sortTimeAsc: false, filter: FilterTimesheets, types: new[] { TimesheetType.Normal });
                //    return timesheets;
                //}
            }, result =>
            {
                if(TimesheetsFiltered == null || TimesheetsFiltered.Count > 0 || result.Count > 0)
                {
                    TimesheetsFiltered = result;
                }
            });
        }

        public void ProcessShortcutKey(ProcessShortcutKeyEventArgs e)
        {
            switch(e.Key)
            {
                case ShortcutKey.Save:
                case ShortcutKey.SaveAndClose:
                    SaveCommand.Execute(null);
                    e.Handled = true;
                    break;
                case ShortcutKey.Close:
                    //case ShortcutKey.Exit:
                    CancelCommand.Execute(null);
                    e.Handled = true;
                    break;
                //case ShortcutKey.Delete:
                //    DeleteCommand.Execute(null);
                //    e.Handled = true;
                //    break;
            }
        }

        public class TimesheetQuickSearchItem
        {
            public DateTime StartTime { get; set; }
            public string TaskNumber { get; set; }
            public string Description { get; set; }

            public string OverviewDescription
            {
                get
                {
                    var sb = new StringBuilder();
                    sb.AppendFormat("{0:dd/MM ddd}", StartTime);
                    if(!string.IsNullOrEmpty(TaskNumber))
                    {
                        sb.AppendFormat(" [{0}]", TaskNumber);
                    }
                    if(!string.IsNullOrEmpty(Description))
                    {
                        sb.AppendFormat(" {0}", Description);
                    }
                    return sb.ToString();
                }
            }
        }
    }
}