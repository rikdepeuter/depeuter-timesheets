using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class ValidationException : Exception
{
    public ValidationException(string format, params object[] args)
        : base(args != null && args.Length > 0 ? string.Format(format, args) : format)
    {
    }

    public ValidationException(Exception ex, string format, params object[] args)
        : base(args != null && args.Length > 0 ? string.Format(format, args) : format, ex)
    {
    }
}