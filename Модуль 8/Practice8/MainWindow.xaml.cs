using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace Practice8
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        // Все задачи и события
        private readonly ObservableCollection<PlannerItem> _items =
            new ObservableCollection<PlannerItem>();

        private ICollectionView _itemsView;

        // Запись
        private PlannerItem _editingItem;

        // Таймер
        private readonly DispatcherTimer _reminderTimer;

        // База
        private bool _databaseReady;

        public event PropertyChangedEventHandler PropertyChanged;

        //посветить
        public IEnumerable<DateTime> DaysWithEntries
        {
            get;
            private set;
        } = new List<DateTime>();

        public MainWindow()
        {
            InitializeComponent();

            _itemsView = CollectionViewSource.GetDefaultView(_items);

            _itemsView.Filter = FilterItem;

            _itemsView.SortDescriptions.Add(
                new SortDescription(
                    nameof(PlannerItem.Deadline),
                    ListSortDirection.Ascending));

            ItemsGrid.ItemsSource = _itemsView;

            PlannerCalendar.SelectedDate = DateTime.Today;

            UpdateSelectedDateText();
            UpdateDaysWithEntries();
            UpdateStatistics();

            // тайме который проверяет напоминания
            _reminderTimer = new DispatcherTimer();

            _reminderTimer.Interval = TimeSpan.FromSeconds(30);

            _reminderTimer.Tick += ReminderTimer_Tick;

            Loaded += MainWindow_Loaded;
        }

//бд
        private void MainWindow_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                DataStorage.Initialize();

                LoadItems();

                _databaseReady = true;

 
                RefreshItemsView();
                _reminderTimer.Start();

                CheckReminders();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    "Не удалось запустить базу данных.\n\n"
                    + ex.Message
                    + "\n\nПроверь установку пакета SQLite "
                    + "и настройки платформы проекта.",
                    "Ошибка запуска",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Close();
            }
        }

        private void LoadItems()
        {
            List<PlannerItem> loadedItems = DataStorage.Load();

            foreach (PlannerItem item in loadedItems)
            {
                _items.Add(item);
            }
        }

