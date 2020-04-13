using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Reflection;
using log4net;

public static class EF
{
    private static readonly ILog Log = LogManager.GetLogger("Global");

    private static readonly List<string> LoadedAssemblies = new List<string>();
    private static List<Type> _allTypes;

    private static readonly object Lock = new object();

    private static bool _triggersEnabled = true;
    public static bool TriggersEnabled
    {
        get { return _triggersEnabled; }
        set { _triggersEnabled = value; }
    }

    public static bool AuditEnabled { get; set; }

    //private static Dictionary<IFieldRule, BaseDatabaseProviderAttribute[]> _fieldRulesOnUpdate = new Dictionary<IFieldRule, BaseDatabaseProviderAttribute[]>();
    //private static Dictionary<IFieldRule, BaseDatabaseProviderAttribute[]> _fieldRulesOnInsert = new Dictionary<IFieldRule, BaseDatabaseProviderAttribute[]>();
    //private static Dictionary<IFieldRule, BaseDatabaseProviderAttribute[]> _fieldRulesOnSelect = new Dictionary<IFieldRule, BaseDatabaseProviderAttribute[]>();

    private static readonly Dictionary<TriggerAction, List<IEntityTrigger>> GeneralTriggers = new Dictionary<TriggerAction, List<IEntityTrigger>>();
    private static readonly Dictionary<Type, Dictionary<TriggerAction, List<IEntityTrigger>>> Triggers = new Dictionary<Type, Dictionary<TriggerAction, List<IEntityTrigger>>>();
    private static readonly Dictionary<Type, Type[]> TriggerMapping = new Dictionary<Type, Type[]>();

    private static readonly object AllRelationsLockObject = new object();
    private static readonly List<SqlRelationAttribute> AllRelations = new List<SqlRelationAttribute>();

    private static readonly List<Type> IgnoreTriggers = new List<Type>();
    private static readonly Dictionary<string, Type> SchemaTableTypeMapping = new Dictionary<string, Type>();
    private static readonly Dictionary<Type, SchemaTableInfo> TypeSchemaTableMapping = new Dictionary<Type, SchemaTableInfo>();

    public static void DisableTrigger(Type type)
    {
        if(!IgnoreTriggers.Contains(type))
        {
            IgnoreTriggers.Add(type);
        }
    }

    public static void EnableTrigger(Type type)
    {
        IgnoreTriggers.Remove(type);
    }

    [Obsolete("Lazy loading is now implemented")]
    public static void Initialize()
    {
        //RegisterAssemblies();
    }

    //Types van een assembly inladen
    //public static void RegisterAssemblies()
    //{
    //    if(_allTypes != null) return;
    //    _allTypes = new List<Type>();

    //    AppDomain.CurrentDomain.OnAllLoadedTypes(RegisterTypes);
    //}
    public static void RegisterAssembly(Assembly assembly)
    {
        lock(Lock)
        {
            if(LoadedAssemblies.Contains(assembly.FullName))
            {
                return;
            }
        }

        if(_allTypes == null)
        {
            _allTypes = new List<Type>();
        }

        LoadedAssemblies.Add(assembly.FullName);

        RegisterTypes(assembly.GetLoadedTypes());
    }

    //public static void RegisterFieldRules(params Type[] types)
    //{
    //    var loadedTypes = types.Where(x => !x.IsAbstract).ToArray();
    //    foreach(var type in loadedTypes)
    //    {
    //        _allTypes.Remove(type);
    //    }
    //    _allTypes.AddRange(loadedTypes);

    //    UpdateFieldRules();
    //}

    //private static void UpdateFieldRules()
    //{
    //    _fieldRulesOnUpdate = _allTypes.Where(x => typeof(FieldRuleOnUpdateAndInsert).IsAssignableFrom(x) || typeof(FieldRuleOnUpdate).IsAssignableFrom(x)).
    //        Select(x => new
    //        {
    //            instance = (IFieldRule)Activator.CreateInstance(x),
    //            providers = x.GetCustomAttributes<BaseDatabaseProviderAttribute>(true)
    //        }).ToDictionary(x => x.instance, x => x.providers.Any() ? x.providers : null);

