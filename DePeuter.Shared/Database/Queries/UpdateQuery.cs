using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DePeuter.Shared.Database.Queries
{
    public class UpdateQuery : BaseInsertUpdateDeleteQuery
    {
        private readonly string _schema;
        private readonly string _table;
        private readonly string _primaryKeyField;
        private readonly object _primaryKeyValue;
        private readonly Dictionary<string, object> _fieldValues;

        public UpdateQuery(string schema, string table, string primaryKeyField, object primaryKeyValue, Dictionary<string, object> fieldValues)
        {
            _schema = schema;
            _table = table;
            _primaryKeyField = primaryKeyField;
            _primaryKeyValue = primaryKeyValue;
            _fieldValues = fieldValues;
        }

        public override void BuildQuery(IDatabaseProvider provider)
        {
            if(_fieldValues == null || !_fieldValues.Any())
            {
                return;
            }

            var primaryKeyValueParameter = "PrimaryKeyValue";

            Builder.AppendFormat("UPDATE {0} SET {1} WHERE {2} = {3};", provider.CombineSchemaAndTable(_schema, _table), _fieldValues.Select(x => string.Format("{0} = {1}", x.Key, provider.CreateParameterName(x.Key.Replace("\"", "").Replace(" ", "_")))).Join(", "), _primaryKeyField, provider.CreateParameterName(primaryKeyValueParameter));

            Parameters.Add(primaryKeyValueParameter, _primaryKeyValue);

            foreach(var x in _fieldValues)
            {
                Parameters.Add(x.Key.Replace("\"", "").Replace(" ", "_"), x.Value);
            }
        }
    }
}
