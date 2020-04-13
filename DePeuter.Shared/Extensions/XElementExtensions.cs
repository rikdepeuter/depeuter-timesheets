using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using DePeuter.Shared;

public static class XElementExtensions
{
    public static void OnAllElements(this XElement tv, Action<XElement> action)
    {
        foreach(XElement tn in tv.Elements())
        {
            action(tn);

            tn.OnAllElements(action);
        }
    }

    public static XElement FindElement(this XElement tv, Func<XElement, bool> predicate)
    {
        if(predicate != null && predicate(tv))
        {
            return tv;
        }

        foreach(var tn in tv.Elements())
        {
            var res = tn.FindElement(predicate);
            if(res != null)
            {
                return res;
            }
        }

        return null;
    }

    public static List<XElement> FindElements(this XElement tv, Func<XElement, bool> predicate)
    {
        var res = new List<XElement>();

        if(predicate != null && predicate(tv))
        {
            res.Add(tv);
        }

        foreach(var tn in tv.Elements())
        {
            res.AddRange(tn.FindElements(predicate));
        }

        return res;
    }

    public static List<XElement> GetAllElements(this XElement tv)
    {
        var res = new List<XElement>();

        res.Add(tv);

        foreach(var tn in tv.Elements())
        {
            res.AddRange(tn.GetAllElements());
        }

        return res;
    }

    public static List<string> GetAllNamespaceNames(this XElement element)
    {
        var elements = element.GetAllElements().ToArray();
        return (new[] { element.Name.NamespaceName }).Union(elements.Select(x => x.Name.NamespaceName)).Union(elements.SelectMany(x => x.Attributes()).Select(x => x.Name.NamespaceName)).Distinct().Where(x => x != string.Empty).ToList();
    }

    public static T Root<T>(this XDocument doc)
    {
        return ToEntity<T>(doc.Root);
    }

    public static T ToEntity<T>(this XElement element)
    {
        return (T)XElementToEntity(element, typeof(T));
    }

    public static XElement ToXElement<T>(this T entity, string elementName = null, XNamespace ns = null)
    {
        return EntityToXElement(entity, typeof(T), elementName, ns);
    }

    public static XElement ToXElement(this object entity, Type type, string elementName = null, XNamespace ns = null)
    {
        return EntityToXElement(entity, type, elementName, ns);
    }

    public static List<TProperty> GetPropertyValues<T, TProperty>(this T entity) where T : class
    {
        return GetPropertyValues(entity, typeof(T), typeof(TProperty)).Select(x => (TProperty)x).ToList();
    }

    public static List<object> GetPropertyValues<T>(this T entity, Type propertyType) where T : class
    {
        return GetPropertyValues(entity, typeof(T), propertyType);
    }

    public static void SetInnerValue(this XElement element, string name, object value)
    {
        var childElement = GetElement(element, name);
        childElement.SetValue(value);
    }

    public static XAttribute GetAttribute(this XElement element, string localName)
    {
        foreach(var e in element.Attributes())
        {
            if(e.Name.LocalName == localName)
                return e;
        }
        return null;
    }
    public static XAttribute GetAttribute(this XElement element, string ns, string localName)
    {
        return element.Attribute(XNamespace.Get(ns) + localName);
    }

    public static XElement GetElement(this XElement element, string ns, string localName)
    {
        return element.Element(XNamespace.Get(ns) + localName);
    }
    public static XElement GetElement(this XElement element, string localName)
    {
        foreach(var e in element.Elements())
        {
            if(e.Name.LocalName == localName)
                return e;
        }
        return null;
    }

    public static IEnumerable<XElement> GetElements(this XElement element, string ns, string localName)
    {
        return element.Elements(XNamespace.Get(ns) + localName);
    }
    public static IEnumerable<XElement> GetElements(this XElement element, string localName)
    {
        foreach(var e in element.Elements())
        {
            if(e.Name.LocalName == localName)
                yield return e;
        }
    }

