using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace DePeuter.Shared
{
    public delegate string GetSupplierHandler(string providerName);

    public class SupplierIoCResolver : DefaultIoCResolver
    {
        protected readonly GetSupplierHandler GetSupplier;
        protected readonly string DefaultSupplierName;

        public SupplierIoCResolver(GetSupplierHandler getSupplier, string defaultSupplierName = null)
        {
            GetSupplier = getSupplier;
            DefaultSupplierName = defaultSupplierName;

            if(getSupplier == null)
            {
                throw new ArgumentNullException("getSupplier");
            }
        }

        protected override Type FindImplementationType(IoC.ResolveData data)
        {
            return IoC.FindImplementationType(data, validateImplementationTypes: ValidateImplementationTypes, resolveImplementationType: ResolveImplementationType);
        }

        private void ValidateImplementationTypes(Type wantedType, Type[] implementationTypes)
        {
            ValidateImplementationTypesProviderAttributes(wantedType, implementationTypes);
            ValidateImplementationTypesSupplierAttributes(wantedType, implementationTypes);
        }

        protected void ValidateImplementationTypesProviderAttributes(Type wantedType, Type[] implementationTypes)
        {
            var providerAttr = wantedType.GetCustomAttribute<ProviderAttribute>() ?? wantedType.GetCustomAttribute<ProviderAttribute>(true);

            if(providerAttr == null)
            {
                var providerNames = implementationTypes.Select(x => x.GetCustomAttribute<ProviderAttribute>() ?? x.GetCustomAttribute<ProviderAttribute>(true)).Where(x => x != null).Select(x => x.Name).Distinct().ToArray();

                if(providerNames.Length > 1)
                {
                    throw new TypeResolvingFailedException(wantedType, "Implementation types have different ProviderAttribute names");
                }
            }
        }

        protected void ValidateImplementationTypesSupplierAttributes(Type wantedType, Type[] implementationTypes)
        {
            var usedSuppliers = new Dictionary<string, Type>();
            foreach(var implementationType in implementationTypes)
            {
                var supplierAttributes = implementationType.GetCustomAttributes<SupplierAttribute>(true);
                if(supplierAttributes.Any())
                {
                    foreach(var supplierAttribute in supplierAttributes)
                    {
                        var supplier = supplierAttribute.Name.ToLower();
                        if(usedSuppliers.ContainsKey(supplier))
                        {
                            throw new TypeResolvingFailedException(wantedType, string.Format("Implementation types '{0}' and '{1}' have conflicting SupplierAttributes '{2}'", usedSuppliers[supplier], implementationType, supplier));
                        }

                        usedSuppliers.Add(supplier, implementationType);
                    }
                }
            }
        }

        private Type ResolveImplementationType(IoC.ResolveData data, Type[] implementationTypes)
        {
            var providerAttr = data.WantedType.GetCustomAttribute<SupplierAttribute>() ?? data.WantedType.GetCustomAttribute<SupplierAttribute>(true);
            if(providerAttr == null)
            {
                providerAttr = implementationTypes.Select(x => x.GetCustomAttribute<SupplierAttribute>() ?? x.GetCustomAttribute<SupplierAttribute>(true)).FirstOrDefault(x => x != null);
            }

            if (providerAttr == null)
            {
                throw new TypeResolvingFailedException(data.WantedType, "No SupplierAttribute found on interface or implementation types");
            }

            var supplierName = GetSupplier(providerAttr.Name) ?? DefaultSupplierName;

            var implementationTypesWithSuppliers = implementationTypes.Select(x =>
            {
                return new
                {
                    Type = x,
                    Suppliers = x.GetCustomAttributes<SupplierAttribute>(true).Where(y => y.Name != null).Select(y => y.Name.ToUpper()).ToArray()
                };
            }).ToArray();

            Type type = null;
            if(!string.IsNullOrEmpty(supplierName))
            {
                type = implementationTypesWithSuppliers.Where(x => x.Suppliers.Contains(supplierName.ToUpper())).Select(x => x.Type).SingleOrDefault();
            }

            if(type == null)
            {
                var possibleLinks = implementationTypesWithSuppliers.SelectMany(x => x.Suppliers).OrderBy(x => x).ToArray();

                var msg = string.IsNullOrEmpty(supplierName) ? string.Format("No supplier configured for provider '{0}'.", providerAttr.Name) : string.Format("Supplier value '{0}' for provider '{1}' is invalid.", supplierName, providerAttr.Name);
                throw new InvalidOperationException(string.Format("{0}\nPossible suppliers: {1}", msg, possibleLinks.Join(", ")));
            }
            
            return type;
        }
    }
}