using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace DePeuter.Shared.Cache
{
    public enum CacheLifetime
    {
        Short = 2,
        Medium = 4,
        Long = 8
    }

    public static class Cache
    {
        private static readonly object LockObject = new object();

        public static bool IsEnabled { get; set; }

        private class CacheItem
        {
            public object Value { get; set; }
            public DateTime? TimeOfDeath { get; set; }
        }

        private static readonly Dictionary<string, Dictionary<string, CacheItem>> CachedItems = new Dictionary<string, Dictionary<string, CacheItem>>();

        public static DataTable GetDataTable()
        {
            var res = new DataTable();
            res.Columns.Add("group");
            res.Columns.Add("key");
            res.Columns.Add("value");
            res.Columns.Add("timeofdeath");

            foreach(var cacheGroup in CachedItems.OrderBy(x => x.Key))
            {
                foreach(var cacheItem in cacheGroup.Value)
                {
                    var value = cacheItem.Value.Value;
                    if(cacheItem.Value.Value is DataSet)
                    {
                        var ds = (DataSet)cacheItem.Value.Value;
                        var dsElement = new XElement("DataSet");

                        foreach(var dt in ds.Tables.Cast<DataTable>())
                        {
                            var dtElement = new XElement("DataTable");

                            foreach(var dr in dt.Select())
                            {
                                var drElement = new XElement("DataRow");

                                foreach(var c in dt.Columns.Cast<DataColumn>())
                                {
                                    var cElement = new XElement(c.ColumnName);
                                    var cValue = dr[c.ColumnName];
                                    if(!(cValue is DBNull))
                                    {
                                        cElement.Add(cValue);
                                    }

                                    drElement.Add(cElement);
                                }

                                dtElement.Add(drElement);
                            }

                            dsElement.Add(dtElement);
                        }

                        value = dsElement.ToString();
                    }

                    res.Rows.Add(cacheGroup.Key, cacheItem.Key, value, cacheItem.Value.TimeOfDeath);
                }
            }

            return res;
        }

        public static T Get<T, TGroup>(Func<T> getItem, CacheLifetime? lifetime = CacheLifetime.Long)
        {
            return Get(typeof(TGroup), getItem.Method.ReturnType.FullName, getItem, lifetime);
        }
        public static T Get<T>(Type group, Func<T> getItem, CacheLifetime? lifetime = CacheLifetime.Long)
        {
            return Get(group, getItem.Method.ReturnType.FullName, getItem, lifetime);
        }
        public static T Get<T, TGroup>(string key, Func<T> getItem, CacheLifetime? lifetime = CacheLifetime.Long)
        {
            return Get(typeof(TGroup), key, getItem, lifetime);
        }
        public static T Get<T>(Type group, string key, Func<T> getItem, CacheLifetime? lifetime = CacheLifetime.Long)
        {
            return Get(group.FullName, key, getItem, lifetime);
        }
        public static T Get<T>(string group, string key, Func<T> getItem, CacheLifetime? lifetime = CacheLifetime.Long)
        {
            if(!IsEnabled)
            {
                if(getItem == null)
                {
                    throw new NullReferenceException("No Func defined to retrieve the data");
                }
                return getItem();
            }

            lock(LockObject)
            {
                if(!CachedItems.ContainsKey(group))
                {
                    CachedItems.Add(group, new Dictionary<string, CacheItem>());
                }

                var cacheGroup = CachedItems[group];
                CacheItem cacheItem;

                if(cacheGroup.ContainsKey(key))
                {
                    cacheItem = cacheGroup[key];
                    if(cacheItem.TimeOfDeath != null && lifetime != null && DateTime.Now > cacheItem.TimeOfDeath.Value)
                    {
                        if(getItem == null)
                        {
                            throw new NullReferenceException("No Func defined to retrieve the data");
                        }
                        cacheItem.Value = getItem();
                        cacheItem.TimeOfDeath = DateTime.Now.AddHours((int)lifetime.Value);
                    }
                    return (T)cacheItem.Value;
                }

                if(getItem == null)
                {
                    throw new NullReferenceException("No Func defined to retrieve the data");
                }

                cacheItem = new CacheItem()
                {
                    Value = getItem(),
                    TimeOfDeath = lifetime != null ? DateTime.Now.AddHours((int)lifetime.Value) : (DateTime?)null
                };
                cacheGroup.Add(key, cacheItem);
                return (T)cacheItem.Value;
            }
        }

        public static T Get<T, TGroup>(string key)
        {
            return Get<T>(typeof(TGroup), key);
        }
        public static T Get<T>(Type group, string key)
        {
            return Get<T>(group.FullName, key);
        }
        public static T Get<T>(string group, string key)
        {
            lock(LockObject)
            {
                if(!CachedItems.ContainsKey(group))
                {
                    CachedItems.Add(group, new Dictionary<string, CacheItem>());
                }

                var cacheGroup = CachedItems[group];

                if(cacheGroup.ContainsKey(key))
                {
                    var cacheItem = cacheGroup[key];
                    return (T)cacheItem.Value;
                }

                return default(T);
            }
        }

        public static object Get<TGroup>(string key)
        {
            return Get(typeof(TGroup), key);
        }
        public static object Get(Type group, string key)
        {
            return Get(group.FullName, key);
        }
        public static object Get(string group, string key)
        {
            lock(LockObject)
            {
                if(!CachedItems.ContainsKey(group))
                {
                    CachedItems.Add(group, new Dictionary<string, CacheItem>());
                }

                var cacheGroup = CachedItems[group];

                if(cacheGroup.ContainsKey(key))
                {
                    var cacheItem = cacheGroup[key];
                    return cacheItem.Value;
                }

                return null;
            }
        }

        public static void Set<TGroup>(string key, object value)
        {
            Set(typeof(TGroup), key, value);
        }
        public static void Set(Type group, string key, object value)
        {
            Set(group.FullName, key, value);
        }
        public static void Set(string group, string key, object value)
        {
            lock(LockObject)
            {
                if(!CachedItems.ContainsKey(group))
                {
                    CachedItems.Add(group, new Dictionary<string, CacheItem>());
                }

                var cacheGroup = CachedItems[group];
                CacheItem cacheItem;

                if(cacheGroup.ContainsKey(key))
                {
                    cacheItem = cacheGroup[key];
                    cacheItem.Value = value;
                    return;
                }

                cacheItem = new CacheItem()
                {
                    Value = value
                };
                cacheGroup.Add(key, cacheItem);
            }
        }

        public static void Clear()
        {
            lock(LockObject)
            {
                CachedItems.Clear();
            }
        }

        public static void Clear<TGroup>()
        {
            Clear(typeof(TGroup));
        }

        public static void Clear(Type group)
        {
            Clear(group.FullName);
        }

        public static void Clear(string group)
        {
            lock(LockObject)
            {
                CachedItems.Remove(group);
            }
        }
    }
}