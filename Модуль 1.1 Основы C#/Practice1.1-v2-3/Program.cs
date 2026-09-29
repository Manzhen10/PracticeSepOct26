class Program
{
    static void Main()
    {
        Console.Write("Введите имя: ");
        string name = Console.ReadLine();
        Console.Write("Введите фамилию: ");
        string fam = Console.ReadLine();
        Console.WriteLine($"Вывод: {fam} {name}");
    }
}