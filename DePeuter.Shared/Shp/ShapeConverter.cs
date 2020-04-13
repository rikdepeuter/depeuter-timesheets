using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using DePeuter.Shared.Spatial;

namespace DePeuter.Shared.Shp
{
    public class ShapeConverter
    {
        private static readonly Dictionary<Type, PropertyInfo[]> EntityTypeExportProperties = new Dictionary<Type, PropertyInfo[]>();
        private static readonly Dictionary<Type, PropertyInfo[]> EntityTypeImportProperties = new Dictionary<Type, PropertyInfo[]>();
        private static readonly object Lock = new object();

        private static void FillEntityTypeExportProperties(Type type)
        {
            lock(Lock)
            {
                if(EntityTypeExportProperties.ContainsKey(type)) return;

                var properties = new List<PropertyInfo>();
                var fieldProperties = new Dictionary<string, PropertyInfo>();

                foreach(var pi in type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(pi => pi.CanRead && pi.GetGetMethod(true).IsPublic))
                {
                    if (pi.HasCustomAttribute<IgnoreAttribute>())
                    {
                        continue;
                    }

                    if(pi.HasCustomAttribute<ShpGeometryAttribute>())
                    {
                        properties.Add(pi);
                        continue;
                    }

                    if(pi.HasCustomAttribute<ShpEntityAttribute>())
                    {
                        properties.Add(pi);
                        FillEntityTypeExportProperties(pi.PropertyType);
                        continue;
                    }

                    var shpFieldAttr = pi.GetCustomAttribute<ShpFieldAttribute>();
                    if(shpFieldAttr != null)
                    {
                        var name = shpFieldAttr.Name.ToLower();
                        if(fieldProperties.ContainsKey(name))
                        {
                            var pi2 = fieldProperties[name];

                            if(pi2.DeclaringType == type)
                            {
                                continue;
                            }

                            if(pi.DeclaringType == type)
                            {
                                properties.Remove(pi2);
                                fieldProperties.Remove(name);

                                properties.Add(pi);
                                fieldProperties.Add(name, pi);
                            }
                        }
                        else
                        {
                            properties.Add(pi);
                            fieldProperties.Add(name, pi);
                        }

                        continue;
                    }
                }

                EntityTypeExportProperties.Set(type, properties.ToArray());
            }
        }

        public PropertyInfo[] GetEntityTypeExportProperties(Type type, List<object> entities, object parameters)
        {
            FillEntityTypeExportProperties(type);

            return EntityTypeExportProperties[type].Select(pi =>
            {
                var exportField = true;
                var attr = pi.GetCustomAttribute<ShpExportFieldValidationAttribute>();
                if(attr != null)
                {
                    exportField = attr.ExportField(type, pi, entities, parameters);
                }
                return new { exportField, pi };
            }).Where(x => x.exportField).Select(x => x.pi).ToArray();
        }

        private static void FillEntityTypeImportProperties(Type type)
        {
            lock(Lock)
            {
                if(EntityTypeImportProperties.ContainsKey(type)) return;

                var properties = new List<PropertyInfo>();
                var fieldProperties = new Dictionary<string, PropertyInfo>();

                foreach(var pi in type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(pi => pi.CanWrite && pi.GetGetMethod(true).IsPublic))
                {
                    if (pi.HasCustomAttribute<IgnoreAttribute>())
                    {
                        continue;
                    }

                    if (pi.HasCustomAttribute<ShpGeometryAttribute>())
                    {
                        properties.Add(pi);
                        continue;
                    }

                    if(pi.HasCustomAttribute<ShpEntityAttribute>())
                    {
                        properties.Add(pi);
                        FillEntityTypeImportProperties(pi.PropertyType);
                        continue;
                    }

                    var shpFieldAttr = pi.GetCustomAttribute<ShpFieldAttribute>();
                    if(shpFieldAttr != null)
                    {
                        var name = shpFieldAttr.Name.ToLower();
                        if (fieldProperties.ContainsKey(name))
                        {
                            var pi2 = fieldProperties[name];

                            if (pi2.DeclaringType == type)
                            {
                                continue;
                            }

                            if (pi.DeclaringType == type)
                            {
                                properties.Remove(pi2);
                                fieldProperties.Remove(name);

                                properties.Add(pi);
                                fieldProperties.Add(name, pi);
                            }
                        }
                        else
                        {
                            properties.Add(pi);
                            fieldProperties.Add(name, pi);    
                        }

                        continue;
                    }
                }

                EntityTypeImportProperties.Set(type, properties.ToArray());
            }
        }

