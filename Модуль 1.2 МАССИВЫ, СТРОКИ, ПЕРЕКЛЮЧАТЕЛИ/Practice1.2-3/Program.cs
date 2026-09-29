class Program
{
    static void Main()
    {
        Console.WriteLine("Введите значение k: ");
        if (int.TryParse(Console.ReadLine(), out int k) && k > 0)
        {

            int pr = 2;
            while (k > 0)
            {
                if (pr / 2 == 1 || pr % 2 > 0 && pr % 3 > 0 && pr % 10 > 0)
                {
                    Console.Write($"{pr} ");
                    k -= 1;
                    pr += 1;
                }
                else
                {
                    pr += 1;
                }
            }
        }
        else
        {
            Console.WriteLine("Введите корректное значение: ");
        }
    }
}