using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

public static class XmlDocumentExtensions
{
    //NOTE: this is same as xmlDoc.OuterXml
    //public static string ToXmlString(this XmlDocument xmlDoc)
    //{
    //    using (var stringWriter = new StringWriter())
    //    using (var xmlTextWriter = XmlWriter.Create(stringWriter))
    //    {
    //        xmlDoc.WriteTo(xmlTextWriter);
    //        xmlTextWriter.Flush();
    //        return stringWriter.GetStringBuilder().ToString();
    //    }
    //}
}