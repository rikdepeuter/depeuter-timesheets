using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DePeuter.Shared
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class XmlFormatAttribute : Attribute
    {
        public string Format { get; private set; }
        public IFormatProvider Provider { get; private set; }

        public XmlFormatAttribute(string format)
        {
            Format = format;
        }

        public XmlFormatAttribute(string format, IFormatProvider provider)
        {
            Format = format;
            Provider = provider;
        }
    }
}
