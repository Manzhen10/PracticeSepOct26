using System;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Practice61
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<Employee> employees =
            new ObservableCollection<Employee>();

        private string dbPath = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "Practice61",
            "employees.db");

        private string ConnectionString
        {
            get { return "Data Source=" + dbPath + ";Version=3;"; }
        }

        public MainWindow()
        {
            InitializeComponent();

            EmployeesGrid.ItemsSource = employees;

            Directory.CreateDirectory(Path.GetDirectoryName(dbPath));

            CreateDatabase();
            LoadEmployees();
        }

        // Создание базы данных
        private void CreateDatabase()
        {
            using (SQLiteConnection connection =
                   new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                string sql = @"
                    CREATE TABLE IF NOT EXISTS Employees
                    (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        FullName TEXT NOT NULL,
                        Position TEXT,
                        Department TEXT,
                        Phone TEXT,
                        Salary REAL NOT NULL
                    );";

                using (SQLiteCommand command =
                       new SQLiteCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        // Загрузка сотрудников из базы
        private void LoadEmployees()
        {
            employees.Clear();

            using (SQLiteConnection connection =
                   new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                string sql = @"
                    SELECT Id, FullName, Position,
                           Department, Phone, Salary
                    FROM Employees
                    ORDER BY Id;";

                using (SQLiteCommand command =
                       new SQLiteCommand(sql, connection))
                {
                    using (SQLiteDataReader reader =
                           command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Employee employee = new Employee();

                            employee.Id = Convert.ToInt32(
                                reader["Id"]);

                            employee.FullName = Convert.ToString(
                                reader["FullName"]);

                            employee.Position = Convert.ToString(
                                reader["Position"]);

                            employee.Department = Convert.ToString(
                                reader["Department"]);

                            employee.Phone = Convert.ToString(
                                reader["Phone"]);

                            employee.Salary = Convert.ToDouble(
                                reader["Salary"]);

                            employees.Add(employee);
                        }
                    }
                }
            }
        }

        // Проверка введённых данных
        private bool ValidateFields(out double salary)
        {
            salary = 0;

            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("Введите ФИО сотрудника.");
                return false;
            }

            string salaryText = txtSalary.Text.Trim()
                .Replace(',', '.');

            if (!double.TryParse(
                salaryText,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out salary))
            {
                MessageBox.Show("Введите правильный оклад.");
                return false;
            }

            if (salary < 0)
            {
                MessageBox.Show("Оклад не может быть отрицательным.");
                return false;
            }

            return true;
        }

        // Добавление сотрудника
        private void AddButton_Click(
            object sender, RoutedEventArgs e)
        {
            double salary;

            if (!ValidateFields(out salary))
                return;

            using (SQLiteConnection connection =
                   new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                string sql = @"
                    INSERT INTO Employees
                    (FullName, Position, Department, Phone, Salary)
                    VALUES
                    (@FullName, @Position, @Department, @Phone, @Salary);";

                using (SQLiteCommand command =
                       new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue(
                        "@FullName", txtFullName.Text.Trim());

                    command.Parameters.AddWithValue(
                        "@Position", txtPosition.Text.Trim());

                    command.Parameters.AddWithValue(
                        "@Department", txtDepartment.Text.Trim());

                    command.Parameters.AddWithValue(
                        "@Phone", txtPhone.Text.Trim());

                    command.Parameters.AddWithValue(
                        "@Salary", salary);

                    command.ExecuteNonQuery();
                }
            }

            LoadEmployees();
            ClearFields();

            MessageBox.Show("Сотрудник добавлен.");
        }

        // Изменение сотрудника
        private void UpdateButton_Click(
            object sender, RoutedEventArgs e)
        {
            Employee employee =
                EmployeesGrid.SelectedItem as Employee;

            if (employee == null)
            {
                MessageBox.Show("Выберите сотрудника в таблице.");
                return;
            }

            double salary;

            if (!ValidateFields(out salary))
                return;

            using (SQLiteConnection connection =
                   new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                string sql = @"
                    UPDATE Employees
                    SET FullName = @FullName,
                        Position = @Position,
                        Department = @Department,
                        Phone = @Phone,
                        Salary = @Salary
                    WHERE Id = @Id;";

                using (SQLiteCommand command =
                       new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue(
                        "@FullName", txtFullName.Text.Trim());

                    command.Parameters.AddWithValue(
                        "@Position", txtPosition.Text.Trim());

                    command.Parameters.AddWithValue(
                        "@Department", txtDepartment.Text.Trim());

                    command.Parameters.AddWithValue(
                        "@Phone", txtPhone.Text.Trim());

                    command.Parameters.AddWithValue(
                        "@Salary", salary);

                    command.Parameters.AddWithValue(
                        "@Id", employee.Id);

                    command.ExecuteNonQuery();
                }
            }

            LoadEmployees();
            ClearFields();

            MessageBox.Show("Данные изменены.");
        }

        // Удаление сотрудника
        private void DeleteButton_Click(
            object sender, RoutedEventArgs e)
        {
            Employee employee =
                EmployeesGrid.SelectedItem as Employee;

            if (employee == null)
            {
                MessageBox.Show("Выберите сотрудника в таблице.");
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Удалить выбранного сотрудника?",
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
                    "DELETE FROM Employees WHERE Id = @Id;";

                using (SQLiteCommand command =
                       new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue(
                        "@Id", employee.Id);

                    command.ExecuteNonQuery();
                }
            }

            LoadEmployees();
            ClearFields();

            MessageBox.Show("Сотрудник удалён.");
        }

        // Заполнение полей при выборе строки
        private void EmployeesGrid_SelectionChanged(
            object sender, SelectionChangedEventArgs e)
        {
            Employee employee =
                EmployeesGrid.SelectedItem as Employee;

            if (employee != null)
            {
                txtFullName.Text = employee.FullName;
                txtPosition.Text = employee.Position;
                txtDepartment.Text = employee.Department;
                txtPhone.Text = employee.Phone;

                txtSalary.Text = employee.Salary.ToString(
                    CultureInfo.CurrentCulture);
            }
        }

        // Очистка полей
        private void ClearFields()
        {
            EmployeesGrid.SelectedItem = null;

            txtFullName.Clear();
            txtPosition.Clear();
            txtDepartment.Clear();
            txtPhone.Clear();
            txtSalary.Clear();
        }
    }

    // Класс сотрудника
    public class Employee
    {
        public int Id { get; set; }

        public string FullName { get; set; }

        public string Position { get; set; }

        public string Department { get; set; }

        public string Phone { get; set; }

        public double Salary { get; set; }
    }
}