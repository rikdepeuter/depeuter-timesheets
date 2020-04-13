using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class MissingAttributeException : Exception
{
    private readonly Type _wantedType;
    private readonly Type _attributeType;

    public Type WantedType { get { return _wantedType; } }
    public Type AttributeType { get { return _attributeType; } }

    public MissingAttributeException(Type attributeType)
        : base(string.Format("Missing attribute '{0}'", attributeType.FullName))
    {
        _attributeType = attributeType;
    }

    public MissingAttributeException(Type wantedType, Type attributeType)
        : base(string.Format("Type '{0}' (or an implementation class) has no '{1}' attached", wantedType.FullName, attributeType.FullName))
    {
        _wantedType = wantedType;
        _attributeType = attributeType;
    }
}

public class MissingServiceAttributeException : Exception
{
    private readonly Type _wantedType;

    public Type WantedType { get { return _wantedType; } }

    public MissingServiceAttributeException(Type wantedType)
        : base(string.Format("Type '{0}' has no ServiceAttribute attached", wantedType.FullName))
    {
        _wantedType = wantedType;
    }
}

public class MissingTypesException : IoCException
{
    private readonly Type _wantedType;

    public Type WantedType { get { return _wantedType; } }

    public MissingTypesException(Type wantedType)
        : base(string.Format("No implementation found for type '{0}'", wantedType.FullName))
    {
        _wantedType = wantedType;
    }
}

public class MissingPublicConstructorException : IoCException
{
    private readonly Type _type;

    public Type Type { get { return _type; } }

    public MissingPublicConstructorException(Type type)
        : base(string.Format("Type '{0}' has no public constructors defined", type.FullName))
    {
        _type = type;
    }
}

public class MissingParameterException : IoCException
{
    private readonly Type _implementationType;
    private readonly Type _parameterType;

    public Type ImplementationType { get { return _implementationType; } }
    public Type ParameterType { get { return _parameterType; } }

    public MissingParameterException(Type implementationType, Type parameterType)
        : base(string.Format("A parameter is missing of the type '{0}' while resolving '{1}'", parameterType.FullName, implementationType.FullName))
    {
        _implementationType = implementationType;
        _parameterType = parameterType;
    }
}

public class MultipleTypesException : IoCException
{
    private readonly Type _wantedType;
    private readonly Type[] _types;

    public Type WantedType { get { return _wantedType; } }
    public Type[] Types { get { return _types; } }

    public MultipleTypesException(Type wantedType, Type[] types)
        : base(string.Format("Multiple implementations found for type '{0}': {1}", wantedType.FullName, string.Join(", ", types.Select(x => x.GetNameWithGenerics()).ToArray())))
    {
        _wantedType = wantedType;
        _types = types;
    }
}

public class TooManyParameterTypesException : IoCException
{
    private readonly Type _wantedType;

    public Type WantedType { get { return _wantedType; } }

    public TooManyParameterTypesException(Type wantedType)
        : base(string.Format("Too many parameters of type '{0}' or that derive from it", wantedType.FullName))
    {
        _wantedType = wantedType;
    }
}

public class TooManyRegisteredParameterTypesException : IoCException
{
    private readonly Type _wantedType;

    public Type WantedType { get { return _wantedType; } }

    public TooManyRegisteredParameterTypesException(Type wantedType)
        : base(string.Format("Too many registered parameters of type '{0}' or that derive from it", wantedType.FullName))
    {
        _wantedType = wantedType;
    }
}

public class TypeResolvingFailedException : IoCException
{
    private readonly Type _wantedType;

    public Type WantedType { get { return _wantedType; } }

    public TypeResolvingFailedException(Type wantedType, Exception innerException)
        : base(string.Format("Failed to resolve type '{0}'", wantedType.FullName), innerException)
    {
        _wantedType = wantedType;
    }

    public TypeResolvingFailedException(Type wantedType, string message)
        : base(string.Format("Failed to resolve type '{0}': {1}", wantedType.FullName, message))
    {
        _wantedType = wantedType;
    }
}

//public class NoSystemTypesException : IoCException
//{
//    public NoSystemTypesException()
//        : base("System types aren't allowed as a parameter")
//    {
//    }
//}

//public class NoResolveSystemTypesException : Exception
//{
//    private readonly Type _implementationType;

//    public Type ImplementationType { get { return _implementationType; } }

//    public NoResolveSystemTypesException(Type implementationType)
//        : base(string.Format("System type parameter can't be resolved for '{0}', these kind of parameters must be registered with IoC.RegisterParameter(...)", implementationType.Name))
//    {
//        _implementationType = implementationType;
//    }
//}