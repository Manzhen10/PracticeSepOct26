class Shape
{
    public virtual double Area()
    {
        return 0;
    }
    public virtual double Perimetr()
    {
        return 0;
    }
}
class Circle : Shape
{
    public double Radius;

    public override double Area()
    {
        return Math.PI * Math.Pow(Radius, 2); 
    }

    public override double Perimetr()
    {
        return 2 * Math.PI * Radius;
    }
}
class Rectangle : Shape
{
    public double Width;
    public double Height;

    
    public override double Area()
    {
        return Width * Height; 
    }

    public override double Perimetr()
    {
        return 2 * (Width + Height); 
    }
}
class Program
{
    static void Main()
    {
        Circle Krug = new Circle();
        Console.Write("Введите радиус: ");
        Krug.Radius = Convert.ToDouble(Console.ReadLine());
        Console.WriteLine($"Площадь круга = {Krug.Area()}");
        Console.WriteLine($"Периметр круга = {Krug.Perimetr()}");

        Rectangle Pryamo = new Rectangle();
        Console.Write("Введите сторону А: ");
        Pryamo.Height = Convert.ToDouble(Console.ReadLine());
        Console.Write("Введите сторону Б: ");
        Pryamo.Width = Convert.ToDouble(Console.ReadLine());
        Console.WriteLine($"Площадь прямоугольника = {Pryamo.Area()}");
        Console.WriteLine($"Периметр прямоугольника = {Pryamo.Perimetr()}");

    }
}