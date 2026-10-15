using System.ComponentModel;

delegate double AreaDelegate();

abstract class Figure
{
    public abstract double S();
}

class Circle : Figure
{
    private double r;

    public Circle(double radius)
    {
        this.r = radius;
    }

    public override double S()
    {
        return Math.PI * Math.Pow(r, 2);
    }
}

class Rectangle : Figure
{
    private double width;
    private double height;

    public Rectangle(double width, double height)
    {
        this.width = width;
        this.height = height;
    }

    public override double S()
    {
        return width * height;
    }
}

class Triangle : Figure
{
    private double w;
    private double height;

    public Triangle(double w, double height)
    {
        this.w = w;
        this.height = height;
    }

    public override double S()
    {
        return w * height / 2;
    }
}

class Program
{
    static void Main()
    {
        AreaDelegate area;
        Figure figure = new Circle(5);
        area = figure.S;
        Console.WriteLine($"Площадь круга: {area()}");
        figure = new Rectangle(4, 6);
        area = figure.S;
        Console.WriteLine($"Площадь прямоугольника: {area()}");
        figure = new Triangle(8, 5);
        area = figure.S;
        Console.WriteLine($"Площадь треугольника: {area()}");
    }
}
