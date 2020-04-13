using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

public static class ObjectExtensions
{
    //private const string SPACE_PREFIX = "    ";

    //public static string ToCustomXML(this object item)
    //{
    //    return ToCustomXML(item, 0);
    //}

    //private static readonly string[] SkipExceptionFields = new[] { "TargetSite" };

    //private static string ToCustomXML(this object item, int level)
    //{
    //    if (item == null) return null;

    //    var type = item.GetType();

    //    if (type.IsSystemType())
    //    {
    //        return string.Format("{0}{1}\n", (level).Repeat(SPACE_PREFIX), item);
    //    }
    //    else
    //    {
    //        if (item is IDictionary)
    //        {
    //            var dict = (IDictionary)item;
    //            var res = string.Empty;
    //            foreach (var key in dict.Keys)
    //            {
    //                res += string.Format("{0}<Key>{1}</Key>\n", (level).Repeat(SPACE_PREFIX), key);
    //                res += string.Format("{0}<Value>{1}</Value>\n", (level).Repeat(SPACE_PREFIX), ToCustomXML(dict[key], level + 1));
    //            }
    //            return res;
    //        }
    //        else if (item is IEnumerable)
    //        {
    //            var collection = (IEnumerable)item;
    //            var res = string.Empty;
    //            foreach (var x in collection)
    //            {
    //                if (x == null) continue;

    //                var xml = ToCustomXML(x, level + 1);
    //                if (string.IsNullOrEmpty(xml))
    //                {
    //                    res += string.Format("{0}<{1} />\n", (level).Repeat(SPACE_PREFIX), x.GetType().GetNameWithGenerics());
    //                }
    //                else
    //                {
    //                    res += string.Format("{0}<{2}>{1}\n", (level).Repeat(SPACE_PREFIX), xml, x.GetType().GetNameWithGenerics());
    //                    res += string.Format("{0}</{1}>\n", (level).Repeat(SPACE_PREFIX), x.GetType().GetNameWithGenerics());
    //                }
    //            }
    //            return res;
    //        }
    //    }

    //    var sb = new StringBuilder();

    //    sb.AppendFormat("{0}<{1}>\n", level.Repeat(SPACE_PREFIX), type.GetNameWithGenerics());

    //    var properties = type.GetProperties().Where(x => x.CanRead).ToArray();
    //    foreach (var p in properties)
    //    {
    //        if (item is Exception)
    //        {
    //            if (SkipExceptionFields.Contains(p.Name))
    //                continue;
    //        }

    //        try
    //        {
    //            if (item != null)
    //            {
    //                var value = p.GetValue(item, null);
    //                var xml = ToCustomXML(value, level + 2);

    //                if (xml != null)
    //                {
    //                    if (xml == string.Empty)
    //                    {
    //                        sb.AppendFormat("{0}<{1} propertyType=\"{2}\" />\n", (level + 1).Repeat(SPACE_PREFIX), p.Name, p.PropertyType.GetNameWithGenerics());
    //                    }
    //                    else
    //                    {
    //                        sb.AppendFormat("{0}<{1} propertyType=\"{2}\">\n", (level + 1).Repeat(SPACE_PREFIX), p.Name, p.PropertyType.GetNameWithGenerics());
    //                        sb.AppendFormat("{0}", xml);
    //                        sb.AppendFormat("{0}</{1}>\n", (level + 1).Repeat(SPACE_PREFIX), p.Name);
    //                    }
    //                }

    //            }
    //        }
    //        catch (Exception) { }
    //    }

    //    sb.AppendFormat("{0}</{1}>\n", level.Repeat(SPACE_PREFIX), type.GetNameWithGenerics());

    //    return sb.ToString().TrimEnd('\n');
    //}

    //public static T As<T>(this object value) where T : class
    //{
    //    return value as T;
    //}

    //public static bool Is<T>(this object value) where T : class
    //{
    //    return value is T;
    //}

    //public static string SerializeAsXml<T>(this T value)
    //{
    //    var xmlserializer = new XmlSerializer(typeof (T));
    //    using (var stringWriter = new StringWriter())
    //    {
    //        using (var writer = XmlWriter.Create(stringWriter))
    //        {
    //            xmlserializer.Serialize(writer, value);
    //            return stringWriter.ToString();
    //        }
    //    }
    //}

    public static T CastTo<T>(this object value)
    {
        if(value == null || value is DBNull) return default(T);

        try
        {
            if (value is string)
            {
                var type = typeof (T);
                type = Nullable.GetUnderlyingType(type) ?? type;

                if (type.IsEnum)
                {
                    if (string.IsNullOrEmpty(value.ToString()))
                    {
                        return default(T);
                    }

                    return (T)Enum.Parse(typeof(T), value.ToString());    
                }
            }

            return (T)value;
        }
        catch(Exception ex)
        {
            throw new InvalidCastException(string.Format("Failed to cast value '{0}' of type '{1}' to type '{2}'", value, value.GetType().GetNameWithGenerics(), typeof(T).GetNameWithGenerics()), ex);
        }
    }

    public static void SetPropertyValue(this object entity, Type type, string property, object value, object[] index = null)
    {
        var propertyInfo = type.GetProperty(property);
        if(propertyInfo == null)
        {
            throw new UnknownPropertyException(type, property);
        }

        try
        {
            propertyInfo.SetValue(entity, value, index);
        }
        catch(Exception ex)
        {
            throw new PropertySetFailedException(type, property, value, index, ex);
        }
    }

    public static object GetPropertyValue(this object entity, Type type, string property, object[] index = null)
    {
        return GetPropertyValue<object>(entity, type, property, index);
    }

    public static T GetPropertyValue<T>(this object entity, Type type, string property, object[] index = null)
    {
        var propertyInfo = type.GetProperty(property);
        if(propertyInfo == null)
        {
            throw new UnknownPropertyException(type, property);
        }

        try
        {
            return propertyInfo.GetValue(entity, index).CastTo<T>();
        }
        catch(Exception ex)
        {
            throw new PropertyGetFailedException(type, property, index, ex);
        }
    }

    internal static IDictionary<string, object> AsLowerDictionary(this object source, bool includeNulls = false, Func<PropertyInfo, bool> filter = null)
    {
        if (source is IDictionary<string, object>)
        {
            return ((IDictionary<string, object>)source).ToDictionary(x => x.Key.ToLower(), x => x.Value);
        }

        return source.GetType().GetProperties()
            .Where(x => filter == null || filter(x))
            .Select(x => new
            {
                Key = x.Name,
                Value = x.GetValue(source, null)
            })
            .Where(x => includeNulls || x.Value != null)
            .ToDictionary(x => x.Key.ToLower(), x => x.Value);
    }
}