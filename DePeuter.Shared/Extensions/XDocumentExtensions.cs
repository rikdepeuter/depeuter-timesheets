using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using DePeuter.Shared.Advanced;

public static class XDocumentExtensions
{
    public static string ToString(this XDocument xDocument, Encoding encoding)
    {
        xDocument.Declaration = new XDeclaration("1.0", encoding.BodyName, null);
        var textWriter = new EncodingStringWriter(encoding);
        xDocument.Save(textWriter);
        return textWriter.ToString();
    }

    public static void Save(this XDocument xDocument, string fileName, Encoding encoding)
    {
        xDocument.Declaration = new XDeclaration("1.0", encoding.BodyName, null);
        var textWriter = new EncodingStringWriter(encoding);
        xDocument.Save(textWriter);
        File.WriteAllText(fileName, textWriter.ToString(), encoding);
    }
}