class Person
{
    public string name;
    public int age;
    public string address;

    public void zadat()
    {
        Console.Write("Введите имя: ");
        name = Console.ReadLine();
        Console.Write("Введите возраст: ");
        age = Convert.ToInt32(Console.ReadLine());
        Console.Write("Введите адрес: ");
        address = Console.ReadLine();
    }
    public void Vivod()
    {
        Console.WriteLine($"Имя: {name}");
        Console.WriteLine($"Возраст: {age}");
        Console.WriteLine($"Адрес: {address}");
    }
    class Program
    {
        static void Main()
        {
            Person lyud = new Person();
            lyud.zadat();
            lyud.Vivod();
        }
    }
}