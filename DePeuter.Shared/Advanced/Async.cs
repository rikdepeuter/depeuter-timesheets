using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Threading;

namespace DePeuter.Shared.Advanced
{
    public static class Async
    {
        public static bool IsEnabled { get; set; }

        public delegate void AsynchStartHandler();
        public delegate void AsynchEndHandler();
        public delegate void AsynchProgressHandler(object sender, string message, object data);
        public delegate void ASyncExceptionHandler(Exception ex);

        public static event AsynchStartHandler OnAsynchStart;
        public static event AsynchEndHandler OnAsynchEnd;
        public static event AsynchProgressHandler OnAsynchProgress;
        public static event ASyncExceptionHandler OnException;

        public static void RaiseOnAsyncStart()
        {
            if(OnAsynchStart != null)
                OnAsynchStart();
        }
        public static void RaiseOnAsyncEnd()
        {
            if(OnAsynchEnd != null)
                OnAsynchEnd();
        }

        public static void RaiseOnAsynchProgress(object sender, string message, object data)
        {
            if(OnAsynchProgress != null)
            {
                OnGUI(() => OnAsynchProgress(sender, message, data));
            }
        }

        private static readonly Dispatcher Gui;
        static Async()
        {
            Gui = Dispatcher.CurrentDispatcher;
            IsEnabled = true;
        }

        private static T OnGUI<T>(Func<T> action, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            return (T)Gui.Invoke(priority, action);
        }
        private static void OnGUI(Action action, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            Gui.Invoke(priority, action);
        }

        public static void Wait(int millisecondsTimeout, Action action)
        {
            Run((s, a) => Thread.Sleep(millisecondsTimeout), null, (s, a) => action());
        }

        public static void Run(Action action)
        {
            Run((s, a) => action(), null, (s, a) =>
            {
                if(a.Error != null && OnException != null)
                {
                    OnException(a.Error);
                }
            });
        }
        public static void Run<TArgument>(Action<TArgument> action, TArgument argument)
        {
            Run((s, a) => action((TArgument)a.Argument), null, (s, a) =>
            {
                if(a.Error != null && OnException != null)
                {
                    OnException(a.Error);
                }
            }, argument);
        }
        public static void Run(Action action, RunWorkerCompletedEventHandler onComplete)
        {
            Run((s, a) =>
            {
                if(action != null)
                {
                    action();
                }
            }, null, (s, a) =>
            {
                if(a.Error != null && OnException != null)
                {
                    OnException(a.Error);
                }

                onComplete(s, a);
            });
        }
        public static void Run<TArgument>(Action<TArgument> action, RunWorkerCompletedEventHandler onComplete, TArgument argument)
        {
            Run((s, a) => action((TArgument)a.Argument), null, (s, a) =>
            {
                if(a.Error != null && OnException != null)
                {
                    OnException(a.Error);
                }

                onComplete(s, a);
            }, argument);
        }

        public static void Run(DoWorkEventHandler doWork, ProgressChangedEventHandler progressChanged, RunWorkerCompletedEventHandler runWorkerCompleted, bool forceEndOnGUIThread = false)
        {
            Run(doWork, progressChanged, runWorkerCompleted, null, forceEndOnGUIThread);
        }
        public static void Run(DoWorkEventHandler doWork, ProgressChangedEventHandler progressChanged, RunWorkerCompletedEventHandler runWorkerCompleted, object argument, bool forceEndOnGUIThread = false)
        {
            var bw = new BackgroundWorker
            {
                WorkerSupportsCancellation = true,
                WorkerReportsProgress = true
            };

            if(!IsEnabled)
            {
                var doWorkArgs = new DoWorkEventArgs(null);
                Exception error = null;
                try
                {
                    doWork(bw, doWorkArgs);
                }
                catch(Exception ex)
                {
                    error = ex;
                }

                var runWorkerCompletedArgs = new RunWorkerCompletedEventArgs(doWorkArgs.Result, error, doWorkArgs.Cancel);
                runWorkerCompleted(bw, runWorkerCompletedArgs);
                return;
            }

            if(doWork != null)
                bw.DoWork += doWork;

            if(progressChanged != null)
                bw.ProgressChanged += progressChanged;

            if(runWorkerCompleted != null)
                bw.RunWorkerCompleted += (sender, e) =>
                {
                    Action complete = () =>
                    {
                        try
                        {
                            runWorkerCompleted(sender, e);
                        }
                        finally
                        {
                            RaiseOnAsyncEnd();
                        }
                    };

                    if (forceEndOnGUIThread)
                    {
                        OnGUI(complete);
                    }
                    else
                    {
                        complete();
                    }
                };

            RaiseOnAsyncStart();

            if(argument != null)
            {
                bw.RunWorkerAsync(argument);
            }
            else
            {
                bw.RunWorkerAsync();
            }
        }

