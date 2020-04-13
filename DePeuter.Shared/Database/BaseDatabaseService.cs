using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data;
using System.Configuration;
using System.Reflection;
using System.Linq.Expressions;
using System.Security.Principal;
using DePeuter.Shared;
using DePeuter.Shared.Cache;
using DePeuter.Shared.Database;
using DePeuter.Shared.Database.Queries;

public interface IBaseDatabaseService : IBaseService, IDataParameters
{
    IQueryObject NewQueryObject();
    IQueryObject NewQueryObject(string query, params object[] args);

    BaseDatabaseService.Transaction StartTransaction(IsolationLevel? isolationLevel = null);
    void CommitTransaction();
    void CancelTransaction();

    List<TableColumn> GetColumns(string schema, string tableName);
    List<TableColumn> GetColumns(string database, string schema, string tableName);

    //DataSet FillDataSet(IQueryObject queryObject);

    //DataTable FillDataTable(IQueryObject queryObject);

    //int ExecuteNonQuery(IQueryObject queryObject);

    //T ExecuteScalar<T>(IQueryObject queryObject);
    //object ExecuteScalar(IQueryObject queryObject);

    //T GetEntity<T>(IQueryObject queryObject);
    //T GetEntity<T>(IQueryObject queryObject, Func<DataRow, T> selector);
    T GetEntityById<T>(object pkValue);
    object GetEntityById(Type type, object pkValue);
    //object GetEntity(Type type, IQueryObject queryObject);
    //T GetEntityByWhere<T>(IQueryObject whereQueryObject, int? skip = null);
    //object GetEntityByWhere(Type type, IQueryObject whereQueryObject, int? skip = null);

    List<T> GetEntities<T>();
    //List<T> GetEntities<T>(string orderBy, int? skip, int? take);
    //List<T> GetEntities<T, TKey>(Expression<Func<T, TKey>> orderBy, OrderDirection direction, int? skip = null, int? take = null);
    //List<T> GetEntities<T>(IQueryObject queryObject);
    //List<T> GetEntities<T>(IQueryObject queryObject, Func<DataRow, T> selector);
    List<T> GetEntitiesByIds<T>(IEnumerable<object> pkValues);
    //List<object> GetEntities(Type type, IQueryObject queryObject);
    //List<T> GetEntitiesByWhere<T>(IQueryObject whereQueryObject, string orderBy = null, int? skip = null, int? take = null);
    //List<object> GetEntitiesByWhere(Type type, IQueryObject whereQueryObject, string orderBy = null, int? skip = null, int? take = null);

    void Truncate<T>(bool cascade = false) where T : class;
    void Truncate(Type type, bool cascade = false);

    void Delete<T>(params T[] entities) where T : class;
    void Delete(Type type, object entity);
    void Delete<T>(int pkValue) where T : class;
    void Delete(Type type, int pkValue);
    void DeleteById<T>(object pkValue) where T : class;
    void DeleteById(Type type, object pkValue);
    //void DeleteByWhere<T>(IQueryObject queryObject) where T : class;
    //void DeleteByWhere(Type type, IQueryObject queryObject);

    int DeleteSqlById<T>(object pkValue) where T : class;
    int DeleteSqlById(Type type, object pkValue);
    //int DeleteSqlByWhere<T>(IQueryObject queryObject) where T : class;
    //int DeleteSqlByWhere(Type type, IQueryObject queryObject);

    object Save<T>(T entity) where T : class;
    object Save(Type type, object entity);
    object Insert<T>(T entity) where T : class;
    object Insert(Type type, object entity);
    object Update<T>(T entity) where T : class;
    object Update(Type type, object entity);

    bool Exists(IQueryObject queryObject);
    bool Exists<T>(T item) where T : class;
    bool Exists(Type type, object entity);
    bool ExistsById<T>(object pkValue) where T : class;
    bool ExistsById(Type type, object pkValue);
    //bool ExistsByWhere<T>(IQueryObject queryObject) where T : class;
    //bool ExistsByWhere(Type type, IQueryObject queryObject);

    void ClearCache();
}

public class ResolvingProviderAndConnectionStringEventArgs : EventArgs
{
    public string ProviderInvariantName;
    public string ConnectionString;
}

public abstract class BaseDatabaseService : BaseService, IBaseDatabaseService
{
    public DataParameters Data { get; set; }

    protected DbConnection Connection;
    protected IDatabaseProvider DatabaseProvider;
    private Transaction _transaction;

    protected virtual bool OpenCloseConnection
    {
        get
        {
            return false;
        }
    }

    protected virtual bool CacheEnabled
    {
        get
        {
            return false;
        }
    }

    private int _disableCloseConnectionCounter;
    protected bool CloseConnectionIsDisabled
    {
        get { return _disableCloseConnectionCounter > 0; }
    }
    protected void DisableCloseConnection()
    {
        _disableCloseConnectionCounter++;
    }
    protected void EnableCloseConnection()
    {
        _disableCloseConnectionCounter--;
        if(_disableCloseConnectionCounter == 0)
        {
            Close();
        }
    }
    
    protected string ProviderName { get; private set; }
    protected IDbCommand LastExecutedCommand { get; private set; }

    #region "Static"

    private static readonly object Lock = new object();

    public static event EventHandler<ResolvingProviderAndConnectionStringEventArgs> ResolvingProviderAndConnectionString;

    #endregion

    #region "Constructors"

    protected BaseDatabaseService()
    {
        IninitializeConnectionParameters();
    }

    protected BaseDatabaseService(ConnectionStringParameter connectionstring)
    {
        var type = GetType();
        var provider = type.GetCustomAttribute<BaseDatabaseProviderAttribute>(true);

        if(provider == null)
        {
            throw new MissingProviderAttributeException(type);
        }

        CreateConnection(provider.ProviderName, connectionstring.Value);
    }

    protected BaseDatabaseService(ProviderParameter provider, ConnectionStringParameter connectionstring)
    {
        CreateConnection(provider.Value, connectionstring.Value);
    }

    protected BaseDatabaseService(BaseDatabaseService databaseService)
    {
        LoadDatabaseProvider(databaseService.ProviderName);
        LoadConnection(databaseService.Connection);

        DisableCloseConnection();
    }

    #endregion

    #region "Private"

    private void IninitializeConnectionParameters()
    {
        ResolveDefaultConnection();
    }
    protected virtual void ResolveDefaultConnection()
    {
        string providerInvariantName, connectionString;

        if(ResolvingProviderAndConnectionString != null)
        {
            var e = new ResolvingProviderAndConnectionStringEventArgs();
            ResolvingProviderAndConnectionString(this, e);
            providerInvariantName = e.ProviderInvariantName;
            connectionString = e.ConnectionString;

            if(providerInvariantName == null)
            {
                throw new NullReferenceException("providerInvariantName");
            }

            if(connectionString == null)
            {
                throw new NullReferenceException("connectionString");
            }
        }
        else
        {
            ResolveProviderAndConnectionString(out providerInvariantName, out connectionString);
        }

        CreateConnection(providerInvariantName, connectionString);
    }

    private void LoadDatabaseProvider(string providerInvariantName)
    {
        ProviderName = providerInvariantName;
        DatabaseProvider = DatabaseProviders.GetDatabaseProvider(providerInvariantName);
    }
    private void LoadConnection(DbConnection connection)
    {
        Connection = connection;
        if(!OpenCloseConnection)
        {
            OpenConnection();
        }
    }

    protected void CreateConnection(string providerInvariantName, string connectionstring)
    {
        LoadDatabaseProvider(providerInvariantName);

        //var dataFactory = System.Data.Common.DbProviderFactories.GetFactory(providerInvariantName);
        //connection = dataFactory.CreateConnection();
        //connection.ConnectionString = connectionstring;

        LoadConnection(DatabaseProvider.CreateConnection(connectionstring));
    }

