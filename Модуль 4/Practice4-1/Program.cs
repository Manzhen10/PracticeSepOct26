interface IFigure
{
    double S();
    double P();
}

//Круг
class Circle : IFigure
{
    double r;

    public Circle(double r)
    {
        this.r = r;
    }

    public double S()
    {
        return Math.PI * Math.Pow(r, 2);
    }

    public double P()
    {
        return 2 * Math.PI * r;
    }
}

//Прямоугольник
class Rectangle : IFigure
{
    double w;
    double h;

    public Rectangle(double w, double h)
    {
        this.w = w;
        this.h = h;
    }

    public double S()
    {
        return w * h;
    }

    public double P()
    {
        return 2 * (w + h);
    }
}

//Треугольник
class Triangle : IFigure
{
    double a;
    double b;
    double c;

    public Triangle(double a, double b, double c)
    {
        this.a = a;
        this.b = b;
        this.c = c;
    }

    public double S()
    {
        double p = (a + b + c) / 2;
        return Math.Sqrt(p * (p - a) * (p - b) * (p - c));
    }

    public double P()
    {
        return a + b + c;
    }
}

class Program
{
    static void Main()
    {
        IFigure circle = new Circle(5);
        IFigure rectangle = new Rectangle(4, 6);
        IFigure triangle = new Triangle(3, 4, 5);
        Console.WriteLine($"Площадь круга: {circle.S():F2}");
        Console.WriteLine($"Периметр круга: {circle.P():F2}");
        Console.WriteLine($"Площадь прямоугольника: {rectangle.S():F2}");
        Console.WriteLine($"Периметр прямоугольника: {rectangle.P():F2}");
        Console.WriteLine($"Площадь треугольника: {triangle.S():F2}");
        Console.WriteLine($"Периметр треугольника: {triangle.P():F2}");
    }
}