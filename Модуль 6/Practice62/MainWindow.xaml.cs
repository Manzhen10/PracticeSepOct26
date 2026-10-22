using System;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Practice62
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<TaskItem> tasks =
            new ObservableCollection<TaskItem>();

        private string dbPath = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "Practice62",
            "tasks.db");

        private string ConnectionString
        {
            get { return "Data Source=" + dbPath + ";Version=3;"; }
        }

        public MainWindow()
        {
            InitializeComponent();

            TasksGrid.ItemsSource = tasks;

            Directory.CreateDirectory(
                Path.GetDirectoryName(dbPath));

            CreateDatabase();
            LoadTasks();
        }

        // Создание базы данных
        private void CreateDatabase()
        {
            using (SQLiteConnection connection =
                   new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                string sql = @"
                    CREATE TABLE IF NOT EXISTS Tasks
                    (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Title TEXT NOT NULL,
                        Description TEXT,
                        DueDate TEXT,
                        Status TEXT NOT NULL
                    );";

                using (SQLiteCommand command =
                       new SQLiteCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        // Загрузка задач из базы
        private void LoadTasks()
        {
            tasks.Clear();

            using (SQLiteConnection connection =
                   new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                string sql = @"
                    SELECT Id, Title, Description, DueDate, Status
                    FROM Tasks
                    ORDER BY Id;";

                using (SQLiteCommand command =
                       new SQLiteCommand(sql, connection))
                {
                    using (SQLiteDataReader reader =
                           command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            TaskItem task = new TaskItem();

                            task.Id = Convert.ToInt32(
                                reader["Id"]);

                            task.Title = Convert.ToString(
                                reader["Title"]);

                            task.Description = Convert.ToString(
                                reader["Description"]);

                            task.DueDate =
                                reader["DueDate"] == DBNull.Value
                                ? ""
                                : Convert.ToString(reader["DueDate"]);

                            task.Status = Convert.ToString(
                                reader["Status"]);

                            tasks.Add(task);
                        }
                    }
                }
            }
        }

        // Получение выбранного статуса
        private string GetStatus()
        {
            ComboBoxItem item =
                cmbStatus.SelectedItem as ComboBoxItem;

            if (item == null)
                return "Новая";

            return item.Content.ToString();
        }

        // Установка статуса в поле
        private void SetStatus(string status)
        {
            foreach (object obj in cmbStatus.Items)
            {
                ComboBoxItem item = obj as ComboBoxItem;

                if (item != null &&
                    item.Content.ToString() == status)
                {
                    cmbStatus.SelectedItem = item;
                    return;
                }
            }

            cmbStatus.SelectedIndex = 0;
        }

        // Проверка данных
        private bool ValidateFields()
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("Введите название задачи.");
                return false;
            }

            return true;
        }

        // Добавление задачи
        private void AddButton_Click(
            object sender, RoutedEventArgs e)
        {
            if (!ValidateFields())
                return;

            object dueDate = DBNull.Value;

            if (dpDueDate.SelectedDate.HasValue)
            {
                dueDate = dpDueDate.SelectedDate.Value
                    .ToString("yyyy-MM-dd");
            }

            using (SQLiteConnection connection =
                   new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                string sql = @"
                    INSERT INTO Tasks
                        (Title, Description, DueDate, Status)
                    VALUES
                        (@Title, @Description, @DueDate, @Status);";

                using (SQLiteCommand command =
                       new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue(
                        "@Title", txtTitle.Text.Trim());

                    command.Parameters.AddWithValue(
                        "@Description",
                        txtDescription.Text.Trim());

                    command.Parameters.AddWithValue(
                        "@DueDate", dueDate);

                    command.Parameters.AddWithValue(
                        "@Status", GetStatus());

                    command.ExecuteNonQuery();
                }
            }

            LoadTasks();
            ClearFields();

            MessageBox.Show("Задача добавлена.");
        }

        // Изменение задачи
        private void UpdateButton_Click(
            object sender, RoutedEventArgs e)
        {
            TaskItem task =
                TasksGrid.SelectedItem as TaskItem;

            if (task == null)
            {
                MessageBox.Show("Выберите задачу в таблице.");
                return;
            }

            if (!ValidateFields())
                return;

            object dueDate = DBNull.Value;

            if (dpDueDate.SelectedDate.HasValue)
            {
                dueDate = dpDueDate.SelectedDate.Value
                    .ToString("yyyy-MM-dd");
            }

            using (SQLiteConnection connection =
                   new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                string sql = @"
                    UPDATE Tasks
                    SET Title = @Title,
                        Description = @Description,
                        DueDate = @DueDate,
                        Status = @Status
                    WHERE Id = @Id;";

                using (SQLiteCommand command =
                       new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue(
                        "@Title", txtTitle.Text.Trim());

                    command.Parameters.AddWithValue(
                        "@Description",
                        txtDescription.Text.Trim());

                    command.Parameters.AddWithValue(
                        "@DueDate", dueDate);

                    command.Parameters.AddWithValue(
                        "@Status", GetStatus());

                    command.Parameters.AddWithValue(
                        "@Id", task.Id);

                    command.ExecuteNonQuery();
                }
            }

            LoadTasks();
            ClearFields();

            MessageBox.Show("Задача изменена.");
        }

        // Удаление задачи
        private void DeleteButton_Click(
            object sender, RoutedEventArgs e)
        {
            TaskItem task =
                TasksGrid.SelectedItem as TaskItem;

            if (task == null)
            {
                MessageBox.Show("Выберите задачу в таблице.");
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Удалить выбранную задачу?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            using (SQLiteConnection connection =
                   new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                string sql =
                    "DELETE FROM Tasks WHERE Id = @Id;";

                using (SQLiteCommand command =
                       new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue(
                        "@Id", task.Id);

                    command.ExecuteNonQuery();
                }
            }

            LoadTasks();
            ClearFields();

            MessageBox.Show("Задача удалена.");
        }

        // Заполнение формы при выборе задачи
        private void TasksGrid_SelectionChanged(
            object sender, SelectionChangedEventArgs e)
        {
            TaskItem task =
                TasksGrid.SelectedItem as TaskItem;

            if (task != null)
            {
                txtTitle.Text = task.Title;
                txtDescription.Text = task.Description;

                DateTime date;

                if (DateTime.TryParse(
                    task.DueDate,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out date))
                {
                    dpDueDate.SelectedDate = date;
                }
                else
                {
                    dpDueDate.SelectedDate = null;
                }

                SetStatus(task.Status);
            }
        }

        // Очистка формы
        private void ClearFields()
        {
            TasksGrid.SelectedItem = null;

            txtTitle.Clear();
            txtDescription.Clear();

            dpDueDate.SelectedDate = null;
            cmbStatus.SelectedIndex = 0;
        }

        private void ClearButton_Click(
            object sender, RoutedEventArgs e)
        {
            ClearFields();
        }
    }

    // Класс задачи
    public class TaskItem
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string DueDate { get; set; }

        public string Status { get; set; }
    }
}