abstract class Shape
{
    public abstract double S();

    public void Vivod()
    {
        Console.WriteLine($"Площадь фигуры = {S()/*:F2*/}");
    }
}

class Triangle : Shape
{
    public double Side;
    public double H;

    public override double S()
    {
        return 0.5 * Side * H;
    }
}

class Rectangle : Shape
{
    public double StoronaA;
    public double StoronaB;

    public override double S()
    {
        return StoronaA * StoronaB;
    }
}

class Circle : Shape
{
    public double Radius;

    public override double S()
    {
        return Math.PI * Radius * Radius;
    }
}

class Program
{
    static void Main()
    {
        Triangle treug = new Triangle();
        Console.Write("Введите сторону треугольника: ");
        treug.Side = Convert.ToDouble(Console.ReadLine());
        Console.Write("Введите высоту треугольника: ");
        treug.H = Convert.ToDouble(Console.ReadLine());
        treug.Vivod();
        Rectangle pryamo = new();
        Console.Write("Введите сторону А прямоугольника: ");
        pryamo.StoronaA = Convert.ToDouble(Console.ReadLine());
        Console.Write("Введите сторону Б прямоугольника: ");
        pryamo.StoronaB = Convert.ToDouble(Console.ReadLine());
        pryamo.Vivod();
        Circle circle = new Circle();
        Console.Write("Введите радиус круга: ");
        circle.Radius = Convert.ToDouble(Console.ReadLine());
        circle.Vivod();
    }
}