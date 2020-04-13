using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using DePeuter.Shared.Extensions;
using Newtonsoft.Json;

namespace DePeuter.Shared.REST
{
    public class RestResponse
    {
        public RestRequest Request { get; private set; }

        private readonly RestClient _client;
        private readonly Stream _stream;
        private readonly ContentType _contentType;

        internal RestResponse(RestClient client, RestRequest request, WebResponse response)
        {
            _client = client;
            Request = request;
            
            var stream = response.GetResponseStream();
            if(stream == null)
            {
                throw new RestException(request, new NullReferenceException("ResponseStream"));
            }

            _stream = stream;
            _contentType = GetContentType(response.ContentType);
        }

        private ContentType GetContentType(string contentType)
        {
            if(contentType == null)
            {
                throw new ArgumentNullException("contentType");
            }

            var enumNames = Enum.GetNames(typeof(ContentType));
            foreach(var enumName in enumNames)
            {
                var value = enumName.ToEnum<ContentType>();
                var valueContentType = value.GetAttribute<ContentTypeAttribute>().Value;

                if(contentType.ToLower().StartsWith(valueContentType.ToLower()))
                {
                    return value;
                }
            }

            throw new NotSupportedException("ContentType: " + contentType);
        }

        private string GetString()
        {
            return _stream.ReadString(Encoding.UTF8);
        }
        //public string GetString(Encoding encoding)
        //{
        //    return _stream.ReadString(encoding);
        //}
        public byte[] GetBytes()
        {
            return _stream.ReadBytes();
        }

        public T Get<T>()
        {
            return (T)Get(typeof(T));
        }
        public object Get(Type type)
        {
            var value = GetString();

            //if(_contentType == ContentType.Xml)
            //{
            //    var root = XDocument.Parse(value).Root;
            //    if(root == null)
            //    {
            //        return null;
            //    }

            //    var xml = root.Value;
            //    if(string.IsNullOrEmpty(xml))
            //    {
            //        return null;
            //    }

            //    value = xml;
            //}
            //else if(_contentType == ContentType.Json)
            //{
            //    try
            //    {
            //        value = JsonConvert.DeserializeObject<string>(value);
            //    }
            //    catch(Exception ex)
            //    {
            //        //throw new Exception(string.Format("Failed to deserialize value to string: {0}", value), ex);
            //    }
            //}

            return _client.Parse(Request.DataType, type, value);
        }
    }
}