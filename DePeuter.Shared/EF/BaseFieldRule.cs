using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

public abstract class FieldRuleParameters
{
    private readonly Type _type;
    public Type Type { get { return _type; } }

    private readonly PropertyInfo _propertyInfo;
    public PropertyInfo PropertyInfo { get { return _propertyInfo; } }

    public bool Cancel;

    protected FieldRuleParameters(Type type, PropertyInfo propertyInfo)
    {
        _type = type;
        _propertyInfo = propertyInfo;
    }
}

public class FieldRuleSelectParameters : FieldRuleParameters
{
    private readonly string _fieldName;
    public string FieldName { get { return _fieldName; } }

    private readonly string _tableAlias;
    public string TableAlias { get { return _tableAlias; } }

    public FieldRuleSelectParameters(Type type, PropertyInfo propertyInfo, string fieldName, string tableAlias)
        : base(type, propertyInfo)
    {
        _fieldName = fieldName;
        _tableAlias = tableAlias;
    }
}

public abstract class FieldRuleInsertUpdateParameters : FieldRuleParameters
{
    private readonly object _entity;
    public object Entity { get { return _entity; } }

    private readonly object _propertyValue;
    public object PropertyValue { get { return _propertyValue; } }

    private readonly string _parameterName;
    public string ParameterName { get { return _parameterName; } }

    protected FieldRuleInsertUpdateParameters(Type type, PropertyInfo propertyInfo, object entity, object propertyValue, string parameterName)
        : base(type, propertyInfo)
    {
        _entity = entity;
        _propertyValue = propertyValue;
        _parameterName = parameterName;
    }
}

public class FieldRuleInsertParameters : FieldRuleInsertUpdateParameters
{
    public FieldRuleInsertParameters(Type type, PropertyInfo propertyInfo, object entity, object propertyValue, string parameterName)
        : base(type, propertyInfo, entity, propertyValue, parameterName)
    {
    }
}

public class FieldRuleUpdateParameters : FieldRuleInsertUpdateParameters
{
    public FieldRuleUpdateParameters(Type type, PropertyInfo propertyInfo, object entity, object propertyValue, string parameterName)
        : base(type, propertyInfo, entity, propertyValue, parameterName)
    {
    }
}