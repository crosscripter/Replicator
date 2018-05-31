using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace Replicator
{
    class Replicator
    {
        public enum SyncLevel
        {
            Procedure,
            Database,
            Site,
            Server
        }

        public static void Replicate(string server, string database = "Master_App",
            string[] servers = null, string[] databases = null, string[] procedures = null)
        {

        }

        public static void Sync(SyncLevel syncLevel = SyncLevel.Procedure)
        {

        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Replicate fe_PrintLabel procedure from tst-sql.master_App 
            // to all databases on all servers.
            Replicator.Replicate(
                server: "tst-sql",
                procedures: new string[] { "fe_PrintLabel" });

            // Sync all sites.
            Replicator.Sync(Replicator.SyncLevel.Site);

            // Replicate fe_PrintLabel and fe_ISoDocDataSROSp procedures
            // from 410 on dev to tst and live 410 and 420.
            Replicator.Replicate("dev-ifsql", "410LVT_App",
                new string[] { "tst-sql", "inbasqlclus001" },
                new string[] { "410LVT_App", "420TUC_App" },
                new string[] { "fe_PrintLabel", "fe_IsoDocDataSROSp" });

            // Replicate all procedures from master to all servers and databases.
            Replicator.Replicate("dev-ifsql");


            //Console.Title = "Replicator";

            //foreach (var server in SL.Servers)
            //{
            //    foreach (var database in server.Databases)
            //    {
            //        try
            //        {
            //            foreach (var sp in database.StoredProcedures)
            //            {
            //                Console.WriteLine(sp);
            //            }
            //        }
            //        catch { continue; }
            //    }
            //}

            //var dev = new SL.Server("dev-ifsql");
            //var test = new SL.Server("tst-sql");
            //Console.Write("Comparing dev and test servers...");
            //Console.WriteLine(dev == test);

            //var dev410 = new SL.Database("410LVT_App", "dev-ifsql");
            //var test410 = new SL.Database("410LVT_App", "tst-sql");
            //Console.Write("Comparing 410LVT_App in dev and test...");
            //Console.WriteLine(dev410 == test410);

            //var devSp = new SL.StoredProcedure("fe_ISODocDataSROSp", "410LVT_App", "dev-ifsql");
            //var testSp = new SL.StoredProcedure("fe_ISODocDataSROSp", "410LVT_App", "tst-sql");
            //Console.Write("Comparing fe_ISODocDataSROSp in 410LVT dev and test...");
            //Console.WriteLine(devSp == testSp); // .Compare(testSp));

            return;
        }
    }
}
