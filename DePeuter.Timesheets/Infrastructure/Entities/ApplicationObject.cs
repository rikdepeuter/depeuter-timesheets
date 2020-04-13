using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace DePeuter.Timesheets.Infrastructure.Entities
{
    public class ApplicationObject
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public DateTime LastModifiedAt { get; set; }
        public string BackupVersions { get { return (Backups ?? new List<ApplicationBackupObject>()).Select(x => x.Display).Join(", "); } }
        public string FullPath { get; set; }

        public List<ApplicationBackupObject> Backups;
    }

    public class ApplicationBackupObject
    {
        public string Version { get; set; }
        public string FullPath { get; set; }
        public DateTime Date { get; private set; }
        public string Display { get; private set; }

        public ApplicationBackupObject(string version, string fullPath)
        {
            Version = version;
            FullPath = fullPath;
            Date = DateTime.ParseExact(version, "yyyyMMdd", CultureInfo.InvariantCulture);
            Display = Date.ToString(Date.Year != DateTime.Now.Year ? "d MMM yyyy" : "d MMM");
        }
    }
}