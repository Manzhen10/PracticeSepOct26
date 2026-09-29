class Programm
{
    static void Main()
    {
        Console.Write("Введите числитель: ");
        int A = Convert.ToInt32(Console.ReadLine());
        Console.Write("Введите знаменатель: ");
        int B = Convert.ToInt32(Console.ReadLine());
        Console.WriteLine($"Введенная дробь: {A}/{B} ");
        Gcm(A, B);      
        
    }
    static void Gcm(int A, int B)
    {
        int samiy = 0;
        int d = Math.Min(A, B);
        int stay = Math.Min(A, B);
        int maxim = Math.Max(A, B);
        while (d >= 0)
        {
            if (maxim % d == 0 && stay % d == 0)
            {
                samiy += d;
                break;
            }
            else
            {
                d--;
            }
        }
        Console.WriteLine($"Наибольший делитель: {samiy}");
        int As = A / samiy;
        int Bs = B / samiy;
        Console.WriteLine($"Сокращенная дробь: {As}/{Bs} ");
    }
}