        public PropertyInfo[] GetEntityTypeImportProperties(Type type)
        {
            FillEntityTypeImportProperties(type);

            return EntityTypeImportProperties[type];
        }

        public void Export<T>(ShapeType? shpType, string shpName, List<T> entities, string exportDirectory, object parameters, bool append) where T : class
        {
            Export(typeof(T), shpType, shpName, entities != null ? entities.Select(x => (object)x).ToList() : null, exportDirectory, parameters, append);
        }

        public void Export(Type entityType, ShapeType? shpType, string shpName, List<object> entities, string exportDirectory, object parameters, bool append)
        {
            if(entities == null)
            {
                throw new ArgumentNullException("entities");
            }

            var type = entityType;

            if(string.IsNullOrEmpty(exportDirectory))
            {
                throw new ArgumentNullException("exportDirectory");
            }

            exportDirectory = exportDirectory.EndWith("\\");

            Directory.CreateDirectory(exportDirectory);

            if(string.IsNullOrEmpty(shpName))
            {
                var elementNameAttr = type.GetCustomAttribute<ShpNameAttribute>();
                if(elementNameAttr == null)
                {
                    throw new InvalidOperationException(string.Format("Missing ShpNameAttribute on type '{0}'", type.FullName));
                }

                shpName = elementNameAttr.Name;
            }

            var properties = GetEntityTypeExportProperties(type, entities, parameters);

            var shpGeometryFields = properties.Where(x => x.HasCustomAttribute<ShpGeometryAttribute>()).ToList();
            if(!shpGeometryFields.Any())
            {
                throw new InvalidOperationException(string.Format("No property found with a ShpGeometryFieldAttribute on type '{0}'", type.FullName));
            }
            if(shpGeometryFields.Count > 1)
            {
                throw new InvalidOperationException(string.Format("Multiple properties found with a ShpGeometryFieldAttribute on type '{0}'", type.FullName));
            }

            var geometryPi = shpGeometryFields.Single();
            var geometryAttr = geometryPi.GetCustomAttribute<ShpGeometryAttribute>();

            if(shpType == null)
            {
                if(geometryAttr.ShapeTypes == null || !geometryAttr.ShapeTypes.Any())
                {
                    throw new InvalidOperationException(string.Format("No ShapeType defined on type '{0}'", type.FullName));
                }

                if(geometryAttr.ShapeTypes.Length > 1)
                {
                    throw new InvalidOperationException(string.Format("Multiple ShapeTypes defined on type '{0}'", type.FullName));
                }

                shpType = geometryAttr.ShapeTypes.Single();
            }

            var shpFilePath = exportDirectory + shpName;
            append = append && File.Exists(shpFilePath + ".shp");
            var shpFile = append ?  ShapeLib.SHPOpen(shpFilePath + ".shp", "rb+") : ShapeLib.SHPCreate(shpFilePath, shpType.Value);
            if(shpFile == IntPtr.Zero)
            {
                throw new ValidationException(shpName +  ": Failed to create/open SHP file.");
            }

            try
            {
                var dbfFilePath = exportDirectory + shpName;
                var dbfFile = append ? ShapeLib.DBFOpen(dbfFilePath + ".dbf", "rb+") : ShapeLib.DBFCreate(dbfFilePath);
                if(dbfFile == IntPtr.Zero)
                {
                    throw new ValidationException(shpName +  ": Failed to create/open DBF file.");
                }

                try
                {
                    if(!append)
                    {
                        //create fields
                        var columnNames = new List<string>();
                        CreateShpFields(dbfFile, type, ref columnNames, entities, parameters);
                    }

                    //export data
                    var rowIndex = -1;
                    var recordCount = append ? ShapeLib.DBFGetRecordCount(dbfFile) : -1;

                    foreach(var entity in entities.Where(x => type.IsInstanceOfType(x)))
                    {
                        rowIndex++;

                        var geometry = (string)geometryAttr.GetValue(entity, geometryPi, parameters);

                        if(string.IsNullOrEmpty(geometry))
                        {
                            throw new NullReferenceException("geometry");
                        }

                        var shape = WriteShpGeometryField(-1, geometry);

                        var ret = ShapeLib.SHPWriteObject(shpFile, rowIndex + (append ? recordCount : 0), shape);
                        if(ret == -1)
                        {
                            throw new ValidationException("Failed to export geometry");
                        }

                        ShapeLib.SHPDestroyObject(shape);

                        var colIndex = 0;
                        WriteShpFields(dbfFile, type, rowIndex + (append ? recordCount : 0), ref colIndex, entity, entities, parameters);
                    }
                }
                finally
                {
                    ShapeLib.DBFClose(dbfFile);
                }
            }
            finally
            {
                ShapeLib.SHPClose(shpFile);
            }
        }

