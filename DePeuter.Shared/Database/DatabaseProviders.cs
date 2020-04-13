using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DePeuter.Shared.Database
{
    public static class DatabaseProviders
    {
        private static readonly object Lock = new object();
        private static readonly Dictionary<string, Type> ImplementationMapping = new Dictionary<string, Type>();

        static DatabaseProviders()
        {
            AppDomain.CurrentDomain.OnAllLoadedTypes(RegisterTypes);
        }

        public static void RegisterTypes(params Type[] types)
        {
            lock(Lock)
            {
                var implementationTypes = types.Where(x => !x.IsAbstract && typeof(IDatabaseProvider).IsAssignableFrom(x)).ToArray();

                foreach(var type in implementationTypes)
                {
                    var attributes = type.GetCustomAttributes<BaseDatabaseProviderAttribute>();
                    foreach(var attribute in attributes)
                    {
                        var key = attribute.ProviderName.ToUpper();
                        if(!ImplementationMapping.ContainsKey(key))
                        {
                            Console.Out.WriteLine("IDatabaseProvider mapping: {0} -> {1}", key, type);
                            ImplementationMapping.Add(key, type);
                        }
                    }
                }
            }
        }

        public static void RegisterProvider<T>() where T : BaseDatabaseProviderAttribute, new()
        {
            RegisterProvider(new T());
        }
        public static void RegisterProvider(BaseDatabaseProviderAttribute attribute)
        {
            if(attribute == null || attribute.ProviderType == null)
            {
                return;
            }

            RegisterProvider(attribute.ProviderName, attribute.ProviderType);
        }
        public static void RegisterProvider(string name, Type type)
        {
            if(string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }
            if(type == null)
            {
                throw new ArgumentNullException("type");
            }

            lock(Lock)
            {
                var key = name.ToUpper();
                if(!ImplementationMapping.ContainsKey(key))
                {
                    Console.Out.WriteLine("IDatabaseProvider mapping: {0} -> {1}", key, type);
                    ImplementationMapping.Add(key, type);
                }
            }
        }

        public static IDatabaseProvider GetDatabaseProvider(string providerInvariantName)
        {
            if(string.IsNullOrEmpty(providerInvariantName))
            {
                throw new ArgumentNullException("providerInvariantName");
            }

            lock(Lock)
            {
                if(ImplementationMapping.ContainsKey(providerInvariantName.ToUpper()))
                    return (IDatabaseProvider)Activator.CreateInstance(ImplementationMapping[providerInvariantName.ToUpper()]);
            }

            throw new Exception("No IDatabaseProvider implementation was found for provider '" + providerInvariantName + "'");
        }
    }
}