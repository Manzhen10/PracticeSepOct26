using System;
using static System.Math;

class Program
{
    static void Main()
    {
        Console.WriteLine("Введите значение радиуса =");
        if (double.TryParse(Console.ReadLine(), out double r) && r > 0)
        {
            double S = Math.PI * Math.Pow(r, 2);
            Console.WriteLine($"Площадь круга = {S}");
        }
        else { Console.WriteLine("Значение отрицательное или введено не корректно"); }
    }
}