using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DePeuter.Shared.Spatial
{
    public class Polygon : Geometry
    {
        public override GeometryType GeometryType
        {
            get { return GeometryType.POLYGON; }
        }

        public CoordinateList OuterRing { get; private set; }

        public List<CoordinateList> InnerRings { get; private set; }

        public Polygon()
        {
            OuterRing = new CoordinateList();
            InnerRings = new List<CoordinateList>();
        }

        public Polygon(string wkt)
        {
            InnerRings = new List<CoordinateList>();

            // POLYGON ((30 10, 40 40, 20 40, 10 20, 30 10))
            // POLYGON ((35 10, 45 45, 15 40, 10 20, 35 10), (20 30, 35 35, 30 20, 20 30))

            var wktContent = GetWktContent(wkt);

            var temp = wktContent.Replace("(", "_").Replace(")", "_").Split('_');
            // __35 10, 45 45, 15 40, 10 20, 35 10_, _20 30, 35 35, 30 20, 20 30__

            foreach (var x in temp)
            {
                var value = x.Trim();
                if (value == string.Empty || value == ",")
                {
                    continue;
                }

                if (OuterRing == null)
                {
                    // 35 10, 45 45, 15 40, 10 20, 35 10
                    OuterRing = new CoordinateList(value);
                }
                else
                {
                    // 20 30, 35 35, 30 20, 20 30
                    InnerRings.Add(new CoordinateList(value));
                }
            }
        }

        public override string GetWktContent()
        {
            if (InnerRings.Any())
            {
                return string.Format("(({0}),({1}))", OuterRing.ToWktSyntax(), InnerRings.Select(x => x.ToWktSyntax()).Join("),("));
            }

            return string.Format("(({0}))", OuterRing.ToWktSyntax());
        }
    }

    public class MultiPolygon : MultiGeometry<Polygon>
    {
        public override GeometryType GeometryType
        {
            get { return GeometryType.MULTIPOLYGON; }
        }

        public MultiPolygon() 
            : base()
        {
        }

        public MultiPolygon(params Polygon[] geometries)
            : base(geometries)
        {
        }

        public MultiPolygon(string wkt)
            : base(wkt)
        {
        }

        protected override void FillGeometriesFromWkt(string wkt)
        {
            // MULTIPOLYGON (((30 20, 45 40, 10 40, 30 20)), ((15 5, 40 10, 10 20, 5 10, 15 5)))
            // MULTIPOLYGON (((40 40, 20 45, 45 30, 40 40)), ((20 35, 10 30, 10 10, 30 5, 45 20, 20 35), (30 20, 20 15, 20 25, 30 20)))

            var wktContent = GetWktContent(wkt);

            var characters = wktContent.ToCharArray();
            // ((30 20, 45 40, 10 40, 30 20)), ((15 5, 40 10, 10 20, 5 10, 15 5))

            int? polygonStart = null;
            var polygonOpenBracketCounter = 0;

            for (var i = 1; i < characters.Length - 1; i++)
            {
                var c = characters[i];

                if (c == '(')
                {
                    if (polygonStart == null)
                    {
                        polygonStart = i;
                    }

                    polygonOpenBracketCounter++;
                }
                else if (c == ')')
                {
                    polygonOpenBracketCounter--;
                }

                if (polygonStart != null && polygonOpenBracketCounter == 0)
                {
                    var polygonWkt = wktContent.Substring(polygonStart.Value, i - polygonStart.Value + 1);

                    AddGeometry(new Polygon(polygonWkt));

                    polygonStart = null;
                }
            }
        }
    }
}
