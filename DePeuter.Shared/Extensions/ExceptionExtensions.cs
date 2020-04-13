using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public interface ISummary
{
    string GetSummary();
}

public static class ExceptionExtensions
{
    public static string Summary(this Exception ex)
    {
        if(ex is ISummary)
        {
            return ((ISummary)ex).GetSummary();
        }

        var summaryPi = ex.GetType().GetProperty("Summary");
        if(summaryPi != null)
        {
            return (string)summaryPi.GetValue(ex, null);
        }

        var sb = new StringBuilder();
        sb.AppendLine(ex.GetType().GetNameWithGenerics() + ": " + ex.Message);

        var stacktraces = new List<string>();
        stacktraces.Add(ex.StackTrace);

        var e = ex.InnerException;
        int counter = 0;
        while(e != null)
        {
            counter++;

            sb.AppendLine("INNER #" + counter + " | " + e.GetType().GetNameWithGenerics() + ": " + e.Message);
            stacktraces.Add(e.StackTrace);

            e = e.InnerException;
        }

        var sb_stacktrace = new StringBuilder();
        for(int i = stacktraces.Count - 1; i >= 0; i--)
        {
            sb_stacktrace.AppendLine(stacktraces[i]);
        }

        return sb.ToString().Trim('\n') + "\n" + sb_stacktrace.ToString();
    }

    public static string ToFullString(this Exception ex)
    {
        return ex.Summary();
        //return ex.ToCustomXML();
    }

    //private static void AppendInfo(StringBuilder sb, Exception ex)
    //{
    //    sb.AppendLine(ex.GetType().GetNameWithGenerics() + ": " + ex.Message);
    //    if (ex.Data != null)
    //    {
    //        sb.AppendLine("<Data>");
    //        foreach (var key in ex.Data.Keys)
    //        {
    //            var value = ex.Data[key];
    //            sb.AppendLine(string.Format("<Key>{0}</Key>", key));
    //            sb.Append("<Value>");
    //            AppendExtendedProperties(sb, value);
    //            sb.Append("</Value>");
    //        }
    //        sb.AppendLine("</Data>");
    //    }
    //    AppendExtendedProperties(sb, ex);
    //    sb.AppendLine(ex.StackTrace);
    //}

    //private static readonly string[] DEFAULT_EXCEPTION_PROPERTIES = new[] { "Data", "HelpLink", "InnerException", "Message", "Source", "StackTrace", "TargetSite" };
    //private static void AppendExtendedProperties(StringBuilder sb, object item)
    //{
    //    if (item == null) return;

    //    var properties = item.GetType().GetProperties();
    //    if (item is Exception)
    //    {
    //        properties = properties.Where(x => !DEFAULT_EXCEPTION_PROPERTIES.Contains(x.Name)).ToArray();
    //    }
    //    properties = properties.Where(x => x.CanRead).ToArray();

    //    foreach (var p in properties)
    //    {
    //        try
    //        {
    //            var value = p.GetValue(item, null);



    //        }
    //        catch (Exception) { }
    //    }
    //}

    public static Exception Skip(this Exception ex, params Type[] exceptionTypes)
    {
        if(exceptionTypes == null) return ex;

        var e = ex;
        while(exceptionTypes.Any(x => e.GetType().IsAssignableFrom(x)) && e.InnerException != null)
        {
            e = e.InnerException;
        }
        return e;
    }
    public static Exception Skip<TException>(this Exception ex) where TException : Exception
    {
        return ex.Skip(typeof(TException));
    }
    public static Exception Skip<TException1, TException2>(this Exception ex)
        where TException1 : Exception
        where TException2 : Exception
    {
        return ex.Skip(typeof(TException1), typeof(TException2));
    }
    public static Exception Skip<TException1, TException2, TException3>(this Exception ex)
        where TException1 : Exception
        where TException2 : Exception
        where TException3 : Exception
    {
        return ex.Skip(typeof(TException1), typeof(TException2), typeof(TException3));
    }
    public static Exception Skip<TException1, TException2, TException3, TException4>(this Exception ex)
        where TException1 : Exception
        where TException2 : Exception
        where TException3 : Exception
        where TException4 : Exception
    {
        return ex.Skip(typeof(TException1), typeof(TException2), typeof(TException3), typeof(TException4));
    }
    public static Exception Skip<TException1, TException2, TException3, TException4, TException5>(this Exception ex)
        where TException1 : Exception
        where TException2 : Exception
        where TException3 : Exception
        where TException4 : Exception
        where TException5 : Exception
    {
        return ex.Skip(typeof(TException1), typeof(TException2), typeof(TException3), typeof(TException4), typeof(TException5));
    }

    public static bool HasInnerException<TException>(this Exception ex) where TException : Exception
    {
        var e = ex.InnerException;
        while(e != null)
        {
            if(e is TException) return true;

            e = e.InnerException;
        }
        return false;
    }

    public static TException GetInnerException<TException>(this Exception ex) where TException : Exception
    {
        var e = ex.InnerException;
        while(e != null)
        {
            if(e is TException) return (TException)e;

            e = e.InnerException;
        }
        return null;
    }

    public static TException FindInnerException<TException>(this Exception ex, Func<TException, bool> predicate) where TException : Exception
    {
        var e = ex.InnerException;
        while(e != null)
        {
            if(e is TException)
            {
                var inner = (TException)e;
                if(predicate(inner))
                {
                    return inner;
                }
            }

            e = e.InnerException;
        }
        return null;
    }

    public static Exception FindInnerException(this Exception ex, Func<Exception, bool> predicate)
    {
        var e = ex.InnerException;
        while(e != null)
        {
            if(predicate(ex))
            {
                return e;
            }

            e = e.InnerException;
        }
        return null;
    }
}