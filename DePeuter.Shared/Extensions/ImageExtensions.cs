using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace DePeuter.Shared.Extensions
{
    public static class ImageExtensions
    {
        public static string ToBase64String(this Image image)
        {
            using(var m = new MemoryStream())
            {
                image.Save(m, image.RawFormat);
                var imageBytes = m.ToArray();

                return Convert.ToBase64String(imageBytes);
            }
        }
    }
}
