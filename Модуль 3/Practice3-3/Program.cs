delegate void Task(string taskName);

class UserTask
{
    public string Name;
    public Task R;

    public UserTask(string name, Task r)
    {
        Name = name;
        R = r;
    }
}

class Program
{
    // Отправка уведомления
    static void SendUved(string taskName)
    {
        Console.WriteLine("Уведомление: " + taskName);
    }

    // Запись в журнал
    static void Write(string taskName)
    {
        Console.WriteLine("Запись в журнал: " + taskName);
    }

    static void Main()
    {
        List<UserTask> tasks = new List<UserTask>();

        while (true)
        {
            Console.Write("Введите задачу (. ля завершения записи и вывода записанного на экран): ");
            string? name = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(name) || name.ToLower() == ".")
                break;

            Console.WriteLine("Выберите действие:");
            Console.WriteLine("1 - Отправить уведомление");
            Console.WriteLine("2 - Записать в журнал");

            string? choice = Console.ReadLine();

            Task handler;

            switch (choice)
            {
                case "1":
                    handler = SendUved;
                    break;

                case "2":
                    handler = Write;
                    break;

                default:
                    Console.WriteLine("Неверный выбор");
                    continue;
            }

            tasks.Add(new UserTask(name, handler));
        }

        Console.WriteLine("\nВыполнение задач:");

        foreach (UserTask task in tasks)
        {
            task.R(task.Name);
        }
    }
}