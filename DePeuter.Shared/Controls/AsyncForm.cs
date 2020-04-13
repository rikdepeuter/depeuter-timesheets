using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Threading;

public partial class AsyncForm : Form
{
    private readonly Func<object> _action;
    private object _result;
    private Exception _error;
    private readonly Dispatcher _createdOnDispatcher = Dispatcher.CurrentDispatcher;

    private AsyncForm(Func<object> action)
    {
        InitializeComponent();

        _action = action;
    }

    public static T Start<T>(Func<T> action)
    {
        ASynch.RaiseOnModalAsyncStart();

        if(ASynch.IsEnabled)
        {
            var form = new AsyncForm(() => action());
            form.ShowDialog();

            ASynch.RaiseOnModalAsyncEnd();

            if(form._error != null)
            {
                throw new ASyncException(form._error);
            }

            return (T)form._result;
        }
        else
        {
            object res = null;
            try
            {
                res = action();

                ASynch.RaiseOnModalAsyncEnd();
            }
            catch
            {
                ASynch.RaiseOnModalAsyncEnd();

                throw;
            }

            return (T)res;
        }
    }

    public static void Start(Action action)
    {
        ASynch.RaiseOnModalAsyncStart();

        if(ASynch.IsEnabled)
        {
            var form = new AsyncForm(() =>
                {
                    action();
                    return null;
                });
            form.ShowDialog();

            ASynch.RaiseOnModalAsyncEnd();

            if(form._error != null)
            {
                throw new ASyncException(form._error);
            }
        }
        else
        {
            try
            {
                action();
            }
            finally
            {
                ASynch.RaiseOnModalAsyncEnd();
            }
        }
    }

    private void Async_Load(object sender, EventArgs e)
    {
        var bw = new BackgroundWorker();
        bw.DoWork += bw_DoWork;
        bw.RunWorkerCompleted += bw_RunWorkerCompleted;
        bw.RunWorkerAsync();
    }

    void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
        _createdOnDispatcher.Invoke(Close);
        //Close();
    }

    void bw_DoWork(object sender, DoWorkEventArgs e)
    {
        try
        {
            _result = _action();
        }
        catch(Exception ex)
        {
            _error = ex;
        }
    }
}

public class ASyncException : Exception
{
    public ASyncException(Exception exception)
        : base("Exception on async thread", exception)
    {
    }
}