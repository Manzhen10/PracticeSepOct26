interface IDrawing
{
    void Line(int x1, int y1, int x2, int y2);
    void Circle(int x, int y, int r);
    void Rectangle(int x, int y, int w, int h);
}

class Canvas : IDrawing
{
    public void Line(int x1, int y1, int x2, int y2)
    {
        Console.WriteLine($"Линия: ({x1}, {y1}) - ({x2}, {y2})");
    }

    public void Circle(int x, int y, int r)
    {
        Console.WriteLine($"Круг: центр ({x}, {y}), радиус {r}");
    }

    public void Rectangle(int x, int y, int w, int h)
    {
        Console.WriteLine($"Прямоугольник: ({x}, {y}), ширина {w}, высота {h}");
    }
}

class Program
{
    static void Main()
    {

        static int Read(string text)
        {
            while (true)
            {
                Console.Write(text);

                if (int.TryParse(Console.ReadLine(), out int num))
                    return num;

                Console.WriteLine("Введите целое число!");
            }
        }

        IDrawing canvas = new Canvas();

        while (true)
        {
            Console.WriteLine("Что нарисовать?");
            Console.WriteLine("1. Линия");
            Console.WriteLine("2. Круг");
            Console.WriteLine("3. Прямоугольник");
            Console.WriteLine("0. Выход");
            Console.Write("Выбор: ");

            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    int x1 = Read("Введите x1: ");
                    int y1 = Read("Введите y1: ");
                    int x2 = Read("Введите x2: ");
                    int y2 = Read("Введите y2: ");
                    canvas.Line(x1, y1, x2, y2);
                    break;

                case "2":
                    int x = Read("Введите координату x: ");
                    int y = Read("Введите координату y: ");
                    int r = Read("Введите радиус: ");
                    canvas.Circle(x, y, r);
                    break;

                case "3":
                    int a = Read("Введите координату x: ");
                    int b = Read("Введите координату y: ");
                    int w = Read("Введите ширину: ");
                    int h = Read("Введите высоту: ");
                    canvas.Rectangle(a, b, w, h);
                    break;

                case "0":
                    return;

                default:
                    Console.WriteLine("Неверный выбор.");
                    break;
            }
        }
    }   
}