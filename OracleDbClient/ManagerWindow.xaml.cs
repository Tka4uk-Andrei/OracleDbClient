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
using System.Windows.Shapes;
using Oracle.ManagedDataAccess.Client;

namespace OracleDbClient
{
    /// <summary>
    /// Логика взаимодействия для ManagerWindow.xaml
    /// </summary>
    public partial class ManagerWindow : Window
    {
        private class Good
        {
            public string Name { get; set; }
            public int Priority { get; set; }
        }

        private string oldName;

        public ManagerWindow()
        {
            InitializeComponent();

            UpdateView();
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Good good = goodsList.SelectedItem as Good;
            if (good != null)
            {
                changeValBtn.Visibility = Visibility.Visible;
                oldName = good.Name;
                goodNameTextBox.Text = good.Name;
                priorityTextBox.Text = good.Priority.ToString();
            }
        }

        private void GoodNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Good good = goodsList.SelectedItem as Good;
            if (goodNameTextBox.Text == "")
            {
                changeValBtn.Visibility = Visibility.Hidden;
                addValBtn.Visibility = Visibility.Hidden;
            }
            else if (good != null)
            {
                changeValBtn.Visibility = Visibility.Visible;
                addValBtn.Visibility = Visibility.Visible;
            }
            else
            {
                changeValBtn.Visibility = Visibility.Hidden;
                addValBtn.Visibility = Visibility.Visible;
            }
        }

        private void PriorityTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Good good = goodsList.SelectedItem as Good;
            if (priorityTextBox.Text == "")
            {
                changeValBtn.Visibility = Visibility.Hidden;
                addValBtn.Visibility = Visibility.Hidden;
            }
            else if (good != null)
            {
                changeValBtn.Visibility = Visibility.Visible;
                addValBtn.Visibility = Visibility.Visible;
            }
            else
            {
                changeValBtn.Visibility = Visibility.Hidden;
                addValBtn.Visibility = Visibility.Visible;
            }
        }

        private void ChangeValBtn_Click(object sender, RoutedEventArgs e)
        {
            OracleCommand command = new OracleCommand(
                String.Format(
                "UPDATE GOODS SET NAME = '{0}', PRIORITY = {1} where NAME = '{2}'",
                goodNameTextBox.Text, priorityTextBox.Text, oldName
            ));
            command.Connection = OracleDbManager.GetConnection();

            try
            {
                var oraReader = command.ExecuteReader();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                MessageBox.Show("Имена товаров должны быть уникальными");
            }

            UpdateView();
        }

        private void AddValBtn_Click(object sender, RoutedEventArgs e)
        {
            int priority;
            if (Int32.TryParse(priorityTextBox.Text, out priority) == false)
            {
                MessageBox.Show("Приоритет должен быть целым числом");
                return;
            }

            OracleCommand command = new OracleCommand( 
                String.Format(
                    "INSERT INTO GOODS (NAME, PRIORITY) VALUES('{0}', {1})",
                    goodNameTextBox.Text, priority
            ));
            command.Connection = OracleDbManager.GetConnection();
            var oraReader = command.ExecuteReader();

            UpdateView();
        }

        private void UpdateView()
        {
            var command = new OracleCommand("SELECT NAME, PRIORITY FROM GOODS ORDER BY NAME, PRIORITY DESC");
            command.Connection = OracleDbManager.GetConnection();
            var oraReader = command.ExecuteReader();

            List<Good> goods = new List<Good>();
            if (oraReader.HasRows)
            {
                while (oraReader.Read())
                {
                    Good good = new Good();
                    good.Name = oraReader.GetString(0);
                    good.Priority = oraReader.GetInt32(1);
                    goods.Add(good);
                }

            }

            goodsList.ItemsSource = goods;
        }
    }
}
