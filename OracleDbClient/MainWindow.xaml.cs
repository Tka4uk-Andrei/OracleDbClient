using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Oracle.ManagedDataAccess.Client;

namespace OracleDbClient
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // create connection
            OracleConnection con = new OracleConnection();

            // create connection string using builder
            OracleConnectionStringBuilder ocsb = new OracleConnectionStringBuilder();
            ocsb.Password = "123456";
            ocsb.UserID = "c##test";
            //SELECT host_name FROM v$instance;
            //SELECT value FROM v$parameter WHERE name like '%service_name%';
            ocsb.DataSource = "SampleDataSource";

            // connect
            con.ConnectionString = ocsb.ConnectionString;
            con.Open();
            Console.WriteLine("Connection established (" + con.ServerVersion + ")");

            con.Close();
            con.Dispose();
        }
    }
}
