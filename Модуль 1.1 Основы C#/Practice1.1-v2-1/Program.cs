using System.ComponentModel.Design;

class Program
{
    static void Main()
    {
        Console.WriteLine("Введите число: ");
        if (double.TryParse(Console.ReadLine(), out double n) && n > 0)
        {
            if (n % 2 == 0)
            {
                Console.WriteLine("Число четное");
            }
            else { Console.Write("число не четное"); }
        }
        else { Console.WriteLine("Число введено не корректно"); }
        

    }
}