    //    _fieldRulesOnInsert = _allTypes.Where(x => typeof(FieldRuleOnUpdateAndInsert).IsAssignableFrom(x) || typeof(FieldRuleOnInsert).IsAssignableFrom(x)).
    //        Select(x => new
    //        {
    //            instance = (IFieldRule)Activator.CreateInstance(x),
    //            providers = x.GetCustomAttributes<BaseDatabaseProviderAttribute>(true)
    //        }).ToDictionary(x => x.instance, x => x.providers.Any() ? x.providers : null);

    //    _fieldRulesOnSelect = _allTypes.Where(x => typeof(FieldRuleOnSelect).IsAssignableFrom(x)).
    //        Select(x => new
    //        {
    //            instance = (IFieldRule)Activator.CreateInstance(x),
    //            providers = x.GetCustomAttributes<BaseDatabaseProviderAttribute>(true)
    //        }).ToDictionary(x => x.instance, x => x.providers.Any() ? x.providers : null);
    //}
    private static void UpdateTriggers()
    {
        GeneralTriggers.Clear();
        Triggers.Clear();
        TriggerMapping.Clear();

        var allTriggers = _allTypes.Where(x => typeof(IEntityTrigger).IsAssignableFrom(x) && x.IsAbstract == false).ToList();
        foreach(var type in allTriggers)
        {
            var triggerActions = type.GetCustomAttributes<TriggerActionAttribute>(true).SelectMany(x => x.Action.GetFlags<TriggerAction>()).ToArray();
            var entityTypes = type.GetCustomAttributes<EntityTypeAttribute>(true).Select(x => x.Type).ToArray();

            // general trigger
            if(!entityTypes.Any())
            {
                foreach(var triggerAction in triggerActions)
                {
                    if(!GeneralTriggers.ContainsKey(triggerAction))
                    {
                        GeneralTriggers.Add(triggerAction, new List<IEntityTrigger>());
                    }

                    if(!GeneralTriggers[triggerAction].Any(x => x.GetType().Equals(type)))
                    {
                        GeneralTriggers[triggerAction].Add((IEntityTrigger)Activator.CreateInstance(type));
                    }
                }
            }
            else
            {
                //entity trigger
                foreach(var entityType in entityTypes)
                {
                    if(!Triggers.ContainsKey(entityType))
                    {
                        Triggers.Add(entityType, new Dictionary<TriggerAction, List<IEntityTrigger>>());
                    }

                    var entityTriggers = Triggers[entityType];
                    foreach(var triggerAction in triggerActions)
                    {
                        if(!entityTriggers.ContainsKey(triggerAction))
                        {
                            entityTriggers.Add(triggerAction, new List<IEntityTrigger>());
                        }

                        if(!entityTriggers[triggerAction].Any(x => x.GetType().Equals(type)))
                        {
                            entityTriggers[triggerAction].Add((IEntityTrigger)Activator.CreateInstance(type));
                        }
                    }
                }
            }
        }
    }
    private static void UpdateRelations()
    {
        lock(AllRelationsLockObject)
        {
            AllRelations.Clear();

            var relations = _allTypes.SelectMany(x => x.GetCustomAttributes<SqlRelationAttribute>(true));
            AllRelations.AddRange(relations);
        }
    }

    private static void RegisterTypes(Type[] types)
    {
        var loadedTypes = types.Where(x => !x.IsAbstract).ToArray();
        _allTypes.AddRange(loadedTypes);

        // FieldRules
        //UpdateFieldRules();

        // Triggers
        UpdateTriggers();

        // Relations
        UpdateRelations();
    }

    public class CloningEventArgs : EventArgs
    {
        public Type SourceType { get; private set; }
        public object Source { get; private set; }
        public Type CloneType { get; private set; }
        public object Clone { get; private set; }

        public CloningEventArgs(Type sourceType, object source, Type cloneType, object clone)
        {
            SourceType = sourceType;
            Source = source;
            CloneType = cloneType;
            Clone = clone;
        }
    }
    public static event EventHandler<CloningEventArgs> Cloning;

