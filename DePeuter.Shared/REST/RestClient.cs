using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using DePeuter.Shared.Extensions;
using DePeuter.Shared.Serialization;
using log4net;
using Newtonsoft.Json;

namespace DePeuter.Shared.REST
{
    public class RestClient
    {
        protected readonly ILog Logger;
        protected string BaseUrl { get; private set; }

        private readonly Dictionary<RequestMethod, int> _defaultTimeouts;
        private readonly Dictionary<Type, ParseValueHandler> _typeHandlers;

        public delegate object ParseValueHandler(RestClient client, ContentType contentType, string value);

        private static readonly Dictionary<Type, ParseValueHandler> DefaultTypeHandlers = new Dictionary<Type, ParseValueHandler>()
            {
                //{typeof(string), value => value},
                //{typeof(bool), (value, contentType) => bool.Parse(value)},
                //{typeof(bool?), (value, contentType) => Convert2.ToNullableBoolean(value)},
                //{typeof(int), (value, contentType) => int.Parse(value)},
                //{typeof(int?), (value, contentType) => Convert2.ToNullableInteger(value)},
                //{typeof(double), (value, contentType) => double.Parse(value, CultureInfo.InvariantCulture)},
                //{typeof(double?), (value, contentType) => Convert2.ToNullableDouble(value, CultureInfo.InvariantCulture)},
                //{typeof(decimal), (value, contentType) => decimal.Parse(value, CultureInfo.InvariantCulture)},
                //{typeof(decimal?), (value, contentType) => Convert2.ToNullableDecimal(value, CultureInfo.InvariantCulture)},
                //{typeof(DateTime), (value, contentType) => DateTime.Parse(value, CultureInfo.InvariantCulture)},
                //{typeof(DateTime?), (value, contentType) => Convert2.ToNullableDateTime(value, CultureInfo.InvariantCulture)},
                {typeof(XmlDocument), (client, contentType, value) => JsonConvert.DeserializeXmlNode(value)},
                {typeof(XDocument), (client, contentType, value) => JsonConvert.DeserializeXNode(value)
                }
            };

        public RestClient(string baseUrl)
        {
            if(baseUrl == null)
            {
                throw new ArgumentNullException("baseUrl");
            }

            BaseUrl = baseUrl.TrimEnd('/');

            Logger = LogManager.GetLogger(GetType());

            _defaultTimeouts = new Dictionary<RequestMethod, int>()
                {
                    {RequestMethod.Get, 60000},
                    {RequestMethod.Post, 60000}
                };

            _typeHandlers = DefaultTypeHandlers.ToDictionary(x => x.Key, x => x.Value);
        }

        private WebResponse GetResponse(RestRequest restRequest)
        {
            var url = new StringBuilder();
            url.AppendFormat("{0}/{1}", BaseUrl, restRequest.Url.TrimStart('/'));

            switch(restRequest.Method)
            {
                case RequestMethod.Get:
                    {
                        var parametersQueryString = restRequest.GetParametersQueryString();

                        if(parametersQueryString != null)
                        {
                            url.Append(url.ToString().Contains("?") ? "&" : "?");
                            url.Append(parametersQueryString);
                        }
                        break;
                    }
            }

            var requestCreatingParameters = new RequestCreatingParameters { Url = url.ToString() };
            RequestCreating(requestCreatingParameters);

            Logger.DebugFormat("URL: {0}", requestCreatingParameters.Url);

            var webRequest = (HttpWebRequest)WebRequest.Create(requestCreatingParameters.Url);
            webRequest.Timeout = restRequest.Timeout ?? _defaultTimeouts[restRequest.Method];
            webRequest.Method = restRequest.Method.ToString();
            webRequest.CookieContainer = new CookieContainer();
            webRequest.Credentials = CredentialCache.DefaultCredentials;

            switch(restRequest.Method)
            {
                case RequestMethod.Post:
                    {
                        restRequest.WriteContent(webRequest);
                        break;
                    }
            }

            if(restRequest.Accept != null)
            {
                webRequest.Accept = restRequest.Accept;
            }

            if(restRequest.ContentType != null)
            {
                webRequest.ContentType = restRequest.ContentType;
            }

            var requestSendingParameters = new RequestSendingParameters { Request = webRequest };
            RequestSending(requestSendingParameters);

            var response = webRequest.GetResponse();

            var responseReceivedParameters = new ResponseReceivedParameters { Response = response };
            ResponseReceived(responseReceivedParameters);

            return response;
        }

        protected class RequestCreatingParameters
        {
            public string Url;

            internal RequestCreatingParameters()
            {
            }
        }
        protected virtual void RequestCreating(RequestCreatingParameters e)
        {
        }

        protected class RequestSendingParameters
        {
            public HttpWebRequest Request;

            internal RequestSendingParameters()
            {
            }
        }
        protected virtual void RequestSending(RequestSendingParameters e)
        {
        }

        protected class ResponseReceivedParameters
        {
            public WebResponse Response;

            internal ResponseReceivedParameters()
            {
            }
        }
        protected virtual void ResponseReceived(ResponseReceivedParameters e)
        {
        }

        public void SetTimeout(RequestMethod method, int timeout)
        {
            _defaultTimeouts[method] = timeout;
        }

        public void SetTypeHandler<T>(ParseValueHandler handler)
        {
            _typeHandlers.Set(typeof(T), handler);
        }
        public void SetTypeHandler(Type type, ParseValueHandler handler)
        {
            _typeHandlers.Set(type, handler);
        }

        private ParseValueHandler FindTypeHandler(Type type)
        {
            if(_typeHandlers.ContainsKey(type))
            {
                return _typeHandlers[type];
            }

            foreach(var t in _typeHandlers.Keys)
            {
                if(t.IsAssignableFrom(type))
                {
                    return _typeHandlers[t];
                }
            }

            return null;
        }

        internal object Parse(ContentType contentType, Type type, string value)
        {
            var handler = FindTypeHandler(type);

            if(handler != null)
            {
                return handler(this, contentType, value);
            }

            switch(contentType)
            {
                case ContentType.Xml:
                    return XmlSerialization.DeserializeFromString(type, value);
                case ContentType.Json:
                    return JsonConvert.DeserializeObject(value, type);
            }

            throw new NotImplementedException("ContentType." + contentType);
        }

        public RestResponse Execute(RestRequest request)
        {
            try
            {
                var response = GetResponse(request);

                return new RestResponse(this, request, response);
            }
            catch(Exception ex)
            {
                return HandleException(request, ex);
            }
        }

        public T Execute<T>(RestRequest request)
        {
            return Execute(request).Get<T>();
        }

        protected virtual RestResponse HandleException(RestRequest request, Exception ex)
        {
            var webEx = ex as WebException;
            if(webEx == null)
            {
                throw new RestException(request, ex);
            }

            var response = webEx.Response as HttpWebResponse;
            if(response == null)
            {
                throw new RestException(request, ex);
            }

            var res = response.GetResponseStream().ReadString();
            if(string.IsNullOrEmpty(res))
            {
                throw new RestException(request, ex);
            }

            throw new RestException(request, ex, res);
        }
    }
}