        private string GetFieldName(Type type, PropertyInfo pi, ShpFieldAttribute attr)
        {
            var fieldName = attr.Name ?? pi.Name;
            if(fieldName.Length > 10)
            {
                throw new ValidationException("Name is invalid (must be not null and length <= 10) for type '{0}' and property '{1}'", type.FullName, pi.Name);
            }
            return fieldName;
        }

        private void CreateShpFields(IntPtr dbfFile, Type type, ref List<string> columnNames, List<object> entities, object parameters)
        {
            var properties = GetEntityTypeExportProperties(type, entities, parameters).Where(x => x.HasCustomAttribute<ShpFieldAttribute>() || x.HasCustomAttribute<ShpEntityAttribute>()).ToList();

            foreach(var pi in properties)
            {
                try
                {
                    var entityAttr = pi.GetCustomAttribute<ShpEntityAttribute>();
                    if(entityAttr != null)
                    {
                        CreateShpFields(dbfFile, pi.PropertyType, ref columnNames, entities, parameters);
                        continue;
                    }

                    var attr = pi.GetCustomAttribute<ShpFieldAttribute>();

                    var fieldName = GetFieldName(type, pi, attr);

                    if(columnNames.Contains(fieldName))
                    {
                        throw new Exception(string.Format("Field '{0}' in type '{1}' already exists", fieldName, type.FullName));
                    }

                    columnNames.Add(fieldName);

                    if(attr.Width < 0)
                    {
                        throw new ValidationException("Invalid width value");
                    }

                    if(attr.Decimals < 0)
                    {
                        throw new ValidationException("Invalid decimals value");
                    }

                    var propertyType = attr.DataType ?? pi.PropertyType;
                    propertyType = (Nullable.GetUnderlyingType(propertyType) ?? propertyType);

                    object value = null;
                    UpdateFieldDefinition(pi, null, parameters, ref propertyType, ref value);

                    var fieldType = GetDBFFieldType(propertyType);

                    var res = ShapeLib.DBFAddField(dbfFile, fieldName.ToLower(), fieldType, attr.Width, attr.Decimals);
                    if(res == -1)
                    {
                        throw new ValidationException("Failed to add field: " + fieldName);
                    }
                }
                catch(Exception ex)
                {
                    throw new Exception(string.Format("Error on type '{0}' at property '{1}': {2}", type.FullName, pi.Name, ex.Message), ex);
                }
            }
        }