    public static T Clone<T>(T source) where T : class, new()
    {
        return (T)Clone(source.GetType(), source, null, null);
    }
    public static T Clone<T>(T source, Type cloneType) where T : class, new()
    {
        return (T)Clone(source.GetType(), source, cloneType, null);
    }
    public static T Clone<T>(T source, Type cloneType, EventHandler<CloningEventArgs> cloning) where T : class, new()
    {
        return (T)Clone(source.GetType(), source, cloneType, cloning);
    }
    public static T CloneAs<T>(object source, EventHandler<CloningEventArgs> cloning = null) where T : class, new()
    {
        return (T)Clone(source.GetType(), source, typeof(T), cloning);
    }
    public static object Clone(Type sourceType, object source)
    {
        return Clone(sourceType, source, null, null);
    }
    public static object Clone(Type sourceType, object source, Type cloneType, EventHandler<CloningEventArgs> cloning, object sender = null)
    {
        if(cloneType == null)
        {
            cloneType = sourceType;
        }

        var clone = Activator.CreateInstance(cloneType);

        string clonePrimarykeyName = null;
        try
        {
            clonePrimarykeyName = GetSqlPrimaryKeyName(cloneType);
        }
        catch(MissingSqlPrimaryKeyAttributeException) { }

        var fieldNamingType = GetSqlFieldNamingType(cloneType);

        Func<PropertyInfo, bool> piFunc = x =>
        {
            if(!x.CanWrite || !x.CanRead) return false;

            if(x.HasCustomAttribute<IgnoreAttribute>())
            {
                return false;
            }

            if(clonePrimarykeyName != null)
            {
                var name = GetFieldName(x, fieldNamingType);

                if(name == clonePrimarykeyName)
                    return false;
            }

            return true;
        };

        var sourceProperties = sourceType.GetProperties().Where(piFunc).ToArray();
        var cloneProperties = cloneType != sourceType ? cloneType.GetProperties().Where(piFunc).ToArray() : sourceProperties;

        foreach(var sourcePi in sourceProperties)
        {
            var clonePi = cloneProperties.SingleOrDefault(x => x.Name == sourcePi.Name);
            if(clonePi != null)
            {
                clonePi.SetValue(clone, sourcePi.GetValue(source, null), null);
            }
        }

        var sourceFields = sourceType.GetFields(BindingFlags.Public | BindingFlags.Instance);
        var cloneFields = cloneType != sourceType ? sourceType.GetFields(BindingFlags.Public | BindingFlags.Instance) : sourceFields;

        foreach(var sourceField in sourceFields)
        {
            var cloneField = cloneFields.SingleOrDefault(x => x.Name == sourceField.Name);
            if(cloneField != null)
            {
                cloneField.SetValue(clone, sourceField.GetValue(source));
            }
        }

        var e = new CloningEventArgs(sourceType, source, cloneType, clone);

        if(Cloning != null)
        {
            Cloning(sender, e);
        }

        if(cloning != null)
        {
            cloning(sender, e);
        }

        return clone;
    }

    public static IEntityTrigger[] GetTriggers<TEntity>(TriggerAction action)
    {
        return GetTriggers(typeof(TEntity), action);
    }
    public static IEntityTrigger[] GetTriggers(Type type, TriggerAction action)
    {
        RegisterAssembly(type.Assembly);

        if(!TriggersEnabled) return new IEntityTrigger[] { };

        if(!TriggerMapping.ContainsKey(type))
        {
            var triggerTypes = Triggers.Keys.ToArray();

            var matchingTriggerTypes = triggerTypes.Where(x => x.IsAssignableFrom(type) || type.Equals(x)).ToArray();

            TriggerMapping.Add(type, matchingTriggerTypes);
        }

        var mappedTriggerTypes = TriggerMapping[type];
        var res = new List<IEntityTrigger>();

        //general triggers
        if(GeneralTriggers.ContainsKey(action))
        {
            var generalTriggers = GeneralTriggers[action];

            foreach(var trigger in generalTriggers)
            {
                var ignoreTrigger = IgnoreTriggers.Any(x => x.IsInstanceOfType(trigger) || trigger.GetType().Equals(x));
                if(!ignoreTrigger)
                {
                    res.Add(trigger);
                }
            }
        }

        //entity triggers
        foreach(var triggerType in mappedTriggerTypes)
        {
            if(!Triggers.ContainsKey(triggerType))
            {
                continue;
            }

            var dict1 = Triggers[triggerType];
            if(!dict1.ContainsKey(action))
            {
                continue;
            }

            var validTriggers = dict1[action];

            foreach(var trigger in validTriggers)
            {
                var ignoreTrigger = IgnoreTriggers.Any(x => x.IsInstanceOfType(trigger) || trigger.GetType().Equals(x));
                if(!ignoreTrigger)
                {
                    res.Add(trigger);
                }
            }
        }

        return res.OrderBy(x =>
            {
                try
                {
                    return x.Order;
                }
                catch(NotImplementedException)
                {
                    return int.MaxValue;
                }
            }).ToArray();
    }

