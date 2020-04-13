using System.IO;
using System.Xml.Serialization;
using System;
using System.Text;
using System.Runtime.Serialization;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Xml;

namespace DePeuter.Shared
{
    [Obsolete("Use DePeuter.Shared.Serialization.XmlSerialization instead.")]
    public static class Xml
    {
        public static string SerializeToString<T>(T toSerialize)
        {
            var xmlSerializer = new XmlSerializer(toSerialize.GetType());
            var textWriter = new StringWriter();

            xmlSerializer.Serialize(textWriter, toSerialize);
            return textWriter.ToString();
        }

        public static string SerializeToString<T>(T toSerialize, IFormatProvider formatProvider)
        {
            var xmlSerializer = new XmlSerializer(toSerialize.GetType());
            var textWriter = new StringWriter(formatProvider);

            xmlSerializer.Serialize(textWriter, toSerialize);
            return textWriter.ToString();
        }

        public static string SerializeToString<T>(T toSerialize, XmlWriterSettings xmlWriterSettings)
        {
            var xmlSerializer = new XmlSerializer(toSerialize.GetType());

            using(var textWriter = new StringWriter())
            {
                using(var xmlWriter = XmlWriter.Create(textWriter, xmlWriterSettings))
                {
                    xmlSerializer.Serialize(xmlWriter, toSerialize);
                }
                return textWriter.ToString();
            }
        }

        public static T DeserializeFromString<T>(string content)
        {
            var xmlSerializer = new XmlSerializer(typeof(T));
            var sr = new StringReader(content);

            return (T)xmlSerializer.Deserialize(sr);
        }

        public static T DeserializeFromString<T>(string content, bool ignoreNamespace)
        {
            if(!ignoreNamespace) return DeserializeFromString<T>(content);

            var xmlSerializer = new XmlSerializer(typeof(T));
            var sr = new StringReader(content);
            return (T)xmlSerializer.Deserialize(new NamespaceIgnorantXmlTextReader(sr));
        }

        // helper class to ignore namespaces when de-serializing
        private class NamespaceIgnorantXmlTextReader : XmlTextReader
        {
            public NamespaceIgnorantXmlTextReader(TextReader reader) : base(reader) { }

            public override string NamespaceURI
            {
                get { return ""; }
            }
        }

        public static T Load<T>(string fileName) where T : class
        {
            return Load(fileName, typeof(T)) as T;
        }

        public static object Load(string fileName, Type type, params Type[] extraTypes)
        {
            if(type.GetCustomAttribute<DataContractAttribute>() != null)
                return XmlContractHelper.Load(fileName, type);
            else
                return XmlHelper.Load(fileName, type, extraTypes);
        }

        public static void Save(string fileName, object content, Type type, params Type[] extraTypes)
        {
            if(type.GetCustomAttribute<DataContractAttribute>() != null)
                XmlContractHelper.Save(fileName, content, type);
            else
                XmlHelper.Save(fileName, content, type, extraTypes);
        }

        public static T CreateNew<T>() where T : class
        {
            return CreateNew(typeof(T)) as T;
        }

        public static object CreateNew(Type type)
        {
            try
            {
                if(type.Equals(typeof(string))) return null;

                var res = Activator.CreateInstance(type);

                if(res is IEnumerable)
                    return res;

                var properties = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).Where(p => p.CanWrite);
                foreach(var p in properties)
                {
                    //Log.Debug("[{0}] -> {1}: {1}", type.Name, p.Name, p.PropertyType.Name);
                    p.SetValue(res, CreateNew(p.PropertyType), null);
                }
                return res;
            }
            catch(MissingMethodException)
            {
                return null;
            }
            catch(TargetParameterCountException)
            {
                return null;
            }
        }

        private static class XmlContractHelper
        {
            public static T Load<T>(string fileName) where T : class
            {
                return Load(fileName, typeof(T)) as T;
            }

            public static object Load(string fileName, Type xmlType)
            {
                if(!File.Exists(fileName))
                {
                    throw new FileNotFoundException(fileName);
                }

                try
                {
                    using(var reader = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                    {
                        var ser = new DataContractSerializer(xmlType);
                        return ser.ReadObject(reader);
                    }
                }
                catch(Exception ex)
                {
                    throw new InvalidXmlException(ex);
                }
            }

            public static void Save(string fileName, object content, Type type)
            {
                using(var writer = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    var ser = new DataContractSerializer(type);
                    ser.WriteObject(writer, content);
                }
            }
        }

        private static class XmlHelper
        {
            public static T Load<T>(string fileName) where T : class
            {
                return Load(fileName, typeof(T)) as T;
            }

            public static object Load(string fileName, Type type, params Type[] extraTypes)
            {
                if(!File.Exists(fileName))
                {
                    throw new FileNotFoundException(fileName);
                }

                try
                {
                    var deserializer = extraTypes.Any() ? new XmlSerializer(type, extraTypes) : new XmlSerializer(type);
                    using(var textReader = new StreamReader(fileName, Encoding.UTF8))
                    {
                        return deserializer.Deserialize(textReader);
                    }
                }
                catch(Exception ex)
                {
                    throw new InvalidXmlException(ex);
                }
            }

            public static void Save(string fileName, object content, Type type, params Type[] extraTypes)
            {
                var serializer = extraTypes.Any() ? new XmlSerializer(type, extraTypes) : new XmlSerializer(type);
                var textWriter = new StreamWriter(fileName, false, Encoding.UTF8);
                serializer.Serialize(textWriter, content);
                textWriter.Close();
            }
        }
    }
}

public class InvalidXmlException : Exception
{
    public InvalidXmlException()
        : base()
    {
    }

    public InvalidXmlException(string message)
        : base(message)
    {
    }

    public InvalidXmlException(Exception ex)
        : base("Xml is invalid", ex)
    {
    }

    public InvalidXmlException(string message, Exception ex)
        : base(message, ex)
    {
    }
}