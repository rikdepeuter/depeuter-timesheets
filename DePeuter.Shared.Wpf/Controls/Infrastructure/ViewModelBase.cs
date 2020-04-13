using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DePeuter.Shared.Advanced;
using DePeuter.Shared.Exceptions;
using DePeuter.Shared.Wpf.Commands;
using log4net;
using PropertyChanged;

namespace DePeuter.Shared.Wpf.Controls.Infrastructure
{
    [ImplementPropertyChanged]
    public abstract class ViewModelBase : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        protected readonly ILog Logger;

        public ViewModelBase Parent { get; set; }

        private readonly object _lock = new object();

        private readonly Dictionary<string, int> _loadingCounters = new Dictionary<string, int>();
        public bool IsLoading
        {
            get
            {
                return LoadingCounter > 0;
            }
        }
        public int LoadingCounter { get; set; }
        public string LoadingMessage { get; set; }
        public string LoadingSubMessage { get; set; }

        public void ShowLoading(string message, string subMessage, [CallerMemberName] string callerMemberName = "")
        {
            LoadingMessage = message;
            LoadingSubMessage = subMessage;
            LoadingCounter++;

            lock(_lock)
            {
                if(!_loadingCounters.ContainsKey(callerMemberName))
                {
                    _loadingCounters.Add(callerMemberName, 0);
                }

                _loadingCounters[callerMemberName]++;
            }
        }
        public void HideLoading([CallerMemberName] string callerMemberName = "")
        {
            lock(_lock)
            {
                var callerLoadingCounter = _loadingCounters[callerMemberName];
                _loadingCounters[callerMemberName] = 0;

                LoadingCounter -= callerLoadingCounter;
            }
        }

        private readonly Dictionary<string, DateTime> _asyncRequests = new Dictionary<string, DateTime>();
        public bool DisableAsync { get; protected set; }

        protected virtual void BeforeLoadAsync(string message, string subMessage, string callerMemberName)
        {
        }

        protected void LoadAsync<T>(Func<T> loadAsync, Action<T> onCompleted, string message = null, string subMessage = null, [CallerMemberName] string callerMemberName = "", bool forceEndOnGUIThread = false)
        {
            LoadAsync(loadAsync, onCompleted, null, message, subMessage, callerMemberName, forceEndOnGUIThread);
        }

        protected void LoadAsync<T>(Func<T> loadAsync, Action<T> onCompleted, Action<Exception> onError, string message = null, string subMessage = null, [CallerMemberName] string callerMemberName = "", bool forceEndOnGUIThread = false)
        {
            Action<Exception> handleException = ex =>
            {
                if(onError != null)
                {
                    onError(ex);
                }
                else
                {
                    HandleException(ex);
                }
            };

            LoadAsync((s, e) =>
            {
                e.Result = loadAsync();
            }, null, (s, e) =>
            {
                if(e.Error != null)
                {
                    handleException(e.Error);
                }
                else if(onCompleted != null)
                {
                    onCompleted((T)e.Result);
                }
            }, handleException, message, subMessage, callerMemberName, forceEndOnGUIThread);
        }

        protected void LoadAsync(Action executeAsync, Action onCompleted, string message = null, string subMessage = null, [CallerMemberName] string callerMemberName = "", bool forceEndOnGUIThread = false)
        {
            LoadAsync(executeAsync, onCompleted, null, message, subMessage, callerMemberName, forceEndOnGUIThread);
        }

        protected void LoadAsync(Action executeAsync, Action onCompleted, Action<Exception> onError, string message = null, string subMessage = null, [CallerMemberName] string callerMemberName = "", bool forceEndOnGUIThread = false)
        {
            Action<Exception> handleException = ex =>
            {
                if(onError != null)
                {
                    onError(ex);
                }
                else
                {
                    HandleException(ex);
                }
            };

            LoadAsync((s, e) =>
            {
                executeAsync();
            }, null, (s, e) =>
            {
                if(e.Error != null)
                {
                    handleException(e.Error);
                }
                else if(onCompleted != null)
                {
                    onCompleted();
                }
            }, handleException, message, subMessage, callerMemberName, forceEndOnGUIThread);
        }

        protected void LoadAsync(DoWorkEventHandler doWork, ProgressChangedEventHandler progressChanged, RunWorkerCompletedEventHandler runWorkerCompleted, Action<Exception> onError = null, string message = null, string subMessage = null, [CallerMemberName] string callerMemberName = "", bool forceEndOnGUIThread = false)
        {
            BeforeLoadAsync(message, subMessage, callerMemberName);

            if(DisableAsync)
            {
                try
                {
                    doWork(null, null);

                    if(runWorkerCompleted != null)
                    {
                        runWorkerCompleted(null, null);
                    }
                }
                catch(Exception ex)
                {
                    if(onError == null)
                    {
                        throw;
                    }

                    onError(ex);
                }
                return;
            }

            ShowLoading(message, subMessage, callerMemberName);

            var requestTime = DateTime.Now;
            lock(_lock)
            {
                _asyncRequests.Set(callerMemberName, requestTime);
            }

            Async.Run(doWork, progressChanged, (s, e) =>
            {
                //only if its the latest request
                lock(_lock)
                {
                    var latestRequestTime = _asyncRequests.Get(callerMemberName);
                    if(latestRequestTime != requestTime)
                    {
                        return;
                    }
                }

                HideLoading(callerMemberName);

                if(runWorkerCompleted != null)
                {
                    try
                    {
                        runWorkerCompleted(s, e);
                    }
                    catch(Exception ex)
                    {
                        if(onError == null)
                        {
                            throw;
                        }

                        onError(ex);
                    }
                }
            }, forceEndOnGUIThread: forceEndOnGUIThread);
        }

