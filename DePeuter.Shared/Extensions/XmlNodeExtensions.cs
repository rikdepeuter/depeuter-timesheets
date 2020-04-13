using System.Xml;

public static class XmlNodeExtensions
{
    public static string InnerTextOrEmpty(this XmlNode xmlnode)
    {
        return (xmlnode != null) ? xmlnode.InnerText : string.Empty;
    }
}
