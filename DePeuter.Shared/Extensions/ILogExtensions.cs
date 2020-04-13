using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace DePeuter.Shared
{
    public static class ILogExtensions
    {
        public static void Sql(this ILog logger, IQueryObject queryObject)
        {
            var sql = logger.GetSqlLog(queryObject);
            if(sql != null)
            {
                logger.Sql(sql);
            }
        }
        public static void Sql(this ILog logger, IDbCommand cmd)
        {
            Sql(logger, cmd.ToQueryObject());
        }

        public static void Sql(this ILog logger, object message)
        {
            logger.Debug(message);
        }

        public static string GetSqlLog(this ILog logger, IQueryObject queryObject)
        {
            if(queryObject == null) return null;
            var query = queryObject.Query.ToString();

            var sql = new StringBuilder();
            sql.AppendLine(query);
            if(queryObject.Parameters != null && queryObject.Parameters.Count > 0)
            {
                sql.AppendLine("Parameters:");
                foreach(var item in queryObject.Parameters)
                {
                    sql.AppendInvariantLine("{0}: {1}", item.Key, item.Value);
                }
            }

            return sql.ToString();
        }
        public static string GetSqlLog(this ILog logger, IDbCommand cmd)
        {
            return GetSqlLog(logger, cmd.ToQueryObject());
        }
    }
}
