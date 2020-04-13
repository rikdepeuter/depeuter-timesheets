using System.IO;
using System.Xml.Serialization;
using System;
using System.Text;
using System.Runtime.Serialization;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Xml;

namespace DePeuter.Shared.Serialization
{
    public static class XmlSerialization
    {
        private static IXmlSerializer GetSerializer(Type type)
        {
            if(type.HasCustomAttribute<DataContractAttribute>())
                return new XmlContractHelper();
            return new XmlHelper();
        }

        public static string SerializeToString<T>(T objectToSerialize)
        {
            return GetSerializer(typeof(T)).SerializeToString(objectToSerialize);
        }
        public static string SerializeToString<T>(T objectToSerialize, IFormatProvider formatProvider)
        {
            return GetSerializer(typeof(T)).SerializeToString(objectToSerialize, formatProvider: formatProvider);
        }
        public static string SerializeToString<T>(T objectToSerialize, XmlWriterSettings xmlWriterSettings)
        {
            return GetSerializer(typeof(T)).SerializeToString(objectToSerialize, xmlWriterSettings: xmlWriterSettings);
        }

        public static T DeserializeFromString<T>(string content, bool ignoreNamespace = false)
        {
            return (T)DeserializeFromString(typeof(T), content, ignoreNamespace);
        }
        public static object DeserializeFromString(Type type, string content, bool ignoreNamespace = false)
        {
            return GetSerializer(type).DeserializeFromString(type, content, ignoreNamespace);
        }

        public static T Deserialize<T>(string fileName, params Type[] extraTypes)
        {
            return (T)Deserialize(fileName, typeof(T), extraTypes);
        }
        public static object Deserialize(string fileName, Type type, params Type[] extraTypes)
        {
            return GetSerializer(type).Deserialize(fileName, type, extraTypes);
        }

        public static void Serialize<T>(string fileName, T content, params Type[] extraTypes)
        {
            Serialize(fileName, content, typeof(T), extraTypes);
        }
        public static void Serialize(string fileName, object content, Type type, params Type[] extraTypes)
        {
            GetSerializer(type).Serialize(fileName, content, type, extraTypes);
        }

        public static T CreateNew<T>()
        {
            return (T)CreateNew(typeof(T));
        }

        public static object CreateNew(Type type)
        {
            try
            {
                if(type == typeof(string)) return null;

                var res = Activator.CreateInstance(type);

                if(res is IEnumerable)
                    return res;

                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanWrite);
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
    }

    internal interface IXmlSerializer
    {
        object Deserialize(string fileName, Type type, params Type[] extraTypes);

        object DeserializeFromString(Type type, string content, bool ignoreNamespace = false);

        void Serialize(string fileName, object content, Type type, params Type[] extraTypes);

        string SerializeToString<T>(T objectToSerialize, IFormatProvider formatProvider = null, XmlWriterSettings xmlWriterSettings = null);
    }

    internal class XmlContractHelper : IXmlSerializer
    {
        public object Deserialize(string fileName, Type xmlType, params Type[] extraTypes)
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

        public object DeserializeFromString(Type type, string content, bool ignoreNamespace = false)
        {
            var serializer = new DataContractSerializer(type);
            using(var sr = new StringReader(content))
            using(var xml = new XmlTextReader(sr))
            {
                return serializer.ReadObject(xml);
            }
        }

        public void Serialize(string fileName, object content, Type type, params Type[] extraTypes)
        {
            using(var writer = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                var ser = new DataContractSerializer(type);
                ser.WriteObject(writer, content);
            }
        }

        public string SerializeToString<T>(T objectToSerialize, IFormatProvider formatProvider = null, XmlWriterSettings xmlWriterSettings = null)
        {
            using(var stream = new MemoryStream())
            {
                var serializer = new DataContractSerializer(typeof(T));
                serializer.WriteObject(stream, objectToSerialize);

                stream.Seek(0, SeekOrigin.Begin);

                using(var streamReader = new StreamReader(stream))
                {
                    return streamReader.ReadToEnd();
                }
            }

            //var serializer = new DataContractSerializer(typeof(T));
            //using(var sr = new StringWriter())
            //using(var xml = XmlWriter.Create(sr))
            //{
            //    serializer.WriteObject(xml, objectToSerialize);

            //    sr.Flush();
            //    return sr.ToString();
            //}
        }
    }

    internal class XmlHelper : IXmlSerializer
    {
        public object Deserialize(string fileName, Type type, params Type[] extraTypes)
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

        public object DeserializeFromString(Type type, string content, bool ignoreNamespace)
        {
            var xmlSerializer = new XmlSerializer(type);
            using(var sr = new StringReader(content))
            {
                if(ignoreNamespace)
                {
                    return xmlSerializer.Deserialize(new NamespaceIgnorantXmlTextReader(sr));
                }

                return xmlSerializer.Deserialize(sr);
            }
        }

        public void Serialize(string fileName, object content, Type type, params Type[] extraTypes)
        {
            var serializer = extraTypes.Any() ? new XmlSerializer(type, extraTypes) : new XmlSerializer(type);
            using(var textWriter = new StreamWriter(fileName, false, Encoding.UTF8))
            {
                serializer.Serialize(textWriter, content);
                textWriter.Close();
            }
        }

        public string SerializeToString<T>(T objectToSerialize, IFormatProvider formatProvider = null, XmlWriterSettings xmlWriterSettings = null)
        {
            var xmlSerializer = new XmlSerializer(objectToSerialize.GetType());

            using(var textWriter = new StringWriter())
            {
                using(var xmlWriter = XmlWriter.Create(textWriter, xmlWriterSettings))
                {
                    xmlSerializer.Serialize(xmlWriter, objectToSerialize);
                }
                return textWriter.ToString();
            }
        }

        // helper class to ignore namespaces when de-serializing
        private class NamespaceIgnorantXmlTextReader : XmlTextReader
        {
            public NamespaceIgnorantXmlTextReader(TextReader reader)
                : base(reader)
            {
            }

            public override string NamespaceURI
            {
                get { return ""; }
            }
        }
    }
}