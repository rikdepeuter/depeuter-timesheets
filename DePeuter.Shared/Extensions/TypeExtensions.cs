using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

public static class TypeExtensions
{
    public static T[] GetCustomAttributes<T>(this Type type, bool inherit = false)
    {
        return type.GetCustomAttributes(typeof(T), inherit).Select(x => (T)x).ToArray();
    }

    public static T GetCustomAttribute<T>(this Type type, bool inherit = false)
    {
        return type.GetCustomAttributes(typeof(T), inherit).Select(x => (T)x).FirstOrDefault();
    }

    public static bool HasCustomAttribute<T>(this Type type, bool inherit = false)
    {
        return type.GetCustomAttributes(typeof(T), inherit).Any();
    }

    public static bool HasCustomAttribute<T>(this Type type, Func<T, bool> predicate, bool inherit = false)
    {
        return type.GetCustomAttributes(typeof(T), inherit).Select(x => (T)x).Any(predicate);
    }

    private readonly static Type[] SimpleTypes = { typeof(String), typeof(Decimal), typeof(DateTime), typeof(DateTimeOffset), typeof(TimeSpan),typeof(Guid) };
    public static bool IsSimpleType(this Type type)
    {
        return type.IsValueType || type.IsPrimitive || SimpleTypes.Contains(type) || Convert.GetTypeCode(type) != TypeCode.Object;
    }

    public static bool IsSystemType(this Type type)
    {
        return type.Namespace == "System";
    }

    public static bool IsComObject(this Type type)
    {
        return type.Namespace == "System" && type.Name == "__ComObject";
    }

    public static bool IsAnonymousType(this Type type)
    {
        var hasCompilerGeneratedAttribute = type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Length > 0;
        var nameContainsAnonymousType = type.FullName.Contains("AnonymousType");
        return hasCompilerGeneratedAttribute && nameContainsAnonymousType;
    }

    public static bool IsNullableSystemType(this Type type)
    {
        if(!type.IsValueType) return false; // ref-type
        if(Nullable.GetUnderlyingType(type) != null) return true; // Nullable<T>
        return false; // value-type
    }

    public static bool IsNullableType(this Type type)
    {
        if(!type.IsValueType) return true; // ref-type
        if(Nullable.GetUnderlyingType(type) != null) return true; // Nullable<T>
        return false; // value-type
    }

    public static bool IsListType(this Type type)
    {
        return typeof(IList).IsAssignableFrom(type);
    }

    public static string GetNameWithGenerics(this Type type)
    {
        var genericArgs = type.GetGenericArguments();
        if(genericArgs.Any())
        {
            return string.Format("{0}<{1}>", type.Name, string.Join(",", genericArgs.Select(x => x.Name).ToArray()));
        }
        return type.Name;
    }

    public static object GetDefaultValue(this Type type)
    {
        // If no Type was supplied, if the Type was a reference type, or if the Type was a System.Void, return null
        if(type == null || !type.IsValueType || type == typeof(void))
        {
            return null;
        }

        // If the supplied Type has generic parameters, its default value cannot be determined
        if(type.ContainsGenericParameters)
            throw new ArgumentException(
                "{" + MethodInfo.GetCurrentMethod() + "} Error:\n\nThe supplied value type <" + type +
                "> contains generic parameters, so the default value cannot be retrieved");

        // If the Type is a primitive type, or if it is another publicly-visible value type (i.e. struct/enum), return a 
        //  default instance of the value type
        if(type.IsPrimitive || !type.IsNotPublic)
        {
            try
            {
                return Activator.CreateInstance(type);
            }
            catch(Exception e)
            {
                throw new ArgumentException(
                    "{" + MethodInfo.GetCurrentMethod() + "} Error:\n\nThe Activator.CreateInstance method could not " +
                    "create a default instance of the supplied value type <" + type +
                    "> (Inner Exception message: \"" + e.Message + "\")", e);
            }
        }

        // Fail with exception
        throw new ArgumentException("{" + MethodInfo.GetCurrentMethod() + "} Error:\n\nThe supplied value type <" + type + 
            "> is not a publicly-visible type, so the default value cannot be retrieved");
    }
}