    public static List<SqlRelationAttribute> GetRelations(string schema, string table)
    {
        lock (AllRelationsLockObject)
        {
            return AllRelations.Where(x => (x.SelfSchema.ToUpper() == schema.ToUpper() && x.SelfTable.ToUpper() == table.ToUpper()) || (x.OtherSchema.ToUpper() == schema.ToUpper() && x.OtherTable.ToUpper() == table.ToUpper())).Distinct().ToList();
        }
    }

    public static List<SqlRelationAttribute> GetRelations(string schema, string table, string field)
    {
        lock (AllRelationsLockObject)
        {
            return AllRelations.Where(x => (x.SelfSchema.ToUpper() == schema.ToUpper() && x.SelfTable.ToUpper() == table.ToUpper() && x.SelfField.ToUpper() == field.ToUpper()) || x.OtherSchema.ToUpper() == schema.ToUpper() && x.OtherTable.ToUpper() == table.ToUpper() && x.OtherField.ToUpper() == field.ToUpper()).Distinct().ToList();
        }
    }

    public static List<SqlRelationAttribute> GetRelationsFrom(string schema, string table)
    {
        lock (AllRelationsLockObject)
        {
            return AllRelations.Where(x => x.SelfSchema.ToUpper() == schema.ToUpper() && x.SelfTable.ToUpper() == table.ToUpper()).Distinct().ToList();
        }
    }

    public static List<SqlRelationAttribute> GetRelationsFrom(string schema, string table, string field)
    {
        lock (AllRelationsLockObject)
        {
            return AllRelations.Where(x => x.SelfSchema.ToUpper() == schema.ToUpper() && x.SelfTable.ToUpper() == table.ToUpper() && x.SelfField.ToUpper() == field.ToUpper()).Distinct().ToList();
        }
    }

    public static List<SqlRelationAttribute> GetRelationsTo(string schema, string table)
    {
        lock (AllRelationsLockObject)
        {
            return AllRelations.Where(x => x.OtherSchema.ToUpper() == schema.ToUpper() && x.OtherTable.ToUpper() == table.ToUpper()).Distinct().ToList();
        }
    }

    public static List<SqlRelationAttribute> GetRelationsTo(string schema, string table, string field)
    {
        lock (AllRelationsLockObject)
        {
            return AllRelations.Where(x => x.OtherSchema.ToUpper() == schema.ToUpper() && x.OtherTable.ToUpper() == table.ToUpper() && x.OtherField.ToUpper() == field.ToUpper()).Distinct().ToList();
        }
    }

    private class RelationPath
    {
        public RelationPath PreviousPath { get; set; }

        private List<RelationPath> _NextPaths = new List<RelationPath>();
        public List<RelationPath> NextPaths
        {
            get { return _NextPaths; }
        }

        public string Schema { get; set; }
        public string Table { get; set; }

        public SqlRelationAttribute SqlRelation { get; set; }

        public void AddNextPath(RelationPath nextPath)
        {
            if(nextPath != null)
            {
                _NextPaths.Add(nextPath);
            }
        }
    }