        protected ViewModelBase()
        {
            Logger = LogManager.GetLogger(GetType());
        }

        protected ViewModelBase(ViewModelBase parent)
            : this()
        {
            Parent = parent;
        }

        public virtual void HandleException(Exception ex)
        {
            if(Parent != null)
            {
                Parent.HandleException(ex);
            }
            else
            {
                throw ex;
            }
        }

        private static readonly Dictionary<Type, Dictionary<Type, PropertyInfo[]>> ViewModelBasePropertiesDictionary = new Dictionary<Type, Dictionary<Type, PropertyInfo[]>>();
        protected void ExecuteOnPublicViewModelBaseProperties<T>(Action<T> action)
            where T : ViewModelBase
        {
            var type = GetType();
            var wantedPropertyType = typeof(T);

            lock(_lock)
            {
                if(!ViewModelBasePropertiesDictionary.ContainsKey(type))
                {
                    ViewModelBasePropertiesDictionary.Add(type, new Dictionary<Type, PropertyInfo[]>());
                }

                if(!ViewModelBasePropertiesDictionary[type].ContainsKey(wantedPropertyType))
                {
                    ViewModelBasePropertiesDictionary[type].Add(wantedPropertyType, type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.Name != "Parent" && wantedPropertyType.IsAssignableFrom(x.PropertyType)).ToArray());
                }
            }

            var viewModelBaseProperties = ViewModelBasePropertiesDictionary[type][wantedPropertyType];
            foreach(var viewModelBaseProperty in viewModelBaseProperties)
            {
                var viewModelBase = viewModelBaseProperty.GetValue(this, null) as T;
                if(viewModelBase == null)
                {
                    continue;
                }

                action(viewModelBase);
            }
        }

        public void RaiseLoaded(object sender, RoutedEventArgs e)
        {
            Loaded(sender, e);

            ExecuteOnPublicViewModelBaseProperties((ViewModelBase vm) => vm.RaiseLoaded(sender, e));
        }
        protected virtual void Loaded(object sender, RoutedEventArgs e)
        {
        }

        protected ICommand NewCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            return new RelayCommand(obj =>
            {
                try
                {
                    execute(obj);
                }
                catch(Exception ex)
                {
                    HandleException(ex);
                }
            }, canExecute);
        }

        protected ICommand NewCommandAsync(Func<object, Task> execute, Predicate<object> canExecute = null)
        {
            return new RelayCommand(async obj =>
            {
                try
                {
                    await execute(obj);
                }
                catch(Exception ex)
                {
                    HandleException(ex);
                }
            }, canExecute);
        }

        public T FindParent<T>() where T : ViewModelBase
        {
            var p = Parent;
            while(p != null)
            {
                var res = p as T;
                if(res != null)
                {
                    return res;
                }

                p = p.Parent;
            }
            return null;
        }

        public ICommand ResetValueCommand { get { return NewCommand(ResetValueCommand_Execute); } }
        private void ResetValueCommand_Execute(object obj)
        {
            var name = obj.ToString();
            var pi = GetType().GetProperty(name);
            if(pi == null)
            {
                throw new InvalidOperationException(string.Format("Unknown property: {0}", name));
            }
            pi.SetValue(this, null);
        }

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Warns the developer if this object does not have
        /// a public property with the specified name. This 
        /// method does not exist in a Release build.
        /// </summary>
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        private void VerifyPropertyName(string propertyName)
        {
            // Verify that the property name matches a real,  
            // public, instance property on this object.
            if(TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                var msg = "Invalid property name: " + propertyName;

                throw new Exception(msg);
            }
        }
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            VerifyPropertyName(propertyName);

            var handler = this.PropertyChanged;
            if(handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
            ValidateAsync();
        }
        //protected void OnPropertyChangedOnGuiIdle([CallerMemberName] string propertyName = "")
        //{
        //    System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() => { OnPropertyChanged(propertyName); }), null);
        //}
        #endregion
        #region INotifyDataErrorInfo
        private ConcurrentDictionary<string, List<string>> _errors = new ConcurrentDictionary<string, List<string>>();

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public void OnErrorsChanged(string propertyName)
        {
            var handler = ErrorsChanged;
            if(handler != null)
                handler(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public IEnumerable GetErrors(string propertyName)
        {
            List<string> errorsForName;
            _errors.TryGetValue(propertyName, out errorsForName);
            return errorsForName;
        }

        public bool HasErrors
        {
            get { return _errors.Any(kv => kv.Value != null && kv.Value.Count > 0); }
        }

        public Task ValidateAsync()
        {
            return Task.Run(() => Validate());
        }

        private object __lock = new object();
        public void Validate()
        {
            lock(__lock)
            {
                var validationContext = new ValidationContext(this, null, null);
                var validationResults = new List<ValidationResult>();
                Validator.TryValidateObject(this, validationContext, validationResults, true);

                foreach(var kv in _errors.ToList())
                {
                    if(validationResults.All(r => r.MemberNames.All(m => m != kv.Key)))
                    {
                        List<string> outLi;
                        _errors.TryRemove(kv.Key, out outLi);
                        OnErrorsChanged(kv.Key);
                    }
                }

                var q = from r in validationResults
                        from m in r.MemberNames
                        group r by m into g
                        select g;

                foreach(var prop in q)
                {
                    var messages = prop.Select(r => r.ErrorMessage).ToList();

                    if(_errors.ContainsKey(prop.Key))
                    {
                        List<string> outLi;
                        _errors.TryRemove(prop.Key, out outLi);
                    }
                    _errors.TryAdd(prop.Key, messages);
                    OnErrorsChanged(prop.Key);
                }
            }
        }
        #endregion
    }
}
