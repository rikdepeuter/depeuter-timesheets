using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class MissingSqlTableAttributeException : Exception
{
    public MissingSqlTableAttributeException(Type type)
        : base(string.Format("Missing SqlTableAttribute on entity '{0}'", type.FullName))
    {
    }
}

public class MissingSqlPrimaryKeyAttributeException : Exception
{
    public MissingSqlPrimaryKeyAttributeException(Type type)
        : base(string.Format("Missing MissingSqlPrimaryKeyAttributeException on entity '{0}'", type.FullName))
    {
    }
}