        public delegate T DoWorkEventHandler2<T>(BackgroundWorker bw, DoWorkEventArgs2 e) where T : class;
        public delegate TResult DoWorkEventHandler3<out TResult, in TArgument>(BackgroundWorker bw, TArgument argument, DoWorkEventArgs2 e)
            where TResult : class
            where TArgument : class;
        public delegate void ProgressChangedEventHandler2(BackgroundWorker bw, ProgressChangedEventArgs e);
        public delegate void RunWorkerCompletedEventHandler2<T>(BackgroundWorker bw, T result, Exception error, bool cancelled) where T : class;

        public class DoWorkEventArgs2
        {
            public bool Cancel { get; set; }
        }

        public static void Run<TArgument, TResult>(TArgument argument, DoWorkEventHandler3<TResult, TArgument> doWork, ProgressChangedEventHandler2 progressChanged, RunWorkerCompletedEventHandler2<TResult> runWorkerCompleted, bool forceEndOnGUIThread = false)
            where TResult : class
            where TArgument : class
        {
            var bw = new BackgroundWorker();
            bw.WorkerSupportsCancellation = true;
            bw.WorkerReportsProgress = true;

            if(!IsEnabled)
            {
                var doWorkArgs = new DoWorkEventArgs2();
                Exception error = null;
                TResult result = null;
                try
                {
                    result = doWork(bw, argument, doWorkArgs);
                }
                catch(Exception ex)
                {
                    error = ex;
                }
                runWorkerCompleted(bw, result, error, doWorkArgs.Cancel);
                return;
            }

            bw.DoWork += (object sender, DoWorkEventArgs e) =>
            {
                var a = new DoWorkEventArgs2();
                e.Result = doWork(bw, (TArgument)e.Argument, a);
                e.Cancel = a.Cancel;
            };

            if(progressChanged != null)
                bw.ProgressChanged += (object sender, ProgressChangedEventArgs e) =>
                {
                    progressChanged(bw, e);
                };

            if(runWorkerCompleted != null)
                bw.RunWorkerCompleted += (object sender, RunWorkerCompletedEventArgs e) =>
                {
                    try
                    {
                        if(forceEndOnGUIThread)
                        {
                            if(e.Error != null)
                                OnGUI(() => runWorkerCompleted(bw, null, e.Error, e.Cancelled));
                            else
                                OnGUI(() => runWorkerCompleted(bw, (TResult)e.Result, null, e.Cancelled));
                        }
                        else
                        {
                            if(e.Error != null)
                                runWorkerCompleted(bw, null, e.Error, e.Cancelled);
                            else
                                runWorkerCompleted(bw, (TResult)e.Result, null, e.Cancelled);
                        }
                    }
                    finally
                    {
                        if(forceEndOnGUIThread)
                        {
                            OnGUI(RaiseOnAsyncEnd);
                        }
                        else
                        {
                            RaiseOnAsyncEnd();
                        }
                    }
                };

            RaiseOnAsyncStart();

            bw.RunWorkerAsync(argument);
        }

        public static void Run<TResult>(DoWorkEventHandler2<TResult> doWork, ProgressChangedEventHandler2 progressChanged, RunWorkerCompletedEventHandler2<TResult> runWorkerCompleted, bool forceEndOnGUIThread = false) where TResult : class
        {
            var bw = new BackgroundWorker();
            bw.WorkerSupportsCancellation = true;
            bw.WorkerReportsProgress = true;

            if(!IsEnabled)
            {
                var doWorkArgs = new DoWorkEventArgs2();
                Exception error = null;
                TResult result = null;
                try
                {
                    result = doWork(bw, doWorkArgs);
                }
                catch(Exception ex)
                {
                    error = ex;
                }
                runWorkerCompleted(bw, result, error, doWorkArgs.Cancel);
                return;
            }

            bw.DoWork += (object sender, DoWorkEventArgs e) =>
            {
                var a = new DoWorkEventArgs2();
                e.Result = doWork(bw, a);
                e.Cancel = a.Cancel;
            };

            if(progressChanged != null)
                bw.ProgressChanged += (object sender, ProgressChangedEventArgs e) =>
                {
                    progressChanged(bw, e);
                };

            if(runWorkerCompleted != null)
                bw.RunWorkerCompleted += (object sender, RunWorkerCompletedEventArgs e) =>
                {
                    try
                    {
                        if(forceEndOnGUIThread)
                        {
                            if(e.Error != null)
                                OnGUI(() => runWorkerCompleted(bw, null, e.Error, e.Cancelled));
                            else
                                OnGUI(() => runWorkerCompleted(bw, (TResult)e.Result, null, e.Cancelled));
                        }
                        else
                        {
                            if(e.Error != null)
                                runWorkerCompleted(bw, null, e.Error, e.Cancelled);
                            else
                                runWorkerCompleted(bw, (TResult)e.Result, null, e.Cancelled);
                        }
                    }
                    finally
                    {
                        if(forceEndOnGUIThread)
                        {
                            OnGUI(RaiseOnAsyncEnd);
                        }
                        else
                        {
                            RaiseOnAsyncEnd();
                        }
                    }
                };

            RaiseOnAsyncStart();

            bw.RunWorkerAsync();
        }
    }
}