    private static List<object> GetPropertyValues(this object entity, Type entityType, Type propertyType)
    {
        var res = new List<object>();

        if(entityType == propertyType)
        {
            res.Add(entity);
            return res;
        }

        if(entityType.IsSystemType() || entityType.IsEnum)
        {
            return res;
        }

        if(entityType.IsArray)
        {
            var arrayType = entityType.GetElementType();

            var array = (Array)entity;

            for(var i = 0; i < array.Length; i++)
            {
                var childValue = array.GetValue(i);
                if(childValue != null)
                {
                    res.AddRange(GetPropertyValues(childValue, arrayType, propertyType));
                }
            }

            return res;
        }

        if(entityType.IsListType())
        {
            var listType = entityType.GetGenericArguments()[0];

            var list = (IList)entity;

            for(var i = 0; i < list.Count; i++)
            {
                var childValue = list[i];
                if(childValue != null)
                {
                    res.AddRange(GetPropertyValues(childValue, listType, propertyType));
                }
            }

            return res;
        }

        var properties = FillTypeSelectProperties(entityType);
        foreach(var pi in properties)
        {
            var childEntity = pi.GetValue(entity, null);
            if(childEntity != null)
            {
                res.AddRange(GetPropertyValues(childEntity, pi.PropertyType, propertyType));
            }
        }

        return res;
    }

    private static XElement EntityToXElement<T>(this T entity, PropertyInfo pi, string elementName = null)
    {
        return EntityToXElement(entity, pi.PropertyType, elementName);
    }

    private static XElement EntityToXElement<T>(this T entity, Type type, string elementName = null, XNamespace ns = null)
    {
        if(elementName == null)
        {
            elementName = GetElementName(type);
        }

        var element = ns != null ? new XElement(ns + elementName) : new XElement(elementName);

        var properties = FillTypeSelectProperties(type);

        foreach(var pi in properties)
        {
            AddValueToXElement(element, type, pi, pi.GetValue(entity, null));
        }

        return element;
    }

    private static void AddValueToXElement(XElement parentElement, Type parentType, PropertyInfo pi, object value)
    {
        var elementName = GetElementName(pi);

        if(value == null)
        {
            //parentElement.Add(new XElement(elementName));
            return;
        }

        var xmlFormatAttr = pi.GetCustomAttribute<XmlFormatAttribute>();
        if(xmlFormatAttr != null && !string.IsNullOrEmpty(xmlFormatAttr.Format))
        {
            if(xmlFormatAttr.Provider != null)
            {
                value = string.Format(xmlFormatAttr.Provider, "{0:" + xmlFormatAttr.Format + "}", value);
            }
            else
            {
                value = string.Format("{0:" + xmlFormatAttr.Format + "}", value);
            }
        }

        if(pi.HasCustomAttribute<XmlAttributeAttribute>())
        {
            if(pi.PropertyType.IsSystemType() || pi.PropertyType.IsEnum)
            {
                parentElement.Add(new XAttribute(elementName, value));
            }
            else
            {
                throw new NotImplementedException(string.Format("Type '{0}' with property '{1}' has be a system type or enum when defined as a XmlAttribute", parentType.FullName, pi.Name));
            }
        }
        else if(pi.HasCustomAttribute<XmlTextAttribute>())
        {
            if(pi.PropertyType.IsSystemType() || pi.PropertyType.IsEnum)
            {
                parentElement.Value = string.Format("{0}", value);
            }
            else
            {
                throw new NotImplementedException(string.Format("Type '{0}' with property '{1}' has to be a system type or enum when defined as a XmlInnerValue", parentType.FullName, pi.Name));
            }
        }
        else // XmlElementAttribute
        {
            if(pi.PropertyType.IsSystemType() || pi.PropertyType.IsEnum)
            {
                parentElement.Add(new XElement(elementName, value));
            }
            else if(pi.PropertyType.IsArray)
            {
                var arrayType = pi.PropertyType.GetElementType();

                var array = (Array)value;

                for(var i = 0; i < array.Length; i++)
                {
                    var childValue = array.GetValue(i);
                    if(childValue != null)
                    {
                        parentElement.Add(EntityToXElement(childValue, arrayType));
                    }
                }
            }
            else if(pi.PropertyType.IsListType())
            {
                var listType = pi.PropertyType.GetGenericArguments()[0];

                var list = (IList)value;

                var listElement = new XElement(elementName);
                for(var i = 0; i < list.Count; i++)
                {
                    var childValue = list[i];
                    if(childValue != null)
                    {
                        listElement.Add(EntityToXElement(childValue, listType));
                    }
                }
                parentElement.Add(listElement);
            }
            else
            {
                parentElement.Add(EntityToXElement(value, pi, elementName));
            }
        }
    }

