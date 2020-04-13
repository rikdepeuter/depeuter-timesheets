using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Data.SqlClient;
using DePeuter.Shared;
using DePeuter.Shared.Database.Queries;

public class MSSQLProviderAttribute : BaseDatabaseProviderAttribute
{
    public override string ProviderName
    {
        get { return "System.Data.SqlClient"; }
    }

    public override Type ProviderType
    {
        get { return typeof (MSSQLProvider); }
    }
}

[MSSQLProvider]
public class MSSQLProvider : IDatabaseProvider
{
    public bool ReturnIdOnInsert { get { return true; } }

    public string WildCard { get { return "%"; } }
    public string SingleQuoteReplace { get { return "'"; } }
    public string StringConcatenation { get { return "+"; } }

    public DbConnection CreateConnection(string connectionstring)
    {
        return new SqlConnection(connectionstring);
    }

    IDbCommand IDatabaseProvider.CreateCommand(DbConnection connection, IQueryObject queryObject)
    {
        return CreateCommand(connection, queryObject);
    }

    public DbCommand CreateCommand(DbConnection connection, IQueryObject queryObject)
    {
        var query = queryObject.Query.ToString();
        var cmd = new SqlCommand(query, (SqlConnection)connection);

        if(queryObject.Parameters != null)
        {
            foreach(var item in queryObject.Parameters)
            {
                cmd.Parameters.AddWithValue(item.Key, item.Value ?? DBNull.Value);
            }
        }

        return cmd;
    }

    public DataSet FillDataSet(IDbCommand command)
    {
        var da = new SqlDataAdapter((SqlCommand)command);
        var ds = new DataSet();
        da.Fill(ds);

        return ds;
    }

    public string CombineSchemaAndTable(string schema, string table)
    {
        return schema + "." + table;
    }

    public string CreateParameterName(string parameter)
    {
        if(parameter == null) throw new ArgumentNullException("parameter");
        return "@" + parameter.Replace("\"", "");
    }

    public IQueryObject GetNewQueryBeforeInsertToReturnId(IQueryObject queryObject, string pkField)
    {
        return queryObject.AppendLine(";SELECT SCOPE_IDENTITY();");
    }

    public IQueryObject GetQueryToGetIdAfterInsert(string database, string schema, string table)
    {
        return null;
    }

    public IQueryObject GetTableColumnsQuery(string database, string schema, string tableName)
    {
        return new QueryObject().Append(string.Format("select column_name as name,data_type as type,(case when is_nullable = 'NO' then 1 else 0 end),column_default from information_schema.columns where upper(table_catalog) = upper('{0}') and upper(table_schema) = upper('{1}') and upper(table_name) = upper('{2}')", database, schema, tableName));
    }

    public Func<DataRow, TableColumn> GetColumnsSelector(string database, string schema, string tableName)
    {
        return (DataRow x) => new TableColumn()
        {
            Name = x["column_name"].ToString(),
            Type = x["data_type"].ToString(),
            NotNull = x["is_nullable"].ToString() != "YES",
            DefaultValue = x["column_default"].ToString()
        };
    }

    public string OrderByClausule<T, TKey>(Expression<Func<T, TKey>> expression, OrderDirection direction)
    {
        throw new NotImplementedException();
    }

    public void Limit(IQueryObject queryObject, int? offset, int? rowcount)
    {
        if(queryObject.Builder.Length == 0)
        {
            return;
        }

        //if(offset != null)
        //{
        //    if(queryObject.Builder[queryObject.Builder.Length - 1] != '\n')
        //    {
        //        queryObject.AppendLine();
        //    }
        //    queryObject.AppendLine("OFFSET " + offset);
        //}

        if(rowcount != null)
        {
            var query = queryObject.Builder.ToString();
            queryObject.Builder.Clear();
            queryObject.AppendLine(string.Format("SELECT TOP {0} * FROM ({1}) mssqlprovider", rowcount, query));
        }
    }
}

[MSSQLProvider]
public abstract class BaseMSSQLService : BaseDatabaseService
{
    protected const string DatabaseProviderName = "System.Data.SqlClient";

    protected BaseMSSQLService()
        : base()
    {
    }

    protected BaseMSSQLService(ConnectionStringParameter connectionstring)
        : base(connectionstring)
    {
    }
}