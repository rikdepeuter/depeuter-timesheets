using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

public static class ColorExtensions
{
    public static int ToOle(this Color color)
    {
        return ColorTranslator.ToOle(color);
    }

    public static int? ToOle(this Color? color)
    {
        if (color == null) return null;
        return ColorTranslator.ToOle(color.Value);
    }
}