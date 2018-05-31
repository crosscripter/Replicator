// -----------------------------------------------------------------------
// <copyright file="SL.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Replicator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class SL
    {
        public class StoredProcedure
        {
            public string Name { get; private set; }
            public string Database { get; private set; }
            public string Server { get; private set; }
            public string SQL { get { return SL.Source(Server, Database, Name); } }

            public StoredProcedure(string name, string database, string server)
            {
                Name = name;
                Database = database;
                Server = server;
            }

            #region EqualityTesting
            public static bool operator ==(StoredProcedure lhs, StoredProcedure rhs) { return Equals(lhs, rhs); }
            public static bool operator !=(StoredProcedure lhs, StoredProcedure rhs) { return !Equals(lhs, rhs); }
            public override bool Equals(object obj) { return Equals(this, (StoredProcedure)obj); }
            public override int GetHashCode() { return ToString().GetHashCode(); }

            public static bool Equals(StoredProcedure lhsp, StoredProcedure rhsp)
            {
                var lhlines = lhsp.SQL.Split('\n');
                var rhlines = rhsp.SQL.Split('\n');
                return lhlines.Except(rhlines).Count() == 0;
            }

            #endregion

            public override string ToString() { return string.Format("{0}.[dbo].[{1}]", Database, Name); }
        }

        public class Database
        {
            public string Name { get; private set; }
            public string Server { get; private set; }
            public bool ShowOnlyCustom { get; set; }

            public List<StoredProcedure> StoredProcedures
            {
                get
                {
                    var spNames = SL.StoredProcedures(Server, Name, ShowOnlyCustom);
                    var sps = new List<StoredProcedure>();
                    foreach (var sp in spNames) sps.Add(new StoredProcedure(sp, Name, Server));
                    return sps;
                }
            }

            public Database(string name, string server, bool showOnlyCustom = true)
            {
                Name = name;
                Server = server;
                ShowOnlyCustom = showOnlyCustom;
            }

            public override string ToString() { return string.Format("[{0}]", Name); }

            #region EqualityTesting
            public static bool operator ==(Database lhs, Database rhs) { return Equals(lhs, rhs); }
            public static bool operator !=(Database lhs, Database rhs) { return !Equals(lhs, rhs); }
            public override bool Equals(object obj) { return Equals(this, (Database)obj); }
            public override int GetHashCode() { return ToString().GetHashCode(); }

            public static bool Equals(Database lhdb, Database rhdb)
            {
                return Enumerable.SequenceEqual(lhdb.StoredProcedures.OrderBy(sp => sp.Name),
                                                rhdb.StoredProcedures.OrderBy(sp => sp.Name));
            }
            #endregion
        }

        public class Server
        {
            public string Name { get; private set; }

            public List<Database> Databases
            {
                get
                {
                    var databases = new List<Database>();
                    foreach (var database in SL.AppDatabases(Name)) databases.Add(new Database(database, Name));
                    return databases;
                }
            }

            public Server(string name)
            {
                Name = name;
            }

            public override string ToString() { return string.Format("[{0}]", Name); }

            #region EqualityTesting
            public static bool operator ==(Server lhs, Server rhs) { return Equals(lhs, rhs); }
            public static bool operator !=(Server lhs, Server rhs) { return !Equals(lhs, rhs); }
            public override bool Equals(object obj) { return Equals(this, (Server)obj); }
            public override int GetHashCode() { return ToString().GetHashCode(); }

            public static bool Equals(Server lhsvr, Server rhsvr)
            {
                return Enumerable.SequenceEqual(lhsvr.Databases.OrderBy(db => db.Name),
                                                rhsvr.Databases.OrderBy(db => db.Name));
            }
            #endregion
        }

        public static List<Server> Servers
        {
            get
            {
                string[] serverNames = { "dev-ifsql", "tst-sql", "inbasqlclus001" };
                var servers = new List<Server>();
                foreach (var server in serverNames) servers.Add(new Server(server));
                return servers;
            }
        }

        static IEnumerable<string> FlandersOnly(IEnumerable<string> names)
        {
            return from name in names where name.Contains("fe_") select name;
        }

        public static string[] AppDatabases(string server)
        {
            DB.Connect(server);
            DB.Open("master");
            DB.Select("SELECT name FROM sysdatabases WHERE name LIKE '%_App'");
            return DB.ReadAll("name");
        }

        public static string[] StoredProcedures(string server, string database, bool showOnlyCustom)
        {
            DB.Connect(server);
            DB.Open(database);
            DB.Select(@"SELECT ROUTINE_NAME
                     FROM information_schema.routines
                     WHERE routine_type = 'PROCEDURE' AND ROUTINE_NAME LIKE '%fe_%'
                     ORDER BY ROUTINE_NAME ASC");

            var sps = DB.ReadAll("ROUTINE_NAME");
            return (showOnlyCustom ? FlandersOnly(sps) : sps).ToArray();
        }

        public static string Source(string server, string database, string objectName)
        {
            DB.Connect(server);
            DB.Open(database);
            DB.Select("EXEC SP_HELPTEXT [" + objectName + "]");
            return String.Join(string.Empty, DB.ReadAll("text"));
        }
    }
}
