using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Reflection;

[AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
public abstract class BaseDatabaseProviderAttribute : Attribute
{
    public abstract string ProviderName { get; }
    public abstract Type ProviderType { get; }
}

public class DatabaseProviderAttribute : BaseDatabaseProviderAttribute
{
    private readonly string _providerName;
    private readonly Type _providerType;

    public override string ProviderName { get { return _providerName; } }
    public override Type ProviderType { get { return _providerType; } }

    public DatabaseProviderAttribute(string providerName)
    {
        _providerName = providerName;
    }

    public DatabaseProviderAttribute(string providerName, Type providerType)
    {
        _providerName = providerName;
        _providerType = providerType;
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Field, AllowMultiple = false)]
public class ConnectionStringNameAttribute : Attribute
{
    private readonly string _name;
    public string Name
    {
        get { return _name; }
    }

    public ConnectionStringNameAttribute(string name)
    {
        if(string.IsNullOrEmpty(name))
        {
            throw new ArgumentNullException("name");
        }

        _name = name;
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
public class ProviderAttribute : Attribute
{
    private readonly string _name;
    public string Name
    {
        get { return _name; }
    }

    public ProviderAttribute(string name)
    {
        if(string.IsNullOrEmpty(name))
        {
            throw new ArgumentNullException("name");
        }

        _name = name;
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
public class SupplierAttribute : Attribute
{
    private readonly string _name;
    public string Name
    {
        get { return _name; }
    }

    public SupplierAttribute(string name)
    {
        if(string.IsNullOrEmpty(name))
        {
            throw new ArgumentNullException("name");
        }

        _name = name;
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
public class SqlTableAttribute : Attribute
{
    private readonly string _table;
    public string Table
    {
        get { return _table; }
    }

    public SqlTableAttribute(string table)
    {
        if(string.IsNullOrEmpty(table))
        {
            throw new Exception("SqlTableAttribute can't be empty");
        }

        _table = table;
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
public class SqlSchemaAttribute : Attribute
{
    private readonly string _schema;
    public string Schema
    {
        get { return _schema; }
    }

    public SqlSchemaAttribute(string schema)
    {
        if(string.IsNullOrEmpty(schema))
        {
            throw new Exception("SqlSchemaAttribute can't be empty");
        }

        _schema = schema;
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
public class SqlRelationAttribute : Attribute
{
    private bool _isReversed = false;
    public bool IsReversed
    {
        get { return _isReversed; }
    }

    private readonly string _selfSchema;
    private readonly string _selfTable;
    private readonly string _selfField;
    private readonly Dictionary<string, object> _selfExtraConditions = new Dictionary<string, object>();

    private readonly string _otherSchema;
    private readonly string _otherTable;
    private readonly string _otherField;
    private readonly Dictionary<string, object> _otherExtraConditions = new Dictionary<string, object>();

    public string SelfSchema
    {
        get { return _selfSchema; }
    }
    public string SelfTable
    {
        get { return _selfTable; }
    }
    public string SelfField
    {
        get { return _selfField; }
    }
    public Dictionary<string, object> SelfExtraConditions
    {
        get { return _selfExtraConditions; }
    }

    public string OtherSchema
    {
        get { return _otherSchema; }
    }
    public string OtherTable
    {
        get { return _otherTable; }
    }
    public string OtherField
    {
        get { return _otherField; }
    }
    public Dictionary<string, object> OtherExtraConditions
    {
        get { return _otherExtraConditions; }
    }

    public SqlRelationAttribute(string selfSchema, string selfTable, string selfField, string otherSchema, string otherTable, string otherField)
        : this(selfSchema, selfTable, selfField, otherSchema, otherTable, otherField, null, null)
    {
    }

    public SqlRelationAttribute(string selfSchema, string selfTable, string selfField, string otherSchema, string otherTable, string otherField, object[] selfExtraConditionFieldsAndValues, object[] otherExtraConditionFieldsAndValues)
    {
        _selfSchema = selfSchema;
        _selfTable = selfTable;
        _selfField = selfField;
        _otherSchema = otherSchema;
        _otherTable = otherTable;
        _otherField = otherField;

        if(selfExtraConditionFieldsAndValues != null && selfExtraConditionFieldsAndValues.Any())
        {
            for(int i = 0; i < selfExtraConditionFieldsAndValues.Length; i += 2)
            {
                _selfExtraConditions.Add(selfExtraConditionFieldsAndValues[i].ToString(), selfExtraConditionFieldsAndValues[i + 1]);
            }
        }

        if(otherExtraConditionFieldsAndValues != null && otherExtraConditionFieldsAndValues.Any())
        {
            for(int i = 0; i < otherExtraConditionFieldsAndValues.Length; i += 2)
            {
                _otherExtraConditions.Add(otherExtraConditionFieldsAndValues[i].ToString(), otherExtraConditionFieldsAndValues[i + 1]);
            }
        }
    }

    public override bool Equals(object obj)
    {
        var other = (SqlRelationAttribute)obj;
        return !(this.SelfSchema != other.SelfSchema || this.SelfTable != other.SelfTable || this.SelfField != other.SelfField || this.OtherSchema != other.OtherSchema || this.OtherTable != other.OtherTable || this.OtherField != other.OtherField);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override string ToString()
    {
        return string.Format("{0}.{1}.{2} -> {3}.{4}.{5}", SelfSchema, SelfTable, SelfField, OtherSchema, OtherTable, OtherField);
    }

    public SqlRelationAttribute CreateReverse()
    {
        var reversed = new SqlRelationAttribute(OtherSchema, OtherTable, OtherField, SelfSchema, SelfTable, SelfField, null, null);

        if(SelfExtraConditions.Any())
        {
            foreach(var condition in SelfExtraConditions)
            {
                reversed.OtherExtraConditions.Add(condition.Key, condition.Value);
            }
        }

        if(OtherExtraConditions.Any())
        {
            foreach(var condition in OtherExtraConditions)
            {
                reversed.SelfExtraConditions.Add(condition.Key, condition.Value);
            }
        }

        reversed._isReversed = true;
        return reversed;
    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class SqlPrimaryKeyAttribute : Attribute
{
    private readonly string _pk;
    public string Name
    {
        get { return _pk; }
    }

    public SqlPrimaryKeyAttribute(string pk)
    {
        if(string.IsNullOrEmpty(pk))
        {
            throw new Exception("SqlPrimaryKeyAttribute can't be empty");
        }
        _pk = pk;
    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class SqlFieldNamingTypeAttribute : Attribute
{
    public FieldNamingType FieldNamingType { get; private set; }

    public SqlFieldNamingTypeAttribute(FieldNamingType fieldNamingType)
    {
        FieldNamingType = fieldNamingType;
    }
}

[Obsolete("Not implemented")]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class SqlUniqueFieldAttribute : Attribute
{
    private readonly string _name;
    public string Name
    {
        get { return _name; }
    }

    public SqlUniqueFieldAttribute(string name)
    {
        if(string.IsNullOrEmpty(name))
        {
            throw new ArgumentNullException("name");
        }

        _name = name;
    }
}

//[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
//public class SqlUniqueAttribute : Attribute
//{
//    public SqlUniqueAttribute()
//    {
//    }
//}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class SqlFieldAttribute : Attribute
{
    private readonly string _fieldName;
    private readonly FieldNamingType? _fieldNamingType;

    public SqlFieldAttribute()
    {
    }
    public SqlFieldAttribute(string fieldName)
    {
        _fieldName = fieldName;
    }
    public SqlFieldAttribute(FieldNamingType fieldNamingType)
    {
        _fieldNamingType = fieldNamingType;
    }

    private static readonly Dictionary<Type, string> TypePrefixes = new Dictionary<Type, string>()
    {
        {typeof(string), "vch"},
        {typeof(bool), "bln"},
        {typeof(int), "int"},
        {typeof(long), "lng"},
        {typeof(double), "dbl"},
        {typeof(decimal), "dec"},
        {typeof(DateTime), "dte"}
    };
    private string GetTypePrefix(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        if(TypePrefixes.ContainsKey(type))
        {
            return TypePrefixes[type];
        }
        throw new NotImplementedException(string.Format("No prefix for type configured: {0}", type));
    }

    public virtual string GetName(PropertyInfo pi, FieldNamingType? defaultFieldNamingType = null)
    {
        var fieldNamingType = _fieldNamingType ?? defaultFieldNamingType;
        if(fieldNamingType != null)
        {
            switch(fieldNamingType.Value)
            {
                case FieldNamingType.LowerCase:
                    return pi.Name.ToLower();
                case FieldNamingType.TypeLowerCase:
                    return GetTypePrefix(pi.PropertyType) + pi.Name.ToLower();
                case FieldNamingType.LowerSnakeCamelCase:
                    var chars = pi.Name.ToCharArray();
                    var res = new StringBuilder();
                    res.Append(char.ToLower(chars[0]));
                    foreach(var c in chars.Skip(1))
                    {
                        if(c == char.ToUpper(c) && !char.IsDigit(c))
                        {
                            res.Append("_");
                        }
                        res.Append(char.ToLower(c));
                    }
                    return res.ToString();
                default:
                    throw new NotImplementedException("FieldNamingType." + fieldNamingType.Value);
            }
        }

        return _fieldName ?? pi.Name;
    }

    public virtual string GetSql(SqlFieldSelectParameters parameters)
    {
        return null;
    }

    public virtual string GetSql(SqlFieldInsertParameters parameters)
    {
        return null;
    }

    public virtual string GetSql(SqlFieldUpdateParameters parameters)
    {
        return null;
    }

    internal protected virtual object GetPropertyValue(SqlFieldGetPropertyValueParameters parameters)
    {
        return parameters.PropertyInfo.GetValue(parameters.Entity, null);
    }

    internal protected virtual void SettingPropertyValue(SqlFieldSettingPropertyValueParameters parameters)
    {
    }
}

public class DataParameters : Dictionary<string, object>
{
    public T GetValue<T>(string key)
    {
        return GetValue<T>(key, false);
    }
    public T GetValue<T>(string key, bool required)
    {
        if(ContainsKey(key))
        {
            return this[key].CastTo<T>();
        }

        if(required)
        {
            throw new ArgumentNullException(string.Format("Missing key '{0}' of type '{1}'", key, typeof(T).FullName));
        }

        return default(T);
    }

    public T FindValueOfType<T>()
    {
        var value = FindValueOfType(typeof(T));
        if(value == null)
        {
            return default(T);
        }
        return (T)value;
    }
    public object FindValueOfType(Type type)
    {
        foreach(var x in this)
        {
            if(type.IsInstanceOfType(x.Value))
            {
                return x.Value;
            }
        }

        return null;
    }
}

public abstract class SqlFieldParametersBase
{
    public DataParameters Data { get; private set; }

    public IDatabaseProvider DatabaseProvider { get; private set; }
    public Type Type { get; private set; }
    public PropertyInfo PropertyInfo { get; private set; }

    public bool Cancel;

    protected SqlFieldParametersBase(DataParameters data, IDatabaseProvider provider, Type type, PropertyInfo propertyInfo)
    {
        Data = data;
        DatabaseProvider = provider;
        Type = type;
        PropertyInfo = propertyInfo;
    }
}
public abstract class SqlFieldPropertyParametersBase
{
    public Type Type { get; private set; }
    public PropertyInfo PropertyInfo { get; private set; }

    protected SqlFieldPropertyParametersBase(Type type, PropertyInfo propertyInfo)
    {
        Type = type;
        PropertyInfo = propertyInfo;
    }
}
public abstract class SqlFieldSetPropertyParametersBase : SqlFieldPropertyParametersBase
{
    public bool Cancel;

    protected SqlFieldSetPropertyParametersBase(Type type, PropertyInfo propertyInfo)
        : base(type, propertyInfo)
    {
    }
}
public abstract class SqlFieldGetPropertyParametersBase : SqlFieldPropertyParametersBase
{
    protected SqlFieldGetPropertyParametersBase(Type type, PropertyInfo propertyInfo)
        : base(type, propertyInfo)
    {
    }
}

public class SqlFieldSelectParameters : SqlFieldParametersBase
{
    public string TableAlias { get; private set; }
    public string FieldName { get; private set; }

    public SqlFieldSelectParameters(DataParameters data, IDatabaseProvider provider, Type type, PropertyInfo propertyInfo, string fieldName, string tableAlias)
        : base(data, provider, type, propertyInfo)
    {
        FieldName = fieldName;
        TableAlias = tableAlias;
    }
}
public class SqlFieldInsertUpdateParameters : SqlFieldParametersBase
{
    public object Entity { get; private set; }
    public object PropertyValue { get; private set; }
    public string ParameterName { get; private set; }

    public bool HasNewPropertyValue { get; private set; }
    private object _newPropertyValue;
    public object NewPropertyValue
    {
        get { return _newPropertyValue; }
        set
        {
            _newPropertyValue = value;
            HasNewPropertyValue = true;
        }
    }

    public SqlFieldInsertUpdateParameters(DataParameters data, IDatabaseProvider provider, Type type, PropertyInfo propertyInfo, object entity, object propertyValue, string parameterName)
        : base(data, provider, type, propertyInfo)
    {
        Entity = entity;
        PropertyValue = propertyValue;
        ParameterName = parameterName;
    }
}
public class SqlFieldInsertParameters : SqlFieldInsertUpdateParameters
{
    public SqlFieldInsertParameters(DataParameters data, IDatabaseProvider provider, Type type, PropertyInfo propertyInfo, object entity, object propertyValue, string parameterName)
        : base(data, provider, type, propertyInfo, entity, propertyValue, parameterName)
    {
    }
}
public class SqlFieldUpdateParameters : SqlFieldInsertUpdateParameters
{
    public SqlFieldUpdateParameters(DataParameters data, IDatabaseProvider provider, Type type, PropertyInfo propertyInfo, object entity, object propertyValue, string parameterName)
        : base(data, provider, type, propertyInfo, entity, propertyValue, parameterName)
    {
    }
}
public class SqlFieldSettingPropertyValueParameters : SqlFieldSetPropertyParametersBase
{
    public object Entity { get; private set; }
    public object PropertyValue { get; private set; }

    public bool HasNewPropertyValue { get; private set; }
    private object _newPropertyValue;
    public object NewPropertyValue
    {
        get { return _newPropertyValue; }
        set
        {
            _newPropertyValue = value;
            HasNewPropertyValue = true;
        }
    }

    public SqlFieldSettingPropertyValueParameters(Type type, PropertyInfo propertyInfo, object entity, object propertyValue)
        : base(type, propertyInfo)
    {
        Entity = entity;
        PropertyValue = propertyValue;
    }
}
public class SqlFieldGetPropertyValueParameters : SqlFieldGetPropertyParametersBase
{
    public object Entity { get; private set; }

    public SqlFieldGetPropertyValueParameters(Type type, PropertyInfo propertyInfo, object entity)
        : base(type, propertyInfo)
    {
        Entity = entity;
    }
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class SqlEntityAttribute : Attribute
{
    internal protected virtual object GetPropertyValue(SqlFieldGetPropertyValueParameters parameters)
    {
        return parameters.PropertyInfo.GetValue(parameters.Entity, null);
    }
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class SqlCustomFieldAttribute : SqlFieldAttribute
{
    public SqlCustomFieldAttribute()
    {
    }
    public SqlCustomFieldAttribute(string fieldName)
        : base(fieldName)
    {
    }
    public SqlCustomFieldAttribute(FieldNamingType fieldNamingType)
        : base(fieldNamingType)
    {
    }

    public override string GetSql(SqlFieldSelectParameters parameters)
    {
        parameters.Cancel = true;
        return null;
    }

    public override string GetSql(SqlFieldInsertParameters parameters)
    {
        parameters.Cancel = true;
        return null;
    }

    public override string GetSql(SqlFieldUpdateParameters parameters)
    {
        parameters.Cancel = true;
        return null;
    }
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class SqlReadOnlyFieldAttribute : SqlFieldAttribute
{
    public SqlReadOnlyFieldAttribute()
    {
    }
    public SqlReadOnlyFieldAttribute(string fieldName)
        : base(fieldName)
    {
    }
    public SqlReadOnlyFieldAttribute(FieldNamingType fieldNamingType)
        : base(fieldNamingType)
    {
    }

    public override string GetSql(SqlFieldInsertParameters parameters)
    {
        parameters.Cancel = true;
        return null;
    }

    public override string GetSql(SqlFieldUpdateParameters parameters)
    {
        parameters.Cancel = true;
        return null;
    }
}

[AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
public class IgnoreAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class AuditAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class SqlDefaultValueAttribute : Attribute
{
    public object SelectValue { get; private set; }
    public object InsertValue { get; private set; }
    public object UpdateValue { get; private set; }

    public SqlDefaultValueAttribute(object value)
    {
        SelectValue = value;
        InsertValue = value;
        UpdateValue = value;
    }
    public SqlDefaultValueAttribute(object selectValue, object insertValue, object updateValue)
    {
        SelectValue = selectValue;
        InsertValue = insertValue;
        UpdateValue = updateValue;
    }
}