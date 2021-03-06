﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Oracle.ManagedDataAccess.Client;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;


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
        private List<string> jornalLogs;

        private const string JORNAL_FILE_NAME = "jornal.txt";

        public ManagerWindow()
        {
            jornalLogs = new List<string>();
            InitializeComponent();
            UpdateAll();
        }

        void UpdateAll()
        {
            UpdateGoodsTableView();
            UpdateWh1View();
            UpdateWh2View();
            UpdateSalesView();
            UpdateAnalyticsView();
        }

        #region GoodsView

        private void OnGoodsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Good good = goodsList.SelectedItem as Good;

            changeValBtn.Visibility = Visibility.Visible;
            addValBtn.Visibility = Visibility.Hidden;

            oldName = good.Name;
            goodNameTextBox.Text = good.Name;
            priorityTextBox.Text = good.Priority.ToString();
        }

        private void GoodsTextBoxs_TextChanged(object sender, TextChangedEventArgs e)
        {
            Good good = goodsList.SelectedItem as Good;
            if (goodNameTextBox.Text == "" || priorityTextBox.Text == "")
            {
                changeValBtn.Visibility = Visibility.Hidden;
                addValBtn.Visibility = Visibility.Hidden;
            }
            else
            {
                var flag = false;
                foreach (var goodName in goodNames)
                    if (goodName == goodNameTextBox.Text)
                        flag = true;
                if (flag)
                {
                    changeValBtn.Visibility = Visibility.Visible;
                    addValBtn.Visibility = Visibility.Hidden;
                }
                else
                {
                    changeValBtn.Visibility = Visibility.Hidden;
                    addValBtn.Visibility = Visibility.Visible;
                }
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

            UpdateAll();
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

            UpdateAll();
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

            UpdateAll();
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

            UpdateAll();
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

            UpdateAll();
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

            UpdateAll();
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

            UpdateAll();
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
            UpdateAll();
        }

        #endregion

        #region AnalyticsView

        private void UpdateAnalyticsView()
        {
            List<string> promList = new List<string>();
            foreach (var item in itemsOnSale)
            {
                var f = true;
                foreach (var val in promList)
                    if (val == item.Name)
                        f = false;
                if (f)
                    promList.Add(item.Name);
            }
            AnalyticsComboBox.ItemsSource = promList;
        }


        private int getSoldCountForPeriod(DateTime start, DateTime end, string goodName)
        {
            start = start.AddHours(1);
            end = end.AddHours(1);
            OracleConnection connection = OracleDbManager.GetConnection();
            OracleCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "demand_count_by_name";
            command.Parameters.Add("start_time", OracleDbType.Varchar2).Value = start.ToString();
            command.Parameters.Add("end_time", OracleDbType.Varchar2).Value = end.ToString();
            command.Parameters.Add("good_name", OracleDbType.Varchar2).Value = goodName;
            command.Parameters.Add("count_", OracleDbType.Int32).Direction = ParameterDirection.Output;
            command.ExecuteNonQuery();

            int result;
            if (!int.TryParse(command.Parameters["count_"].Value.ToString(), out result))
                result = 0;

            OracleDbManager.CloseConnection();

            return result;
        }

        private void CalculateBtn_Click(object sender, RoutedEventArgs e)
        {
            // actions lock
            calculateBtn.IsEnabled = false;
            writeToJornalBtn.IsEnabled = false;

            var startDate = analyticsCalendar.SelectedDates[0];
            var endDate = analyticsCalendar.SelectedDates[analyticsCalendar.SelectedDates.Count - 1];
            var dateList = analyticsCalendar.SelectedDates;

            List<DateTime> dates = new List<DateTime>();
            List<int> values = new List<int>();

            // Get demand of good for each day
            if (startDate > endDate)
                dateList.Reverse();
            for (int i = 0; i < dateList.Count - 1; i++)
            {
                dates.Add(dateList[i]);
                values.Add(getSoldCountForPeriod(dateList[i], dateList[i+1], AnalyticsComboBox.Text));
            }

            // Calculate prognosis
            List<int> promValues = new List<int>();
            foreach (var val in values)
                promValues.Add(val);
            while (promValues.Count > 2)
            {
                for (int i = 0; i < promValues.Count - 1; i++)
                    promValues[i] = (promValues[i] + promValues[i + 1]) / 2;

                promValues.RemoveAt(promValues.Count - 1);
            }

            // Write calculations
            var delta = promValues[1] - promValues[0];
            for (int i = 1; i <= 7; i++)
            {
                dates.Add(dates[dates.Count - 1].AddDays(1));
                if (values[values.Count - 1] + delta < 0)
                    values.Add(0);
                else
                    values.Add(values[values.Count - 1] + delta);
            }

            // Make plot
            PlotModel PlotModel = new PlotModel { Title = "Прогноз для " + AnalyticsComboBox.Text };
            var minValue = DateTimeAxis.ToDouble(dates[0].AddHours(-2));
            var maxValue = DateTimeAxis.ToDouble(dates[dates.Count - 1].AddHours(1));
            var sellSeries = new LineSeries { Title = "Оригинальный спрос", MarkerType = MarkerType.Circle, Color = OxyColor.FromRgb(57, 240, 172) };
            var prognoseSeries = new LineSeries { Title = "Прогнозируемый спрос", MarkerType = MarkerType.Square, Color = OxyColor.FromRgb(240,172,57)};
            // write demand and prognose series
            for (int i = 0; i < dateList.Count - 1; i++)
            {
                jornalLogs.Add(string.Format("+++  Demand  +++ on {0} for good <<{1}>> is {2}", dates[i].ToString(), AnalyticsComboBox.Text, values[i]));
                sellSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(dates[i]), values[i]));
            }
            for (int i = dateList.Count - 2; i < dates.Count; i++)
            {
                jornalLogs.Add(string.Format("??? Prognose ??? on {0} for good <<{1}>> is {2}", dates[i].ToString(), AnalyticsComboBox.Text, values[i]));
                prognoseSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(dates[i]), values[i]));
            }

            // establish data axis
            PlotModel.Axes.Add(new DateTimeAxis
                { Position = AxisPosition.Bottom, Minimum = minValue, Maximum = maxValue, StringFormat = "MM/dd/yyyy"});
            // draw plot
            PlotModel.Series.Add(sellSeries);
            PlotModel.Series.Add(prognoseSeries);
            PlotView.Model = PlotModel;

            // actions enable
            calculateBtn.IsEnabled = true;
            writeToJornalBtn.IsEnabled = true;
        }

        private void AnalyticsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            analytictsBtnDisableEnable(e.AddedItems[0] as string);
        }

        private void AnalyticsCalendar_OnSelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            analytictsBtnDisableEnable(AnalyticsComboBox.Text);
        }

        private void analytictsBtnDisableEnable(string goodName)
        {
            if (analyticsCalendar.SelectedDates.Count <= 1)
            {
                calculateBtn.IsEnabled = false;
                return;
            }

            if (goodName == "")
            {
                calculateBtn.IsEnabled = false;
                return;
            }

            calculateBtn.IsEnabled = true;
        }

        private void WriteToJornalBtn_OnClick(object sender, RoutedEventArgs e)
        {
            // actions lock
            writeToJornalBtn.IsEnabled = false;
            calculateBtn.IsEnabled = false;

            // append write to file
            using (var jornalFile = new FileStream(JORNAL_FILE_NAME, FileMode.Append))
            {
                foreach (var str in jornalLogs)
                {
                    byte[] array = System.Text.Encoding.Default.GetBytes(str + '\n');
                    jornalFile.Write(array, 0, array.Length);
                }
            }
            // Clear logs, that was wrote to file 
            jornalLogs.Clear();

            // actions enable
            writeToJornalBtn.IsEnabled = true;
            calculateBtn.IsEnabled = true;
        }

        #endregion
    }
}
