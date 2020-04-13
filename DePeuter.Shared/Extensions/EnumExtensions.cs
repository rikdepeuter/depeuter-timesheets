using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Reflection;

public static class EnumExtensions
{
    public static T GetAttribute<T>(this Enum enumValue) where T : Attribute
    {
        return (T)Attribute.GetCustomAttribute(enumValue.GetType().GetField(enumValue.ToString()), typeof(T));
    }

    public static T[] GetAttributes<T>(this Enum enumValue) where T : Attribute
    {
        return (T[])Attribute.GetCustomAttributes(enumValue.GetType().GetField(enumValue.ToString()), typeof(T));
    }

    public static bool HasAttribute<T>(this Enum enumValue) where T : Attribute
    {
        return Attribute.GetCustomAttribute(enumValue.GetType().GetField(enumValue.ToString()), typeof(T)) != null;
    }

    public static string GetDescriptionAttribute(this Enum enumValue)
    {
        var attr = Attribute.GetCustomAttribute(enumValue.GetType().GetField(enumValue.ToString()), typeof(DescriptionAttribute)) as DescriptionAttribute;
        if(attr != null)
        {
            return attr.Description;
        }

        return null;
    }

    public static string GetDescription(this Enum enumValue)
    {
        var attr = Attribute.GetCustomAttribute(enumValue.GetType().GetField(enumValue.ToString()), typeof(DescriptionAttribute)) as DescriptionAttribute;
        if (attr != null)
        {
            return attr.Description;
        }

        var enumStrings = enumValue.ToString().Split('_');
        var sb = new StringBuilder();

        var firstEnumString = enumStrings.First();
        sb.Append(firstEnumString.Substring(0, 1).ToUpper());
        if (firstEnumString.Length > 1)
        {
            sb.Append(firstEnumString.Substring(1).ToLower());    
        }

        foreach (var enumString in enumStrings.Skip(1))
        {
            sb.Append(" ");
            sb.Append(enumString.ToLower());
        }

        return sb.ToString();
    }

    public static bool HasFlag(this Enum variable, Enum value)
    {
        // check if from the same type.
        if (variable.GetType() != value.GetType())
        {
            throw new ArgumentException("The checked flag is not from the same type as the checked variable.");
        }

        ulong num = Convert.ToUInt64(value);
        ulong num2 = Convert.ToUInt64(variable);

        return (num2 & num) == num;
    }

    public static TEnum[] GetFlags<TEnum>(this Enum enumValue)
    {
        return Enum.GetValues(typeof(TEnum)).Cast<Enum>().Where(v => enumValue.HasFlag(v)).Select(v => (TEnum)Enum.Parse(typeof(TEnum), v.ToString())).ToArray();
    }

    
}