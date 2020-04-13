using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

public static class BitmapExtensions
{
    public static Bitmap Crop(this Bitmap source, Rectangle section)
    {
        // An empty bitmap which will hold the cropped image
        var bmp = new Bitmap(section.Width, section.Height);

        using (var g = Graphics.FromImage(bmp))
        {
            // Draw the given area (section) of the source image at location 0,0 on the empty bitmap (bmp)
            g.DrawImage(source, 0, 0, section, GraphicsUnit.Pixel);
        }

        return bmp;
    }
}