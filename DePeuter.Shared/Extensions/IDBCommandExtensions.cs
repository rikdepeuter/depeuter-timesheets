using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Data.Common;
using DePeuter.Shared.Database.Queries;

public static class DbCommandExtensions
{
    public static int ExecuteFile(this IDbCommand cmd, string fileName)
    {
        var strText = System.IO.File.ReadAllText(fileName, Encoding.UTF8);
        cmd.CommandText = strText;
        return cmd.ExecuteNonQuery();
    }

    public static Dictionary<string, object> GetParameters(this IDbCommand cmd)
    {
        var parameters = new Dictionary<string, object>();

        if(cmd.Parameters != null)
        {
            foreach(var p in cmd.Parameters.Cast<IDbDataParameter>())
            {
                parameters.Add(p.ParameterName, p.Value);
            }
        }

        return parameters;
    }

    public static IQueryObject ToQueryObject(this IDbCommand cmd)
    {
        var queryObject = new QueryObject();
        if(cmd == null) return queryObject;

        queryObject.Builder.Append(cmd.CommandText);
        
        if(cmd.Parameters != null && cmd.Parameters.Count > 0)
        {
            foreach(var item in cmd.Parameters.Cast<IDbDataParameter>())
            {
                queryObject.Parameters.Add(item.ParameterName, item.Value);
            }
        }

        return queryObject;
    }
}