        private void WriteShpFields(IntPtr dbfFile, Type type, int rowIndex, ref int colIndex, object entity, List<object> entities, object parameters)
        {
            var properties = GetEntityTypeExportProperties(type, entities, parameters).Where(x => x.HasCustomAttribute<ShpFieldAttribute>() || x.HasCustomAttribute<ShpEntityAttribute>()).ToList();

            foreach(var pi in properties)
            {
                try
                {
                    var entityAttr = pi.GetCustomAttribute<ShpEntityAttribute>();
                    if(entityAttr != null)
                    {
                        WriteShpFields(dbfFile, pi.PropertyType, rowIndex, ref colIndex, pi.GetValue(entity, null), entities, parameters);
                        continue;
                    }

                    var res = WriteShpField(dbfFile, rowIndex, ref colIndex, entity, pi, parameters);
                    if(res == -1)
                    {
                        throw new ValidationException("Failed to write value to field");
                    }

                    colIndex++;
                }
                catch(Exception ex)
                {
                    throw new Exception(string.Format("Error on type '{0}' at property '{1}': {2}", type.FullName, pi.Name, ex.Message), ex);
                }
            }
        }

        protected void FillPropertyTypeAndValue(PropertyInfo pi, object entity, object parameters, out Type propertyType, out object value)
        {
            var attr = pi.GetCustomAttribute<ShpFieldAttribute>();

            value = attr.GetValue(entity, pi, parameters);

            if(value == null)
            {
                propertyType = null;
                return;
            }

            propertyType = attr.DataType ?? pi.PropertyType;
            propertyType = (Nullable.GetUnderlyingType(propertyType) ?? propertyType);
        }

        protected virtual void UpdateFieldDefinition(PropertyInfo pi, object entity, object parameters, ref Type propertyType, ref object value)
        {
        }

        private int WriteShpField(IntPtr dbfFile, int rowIndex, ref int colIndex, object entity, PropertyInfo pi, object parameters)
        {
            Type propertyType;
            object value;

            FillPropertyTypeAndValue(pi, entity, parameters, out propertyType, out value);

            UpdateFieldDefinition(pi, entity, parameters, ref propertyType, ref value);

            if (value == null)
            {
                return 0;
            }

            if(propertyType == typeof(string))
            {
                return ShapeLib.DBFWriteStringAttribute(dbfFile, rowIndex, colIndex, value.ToString());
            }
            if(propertyType == typeof(double) || propertyType == typeof(decimal))
            {
                return ShapeLib.DBFWriteDoubleAttribute(dbfFile, rowIndex, colIndex, (double)value);
            }
            if(propertyType == typeof(short) || propertyType == typeof(int) || propertyType == typeof(long))
            {
                return ShapeLib.DBFWriteIntegerAttribute(dbfFile, rowIndex, colIndex, int.Parse(value.ToString()));
            }
            if(propertyType == typeof(DateTime))
            {
                return ShapeLib.DBFWriteDateAttribute(dbfFile, rowIndex, colIndex, (DateTime)value);
            }

            throw new ValidationException("Unknown field type: {0}", pi.PropertyType);
        }

        protected virtual ShapeLib.DBFFieldType GetDBFFieldType(Type propertyType)
        {
            propertyType = (Nullable.GetUnderlyingType(propertyType) ?? propertyType);

            if(propertyType == typeof(string))
            {
                return ShapeLib.DBFFieldType.FTString;
            }
            if(propertyType == typeof(double) || propertyType == typeof(decimal))
            {
                return ShapeLib.DBFFieldType.FTDouble;
            }
            if(propertyType == typeof(short) || propertyType == typeof(int) || propertyType == typeof(long))
            {
                return ShapeLib.DBFFieldType.FTInteger;
            }
            if(propertyType == typeof(DateTime))
            {
                return ShapeLib.DBFFieldType.FTDate;
            }

            throw new ValidationException("Unknown field type: {0}", propertyType);
        }

        protected IntPtr WriteShpGeometryField(int iShape, string wkt)
        {
            var geom = Geometry.ParseFromWkt(wkt);

            return WriteShpGeometryField(iShape, geom);
        }