    protected virtual void OpenConnection()
    {
        if(Connection.State == ConnectionState.Open)
        {
            return;
        }

        Connection.Open();
    }
    protected virtual void CloseConnection()
    {
        if(Connection.State == ConnectionState.Closed)
        {
            return;
        }

        Connection.Close();
    }

    protected virtual object GetRegistryValue(string key)
    {
        return DePeuterRegistry.Get(key);
    }

    public static void FindProviderAndConnectionString(Type databaseServiceType, out string providerInvariantName, out string connectionString)
    {
        //cascading
        var databaseProvider = databaseServiceType.GetCustomAttribute<BaseDatabaseProviderAttribute>() ?? databaseServiceType.GetCustomAttribute<BaseDatabaseProviderAttribute>(true);
        var connectionStringName = databaseServiceType.GetCustomAttribute<ConnectionStringNameAttribute>() ?? databaseServiceType.GetCustomAttribute<ConnectionStringNameAttribute>(true);
        
        DatabaseProviders.RegisterProvider(databaseProvider);

        if(connectionStringName != null)
        {
            var connString = ConfigurationManager.ConnectionStrings[connectionStringName.Name];
            if(connString == null)
            {
                connectionString = DePeuterRegistry.Get(connectionStringName.Name) as string;
                if(connectionString == null)
                {
                    throw new MissingConnectionStringException(connectionStringName.Name);
                }

                if(databaseProvider == null)
                {
                    throw new MissingProviderAttributeException(databaseServiceType);
                }

                providerInvariantName = databaseProvider.ProviderName;
            }
            else
            {
                if(connString.ProviderName == string.Empty)
                {
                    throw new Exception(string.Format("ConnectionString '{0}' has no Provider", connString.Name));
                }

                if(connString.ConnectionString == string.Empty)
                {
                    throw new Exception(string.Format("ConnectionString '{0}' has no ConnectionString", connString.Name));
                }

                providerInvariantName = connString.ProviderName;
                connectionString = connString.ConnectionString;
            }
            return;
        }

        if(databaseProvider == null)
        {
            throw new MissingProviderAttributeException(databaseServiceType);
        }

        providerInvariantName = databaseProvider.ProviderName;

        var registryKey = databaseServiceType.GetCustomAttribute<RegistryKeyAttribute>(true);
        if(registryKey != null)
        {
            // registry specific
            var registryConnectionString1 = DePeuterRegistry.Get(registryKey.Key);
            if(registryConnectionString1 != null)
            {
                connectionString = (string)registryConnectionString1;
                return;
            }
        }

        // registry provider
        /*var registryConnectionString = DePeuterRegistry.Get(providerInvariantName);
        if (registryConnectionString != null)
        {
            connectionString = registryConnectionString;
            return;
        }*/

        // appconfig
        if(ConfigurationManager.ConnectionStrings.Count == 0)
        {
            throw new Exception("There are no connectionstrings configured");
        }

        var connectionStrings = ConfigurationManager.ConnectionStrings.Cast<ConnectionStringSettings>().Where(x => string.Equals(x.ProviderName, databaseProvider.ProviderName, StringComparison.InvariantCultureIgnoreCase)).ToArray();
        if(connectionStrings.Length == 0)
        {
            throw new Exception("There are no connectionstrings configured with the provider '" + providerInvariantName + "'");
        }

        if(connectionStrings.Length == 1)
        {
            connectionString = connectionStrings.First().ConnectionString;
            return;
        }

        throw new Exception("There are multiple connectionstrings configured in the app.config, but there is no ConnectionStringNameAttribute set for type '" + databaseServiceType.Name + "'");
    }

    protected virtual void ResolveProviderAndConnectionString(out string providerInvariantName, out string connectionString)
    {
        FindProviderAndConnectionString(GetType(), out providerInvariantName, out connectionString);
    }

    protected IDbCommand CreateCommand()
    {
        var cmd = DatabaseProvider.CreateCommand(Connection, null);
        if(_transaction != null)
        {
            cmd.Transaction = _transaction.Value;
        }
        return cmd;
    }
    protected IDbCommand CreateCommand(IQueryObject queryObject)
    {
        var cmd = DatabaseProvider.CreateCommand(Connection, queryObject);
        if(_transaction != null)
        {
            cmd.Transaction = _transaction.Value;
        }
        return cmd;
    }

    #endregion


    #region "Basic functions"

    private T CastTo<T>(object value, IDbCommand cmd)
    {
        return CastTo<T>(value, cmd.ToQueryObject());
    }
    private T CastTo<T>(object value, IQueryObject queryObject)
    {
        if(value == null || value is DBNull) return default(T);

        try
        {
            return (T)value;
        }
        catch
        {
            Logger.Error(Logger.GetSqlLog(queryObject));

            throw new InvalidCastException(string.Format("Failed to cast value '{0}' of type '{1}' to type '{2}'", value, value.GetType().FullName, typeof(T).FullName));
        }
    }

    private T ExecuteCommand<T>(IDbCommand cmd, Func<T> action)
    {
        LastExecutedCommand = cmd;

        try
        {
            if(cmd != null)
            {
                Logger.Sql(cmd);
            }

            var sw = Stopwatch.StartNew();

            OpenConnection();

            try
            {
                var res = action();

                Logger.Debug(sw.ElapsedMilliseconds);

                return res;
            }
            finally
            {
                if(OpenCloseConnection && !CloseConnectionIsDisabled)
                {
                    CloseConnection();
                }
            }
        }
        catch(Exception ex)
        {
            if(cmd != null)
            {
                Logger.Error(string.Format("Query: {0}", Logger.GetSqlLog(cmd)), ex);
            }
            throw;
        }
    }

    protected DataSet OnFillDataSet(IQueryObject queryObject)
    {
        return OnFillDataSet(CreateCommand(queryObject));
    }
    protected virtual DataSet OnFillDataSet(IDbCommand cmd)
    {
        return DatabaseProvider.FillDataSet(cmd);
    }
    protected DataSet FillDataSet(IQueryObject queryObject)
    {
        return FillDataSet(CreateCommand(queryObject));
    }
    protected DataSet FillDataSet(IDbCommand cmd)
    {
        if(CacheEnabled)
        {
            var cacheKey = CreateCacheKey(cmd, typeof(DataSet));

            var cacheValue = GetCacheValue(cacheKey);
            if(cacheValue != null)
            {
                //Logger.Debug("Getting data from cache");
                return (DataSet)cacheValue;
            }
        }

        var res = ExecuteCommand(cmd, () => OnFillDataSet(cmd));

        if(CacheEnabled)
        {
            var cacheKey = CreateCacheKey(cmd, typeof(DataSet));

            SetCacheValue(cacheKey, res);
        }

        return res;
    }

    protected IDataReader OnExecuteReader(IQueryObject queryObject)
    {
        return OnExecuteReader(CreateCommand(queryObject));
    }
    protected virtual IDataReader OnExecuteReader(IDbCommand cmd)
    {
        return cmd.ExecuteReader();
    }
    protected IDataReader ExecuteReader(IQueryObject queryObject)
    {
        return ExecuteReader(CreateCommand(queryObject));
    }
    protected IDataReader ExecuteReader(IDbCommand cmd)
    {
        if(OpenCloseConnection)
        {
            throw new NotSupportedException("Not supported when OpenCloseConnection is true");
        }

        return ExecuteCommand(cmd, () => OnExecuteReader(cmd));
    }

    protected virtual int OnExecuteFile(string fileName)
    {
        var cmd = CreateCommand();
        return cmd.ExecuteFile(fileName);
    }
    protected int ExecuteFile(string fileName)
    {
        Logger.Sql(fileName);

        return ExecuteCommand(null, () => OnExecuteFile(fileName));
    }

    protected virtual DataTable FillDataTable(IQueryObject queryObject)
    {
        return FillDataTable(CreateCommand(queryObject));
    }
    protected virtual DataTable FillDataTable(IDbCommand cmd)
    {
        return FillDataSet(cmd).Tables[0];
    }