    private static object GetValueFromString(string value, Type type)
    {
        if(type.IsEnum)
        {
            try
            {
                return Enum.Parse(type, value);
            }
            catch(Exception ex)
            {
                throw new InvalidOperationException(string.Format("Value '{1}' is invalid for the Enum '{0}'", type.FullName, value), ex);
            }
        }

        if(type.IsValueType)
        {
            var nullableType = Nullable.GetUnderlyingType(type);
            if(nullableType != null && nullableType.IsEnum)
            {
                try
                {
                    return Enum.Parse(nullableType, value);
                }
                catch(Exception ex)
                {
                    throw new InvalidOperationException(string.Format("Value '{1}' is invalid for the Enum '{0}'", nullableType.FullName, value), ex);
                }
            }
        }

        if(type == typeof(string))
        {
            return value;
        }

        if(string.IsNullOrEmpty(value))
        {
            return Activator.CreateInstance(type);
        }

        if(type == typeof(bool) || type == typeof(bool?))
        {
            return bool.Parse(value);
        }
        if(type == typeof(short) || type == typeof(short?))
        {
            return short.Parse(value);
        }
        if(type == typeof(int) || type == typeof(int?))
        {
            return int.Parse(value);
        }
        if(type == typeof(long) || type == typeof(long?))
        {
            return long.Parse(value);
        }
        if(type == typeof(double) || type == typeof(double?))
        {
            return double.Parse(value, CultureInfo.InvariantCulture);
        }
        if(type == typeof(DateTime) || type == typeof(DateTime?))
        {
            return DateTime.Parse(value, CultureInfo.InvariantCulture);
        }

        throw new NotImplementedException(string.Format("Conversion from String to {0} is unhandled with value: {1}", type.FullName, value));
    }

    private static string GetElementName(this Type type)
    {
        var attribute = type.GetCustomAttribute<XmlTypeAttribute>();
        if(attribute != null && !string.IsNullOrEmpty(attribute.TypeName))
        {
            return attribute.TypeName;
        }

        return type.Name;
    }

    private static string GetElementName(this PropertyInfo pi)
    {
        var attribute = pi.GetCustomAttribute<XmlElementAttribute>();
        if(attribute != null && !string.IsNullOrEmpty(attribute.ElementName))
        {
            return attribute.ElementName;
        }

        return pi.Name;
    }

    private static object XElementToEntity(this XElement element, PropertyInfo pi)
    {
        return XElementToEntity(element, pi.PropertyType);
    }

    private static object XElementToEntity(this XElement element, Type type)
    {
        if(type.IsArray)
        {
            //get T type of T[]
            type = type.GetElementType();
        }

        if(type.IsListType())
        {
            //get T type of List<T>
            type = type.GetGenericArguments()[0];
        }

        if(type.IsSystemType() || type.IsEnum)
        {
            return GetValueFromString(element.Value, type);
        }

        var entity = Activator.CreateInstance(type);

        var properties = FillTypeSelectProperties(type);

        //attributes
        foreach(var child in element.Attributes())
        {
            var pi = properties.SingleOrDefault(x => GetElementName(x) == child.Name.LocalName && x.HasCustomAttribute<XmlAttributeAttribute>());

            if(pi == null)
            {
                continue;
                //throw new NotImplementedException(string.Format("Type '{0}' has no property with XmlAttributeAttribute(\"{1}\") attribute", type.FullName, child.Name.LocalName));
            }

            if(pi.PropertyType.IsSystemType() || pi.PropertyType.IsEnum)
            {
                pi.SetValue(entity, GetValueFromString(child.Value, pi.PropertyType), null);
            }
            else
            {
                throw new InvalidOperationException(string.Format("Type '{0}' his property '{1}' has to be a system type or an enum when defined as a XmlAttribute", type.FullName, pi.Name));
            }
        }

