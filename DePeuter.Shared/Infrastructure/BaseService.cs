using System;
using System.Reflection;
using log4net;

public delegate void DisposedHandler();

public interface IDataParameters
{
    DataParameters Data { get; set; }
}

public interface IBaseService : IDisposable
{
    //event DisposedHandler OnDisposed;
}

public abstract class BaseService : IBaseService
{
    protected readonly ILog Logger;

    //public event DisposedHandler OnDisposed;

    protected BaseService()
    {
        Logger = LogManager.GetLogger(GetType());
    }

    //protected virtual bool DisposeNonPublicIDisposableFieldsOnDispose
    //{
    //    get { return false; }
    //}

    public void Dispose()
    {
        Close();

        //if(DisposeNonPublicIDisposableFieldsOnDispose)
        //{
        //    //dispose alle non-public variabelen binnen de klasse
        //    var fields = GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
        //    foreach(var f in fields)
        //    {
        //        var disposable = f.GetValue(this) as IDisposable;
        //        if(disposable != null)
        //        {
        //            disposable.Dispose();
        //        }
        //    }
        //}

        //if(OnDisposed != null)
        //    OnDisposed();
    }

    protected virtual void Close()
    {
    }
}