    protected int ExecuteNonQuery(IQueryObject queryObject)
    {
        queryObject.BuildQuery(DatabaseProvider);
        return ExecuteNonQuery(CreateCommand(queryObject));
    }
    protected int ExecuteNonQuery(IDbCommand cmd)
    {
        return ExecuteCommand(cmd, () => OnExecuteNonQuery(cmd));
    }
    protected virtual int OnExecuteNonQuery(IDbCommand cmd)
    {
        return cmd.ExecuteNonQuery();
    }

    protected T ExecuteScalar<T>(IQueryObject queryObject)
    {
        var res = ExecuteScalar(queryObject);
        return CastTo<T>(res, queryObject);
    }
    protected T ExecuteScalar<T>(IDbCommand cmd)
    {
        var res = ExecuteScalar(cmd);
        return CastTo<T>(res, cmd);
    }
    protected T ExecuteScalar<T>(IQueryObject queryObject, Func<object, T> selector)
    {
        var res = ExecuteScalar(queryObject);
        if(res == null || res is DBNull) return default(T);
        return selector(res);
    }
    protected object ExecuteScalar(IQueryObject queryObject)
    {
        queryObject.BuildQuery(DatabaseProvider);
        return ExecuteScalar(CreateCommand(queryObject));
    }
    protected object ExecuteScalar(IDbCommand cmd)
    {
        if(CacheEnabled)
        {
            var cacheKey = CreateCacheKey(cmd, typeof(object));

            var cacheValue = GetCacheValue(cacheKey);
            if(cacheValue != null)
            {
                //Logger.Debug("Getting data from cache");
                return cacheValue;
            }
        }

        var res = ExecuteCommand(cmd, () => OnExecuteScalar(cmd));

        if(CacheEnabled)
        {
            var cacheKey = CreateCacheKey(cmd, typeof(object));

            SetCacheValue(cacheKey, res);
        }

        return res;
    }
    protected virtual object OnExecuteScalar(IDbCommand cmd)
    {
        return cmd.ExecuteScalar();
    }
    #endregion

    #region "Protected"
    private string CreateCacheKey(IDbCommand cmd, Type dataType)
    {
        var query = cmd.CommandText;
        if(cmd.Parameters != null)
        {
            return dataType.FullName + query + cmd.Parameters.Cast<IDbDataParameter>().Select(x => string.Format(CultureInfo.InvariantCulture, "{0}={1}", x.ParameterName, x.Value)).Join(",");
        }
        return dataType.FullName + query;
    }

    protected static PropertyInfo GetPropertyInfo(Type type, string name)
    {
        var pi = type.GetProperty(name);
        if(pi == null)
        {
            throw new Exception(string.Format("Missing public property '{0}' on entity '{1}'", name, type.FullName));
        }
        return pi;
    }
    private static readonly Dictionary<Type, PropertyInfo> TypePrimaryKeyProperties = new Dictionary<Type, PropertyInfo>();
    protected static PropertyInfo GetPrimaryKeyPropertyInfo(Type type)
    {
        if(TypePrimaryKeyProperties.ContainsKey(type))
        {
            return TypePrimaryKeyProperties[type];
        }

        lock(Lock)
        {
            var primaryKeyName = EF.GetSqlPrimaryKeyName(type);
            var fieldNamingType = EF.GetSqlFieldNamingType(type);

            foreach(var pi in type.GetProperties().Where(pi => pi.CanWrite && pi.CanRead && pi.GetGetMethod(true).IsPublic))
            {
                var fieldName = EF.GetFieldName(pi, fieldNamingType);

                if(string.IsNullOrEmpty(fieldName))
                {
                    continue;
                }

                if(string.Equals(fieldName, primaryKeyName, StringComparison.InvariantCultureIgnoreCase))
                {
                    TypePrimaryKeyProperties.Add(type, pi);
                    return TypePrimaryKeyProperties[type];
                }
            }
        }

        throw new InvalidOperationException(string.Format("Type '{0}' is missing a property that is the PrimaryKey", type));
    }

    protected string CreateParameterName(string parameter)
    {
        return DatabaseProvider.CreateParameterName(parameter);
    }

    protected override void Close()
    {
        if(Connection != null && !CloseConnectionIsDisabled)
        {
            CancelTransaction();
            CloseConnection();
        }
    }

    public virtual List<TableColumn> GetColumns(string database, string schema, string tableName)
    {
        var query = DatabaseProvider.GetTableColumnsQuery(database, schema, tableName);

        return GetEntities<TableColumn>(query).Where(x => !string.IsNullOrEmpty(x.Name)).ToList();
    }

    public List<TableColumn> GetColumns(string schema, string tableName)
    {
        return GetColumns(Connection.Database, schema, tableName);
    }

    protected string WildCard { get { return DatabaseProvider.WildCard; } }
    protected string StringConcatenation { get { return DatabaseProvider.StringConcatenation; } }

    #endregion

    #region "Public"

    protected List<MetaDataItem> GetMetaDataList(IQueryObject queryObject, bool addEmptyAtBegin = false)
    {
        var result = GetEntities(queryObject, x => new MetaDataItem() { Value = int.Parse(x[0].ToString()), Display = x[1].ToString() });

        if(addEmptyAtBegin)
            result.Insert(0, MetaDataItem.CreateNullItem());

        return result;
    }

    protected List<string> GetStringList(IQueryObject queryObject)
    {
        return GetEntities(queryObject, x => x[0].ToString());
    }

    protected string GetString(IQueryObject queryObject)
    {
        return GetStringList(queryObject).FirstOrDefault();
    }

    public Transaction StartTransaction(IsolationLevel? isolationLevel = null)
    {
        if(_transaction != null)
        {
            throw new InvalidOperationException("Can't start a new Transaction when one is active");
        }

        OpenConnection();

        _transaction = new Transaction(isolationLevel == null ? Connection.BeginTransaction() : Connection.BeginTransaction(isolationLevel.Value));
        _transaction.Committed += _transaction_Committed;
        _transaction.Cancelled += _transaction_Cancelled;
        return _transaction;
    }

    private void _transaction_Committed(object sender, EventArgs e)
    {
        _transaction = null;
    }
    void _transaction_Cancelled(object sender, EventArgs e)
    {
        _transaction = null;
    }

    public void CommitTransaction()
    {
        if(_transaction != null)
        {
            _transaction.Commit();
        }
    }
    public void CancelTransaction()
    {
        if(_transaction != null)
        {
            _transaction.Cancel();
        }
    }

    #endregion

    #region "Entities"

    protected string CombineSchemaAndTable(string schema, string table)
    {
        return DatabaseProvider.CombineSchemaAndTable(schema, table);
    }

    protected IQueryObject GenerateInsertQuery(string schema, string table, string[] fields, string[] parameterNames)
    {
        var queryObject = NewQueryObject();
        queryObject.Append(string.Format("INSERT INTO {0} ({1}) VALUES ({2})", CombineSchemaAndTable(schema, table), string.Join(",", fields), string.Join(",", parameterNames)));
        return queryObject;
    }
    protected IQueryObject GenerateUpdateQuery(string schema, string table, string[] propertyNamesAndValues, string pkField, string pkParameter)
    {
        var queryObject = NewQueryObject();
        queryObject.Append(string.Format("UPDATE {0} SET {1} WHERE {2} = {3}", CombineSchemaAndTable(schema, table), string.Join(",", propertyNamesAndValues), pkField, pkParameter));
        return queryObject;
    }

