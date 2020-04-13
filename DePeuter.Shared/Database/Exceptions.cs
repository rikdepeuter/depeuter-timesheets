using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class MissingProviderAttributeException : Exception
{
    public MissingProviderAttributeException(Type type)
        : base("ProviderAttribute is required on type '" + type.Name + "'")
    {
    }
}

public class NotConnectedException : Exception
{
    public NotConnectedException(string connectionstring, System.Exception ex)
        : base("There was no connection to the server available: " + connectionstring, ex)
    {
    }
}

public class MissingConnectionStringException : Exception
{
    public MissingConnectionStringException(string name)
        : base("No ConnectionString configured with the name '" + name + "'")
    {
    }
}