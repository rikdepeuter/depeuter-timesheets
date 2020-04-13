using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public delegate void DetachAllHandler(object target);
public delegate void DetachHandler(Type requestType);

public interface IRequestDetacher
{
    event DetachAllHandler DetachAll;
    event DetachHandler Detach;
}

public static class Request
{
    public delegate void RequestHandler(object sender);

    private static readonly Dictionary<Type, RequestHandler> RequestHandlerMapping = new Dictionary<Type, RequestHandler>();
    private static readonly Dictionary<object, List<Type>> RequestHandlerTargets = new Dictionary<object, List<Type>>();

    public static T Execute<T>()
        where T : class, new()
    {
        return Execute(new T());
    }

    public static T Execute<T>(T request)
        where T : class
    {
        var requestType= typeof(T);
        if(!RequestHandlerMapping.ContainsKey(requestType))
        {
            throw new NoRequestHandlerException(requestType);
        }

        RequestHandlerMapping[requestType](request);

        return request;
    }

    public static void Attach<T>(RequestHandler handler)
        where T : class
    {
        var requestType = typeof(T);
        if(RequestHandlerMapping.ContainsKey(requestType))
        {
            throw new RequestHandlerExistsException(requestType);
        }

        RequestHandlerMapping.Add(requestType, handler);

        var target = handler.Target as IRequestDetacher;
        if (target == null) return;

        if (!RequestHandlerTargets.ContainsKey(target))
        {
            RequestHandlerTargets.Add(handler.Target, new List<Type>());
        }

        RequestHandlerTargets[handler.Target].Add(requestType);

        target.DetachAll += target_DetachAll;
        target.Detach += target_Detach;
    }

    static void target_DetachAll(object target)
    {
        foreach(var requestType in RequestHandlerTargets[target])
        {
            RequestHandlerMapping.Remove(requestType);
        }
    }

    static void target_Detach(Type requestType)
    {
        RequestHandlerMapping.Remove(requestType);
    }

    public static void Detach<T>()
        where T : class
    {
        var requestType= typeof(T);

        RequestHandlerMapping.Remove(requestType);
    }
}

public class NoRequestHandlerException : Exception
{
    public NoRequestHandlerException(Type requestType)
        : base(string.Format("No handler active for the request '{0}'", requestType.FullName))
    {
    }
}

public class RequestHandlerExistsException : Exception
{
    public RequestHandlerExistsException(Type requestType)
        : base(string.Format("There is already a handler hooked for the request '{0}'", requestType.FullName))
    {
    }
}