    protected T GetEntity<T>(IQueryObject queryObject)
    {
        return GetEntities<T>(queryObject).FirstOrDefault();
    }
    protected T GetEntity<T>(IQueryObject queryObject, Func<DataRow, T> selector)
    {
        return GetEntities(queryObject, selector).FirstOrDefault();
    }
    protected object GetEntity(Type type, IQueryObject queryObject)
    {
        return GetEntities(type, queryObject).FirstOrDefault();
    }
    protected T GetEntityByWhere<T>(IQueryObject queryObject, int? skip = null)
    {
        return GetEntitiesByWhere<T>(queryObject, null, skip, 1).FirstOrDefault();
    }
    protected object GetEntityByWhere(Type type, IQueryObject queryObject, int? skip = null)
    {
        return GetEntitiesByWhere(type, queryObject, null, skip, 1).FirstOrDefault();
    }

    public T GetEntityById<T>(object pkValue)
    {
        return GetEntitiesByIds<T>(new List<object>() { pkValue }).FirstOrDefault();
    }
    public object GetEntityById(Type type, object pkValue)
    {
        return GetEntitiesByIds(type, new List<object>() { pkValue }).FirstOrDefault();
    }
    public List<T> GetEntitiesByIds<T>(IEnumerable<object> pkValues)
    {
        return GetEntitiesByIds(typeof(T), pkValues).Select(x => x.CastTo<T>()).ToList();
    }
    public List<object> GetEntitiesByIds(Type type, IEnumerable<object> pkValues)
    {
        var primaryKeyName = EF.GetSqlPrimaryKeyName(type);

        var query = NewQueryObject();
        query.AppendLine(string.Format("{0} IN (", primaryKeyName));
        
        var pkValuesArr = pkValues.ToArray();
        for(var i = 0; i < pkValuesArr.Length; i++)
        {
            query.AddParameter("id" + i, pkValuesArr[i]);
            query.Append(CreateParameterName("id" + i));
            if(i < pkValuesArr.Length - 1)
            {
                query.Append(",");
            }
        }
        query.Append(")");

        return GetEntitiesByWhere(type, query, take: pkValuesArr.Length);
    }

    public List<T> GetEntities<T>()
    {
        return GetEntities<T>(null, null, null);
    }
    public List<object> GetEntities(Type type)
    {
        return GetEntities(type, null, null, null);
    }
    protected List<T> GetEntities<T, TKey>(Expression<Func<T, TKey>> orderBy, OrderDirection direction, int? skip = null, int? take = null)
    {
        var orderByClausule = OrderByClausule(orderBy, direction);
        return GetEntities<T>(orderByClausule, skip, take);
    }

    private string OrderByClausule<T, TKey>(Expression<Func<T, TKey>> expression, OrderDirection direction)
    {
        var analyzeResult = Analyzer.AnalyzeSelect((MemberExpression)expression.Body);

        return analyzeResult + (direction == OrderDirection.Ascending ? " ASC" : " DESC");
    }

    protected List<T> GetEntities<T>(string orderBy, int? skip, int? take)
    {
        return GetEntities(typeof(T), orderBy, skip, take).Select(x => x.CastTo<T>()).ToList();
    }
    protected List<object> GetEntities(Type type, string orderBy, int? skip, int? take)
    {
        var tableInfo = EF.GetSqlSchemaTableInfo(type);

        var queryObject = NewQueryObject();

        queryObject.AppendLine("SELECT " + GetSelectFields(type, tableInfo.Schema, tableInfo.Table).Join(","));
        queryObject.AppendLine("FROM " + CombineSchemaAndTable(tableInfo.Schema, tableInfo.Table));

        if(orderBy != null)
        {
            queryObject.AppendLine("ORDER BY " + orderBy);
        }

        DatabaseProvider.Limit(queryObject, skip, take);
        
        return GetEntities(type, queryObject);
    }

    protected List<T> GetEntities<T>(IQueryObject queryObject)
    {
        queryObject.BuildQuery(DatabaseProvider);
        return GetEntities<T>(CreateCommand(queryObject));
    }
    protected List<T> GetEntities<T>(IDbCommand cmd)
    {
        return GetEntities(typeof(T), cmd).Select(x => x.CastTo<T>()).ToList();
    }

    protected IEnumerable<string> GetSelectFields(Type type)
    {
        return GetSelectFields(type, null, null);
    }

    protected IEnumerable<string> GetSelectFields(Type type, string tableAlias)
    {
        return GetSelectFields(type, tableAlias, null, null);
    }

    protected IEnumerable<string> GetSelectFields(Type type, string schema, string table)
    {
        return GetSelectFields(type, null, schema, table);
    }

    protected IEnumerable<string> GetSelectFields(Type type, string tableAlias, string schema, string table)
    {
        var fields = new List<string>();

        var properties = EF.GetTypeSelectProperties(type);

        if(table == null)
        {
            var tableInfo = EF.GetSqlSchemaTableInfo(type);

            table = tableInfo.Table;
            schema = tableInfo.Schema;
        }

        var fieldNamingType = EF.GetSqlFieldNamingType(type);

        foreach(var pi in properties)
        {
            if(pi.HasCustomAttribute<SqlEntityAttribute>())
            {
                fields.AddRange(GetSelectFields(pi.PropertyType, tableAlias, schema, table));
            }
            else
            {
                //field
                var fieldName = GetSqlSelectFieldName(type, tableAlias, schema, table, pi, fieldNamingType);
                if (fieldName == null)
                {
                    continue;
                }

                if(fields.Contains(fieldName))
                {
                    throw new Exception(string.Format("Property '{0}' in type '{1}' already exists in a parent type", pi.Name, type.FullName));
                }

                fields.Add(fieldName);
            }
        }

        return fields;
    }

    protected string GetSqlSelectFieldName(Type type, string tableAlias, string schema, string table, PropertyInfo pi, FieldNamingType? fieldNamingType = null)
    {
        if(tableAlias == null)
        {
            if(table == null)
            {
                throw new ArgumentNullException("table");
            }

            tableAlias = CombineSchemaAndTable(schema, table);
        }

        var fieldName = EF.GetFieldName(pi, fieldNamingType);

        var attr = pi.GetCustomAttribute<SqlFieldAttribute>();
        var fieldParameters = new SqlFieldSelectParameters(Data, DatabaseProvider, type, pi, fieldName, tableAlias);
        var sql = attr.GetSql(fieldParameters);

        if(fieldParameters.Cancel)
        {
            return null;
        }

        if(sql != null)
        {
            return sql;
        }

        return tableAlias + "." + fieldName;
    }

    protected List<object> GetEntities(Type type, IQueryObject queryObject)
    {
        queryObject.BuildQuery(DatabaseProvider);
        return GetEntities(type, CreateCommand(queryObject));
    }
    protected List<object> GetEntities(Type type, IDbCommand cmd)
    {
        if(CacheEnabled)
        {
            var cacheKey = CreateCacheKey(cmd, typeof(List<object>));

            var cacheRes = GetCacheValue(cacheKey);
            if(cacheRes != null)
            {
                //Logger.Debug("Getting data from cache");
                return (List<object>)cacheRes;
            }
        }

        var dt = FillDataTable(cmd);

        var res = EF.ConvertToSqlEntities(dt, type);

        if(CacheEnabled)
        {
            var cacheKey = CreateCacheKey(cmd, typeof(List<object>));

            SetCacheValue(cacheKey, res);
        }

        return res;
    }

    protected List<T> GetEntities<T>(IQueryObject queryObject, Func<DataRow, T> selector)
    {
        var dt = FillDataTable(queryObject);

        return dt.Select().Select(selector).ToList();
    }

    protected List<object> GetEntitiesByWhere(Type type, IQueryObject whereQueryObject, string orderBy = null, int? skip = null, int? take = null)
    {
        var queryObject = NewQueryObject();

        var tableInfo = EF.GetSqlSchemaTableInfo(type);

        queryObject.AppendLine("SELECT " + GetSelectFields(type, tableInfo.Schema, tableInfo.Table).Join(","));
        queryObject.AppendLine("FROM " + CombineSchemaAndTable(tableInfo.Schema, tableInfo.Table));
        queryObject.Append("WHERE ");
        queryObject.Append(whereQueryObject);
        queryObject.AppendLine();

        if(orderBy != null)
        {
            queryObject.AppendLine("ORDER BY " + orderBy);
        }

        DatabaseProvider.Limit(queryObject, skip, take);

        return GetEntities(type, queryObject);
    }

