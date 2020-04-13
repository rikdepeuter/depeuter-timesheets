using System;
using System.Collections.Generic;
using System.Reflection;

namespace DePeuter.Shared.Shp
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ShpNameAttribute : Attribute
    {
        public string Name { get; private set; }

        public ShpNameAttribute(string name)
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ShpGeometryAttribute : Attribute
    {
        public ShapeType[] ShapeTypes { get; private set; }

        public ShpGeometryAttribute(params ShapeType[] shapeTypes)
        {
            ShapeTypes = shapeTypes;
        }

        public virtual object GetValue(object entity, PropertyInfo pi, object parameters)
        {
            return pi.GetValue(entity, null);
        }

        public virtual void SetValue(object entity, object value, PropertyInfo pi, object parameters)
        {
            pi.SetValue(entity, value, null);
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ShpFieldAttribute : Attribute
    {
        public string Name { get; protected set; }
        public int Width { get; protected set; }
        public int Decimals { get; protected set; }
        public Type DataType { get; protected set; }

        public ShpFieldAttribute(int width)
        {
            Width = width;
        }

        public ShpFieldAttribute(int width, Type dataType)
            : this(width)
        {
            DataType = dataType;
        }

        public ShpFieldAttribute(string name, int width)
            : this(width)
        {
            Name = name;
        }

        public ShpFieldAttribute(string name, int width, Type dataType)
            : this(name, width)
        {
            DataType = dataType;
        }

        public ShpFieldAttribute(int width, int decimals)
            : this(width)
        {
            Decimals = decimals;
        }

        public ShpFieldAttribute(string name, int width, int decimals)
            : this(name, width)
        {
            Decimals = decimals;
        }

        public ShpFieldAttribute(string name, int width, int decimals, Type dataType)
            : this(name, width, decimals)
        {
            DataType = dataType;
        }

        public virtual object GetValue(object entity, PropertyInfo pi, object parameters)
        {
            return pi.GetValue(entity, null);
        }

        public virtual void SetValue(object entity, object value, PropertyInfo pi, object parameters)
        {
            SetValue(entity, pi, value);
        }

        protected virtual void SetValue(object entity, PropertyInfo pi, object value)
        {
            pi.SetValue(entity, value, null);
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ShpEntityAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public abstract class ShpExportFieldValidationAttribute : Attribute
    {
        public abstract bool ExportField(Type type, PropertyInfo pi, List<object> entities, object parameters);
    }
}