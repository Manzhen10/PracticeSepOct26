using System.Linq;
class Program
{
    static void Main()
    {
        double[] massiv = new double[15];
        Random r = new Random();
        for (int i = 0; i < 15; i++)
        {
            massiv[i] = r.Next(1, 100);
        }
        Console.WriteLine(String.Join(", ", massiv));
        double verstappen = massiv.Max();
        double min = massiv.Min();
        Console.WriteLine($"Максимальное значение = {verstappen}");
        Console.WriteLine($"Максимальное значение = {min}");
    }
}