    public static List<SqlRelationAttribute> FindRelationsTo(string fromSchema, string fromTable, Func<string, string, bool> isDestination, IEnumerable<SqlRelationAttribute> blacklist)
    {
        if(isDestination(fromSchema, fromTable))
        {
            return new List<SqlRelationAttribute>();
        }

        var paths = BuildPathsTo(null, fromSchema, fromTable, isDestination, null, null, blacklist, null);

        List<SqlRelationAttribute> res = new List<SqlRelationAttribute>();

        BuildShortestPath(ref res, paths, isDestination, null, null);

        return res;
    }

    private static Dictionary<string, List<SqlRelationAttribute>> cachedPaths = new Dictionary<string, List<SqlRelationAttribute>>();
    public static List<SqlRelationAttribute> GetRelationsTo(string fromSchema, string fromTable, string toSchema, string toTable, IEnumerable<SqlRelationAttribute> blacklist)
    {
        if(fromSchema == toSchema && fromTable == toTable)
        {
            return new List<SqlRelationAttribute>();
        }

        var pathName = string.Format("{0}.{1}->{2}.{3}", fromSchema, fromTable, toSchema, toTable);
        if(cachedPaths.ContainsKey(pathName))
        {
            return cachedPaths[pathName];
        }

        var paths = BuildPathsTo(null, fromSchema, fromTable, toSchema, toTable, null, null, blacklist, null);

        List<SqlRelationAttribute> res = new List<SqlRelationAttribute>();

        BuildShortestPath(ref res, paths, toSchema, toTable, null, null);

        if(!cachedPaths.ContainsKey(pathName))
        {
            cachedPaths.Add(pathName, res);
        }

        return res;
    }

    private static bool BuildShortestPath(ref List<SqlRelationAttribute> res, RelationPath paths, string wantedSchema, string wantedTable, string previousSchema, string previousTable)
    {
        return BuildShortestPath(ref res, paths, (schema, table) => schema == wantedSchema && table == wantedTable, previousSchema, previousTable);
    }

    private static bool BuildShortestPath(ref List<SqlRelationAttribute> res, RelationPath paths, Func<string, string, bool> isDestination, string previousSchema, string previousTable)
    {
        foreach(var nextPath in paths.NextPaths)
        {
            if(isDestination(nextPath.Schema, nextPath.Table))
            {
                res.Insert(0, nextPath.SqlRelation);
                return true;
            }

            var foundWanted = BuildShortestPath(ref res, nextPath, isDestination, paths.Schema, paths.Table);
            if(foundWanted)
            {
                //check if relation needs to be turned around, fex. 'gebouw -> terrobj <- tobjhnr', should be 'gebouw -> terrobj -> tobjhnr'
                if(nextPath.SqlRelation.SelfSchema == paths.Schema && nextPath.SqlRelation.SelfTable == paths.Table)
                {
                    res.Insert(0, nextPath.SqlRelation);
                }
                else
                {
                    res.Insert(0, nextPath.SqlRelation.CreateReverse());
                }
                return true;
            }
        }

        return false;
    }

