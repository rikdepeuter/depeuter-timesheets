using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DePeuter.Shared.Spatial
{
    public class Point : Geometry
    {
        public override GeometryType GeometryType { get { return GeometryType.POINT; } }

        public Coordinate Coordinate { get; private set; }

        public Point(double x, double y)
        {
            Coordinate = new Coordinate(x, y);
        }

        public Point(double[] xy)
        {
            Coordinate = new Coordinate(xy);
        }

        public Point(Coordinate coordinate)
        {
            Coordinate = coordinate;
        }

        public Point(string wkt)
        {
            // POINT (30 10)

            var wktContent = GetWktContent(wkt);

            Coordinate = new Coordinate(wktContent);
        }

        public override string GetWktContent()
        {
            return string.Format("({0})", Coordinate.ToWktSyntax());
        }
    }

    public class MultiPoint : MultiGeometry<Point>
    {
        public override GeometryType GeometryType { get { return GeometryType.MULTIPOINT; } }

        public MultiPoint()
            : base()
        {
        }

        public MultiPoint(params Point[] geometries)
            : base(geometries)
        {
        }

        public MultiPoint(string wkt)
            : base(wkt)
        {
        }

        protected override void FillGeometriesFromWkt(string wkt)
        {
            // MULTIPOINT ((10 40), (40 30), (20 20), (30 10))
            // MULTIPOINT (10 40, 40 30, 20 20, 30 10)

            var wktContent = GetWktContent(wkt);

            if(wktContent.StartsWith("(("))
            { // ((10 40), (40 30), (20 20), (30 10))
                wktContent = wktContent.Remove("(", ")");
            }

            // (10 40, 40 30, 20 20, 30 10)
            var coordinateList = new CoordinateList(wktContent);
            foreach(var coordinate in coordinateList.Coordinates)
            {
                AddGeometry(new Point(coordinate));
            }
        }
    }
}