    protected List<T> GetEntitiesByWhere<T>(IQueryObject queryObject, string orderBy = null, int? skip = null, int? take = null)
    {
        return GetEntitiesByWhere(typeof(T), queryObject, orderBy, skip, take).Select(x => x.CastTo<T>()).ToList();
    }

    public virtual void Truncate<T>(bool cascade = false) where T : class
    {
        Truncate(typeof(T), cascade);
    }
    public virtual void Truncate(Type type, bool cascade = false)
    {
        var tableInfo = EF.GetSqlSchemaTableInfo(type);

        var queryObject = NewQueryObject();
        queryObject.Append(string.Format("TRUNCATE {0}", CombineSchemaAndTable(tableInfo.Schema, tableInfo.Table)));

        if(cascade)
        {
            queryObject.Append(" CASCADE");
        }

        ExecuteNonQuery(queryObject);
    }

    //public virtual int DeleteEntities<T>(Expression<Func<T, bool>> expression = null) where T : class
    //{
    //    var type = typeof(T);

    //    var table = GetSqlTableAttribute(type);

    //    var query = "delete from " + CombineSchemaAndTable(table.Schema, table.Table);
    //    if(expression != null)
    //    {
    //        var whereClausule = Implementation.CreateWhereClausule(expression);
    //        return ExecuteNonQuery(query + " where " + whereClausule.Query, whereClausule.Parameters);
    //    }

    //    var res = ExecuteNonQuery(query, null);
    //    Cache.Clear<T>();
    //    return res;
    //}

    public virtual void Delete<T>(params T[] entities) where T : class
    {
        foreach (var entity in entities)
        {
            Delete(entity.GetType(), entity);    
        }
    }

    public virtual void Delete(Type type, object entity)
    {
        if(entity == null) return;

        if(ExecuteTriggers(TriggerAction.BeforeDelete, type, entity))
        {
            return;
        }

        var tableInfo = EF.GetSqlSchemaTableInfo(type);

        var primaryKeyName = EF.GetSqlPrimaryKeyName(type);

        var pkValue = GetPrimaryKeyPropertyInfo(type).GetValue(entity, null);

        int res;
        try
        {
            res = OnDelete(entity, tableInfo.Schema, tableInfo.Table, primaryKeyName, pkValue);
        }
        catch(Exception ex)
        {
            Audit(entity, tableInfo.Schema, tableInfo.Table, "DELETE", "FAILED: " + ex.Summary());
            throw;
        }
        if(res < 1)
        {
            Audit(entity, tableInfo.Schema, tableInfo.Table, "DELETE", "FAILED");
            throw new Exception("Failed to delete record");
        }

        Audit(entity, tableInfo.Schema, tableInfo.Table, "DELETE", "SUCCESS");

        ExecuteTriggers(TriggerAction.AfterDelete, type, entity);

        Cache.Clear(type);
    }

    public void Delete<T>(int pkValue) where T : class
    {
        DeleteById(typeof(T), pkValue);
    }

    public void Delete(Type type, int pkValue)
    {
        DeleteById(type, pkValue);
    }

    public void DeleteById<T>(object pkValue) where T : class
    {
        DeleteById(typeof(T), pkValue);
    }

    public void DeleteById(Type type, object pkValue)
    {
        Delete(type, GetEntityById(type, pkValue));
    }

    protected void DeleteByWhere<T>(IQueryObject queryObject) where T : class
    {
        var entities = GetEntitiesByWhere<T>(queryObject);
        foreach(var entity in entities)
        {
            Delete(entity);
        }
    }
    protected void DeleteByWhere(Type type, IQueryObject queryObject)
    {
        var entities = GetEntitiesByWhere(type, queryObject);
        foreach(var entity in entities)
        {
            Delete(entity);
        }
    }

    public int DeleteSqlById<T>(object pkValue) where T : class
    {
        return DeleteSqlById(typeof(T), pkValue);
    }

    public int DeleteSqlById(Type type, object pkValue)
    {
        var tableInfo = EF.GetSqlSchemaTableInfo(type);

        var primaryKeyName = EF.GetSqlPrimaryKeyName(type);

        var queryObject = NewQueryObject();

        queryObject.Append(string.Format("DELETE FROM {0} WHERE {1} = {2}", CombineSchemaAndTable(tableInfo.Schema, tableInfo.Table), primaryKeyName, CreateParameterName("pk")));
        queryObject.AddParameter("pk", pkValue);

        return ExecuteNonQuery(queryObject);
    }

    protected int DeleteSqlByWhere<T>(IQueryObject queryObject) where T : class
    {
        return DeleteSqlByWhere(typeof(T), queryObject);
    }
    protected int DeleteSqlByWhere(Type type, IQueryObject whereQueryObject)
    {
        var tableInfo = EF.GetSqlSchemaTableInfo(type);

        var queryObject = NewQueryObject();
        queryObject.AppendLine("DELETE FROM " + CombineSchemaAndTable(tableInfo.Schema, tableInfo.Table));
        queryObject.Append("WHERE ");
        queryObject.Append(whereQueryObject);
        queryObject.AppendLine();

        return ExecuteNonQuery(queryObject);
    }

    protected int OnDelete<T>(T entity, string schema, string table, string pkField, object pkValue) where T : class
    {
        return OnDelete(entity.GetType(), entity, schema, table, pkField, pkValue);
    }

    protected virtual int OnDelete(Type type, object entity, string schema, string table, string pkField, object pkValue)
    {
        var hasDeleted = entity as IHasDeleted;
        if (hasDeleted != null)
        {
            var pi = type.GetProperty("Deleted");
            if (pi == null)
            {
                throw new InvalidOperationException(string.Format("Missing property: {0}", "Deleted"));
            }


            var queryObject = GenerateUpdateQuery(schema, table, new[] { string.Format("{0} = {1}", EF.GetFieldName(pi), CreateParameterName("deleted")) }, pkField, CreateParameterName("pk"));

            queryObject.AddParameter("pk", pkValue);
            queryObject.AddParameter("deleted", true);

            var res = ExecuteNonQuery(queryObject);
            if (res == 1)
            {
                hasDeleted.Deleted = true;
            }
            return res;
        }
        else
        {
            var queryObject = NewQueryObject();

            queryObject.Append(string.Format("DELETE FROM {0} WHERE {1} = {2}", CombineSchemaAndTable(schema, table), pkField, CreateParameterName("pk")));
            queryObject.AddParameter("pk", pkValue);

            return ExecuteNonQuery(queryObject);
        }
    }

    public virtual object Save<T>(T entity) where T : class
    {
        return Save(entity.GetType(), entity);
    }
    public virtual object Save(Type type, object entity)
    {
        var primaryKeyName = EF.GetSqlPrimaryKeyName(type);
        var fieldNamingType = EF.GetSqlFieldNamingType(type);

        FillTypeInsertProperties(type);
        var properties = TypeInsertProperties[type];
        var pkProperty = properties.SingleOrDefault(pi =>
        {
            var fieldName = EF.GetFieldName(pi, fieldNamingType);
            return string.Equals(fieldName, primaryKeyName, StringComparison.InvariantCultureIgnoreCase);
        }) ?? GetPropertyInfo(type, primaryKeyName);

        var pkValue = pkProperty.GetValue(entity, null);
        var hasNoPk = PrimaryKeyValueIsEmpty(pkValue);

        return hasNoPk ? Insert(type, entity) : Update(type, entity);
    }