    private static RelationPath BuildPathsTo(RelationPath path, string currentSchema, string currentTable, string wantedSchema, string wantedTable, string previousSchema, string previousTable, IEnumerable<SqlRelationAttribute> blacklist, List<string> usedPaths)
    {
        return BuildPathsTo(path, currentSchema, currentTable, (schema, table) => schema == wantedSchema && table == wantedTable, previousSchema, previousTable, blacklist, usedPaths);
    }
    private static RelationPath BuildPathsTo(RelationPath path, string currentSchema, string currentTable, Func<string, string, bool> isDestination, string previousSchema, string previousTable, IEnumerable<SqlRelationAttribute> blacklist, List<string> usedPaths)
    {
        if(usedPaths == null)
        {
            usedPaths = new List<string>();
        }
        if(path == null)
        {
            path = new RelationPath();
        }

        path.Schema = currentSchema;
        path.Table = currentTable;

        var relationsFrom = GetRelationsFrom(currentSchema, currentTable);
        var relationsTo = GetRelationsTo(currentSchema, currentTable);

        foreach(var relation in relationsFrom)
        {
            //illegal paths
            if(blacklist != null && blacklist.Contains(relation))
            {
                continue;
            }

            //when you go down the rabbit hole... no turning backsies
            dynamic currentPath = relation.ToString();
            if(usedPaths.Contains(currentPath))
            {
                continue;
            }
            usedPaths.Add(currentPath);

            //are we there yet?
            if(isDestination(relation.OtherSchema, relation.OtherTable))
            {
                //yes!
                RelationPath nextPath = new RelationPath();
                nextPath.PreviousPath = path;
                nextPath.Schema = relation.OtherSchema;
                nextPath.Table = relation.OtherTable;
                nextPath.SqlRelation = relation;
                path.AddNextPath(nextPath);
                //Return path
            }
            else
            {
                //no :( go further down the hole
                RelationPath nextPath = new RelationPath();
                nextPath.PreviousPath = path;
                nextPath.SqlRelation = relation;
                nextPath = BuildPathsTo(nextPath, relation.OtherSchema, relation.OtherTable, isDestination, currentSchema, currentTable, blacklist, usedPaths);
                path.AddNextPath(nextPath);
            }
        }

        foreach(var relation in relationsTo)
        {
            //illegal paths
            if(blacklist != null && blacklist.Contains(relation))
            {
                continue;
            }

            //when you go down the rabbit hole... no turning back
            var currentPath = relation.ToString();
            if(usedPaths.Contains(currentPath))
            {
                continue;
            }
            usedPaths.Add(currentPath);

            //are we there yet?
            if(isDestination(relation.SelfSchema, relation.SelfTable))
            {
                //yes!
                RelationPath nextPath = new RelationPath();
                nextPath.PreviousPath = path;
                nextPath.Schema = relation.SelfSchema;
                nextPath.Table = relation.SelfTable;
                nextPath.SqlRelation = relation;
                path.AddNextPath(nextPath);
                //Return path
            }
            else
            {
                //no :( go further down the hole
                RelationPath nextPath = new RelationPath();
                nextPath.PreviousPath = path;
                nextPath.SqlRelation = relation;
                nextPath = BuildPathsTo(nextPath, relation.SelfSchema, relation.SelfTable, isDestination, currentSchema, currentTable, blacklist, usedPaths);
                path.AddNextPath(nextPath);
            }
        }

        return path;
    }

    public static Type GetType(string schema, string table)
    {
        var key = (schema + "_" + table).ToLower();

        lock(Lock)
        {
            if(!SchemaTableTypeMapping.ContainsKey(key))
            {
                var types = _allTypes.Where(x =>
                {
                    try
                    {
                        var info = GetSqlSchemaTableInfo(x);

                        return info.Schema.ToLower() == (schema ?? string.Empty).ToLower() && info.Table.ToLower() == table.ToLower();
                    }
                    catch(MissingSqlTableAttributeException)
                    {
                        return false;
                    }
                }).ToArray();

                var type = types.FirstOrDefault();
                if(types.Length > 1)
                {
                    type = types.FirstOrDefault(x => x.Name.ToLower() == key);
                    if(type == null) type = types.First();
                }

                SchemaTableTypeMapping.Add(key, type);
            }
        }

        try
        {
            return SchemaTableTypeMapping[key];
        }
        catch(KeyNotFoundException)
        {
            throw new KeyNotFoundException(string.Format("No type found for {0}.{1}", schema, table));
        }
    }
    public static Type GetType(Assembly assembly, string schema, string table)
    {
        RegisterAssembly(assembly);

        var key = (schema + "_" + table).ToLower();

        lock(Lock)
        {
            if(!SchemaTableTypeMapping.ContainsKey(key))
            {
                var types = assembly.GetLoadedTypes().Select(x =>
                {
                    try
                    {
                        var info = GetSqlSchemaTableInfo(x);

                        return new { Type = x, Info = info };
                    }
                    catch(MissingSqlTableAttributeException)
                    {
                        return null;
                    }
                }).Where(x => x != null).ToArray();

                foreach(var x in types)
                {
                    var typeKey = (x.Info.Schema + "_" + x.Info.Table).ToLower();

                    if(!SchemaTableTypeMapping.ContainsKey(typeKey))
                    {
                        SchemaTableTypeMapping.Add(typeKey, x.Type);
                    }
                }

                if(!SchemaTableTypeMapping.ContainsKey(key))
                {
                    SchemaTableTypeMapping.Add(key, null);
                }

                //var type = types.FirstOrDefault();
                //if(types.Length > 1)
                //{
                //    type = types.FirstOrDefault(x => x.Name.ToUpper() == key.ToUpper());
                //    if(type == null) type = types.First();
                //}

                //SchemaTableTypeMapping.Add(key, type);
            }
        }

        try
        {
            return SchemaTableTypeMapping[key];
        }
        catch(KeyNotFoundException)
        {
            throw new KeyNotFoundException(string.Format("No type found for {0}.{1} in assembly {2}", schema, table, assembly.Location));
        }
    }

