using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DePeuter.Shared
{
    public class DefaultIoCResolver : IIoCResolver
    {
        public object Resolve(Type wantedType, object[] parameters)
        {
            var data = new IoC.ResolveData(wantedType, parameters);

            var type = FindImplementationType(data);
            data.ImplementationType = type;
            var parametersInfo = FindImplementationTypeConstructorParameters(wantedType, type);
            var resolvedParameters = ResolveParameters(data, parametersInfo);

            return resolvedParameters.Any() ? Activator.CreateInstance(type, resolvedParameters) : Activator.CreateInstance(type);
        }

        protected static ParameterInfo[] FindImplementationTypeConstructorParameters(Type wantedType, Type implementationType)
        {
            lock (IoC.Lock)
            {
                if (IoC.WantedTypeParametersInfo.ContainsKey(wantedType))
                    return IoC.WantedTypeParametersInfo[wantedType];

                var constructors = implementationType.GetConstructors();
                if (!constructors.Any())
                {
                    throw new MissingPublicConstructorException(implementationType);
                }
                var constructor = constructors.OrderByDescending(c => c.GetParameters().Length).First();
                var constructorParameters = constructor.GetParameters().OrderBy(x => x.Position).ToArray();

                IoC.WantedTypeParametersInfo.Set(wantedType, constructorParameters);

                return constructorParameters;
            }
        }

        protected virtual Type FindImplementationType(IoC.ResolveData data)
        {
            return IoC.FindImplementationType(data);
        }

        protected object[] ResolveParameters(IoC.ResolveData data, ParameterInfo[] parametersInfo)
        {
            var result = GetParameters(data, parametersInfo);

            for(var i = 0; i < parametersInfo.Length; i++)
            {
                if(result[i] != null)
                {
                    continue;
                }

                var parameterInfo = parametersInfo[i];

                //custom rules
                var possibleRegisteredParameters = IoC.RegisteredParameters.Where(x => string.Equals(x.ParameterName, parameterInfo.Name, StringComparison.OrdinalIgnoreCase) && (parameterInfo.ParameterType == x.ParameterType || parameterInfo.ParameterType.IsInstanceOfType(x.ParameterType))).ToArray();
                if(possibleRegisteredParameters.Length > 1)
                {
                    throw new TooManyRegisteredParameterTypesException(parameterInfo.ParameterType);
                }

                var possibleRegisteredParameter = possibleRegisteredParameters.SingleOrDefault();
                if(possibleRegisteredParameter != null)
                {
                    result[i] = possibleRegisteredParameter.GetParameterValue();
                    continue;
                }

                if(parameterInfo.ParameterType.IsSimpleType())
                {
                    result[i] = null;
                    continue;
                }

                try
                {
                    //default action
                    result[i] = IoC.Resolve(this, parameterInfo.ParameterType, data.Parameters.ToArray());
                }
                catch
                {
                    if(data.WantedType.IsInterface && !parameterInfo.ParameterType.IsInterface)
                    {
                        throw new MissingParameterException(FindImplementationType(data), parameterInfo.ParameterType);
                    }

                    throw;
                }
            }

            return result;
        }

        protected object[] GetParameters(IoC.ResolveData data, ParameterInfo[] parametersInfo)
        {
            var result = new object[parametersInfo.Length];

            IDictionary<string, object> parameterDict = null;
            if(data.Parameters.Count == 1 && data.Parameters[0] != null)
            {
                if(data.Parameters[0] is IDictionary<string, object> || data.Parameters[0].GetType().IsAnonymousType())
                {
                    parameterDict = data.Parameters[0].AsLowerDictionary();
                }
            }

            for(var i = 0; i < parametersInfo.Length; i++)
            {
                var parameterInfo = parametersInfo[i];

                if(parameterDict != null && parameterDict.ContainsKey(parameterInfo.Name.ToLower()))
                {
                    result[i] = parameterDict[parameterInfo.Name.ToLower()];
                    continue;
                }

                //user rules
                var possibleParameters = data.Parameters.Where(x => parameterInfo.ParameterType == x.GetType() || parameterInfo.ParameterType.IsInstanceOfType(x)).ToArray();
                if(possibleParameters.Length > 1)
                {
                    throw new TooManyParameterTypesException(parameterInfo.ParameterType);
                }

                var possibleParameter = possibleParameters.SingleOrDefault();
                if(possibleParameter != null)
                {
                    result[i] = possibleParameter;
                    continue;
                }

                result[i] = null;
            }

            return result;
        }

        protected void ValidateImplementationTypesConnectionStringNameAttributes(Type wantedType, Type[] implementationTypes)
        {
            var connectionStringAttr = wantedType.GetCustomAttribute<ConnectionStringNameAttribute>() ?? wantedType.GetCustomAttribute<ConnectionStringNameAttribute>(true);

            if (connectionStringAttr == null)
            {
                var connectionStringNames = implementationTypes.Select(x => x.GetCustomAttribute<ConnectionStringNameAttribute>() ?? x.GetCustomAttribute<ConnectionStringNameAttribute>(true)).Where(x => x != null).Select(x => x.Name).Distinct().ToArray();

                //indien er geen connectionstringname is dan wordt de default GEOIT genomen
                //if(connectionStringNames.Length == 0)
                //{
                //    throw new TypeResolvingFailedException(wantedType, "No ConnectionStringNameAttribute found on interface or implementation types");
                //}
                if (connectionStringNames.Length > 1)
                {
                    throw new TypeResolvingFailedException(wantedType, "Implementation types have different ConnectionStringNameAttribute names");
                }
            }
        }
    }
}