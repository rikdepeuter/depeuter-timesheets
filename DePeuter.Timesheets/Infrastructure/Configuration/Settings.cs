using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DePeuter.Timesheets.Infrastructure.Configuration
{
    public class Settings
    {
        private static Settings _my;
        public static Settings My
        {
            get
            {
                if (_my == null)
                {
                    _my = new Settings();
                }
                return _my;
            }
        }

        public string ApplicationsDirectory { get { return GetValue("ApplicationsDirectory"); } }
        public string[] ApplicationTypes { get { return GetValues("ApplicationTypes"); } }
        public string BackupDirectory { get { return GetValue("BackupDirectory"); } }
        public int ApplicationMaxBackups { get { return int.Parse(GetValue("ApplicationMaxBackups") ?? "5"); } }

        private string GetValue(string key)
        {
            return System.Configuration.ConfigurationManager.AppSettings[key];
        }
        private string[] GetValues(string key)
        {
            return (System.Configuration.ConfigurationManager.AppSettings[key] ?? string.Empty).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
