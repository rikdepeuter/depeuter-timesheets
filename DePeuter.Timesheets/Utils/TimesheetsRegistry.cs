using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DePeuter.Timesheets.Utils
{
    public static class TimesheetsRegistry
    {
        public static List<string> GetDefaultTaskNumbers()
        {
            return (DePeuterRegistry.Get("Timesheets", "DefaultTaskNumbers") as string ?? string.Empty).Split(';').Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToList();
        }
    }
}
