interface IProduct
{
    double Cost();
    int sht();
}

// Яблоки
class Apple : IProduct
{
    double price;
    int shtuk;

    public Apple(double price, int shtuk)
    {
        this.price = price;
        this.shtuk = shtuk;
    }

    public double Cost()
    {
        return price * shtuk;
    }

    public int sht()
    {
        return shtuk;
    }
}

// Молоко
class Milk : IProduct
{
    double price;
    int shtuk;

    public Milk(double price, int shtuk)
    {
        this.price = price;
        this.shtuk = shtuk;
    }

    public double Cost()
    {
        return price * shtuk;
    }

    public int sht()
    {
        return shtuk;
    }
}

// Хлеб
class Bread : IProduct
{
    double price;
    int shtuk;

    public Bread(double price, int shtuk)
    {
        this.price = price;
        this.shtuk = shtuk;
    }

    public double Cost()
    {
        return price * shtuk;
    }

    public int sht()
    {
        return shtuk;
    }
}


class Program
{
    static void Main()
    {
        static void ShowProduct(string name, IProduct product)
        {
            Console.WriteLine($"{name}: остаток {product.sht()} шт., стоимость {product.Cost():F2} белорусских зайчиков");
        }

        IProduct apple = new Apple(2.5, 20);
        IProduct milk = new Milk(1.2, 15);
        IProduct bread = new Bread(1.5, 10);

        ShowProduct("Яблоки", apple);
        ShowProduct("Молоко", milk);
        ShowProduct("Хлеб", bread);
    }

    
}