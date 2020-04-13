using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DePeuter.Shared.Advanced;
using DePeuter.Shared.Exceptions;

public static class GenericExtensions
{
    public static T WithAction<T>(this T item, Action<T> action) where T : class
    {
        action(item);
        return item;
    }

    //public static T Clone<T>(this T item) where T : class, new()
    //{
    //    return EF.Clone(item);
    //}

    public static void SetPropertyValue<T>(this T entity, string property, object value, object[] index = null) where T : class
    {
        var type = entity.GetType();
        var propertyInfo = type.GetProperty(property);
        if(propertyInfo == null)
        {
            throw new UnknownPropertyException(type, property);
        }

        try
        {
            propertyInfo.SetValue(entity, value, index);
        }
        catch(Exception ex)
        {
            throw new PropertySetFailedException(type, property, value, index, ex);
        }
    }

    public static object GetPropertyValue<T>(this T entity, string property, object[] index = null) where T : class
    {
        var type = entity.GetType();
        var propertyInfo = type.GetProperty(property);
        if(propertyInfo == null)
        {
            throw new UnknownPropertyException(type, property);
        }

        try
        {
            return propertyInfo.GetValue(entity, index);
        }
        catch(Exception ex)
        {
            throw new PropertyGetFailedException(type, property, index, ex);
        }
    }

    public static TResult GetPropertyValue<T, TResult>(this T entity, string property, object[] index = null) where T : class
    {
        var type = entity.GetType();
        var propertyInfo = type.GetProperty(property);
        if(propertyInfo == null)
        {
            throw new UnknownPropertyException(type, property);
        }

        try
        {
            return propertyInfo.GetValue(entity, index).CastTo<TResult>();
        }
        catch(Exception ex)
        {
            throw new PropertyGetFailedException(type, property, index, ex);
        }
    }

    public static TResult Using<T, TResult>(this T entity, Func<T, TResult> action) where T : class, IDisposable
    {
        using(entity)
        {
            return action(entity);
        }
    }
    public static void UsingAsync<T>(this T entity, Action<T> action) where T : class, IDisposable
    {
        Async.Run((s, e) =>
        {
            using(entity)
            {
                action(entity);
            }
        }, null,
            (s, e) =>
            {
                if(e.Error != null)
                {
                    throw new AsyncException(e.Error);
                }
            });
    }
    public static void UsingAsync<T, TResult>(this T entity, Func<T, TResult> action, Action<TResult> callback) where T : class, IDisposable
    {
        Async.Run((s, e) =>
            {
                using(entity)
                {
                    e.Result = action(entity);
                }
            }, null,
            (s, e) =>
            {
                if (e.Error != null)
                {
                    throw new AsyncException(e.Error);
                }

                if (callback != null)
                {
                    callback((TResult)e.Result);
                }
            });
    }

    public static void Using<T>(this T entity, Action<T> action) where T : class, IDisposable
    {
        using(entity)
        {
            action(entity);
        }
    }
    public static void UsingAsync<T>(this T entity, Action<T> action, Action callback) where T : class, IDisposable
    {
        Async.Run((s, e) =>
        {
            using(entity)
            {
                action(entity);
            }
        }, null,
            (s, e) =>
            {
                if(e.Error != null)
                {
                    throw new AsyncException(e.Error);
                }

                if(callback != null)
                {
                    callback();
                }
            });
    }
}