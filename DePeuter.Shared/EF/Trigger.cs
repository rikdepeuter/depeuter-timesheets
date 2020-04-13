using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Linq.Expressions;
using System.Diagnostics;
using log4net;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class TriggerActionAttribute : Attribute
{
    private readonly TriggerAction _action;
    public TriggerAction Action { get { return _action; } }

    public TriggerActionAttribute(TriggerAction action)
    {
        _action = action;
    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class EntityTypeAttribute : Attribute
{
    private readonly Type _type;
    public Type Type { get { return _type; } }

    public EntityTypeAttribute(Type type)
    {
        _type = type;
    }
}

public enum TriggerAction
{
    BeforeInsert = 1,
    BeforeUpdate = 2,
    BeforeDelete = 4,
    AfterInsert = 8,
    AfterUpdate = 16,
    AfterDelete = 32
}

public class TriggerException : Exception
{
    private readonly object _entity;
    public object Entity { get { return _entity; } }

    public TriggerException(object entity, string message)
        : base(message)
    {
        _entity = entity;
    }
}

public class TriggerParameters
{
    public TriggerAction Action { get; private set; }
    public object Entity { get; private set; }
    public bool Cancel;

    public TriggerParameters(TriggerAction action, object entity)
    {
        Action = action;
        Entity = entity;
    }
}

public interface IEntityTrigger
{
    int Order { get; }
    void Execute(IBaseDatabaseService db, TriggerParameters e);
}

public abstract class BaseEntityTrigger : IEntityTrigger
{
    protected readonly ILog Logger;

    protected BaseEntityTrigger()
    {
        Logger = LogManager.GetLogger(GetType());
    }

    public abstract int Order { get; }
    public abstract void Run(IBaseDatabaseService db, TriggerParameters e);

    public void Execute(IBaseDatabaseService db, TriggerParameters e)
    {
        var type = e.Entity.GetType();
        Logger.DebugFormat("Executing {1}: '{0}'", type.Name, e.Action);

        var sw = Stopwatch.StartNew();

        try
        {
            Run(db, e);
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            throw;
        }
        finally
        {
            Logger.DebugFormat("Executed {1}: '{0}' ({2}ms)", type.Name, e.Action, sw.ElapsedMilliseconds);
        }
    }
}