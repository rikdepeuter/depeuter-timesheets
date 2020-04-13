using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using DePeuter.Shared.Wpf.Annotations;

namespace DePeuter.Shared.Wpf.Controls
{
    public class AsyncViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsRunning
        {
            get
            {
                return _asyncCounter > 0;
            }
        }

        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                OnPropertyChanged();
            }
        }

        public string SubMessage
        {
            get { return _subMessage; }
            set
            {
                _subMessage = value;
                OnPropertyChanged();
            }
        }

        private readonly object _lock = new object();
        private readonly Dictionary<string, int> _loadingCounters = new Dictionary<string, int>();
        private readonly Dictionary<string, DateTime> _asyncRequests = new Dictionary<string, DateTime>();
        private string _message;
        private string _subMessage;
        private int _asyncCounter;

        private void ShowLoading(string message, string subMessage, [CallerMemberName] string callerMemberName = "")
        {
            Message = message;
            SubMessage = subMessage;

            lock(_lock)
            {
                IncrementAsyncCounter(1);

                if(!_loadingCounters.ContainsKey(callerMemberName))
                {
                    _loadingCounters.Add(callerMemberName, 0);
                }

                _loadingCounters[callerMemberName]++;
            }
        }

        private void HideLoading([CallerMemberName] string callerMemberName = "")
        {
            lock(_lock)
            {
                var callerLoadingCounter = _loadingCounters[callerMemberName];
                _loadingCounters[callerMemberName] = 0;

                IncrementAsyncCounter(-callerLoadingCounter);
            }
        }

        private void IncrementAsyncCounter(int value)
        {
            _asyncCounter += value;
            OnPropertyChanged("IsRunning");
        }

        public async Task Run(Action action, Action handleResult, string message = null, string subMessage = null, [CallerMemberName] string callerMemberName = "")
        {
            await Run(() =>
            {
                action();
                return 0;
            }, x =>
            {
                if (handleResult != null)
                {
                    handleResult.Invoke();    
                }
            }, message, subMessage, callerMemberName);
        }

        public async Task Run<T>(Func<T> action, Action<T> handleResult, string message = null, string subMessage = null, [CallerMemberName] string callerMemberName = "")
        {
            var requestTime = DateTime.Now;
            lock(_lock)
            {
                if (_asyncRequests.ContainsKey(callerMemberName))
                {
                    _asyncRequests[callerMemberName] = requestTime;
                }
                else
                {
                    _asyncRequests.Add(callerMemberName, requestTime);
                }
            }

            ShowLoading(message, subMessage, callerMemberName);

            try
            {
                var result = await Task.Run(() =>
                {
                    var actionResult = action();
                    var cancel = false;
 
                    //only continue if its the latest request
                    lock(_lock)
                    {
                        if (_asyncRequests.ContainsKey(callerMemberName))
                        {
                            var latestRequestTime = _asyncRequests[callerMemberName];
                            if (latestRequestTime != requestTime)
                            {
                                cancel = true;
                                //cancelTokenSource.Cancel();
                                //cancelTokenSource.Token.ThrowIfCancellationRequested();
                            }
                        }
                    }
 
                    return new
                    {
                        Result = actionResult,
                        Cancel = cancel
                    };
                });

                if (!result.Cancel)
                {
                    if (handleResult != null)
                    {
                        handleResult.Invoke(result.Result);    
                    }
                }
            }
            catch(OperationCanceledException)
            {
            }
            finally
            {
                HideLoading(callerMemberName);
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if(PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
