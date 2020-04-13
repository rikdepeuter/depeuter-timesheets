using System;
using System.Linq;
using System.Reflection;
using DePeuter.Timesheets.Database.Entities;

namespace DePeuter.Timesheets.Database.Services
{
    public class UpdaterDb : DbBase
    {
        private class UpdateAttribute : Attribute
        {
            public int Version { get; private set; }

            public UpdateAttribute(int version)
            {
                Version = version;
            }
        }

        public void Update()
        {
            var config = GetConfiguration("DB_VERSION");
            
            var currentVersion = int.Parse(config.Value ?? "0");

            var updates = GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Select(x => new
            {
                Method = x,
                Update = x.GetCustomAttribute<UpdateAttribute>()
            }).Where(x => x.Update != null && x.Update.Version > currentVersion).OrderBy(x => x.Update.Version).ToArray();

            foreach (var update in updates)
            {
                update.Method.Invoke(this, null);

                config.Value = update.Update.Version.ToString();
                Save(config);
            }
        }

        [Update(1)]
        private void _init()
        {
            var query = NewQueryObject();
            query.AppendLine("CREATE SCHEMA timesheets");

            ExecuteNonQuery(query);

            query = NewQueryObject();
            query.AppendLine("CREATE TABLE timesheets.configuration (intId int IDENTITY(1,1) NOT NULL PRIMARY KEY, dteCreatedAt datetime NOT NULL DEFAULT getdate(), vchKey character varying(max) NOT NULL, vchValue character varying(max));");
            query.AppendLine("CREATE TABLE timesheets.jobs (intId int IDENTITY(1,1) NOT NULL PRIMARY KEY, dteCreatedAt datetime NOT NULL DEFAULT getdate(), vchClient character varying(max), vchProject character varying(max), vchCode character varying(max) NOT NULL);");
            query.AppendLine("CREATE TABLE timesheets.timesheets (intId int IDENTITY(1,1) NOT NULL PRIMARY KEY, dteCreatedAt datetime NOT NULL DEFAULT getdate(), vchUsername character varying(max), intJobId integer, dteStartTime datetime, dteEndTime datetime, vchDescription character varying(max), bDeleted bit NOT NULL DEFAULT 0, intType integer NOT NULL DEFAULT 0);");
            query.AppendLine("CREATE TABLE timesheets.session_switches (intid int IDENTITY(1,1) NOT NULL PRIMARY KEY, dtecreatedat datetime NOT NULL DEFAULT getdate(), vchusername character varying(max), vchreason character varying(max));");

            ExecuteNonQuery(query);
        }

        [Update(2)]
        private void _tasknumber()
        {
            ExecuteNonQuery(NewQueryObject("ALTER TABLE timesheets.timesheets ADD vchTaskNumber character varying(max)"));
        }
    }
}