        //innerValue
        var piInnerValue = properties.SingleOrDefault(x => x.HasCustomAttribute<XmlTextAttribute>());
        if(piInnerValue != null)
        {
            piInnerValue.SetValue(entity, GetValueFromString(element.Value, piInnerValue.PropertyType), null);
            return entity;
        }

        //elements
        //var xmlIndex = -1;
        foreach(var child in element.Elements())
        {
            //xmlIndex++;

            var pi = properties.SingleOrDefault(x => GetElementName(x) == child.Name.LocalName && !x.HasCustomAttribute<XmlAttributeAttribute>());

            if(pi == null)
            {
                continue;
                //throw new NotImplementedException(string.Format("Type '{0}' has no property with name '{1}' or a property with XmlElementAttribute(\"{1}\") attribute", type.FullName, child.Name.LocalName));
            }

            if(pi.PropertyType.IsSystemType() || pi.PropertyType.IsEnum)
            {
                pi.SetValue(entity, XElementToEntity(child, pi), null);
            }
            else if(pi.PropertyType.IsArray)
            {
                var array = (Array)pi.GetValue(entity, null);
                if(array == null)
                {
                    var childElementName = pi.PropertyType.GetElementType().GetElementName();
                    var arrayElementsCount = element.Elements().Count(x => x.Name.LocalName == childElementName);

                    array = (Array)Activator.CreateInstance(pi.PropertyType, arrayElementsCount);
                    pi.SetValue(entity, array, null);
                }

                for(var i = 0; i < array.Length; i++)
                {
                    if(array.GetValue(i) == null)
                    {
                        var childEntity = XElementToEntity(child, pi);
                        array.SetValue(childEntity, i);
                        break;
                    }
                }
            }
            else if(pi.PropertyType.IsListType())
            {
                var list = (IList)pi.GetValue(entity, null);
                if(list == null)
                {
                    list = (IList)Activator.CreateInstance(pi.PropertyType);
                    pi.SetValue(entity, list, null);
                }

                var childElementName = pi.PropertyType.GetGenericArguments()[0].GetElementName();
                var listElements = child.Elements().Where(x => x.Name.LocalName == childElementName);
                foreach(var listElement in listElements)
                {
                    var childEntity = XElementToEntity(listElement, pi);
                    list.Add(childEntity);
                }
            }
            else
            {
                var childEntity = XElementToEntity(child, pi);
                pi.SetValue(entity, childEntity, null);
            }
        }

        return entity;
    }

    //private static void SetXmlIndex(object item, int xmlIndex)
    //{
    //    if(item is IXmlIndex)
    //    {
    //        ((IXmlIndex)item).XmlIndex = xmlIndex;
    //    }
    //}

    private static readonly Dictionary<Type, PropertyInfo[]> TypeSelectProperties = new Dictionary<Type, PropertyInfo[]>();
    private static PropertyInfo[] FillTypeSelectProperties(Type type)
    {
        if(!TypeSelectProperties.ContainsKey(type))
        {
            TypeSelectProperties.Add(type, null);

            var properties = new Dictionary<PropertyInfo, int>();

            foreach(var pi in type.GetProperties().Where(pi => pi.CanWrite && pi.GetSetMethod(true).IsPublic))
            {
                if (pi.HasCustomAttribute<XmlIgnoreAttribute>())
                {
                    continue;
                }

                var attrs = pi.GetCustomAttributes<XmlElementAttribute>(true);

                var order = -1;
                if(attrs.Any())
                {
                    order = attrs.Min(x => x.Order);
                }

                properties.Add(pi, order);

                if(!pi.PropertyType.IsSystemType())
                {
                    FillTypeSelectProperties(pi.PropertyType);
                }
            }

            TypeSelectProperties.Set(type, properties.OrderBy(x => x.Value).Select(x => x.Key).ToArray());
        }

        return TypeSelectProperties[type];
    }
}