using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace DePeuter.Shared
{
    public class XmlColor
    {
        public XmlColor()
        {
            FromColor(Color.Black);
        }

        public XmlColor(Color c)
        {
            FromColor(c);
        }

        public Color ToColor()
        {
            return Color.FromArgb(Alpha, Red, Green, Blue);
        }

        public void FromColor(Color c)
        {
            Alpha = c.A;
            Red = c.R;
            Green = c.G;
            Blue = c.B;
        }

        public static implicit operator Color(XmlColor x)
        {
            return x.ToColor();
        }

        public static implicit operator XmlColor(Color c)
        {
            return new XmlColor(c);
        }

        [XmlAttribute]
        public byte Alpha { get; set; }
        [XmlAttribute]
        public byte Red { get; set; }
        [XmlAttribute]
        public byte Green { get; set; }
        [XmlAttribute]
        public byte Blue { get; set; }

        [XmlAttribute]
        public string Web
        {
            get
            {
                return ColorTranslator.ToHtml(ToColor());
            }
            set
            {
                try
                {
                    if(Alpha == 0xFF) // preserve named color value if possible
                        FromColor(ColorTranslator.FromHtml(value));
                    else
                        FromColor(Color.FromArgb(Alpha, ColorTranslator.FromHtml(value)));
                }
                catch(Exception)
                {
                    FromColor(Color.Black);
                }
            }
        }

        public bool ShouldSerializeWeb()
        {
            return false;
        }
    }
}