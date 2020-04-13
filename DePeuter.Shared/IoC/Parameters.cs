using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class ProviderParameter
{
    public string Value { get; set; }

    public ProviderParameter(string value)
    {
        Value = value;
    }
}

public class ConnectionStringParameter
{
    public string Value { get; set; }

    public ConnectionStringParameter(string value)
    {
        Value = value;
    }
}

public class NiscodeParameter
{
    public string Value { get; set; }

    public NiscodeParameter(string value)
    {
        Value = value;
    }
}