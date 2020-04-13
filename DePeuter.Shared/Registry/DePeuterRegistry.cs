using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using Microsoft.Win32;

public static class DePeuterRegistry
{
    public const string RootPath = "SOFTWARE\\DePeuter\\";

    public static RegistryKey CurrentUser
    {
        get { return RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32); }
    }
    public static RegistryKey LocalMachine
    {
        get { return RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32); }
    }

    public static T GetValue<T>(string key, string path = null, RegistryType registryType = RegistryType.CurrentUser)
    {
        if(string.IsNullOrEmpty(key))
        {
            throw new ArgumentNullException("key");
        }

        var root = GetRoot(registryType);

        using(var registryKey = root.OpenSubKey(RootPath + (path ?? string.Empty), RegistryKeyPermissionCheck.ReadSubTree, RegistryRights.ReadKey))
        {
            if(registryKey != null)
            {
                var value = registryKey.GetValue(key);
                if(value != null)
                {
                    return value.CastTo<T>();
                }
            }
        }

        return default(T);
    }

    public static object Get(string key)
    {
        return Get(null, key);
    }
    public static object Get(string name, string key)
    {
        var result = Get(CurrentUser, name, key);
        if(result == null)
        {
            try
            {
                result = Get(LocalMachine, name, key);
            }
            catch { }
        }
        return result;
    }
    private static object Get(RegistryKey root, string name, string key)
    {
        using(var registryKey = root.OpenSubKey(RootPath + (name ?? string.Empty), RegistryKeyPermissionCheck.ReadSubTree, RegistryRights.ReadKey))
        {
            if(registryKey != null)
            {
                return registryKey.GetValue(key);
            }
        }

        return null;
    }

    public static void Set(string key, object value)
    {
        Set(CurrentUser, null, key, value);
    }
    public static void Set(string name, string key, object value)
    {
        Set(CurrentUser, name, key, value);
    }
    private static void Set(RegistryKey root, string name, string key, object value)
    {
        var registryKey = root.OpenSubKey(RootPath, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.SetValue | RegistryRights.CreateSubKey);
        try
        {
            if(registryKey == null)
            {
                registryKey = root.CreateSubKey(RootPath);
            }

            if(!string.IsNullOrEmpty(name))
            {
                Set(root, registryKey, name, key, value);
            }
            else
            {
                if(registryKey != null)
                {
                    registryKey.SetValue(key, value);
                }
            }
        }
        finally
        {
            if(registryKey != null)
            {
                registryKey.Close();
            }
        }
    }
    private static void Set(RegistryKey root, RegistryKey parent, string name, string key, object value)
    {
        var registryKey = parent.OpenSubKey(name, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.SetValue | RegistryRights.CreateSubKey);
        try
        {
            if(registryKey == null)
            {
                registryKey = parent.CreateSubKey(name);
            }

            if(registryKey != null)
            {
                registryKey.SetValue(key, value);
            }
        }
        finally
        {
            if(registryKey != null)
            {
                registryKey.Close();
            }
        }
    }

    private static RegistryKey GetRoot(RegistryType registryType)
    {
        return registryType == RegistryType.CurrentUser ? CurrentUser : LocalMachine;
    }
    public static T GetEntity<T>(RegistryType registryType = RegistryType.CurrentUser) where T : class
    {
        var root = GetRoot(registryType);

        return GetEntity(typeof(T), root) as T;
    }

    public static void SaveEntity<T>(T entity, RegistryType registryType = RegistryType.CurrentUser)
    {
        var root = GetRoot(registryType);

        SaveEntity(entity.GetType(), entity, root);
    }

    private static object GetEntity(Type type, RegistryKey root)
    {
        var entityAttribute = type.GetCustomAttribute<RegistryEntityAttribute>();
        if(entityAttribute == null)
        {
            throw new MissingAttributeException(type, typeof(RegistryEntityAttribute));
        }

        var properties = type.GetProperties().Select(x =>
        {
            return new
            {
                PropertyInfo = x,
                FieldAttribute = x.GetCustomAttribute<RegistryFieldAttribute>()
            };
        }).Where(x => x.FieldAttribute != null).ToArray();

        object entity = null;

        using(var registryKey = root.OpenSubKey(RootPath + entityAttribute.Path, RegistryKeyPermissionCheck.ReadSubTree, RegistryRights.ReadKey))
        {
            if(registryKey != null)
            {
                entity = Activator.CreateInstance(type);

                //TODO subentities
                foreach(var property in properties)
                {
                    var pi = property.PropertyInfo;

                    var e1 = new RegistryFieldGetRegistryValueParameters(registryKey, type, pi);
                    var registryValue = property.FieldAttribute.GetRegistryValue(e1);

                    property.FieldAttribute.SetPropertyValue(new RegistryFieldSetPropertyValueParameters(type, pi, entity, registryValue));
                }
            }
        }

        return entity;
    }

    private static void SaveEntity(Type type, object entity, RegistryKey root)
    {
        var entityAttribute = type.GetCustomAttribute<RegistryEntityAttribute>();
        if(entityAttribute == null)
        {
            throw new MissingAttributeException(type, typeof(RegistryEntityAttribute));
        }

        var properties = type.GetProperties().Select(x =>
        {
            return new
            {
                PropertyInfo = x,
                FieldAttribute = x.GetCustomAttribute<RegistryFieldAttribute>()
            };
        }).Where(x => x.FieldAttribute != null).ToArray();

        var registryKey = root.OpenSubKey(RootPath + entityAttribute.Path, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.WriteKey | RegistryRights.ReadKey | RegistryRights.CreateSubKey | RegistryRights.SetValue);
        try
        {
            if(registryKey == null)
            {
                registryKey = root.CreateSubKey(RootPath + entityAttribute.Path, RegistryKeyPermissionCheck.ReadWriteSubTree);
            }

            //TODO subentities
            foreach(var property in properties)
            {
                var pi = property.PropertyInfo;

                var e1 = new RegistryFieldGetPropertyValueParameters(type, pi, entity);
                var propertyValue = property.FieldAttribute.GetPropertyValue(e1);

                var e2 = new RegistryFieldSettingRegistryValueParameters(registryKey, type, pi, propertyValue);
                property.FieldAttribute.SettingRegistryValue(e2);

                property.FieldAttribute.SetRegistryValue(new RegistryFieldSetRegistryValueParameters(registryKey, type, pi, e2.Value));
            }
        }
        finally
        {
            if(registryKey != null)
            {
                registryKey.Dispose();
            }
        }
    }
}

