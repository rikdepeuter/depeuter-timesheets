using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

public static class MethodInfoExtensions
{
    public static T[] GetCustomAttributes<T>(this MethodInfo methodInfo, bool inherit = false)
    {
        return methodInfo.GetCustomAttributes(typeof(T), inherit).Select(x => (T)x).ToArray();
    }

    public static T GetCustomAttribute<T>(this MethodInfo methodInfo, bool inherit = false)
    {
        return methodInfo.GetCustomAttributes(typeof(T), inherit).Select(x => (T)x).FirstOrDefault();
    }
}