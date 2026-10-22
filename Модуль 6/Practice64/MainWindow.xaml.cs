
using System;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Practice64
{
    public partial class MainWindow : Window
    {
        private string connectionString;
        private int editingId = 0;

        private ObservableCollection<FinanceRecord> records =
            new ObservableCollection<FinanceRecord>();

        public MainWindow()
        {
            InitializeComponent();

            TransactionsGrid.ItemsSource = records;
            dpDate.SelectedDate = DateTime.Today;

            string folder = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "Practice64");

            Directory.CreateDirectory(folder);

            connectionString = "Data Source=" +
                Path.Combine(folder, "finance.db") + ";Version=3;";

            CreateDatabase();
            LoadRecords();
            UpdateSummary();
        }

        // Создание базы данных
        private void CreateDatabase()
        {
            using (SQLiteConnection connection =
                   new SQLiteConnection(connectionString))
            {
                connection.Open();

                string sql =
                    "CREATE TABLE IF NOT EXISTS FinanceRecords (" +
                    "Id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                    "Date TEXT NOT NULL, " +
                    "Type TEXT NOT NULL, " +
                    "Category TEXT NOT NULL, " +
                    "Description TEXT, " +
                    "Amount REAL NOT NULL)";

                using (SQLiteCommand command =
                       new SQLiteCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        // Добавление новой записи или сохранение изменений
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (cmbType.SelectedItem == null ||
                txtCategory.Text.Trim() == "")
            {
                MessageBox.Show("Выбери тип операции и введи категорию.");
                return;
            }

            double amount;

            if (!TryReadAmount(txtAmount.Text, out amount) || amount <= 0)
            {
                MessageBox.Show("Введи корректную сумму больше нуля.");
                return;
            }

            if (!dpDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Выбери дату операции.");
                return;
            }

            string type = ((ComboBoxItem)cmbType.SelectedItem)
                .Content.ToString();

            string date = dpDate.SelectedDate.Value.ToString("yyyy-MM-dd");

            using (SQLiteConnection connection =
                   new SQLiteConnection(connectionString))
            {
                connection.Open();

                string sql;

                if (editingId == 0)
                {
                    sql = "INSERT INTO FinanceRecords " +
                          "(Date, Type, Category, Description, Amount) " +
                          "VALUES (@date, @type, @category, @description, @amount)";
                }
                else
                {
                    sql = "UPDATE FinanceRecords SET " +
                          "Date = @date, Type = @type, Category = @category, " +
                          "Description = @description, Amount = @amount " +
                          "WHERE Id = @id";
                }

                using (SQLiteCommand command =
                       new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@date", date);
                    command.Parameters.AddWithValue("@type", type);
                    command.Parameters.AddWithValue(
                        "@category", txtCategory.Text.Trim());
                    command.Parameters.AddWithValue(
                        "@description", txtDescription.Text.Trim());
                    command.Parameters.AddWithValue("@amount", amount);

                    if (editingId != 0)
                    {
                        command.Parameters.AddWithValue("@id", editingId);
                    }

                    command.ExecuteNonQuery();
                }
            }

            ResetForm();
            LoadRecords();
            UpdateSummary();

            txtStatus.Text = "Запись успешно сохранена.";
        }

        // Загрузка записи в поля для редактирования
        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            FinanceRecord record =
                TransactionsGrid.SelectedItem as FinanceRecord;

            if (record == null)
            {
                MessageBox.Show("Сначала выбери запись в таблице.");
                return;
            }

            editingId = record.Id;

            cmbType.SelectedIndex = record.Type == "Доход" ? 0 : 1;
            txtCategory.Text = record.Category;
            txtDescription.Text = record.Description;
            txtAmount.Text = record.Amount.ToString(
                "0.##", CultureInfo.GetCultureInfo("ru-RU"));
            dpDate.SelectedDate = record.Date;

            btnSave.Content = "Сохранить";
            txtStatus.Text = "Редактирование записи №" + record.Id;
        }

        // Очистка формы
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            ResetForm();
            txtStatus.Text = "Форма очищена.";
        }

        private void ResetForm()
        {
            editingId = 0;

            cmbType.SelectedIndex = 1;
            txtCategory.Clear();
            txtDescription.Clear();
            txtAmount.Clear();
            dpDate.SelectedDate = DateTime.Today;

            btnSave.Content = "Добавить";
        }

        // Удаление записи
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            FinanceRecord record =
                TransactionsGrid.SelectedItem as FinanceRecord;

            if (record == null)
            {
                MessageBox.Show("Выбери запись для удаления.");
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Удалить выбранную запись?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            using (SQLiteConnection connection =
                   new SQLiteConnection(connectionString))
            {
                connection.Open();

                using (SQLiteCommand command =
                       new SQLiteCommand(
                           "DELETE FROM FinanceRecords WHERE Id = @id",
                           connection))
                {
                    command.Parameters.AddWithValue("@id", record.Id);
                    command.ExecuteNonQuery();
                }
            }

            if (editingId == record.Id)
            {
                ResetForm();
            }

            LoadRecords();
            UpdateSummary();

            txtStatus.Text = "Запись удалена.";
        }

        // Загрузка записей с учётом поиска и фильтра
        private void LoadRecords()
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                return;
            }

            records.Clear();

            string filter =
                cmbFilter.SelectedItem == null
                ? "Все операции"
                : ((ComboBoxItem)cmbFilter.SelectedItem).Content.ToString();

            string sql =
                "SELECT * FROM FinanceRecords " +
                "WHERE (Category LIKE @search OR Description LIKE @search) ";

            if (filter == "Доходы")
            {
                sql += "AND Type = 'Доход' ";
            }
            else if (filter == "Расходы")
            {
                sql += "AND Type = 'Расход' ";
            }
            else if (filter == "Только этот месяц")
            {
                sql += "AND substr(Date, 1, 7) = @month ";
            }

            sql += "ORDER BY Date DESC, Id DESC";

            using (SQLiteConnection connection =
                   new SQLiteConnection(connectionString))
            {
                connection.Open();

                using (SQLiteCommand command =
                       new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue(
                        "@search", "%" + txtSearch.Text.Trim() + "%");

                    if (filter == "Только этот месяц")
                    {
                        command.Parameters.AddWithValue(
                            "@month", DateTime.Today.ToString("yyyy-MM"));
                    }

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            FinanceRecord record = new FinanceRecord();

                            record.Id = Convert.ToInt32(reader["Id"]);

                            record.Date = DateTime.ParseExact(
                                reader["Date"].ToString(),
                                "yyyy-MM-dd",
                                CultureInfo.InvariantCulture);

                            record.Type = reader["Type"].ToString();
                            record.Category = reader["Category"].ToString();
                            record.Description = reader["Description"].ToString();
                            record.Amount = Convert.ToDouble(reader["Amount"]);

                            records.Add(record);
                        }
                    }
                }
            }
        }

        private void Search_TextChanged(
            object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(connectionString))
            {
                LoadRecords();
            }
        }

        private void Filter_Changed(
            object sender, SelectionChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(connectionString))
            {
                LoadRecords();
            }
        }

        // Подсчёт доходов, расходов и остатка за текущий месяц
        private void UpdateSummary()
        {
            double income = 0;
            double expense = 0;

            using (SQLiteConnection connection =
                   new SQLiteConnection(connectionString))
            {
                connection.Open();

                string sql =
                    "SELECT " +
                    "COALESCE(SUM(CASE WHEN Type = 'Доход' " +
                    "THEN Amount ELSE 0 END), 0), " +
                    "COALESCE(SUM(CASE WHEN Type = 'Расход' " +
                    "THEN Amount ELSE 0 END), 0) " +
                    "FROM FinanceRecords " +
                    "WHERE substr(Date, 1, 7) = @month";

                using (SQLiteCommand command =
                       new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue(
                        "@month", DateTime.Today.ToString("yyyy-MM"));

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            income = Convert.ToDouble(reader[0]);
                            expense = Convert.ToDouble(reader[1]);
                        }
                    }
                }
            }

            txtIncome.Text = FormatMoney(income);
            txtExpense.Text = FormatMoney(expense);
            txtBalance.Text = FormatMoney(income - expense);

            txtBalance.Foreground =
                income >= expense ? Brushes.SeaGreen : Brushes.IndianRed;
        }

        // Обработка суммы: принимает и запятую, и точку
        private bool TryReadAmount(string text, out double amount)
        {
            string value = text.Trim()
                .Replace(" ", "")
                .Replace("\u00A0", "");

            if (value.Contains(",") && value.Contains("."))
            {
                if (value.LastIndexOf(",") > value.LastIndexOf("."))
                {
                    value = value.Replace(".", "").Replace(",", ".");
                }
                else
                {
                    value = value.Replace(",", "");
                }
            }
            else
            {
                value = value.Replace(",", ".");
            }

            return double.TryParse(
                value,
                NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out amount);
        }

        private string FormatMoney(double amount)
        {
            return amount.ToString(
                "N2", CultureInfo.GetCultureInfo("BYN")) + " BYN";
        }
    }

    // Данные одной операции
    public class FinanceRecord
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public double Amount { get; set; }

        public string AmountText
        {
            get
            {
                return Amount.ToString(
                    "N2", CultureInfo.GetCultureInfo("BYN")) + " BYN";
            }
        }
    }
}
