namespace Replicator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Data.SqlClient;
    using System.Data;

    public static class DB
    {
        const string ConnectionString = @"Server={0};Integrated Security=True";
        static SqlConnection Connection;
        static SqlDataAdapter Adapter;
        static DataTable Table;
        static SqlCommand Command;
        static SqlDataReader Results;

        public static DataTable Data
        {
            get
            {
                Adapter = new SqlDataAdapter(Command.CommandText, Connection);
                Table = new DataTable();
                Results.Close();
                Adapter.Fill(Table);
                Adapter.Dispose();
                return Table;
            }
        }

        public static void Connect(string server, string user = null, string password = null)
        {
            string connectionString = ConnectionString;

            if (!string.IsNullOrEmpty(user))
            {
                var creds = string.Format("User ID={0};Password={1}", user, password);
                connectionString = connectionString.Replace("Integrated Security=True", creds);
            }

            try
            {
                Connection = new SqlConnection(String.Format(connectionString, server));
                Connection.Open();
            }
            catch { throw; }
        }

        public static void Disconnect()
        {
            try
            {
                Connection.Close();
            }
            catch { throw; }
            finally
            {
                // Cleanup resources
                if (Connection != null) Connection.Dispose();
                if (Adapter != null) Adapter.Dispose();
                if (Command != null) Command.Dispose();
                if (Results != null) Results.Dispose();
                if (Command != null) Command.Dispose();
                if (Table != null) Table.Dispose();
            }
        }

        public static void Open(string database)
        {
            try
            {
                Connection.ChangeDatabase(database);
            }
            catch { throw; }
        }

        public static string Read(string name)
        {
            try
            {
                if (Results.HasRows)
                {
                    return Results[name].ToString();
                }
            }
            catch { throw; }
            return string.Empty;
        }

        public static string[] ReadAll(string name)
        {
            var fields = new List<string>();

            do
            {
                fields.Add(Read(name));
            } while (Results.Read());

            Disconnect();
            return fields.ToArray<string>();
        }

        public static void Execute(string sql, bool nonQuery = false)
        {
            try
            {
                Command = new SqlCommand(sql, Connection);

                if (nonQuery)
                {
                    Command.ExecuteNonQuery();
                }
                else
                {
                    Results = Command.ExecuteReader();

                    if (Results.HasRows)
                    {
                        Results.Read();
                    }
                }
            }
            catch { throw; }
        }

        public static void Select(string sql)
        {
            Execute(sql);
        }

        public static void Update(string sql)
        {
            Execute(sql, true);
        }
    }
}
