class Program
{
    static void Main()
    {
        int[] massiv = new int[10];
        Random r = new Random();
        for (int i = 0; i < massiv.Length; i++)
        {
            massiv[i] = r.Next(-10, 10);
        }
        Console.WriteLine("Массив: " + String.Join(",", massiv));
        
        int[] indexs = new int[massiv.Length];
        for (int i = 0; i < indexs.Length;i++)
        {
            indexs[i] = i;
        }
        Console.WriteLine("Массив индексов: " + String.Join(",", indexs));
        Array.Sort(massiv);
        for (int i = 0; i < massiv.Length; i++)
        {
            Console.WriteLine("#" + indexs[i] + " = " + massiv[i]);
        }


    }
}