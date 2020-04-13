using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static partial class Math2
{
    public static bool IsInteger(string value)
    {
        int res;
        return int.TryParse(value, out res);
    }

    public static bool IsDouble(string value)
    {
        double res;
        return double.TryParse(value, out res);
    }

    public static void GetNewDimension(ref double width, ref double height, double maxWidth, double maxHeight, bool useAllAvailableSpace = false)
    {
        if(width < maxWidth && height < maxHeight)
        {
            //fits within max dimension
            if(useAllAvailableSpace)
            {
                //gwn te groot maken en dan terug functie oproepen
                var aspectRatio = width/height;
                width += maxWidth*aspectRatio;
                height += maxHeight*aspectRatio;
                GetNewDimension(ref width, ref height, maxWidth, maxHeight);
            }
        }
        else
        {
            var aspectRatio = width/height;

            if(aspectRatio > 1)
            {
                //landscape
                height = (height*maxWidth)/width; //329
                width = maxWidth; //585
                if(height > maxHeight)
                {
                    width = width - ((height - maxHeight)*aspectRatio);
                    height = maxHeight;
                }
            }
            else
            {
                //portrait
                width = (width*maxHeight)/height;
                height = maxHeight;
                if(width > maxWidth)
                {
                    height = height - ((width - maxWidth)*aspectRatio);
                    width = maxWidth;
                }
            }
        }
    }

    private const double FeetToMeter = 0.3048;
    public static double FeetToMeters(double value)
    {
        return value*FeetToMeter;
    }
    public static double FeetToMeters(double value, int dimension)
    {
        return value*Math.Pow(FeetToMeter, dimension);
    }
    public static double MetersToFeet(double value)
    {
        return value/FeetToMeter;
    }
    public static double MetersToFeet(double value, int dimension)
    {
        return value/Math.Pow(FeetToMeter, dimension);
    }

    public static double Average(params double[] values)
    {
        return values.Sum()/values.Length;
    }

    public static double PrecisionToTolerance(int precision)
    {
        return 1.0/Math.Pow(10, precision);
    }
}