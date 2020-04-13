using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using DePeuter.Shared.Serialization;
using Newtonsoft.Json;

namespace DePeuter.Shared.REST
{
    public class RestRequest
    {
        public RequestMethod Method { get; private set; }
        public string Url { get; private set; }

        public string ContentType { get; set; }
        public string Accept { get; set; }
        public int? Timeout { get; set; }
        public object Content { get; set; }

        private readonly ContentType _dataType;
        internal ContentType DataType { get { return _dataType; } }

        private readonly Dictionary<string, object> _parameters;

        public RestRequest(RequestMethod method, string url, ContentType? dataType = null, object content = null)
        {
            if(url == null)
            {
                throw new ArgumentNullException("url");
            }

            Method = method;
            Url = url;
            _dataType = dataType ?? REST.ContentType.Json;
            Accept = _dataType.GetAttribute<ContentTypeAttribute>().Value;
            Content = content;

            _parameters = new Dictionary<string, object>();
        }

        public RestRequest AddParameter(string key, object value)
        {
            if(Method != RequestMethod.Get)
            {
                throw new NotSupportedException("RequestMethod." + Method);
            }

            if(_parameters.ContainsKey(key))
            {
                throw new RestException(this, string.Format("Parameter key '{0}' already exists.", key));
            }

            _parameters.Add(key, value);
            return this;
        }

        internal string GetParametersQueryString()
        {
            if(!_parameters.Any())
            {
                return null;
            }

            var url = new StringBuilder();

            foreach(var x in _parameters)
            {
                url.AppendFormat(CultureInfo.InvariantCulture, "&{0}={1}", x.Key, x.Value);
            }

            return url.ToString().Substring(1);
        }

        public virtual void WriteContent(HttpWebRequest request)
        {
            var content = Content;

            if(content != null)
            {
                if(content is string)
                {
                    var requestWriter = new StreamWriter(request.GetRequestStream());
                    requestWriter.Write(content);
                    requestWriter.Close();
                }
                else if(content is byte[])
                {
                    var data = (byte[])content;

                    request.ContentLength = data.Length;

                    using(var requestStream = request.GetRequestStream())
                    {
                        requestStream.Write(data, 0, data.Length);
                        requestStream.Close();
                    }
                }
                else
                {
                    string serializedContent;
                    if(_dataType == REST.ContentType.Xml)
                    {
                        serializedContent = XmlSerialization.SerializeToString(content);
                        if (ContentType == null)
                        {
                            ContentType = REST.ContentType.Xml.GetAttribute<ContentTypeAttribute>().Value;
                        }
                    }
                    else
                    {
                        serializedContent = JsonConvert.SerializeObject(content);
                        if(ContentType == null)
                        {
                            ContentType = REST.ContentType.Json.GetAttribute<ContentTypeAttribute>().Value;
                        }
                    }

                    var requestWriter = new StreamWriter(request.GetRequestStream());
                    requestWriter.Write(serializedContent);
                    requestWriter.Close();
                }
            }
        }
    }
}