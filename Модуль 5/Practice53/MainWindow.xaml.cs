
using System.Collections.ObjectModel;
using System.Windows;

namespace Practice53
{
    public partial class MainWindow : Window
    {
        ObservableCollection<string> tasks = new ObservableCollection<string>();

        public MainWindow()
        {
            InitializeComponent();
            TaskList.ItemsSource = tasks;
        }

        // Добавление задачи
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            string task = TaskBox.Text.Trim();

            if (task == "")
            {
                MessageBox.Show("Введите задачу!");
                return;
            }

            tasks.Add(task);
            TaskBox.Clear();
        }

        // Выполнение задачи
        private void Done_Click(object sender, RoutedEventArgs e)
        {
            int index = TaskList.SelectedIndex;

            if (index == -1)
            {
                MessageBox.Show("Выберите задачу!");
                return;
            }

            if (!tasks[index].StartsWith("✓ "))
            {
                tasks[index] = "✓ " + tasks[index];
            }
        }

        // Удаление задачи
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            int index = TaskList.SelectedIndex;

            if (index == -1)
            {
                MessageBox.Show("Выберите задачу!");
                return;
            }

            tasks.RemoveAt(index);
        }
    }
}
