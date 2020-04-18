using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;

namespace OracleDbClient
{
    static class OracleDbManager
    {
        private static bool init_flag = false;
        private static OracleConnection connection = null;

        private static void InitConnection()
        {
            // create connection
            connection = new OracleConnection();

            // fill connection params
            OracleConnectionStringBuilder ocsb = new OracleConnectionStringBuilder();
            ocsb.Password = "123456";
            ocsb.UserID = "c##test";
            ocsb.DataSource = "SampleDataSource";

            // establishing connection
            connection.ConnectionString = ocsb.ConnectionString;
            connection.Open();
            Console.WriteLine("Connection established (" + connection.ServerVersion + ")");
        }

        public static OracleConnection GetConnection()
        {
            if (!init_flag)
            {
                InitConnection();
                init_flag = true;
            }

            return connection;
        }

        public static void CloseConnection()
        {
            connection.Close();
            connection.Dispose();
            init_flag = false;
        }
    }
}