    public virtual object Insert<T>(T entity) where T : class
    {
        return Insert(entity.GetType(), entity);
    }
    public virtual object Insert(Type type, object entity)
    {
        var tableInfo = EF.GetSqlSchemaTableInfo(type);

        string primaryKeyName = null;
        try
        {
            primaryKeyName = EF.GetSqlPrimaryKeyName(type);
        }
        catch(MissingSqlPrimaryKeyAttributeException)
        {
        }

        return Insert(tableInfo.Schema, tableInfo.Table, primaryKeyName, type, entity);
    }
    protected virtual void BeforeInsert(ref string schema, ref  string table, ref string pkfield, ref Type type, ref object entity)
    {
    }
    protected virtual void AfterInsert(ref string schema, ref  string table, ref string pkfield, ref Type type, ref object entity)
    {
    }

    protected virtual object Insert<T>(string schema, string table, string pkfield, T entity) where T : class
    {
        return Insert(schema, table, pkfield, entity.GetType(), entity);
    }

    private static readonly Dictionary<Type, PropertyInfo[]> TypeInsertProperties = new Dictionary<Type, PropertyInfo[]>();
    private static void FillTypeInsertProperties(Type type)
    {
        lock(Lock)
        {
            if(TypeInsertProperties.ContainsKey(type)) return;

            var properties = new List<PropertyInfo>();

            //foreach(var pi in type.GetProperties().Where(pi => pi.CanWrite && pi.GetSetMethod(true).IsPublic))
            foreach(var pi in type.GetProperties().Where(pi => pi.CanRead && pi.GetGetMethod(true).IsPublic))
            {
                if(pi.HasCustomAttribute<IgnoreAttribute>())
                    continue;

                if(pi.HasCustomAttribute<SqlFieldAttribute>())
                {
                    properties.Add(pi);
                }

                if(pi.HasCustomAttribute<SqlEntityAttribute>())
                {
                    properties.Add(pi);
                    FillTypeInsertProperties(pi.PropertyType);
                }
            }

            TypeInsertProperties.Set(type, properties.ToArray());
        }
    }
    private void FillTypeInsertProperties(Type type, object entity, ref Dictionary<PropertyInfo, object> propertyValues)
    {
        var properties = TypeInsertProperties[type];
        foreach(var pi in properties)
        {
            var sqlEntityAttr = pi.GetCustomAttribute<SqlEntityAttribute>();
            if(sqlEntityAttr != null)
            {
                var sqlFieldParameters = new SqlFieldGetPropertyValueParameters(type, pi, entity);
                var value = sqlEntityAttr.GetPropertyValue(sqlFieldParameters);
                FillTypeInsertProperties(pi.PropertyType, value, ref propertyValues);
            }
            else
            {
                if(propertyValues.ContainsKey(pi))
                {
                    throw new Exception(string.Format("Property '{0}' in type '{1}' already exists in a parent type", pi.Name, type.FullName));
                }

                var sqlFieldAttr = pi.GetCustomAttribute<SqlFieldAttribute>();
                var value = sqlFieldAttr.GetPropertyValue(new SqlFieldGetPropertyValueParameters(type, pi, entity));
                var fieldParameters = new SqlFieldInsertParameters(Data, DatabaseProvider, type, pi, entity, value, CreateParameterName(pi.Name));
                var sql = sqlFieldAttr.GetSql(fieldParameters);

                if(fieldParameters.Cancel)
                {
                    continue;
                }

                if(fieldParameters.HasNewPropertyValue)
                {
                    value = fieldParameters.NewPropertyValue;
                }

                if(sql != null)
                {
                    value = new SqlChangedClass(value, sql);
                }
                //else
                //{
                //    string newValue;
                //    var ruleParameters = new FieldRuleInsertParameters(type, pi, entity, value, CreateParameterName(pi.Name));
                //    if(FieldRuler.GetNewSql(ruleParameters, out newValue))
                //    {
                //        value = new SqlChangedClass(value, newValue);
                //    }

                //    if(ruleParameters.Cancel)
                //    {
                //        continue;
                //    }
                //}

                if(value == null)
                {
                    var defaultValueAttr = pi.GetCustomAttribute<SqlDefaultValueAttribute>();
                    if(defaultValueAttr != null && defaultValueAttr.InsertValue != null)
                    {
                        value = defaultValueAttr.InsertValue;
                        pi.SetValue(entity, value, null);
                    }
                }

                propertyValues.Add(pi, value);
            }
        }
    }

    private class SqlChangedClass
    {
        public object PropertyValue { get; private set; }
        public string NewSql { get; private set; }

        public SqlChangedClass(object propertyValue, string newSqlForParameter)
        {
            PropertyValue = propertyValue;
            NewSql = newSqlForParameter;
        }

        public override string ToString()
        {
            return string.Format("Old: {0} | New: {1}", PropertyValue, NewSql);
        }
    }

    private bool PrimaryKeyValueIsEmpty(object pkValue)
    {
        return pkValue == null || pkValue is DBNull || (pkValue is string && pkValue.ToString().Trim() == string.Empty) || (!(pkValue is string) && pkValue.GetType().GetDefaultValue().Equals(pkValue));
    }
    protected virtual object Insert(string schema, string table, string pkfield, Type type, object entity)
    {
        if(ExecuteTriggers(TriggerAction.BeforeInsert, type, entity))
        {
            return null;
            //throw new TriggerException(entity, "Insert cancelled");
        }

        BeforeInsert(ref schema, ref table, ref pkfield, ref type, ref entity);

        FillTypeInsertProperties(type);

        var propertyValues = new Dictionary<PropertyInfo, object>();
        FillTypeInsertProperties(type, entity, ref propertyValues);

        var pkProperty = pkfield != null ? GetPrimaryKeyPropertyInfo(type) : null;
        var pkValue = pkProperty != null ? pkProperty.GetValue(entity, null) : null;
        var hasNoPk = PrimaryKeyValueIsEmpty(pkValue);
        var keepPk = pkProperty == null || !hasNoPk;

        var fieldNamingType = EF.GetSqlFieldNamingType(type);

        var properties = propertyValues.Select(x =>
        {
            var fieldName = EF.GetFieldName(x.Key, fieldNamingType);

            if(string.Equals(fieldName, pkfield, StringComparison.InvariantCultureIgnoreCase) && !keepPk)
            {
                return null;
            }

            var propertyValue = x.Value;

            var sqlChanged = propertyValue as SqlChangedClass;
            if(sqlChanged != null)
            {
                propertyValue = sqlChanged.PropertyValue;
            }

            var propertyType = x.Key.PropertyType;
            if(propertyValue != null)
            {
                propertyType = propertyValue.GetType();
            }

            propertyType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
            if(propertyType.IsEnum && propertyValue != null)
            {
                propertyValue = propertyValue.ToString();
            }

            return new
            {
                pi = x.Key,
                fieldName,
                parameterName = CreateParameterName(x.Key.Name),
                propertyType,
                propertyValue = propertyValue ?? DBNull.Value,
                sqlChanged
            };
        }).Where(x => x != null).ToArray();

        var propertyNames = properties.Select(x => x.fieldName).ToArray();

        var propertyParameterNames = properties.Select(x => x.sqlChanged != null ? x.sqlChanged.NewSql : x.parameterName).ToArray();
        var parameters = properties.ToDictionary(x => x.pi.Name, x => x.propertyValue);
        
        var queryObject = GenerateInsertQuery(schema, table, propertyNames, propertyParameterNames);
        queryObject.AddParameters(parameters);

        var returnIdOnInsert = false;
        if(pkfield != null)
        {
            try
            {
                var newQueryObject = DatabaseProvider.GetNewQueryBeforeInsertToReturnId(queryObject, pkfield);
                if(newQueryObject != null)
                {
                    queryObject = newQueryObject;
                    returnIdOnInsert = true;
                }
            }
            catch(NotSupportedException)
            {
            }
        }

        var newId = pkValue;

        if(returnIdOnInsert)
        {
            try
            {
                newId = ExecuteScalar(queryObject);
            }
            catch(Exception ex)
            {
                Audit(type, entity, schema, table, "INSERT", "FAILED: " + ex.Summary());
                throw;
            }
            if(newId == null || newId is DBNull)
            {
                Audit(type, entity, schema, table, "INSERT", "FAILED");
                throw new Exception("Failed to insert record");
            }
        }
        else
        {
            int res;
            try
            {
                res = ExecuteNonQuery(queryObject);
            }
            catch(Exception ex)
            {
                Audit(type, entity, schema, table, "INSERT", "FAILED: " + ex.Summary());
                throw;
            }
            if(res == 0)
            {
                Audit(type, entity, schema, table, "INSERT", "FAILED");
                throw new Exception("Failed to insert record");
            }

            if(!keepPk)
            {
                try
                {
                    var newQueryObject = DatabaseProvider.GetQueryToGetIdAfterInsert(Connection.Database, schema, table);
                    if(newQueryObject != null)
                    {
                        newId = ExecuteScalar(newQueryObject);
                    }
                }
                catch(NotSupportedException)
                {
                }
            }
        }

        if(!keepPk)
        {
            newId = Convert.ChangeType(newId, pkProperty.PropertyType);
            pkProperty.SetValue(entity, newId, null);
        }

        Audit(type, entity, schema, table, "INSERT", "SUCCESS");

        AfterInsert(ref schema, ref table, ref pkfield, ref type, ref entity);

        ExecuteTriggers(TriggerAction.AfterInsert, type, entity);

        Cache.Clear(type);

        return newId;
    }
    protected virtual void Insert<T>(string schema, string table, T entity) where T : class
    {
        var properties = entity.GetType().GetProperties().Where(x => x.CanWrite && x.CanRead && x.GetSetMethod(true).IsPublic).ToArray();
        var propertyNames = properties.Select(x => x.Name).ToArray();
        var propertyParameterNames = properties.Select(x => CreateParameterName(x.Name)).ToArray();

        var parameters = properties.ToDictionary(x => x.Name, x => x.GetValue(entity, null));
        var queryObject = GenerateInsertQuery(schema, table, propertyNames, propertyParameterNames);
        queryObject.AddParameters(parameters);

        var res = ExecuteNonQuery(queryObject);
        if(res < 1)
        {
            throw new Exception("Failed to insert record");
        }
        Cache.Clear<T>();
    }