    public static SchemaTableInfo GetSqlSchemaTableInfo<T>() where T : class
    {
        return GetSqlSchemaTableInfo(typeof(T));
    }
    public static SchemaTableInfo GetSqlSchemaTableInfo(Type type)
    {
        lock(Lock)
        {
            if(!TypeSchemaTableMapping.ContainsKey(type))
            {
                //cascading
                var tableAttr = type.GetCustomAttribute<SqlTableAttribute>(false);
                if(tableAttr == null)
                {
                    tableAttr = type.GetCustomAttribute<SqlTableAttribute>(true);
                }
                if(tableAttr == null)
                {
                    throw new MissingSqlTableAttributeException(type);
                }

                var schemaAttr = type.GetCustomAttribute<SqlSchemaAttribute>(false);
                if(schemaAttr == null)
                {
                    schemaAttr = type.GetCustomAttribute<SqlSchemaAttribute>(true);
                }
                TypeSchemaTableMapping.Add(type, new SchemaTableInfo(schemaAttr != null ? schemaAttr.Schema : string.Empty, tableAttr.Table));
            }
        }

        return TypeSchemaTableMapping[type];
    }

    public static string GetSqlPrimaryKeyName<T>() where T : class
    {
        return GetSqlPrimaryKeyName(typeof(T));
    }
    public static string GetSqlPrimaryKeyName(Type type)
    {
        var primarykey = type.GetCustomAttribute<SqlPrimaryKeyAttribute>(true);
        if(primarykey == null)
        {
            throw new MissingSqlPrimaryKeyAttributeException(type);
        }
        return primarykey.Name;
    }

    public static FieldNamingType? GetSqlFieldNamingType(Type type)
    {
        var attr = type.GetCustomAttribute<SqlFieldNamingTypeAttribute>(true);
        if(attr != null)
        {
            return attr.FieldNamingType;
        }
        return null;
    }

    private static readonly object FieldNamesLock = new object();
    private static readonly Dictionary<PropertyInfo, string> FieldNames = new Dictionary<PropertyInfo, string>();
    public static string GetFieldName(PropertyInfo pi, FieldNamingType? defaultFieldNamingType = null)
    {
        lock(FieldNamesLock)
        {
            if(!FieldNames.ContainsKey(pi))
            {
                var attr = pi.GetCustomAttribute<SqlFieldAttribute>();
                FieldNames.Add(pi, attr != null ? attr.GetName(pi, defaultFieldNamingType) : string.Empty);
            }

            return FieldNames[pi];
        }
    }

    private static readonly Dictionary<Type, PropertyInfo[]> TypeSelectProperties = new Dictionary<Type, PropertyInfo[]>();

    public static PropertyInfo[] GetTypeSelectProperties(Type type)
    {
        FillTypeSelectProperties(type);

        return TypeSelectProperties[type];
    }
    private static void FillTypeSelectProperties(Type type)
    {
        lock(Lock)
        {
            if(TypeSelectProperties.ContainsKey(type)) return;

            var properties = new List<PropertyInfo>();

            foreach(var pi in type.GetProperties().Where(pi => pi.CanWrite))
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
                    FillTypeSelectProperties(pi.PropertyType);
                }
            }

