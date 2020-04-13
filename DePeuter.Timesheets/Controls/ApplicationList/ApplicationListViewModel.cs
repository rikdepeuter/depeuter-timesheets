using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using DePeuter.Timesheets.Utils;
using DePeuter.Timesheets.Infrastructure.Configuration;
using DePeuter.Timesheets.Infrastructure.Entities;
using DePeuter.Timesheets.Infrastructure.ViewModel;

namespace DePeuter.Timesheets.Controls.ApplicationList
{
    public class ApplicationListViewModel : ControlViewModelBase, ITabViewModel
    {
        public string HeaderText { get { return "Applications"; } }

        public string FilterText
        {
            get { return _filterText; }
            set
            {
                _filterText = value;
                Search();
            }
        }

        public ObservableCollection<ApplicationObject> ApplicationsFiltered
        {
            get
            {
                var filterText = (FilterText ?? string.Empty).ToLower();
                return _applications.Where(x => x.Name.ToLower().Contains(filterText)).ToObservableCollection();
            }
        }

        public ApplicationObject SelectedApplication
        {
            get { return _selectedApplication; }
            set
            {
                _selectedApplication = value;
                OnPropertyChanged("SelectedApplication");
            }
        }

        public ObservableCollection<IsCheckedItem> UpdateFileExtensions
        {
            get { return _updateFileExtensions; }
            set
            {
                _updateFileExtensions = value;
                OnPropertyChanged("UpdateFileExtensions");

                if (value != null)
                {
                    foreach (var item in value)
                    {
                        item.IsCheckedChanged += item_IsCheckedChanged;
                    }
                }

                CalculateAllUpdateFileExtensionsAreChecked();
            }
        }

        void item_IsCheckedChanged(object sender, EventArgs e)
        {
            CalculateAllUpdateFileExtensionsAreChecked();
        }

        public bool? AllUpdateFileExtensionsAreChecked
        {
            get { return _allUpdateFileExtensionsAreChecked; }
            set
            {
                var newValue = value ?? false;
                if (UpdateFileExtensions != null)
                {
                    foreach (var item in UpdateFileExtensions)
                    {
                        item.IsChecked = newValue;
                    }
                    OnPropertyChanged("UpdateFileExtensions");
                }

                _allUpdateFileExtensionsAreChecked = newValue;
                OnPropertyChanged("AllUpdateFileExtensionsAreChecked");
            }
        }

        private void CalculateAllUpdateFileExtensionsAreChecked()
        {
            if (UpdateFileExtensions == null)
            {
                return;
            }

            if (UpdateFileExtensions.All(x => x.IsChecked))
            {
                _allUpdateFileExtensionsAreChecked = true;
            }
            else if (UpdateFileExtensions.All(x => !x.IsChecked))
            {
                _allUpdateFileExtensionsAreChecked = false;
            }
            else
            {
                _allUpdateFileExtensionsAreChecked = null;
            }
            OnPropertyChanged("AllUpdateFileExtensionsAreChecked");
        }

        private List<ApplicationObject> _applications;
        private string _filterText;
        private ApplicationObject _selectedApplication;
        private ObservableCollection<IsCheckedItem> _updateFileExtensions;

        public ApplicationListViewModel(ViewModelBase parent)
            : base(parent)
        {
            _applications = new List<ApplicationObject>();
        }

        protected override void Loaded(object sender, RoutedEventArgs e)
        {
            var applicationsDirectory = Settings.My.ApplicationsDirectory;
            var applicationTypes = Settings.My.ApplicationTypes;
            var backupDirectory = Settings.My.BackupDirectory;

            foreach (var applicationType in applicationTypes)
            {
                var applicationTypeDirectory = new DirectoryInfo(Path.Combine(applicationsDirectory, applicationType));
                if (!applicationTypeDirectory.Exists)
                {
                    continue;
                }

                var applicationDirectories = applicationTypeDirectory.GetDirectories("*", SearchOption.TopDirectoryOnly);
                foreach (var applicationDirectory in applicationDirectories)
                {
                    var application = new ApplicationObject()
                    {
                        Type = applicationTypeDirectory.Name,
                        Name = applicationDirectory.Name,
                        LastModifiedAt = applicationDirectory.LastWriteTime
                    };

                    application.FullPath = applicationDirectory.FullName;

                    LoadBackupVersions(backupDirectory, application);

                    _applications.Add(application);
                }
            }

            _applications = _applications.OrderBy(x => x.Type).ThenBy(x => x.Name).ToList();
            Search();
        }

        private static void LoadBackupVersions(string backupDirectory, ApplicationObject application)
        {
            var applicationTypeBackupDirectory = new DirectoryInfo(Path.Combine(backupDirectory, application.Type));
            if (applicationTypeBackupDirectory.Exists)
            {
                var applicationBackupDirectories = applicationTypeBackupDirectory.GetDirectories(application.Name + ".*", SearchOption.TopDirectoryOnly);
                application.Backups = applicationBackupDirectories.Select(x => new ApplicationBackupObject(x.Name.Substring(application.Name.Length + 1), x.FullName)).OrderByDescending(x => x.Version).ToList();
            }
        }