    public virtual object Update<T>(T entity) where T : class
    {
        return Update(entity.GetType(), entity);
    }
    public virtual object Update(Type type, object entity)
    {
        var tableInfo = EF.GetSqlSchemaTableInfo(type);

        var primaryKeyName = EF.GetSqlPrimaryKeyName(type);

        var pkProperty = GetPrimaryKeyPropertyInfo(type);
        var pkValue = pkProperty.GetValue(entity, null);

        Update(tableInfo.Schema, tableInfo.Table, type, entity, primaryKeyName, pkValue);

        return pkValue;
    }

    protected virtual void BeforeUpdate(ref string schema, ref string table, ref Type type, ref object entity, ref string pkField, ref object pkValue)
    {
    }
    protected virtual void AfterUpdate(ref string schema, ref string table, ref Type type, ref object entity, ref string pkField, ref object pkValue)
    {
    }

    protected virtual void Update<T>(string schema, string table, T entity, string pkField, object pkValue)
        where T : class
    {
        Update(schema, table, entity.GetType(), entity, pkField, pkValue);
    }

    private static readonly Dictionary<Type, PropertyInfo[]> TypeUpdateProperties = new Dictionary<Type, PropertyInfo[]>();
    private static void FillTypeUpdateProperties(Type type)
    {
        lock(Lock)
        {
            if(TypeUpdateProperties.ContainsKey(type)) return;

            var properties = new List<PropertyInfo>();

            //foreach(var pi in type.GetProperties().Where(pi => pi.CanWrite && pi.GetSetMethod(true).IsPublic))
            foreach(var pi in type.GetProperties().Where(pi => pi.CanRead && pi.GetGetMethod(true).IsPublic))
            {
                if(pi.HasCustomAttribute<IgnoreAttribute>())
                    continue;

                if(pi.HasCustomAttribute<SqlFieldAttribute>())
                {
                    properties.Add(pi);
                }

                if(pi.HasCustomAttribute<SqlEntityAttribute>())
                {
                    properties.Add(pi);
                    FillTypeUpdateProperties(pi.PropertyType);
                }
            }

            TypeUpdateProperties.Set(type, properties.ToArray());
        }
    }
    
    private void FillTypeUpdateProperties(Type type, object entity, ref Dictionary<PropertyInfo, object> propertyValues)
    {
        var properties = TypeUpdateProperties[type];
        foreach(var pi in properties)
        {
            var sqlEntityAttr = pi.GetCustomAttribute<SqlEntityAttribute>();
            if(sqlEntityAttr != null)
            {
                var value = sqlEntityAttr.GetPropertyValue(new SqlFieldGetPropertyValueParameters(type, pi, entity));
                FillTypeUpdateProperties(pi.PropertyType, value, ref propertyValues);
            }
            else
            {
                if(propertyValues.ContainsKey(pi))
                {
                    throw new Exception(string.Format("Property '{0}' in type '{1}' already exists in a parent type", pi.Name, type.FullName));
                }

                var sqlFieldAttr = pi.GetCustomAttribute<SqlFieldAttribute>();
                var value = sqlFieldAttr.GetPropertyValue(new SqlFieldGetPropertyValueParameters(type, pi, entity));
                var fieldParameters = new SqlFieldUpdateParameters(Data, DatabaseProvider, type, pi, entity, value, CreateParameterName(pi.Name));
                var sql = sqlFieldAttr.GetSql(fieldParameters);

                if(fieldParameters.Cancel)
                {
                    continue;
                }

                if(fieldParameters.HasNewPropertyValue)
                {
                    value = fieldParameters.NewPropertyValue;
                }

                if(sql != null)
                {
                    value = new SqlChangedClass(value, sql);
                }
                //else
                //{
                //    string newValue;
                //    var ruleParameters = new FieldRuleUpdateParameters(type, pi, entity, value, CreateParameterName(pi.Name));
                //    if(FieldRuler.GetNewSql(ruleParameters, out newValue))
                //    {
                //        value = new SqlChangedClass(value, newValue);
                //    }

                //    if(ruleParameters.Cancel)
                //    {
                //        continue;
                //    }
                //}

                if(value == null)
                {
                    var defaultValueAttr = pi.GetCustomAttribute<SqlDefaultValueAttribute>();
                    if(defaultValueAttr != null && defaultValueAttr.UpdateValue != null)
                    {
                        value = defaultValueAttr.UpdateValue;
                        pi.SetValue(entity, value, null);
                    }
                }

                propertyValues.Add(pi, value);
            }
        }
    }