            TypeSelectProperties.Set(type, properties.ToArray());
        }
    }

    public static List<T> ConvertToSqlEntities<T>(DataTable dt)
    {
        return ConvertToSqlEntities(dt, typeof(T)).Select(x => x.CastTo<T>()).ToList();
    }

    public static List<object> ConvertToSqlEntities(DataTable dt, Type type)
    {
        var res = new List<object>();

        if(dt.Rows.Count == 0) return res;

        //var columns = (from DataColumn c in dt.Columns
        //               from p in type.GetProperties()
        //               where c.ColumnName.ToUpper() == EF.GetFieldName(p).Trim('"').ToUpper() && p.CanWrite && p.GetSetMethod(true).IsPublic
        //               select new { db = c, entity = p }).ToArray();

        if(type.IsSystemType())
        {
            //var nullValue = FormatterServices.GetUninitializedObject(type);
            var nullValue = type.GetDefaultValue();

            foreach(DataRow dr in dt.Rows)
            {
                var value = dr[0];
                res.Add(value is DBNull ? nullValue : value);
            }
        }
        else
        {
            foreach(DataRow dr in dt.Rows)
            {
                res.Add(CreateSqlEntity(type, dr));
            }
        }

        return res;
    }

    public static object CreateSqlEntity(Type type, DataRow dr)
    {
        return CreateSqlEntity(type, dr, null);
    }

    public static object CreateSqlEntity(Type type, DataRow dr, Action<object, PropertyInfo, object> setPropertyValue)
    {
        var e = Activator.CreateInstance(type, true);

        if(setPropertyValue == null)
        {
            setPropertyValue = (entity, pi, value) =>
            {
                pi.SetValue(entity, value, null);
            };
        }

        var properties = GetTypeSelectProperties(type);
        var fieldNamingType = GetSqlFieldNamingType(type);

        foreach(var pi in properties)
        {
            if(pi.HasCustomAttribute<SqlEntityAttribute>())
            {
                var eChild = CreateSqlEntity(pi.PropertyType, dr, setPropertyValue);
                pi.SetValue(e, eChild, null);
            }
            else
            {
                //field
                var fieldName = GetFieldName(pi, fieldNamingType).Trim('"');
                if(!dr.Table.Columns.Contains(fieldName))
                {
                    continue;
                }

                object value = null;
                try
                {
                    value = dr[fieldName];

                    if(value is DBNull)
                    {
                        var defaultValueAttr = pi.GetCustomAttribute<SqlDefaultValueAttribute>();
                        value = defaultValueAttr != null ? defaultValueAttr.SelectValue : null;
                    }

                    var propertyType = Nullable.GetUnderlyingType(pi.PropertyType) ?? pi.PropertyType;

                    if(value != null && propertyType.IsEnum)
                    {
                        value = Enum.Parse(propertyType, value.ToString());
                    }

                    var attr = pi.GetCustomAttribute<SqlFieldAttribute>();
                    var settingParameters = new SqlFieldSettingPropertyValueParameters(type, pi, e, value);
                    attr.SettingPropertyValue(settingParameters);

                    if(!settingParameters.Cancel)
                    {
                        if(settingParameters.HasNewPropertyValue)
                        {
                            value = settingParameters.NewPropertyValue;
                            if(value != null)
                            {
                                propertyType = value.GetType();
                            }
                        }
                    }

                    if(value != null && !propertyType.IsEnum)
                    {
                        value = Convert.ChangeType(value, propertyType);
                    }

                    setPropertyValue(e, pi, value);
                }
                catch(Exception ex)
                {
                    Log.Error(string.Format("Type '{2}': failed to set property '{0}' with value '{1}' ({3})", pi.Name, value, type.Name, value == null ? "null" : value.GetType().Name), ex);
                    throw;
                }
            }
        }

        return e;
    }
}

public class SchemaTableInfo
{
    public string Schema { get; private set; }
    public string Table { get; private set; }

    public string FullName
    {
        get
        {
            if(Schema == null) return Table;
            return Schema + "." + Table;
        }
    }

    public SchemaTableInfo(string schema, string table)
    {
        Schema = schema;
        Table = table;
    }

    public override string ToString()
    {
        return FullName;
    }
}

