using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DePeuter.Shared.Spatial
{
    public enum GeometryType
    {
        POINT,
        MULTIPOINT,
        LINESTRING,
        MULTILINESTRING,
        POLYGON,
        MULTIPOLYGON,
        //GEOMETRYCOLLECTION
    }

    public class InvalidWktException : Exception
    {
    }

    public abstract class Geometry
    {
        public abstract GeometryType GeometryType { get; }

        public static GeometryType GetGeometryTypeFromWkt(string wkt)
        {
            if(wkt == null)
            {
                throw new NullReferenceException("wkt");
            }

            var index = Math.Min(wkt.IndexOf('('), wkt.IndexOf(' '));
            if(index == -1)
            {
                throw new InvalidWktException();
            }

            return wkt.Substring(0, index).ToEnum<GeometryType>();
        }
        public static Geometry ParseFromWkt(string wkt)
        {
            var geometryType = GetGeometryTypeFromWkt(wkt);

            switch(geometryType)
            {
                case GeometryType.POINT:
                    {
                        return new Point(wkt);
                    }
                case GeometryType.MULTIPOINT:
                    {
                        return new MultiPoint(wkt);
                    }
                case GeometryType.LINESTRING:
                    {
                        return new LineString(wkt);
                    }
                case GeometryType.MULTILINESTRING:
                    {
                        return new MultiLineString(wkt);
                    }
                case GeometryType.POLYGON:
                    {
                        return new Polygon(wkt);
                    }
                case GeometryType.MULTIPOLYGON:
                    {
                        return new MultiPolygon(wkt);
                    }
            }

            throw new NotImplementedException("GeometryType." + geometryType);
        }

        protected string GetWktContent(string wkt)
        {
            if(wkt == null)
            {
                throw new NullReferenceException("wkt");
            }

            if(wkt.StartsWith(GeometryType.ToString()))
            {
                return wkt.Substring(GeometryType.ToString().Length).Trim();
            }

            return wkt.Trim();
            //return wkt.Trim().ToCharArray().Where(x => char.IsDigit(x) || x == ' ' || x == ',' || x == ')' || x == '(').Join("");
        }

        public string ToWkt()
        {
            return string.Format("{0} {1}", GeometryType, GetWktContent());
        }
        public abstract string GetWktContent();

        public override string ToString()
        {
            return ToWkt();
        }
    }

    public abstract class MultiGeometry<T> : Geometry
        where T : Geometry
    {
        private readonly List<T> _geometries = new List<T>();
        public T[] Geometries { get { return _geometries.ToArray(); } }

        protected MultiGeometry()
        {
        }

        protected MultiGeometry(params T[] geometries)
            : this()
        {
            AddGeometries(geometries);
        }

        protected MultiGeometry(string wkt)
            : this()
        {
            FillGeometriesFromWkt(wkt);
        }

        protected abstract void FillGeometriesFromWkt(string wkt);

        public override string GetWktContent()
        {
            return string.Format("({0})", Geometries.Select(x => x.GetWktContent()).Join("),("));
            //return string.Format("({0})", Geometries.Select(x => x.Coordinate.ToWktSyntax()).Join(","));
        }

        public MultiGeometry<T> AddGeometry(T geometry)
        {
            if (geometry == null)
            {
                return this;
            }
            _geometries.Add(geometry);
            return this;
        }
        public MultiGeometry<T> AddGeometries(IEnumerable<T> geometries)
        {
            if(geometries == null)
            {
                return this;
            }
            _geometries.AddRange(geometries);
            return this;
        }

        public MultiGeometry<T> RemoveGeometry(T geometry)
        {
            if(geometry == null)
            {
                return this;
            }
            _geometries.Remove(geometry);
            return this;
        }
        public MultiGeometry<T> RemoveGeometries(IEnumerable<T> geometries)
        {
            if(geometries == null)
            {
                return this;
            }
            _geometries.RemoveRange(geometries);
            return this;
        }
    }
}