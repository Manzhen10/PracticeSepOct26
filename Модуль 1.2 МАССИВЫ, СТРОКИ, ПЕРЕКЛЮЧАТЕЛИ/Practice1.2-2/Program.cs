using System;
using System.Linq;
class Program
{
    static void Main()
    {
        int[] massiv = new int[10];
        Random r = new Random();
        for (int i = 0; i < massiv.Length; i++)
        {
            massiv[i] = r.Next(1, 100);
        }
        Console.WriteLine("Исходный массив: " + string.Join(", ", massiv));
        int ver = massiv.Max();
        Console.WriteLine($"Максимальный элемент: {ver}");
        Console.WriteLine("Введите целое число: ");
        int chislo = Convert.ToInt32(Console.ReadLine());
        for (int i = 0; i < massiv.Length; i++)
        {
            if (massiv[i] == ver)
            {
                massiv[i] = chislo;
            }
            else { massiv[i] += 0; }
        }
        Console.WriteLine("Обновленный массив: " + string.Join(", ", massiv));

    }
}