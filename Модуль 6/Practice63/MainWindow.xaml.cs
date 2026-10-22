
using System;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Practice63
{
    public partial class MainWindow : Window
    {
        private string connectionString;
        private ObservableCollection<Book> books =
            new ObservableCollection<Book>();

        public MainWindow()
        {
            InitializeComponent();

            string folder = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "Practice63");

            Directory.CreateDirectory(folder);

            string databasePath = Path.Combine(folder, "library.db");

            connectionString = "Data Source=" + databasePath +
                               ";Version=3;";

            CreateDatabase();
            LoadBooks("");
        }

        // Создание базы данных и таблицы
        private void CreateDatabase()
        {
            using (SQLiteConnection connection =
                   new SQLiteConnection(connectionString))
            {
                connection.Open();

                string sql =
                    "CREATE TABLE IF NOT EXISTS Books (" +
                    "Id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                    "Title TEXT NOT NULL, " +
                    "Author TEXT NOT NULL, " +
                    "Year INTEGER NOT NULL, " +
                    "IsRented INTEGER NOT NULL DEFAULT 0)";

                using (SQLiteCommand command =
                       new SQLiteCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        // Загрузка и поиск книг
        private void LoadBooks(string search)
        {
            books.Clear();

            using (SQLiteConnection connection =
                   new SQLiteConnection(connectionString))
            {
                connection.Open();

                string sql =
                    "SELECT Id, Title, Author, Year, IsRented " +
                    "FROM Books " +
                    "WHERE Title LIKE @search OR Author LIKE @search " +
                    "ORDER BY Title";

                using (SQLiteCommand command =
                       new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue(
                        "@search", "%" + search + "%");

                    using (SQLiteDataReader reader =
                           command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Book book = new Book();

                            book.Id = Convert.ToInt32(reader["Id"]);
                            book.Title = reader["Title"].ToString();
                            book.Author = reader["Author"].ToString();
                            book.Year = Convert.ToInt32(reader["Year"]);
                            book.IsRented =
                                Convert.ToInt32(reader["IsRented"]) == 1;

                            books.Add(book);
                        }
                    }
                }
            }

            BooksGrid.ItemsSource = books;
        }

        // Добавление книги
        private void AddBook_Click(object sender, RoutedEventArgs e)
        {
            string title = txtTitle.Text.Trim();
            string author = txtAuthor.Text.Trim();
            int year;

            if (title == "" || author == "")
            {
                MessageBox.Show(
                    "Введите название книги и автора.",
                    "Проверка данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtYear.Text, out year) ||
                year < 1 || year > DateTime.Now.Year)
            {
                MessageBox.Show(
                    "Введите корректный год издания.",
                    "Проверка данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            using (SQLiteConnection connection =
                   new SQLiteConnection(connectionString))
            {
                connection.Open();

                string sql =
                    "INSERT INTO Books (Title, Author, Year, IsRented) " +
                    "VALUES (@title, @author, @year, 0)";

                using (SQLiteCommand command =
                       new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@title", title);
                    command.Parameters.AddWithValue("@author", author);
                    command.Parameters.AddWithValue("@year", year);

                    command.ExecuteNonQuery();
                }
            }

            txtTitle.Clear();
            txtAuthor.Clear();
            txtYear.Clear();

            LoadBooks(txtSearch.Text);

            MessageBox.Show("Книга успешно добавлена!");
        }

        // Взять книгу
        private void RentBook_Click(object sender, RoutedEventArgs e)
        {
            Book selectedBook = BooksGrid.SelectedItem as Book;

            if (selectedBook == null)
            {
                MessageBox.Show("Сначала выберите книгу в списке.");
                return;
            }

            if (selectedBook.IsRented)
            {
                MessageBox.Show("Эта книга уже выдана.");
                return;
            }

            UpdateRentalStatus(selectedBook.Id, true);
            LoadBooks(txtSearch.Text);

            MessageBox.Show("Книга выдана.");
        }

        // Вернуть книгу
        private void ReturnBook_Click(object sender, RoutedEventArgs e)
        {
            Book selectedBook = BooksGrid.SelectedItem as Book;

            if (selectedBook == null)
            {
                MessageBox.Show("Сначала выберите книгу в списке.");
                return;
            }

            if (!selectedBook.IsRented)
            {
                MessageBox.Show("Эта книга уже находится в библиотеке.");
                return;
            }

            UpdateRentalStatus(selectedBook.Id, false);
            LoadBooks(txtSearch.Text);

            MessageBox.Show("Книга возвращена.");
        }

        // Изменение статуса книги в базе
        private void UpdateRentalStatus(int id, bool isRented)
        {
            using (SQLiteConnection connection =
                   new SQLiteConnection(connectionString))
            {
                connection.Open();

                string sql =
                    "UPDATE Books SET IsRented = @status WHERE Id = @id";

                using (SQLiteCommand command =
                       new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue(
                        "@status", isRented ? 1 : 0);

                    command.Parameters.AddWithValue("@id", id);

                    command.ExecuteNonQuery();
                }
            }
        }

        // Поиск при вводе текста
        private void txtSearch_TextChanged(
            object sender, TextChangedEventArgs e)
        {
            if (connectionString != null && BooksGrid != null)
            {
                LoadBooks(txtSearch.Text);
            }
        }

        // Очистка поиска
        private void ClearSearch_Click(
            object sender, RoutedEventArgs e)
        {
            txtSearch.Clear();
            LoadBooks("");
        }
    }

    // Модель книги
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public int Year { get; set; }
        public bool IsRented { get; set; }

        public string Status
        {
            get
            {
                return IsRented ? "На руках" : "Доступна";
            }
        }
    }
}
