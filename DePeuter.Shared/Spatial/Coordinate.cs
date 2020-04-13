using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace DePeuter.Shared.Spatial
{
    public class Coordinate
    {
        [SqlField("x")]
        public double X { get; set; }
        [SqlField("y")]
        public double Y { get; set; }

        public Coordinate()
        {
        }

        public Coordinate(double x, double y)
        {
            X = x;
            Y = y;
        }

        public Coordinate(double[] xy)
        {
            if (xy == null)
            {
                return;
            }

            if (xy.Length > 2)
            {
                throw new ArgumentOutOfRangeException("xy");
            }

            if (xy.Length >= 1)
            {
                X = xy[0];
            }
            if(xy.Length == 2)
            {
                Y = xy[1];
            }
        }

        public Coordinate(string wkt)
        {
            if (wkt == null)
            {
                throw new NullReferenceException("wkt");
            }

            var temp = wkt.Trim().TrimStart('(').TrimEnd(')').Split(' ');

            if (temp.Length != 2)
            {
                throw new InvalidWktException();
            }

            X = double.Parse(temp[0], CultureInfo.InvariantCulture);
            Y = double.Parse(temp[1], CultureInfo.InvariantCulture);
        }

        public string ToWktSyntax()
        {
            //20 30
            return string.Format(CultureInfo.InvariantCulture, "{0} {1}", X, Y);
        }
    }

    public class CoordinateList
    {
        public List<Coordinate> Coordinates { get; private set; }

        public CoordinateList()
        {
            Coordinates = new List<Coordinate>();
        }

        public CoordinateList(IEnumerable<Coordinate> coordinates)
            : this()
        {
            if (coordinates != null)
            {
                Coordinates.AddRange(coordinates);
            }
        }

        public CoordinateList(string wkt)
            : this()
        {
            if (wkt == null)
            {
                throw new NullReferenceException("wkt");
            }

            var temp = wkt.Trim().TrimStart('(').TrimEnd(')').Split(',');

            foreach (var x in temp)
            {
                Coordinates.Add(new Coordinate(x));
            }
        }

        public string ToWktSyntax()
        {
            //20 30, 35 35, 30 20, 20 30
            return Coordinates.Select(x => x.ToWktSyntax()).Join(",");
        }
    }
}