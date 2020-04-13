using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

public static class StringBuilderExtensions
{
    public static void AppendLine(this StringBuilder sb, string format, params object[] args)
    {
        sb.AppendLine(string.Format(format, args));
    }

    public static void AppendInvariantLine(this StringBuilder sb, string format, params object[] args)
    {
        sb.AppendLine(CultureInfo.InvariantCulture, format, args);
    }

    public static void AppendLine(this StringBuilder sb, IFormatProvider provider, string format, params object[] args)
    {
        sb.AppendLine(string.Format(provider, format, args));
    }

    public static XmlDocument ToXmlDocument(this StringBuilder sb)
    {
        var doc = new XmlDocument();
        doc.LoadXml(sb.ToString());
        return doc;
    }

    public static XDocument ToXDocument(this StringBuilder sb)
    {
        return XDocument.Parse(sb.ToString());
    }
}