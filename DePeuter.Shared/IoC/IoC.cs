using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Configuration;
using DePeuter.Shared;
using DePeuter.Shared.Database;

public interface IIoCResolver
{
    object Resolve(Type wantedType, object[] parameters);
}

public static class IoC
{
    public delegate void ValidateImplementationTypesHandler(Type wantedType, Type[] implementationTypes);
    public delegate Type ResolveImplementationTypeHandler(ResolveData data, Type[] implementationTypes);
    public delegate void TypeResolvingFailedHandler(Type wantedType, Exception ex);
    public delegate void TypeResolvingHandler(Type wantedType, object[] parameters);
    public delegate void TypeResolvedHandler(Type wantedType, object implementationObject, long requiredTimeMilliseconds);

    public static event ValidateImplementationTypesHandler ValidateImplementationTypes;
    public static event ResolveImplementationTypeHandler ResolveImplementationType;
    public static event TypeResolvingFailedHandler TypeResolvingFailed;
    public static event TypeResolvingHandler TypeResolving;
    public static event TypeResolvedHandler TypeResolved;

    private static IIoCResolver _resolver = new DefaultIoCResolver();
    public static IIoCResolver Resolver
    {
        get { return _resolver; }
        set { _resolver = value ?? new DefaultIoCResolver(); }
    }

    internal static readonly object Lock = new object();
    private static readonly List<string> LoadedAssemblies = new List<string>();

    private static readonly List<Type> AllImplementationTypes = new List<Type>();
    internal static readonly List<IoCParameter> RegisteredParameters = new List<IoCParameter>();
    private static readonly Dictionary<Type, Type> WantedTypeToImplementationType = new Dictionary<Type, Type>();
    private static readonly Dictionary<Type, Type[]> ImplementationTypes = new Dictionary<Type, Type[]>();
    internal static readonly Dictionary<Type, ParameterInfo[]> WantedTypeParametersInfo = new Dictionary<Type, ParameterInfo[]>();
    private static readonly Dictionary<Type, Func<object>> InitializeShortcuts = new Dictionary<Type, Func<object>>();

    public static void RegisterAssembly(Assembly assembly)
    {
        lock(Lock)
        {
            if(LoadedAssemblies.Contains(assembly.FullName))
            {
                return;
            }
        }

        LoadedAssemblies.Add(assembly.FullName);

        RegisterTypes(assembly.GetLoadedTypes());
    }

    private static void RegisterTypes(Type[] types)
    {
        var loadedTypes = types.Where(x => x.IsInterface || !x.IsAbstract).ToArray();
        AllImplementationTypes.AddRange(loadedTypes.Where(x => !x.IsInterface));
    }

    //een instantie van een interface type opvragen
    public static T Resolve<T>(params object[] parameters) where T : class
    {
        return (T)Resolve(typeof(T), parameters);
    }
    public static T Resolve<T>(IIoCResolver resolver, params object[] parameters) where T : class
    {
        return (T)Resolve(resolver, typeof(T), parameters);
    }

    //Voor speciale gevallen, zoals de IMGCommunicator/ILookup. Dit zou je dan in de Global.asax/program.cs moeten definiëren
    public static void RegisterImplementation<T>(Func<object> initializeType) where T : class
    {
        var wantedType = typeof(T);

        lock(Lock)
        {
            InitializeShortcuts.Set(wantedType, initializeType);
        }
    }

    //Speciale parameters
    public static void RegisterParameter(string parameterName, Type parameterType, Func<object> getParameterValue)
    {
        RegisteredParameters.Add(new IoCParameter(parameterName, parameterType, getParameterValue));
    }

    internal static object Resolve(Type wantedType, object[] parameters)
    {
        return Resolve(Resolver, wantedType, parameters);
    }

    internal static object Resolve(IIoCResolver resolver, Type wantedType, object[] parameters)
    {
        RegisterAssembly(wantedType.Assembly);

        if(TypeResolving != null)
            TypeResolving(wantedType, parameters);

        var sw = Stopwatch.StartNew();

