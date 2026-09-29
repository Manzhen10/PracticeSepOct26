class Program
{
    static void Main()
    {
        Console.Write("Введите возраст: ");
        if (int.TryParse(Console.ReadLine(), out int age) && age >= 18 )
        {
            Console.WriteLine($"Пользователь может получить водительские права, так как ему {age} лет");
        }
        else
        {
            Console.WriteLine("Пользователь не сможет получить водительские права");
        }
    }
}