using System.Globalization;
using System.Text;
using Microsoft.Data.Sqlite;

namespace StudentManager;

internal class Program
{
    private static StudentRepository _repository = null!;

    private static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        string appFolder = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "StudentManager");

        Directory.CreateDirectory(appFolder);

        string databasePath = Path.Combine(appFolder, "students.db");

        try
        {
            _repository = new StudentRepository(databasePath);

            Console.WriteLine("=== Управление студентами ===");
            Console.WriteLine($"База данных: {databasePath}");

            RunMenu();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }

    private static void RunMenu()
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("========== МЕНЮ ==========");
            Console.WriteLine("1. Добавить студента");
            Console.WriteLine("2. Показать всех студентов");
            Console.WriteLine("3. Редактировать студента");
            Console.WriteLine("4. Удалить студента");
            Console.WriteLine("5. Найти студента");
            Console.WriteLine("6. Отсортировать студентов");
            Console.WriteLine("0. Выход");
            Console.WriteLine("==========================");

            Console.Write("Выберите действие: ");
            string choice = Console.ReadLine()?.Trim() ?? "";

            try
            {
                switch (choice)
                {
                    case "1":
                        AddStudent();
                        break;

                    case "2":
                        ShowStudents();
                        break;

                    case "3":
                        UpdateStudent();
                        break;

                    case "4":
                        DeleteStudent();
                        break;

                    case "5":
                        SearchStudents();
                        break;

                    case "6":
                        SortStudents();
                        break;

                    case "0":
                        Console.WriteLine("Программа завершена.");
                        return;

                    default:
                        Console.WriteLine("Такого пункта меню нет.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }
    }

    // Добавление студента

    private static void AddStudent()
    {
        Console.WriteLine();
        Console.WriteLine("--- Добавление студента ---");

        Student student = ReadStudentDetails();

        _repository.Add(student);

        Console.WriteLine("Студент успешно добавлен.");
    }

    // Просмотр студентов

    private static void ShowStudents()
    {
        PrintStudents(_repository.GetAll());
    }

    private static void PrintStudents(IEnumerable<Student> students)
    {
        List<Student> list = students.ToList();

        if (list.Count == 0)
        {
            Console.WriteLine("Список студентов пуст.");
            return;
        }

        Console.WriteLine();

        Console.WriteLine(
            "{0,-5} {1,-15} {2,-15} {3,-12} {4,-12} {5,-8} {6,-8}",
            "ID",
            "Имя",
            "Фамилия",
            "Рождение",
            "Группа",
            "Возраст",
            "Балл");

        Console.WriteLine(new string('-', 85));

        foreach (Student student in list)
        {
            Console.WriteLine(
                "{0,-5} {1,-15} {2,-15} {3,-12} {4,-12} {5,-8} {6,-8}",
                student.Id,
                student.FirstName,
                student.LastName,
                student.BirthDate.ToString("dd.MM.yyyy"),
                student.GroupName,
                student.Age,
                student.AverageGrade.ToString(
                    "0.##",
                    CultureInfo.CurrentCulture));
        }

        Console.WriteLine();
        Console.WriteLine($"Всего студентов: {list.Count}");
    }

    // Редактирование студента

    private static void UpdateStudent()
    {
        Console.WriteLine();
        Console.WriteLine("--- Редактирование студента ---");

        int id = ReadId();

        Student? student = _repository.GetAll()
            .FirstOrDefault(s => s.Id == id);

        if (student == null)
        {
            Console.WriteLine("Студент не найден.");
            return;
        }

        Console.WriteLine("Введите новые данные:");

        Student updatedStudent = ReadStudentDetails(id);

        _repository.Update(updatedStudent);

        Console.WriteLine("Данные студента обновлены.");
    }

    // Удаление студента

    private static void DeleteStudent()
    {
        Console.WriteLine();
        Console.WriteLine("--- Удаление студента ---");

        int id = ReadId();

        Student? student = _repository.GetAll()
            .FirstOrDefault(s => s.Id == id);

        if (student == null)
        {
            Console.WriteLine("Студент не найден.");
            return;
        }

        Console.Write(
            $"Удалить {student.FirstName} {student.LastName}? (да/нет): ");

        string answer = Console.ReadLine()?.Trim().ToLowerInvariant() ?? "";

        if (answer == "да" || answer == "д")
        {
            _repository.Delete(id);
            Console.WriteLine("Студент удалён.");
        }
        else
        {
            Console.WriteLine("Удаление отменено.");
        }
    }

    // Поиск студента

    private static void SearchStudents()
    {
        Console.WriteLine();
        Console.WriteLine("--- Поиск студентов ---");

        Console.Write("Введите имя, фамилию или группу: ");
        string query = Console.ReadLine()?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(query))
        {
            Console.WriteLine("Введите текст для поиска.");
            return;
        }

        List<Student> results = _repository.GetAll()
            .Where(s =>
                s.FirstName.Contains(
                    query, StringComparison.CurrentCultureIgnoreCase)
                ||
                s.LastName.Contains(
                    query, StringComparison.CurrentCultureIgnoreCase)
                ||
                s.GroupName.Contains(
                    query, StringComparison.CurrentCultureIgnoreCase))
            .ToList();

        Console.WriteLine($"Найдено студентов: {results.Count}");
        PrintStudents(results);
    }

    // Сортировка студентов

    private static void SortStudents()
    {
        List<Student> students = _repository.GetAll();

        if (students.Count == 0)
        {
            Console.WriteLine("Список студентов пуст.");
            return;
        }

        Console.WriteLine();
        Console.WriteLine("--- Сортировка ---");
        Console.WriteLine("1. По имени");
        Console.WriteLine("2. По фамилии");
        Console.WriteLine("3. По группе");
        Console.WriteLine("4. По возрасту");
        Console.WriteLine("5. По среднему баллу");

        Console.Write("Выберите вариант: ");
        string choice = Console.ReadLine()?.Trim() ?? "";

        IEnumerable<Student>? sortedStudents = choice switch
        {
            "1" => students.OrderBy(
                s => s.FirstName,
                StringComparer.CurrentCultureIgnoreCase),

            "2" => students.OrderBy(
                s => s.LastName,
                StringComparer.CurrentCultureIgnoreCase),

            "3" => students.OrderBy(
                s => s.GroupName,
                StringComparer.CurrentCultureIgnoreCase),

            "4" => students.OrderBy(s => s.Age),

            "5" => students.OrderByDescending(s => s.AverageGrade),

            _ => null
        };

        if (sortedStudents == null)
        {
            Console.WriteLine("Неверный вариант сортировки.");
            return;
        }

        PrintStudents(sortedStudents);
    }

    // Ввод данных студента

    private static Student ReadStudentDetails(int id = 0)
    {
        string firstName = ReadRequiredText("Имя: ");
        string lastName = ReadRequiredText("Фамилия: ");
        DateTime birthDate = ReadBirthDate();
        string groupName = ReadRequiredText("Группа: ");
        double averageGrade = ReadGrade();

        return new Student
        {
            Id = id,
            FirstName = firstName,
            LastName = lastName,
            BirthDate = birthDate,
            GroupName = groupName,
            AverageGrade = averageGrade
        };
    }

    private static string ReadRequiredText(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string value = Console.ReadLine()?.Trim() ?? "";

            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            Console.WriteLine("Поле не должно быть пустым.");
        }
    }

    // Проверка даты рождения

    private static DateTime ReadBirthDate()
    {
        while (true)
        {
            Console.Write("Дата рождения (дд.мм.гггг): ");
            string input = Console.ReadLine()?.Trim() ?? "";

            if (DateTime.TryParseExact(
                    input,
                    "dd.MM.yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime birthDate)
                && birthDate.Date <= DateTime.Today)
            {
                return birthDate;
            }

            Console.WriteLine(
                "Введите корректную дату не в будущем, например 15.03.2004.");
        }
    }

    // Проверка оценки по 10-балльной шкале

    private static double ReadGrade()
    {
        while (true)
        {
            Console.Write("Средний балл (от 0 до 10): ");
            string input = Console.ReadLine()?.Trim() ?? "";

            input = input.Replace(',', '.');

            if (double.TryParse(
                    input,
                    NumberStyles.AllowLeadingSign |
                    NumberStyles.AllowDecimalPoint,
                    CultureInfo.InvariantCulture,
                    out double grade)
                && double.IsFinite(grade)
                && grade >= 0
                && grade <= 10)
            {
                return grade;
            }

            Console.WriteLine(
                "Введите число от 0 до 10, например 8,5.");
        }
    }

    // Проверка айди

    private static int ReadId()
    {
        while (true)
        {
            Console.Write("Введите ID студента: ");
            string input = Console.ReadLine()?.Trim() ?? "";

            if (int.TryParse(input, out int id) && id > 0)
            {
                return id;
            }

            Console.WriteLine("ID должен быть положительным целым числом.");
        }
    }
}