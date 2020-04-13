using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Media.Imaging;

public static class BitmapExtensions
{
    public static BitmapImage ToBitmapImage(this Bitmap source)
    {
        using (var memory = new MemoryStream())
        {
            source.Save(memory, ImageFormat.Png);
            memory.Position = 0;
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memory;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            return bitmapImage;
        }
    }
}