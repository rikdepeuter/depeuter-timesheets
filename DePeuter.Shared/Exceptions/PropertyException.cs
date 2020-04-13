using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class UnknownPropertyException: Exception
{
    public UnknownPropertyException(Type type, string property)
        : base(string.Format("Type '{0}' does not have the property '{1}'", type.FullName, property))
    {
    }
}

public class PropertySetFailedException : Exception
{
    public PropertySetFailedException(Type type, string property, object value, object[] index, Exception ex)
        : base(string.Format("Failed to set value '{2}' on property '{1}' of type '{0}' with index '{3}'", type.FullName, property, value, index), ex)
    {
    }
}

public class PropertyGetFailedException : Exception
{
    public PropertyGetFailedException(Type type, string property, object[] index, Exception ex)
        : base(string.Format("Failed to get property '{1}' of type '{0}' with index '{2}'", type.FullName, property, index), ex)
    {
    }
}