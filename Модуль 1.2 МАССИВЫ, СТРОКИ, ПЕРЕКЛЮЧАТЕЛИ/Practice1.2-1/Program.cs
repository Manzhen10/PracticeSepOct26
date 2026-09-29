using System;
using System.Linq;
class Program
{
    static void Main()
    {
        double[] massiv = new double[10];
        Random r = new Random();
        for (int i = 0; i < massiv.Length; i++) {
            massiv[i] = r.Next(1, 100);
        }
        Console.WriteLine("Исходный массив: " + String.Join("; ", massiv));
        double verstappen = massiv.Max();
        Console.WriteLine($"Максимальный элемент: {verstappen}");
        for ( int i = 0; i < massiv.Length; i++ )
        {
            massiv[i] = massiv[i] / verstappen;
        }
        Console.WriteLine("Нормированный массив: " + String.Join ("; ", massiv));
        
    }
}