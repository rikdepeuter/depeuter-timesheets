using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class CustomInvalidOperationException : InvalidOperationException
{
    public CustomInvalidOperationException(string message)
        : base(message)
    {
    }

    public CustomInvalidOperationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}