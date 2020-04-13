using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;

public static class StringExtensions
{
    public static T ToEnum<T>(this string value)
    {
        return (T)Enum.Parse(typeof(T), value);
    }

    public static T ToEnumFromDescription<T>(this string value)
    {
        var type = typeof(T);
        if(!type.IsEnum) throw new InvalidOperationException();
        foreach(var field in type.GetFields())
        {
            var attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
            if(attribute != null)
            {
                if(attribute.Description == value)
                    return (T)field.GetValue(null);
            }
            else
            {
                if(field.Name == value)
                    return (T)field.GetValue(null);
            }
        }
        throw new ArgumentException("No enum (" + type.FullName + ") value found with description: " + value);
    }

    public static string ReplaceIgnoreCase(this string value, char oldChar, char newChar)
    {
        return ReplaceIgnoreCase(value, oldChar.ToString(), newChar.ToString());
    }
    public static string ReplaceIgnoreCase(this string value, string oldText, string newText)
    {
        if(value == null)
        {
            return null;
        }

        if(string.IsNullOrEmpty(oldText))
        {
            return value;
        }

        var sb = new StringBuilder();

        while(true)
        {
            var index = value.IndexOf(oldText, StringComparison.InvariantCultureIgnoreCase);
            if(index == -1)
            {
                sb.Append(value);
                break;
            }

            sb.Append(value.Substring(0, index));
            sb.Append(newText);

            value = value.Substring(index + oldText.Length);
        }

        return sb.ToString();
    }

    public static string ReplaceRepeatedly(this string value, string oldText, string newText)
    {
        if(value == null)
        {
            return null;
        }

        if(string.IsNullOrEmpty(oldText))
        {
            return value;
        }

        if (oldText == newText)
        {
            return value;
        }

        while(value.Contains(oldText))
        {
            value = value.Replace(oldText, newText);
        }

        return value;
    }

    public static string Replace(this string value, char[] oldChars, char newChar)
    {
        if(value == null)
        {
            return null;
        }
        foreach(var c in oldChars)
            value = value.Replace(c, newChar);
        return value;
    }

    public static string Remove(this string value, params string[] chars)
    {
        if(value == null)
        {
            return null;
        }
        foreach(var c in chars)
            value = value.Replace(c, string.Empty);
        return value;
    }

    public static string Remove(this string value, params char[] chars)
    {
        if(value == null)
        {
            return null;
        }
        foreach(var c in chars)
            value = value.Replace(c.ToString(), string.Empty);
        return value;
    }

    public static byte[] ToByteArray(this string value)
    {
        return Encoding.Convert(Encoding.Unicode, Encoding.UTF8, Encoding.Unicode.GetBytes(value));
    }

    public static string GetLastSection(this string value, char delimiter)
    {
        return value.Substring(value.LastIndexOf(delimiter) + 1);
    }

    public static string WithoutLastSection(this string value, char delimiter, bool trimEnd = false)
    {
        var index = value.LastIndexOf(delimiter);
        if(index == -1) return value;

        if(trimEnd)
            return value.Substring(0, index).TrimEnd(delimiter);
        else
            return value.Substring(0, index);
    }

    public static string Prefix(this string value, char prefix, int count)
    {
        for(var i = 0; i < count; i++)
            value = prefix + value;
        return value;
    }
    public static string Prefix(this string value, string prefix, int count)
    {
        for(var i = 0; i < count; i++)
            value = prefix + value;
        return value;
    }

    public static string Affix(this string value, char affix, int count)
    {
        for(var i = 0; i < count; i++)
            value += affix;
        return value;
    }
    public static string Affix(this string value, string affix, int count)
    {
        for(var i = 0; i < count; i++)
            value += affix;
        return value;
    }

    public static string StartAfter(this string value, string start)
    {
        return value.Substring(value.IndexOf(start) + start.Length);
    }

    public static string TakeBefore(this string value, string end)
    {
        return value.Substring(0, value.IndexOf(end));
    }

    public static string[] SplitByWidth(this string value, int width)
    {
        // SplitByWidth("aabbccdd", 2) --> {"aa","bb","cc","dd"}
        // SplitByWidth("aabbc", 2) --> {"aa","bb","c"}
        var res = new string[value.Length % width == 0 ? value.Length / width : (value.Length / width) + 1];
        for(int i = 0; i < value.Length; i += width)
        {
            if(i < value.Length - width)
            {
                res[i / width] = value.Substring(i, width);
            }
            else
            {
                res[i / width] = value.Substring(i);
            }
        }
        return res;
    }

