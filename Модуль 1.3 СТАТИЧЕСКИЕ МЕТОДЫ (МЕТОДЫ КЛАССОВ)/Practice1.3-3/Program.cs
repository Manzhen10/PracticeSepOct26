class Program
{
    static void Main()
    {
        const int n = 5; // столбы и строки

        int[,] matrix = new int[n, n];
        Random rnd = new Random();

        // Заполнение 
        Console.WriteLine("Исходная матрица 5x5:");
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                matrix[i, j] = rnd.Next(-50, 51); 
                Console.Write($"{matrix[i, j],5}");
            }
            Console.WriteLine();
        }

        // суммы строк
        int[] sums = new int[n];
        for (int i = 0; i < n; i++)
        {
            int sum = 0;
            for (int j = 0; j < n; j++)
                sum += matrix[i, j];

            sums[i] = sum;
        }

        // порядок строк по возрастанию
        int[] order = Enumerable.Range(0, n).OrderBy(i => sums[i]).ToArray();

        // Новая
        int[,] sortedMatrix = new int[n, n];
        for (int i = 0; i < n; i++)
        {
            int rowIndex = order[i];
            for (int j = 0; j < n; j++)
                sortedMatrix[i, j] = matrix[rowIndex, j];
        }

        // Вывод 
        Console.WriteLine("Матрица после сортировки строк по возрастанию суммы:");
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
                Console.Write($"{sortedMatrix[i, j],5}");

            Console.WriteLine($"   | сумма = {sums[order[i]]}");
        }

        Console.ReadKey();
    }
}