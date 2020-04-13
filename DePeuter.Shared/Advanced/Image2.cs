using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using DePeuter.Shared.Extensions;

namespace DePeuter.Shared.Advanced
{
    public static class Image2
    {
        public static Image ResizeImage(Image img, int width, int height)
        {
            var b = new Bitmap(width, height);
            using(var g = Graphics.FromImage(b))
            {
                g.DrawImage(img, 0, 0, width, height);
            }

            return b;
        }

        public static string GetBase64(string path)
        {
            using(var image = Image.FromFile(path))
            {
                return image.ToBase64String();
            }
        }
    }
}
