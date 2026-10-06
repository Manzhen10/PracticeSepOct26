interface IDrawable
{
    public void Draw();
}

class Circle : IDrawable
{
    public void Draw()
    {
        Console.WriteLine("КРУГ: без углов, круглый, без сторон");
    }
}

class Rectangle : IDrawable
{
    public void Draw()
    {
        Console.WriteLine("ПРЯМОУГОЛЬНИК: 4 стороны, стороны попарно равны, 4 угла 90 градусов");
    }
}

class Triangle : IDrawable
{
    public void Draw()
    {
        Console.WriteLine("ТРЕУГОЛЬНИК: три угла, в сумме дают 180 градусов, три стороны");
    }
}

class Program
{
    static void Main()
    {
        IDrawable[] Figs = new IDrawable[] {
            new Circle(),
            new Rectangle(),
            new Triangle(),
        };
        for (int i = 0; i < Figs.Length; i++)
        {
            Figs[i].Draw();
        }
    }
}