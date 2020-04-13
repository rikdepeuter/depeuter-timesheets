using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;

public static class FlowDocumentExtensions
{
    public static string ToBase64String(this FlowDocument doc)
    {
        var tr = new TextRange(doc.ContentStart, doc.ContentEnd);

        using(var ms = new MemoryStream())
        {
            tr.Save(ms, DataFormats.XamlPackage);
            var xamlText = Convert.ToBase64String(ms.ToArray());

            return xamlText;
        }
    }

    public static string GetAllText(this FlowDocument doc)
    {
        var tr = new TextRange(doc.ContentStart, doc.ContentEnd);
        return tr.Text;
    }

    public static FlowDocument LoadXaml(this FlowDocument doc, string xaml)
    {
        doc.Blocks.Clear();

        if(!string.IsNullOrEmpty(xaml))
        {
            try
            {
                var tr = new TextRange(doc.ContentStart, doc.ContentEnd);

                using(var ms = new MemoryStream(Convert.FromBase64String(xaml)))
                {
                    tr.Load(ms, DataFormats.XamlPackage);
                }
                return doc;
            }
            catch { }

            try
            {
                if(!xaml.StartsWith("<FlowDocument"))
                {
                    xaml = string.Format("<FlowDocument xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><Paragraph>{0}</Paragraph></FlowDocument>", xaml);
                }

                using(var ms = new MemoryStream(Encoding.UTF8.GetBytes(xaml)))
                {
                    var doc2 = (FlowDocument)XamlReader.Load(ms);
                    doc.Blocks.AddRange(doc2.Blocks);
                }
                return doc;
            }
            catch { }

            //var parser = new ParserContext();
            //parser.XmlnsDictionary.Add("", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
            //parser.XmlnsDictionary.Add("x", "http://schemas.microsoft.com/winfx/2006/xaml");
            ////FlowDocument doc = new FlowDocument();
            //var section = (Section)XamlReader.Load(xamlMemoryStream, parser);

            //box.Document.Blocks.Add(section);
        }

        return doc;
    }
}