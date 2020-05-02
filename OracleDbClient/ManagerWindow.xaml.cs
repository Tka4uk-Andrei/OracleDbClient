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

        private class WhItem
        {
            public string Name { get; set; }
            public int Amount { get; set; }
        }

        private string oldName;
        private List<Good> goods;
        private List<WhItem> wh1Items;
        private List<WhItem> wh2Items;
        private Dictionary<int, string> goodsDictionary;

        public ManagerWindow()
        {
            InitializeComponent();

            UpdateGoodsTableView();
            UpdateWh1View();
            UpdateWh2View();
        }

        #region GoodsView

        private void OnGoodsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Good good = goodsList.SelectedItem as Good;
            if (good != null)
            {
                changeValBtn.Visibility = Visibility.Visible;
                oldName = good.Name;
                goodNameTextBox.Text = good.Name;
                priorityTextBox.Text = good.Priority.ToString();
            }
            else
            {
                changeValBtn.Visibility = Visibility.Hidden;
                addValBtn.Visibility = Visibility.Hidden;
                goodNameTextBox.Text = "";
                priorityTextBox.Text = "";
            }
        }

        private void GoodsTextBoxs_TextChanged(object sender, TextChangedEventArgs e)
        {
            Good good = goodsList.SelectedItem as Good;
            if (goodNameTextBox.Text == "" || priorityTextBox.Text == "")
            {
                changeValBtn.Visibility = Visibility.Hidden;
                addValBtn.Visibility = Visibility.Hidden;
                goodsList.SelectedItem = null;
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

            UpdateGoodsTableView();
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

            UpdateGoodsTableView();
        }

        private void UpdateGoodsTableView()
        {
            var command = new OracleCommand("SELECT ID, NAME, PRIORITY FROM GOODS ORDER BY NAME, PRIORITY DESC");
            command.Connection = OracleDbManager.GetConnection();
            var oraReader = command.ExecuteReader();

            goods = new List<Good>();
            goodsDictionary = new Dictionary<int, string>();
            if (oraReader.HasRows)
            {
                while (oraReader.Read())
                {
                    Good good = new Good();
                    good.Name = oraReader.GetString(1);
                    good.Priority = oraReader.GetInt32(2);
                    goodsDictionary.Add(oraReader.GetInt32(0), good.Name);
                    goods.Add(good);
                }

            }

            goodsList.ItemsSource = goods;
        }

        #endregion

        #region Wharehouse1

        private void UpdateWh1View()
        {
            var command = new OracleCommand("SELECT good_id, good_count FROM WAREHOUSE1");
            command.Connection = OracleDbManager.GetConnection();
            var oraReader = command.ExecuteReader();

            wh1Items = new List<WhItem>();
            if (oraReader.HasRows)
            {
                while (oraReader.Read())
                {
                    WhItem whItem = new WhItem();
                    whItem.Name = goodsDictionary[oraReader.GetInt32(0)];
                    whItem.Amount = oraReader.GetInt32(1);
                    wh1Items.Add(whItem);
                }

            }

            List<string> goodsNames = new List<string>();
            foreach (var good in goods)
            {
                goodsNames.Add(good.Name);
            }

            wh1DataList.ItemsSource = wh1Items;
            wh1GoodCmbBox.ItemsSource = goodsNames;
        }

        private void OnWh1SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            WhItem whItem = wh1DataList.SelectedItem as WhItem;
            if (whItem != null)
            {
                updatePositionWh1Btn.Visibility = Visibility.Visible;
                addToWh1Btn.Visibility = Visibility.Hidden;
                deleteFromWh1Btn.Visibility = Visibility.Visible;

                wh1GoodCmbBox.Text = whItem.Name;
                wh1GoodCountTxtBox.Text = whItem.Amount.ToString();
            }
            else
            {
                updatePositionWh1Btn.Visibility = Visibility.Hidden;
                addToWh1Btn.Visibility = Visibility.Hidden;
                deleteFromWh1Btn.Visibility = Visibility.Hidden;

                wh1GoodCmbBox.Text = "";
                wh1GoodCountTxtBox.Text = "";
            }
        }

        private void Wh1GoodDescriptionChanged(object sender, SelectionChangedEventArgs e)
        {
            Wh1GoodDescriptionChanged(e.AddedItems[0] as string);
        }

        private void Wh1GoodDescriptionChanged(object sender, TextChangedEventArgs e)
        {
            Wh1GoodDescriptionChanged(wh1GoodCmbBox.Text);
        }

        private void Wh1GoodDescriptionChanged(string goodName)
        {
            WhItem whItem = wh1DataList.SelectedItem as WhItem;
            int count;
            foreach (var good in wh1Items)
            {
                if (good.Name.Equals(goodName))
                {
                    addToWh1Btn.Visibility = Visibility.Hidden;
                    if (!int.TryParse(wh1GoodCountTxtBox.Text, out count))
                    {
                        updatePositionWh1Btn.Visibility = Visibility.Hidden;
                        deleteFromWh1Btn.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        deleteFromWh1Btn.Visibility = Visibility.Visible;
                        updatePositionWh1Btn.Visibility = Visibility.Visible;
                    }
                    return;
                }
            }

            updatePositionWh1Btn.Visibility = Visibility.Hidden;
            deleteFromWh1Btn.Visibility = Visibility.Hidden;
            if (goodName == "" || wh1GoodCountTxtBox.Text == "" || !int.TryParse(wh1GoodCountTxtBox.Text, out count))
            {
                addToWh1Btn.Visibility = Visibility.Hidden;
            }
            else
            {
                addToWh1Btn.Visibility = Visibility.Visible;
            }
        }

        private void UpdatePositionWh1Btn_Click(object sender, RoutedEventArgs e)
        {
            OracleCommand command = new OracleCommand(
                String.Format(
                    "UPDATE WAREHOUSE1 SET good_count = {0} where good_id = (SELECT MAX(id) FROM GOODS WHERE NAME = '{1}')",
                    wh1GoodCountTxtBox.Text, wh1GoodCmbBox.Text
                ));
            command.Connection = OracleDbManager.GetConnection();
            var oraReader = command.ExecuteReader();

            UpdateWh1View();
        }

        private void AddToWh1Btn_Click(object sender, RoutedEventArgs e)
        {
            OracleCommand command = new OracleCommand(
                String.Format(
                    "INSERT INTO WAREHOUSE1(good_id, good_count) VALUES((SELECT MAX(id) FROM GOODS WHERE NAME = '{0}'), {1})",
                    wh1GoodCmbBox.Text, wh1GoodCountTxtBox.Text
                ));
            command.Connection = OracleDbManager.GetConnection();
            var oraReader = command.ExecuteReader();

            UpdateWh1View();
        }

        private void DeleteFromWh1Btn_Click(object sender, RoutedEventArgs e)
        {
            //WhItem whItem = wh1DataList.SelectedItem as WhItem;
            //if (wh1GoodCmbBox.Text == whItem.Name && wh1GoodCountTxtBox.Text == whItem.Amount.ToString())
            //{
                OracleCommand command = new OracleCommand(
                    String.Format(
                        "delete from WAREHOUSE1 where GOOD_ID = (select max(ID) FROM GOODS where NAME = '{0}')",
                        wh1GoodCmbBox.Text
                    ));
                command.Connection = OracleDbManager.GetConnection();
                var oraReader = command.ExecuteReader();

                UpdateWh1View();
            //}
        }

        #endregion

        #region Wharehouse2

        private void UpdateWh2View()
        {
            var command = new OracleCommand("SELECT good_id, good_count FROM WAREHOUSE2");
            command.Connection = OracleDbManager.GetConnection();
            var oraReader = command.ExecuteReader();

            wh2Items = new List<WhItem>();
            if (oraReader.HasRows)
            {
                while (oraReader.Read())
                {
                    WhItem whItem = new WhItem();
                    whItem.Name = goodsDictionary[oraReader.GetInt32(0)];
                    whItem.Amount = oraReader.GetInt32(1);
                    wh2Items.Add(whItem);
                }

            }

            wh2DataList.ItemsSource = wh2Items;
        }


        #endregion

        #region AnalyticsView



        #endregion


    }
}
