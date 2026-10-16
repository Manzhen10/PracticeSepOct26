interface IBook
{
    bool Dostup();
    void Issue();
}

class Novel : IBook
{
    bool dostupen;

    public Novel(bool dostupen)
    {
        this.dostupen = dostupen;
    }

    public bool Dostup()
    {
        return dostupen;
    }

    public void Issue()
    {
        if (dostupen)
        {
            Console.WriteLine("Книга свободна.");
            dostupen = false;
        }
        else
        {
            Console.WriteLine("Книга уже выдана.");
        }
    }
}

// Учебник
class Uchebnik : IBook
{
    bool dostupen;

    public Uchebnik(bool dostupen)
    {
        this.dostupen = dostupen;
    }

    public bool Dostup()
    {
        return dostupen;
    }

    public void Issue()
    {
        if (dostupen)
        {
            Console.WriteLine("Учебник свободен.");
            dostupen = false;
        }
        else
        {
            Console.WriteLine("Учебник уже выдан.");
        }
    }
}

class Program
{
    static void Main()
    {
        IBook book1 = new Novel(true);
        IBook book2 = new Uchebnik(false);
        Console.WriteLine("Художественная книга доступна: " + book1.Dostup());
        book1.Issue();
        Console.WriteLine("Учебник доступен: " + book2.Dostup());
        book2.Issue();
    }
}