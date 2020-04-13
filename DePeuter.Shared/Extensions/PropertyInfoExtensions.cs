using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

public static class PropertyInfoExtensions
{
    public static T[] GetCustomAttributes<T>(this PropertyInfo propertyInfo, bool inherit = false)
    {
        return propertyInfo.GetCustomAttributes(typeof(T), inherit).Select(x => (T)x).ToArray();
    }

    public static T GetCustomAttribute<T>(this PropertyInfo propertyInfo, bool inherit = false)
    {
        return propertyInfo.GetCustomAttributes(typeof(T), inherit).Select(x => (T)x).FirstOrDefault();
    }

    public static bool HasCustomAttribute<T>(this PropertyInfo propertyInfo, bool inherit = false)
    {
        return propertyInfo.GetCustomAttributes(typeof(T), inherit).Any();
    }

    public static bool HasCustomAttribute<T>(this PropertyInfo propertyInfo, Func<T, bool> predicate, bool inherit = false)
    {
        return propertyInfo.GetCustomAttributes(typeof(T), inherit).Select(x => (T)x).Any(predicate);
    }
}