class Program
{
    static void Main()
    {
        Console.Write("Введите первую границу А: ");
        int A = Convert.ToInt32(Console.ReadLine());
        Console.Write("Введите первую границу B: ");
        int B = Convert.ToInt32(Console.ReadLine());
        int[] massiv = new int[10];
        Random r = new Random();
        for (int i = 0; i < massiv.Length; i++)
        {
            massiv[i] = r.Next(A, B);
        }
        Console.WriteLine("Массив: " + string.Join(",", massiv));
        int ver = massiv.Max();
        int min = massiv.Min();
        int start = 0;
        int end = 0;
        for (int i = 0; i < massiv.Length; i++)
        {
            if (massiv[i] == ver)
            {
                end = i;
                Console.WriteLine($"Индекс максимального элемента массива: [{i}]");                
                break;
            }
        }
        for (int l = 0; l < massiv.Length; l++)
        {

            if (massiv[l] == min)
            {
                start = l;
                Console.WriteLine($"Индекс минимального элемента массива: [{l}]");
                break;
                
            }
        }
        Console.WriteLine("Массив в промежутке: ");
        if (start < end)
        {
            for (int v = start; v <= end; v++)
            {
                Console.Write($"{massiv[v]}; ");
            }
        }
        else
        {
            for (int v = end; v <= start; v++)
            {    
                Console.Write($"{massiv[v]}; ");
            }
        } 
        
    }
}