        private IntPtr WriteShpGeometryField(int iShape, MultiPolygon multiPolygon)
        {
            if(multiPolygon.Geometries.Length == 1)
            {
                return WriteShpGeometryField(iShape, multiPolygon.Geometries[0]);
            }

            var nvertices = 0;
            var apartStart = new List<int>();
            var apartType = new List<ShapeLib.PartType>();
            var vX = new List<double>();
            var vY = new List<double>();

            apartStart.Add(nvertices);

            for(var i = 0; i < multiPolygon.Geometries.Length; i++)
            {
                var polygon = multiPolygon.Geometries[i];

                var vertices = polygon.OuterRing.Coordinates;

                vX.AddRange(vertices.Select(x => x.X));
                vY.AddRange(vertices.Select(x => x.Y));

                vX.Add(vertices.First().X);
                vY.Add(vertices.First().Y);

                nvertices += vertices.Count;
                if(i + 1 != multiPolygon.Geometries.Length)
                    apartStart.Add(nvertices);
                apartType.Add(ShapeLib.PartType.Ring);
            }

            return ShapeLib.SHPCreateObject(ShapeType.Polygon, iShape, apartStart.Count, apartStart.ToArray(), apartType.ToArray(), nvertices, vX.ToArray(), vY.ToArray(), null, null);
        }
        private IntPtr WriteShpGeometryField(int iShape, Polygon polygon)
        {
            var exteriorRingVertices = polygon.OuterRing.Coordinates.ToList();
            exteriorRingVertices.Add(exteriorRingVertices.First());

            if(polygon.InnerRings.Count > 0)
            {
                var nvertices = 0;
                var apartStart = new List<int>();
                var apartType = new List<ShapeLib.PartType>();
                var vX = new List<double>();
                var vY = new List<double>();

                apartStart.Add(nvertices);
                apartType.Add(ShapeLib.PartType.Ring);
                nvertices += exteriorRingVertices.Count;
                for(var v = 0; v < exteriorRingVertices.Count; v++)
                {
                    vX.Add(exteriorRingVertices[v].X);
                    vY.Add(exteriorRingVertices[v].Y);
                }
                apartStart.Add(nvertices);

                for(var j = 0; j < polygon.InnerRings.Count; j++)
                {
                    var interiorRingVertices = polygon.InnerRings[j].Coordinates.ToList();
                    interiorRingVertices.Add(interiorRingVertices.First());
                    for(var v = 0; v < interiorRingVertices.Count; v++)
                    {
                        vX.Add(interiorRingVertices[v].X);
                        vY.Add(interiorRingVertices[v].Y);
                    }
                    nvertices += interiorRingVertices.Count;
                    if(j + 1 != polygon.InnerRings.Count)
                        apartStart.Add(nvertices);
                    apartType.Add(ShapeLib.PartType.Ring);
                }

                return ShapeLib.SHPCreateObject(ShapeType.Polygon, iShape, apartStart.Count, apartStart.ToArray(), apartType.ToArray(), nvertices, vX.ToArray(), vY.ToArray(), null, null);
            }
            else
            {
                var vX = new List<double>();
                var vY = new List<double>();
                for(var v = 0; v < exteriorRingVertices.Count; v++)
                {
                    vX.Add(exteriorRingVertices[v].X);
                    vY.Add(exteriorRingVertices[v].Y);
                }
                return ShapeLib.SHPCreateSimpleObject(ShapeType.Polygon, exteriorRingVertices.Count, vX.ToArray(), vY.ToArray(), null);
            }
        }
        private IntPtr WriteShpGeometryField(int iShape, MultiLineString multiLineString)
        {
            if(multiLineString.Geometries.Length == 1)
            {
                return WriteShpGeometryField(iShape, multiLineString.Geometries[0]);
            }

            var nvertices = 0;
            var apartStart = new List<int>();
            var vX = new List<double>();
            var vY = new List<double>();

            apartStart.Add(nvertices);

            for(var i = 0; i < multiLineString.Geometries.Length; i++)
            {
                var lineString = multiLineString.Geometries[i];

                var isClosed = lineString.CoordinateList.Coordinates.First().Equals(lineString.CoordinateList.Coordinates.Last());

                var coordinates = lineString.CoordinateList.Coordinates.ToList();
                if(isClosed)
                    coordinates.Add(coordinates.First());
                else
                    coordinates.Add(coordinates.Last());

                foreach(var coordinate in coordinates)
                {
                    vX.Add(coordinate.X);
                    vY.Add(coordinate.Y);
                }

                nvertices += coordinates.Count;
                if(i + 1 != multiLineString.Geometries.Length)
                    apartStart.Add(nvertices);
            }

            return ShapeLib.SHPCreateObject(ShapeType.PolyLine, iShape, apartStart.Count, apartStart.ToArray(), null, nvertices, vX.ToArray(), vY.ToArray(), null, null);
        }
        private IntPtr WriteShpGeometryField(int iShape, LineString lineString)
        {
            var isClosed = lineString.CoordinateList.Coordinates.First().Equals(lineString.CoordinateList.Coordinates.Last());

            var coordinates = lineString.CoordinateList.Coordinates.ToList();
            if(isClosed)
                coordinates.Add(coordinates.First());
            else
                coordinates.Add(coordinates.Last());

            var xArray = coordinates.Select(x => x.X).ToArray();
            var yArray = coordinates.Select(x => x.Y).ToArray();

            return ShapeLib.SHPCreateObject(ShapeType.PolyLine, iShape, 0, null, null, xArray.Length, xArray, yArray, null, null);
        }
        private IntPtr WriteShpGeometryField(int iShape, MultiPoint multiPoint)
        {
            var nvertices = 0;
            var apartStart = new List<int>();
            var vX = new List<double>();
            var vY = new List<double>();

            apartStart.Add(nvertices);

            for(var i = 0; i < multiPoint.Geometries.Length; i++)
            {
                var point = multiPoint.Geometries[i];

                var coordinate = point.Coordinate;

                vX.Add(coordinate.X);
                vY.Add(coordinate.Y);

                nvertices += 1;
                if(i + 1 != multiPoint.Geometries.Length)
                    apartStart.Add(nvertices);
            }

            return ShapeLib.SHPCreateObject(ShapeType.MultiPoint, iShape, apartStart.Count, apartStart.ToArray(), null, nvertices, vX.ToArray(), vY.ToArray(), null, null);

            //var xArray = multiPoint.Geometries.Select(x => x.Coordinate.X).ToArray();
            //var yArray = multiPoint.Geometries.Select(x => x.Coordinate.Y).ToArray();

            //return ShapeLib.SHPCreateObject(ShapeType.Point, iShape, 0, null, null, xArray.Length, xArray, yArray, null, null);
        }
        private IntPtr WriteShpGeometryField(int iShape, Point point)
        {
            var xArray = new[] { point.Coordinate.X };
            var yArray = new[] { point.Coordinate.Y };

            return ShapeLib.SHPCreateObject(ShapeType.Point, iShape, 0, null, null, xArray.Length, xArray, yArray, null, null);
        }

        protected IntPtr WriteShpGeometryField(int iShape, Geometry geometry)
        {
            if(geometry.GeometryType == GeometryType.MULTIPOLYGON)
            {
                //https://sharpmap.codeplex.com/discussions/347029
                return WriteShpGeometryField(iShape, (MultiPolygon) geometry);
            }

            if (geometry.GeometryType == GeometryType.POLYGON)
            {
                return WriteShpGeometryField(iShape, (Polygon) geometry);
            }

            if(geometry.GeometryType == GeometryType.MULTILINESTRING)
            {
                return WriteShpGeometryField(iShape, (MultiLineString)geometry);
            }

            if(geometry.GeometryType == GeometryType.LINESTRING)
            {
                return WriteShpGeometryField(iShape, (LineString)geometry);
            }

            if(geometry.GeometryType == GeometryType.MULTIPOINT)
            {
                return WriteShpGeometryField(iShape, (MultiPoint)geometry);
            }

            if(geometry.GeometryType == GeometryType.POINT)
            {
                return WriteShpGeometryField(iShape, (Point)geometry);
            }

            throw new NotImplementedException("No WriteShpGeometryField defined for GeometryType." + geometry.GeometryType);
        }
    }
}