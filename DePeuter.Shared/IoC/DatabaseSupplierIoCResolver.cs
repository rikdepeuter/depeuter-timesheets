using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace DePeuter.Shared
{
    public delegate ConnectionStringSettings GetConnectionStringSettingsHandler(string connectionStringName);
    //public delegate string GetUserNiscodeHandler();

    public class DatabaseSupplierIoCResolver : SupplierIoCResolver
    {
        private readonly GetConnectionStringSettingsHandler _getConnectionStringSettings;
        private readonly string _defaultConnectionStringName;

        public DatabaseSupplierIoCResolver(GetSupplierHandler getSupplier, GetConnectionStringSettingsHandler getConnectionStringSettings = null, string defaultSupplierName = null, string defaultConnectionStringName = null)
            : base(getSupplier, defaultSupplierName)
        {
            _getConnectionStringSettings = getConnectionStringSettings;
            _defaultConnectionStringName = defaultConnectionStringName;
        }

        protected override Type FindImplementationType(IoC.ResolveData data)
        {
            return IoC.FindImplementationType(data, validateImplementationTypes: ValidateImplementationTypes, resolveImplementationType: ResolveImplementationType);
        }
        
        private void ValidateImplementationTypes(Type wantedType, Type[] implementationTypes)
        {
            ValidateImplementationTypesConnectionStringNameAttributes(wantedType, implementationTypes);
            ValidateImplementationTypesSupplierAttributes(wantedType, implementationTypes);
        }

        private Type ResolveImplementationType(IoC.ResolveData data, Type[] implementationTypes)
        {
            var isDatabaseService = typeof(IBaseDatabaseService).IsAssignableFrom(data.WantedType);

            if(!isDatabaseService)
            {
                return null;
            }

            //var niscode = IoC.FindNiscodeParameter(data) ?? (_getUserNiscode != null ? _getUserNiscode() : null);

            var connectionStringAttr = data.WantedType.GetCustomAttribute<ConnectionStringNameAttribute>() ?? data.WantedType.GetCustomAttribute<ConnectionStringNameAttribute>(true);
            if(connectionStringAttr == null)
            {
                connectionStringAttr = implementationTypes.Select(x => x.GetCustomAttribute<ConnectionStringNameAttribute>() ?? x.GetCustomAttribute<ConnectionStringNameAttribute>(true)).FirstOrDefault(x => x != null);
            }

            var connectionStringName = connectionStringAttr != null ? connectionStringAttr.Name : _defaultConnectionStringName;

            var supplierName = GetSupplier(connectionStringName);

            var implementationTypesWithSuppliers = implementationTypes.Select(x =>
            {
                return new
                {
                    Type = x,
                    Suppliers = x.GetCustomAttributes<SupplierAttribute>(true).Where(y => y.Name != null).Select(y => y.Name.ToUpper()).ToArray()
                };
            }).ToArray();

            //met geen of "GEOIT" connectionstring is er geen link en geen supplier nodig
            Type type;
            if(string.IsNullOrEmpty(supplierName))
            {
                if(string.Equals(connectionStringName, _defaultConnectionStringName, StringComparison.InvariantCultureIgnoreCase))
                {
                    if(implementationTypes.Length > 1)
                    {
                        throw new TypeResolvingFailedException(data.WantedType, "No ConnectionStringNameAttribute found on interface or implementation types");
                    }

                    type = implementationTypes.SingleOrDefault();
                }
                else if(implementationTypesWithSuppliers.Any(x => x.Suppliers.Contains(DefaultSupplierName)))
                {
                    //als er geen link is bepaald, dan GEOIT nemen indien er zo'n implementatie voor bestaat
                    type = implementationTypesWithSuppliers.First(x => x.Suppliers.Contains(DefaultSupplierName)).Type;
                }
                else
                {
                    type = null;
                }
            }
            else
            {
                type = implementationTypesWithSuppliers.Where(x => x.Suppliers.Contains(supplierName.ToUpper())).Select(x => x.Type).SingleOrDefault();
            }

            if(type == null)
            {
                var possibleLinks = implementationTypesWithSuppliers.SelectMany(x => x.Suppliers).OrderBy(x => x).ToArray();

                var msg = string.IsNullOrEmpty(supplierName) ? string.Format("No supplier configured for connectionstring '{0}'.", connectionStringName) : string.Format("Supplier value '{0}' for connectionstring '{1}' is invalid.", supplierName, connectionStringName);
                throw new InvalidOperationException(string.Format("{0}\nPossible suppliers: {1}", msg, possibleLinks.Join(", ")));
            }

            var hasConnectionStringParameter = data.HasParameter<ConnectionStringParameter>();
            var hasProviderParameter = data.HasParameter<ProviderParameter>();

            if(!hasConnectionStringParameter || !hasProviderParameter)
            {
                var conn = _getConnectionStringSettings(connectionStringName);

                //als er geen connectiestring is geconfigureerd voor de gewenste, neem dan die van GEOIT
                if(conn == null && _defaultConnectionStringName != null)
                {
                    conn = _getConnectionStringSettings(_defaultConnectionStringName);
                }

                if(conn == null)
                {
                    throw new TypeResolvingFailedException(data.WantedType, string.Format("No ConnectionString found for '{0}'", connectionStringName));
                }

                if(!hasConnectionStringParameter)
                {
                    data.Parameters.Add(new ConnectionStringParameter(conn.ConnectionString));
                }

                if(!hasProviderParameter)
                {
                    data.Parameters.Add(new ProviderParameter(conn.ProviderName));
                }
            }

            //if(!string.IsNullOrEmpty(niscode))
            //{
            //    if(!data.HasParameter<NiscodeParameter>())
            //    {
            //        data.Parameters.Add(new NiscodeParameter(niscode));
            //    }
            //}

            return type;
        }
    }
}