        private void Search()
        {
            OnPropertyChanged("ApplicationsFiltered");
        }

        public ICommand ExplorerCommand { get { return NewCommand(ExplorerCommand_Execute); } }
        private void ExplorerCommand_Execute(object obj)
        {
            var application = obj as ApplicationObject;
            if (application != null)
            {
                Process.Start("explorer", string.Format("\"{0}\"", application.FullPath));
            }
        }

        public ICommand BackupCommand { get { return NewCommand(BackupCommand_Execute); } }
        private void BackupCommand_Execute(object obj)
        {
            var application = obj as ApplicationObject;
            if (application != null)
            {
                if (!BackupApplication(application))
                {
                    MessageBox.Show("A backup already exists for today.", application.Name, MessageBoxButton.OK, MessageBoxImage.Stop);
                    return;
                }

                Search();
                SelectedApplication = application;
            }
        }

        private static bool BackupApplication(ApplicationObject application)
        {
            var backupDirectory = Settings.My.BackupDirectory;

            var applicationBackupDirectory = Path.Combine(backupDirectory, application.Type, string.Format("{0}.{1:yyyyMMdd}", application.Name, DateTime.Now));
            if (Directory.Exists(applicationBackupDirectory))
            {
                return false;
            }

            Directory2.CopyDirectory(application.FullPath, applicationBackupDirectory, true);

            LoadBackupVersions(backupDirectory, application);

            var deleteBackupVersions = application.Backups.Skip(Settings.My.ApplicationMaxBackups).ToArray();
            if (deleteBackupVersions.Any())
            {
                foreach (var backup in deleteBackupVersions)
                {
                    if (Directory.Exists(backup.FullPath))
                    {
                        Directory.Delete(backup.FullPath, true);
                    }
                    application.Backups.Remove(backup);
                }
            }

            return true;
        }

        public void UpdateFilesForSelectedApplication(string[] files)
        {
            if (SelectedApplication == null || !files.Any())
            {
                return;
            }

            var di = new DirectoryInfo(files[0]);
            if (!di.Exists)
            {
                return;
            }

            FillUpdateFileExtensions(di);
        }

        private DirectoryInfo _updateParentDirectory;
        private bool? _allUpdateFileExtensionsAreChecked;

        private void FillUpdateFileExtensions(DirectoryInfo parentDirectory)
        {
            _updateParentDirectory = parentDirectory;
            var files = parentDirectory.GetFiles("*", SearchOption.AllDirectories);

            var safeFileExtensions = new[] { ".dll", ".exe" };
            UpdateFileExtensions = files.Select(x => x.Extension.ToLower()).Distinct().OrderBy(x => x).Select(x => new IsCheckedItem(x, safeFileExtensions.Contains(x), x)).ToObservableCollection();
        }

        public ICommand UpdateApplicationOkCommand { get { return NewCommand(UpdateApplicationOkCommand_Execute); } }
        private void UpdateApplicationOkCommand_Execute(object obj)
        {
            var application = SelectedApplication;

            var fileExtensions = UpdateFileExtensions.Where(x => x.IsChecked).Select(x => (string)x.Tag).ToArray();
            if (!fileExtensions.Any())
            {
                return;
            }

            var search = false;

            if (application.Backups.All(x => x.Date != DateTime.Today))
            {
                if (BackupApplication(application))
                {
                    search = true;
                }
            }

            var directories = _updateParentDirectory.GetDirectories("*", SearchOption.AllDirectories);
            foreach (var directory in directories)
            {
                var relativePath = directory.FullName.Substring(_updateParentDirectory.FullName.Length + 1);
                var newPath = Path.Combine(application.FullPath, relativePath);
                Directory.CreateDirectory(newPath);
            }

            var updatedFilesCount = 0;

            var files = _updateParentDirectory.GetFiles("*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                if (!fileExtensions.Contains(file.Extension.ToLower()))
                {
                    continue;
                }

                var relativePath = file.FullName.Substring(_updateParentDirectory.FullName.Length + 1);
                var newPath = Path.Combine(application.FullPath, relativePath);
                while (true)
                {
                    try
                    {
                        File.Copy(file.FullName, newPath, true);
                        updatedFilesCount++;
                        break;
                    }
                    catch (IOException ex)
                    {
                        var res2 = MessageBox.Show(string.Format("{0}\nError: {1}\n\nUnable to copy the file.\nClick OK to retry or Cancel to skip.", newPath, ex.Message), string.Format("Update {0}", application.Name), MessageBoxButton.OKCancel);
                        if (res2 == MessageBoxResult.Cancel)
                        {
                            break;
                        }
                    }
                }
            }

            UpdateFileExtensions = null;

            if (search)
            {
                Search();
                SelectedApplication = application;
            }

            MessageBox.Show(string.Format("Updated {0} files.", updatedFilesCount), string.Format("Update {0}", application.Name), MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public ICommand UpdateApplicationCancelCommand { get { return NewCommand(UpdateApplicationCancelCommand_Execute); } }
        private void UpdateApplicationCancelCommand_Execute(object obj)
        {
            UpdateFileExtensions = null;
        }
    }
}