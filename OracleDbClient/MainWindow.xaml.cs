using System;
using System.Collections.Generic;
using System.Data.Common;
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
using System.Security.Cryptography;

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
            

        }

        public static string getHashSha256(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            SHA256Managed hashstring = new SHA256Managed();
            byte[] hash = hashstring.ComputeHash(bytes);
            string hashString = string.Empty;
            foreach (byte x in hash)
            {
                hashString += String.Format("{0:x2}", x);
            }
            return hashString;
        }

        private void Login_Button_Click(object sender, RoutedEventArgs e)
        {
            string hashedPassword = getHashSha256(password.Password);
            OracleCommand command = new OracleCommand(
                string.Format(
                    "SELECT * FROM USERS_LIST WHERE USER_NAME = '{0}' and USER_PASSWORD = '{1}'",
                    userName.Text, hashedPassword
                )
            );
            command.Connection = OracleDbManager.GetConnection();
            var oraReader = command.ExecuteReader();
            if (oraReader.HasRows)
            {
                MessageBox.Show("Вы вошли!!!");
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль");
            }
        }
    }
}
