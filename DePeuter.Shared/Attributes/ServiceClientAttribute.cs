using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DePeuter.Shared.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ServiceClientAttribute : Attribute
    {
        public Type ClientType { get; private set; }

        public ServiceClientAttribute(Type clientType)
        {
            ClientType = clientType;
        }
    }
}
