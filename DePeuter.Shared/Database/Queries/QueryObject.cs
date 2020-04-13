using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DePeuter.Shared.Database.Queries
{
    public class QueryObject : IQueryObject
    {
        public object Query { get; set; }
        public Dictionary<string, object> Parameters { get; private set; }
        public StringBuilder Builder { get; private set; }

        public QueryObject()
        {
            Builder = new StringBuilder();
            Query = Builder;
            Parameters = new Dictionary<string, object>();
        }

        public virtual void BuildQuery(IDatabaseProvider provider)
        {
        }

        public IQueryObject Append(IQueryObject queryObject)
        {
            if(queryObject != null)
            {
                Append(queryObject.Builder);
                AddParameters(queryObject.Parameters);
            }
            return this;
        }

        public IQueryObject AddParameters(params object[] namesAndValues)
        {
            if(namesAndValues != null)
            {
                if (namesAndValues.Length == 1)
                {
                    var parameters = namesAndValues[0] as Dictionary<string, object>;
                    if (parameters != null)
                    {
                        foreach (var x in parameters)
                        {
                            Parameters.Add(x.Key, x.Value);
                        }
                    }
                }
                else
                {
                    for(var i = 0; i < namesAndValues.Length; i += 2)
                    {
                        AddParameter(namesAndValues[i].ToString(), namesAndValues[i + 1]);
                    }    
                }
            }
            return this;
        }

        public IQueryObject AddParameter(string key, object value)
        {
            Parameters.Add(key, value);
            return this;
        }

        public IQueryObject ClearQuery()
        {
            Builder.Clear();
            return this;
        }
        public IQueryObject ClearParameters()
        {
            Parameters.Clear();
            return this;
        }

        public IQueryObject Append(StringBuilder value)
        {
            return Append(value.ToString());
        }
        public IQueryObject Append(string value)
        {
            Builder.Append(value);
            return this;
        }

        public IQueryObject AppendLine()
        {
            Builder.AppendLine();
            return this;
        }
        public IQueryObject AppendLine(StringBuilder value)
        {
            return AppendLine(value.ToString());
        }
        public IQueryObject AppendLine(string value)
        {
            Builder.AppendLine(value);
            return this;
        }

        public override string ToString()
        {
            if (Query != null)
            {
                return Query.ToString();
            }
            return Builder.ToString();
        }
    }
}