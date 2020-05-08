using System;
using System.Collections.Generic;
using System.Globalization;
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

        private class SaleItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int Count { get; set; }
            public DateTime CreateDate { get; set; }
        }

        private string oldName;
        private List<Good> goods;
        private List<string> goodNames;
        private List<WhItem> wh1Items;
        private List<WhItem> wh2Items;
        private List<SaleItem> itemsOnSale;
        private Dictionary<int, string> goodsDictionary;

        public ManagerWindow()
        {
            InitializeComponent();

            UpdateGoodsTableView();
            UpdateWh1View();
            UpdateWh2View();
            UpdateSalesView();

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
            goodNames = new List<string>();
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
                    goodNames.Add(good.Name);
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
                    deleteFromWh1Btn.Visibility = Visibility.Visible;
                    if (!int.TryParse(wh1GoodCountTxtBox.Text, out count))
                        updatePositionWh1Btn.Visibility = Visibility.Hidden;
                    else
                        updatePositionWh1Btn.Visibility = Visibility.Visible;
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
            OracleCommand command = new OracleCommand(
                String.Format(
                    "delete from WAREHOUSE1 where GOOD_ID = (select max(ID) FROM GOODS where NAME = '{0}')",
                    wh1GoodCmbBox.Text
            ));
            command.Connection = OracleDbManager.GetConnection();
            var oraReader = command.ExecuteReader();

            UpdateWh1View();
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

            List<string> goodsNames = new List<string>();
            foreach (var good in goods)
            {
                goodsNames.Add(good.Name);
            }

            wh2DataList.ItemsSource = wh2Items;
            wh2GoodCmbBox.ItemsSource = goodsNames;
        }

        private void OnWh2SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            WhItem whItem = wh2DataList.SelectedItem as WhItem;
            if (whItem != null)
            {
                updatePositionWh2Btn.Visibility = Visibility.Visible;
                addToWh2Btn.Visibility = Visibility.Hidden;
                deleteFromWh2Btn.Visibility = Visibility.Visible;

                wh2GoodCmbBox.Text = whItem.Name;
                wh2GoodCountTxtBox.Text = whItem.Amount.ToString();
            }
            else
            {
                updatePositionWh2Btn.Visibility = Visibility.Hidden;
                addToWh2Btn.Visibility = Visibility.Hidden;
                deleteFromWh2Btn.Visibility = Visibility.Hidden;

                wh2GoodCmbBox.Text = "";
                wh2GoodCountTxtBox.Text = "";
            }
        }

        private void Wh2GoodDescriptionChanged(object sender, SelectionChangedEventArgs e)
        {
            Wh2GoodDescriptionChanged(e.AddedItems[0] as string);
        }

        private void Wh2GoodDescriptionChanged(object sender, TextChangedEventArgs e)
        {
            Wh2GoodDescriptionChanged(wh2GoodCmbBox.Text);
        }

        private void Wh2GoodDescriptionChanged(string goodName)
        {
            int count;
            foreach (var good in wh2Items)
            {
                if (good.Name.Equals(goodName))
                {
                    addToWh2Btn.Visibility = Visibility.Hidden;
                    deleteFromWh2Btn.Visibility = Visibility.Visible;
                    if (!int.TryParse(wh2GoodCountTxtBox.Text, out count))
                        updatePositionWh2Btn.Visibility = Visibility.Hidden;
                    else
                        updatePositionWh2Btn.Visibility = Visibility.Visible;
                    return;
                }
            }

            updatePositionWh2Btn.Visibility = Visibility.Hidden;
            deleteFromWh2Btn.Visibility = Visibility.Hidden;
            if (goodName == "" || wh2GoodCountTxtBox.Text == "" || !int.TryParse(wh2GoodCountTxtBox.Text, out count))
            {
                addToWh2Btn.Visibility = Visibility.Hidden;
            }
            else
            {
                addToWh2Btn.Visibility = Visibility.Visible;
            }
        }

        private void UpdatePositionWh2Btn_Click(object sender, RoutedEventArgs e)
        {
            OracleCommand command = new OracleCommand(
                String.Format(
                    "UPDATE WAREHOUSE2 SET good_count = {0} where good_id = (SELECT MAX(id) FROM GOODS WHERE NAME = '{1}')",
                    wh2GoodCountTxtBox.Text, wh2GoodCmbBox.Text
                ));
            command.Connection = OracleDbManager.GetConnection();
            var oraReader = command.ExecuteReader();

            UpdateWh2View();
        }

        private void AddToWh2Btn_Click(object sender, RoutedEventArgs e)
        {
            OracleCommand command = new OracleCommand(
                String.Format(
                    "INSERT INTO WAREHOUSE2(good_id, good_count) VALUES((SELECT MAX(id) FROM GOODS WHERE NAME = '{0}'), {1})",
                    wh2GoodCmbBox.Text, wh2GoodCountTxtBox.Text
                ));
            command.Connection = OracleDbManager.GetConnection();
            var oraReader = command.ExecuteReader();

            UpdateWh2View();
        }

        private void DeleteFromWh2Btn_Click(object sender, RoutedEventArgs e)
        {
            OracleCommand command = new OracleCommand(
                String.Format(
                    "delete from WAREHOUSE2 where GOOD_ID = (select max(ID) FROM GOODS where NAME = '{0}')",
                    wh2GoodCmbBox.Text
            ));
            command.Connection = OracleDbManager.GetConnection();
            var oraReader = command.ExecuteReader();

            UpdateWh2View();
        }

        #endregion

        #region SalesView

        private void UpdateSalesView()
        {
            var command = new OracleCommand("SELECT SALES.ID, GOODS.NAME, SALES.GOOD_COUNT, to_char(SALES.CREATE_DATE, 'DD-MM-YYYY HH24:MI:SS')as Sale_Date FROM SALES, GOODS WHERE GOODS.ID = SALES.GOOD_ID ORDER BY Sale_Date DESC, GOODS.NAME");
            command.Connection = OracleDbManager.GetConnection();
            var oraReader = command.ExecuteReader();

            itemsOnSale = new List<SaleItem>();
            if (oraReader.HasRows)
            {
                while (oraReader.Read())
                {
                    var itemOnSale = new SaleItem();
                    itemOnSale.Id = oraReader.GetInt32(0);
                    itemOnSale.Name = oraReader.GetString(1);
                    itemOnSale.Count = oraReader.GetInt32(2);
                    itemOnSale.CreateDate = DateTime.ParseExact(oraReader.GetString(3), "dd-MM-yyyy HH:mm:ss", null);
                    itemsOnSale.Add(itemOnSale);
                }
            }

            salesGoodCmbBox.ItemsSource = goodNames;
            salesList.ItemsSource = itemsOnSale;
        }

        private void SalesGoodCmbBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SalesViewUpdate(e.AddedItems[0] as string);
        }

        private void SaleCountTxtBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
           SalesViewUpdate(salesGoodCmbBox.Text);
        }

        private void SalesViewUpdate(string goodName)
        {
            int count;
            addToSalesBtn.IsEnabled = false;
            if (int.TryParse(saleCountTxtBox.Text, out count))
            {
                int wh1Count = 0;
                int wh2Count = 0;
                addToSalesBtn.Visibility = Visibility.Visible;
                foreach (var wh1Item in wh1Items)
                    if (wh1Item.Name == goodName)
                        wh1Count = wh1Item.Amount;

                foreach (var wh2Item in wh2Items)
                    if (wh2Item.Name == goodName)
                        wh2Count = wh2Item.Amount;

                if (wh1Count + wh2Count >= count)
                    addToSalesBtn.IsEnabled = true;
            }
            else
            {
                addToSalesBtn.IsEnabled = false;
            }
        }

        private void AddToSalesBtn_Click(object sender, RoutedEventArgs e)
        {
            int count = int.Parse(saleCountTxtBox.Text);
            int wh1Count = 0;
            int wh2Count = 0;
            foreach (var wh1Item in wh1Items)
                if (wh1Item.Name == salesGoodCmbBox.Text)
                    wh1Count = wh1Item.Amount;

            foreach (var wh2Item in wh2Items)
                if (wh2Item.Name == salesGoodCmbBox.Text)
                    wh2Count = wh2Item.Amount;

            // add to sales table whiout view update
            OracleCommand command = new OracleCommand(
                string.Format(
                    "INSERT INTO SALES (GOOD_ID, GOOD_COUNT, CREATE_DATE) VALUES((SELECT MAX(ID) FROM GOODS WHERE NAME = '{0}'), {1}, CURRENT_TIMESTAMP)",
                    salesGoodCmbBox.Text, count
                ));
            command.Connection = OracleDbManager.GetConnection();
            var oraReader = command.ExecuteReader();

            // decide, that we should update only one WH or two we should
            if (wh2Count >= count)
            {
                // update note in spb WH with Wh2 view update
                command = new OracleCommand(
                    string.Format(
                        "UPDATE WAREHOUSE2 SET good_count = {0} where good_id = (SELECT MAX(id) FROM GOODS WHERE NAME = '{1}')",
                        wh2Count - count, salesGoodCmbBox.Text
                    ));
                command.Connection = OracleDbManager.GetConnection();
                oraReader = command.ExecuteReader();
                UpdateWh2View();
            }
            else
            {
                // delete note from spb WH with Wh2 view update
                command = new OracleCommand(
                    string.Format(
                        "DELETE FROM WAREHOUSE2 WHERE GOOD_ID = (SELECT MAX(id) FROM GOODS WHERE NAME = '{0}')",
                        salesGoodCmbBox.Text
                    ));
                command.Connection = OracleDbManager.GetConnection();
                oraReader = command.ExecuteReader();
                count = count - wh2Count;
                UpdateWh2View();

                // update note in regional WH with Wh1 view update
                command = new OracleCommand(
                    string.Format(
                        "UPDATE WAREHOUSE1 SET good_count = {0} where good_id = (SELECT MAX(id) FROM GOODS WHERE NAME = '{1}')",
                        wh1Count - count, salesGoodCmbBox.Text
                    ));
                command.Connection = OracleDbManager.GetConnection();
                oraReader = command.ExecuteReader();
                UpdateWh1View();
            }

            // update sales view after all
            UpdateSalesView();
        }

        #endregion

        #region AnalyticsView

        #endregion
    }
}
