using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DePeuter.Shared.REST
{
    public enum RequestMethod
    {
        Get,
        Post,
        Put,
        Connect,
        Head,
        MkCol
    }

    public enum ContentType
    {
        [ContentType("application/json")]
        Json,
        [ContentType("application/xml")]
        Xml
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    internal class ContentTypeAttribute : Attribute
    {
        public string Value { get; private set; }

        internal ContentTypeAttribute(string value)
        {
            Value = value;
        }
    }
}
