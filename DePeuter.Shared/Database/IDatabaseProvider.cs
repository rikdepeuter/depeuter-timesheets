using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data;

public interface IDatabaseProvider
{
    DbConnection CreateConnection(string connectionstring);
    IDbCommand CreateCommand(DbConnection connection, IQueryObject queryObject);
    DataSet FillDataSet(IDbCommand command);

    string WildCard { get; }
    string StringConcatenation { get; }
    
    string CombineSchemaAndTable(string schema, string table);
    string CreateParameterName(string parameter);

    IQueryObject GetNewQueryBeforeInsertToReturnId(IQueryObject queryObject, string pkField);
    IQueryObject GetQueryToGetIdAfterInsert(string database, string schema, string table);
    IQueryObject GetTableColumnsQuery(string database, string schema, string tableName);

    void Limit(IQueryObject queryObject, int? offset, int? rowcount);
}