using DePeuter.Timesheets.Database.Entities;

namespace DePeuter.Timesheets.Database.Services
{
    [ConnectionStringName("AppDatabaseConnectionString")]
    public abstract class DbBase : BaseMSSQLService
    {
        protected DbBase()
            : base()
        {
        }

        protected override void ResolveProviderAndConnectionString(out string providerInvariantName, out string connectionString)
        {
            connectionString = DePeuterRegistry.Get("Timesheets", "ConnectionString") as string ?? @"Data Source=localhost\SQLEXPRESS;Initial Catalog=depeuter;Integrated Security=SSPI";
            providerInvariantName = DatabaseProviderName;
        }

        public Configuration GetConfiguration(string key)
        {
            var config = GetEntityByWhere<Configuration>(NewQueryObject("vchKey = @key", "key", key));
            if(config == null)
            {
                config = new Configuration() { Key = key };
            }
            return config;
        }
    }
}