//сохранение
        private void SaveItems()
        {
            if (!_databaseReady)
            {
                return;
            }

            try
            {
                DataStorage.Save(_items);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    "Не удалось сохранить данные.\n\n"
                    + ex.Message,
                    "Ошибка сохранения",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        private void RefreshItemsView()
        {
            if (_itemsView != null)
            {
                _itemsView.Refresh();
            }

            if (ItemsGrid != null)
            {
                ItemsGrid.Items.Refresh();
            }

            UpdateDaysWithEntries();
            UpdateStatistics();

            if (PlannerCalendar != null)
            {
                PlannerCalendar.InvalidateVisual();
            }
        }

        private void UpdateDaysWithEntries()
        {
            DaysWithEntries = _items
                .Select(item => item.Deadline.Date)
                .Distinct()
                .ToList();

            OnPropertyChanged(nameof(DaysWithEntries));
        }

        private void UpdateStatistics()
        {
            if (StatsTextBlock == null)
            {
                return;
            }

            int total = _items.Count;

            int completed = _items.Count(
                item => item.IsCompleted);

            int active = total - completed;

            StatsTextBlock.Text =
                "Всего: " + total
                + "  |  Выполнено: " + completed
                + "  |  Активных: " + active;
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(
                    this,
                    new PropertyChangedEventArgs(propertyName));
            }
        }

//календарь
        private bool FilterItem(object obj)
        {
            PlannerItem item = obj as PlannerItem;

            if (item == null)
            {
                return false;
            }

            if (ShowAllCheckBox.IsChecked == true)
            {
                return true;
            }

            DateTime selectedDate =
                PlannerCalendar.SelectedDate ?? DateTime.Today;

            return item.Deadline.Date == selectedDate.Date;
        }

        private void PlannerCalendar_SelectedDatesChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            UpdateSelectedDateText();

            RefreshItemsView();
        }

        private void UpdateSelectedDateText()
        {
            DateTime selectedDate =
                PlannerCalendar.SelectedDate ?? DateTime.Today;

            SelectedDateTextBlock.Text =
                "Выбрано: "
                + selectedDate.ToString("dd.MM.yyyy");

            SelectedDateTextBlock2.Text =
                selectedDate.ToString("dd.MM.yyyy");
        }

        private void FilterSettings_Changed(
            object sender,
            RoutedEventArgs e)
        {
            RefreshItemsView();
        }

        private void TodayButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            PlannerCalendar.SelectedDate = DateTime.Today;

            ShowAllCheckBox.IsChecked = false;

            UpdateSelectedDateText();

            RefreshItemsView();
        }

        private bool TryReadForm(out PlannerItem item)
        {
            item = null;

            // Название обязательно
            string title = TitleTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show(
                    this,
                    "Введи название задачи или события.",
                    "Проверка данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                TitleTextBox.Focus();

                return false;
            }

            // Проверка выбранной даты
            if (!PlannerCalendar.SelectedDate.HasValue)
            {
                MessageBox.Show(
                    this,
                    "Выбери дату в календаре.",
                    "Проверка данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return false;
            }

            // Проверка времени
            string[] timeFormats = { "HH:mm", "H:mm" };

            DateTime timeValue;

            bool validTime = DateTime.TryParseExact(
                TimeTextBox.Text.Trim(),
                timeFormats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out timeValue);

            if (!validTime)
            {
                MessageBox.Show(
                    this,
                    "Укажи корректное время, например 09:30.",
                    "Проверка данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                TimeTextBox.Focus();

                return false;
            }

            // тип записи
            ComboBoxItem selectedType =
                TypeComboBox.SelectedItem as ComboBoxItem;

            if (selectedType == null)
            {
                MessageBox.Show(
                    this,
                    "Выбери тип записи.",
                    "Проверка данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return false;
            }

            // Календарь плюс дата
            DateTime selectedDate =
                PlannerCalendar.SelectedDate.Value;

            DateTime deadline = new DateTime(
                selectedDate.Year,
                selectedDate.Month,
                selectedDate.Day,
                timeValue.Hour,
                timeValue.Minute,
                0);

           //По стандарту напоминание за десятку
            int reminderMinutes = 10;

            ComboBoxItem selectedReminder =
                ReminderMinutesComboBox.SelectedItem
                as ComboBoxItem;

            if (selectedReminder != null)
            {
                int parsedMinutes;

                if (int.TryParse(
                    selectedReminder.Content.ToString(),
                    out parsedMinutes))
                {
                    reminderMinutes = parsedMinutes;
                }
            }

            bool hasReminder =
                HasReminderCheckBox.IsChecked == true;

            bool sameReminderSchedule =
                _editingItem != null
                && _editingItem.Deadline == deadline
                && _editingItem.HasReminder == hasReminder
                && _editingItem.ReminderMinutesBefore
                    == reminderMinutes;

            item = new PlannerItem
            {
                Title = title,

                Description = DescriptionTextBox.Text.Trim(),

                Category = selectedType.Content.ToString(),

                Deadline = deadline,

                IsCompleted = _editingItem != null
                    && _editingItem.IsCompleted,

                HasReminder = hasReminder,

                ReminderMinutesBefore = reminderMinutes,

                ReminderShown = sameReminderSchedule
                    && _editingItem.ReminderShown
            };

            return true;
        }

 //Работа

        private void AddOrUpdateButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            PlannerItem newItem;

            if (!TryReadForm(out newItem))
            {
                return;
            }

            if (_editingItem == null)
            {
                // Добавление новой записи
                _items.Add(newItem);
            }
            else
            {
                // Замена старой записи изменённой
                int index = _items.IndexOf(_editingItem);

                if (index >= 0)
                {
                    _items[index] = newItem;
                }
                else
                {
                    _items.Add(newItem);
                }
            }

            SaveItems();

            RefreshItemsView();

            ClearForm();
        }

        private void EditSelectedButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            PlannerItem selectedItem =
                ItemsGrid.SelectedItem as PlannerItem;

            if (selectedItem == null)
            {
                MessageBox.Show(
                    this,
                    "Сначала выбери запись в таблице.",
                    "Редактирование",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            _editingItem = selectedItem;
            TitleTextBox.Text =
                selectedItem.Title ?? string.Empty;

            DescriptionTextBox.Text =
                selectedItem.Description ?? string.Empty;

            TimeTextBox.Text =
                selectedItem.Deadline.ToString("HH:mm");

            PlannerCalendar.SelectedDate =
                selectedItem.Deadline.Date;

 //тип
            for (int i = 0; i < TypeComboBox.Items.Count; i++)
            {
                ComboBoxItem comboItem =
                    TypeComboBox.Items[i] as ComboBoxItem;

                if (comboItem != null
                    && comboItem.Content.ToString()
                        == selectedItem.Category)
                {
                    TypeComboBox.SelectedIndex = i;
                    break;
                }
            }

            HasReminderCheckBox.IsChecked =
                selectedItem.HasReminder;

 // когда напомнить
            bool reminderFound = false;

            for (int i = 0;
                 i < ReminderMinutesComboBox.Items.Count;
                 i++)
            {
                ComboBoxItem comboItem =
                    ReminderMinutesComboBox.Items[i]
                    as ComboBoxItem;

                int minutes;

                if (comboItem != null
                    && int.TryParse(
                        comboItem.Content.ToString(),
                        out minutes)
                    && minutes == selectedItem.ReminderMinutesBefore)
                {
                    ReminderMinutesComboBox.SelectedIndex = i;

                    reminderFound = true;

                    break;
                }
            }

            if (!reminderFound)
            {
                ReminderMinutesComboBox.SelectedIndex = 1;
            }

            AddOrUpdateButton.Content = "Сохранить изменения";

            CancelEditButton.Visibility = Visibility.Visible;

            TitleTextBox.Focus();
        }
//редактирование по двойному тапу
        private void ItemsGrid_MouseDoubleClick(
            object sender,
            MouseButtonEventArgs e)
        {
            if (ItemsGrid.SelectedItem != null)
            {
                EditSelectedButton_Click(sender, e);
            }
        }

//очитска формы

        private void ClearForm()
        {
            _editingItem = null;

            TitleTextBox.Clear();

            DescriptionTextBox.Clear();

            TypeComboBox.SelectedIndex = 0;

            TimeTextBox.Text = "09:00";

            HasReminderCheckBox.IsChecked = true;

            ReminderMinutesComboBox.SelectedIndex = 1;

            AddOrUpdateButton.Content = "+ Добавить запись";

            CancelEditButton.Visibility = Visibility.Collapsed;

            TitleTextBox.Focus();
        }

        private void ClearFormButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            ClearForm();
        }

        private void CancelEditButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            ClearForm();
        }

//кнопка снести все

        private void DeleteSelectedButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            PlannerItem selectedItem =
                ItemsGrid.SelectedItem as PlannerItem;

            if (selectedItem == null)
            {
                MessageBox.Show(
                    this,
                    "Выбери запись, которую нужно удалить.",
                    "Удаление",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            MessageBoxResult result = MessageBox.Show(
                this,
                "Удалить запись \"" + selectedItem.Title + "\"?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            if (_editingItem == selectedItem)
            {
                ClearForm();
            }

            _items.Remove(selectedItem);

            SaveItems();

            RefreshItemsView();
        }

//статус

        private void MarkCompletedButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            PlannerItem selectedItem =
                ItemsGrid.SelectedItem as PlannerItem;

            if (selectedItem == null)
            {
                MessageBox.Show(
                    this,
                    "Выбери запись в таблице.",
                    "Изменение статуса",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            selectedItem.IsCompleted = !selectedItem.IsCompleted;

            if (selectedItem.IsCompleted)
            {
                selectedItem.ReminderShown = true;
            }
            else
            {

                selectedItem.ReminderShown = false;
            }

            SaveItems();

            RefreshItemsView();
        }

//напоминания

        private void ReminderTimer_Tick(
            object sender,
            EventArgs e)
        {
            CheckReminders();
        }

        private void CheckReminders()
        {
            DateTime now = DateTime.Now;

            // Записи которые будуь напомниатся
            List<PlannerItem> dueItems = _items
                .Where(item =>
                    item.HasReminder
                    && !item.IsCompleted
                    && !item.ReminderShown
                    && now >= item.Deadline.AddMinutes(
                        -item.ReminderMinutesBefore))
                .ToList();

            foreach (PlannerItem item in dueItems)
            {
                // Напоминание уже показано не трогать
                item.ReminderShown = true;

                SaveItems();

                string message;

                if (item.Deadline < now)
                {
                    message =
                        "Срок выполнения уже прошёл.\n\n"
                        + "Запись: " + item.Title + "\n"
                        + "Срок: "
                        + item.Deadline.ToString("dd.MM.yyyy HH:mm");
                }
                else
                {
                    message =
                        "Скоро наступит срок выполнения!\n\n"
                        + "Запись: " + item.Title + "\n"
                        + "Срок: "
                        + item.Deadline.ToString("dd.MM.yyyy HH:mm");
                }

                MessageBox.Show(
                    this,
                    message,
                    "Напоминание",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        // Закрытие

        protected override void OnClosed(EventArgs e)
        {
            _reminderTimer.Stop();

            base.OnClosed(e);
        }
    }
}