    protected virtual void Update(string schema, string table, Type type, object entity, string pkField, object pkValue)
    {
        if(ExecuteTriggers(TriggerAction.BeforeUpdate, type, entity))
        {
            return;
        }

        BeforeUpdate(ref schema, ref table, ref type, ref entity, ref pkField, ref pkValue);

        FillTypeUpdateProperties(type);

        var propertyValues = new Dictionary<PropertyInfo, object>();
        FillTypeUpdateProperties(type, entity, ref propertyValues);

        var fieldNamingType = EF.GetSqlFieldNamingType(type);

        var properties = propertyValues.Select(x =>
        {
            var fieldName = EF.GetFieldName(x.Key, fieldNamingType);
            var propertyValue = x.Value;

            var sqlChanged = propertyValue as SqlChangedClass;
            if(sqlChanged != null)
            {
                propertyValue = sqlChanged.PropertyValue;
            }

            var propertyType = x.Key.PropertyType;
            if(propertyValue != null)
            {
                propertyType = propertyValue.GetType();
            }

            propertyType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
            if(propertyType.IsEnum && propertyValue != null)
            {
                propertyValue = propertyValue.ToString();
            }

            return new
            {
                pi = x.Key,
                fieldName,
                parameterName = CreateParameterName(x.Key.Name),
                propertyType,
                propertyValue,
                sqlChanged
            };
        }).ToArray();

        var parameters = properties.ToDictionary(x => x.pi.Name, x => x.propertyValue);
        var pkParameter = properties.Single(x => string.Equals(x.fieldName, pkField, StringComparison.InvariantCultureIgnoreCase)).parameterName;

        var propertyNamesAndValues = properties.Where(x => x.parameterName != pkParameter).Select(x => x.fieldName + " = " + (x.sqlChanged != null ? x.sqlChanged.NewSql : x.parameterName)).ToArray();

        var queryObject = GenerateUpdateQuery(schema, table, propertyNamesAndValues, pkField, pkParameter);
        queryObject.AddParameters(parameters);

        int res;
        try
        {
            res = ExecuteNonQuery(queryObject);
        }
        catch(Exception ex)
        {
            Audit(type, entity, schema, table, "UPDATE", "FAILED: " + ex.Summary());
            throw;
        }
        if(res < 1)
        {
            Audit(type, entity, schema, table, "UPDATE", "FAILED");
            throw new Exception("Failed to update record");
        }

        Audit(type, entity, schema, table, "UPDATE", "SUCCESS");

        AfterUpdate(ref schema, ref table, ref type, ref entity, ref pkField, ref pkValue);

        ExecuteTriggers(TriggerAction.AfterUpdate, type, entity);

        Cache.Clear(type);
    }

    public bool Exists<T>(T entity) where T : class
    {
        return Exists(entity.GetType(), entity);
    }
    public virtual bool Exists(Type type, object entity)
    {
        if(entity is StringBuilder || entity is string)
        {
            var queryObject = NewQueryObject().Append(entity.ToString());
            return Exists(queryObject);
        }

        var pkValue = GetPrimaryKeyPropertyInfo(type).GetValue(entity, null);
        var hasNoPk = PrimaryKeyValueIsEmpty(pkValue);
        if(hasNoPk)
        {
            return false;
        }

        return ExistsById(type, pkValue);
    }
    public virtual bool Exists(IQueryObject queryObject)
    {
        var res = ExecuteScalar(queryObject);
        if(res == null || res is DBNull)
        {
            return false;
        }

        return true;
    }

    public bool ExistsById<T>(object pkValue) where T : class
    {
        return ExistsById(typeof(T), pkValue);
    }
    public bool ExistsById(Type type, object pkValue)
    {
        var tableInfo = EF.GetSqlSchemaTableInfo(type);

        var primaryKeyName = EF.GetSqlPrimaryKeyName(type);

        var queryObject = NewQueryObject();
        queryObject.Append(string.Format("SELECT 1 FROM {0} WHERE {1} = {2}", CombineSchemaAndTable(tableInfo.Schema, tableInfo.Table), primaryKeyName, CreateParameterName("pk")));
        queryObject.AddParameter("pk", pkValue);

        return Exists(queryObject);
    }

    public bool ExistsByWhere<T>(IQueryObject queryObject) where T : class
    {
        return ExistsByWhere(typeof(T), queryObject);
    }
    public bool ExistsByWhere(Type type, IQueryObject whereQueryObject)
    {
        var tableInfo = EF.GetSqlSchemaTableInfo(type);

        var queryObject = NewQueryObject();
        queryObject.AppendLine("SELECT 1");
        queryObject.AppendLine("FROM " + CombineSchemaAndTable(tableInfo.Schema, tableInfo.Table));
        queryObject.Append("WHERE ");
        queryObject.Append(whereQueryObject);
        queryObject.AppendLine();

        DatabaseProvider.Limit(queryObject, 0, 1);

        var res = ExecuteScalar(queryObject);
        if(res == null || res is DBNull)
        {
            return false;
        }

        return true;
    }

    public virtual void ClearCache()
    {
        Cache.Clear(GetType());
    }
    protected virtual object GetCacheValue(string key)
    {
        return Cache.Get(GetType(), key);
    }
    protected virtual void SetCacheValue(string key, object value)
    {
        Cache.Set(GetType(), key, value);
    }

    public IQueryObject NewQueryObject()
    {
        return new QueryObject();
    }
    public IQueryObject NewQueryObject(string query, params object[] args)
    {
        var queryObject = new QueryObject();
        queryObject.Append(query);

        if (args != null)
        {
            for (var i = 0; i < args.Length; i++)
            {
                if (i == args.Length - 1)
                {
                    throw new InvalidOperationException("Not enough arguments to create a parameter");
                }

                var key = (string)args[i];
                var value = args[i + 1];
                queryObject.AddParameter(key, value);
                i++;
            }
        }

        return queryObject;
    }
    
    private bool ExecuteTriggers(TriggerAction action, Type type, object entity)
    {
        var triggers = EF.GetTriggers(type, action);
        foreach(var trigger in triggers)
        {
            var e = new TriggerParameters(action, entity);
            trigger.Execute(this, e);
            if(e.Cancel)
            {
                return true;
            }
        }
        return false;
    }

    private static readonly string[] AuditFields = new[] { "xmlcontent", "sqlaction", "message", "created_at", "created_by" };

    private void Audit<T>(T entity, string schema, string table, string action, string message)
    {
        Audit(entity.GetType(), entity, schema, table, action, message);
    }
    private void Audit(Type type, object entity, string schema, string table, string action, string message)
    {
        //if(!EF.AuditEnabled) return;

        //if(type.GetCustomAttribute<AuditAttribute>() != null)
        //{
        //    var parameters = NewParameters("xmlcontent", Xml.SerializeToString(entity), "sqlaction", action, "message", message, "created_at", DateTime.Now, "created_by", WindowsIdentity.GetCurrent().Name);

        //    var q = string.Format("insert into {0}_audit ({1}) values ({2})", DatabaseProvider.CombineSchemaAndTable(schema, table), string.Join(",", AuditFields), string.Join(",", AuditFields.Select(CreateParameterName).ToArray()));

        //    try
        //    {
        //        ExecuteNonQuery(q, parameters);
        //    }
        //    catch(Exception ex)
        //    {
        //        if(ex.Message.Contains("does not exist"))
        //        {
        //            var createTableSql = string.Format("CREATE TABLE {0}_audit (id serial NOT NULL, xmlcontent text NOT NULL, sqlaction character varying(10) NOT NULL, message text, created_at timestamp without time zone NOT NULL, created_by character varying(50) NOT NULL, CONSTRAINT pk_{1}_{2}_audit_id PRIMARY KEY (id))", DatabaseProvider.CombineSchemaAndTable(schema, table), schema, table);
        //            ExecuteNonQuery(createTableSql, null);
        //            Audit(entity, schema, table, action, message);
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }
        //}
    }

    #endregion

    public class Transaction : IDisposable
    {
        internal DbTransaction Value { get; private set; }
        private bool _hasCommited;

        internal event EventHandler Committed;
        internal event EventHandler Cancelled;

        internal Transaction(DbTransaction transaction)
        {
            Value = transaction;
        }

        public void Commit()
        {
            Value.Commit();
            _hasCommited = true;

            if(Committed != null)
            {
                Committed(this, EventArgs.Empty);
            }
        }

        public void Cancel()
        {
            try
            {
                Value.Rollback();
            }
            catch(InvalidOperationException)
            {
                // No transaction in progress
            }

            if(Cancelled != null)
            {
                Cancelled(this, EventArgs.Empty);
            }
        }

        public void Dispose()
        {
            if(!_hasCommited)
            {
                Cancel();
            }
        }
    }
}