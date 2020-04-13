using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

public static class XNamespaceExtensions
{
    public static XAttribute ToXmlnsAttribute(this XNamespace ns, string name)
    {
        return new XAttribute(XNamespace.Xmlns + name, ns);
    }

    public static XAttribute ToXAttribute(this XNamespace ns, XNamespace xNamespace, string name)
    {
        return new XAttribute(xNamespace + name, ns);
    }
}