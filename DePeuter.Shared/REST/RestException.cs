using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace DePeuter.Shared.REST
{
    public class RestException : WebException
    {
        public RestRequest Request { get; private set; }

        public string ResponseString { get; private set; }

        internal RestException(RestRequest request, Exception innerException, string responseString = null)
            : base(innerException.Message, innerException)
        {
            Request = request;
            ResponseString = responseString;
        }

        internal RestException(RestRequest request, string message)
            : base(message)
        {
            Request = request;
        }
    }
}
