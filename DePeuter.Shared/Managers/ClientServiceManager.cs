using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using DePeuter.Shared.Attributes;

namespace DePeuter.Shared.Managers
{
    public static class ClientServiceManager
    {
        private static readonly Dictionary<Type, Type[]> ClientToBindingsMapping = new Dictionary<Type, Type[]>();
        private static readonly Dictionary<Type, Type[]> ClientToAddressesMapping = new Dictionary<Type, Type[]>();

        public delegate string RequestConfigurationValueHandler(string key, object parameters);
        private static RequestConfigurationValueHandler _requestConfigurationValueHandler;

        private static readonly object Lock = new object();

        public static void Initialize(RequestConfigurationValueHandler handler)
        {
            _requestConfigurationValueHandler = handler;
        }

        private static string GetConfigurationValue(string key, object parameters)
        {
            if(_requestConfigurationValueHandler == null)
            {
                throw new InvalidOperationException("Execute ClientServiceManager.Initialize() once before calling any other function");
            }

            return _requestConfigurationValueHandler(key, parameters);
        }

        public static T GetClient<T>(object parameters) where T : class
        {
            var clientType = typeof(T);

            if(!clientType.Name.EndsWith("Client"))
            {
                throw new InvalidOperationException("Type parameter must be a client class");
            }

            var serviceName = clientType.Name.Substring(0, clientType.Name.Length - "Client".Length);

            var bindingTypeName = serviceName + "Binding";
            var addressTypeName = serviceName + "Address";

            lock(Lock)
            {
                if(!ClientToBindingsMapping.ContainsKey(clientType))
                {
                    //find binding
                    var bindingTypesInAssembly = clientType.Assembly.GetTypes().Where(x => typeof(Binding).IsAssignableFrom(x) && !x.IsAbstract && x.IsClass).ToArray();

                    var serviceBindings = bindingTypesInAssembly.Where(x =>
                    {
                        var attr = x.GetCustomAttribute<ServiceClientAttribute>();
                        if(attr == null)
                        {
                            return false;
                        }

                        return attr.ClientType == clientType;
                    }).ToArray();
                    if(!serviceBindings.Any())
                    {
                        throw new InvalidOperationException(string.Format("No Binding types found with a ServiceClientAttribute for '{0}'", clientType.FullName));
                    }

                    ClientToBindingsMapping.Add(clientType, serviceBindings);

                    //find address type (optional)
                    var addressTypesInAssembly = clientType.Assembly.GetTypes().Where(x => typeof(EndpointAddress).IsAssignableFrom(x) && !x.IsAbstract && x.IsClass).ToArray();

                    var serviceAddresses = addressTypesInAssembly.Where(x =>
                    {
                        var attr = x.GetCustomAttribute<ServiceClientAttribute>();
                        if(attr == null)
                        {
                            return false;
                        }

                        return attr.ClientType == clientType;
                    }).ToArray();
                    if(serviceAddresses.Any())
                    {
                        ClientToAddressesMapping.Add(clientType, serviceAddresses);
                    }
                }
            }

            var bindingTypes = ClientToBindingsMapping[clientType];

            Type bindingType;
            if(bindingTypes.Length == 1)
            {
                bindingType = bindingTypes.Single();
            }
            else
            {
                var bindingValue = GetConfigurationValue(bindingTypeName, parameters);
                if(string.IsNullOrEmpty(bindingValue))
                {
                    throw new InvalidOperationException(string.Format("No Binding type configured for name '{0}'. Possible values are: {1}", bindingTypeName, bindingTypes.Select(x => x.Name).Join(", ")));
                }

                var possibleBindingTypes = bindingTypes.Where(x => x.Name.ToLower() == bindingValue.ToLower()).ToArray();
                if(!possibleBindingTypes.Any())
                {
                    throw new InvalidOperationException(string.Format("No Binding type found with name '{0}'", bindingValue));
                }

                if(possibleBindingTypes.Length > 1)
                {
                    throw new InvalidOperationException(string.Format("BUG: Multiple Binding types declared with name '{0}'", bindingValue));
                }

                bindingType = possibleBindingTypes.Single();
            }

            var binding = (Binding)Activator.CreateInstance(bindingType);
            EndpointAddress address;

            var addressValue = GetConfigurationValue(addressTypeName, parameters);
            lock(Lock)
            {
                if(!string.IsNullOrEmpty(addressValue))
                {
                    if(ClientToAddressesMapping.ContainsKey(clientType))
                    {
                        var addressTypes = ClientToAddressesMapping[clientType];

                        var possibleAddressTypes = addressTypes.Where(x => x.Name.ToLower() == addressValue.ToLower()).ToArray();
                        if(!possibleAddressTypes.Any())
                        {
                            address = new EndpointAddress(addressValue);
                        }
                        else if(possibleAddressTypes.Length > 1)
                        {
                            throw new InvalidOperationException(string.Format("BUG: Multiple Address types declared with name '{0}'", addressValue));
                        }
                        else
                        {
                            address = (EndpointAddress)Activator.CreateInstance(possibleAddressTypes.Single());
                        }
                    }
                    else
                    {
                        address = new EndpointAddress(addressValue);
                    }
                }
                else
                {
                    if(ClientToAddressesMapping.ContainsKey(clientType))
                    {
                        var addressTypes = ClientToAddressesMapping[clientType];

                        if(addressTypes.Length == 1)
                        {
                            address = (EndpointAddress)Activator.CreateInstance(addressTypes.Single());
                        }
                        else
                        {
                            throw new InvalidOperationException(string.Format("No Address type configured for name '{0}'. Possible values are: {1}", addressTypeName, bindingTypes.Select(x => x.Name).Join(", ")));
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException(string.Format("No Address type or config found with name '{0}'", addressTypeName));
                    }
                }
            }

            return (T)Activator.CreateInstance(clientType, binding, address);
        }
    }
}