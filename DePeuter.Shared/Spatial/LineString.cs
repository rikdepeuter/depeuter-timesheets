using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DePeuter.Shared.Spatial
{
    public class LineString : Geometry
    {
        public override GeometryType GeometryType { get { return GeometryType.LINESTRING; } }

        public CoordinateList CoordinateList { get; private set; }

        public LineString()
        {
            CoordinateList = new CoordinateList();
        }

        public LineString(string wkt)
        {
            // LINESTRING (30 10, 10 30, 40 40)

            var wktContent = GetWktContent(wkt);

            CoordinateList = new CoordinateList(wktContent);
        }

        public override string GetWktContent()
        {
            return string.Format("({0})", CoordinateList.ToWktSyntax());
        }
    }

    public class MultiLineString : MultiGeometry<LineString>
    {
        public override GeometryType GeometryType { get { return GeometryType.MULTILINESTRING; } }

        public MultiLineString()
            : base()
        {
        }

        public MultiLineString(params LineString[] geometries)
            : base(geometries)
        {
        }

        public MultiLineString(string wkt)
            : base(wkt)
        {
        }

        protected override void FillGeometriesFromWkt(string wkt)
        {
            // MULTILINESTRING ((10 10, 20 20, 10 40), (40 40, 30 30, 40 20, 30 10))

            var wktContent = GetWktContent(wkt);
            // ((10 10, 20 20, 10 40), (40 40, 30 30, 40 20, 30 10))

            var temp = wktContent.Replace("(", "_").Replace(")", "_").Split('_');
            // __10 10, 20 20, 10 40_, _40 40, 30 30, 40 20, 30 10__

            foreach(var x in temp)
            {
                var value = x.Trim();
                if(value == string.Empty || value == ",")
                {
                    continue;
                }

                // 10 10, 20 20, 10 40
                AddGeometry(new LineString(value));
            }
        }
    }
}
