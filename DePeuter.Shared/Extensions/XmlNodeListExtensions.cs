using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

public static class XmlNodeListExtensions
{
    public static IEnumerable<XmlNode> AsEnumerable(this XmlNodeList xmlNodeList)
    {
        foreach (XmlNode node in xmlNodeList)
            yield return node;
    }
}