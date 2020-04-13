using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DePeuter.Shared.Database.Queries;

public interface IQueryObject
{
    object Query { get; }
    StringBuilder Builder { get; }
    Dictionary<string, object> Parameters { get; }

    void BuildQuery(IDatabaseProvider provider);

    IQueryObject Append(IQueryObject queryObject);

    IQueryObject AddParameters(params object[] namesAndValues);
    IQueryObject AddParameter(string key, object value);

    IQueryObject ClearQuery();
    IQueryObject ClearParameters();
    IQueryObject Append(string value);
    IQueryObject AppendLine();
    IQueryObject AppendLine(string value);
}

public abstract class BaseSelectQuery : QueryObject
{
}

public abstract class BaseInsertUpdateDeleteQuery : QueryObject
{
}