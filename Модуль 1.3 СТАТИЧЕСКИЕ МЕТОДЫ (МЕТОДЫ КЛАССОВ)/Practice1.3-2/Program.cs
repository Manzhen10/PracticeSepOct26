class Program
{
    static void Main()
    {
        Console.Write("Введите сумму: ");
        int limit = int.Parse(Console.ReadLine());
        Console.WriteLine($"Заданный предел: {limit}");

        Random r = new Random();
        List<int> massiv = new List<int>();
        int sum = 0;

        while (true)
        {
            int next = r.Next(1, 10); // случайное число от 1 до 9
            if (sum + next > limit)
                break; // превышение служебных

            massiv.Add(next);
            sum += next;
        }

        Console.WriteLine("Созданный массив: " + string.Join(", ", massiv));
        Console.WriteLine($"Количество элементов: {massiv.Count}");
        Console.WriteLine($"Сумма элементов: {sum}");
        

        
    }
}