using System.Globalization;

delegate bool Filter(DataItem item);

class DataItem
{
    public string Text;
    public DateTime Date;

    public DataItem(string text, DateTime date)
    {
        Text = text;
        Date = date;
    }
}

class Program
{
    static void Main()
    {
        List<DataItem> list = new List<DataItem>
        {
            new DataItem("Изучить делегаты", new DateTime(2026, 10, 06)),
            new DataItem("Написать программу", new DateTime(2026, 10, 12)),
            new DataItem("Ашалеть от количества материала", new DateTime(2026, 10, 14)),
            new DataItem("Сдать лабораторную", new DateTime(2135, 10, 16))
        };

        while (true)
        {
            Console.WriteLine("\nВыберите фильтр:");
            Console.WriteLine("1. По слову");
            Console.WriteLine("2. По дате");

            string? choice = Console.ReadLine();
            Filter filter;

            switch (choice)
            {
                case "1":
                    {
                        Console.Write("Введите ключевое слово: ");
                        string? word = Console.ReadLine();

                        if (string.IsNullOrWhiteSpace(word))
                        {
                            Console.WriteLine("Введите слово правильно");
                            continue;
                        }

                        filter = item => item.Text.Contains(word, StringComparison.OrdinalIgnoreCase);
                        break;
                    }

                case "2":
                    {
                        Console.Write("Введите дату (д.м.г): ");
                        string? input = Console.ReadLine();

                        if (!DateTime.TryParseExact(input, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                        {
                            Console.WriteLine("Неверный формат даты!");
                            continue;
                        }

                        filter = item => item.Date.Date == date.Date;
                        break;
                    }

                default:
                    Console.WriteLine("Неверный выбор! Введите 1 или 2.");
                    continue;
            }

            Console.WriteLine("\nРезультат фильтрации:");

            bool found = false;

            foreach (DataItem item in list)
            {
                if (filter(item))
                {
                    Console.WriteLine($"{item.Date:dd.MM.yyyy} — {item.Text}");
                    found = true;
                }
            }

            if (!found)
                Console.WriteLine("Подходящих записей не найдено.");

            break;
        }
    }
}