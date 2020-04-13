using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DePeuter.Shared.Spatial
{
    public class SqlGeometryFieldAttribute : SqlFieldAttribute
    {
        private readonly bool _multi;
        private readonly bool _makeValid;

        public SqlGeometryFieldAttribute(bool multi = false, bool makeValid = false)
            : this(null, multi, makeValid)
        {
        }

        public SqlGeometryFieldAttribute(string fieldName, bool multi = false, bool makeValid = false)
            : base(fieldName)
        {
            _multi = multi;
            _makeValid = makeValid;
        }

        public override string GetSql(SqlFieldSelectParameters parameters)
        {
            return string.Format("public.st_astext({0}.{1}) as {1}", parameters.TableAlias, parameters.FieldName);
        }

        public override string GetSql(SqlFieldInsertParameters parameters)
        {
            return GetNewSqlInsertUpdate(parameters);
        }

        public override string GetSql(SqlFieldUpdateParameters parameters)
        {
            return GetNewSqlInsertUpdate(parameters);
        }

        private string GetNewSqlInsertUpdate(SqlFieldInsertUpdateParameters parameters)
        {
            if(_multi)
            {
                if(_makeValid)
                {
                    return "public.st_multi(public.st_makevalid(public.st_geometryfromtext(" + parameters.ParameterName + ")))";
                }
                else
                {
                    return "public.st_multi(public.st_geometryfromtext(" + parameters.ParameterName + "))";
                }
            }
            else
            {
                if(_makeValid)
                {
                    return "public.st_makevalid(public.st_geometryfromtext(" + parameters.ParameterName + "))";
                }
                else
                {
                    return "public.st_geometryfromtext(" + parameters.ParameterName + ")";
                }
            }
        }
    }
}