    public static long? GetFirstLong(this string value)
    {
        var chars = value.ToCharArray();
        var number = string.Empty;
        foreach(var c in chars)
        {
            if(Math2.IsInteger(c.ToString()))
            {
                number += c.ToString();
            }
            else
            {
                if(number != string.Empty)
                    break;
            }
        }

        if(number == string.Empty) return null;
        return long.Parse(number);
    }
    public static int? GetFirstInteger(this string value)
    {
        var chars = value.ToCharArray();
        var number = string.Empty;
        foreach(var c in chars)
        {
            if(Math2.IsInteger(c.ToString()))
            {
                number += c.ToString();
            }
            else
            {
                if(number != string.Empty)
                    break;
            }
        }

        if(number == string.Empty) return null;
        return int.Parse(number);
    }
    public static int[] GetIntegers(this string value)
    {
        var res = new List<int>();
        var chars = value.ToCharArray();
        var number = string.Empty;
        foreach(var c in chars)
        {
            if(Math2.IsInteger(c.ToString()))
            {
                number += c.ToString();
            }
            else
            {
                if(number != string.Empty)
                {
                    res.Add(int.Parse(number));
                    number = string.Empty;
                }
            }
        }

        return res.ToArray();
    }

    //public static string[] Split(this string value, params char[] seperators)
    //{
    //    if (seperators == null || seperators.Length == 0)
    //    {
    //        throw new InvalidOperationException("At least 1 seperator is required");
    //    }

    //    var q = value.Split(seperators[0]).Select(x => x);
    //    foreach (var seperator in seperators)
    //    {
    //        q = q.Select(x => x.Split(seperator)).SelectMany(x => x);
    //    }
    //    return q.ToArray();
    //}

    public static string EmptyAsNull(this string value)
    {
        return value == string.Empty ? null : value;
    }

    public static string EndWith(this string original, string value)
    {
        return original.EndsWith(value) ? original : original + value;
    }

    public static string StartWith(this string original, string value)
    {
        return original.StartsWith(value) ? original : value + original;
    }

    public static int IndexOfNonNumeric(this string value)
    {
        for(int i = 0; i < value.Length; i++)
        {
            if(!Math2.IsInteger(value.Substring(i, 1)))
                return i;
        }

        return -1;
    }

    public static string WithLength(this string value, int wantedLength)
    {
        return WithLength(value, wantedLength, null);
    }
    public static string WithLength(this string value, int wantedLength, string fillValue, FillValueType fillValueType = FillValueType.Affix)
    {
        if(value == null) return null;
        if(string.IsNullOrEmpty(fillValue)) fillValue = " ";

        if(value.Length == wantedLength) return value;

        if(value.Length > wantedLength) return value.Substring(0, wantedLength);

        while(value.Length < wantedLength)
        {
            if(fillValueType == FillValueType.Affix)
            {
                value += fillValue;
            }
            else
            {
                value = fillValue + value;
            }
        }

        return value.Substring(0, wantedLength);
    }

    public static string IfNullOrEmpty(this string value, string newValue)
    {
        return string.IsNullOrEmpty(value) ? newValue : value;
    }
    public static string IfNullOrWhiteSpace(this string value, string newValue)
    {
        return string.IsNullOrWhiteSpace(value) ? newValue : value;
    }

    public static string WithMaxLength(this string value, int maxLength)
    {
        return WithMaxLength(value, maxLength, null);
    }
    public static string WithMaxLength(this string value, int maxLength, string affixOnCut)
    {
        if(value == null) return null;

        if(value.Length > maxLength)
        {
            return string.Format("{0}{1}", value.Substring(0, maxLength), affixOnCut);
        }

        return value;
    }

    public static string ToValidFileName(this string value, char replaceChar)
    {
        var invalidChars = new List<char>(new char[] { '/', '\\' });
        invalidChars.AddRange(Path.GetInvalidPathChars().ToList());

        return invalidChars.Aggregate(value, (current, c) => current.Replace(c, replaceChar));
    }
}
public enum FillValueType
{
    Prefix,
    Affix
}