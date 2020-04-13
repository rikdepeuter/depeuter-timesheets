using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Reflection;

public interface IHash
{
    string GenerateHash();
}
public interface IHashProperties
{
    PropertyInfo[] GetHashProperties();
}

public static class Hash
{
    private static readonly object Lock = new object();
    private static readonly Dictionary<Type, PropertyInfo[]> HashProperties = new Dictionary<Type, PropertyInfo[]>();
    private static readonly Dictionary<string, int> Hashcodes = new Dictionary<string, int>();

    public static string SHA1FromString(string text)
    {
        return SHA1FromString(text, Encoding.UTF8);
    }
    public static string SHA1FromString(string text, Encoding encoding)
    {
        var buffer = encoding.GetBytes(text);
        var cryptoTransformSHA1 = new SHA1CryptoServiceProvider();
        return BitConverter.ToString(cryptoTransformSHA1.ComputeHash(buffer)).ToUpper();
    }
    public static string SHA1FromFile(string filename)
    {
        using(var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using(var bs = new BufferedStream(fs))
        using(var cryptoProvider = new SHA1CryptoServiceProvider())
        {
            return BitConverter.ToString(cryptoProvider.ComputeHash(bs)).ToUpper();
        }
    }

    public static string FromObject<T>(T entity, PropertyInfo[] properties = null)
    {
        return FromObject(entity, entity.GetType(), properties);
    }
    public static string FromObject(object entity, Type type, PropertyInfo[] properties = null)
    {
        if(properties == null)
        {
            lock(Lock)
            {
                if(!HashProperties.ContainsKey(type))
                {
                    if(entity is IHashProperties)
                    {
                        HashProperties.Add(type, ((IHashProperties)entity).GetHashProperties());
                    }
                    else
                    {
                        HashProperties.Add(type, type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => !x.HasCustomAttribute<IgnoreHashAttribute>(true) && (x.HasCustomAttribute<HashAttribute>(true) || typeof(IHash).IsAssignableFrom(x.PropertyType) || x.PropertyType.HasCustomAttribute<HashAttribute>(true))).ToArray());
                    }
                }
            }

            properties = HashProperties[type];
        }

        var sb = new StringBuilder();

        foreach(var pi in properties.Where(x => x.CanRead))
        {
            var value = pi.GetValue(entity, null);

            BuildHash(sb, value, pi.PropertyType);
        }

        if (type.IsPrimitive)
        {
            sb.AppendFormat("{0}", entity);
        }

        return SHA1FromString(sb.ToString());
    }

    public static void BuildHash(StringBuilder sb, object value, Type type)
    {
        if(value == null)
        {
            return;
        }

        if(value is IHash)
        {
            sb.Append(((IHash)value).GenerateHash());
            return;
        }

        if(type.HasCustomAttribute<HashAttribute>(true))
        {
            sb.Append(FromObject(value, type));
            return;
        }

        if(type != typeof(string))
        {
            if(typeof(IEnumerable).IsAssignableFrom(type))
            {
                var collection = (IEnumerable)value;
                foreach(var item in collection)
                {
                    if(item == null)
                    {
                        continue;
                    }

                    sb.Append(FromObject(item, item.GetType()));
                    //BuildHash(sb, item, item.GetType());
                }
                return;
            }
        }

        sb.AppendFormat("{0}", value);
    }

    public static int GetHashCode(string hash)
    {
        lock (Lock)
        {
            if(!Hashcodes.ContainsKey(hash))
            {
                Hashcodes.Add(hash, Hashcodes.Count);
            }
            
            return Hashcodes[hash];
        }
    }
}

[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
public class HashAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
public class IgnoreHashAttribute : Attribute
{
}