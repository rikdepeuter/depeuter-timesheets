using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace DePeuter.Shared.Extensions
{
    public static class DataReaderExtensions
    {
        public static IEnumerable<string> GetColumnNames(this IDataReader reader)
        {
            for (int i = 0; i < reader.FieldCount; i++)
                yield return reader.GetName(i);
        }
    }
}