public enum RegistryType
{
    LocalMachine,
    CurrentUser
}

public class RegistryKeyAttribute : Attribute
{
    public string Key { get; private set; }

    public RegistryKeyAttribute(string key)
    {
        if(string.IsNullOrEmpty(key))
        {
            throw new ArgumentNullException("key");
        }

        Key = key;
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
public class RegistryEntityAttribute : Attribute
{
    public string Path { get; private set; }

    public RegistryEntityAttribute(string path)
    {
        if(string.IsNullOrEmpty(path))
        {
            throw new ArgumentNullException("path");
        }

        Path = path;
    }
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class RegistryFieldAttribute : Attribute
{
    private readonly string _key;

    public RegistryFieldAttribute()
    {
    }
    public RegistryFieldAttribute(string key)
    {
        _key = key;
    }

    protected virtual string GetRegistryKey(PropertyInfo pi)
    {
        return _key ?? pi.Name;
    }

    internal protected virtual object GetRegistryValue(RegistryFieldGetRegistryValueParameters e)
    {
        var key = GetRegistryKey(e.PropertyInfo);
        var value = e.Root.GetValue(key);

        var propertyType = Nullable.GetUnderlyingType(e.PropertyInfo.PropertyType) ?? e.PropertyInfo.PropertyType;

        if(value != null && propertyType.IsEnum)
        {
            value = Enum.Parse(propertyType, value.ToString());
        }

        if(value != null && !propertyType.IsEnum)
        {
            if(value is string && propertyType != typeof(string))
            {
                //var parseMethod = propertyType.GetMethod("Parse", BindingFlags.Static);
                //if(parseMethod != null)
                //{
                //    value = parseMethod.Invoke(value)
                //}
                if(propertyType == typeof(bool))
                {
                    value = bool.Parse((string)value);
                }
                else if(propertyType == typeof(short))
                {
                    value = short.Parse((string)value);
                }
                else if(propertyType == typeof(long))
                {
                    value = long.Parse((string)value);
                }
                else if(propertyType == typeof(int))
                {
                    value = int.Parse((string)value);
                }
                else if(propertyType == typeof(double))
                {
                    value = double.Parse((string)value, CultureInfo.InvariantCulture);
                }
                else if(propertyType == typeof(decimal))
                {
                    value = decimal.Parse((string)value, CultureInfo.InvariantCulture);
                }
                else if (propertyType == typeof (DateTime))
                {
                    value = DateTime.Parse((string) value, CultureInfo.InvariantCulture);
                }
                else
                {
                    throw new NotImplementedException("PropertyType: " + propertyType.FullName);
                }
            }

            value = Convert.ChangeType(value, propertyType);
        }

        return value;
    }
    internal protected virtual void SettingRegistryValue(RegistryFieldSettingRegistryValueParameters e)
    {
    }
    internal protected virtual void SetRegistryValue(RegistryFieldSetRegistryValueParameters e)
    {
        var key = GetRegistryKey(e.PropertyInfo);

        if(e.Value != null)
        {
            e.Root.SetValue(key, ConvertValueToRegistryValue(e.Value));
        }
        else if(e.Root.GetValue(key) != null)
        {
            e.Root.DeleteValue(key);
        }
    }

    protected static object ConvertValueToRegistryValue(object value)
    {
        if(value is string)
        {
            return value;
        }

        var boolValue = value as bool?;
        if(boolValue != null)
        {
            return boolValue.Value ? 1 : 0;
        }

        return string.Format(CultureInfo.InvariantCulture, "{0}", value);
    }

    internal protected virtual object GetPropertyValue(RegistryFieldGetPropertyValueParameters e)
    {
        return e.PropertyInfo.GetValue(e.Entity, null);
    }

    internal protected virtual void SetPropertyValue(RegistryFieldSetPropertyValueParameters e)
    {
        e.PropertyInfo.SetValue(e.Entity, e.PropertyValue, null);
    }
}

public abstract class RegistryFieldRegistryValueParametersBase
{
    public RegistryKey Root { get; private set; }
    public Type Type { get; private set; }
    public PropertyInfo PropertyInfo { get; private set; }

    protected RegistryFieldRegistryValueParametersBase(RegistryKey root, Type type, PropertyInfo propertyInfo)
    {
        Root = root;
        Type = type;
        PropertyInfo = propertyInfo;
    }
}
public abstract class RegistryFieldPropertyParametersBase
{
    public Type Type { get; private set; }
    public PropertyInfo PropertyInfo { get; private set; }

    protected RegistryFieldPropertyParametersBase(Type type, PropertyInfo propertyInfo)
    {
        Type = type;
        PropertyInfo = propertyInfo;
    }
}
public abstract class RegistryFieldSetPropertyParametersBase : RegistryFieldPropertyParametersBase
{
    public bool Cancel;

    protected RegistryFieldSetPropertyParametersBase(Type type, PropertyInfo propertyInfo)
        : base(type, propertyInfo)
    {
    }
}

public class RegistryFieldGetRegistryValueParameters : RegistryFieldRegistryValueParametersBase
{
    public RegistryFieldGetRegistryValueParameters(RegistryKey root, Type type, PropertyInfo propertyInfo)
        : base(root, type, propertyInfo)
    {
    }
}
public class RegistryFieldSetRegistryValueParameters : RegistryFieldRegistryValueParametersBase
{
    public object Value { get; set; }

    public RegistryFieldSetRegistryValueParameters(RegistryKey root, Type type, PropertyInfo propertyInfo, object value)
        : base(root, type, propertyInfo)
    {
        Value = value;
    }
}
public class RegistryFieldSettingRegistryValueParameters : RegistryFieldRegistryValueParametersBase
{
    public object Value { get; set; }

    public RegistryFieldSettingRegistryValueParameters(RegistryKey root, Type type, PropertyInfo propertyInfo, object value)
        : base(root, type, propertyInfo)
    {
        Value = value;
    }
}

public class RegistryFieldGetPropertyValueParameters : RegistryFieldPropertyParametersBase
{
    public object Entity { get; private set; }

    public RegistryFieldGetPropertyValueParameters(Type type, PropertyInfo propertyInfo, object entity)
        : base(type, propertyInfo)
    {
        Entity = entity;
    }
}
public class RegistryFieldSetPropertyValueParameters : RegistryFieldPropertyParametersBase
{
    public object Entity { get; private set; }
    public object PropertyValue { get; private set; }

    public RegistryFieldSetPropertyValueParameters(Type type, PropertyInfo propertyInfo, object entity, object propertyValue)
        : base(type, propertyInfo)
    {
        Entity = entity;
        PropertyValue = propertyValue;
    }
}