        if(InitializeShortcuts.ContainsKey(wantedType))
        {
            try
            {
                var resShortcut = InitializeShortcuts[wantedType]();

                if(TypeResolved != null)
                    TypeResolved(wantedType, resShortcut, sw.ElapsedMilliseconds);

                return resShortcut;
            }
            catch(Exception ex)
            {
                if(TypeResolvingFailed != null)
                    TypeResolvingFailed(wantedType, ex.Skip<TargetInvocationException>());

                throw;
            }
        }

        try
        {
            var res = (resolver ?? Resolver).Resolve(wantedType, parameters);

            if(TypeResolved != null)
                TypeResolved(wantedType, res, sw.ElapsedMilliseconds);

            return res;
        }
        catch(TargetInvocationException ex)
        {
            if(TypeResolvingFailed != null)
                TypeResolvingFailed(wantedType, ex.Skip<TargetInvocationException>());

            throw new TypeResolvingFailedException(wantedType, ex.InnerException);
        }
        catch(Exception ex)
        {
            if(TypeResolvingFailed != null)
                TypeResolvingFailed(wantedType, ex);

            throw new TypeResolvingFailedException(wantedType, ex);
        }
    }

    public static Type FindImplementationType(ResolveData data, ValidateImplementationTypesHandler validateImplementationTypes = null, ResolveImplementationTypeHandler resolveImplementationType = null)
    {
        var wantedType = data.WantedType;

        if(!wantedType.IsAbstract)
            return wantedType;

        lock(Lock)
        {
            if(WantedTypeToImplementationType.ContainsKey(wantedType))
            {
                return WantedTypeToImplementationType[wantedType];
            }

            Type type = null;
            Type[] types;

            if(ImplementationTypes.ContainsKey(wantedType))
            {
                types = ImplementationTypes[wantedType];
            }
            else
            {
                types = AllImplementationTypes.Where(wantedType.IsAssignableFrom).ToArray();

                if(!types.Any())
                {
                    types = wantedType.Assembly.GetTypes().Where(x => !x.IsAbstract && wantedType.IsAssignableFrom(x)).ToArray();
                }

                if(!types.Any())
                {
                    throw new MissingTypesException(wantedType);
                }

                if(validateImplementationTypes != null)
                {
                    validateImplementationTypes(wantedType, types);
                }
                else if(ValidateImplementationTypes != null)
                {
                    ValidateImplementationTypes(wantedType, types);
                }

                ImplementationTypes.Set(wantedType, types);
            }

            if(resolveImplementationType != null)
            {
                type = resolveImplementationType(data, types.AreNotNull().ToArray());
            }
            else if(ResolveImplementationType != null)
            {
                type = ResolveImplementationType(data, types.AreNotNull().ToArray());
            }

            if(type == null)
            {
                if(types.Length == 1)
                {
                    type = types[0];
                }
                else if(typeof(IBaseDatabaseService).IsAssignableFrom(wantedType))
                {
                    //basic logica voor implementatie van inherited interface van IBaseDatabaseService
                    //dan moet hij obv providername van connectionstring de juiste databaseprovider vinden
                    foreach(var x in types.AreNotNull())
                    {
                        string connectionStringProvider = null;

                        var connectionStringNameAttr = x.GetCustomAttribute<ConnectionStringNameAttribute>() ?? x.GetCustomAttribute<ConnectionStringNameAttribute>(true);
                        if(connectionStringNameAttr != null)
                        {
                            var connectionString = ConfigurationManager.ConnectionStrings[connectionStringNameAttr.Name];
                            if(connectionString == null)
                            {
                                throw new MissingConnectionStringException(connectionStringNameAttr.Name);
                            }

                            connectionStringProvider = connectionString.ProviderName;
                        }

                        if(connectionStringProvider == null)
                        {
                            var registryKeyAttribute = x.GetCustomAttribute<RegistryKeyAttribute>() ?? x.GetCustomAttribute<RegistryKeyAttribute>(true);
                            if(registryKeyAttribute != null)
                            {
                                connectionStringProvider = DePeuterRegistry.Get(registryKeyAttribute.Key + "ProviderName") as string;
                            }
                        }

                        if(connectionStringProvider == null)
                        {
                            continue;
                        }

                        var databaseProviderAttrs = x.GetCustomAttributes<BaseDatabaseProviderAttribute>();
                        if(databaseProviderAttrs == null || !databaseProviderAttrs.Any())
                        {
                            databaseProviderAttrs = x.GetCustomAttributes<BaseDatabaseProviderAttribute>(true);
                        }

                        if(databaseProviderAttrs != null)
                        {
                            foreach(var databaseProviderAttr in databaseProviderAttrs)
                            {
                                if(type != null)
                                {
                                    break;
                                }

                                var provider = DatabaseProviders.GetDatabaseProvider(connectionStringProvider);
                                if(databaseProviderAttr.ProviderType == provider.GetType())
                                {
                                    type = x;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            if(type == null)
            {
                type = types.SingleOrDefault(x => "I" + x.Name == wantedType.Name && x.Namespace == wantedType.Namespace && x.Assembly.FullName == wantedType.Assembly.FullName);

                if(type == null)
                {
                    throw new MultipleTypesException(wantedType, types);
                }
            }

            if(resolveImplementationType == null || ResolveImplementationType == null)
            {
                WantedTypeToImplementationType.Set(wantedType, type);
            }

            return type;
        }
    }

    public static string FindNiscodeParameter(ResolveData data)
    {
        if(!data.Parameters.Any())
        {
            return null;
        }

        IDictionary<string, object> parameterDict = null;
        if(data.Parameters.Count == 1 && data.Parameters[0] != null)
        {
            if(data.Parameters[0] is IDictionary<string, object> || data.Parameters[0].GetType().IsAnonymousType())
            {
                parameterDict = data.Parameters[0].AsLowerDictionary();
            }
        }

        var parameterName = "niscode";
        if(parameterDict != null && parameterDict.ContainsKey(parameterName))
        {
            return parameterDict[parameterName].ToString();
        }

        var parameterType = typeof(NiscodeParameter);
        var possibleParameters = data.Parameters.Where(x => parameterType == x.GetType() || parameterType.IsInstanceOfType(x)).ToArray();
        if(possibleParameters.Length > 1)
        {
            if(TypeResolvingFailed != null)
                TypeResolvingFailed(data.WantedType, new TooManyParameterTypesException(parameterType));

            throw new TooManyParameterTypesException(parameterType);
        }

        if(possibleParameters.Any())
        {
            return possibleParameters.Cast<NiscodeParameter>().Single().Value;
        }

        parameterType = typeof(string);
        possibleParameters = data.Parameters.Where(x => parameterType == x.GetType() || parameterType.IsInstanceOfType(x)).ToArray();
        if(possibleParameters.Length > 1)
        {
            if(TypeResolvingFailed != null)
                TypeResolvingFailed(data.WantedType, new TooManyParameterTypesException(parameterType));

            throw new TooManyParameterTypesException(parameterType);
        }

        if(possibleParameters.Any())
        {
            return possibleParameters.Cast<string>().Single();
        }

        return null;
    }

    internal class IoCParameter
    {
        public string ParameterName { get; private set; }
        public Type ParameterType { get; private set; }
        public Func<object> GetParameterValue { get; private set; }

        public IoCParameter(string parameterName, Type parameterType, Func<object> getParameterValue)
        {
            ParameterName = parameterName;
            ParameterType = parameterType;
            GetParameterValue = getParameterValue;
        }
    }

    public class ResolveData
    {
        public Type WantedType { get; private set; }
        public List<object> Parameters { get; private set; }

        public Type ImplementationType { get; internal set; }

        public object Data { get; set; }

        internal ResolveData(Type wantedType, object[] parameters)
        {
            WantedType = wantedType;
            Parameters = parameters != null ? parameters.ToList() : new List<object>();
        }

        public bool HasParameter<T>()
        {
            return Parameters.Any(x => x is T);
        }
    }
}

public abstract class IoCException : Exception
{
    protected IoCException()
    {
    }

    protected IoCException(string message)
        : base(message)
    {
    }

    protected IoCException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}