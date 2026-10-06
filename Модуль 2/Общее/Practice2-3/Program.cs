class Author
{
    public string Name;   
    public int Year; 
}

class Book
{
    public string Tit;
    public Author BAut;
    public int Release;
    
    public void Vivod()
    {
        Console.WriteLine($"Книга: {Tit} ({Release}г.)");
        Console.WriteLine($"Автор: {BAut.Name} ({BAut.Year}г.)");
    }
}

class Program
{
    static void Main()
    {
        Author author1 = new Author();
        author1.Name = "Лев Толстой";
        author1.Year = 1828;

        Author author2 = new Author();
        author2.Name = "Антон Чехов";
        author2.Year = 1860;

        Book book1 = new Book();
        book1.Tit = "Война и мир";
        book1.Release = 1869;
        book1.BAut = author1;

        Book book2 = new Book();
        book2.Tit = "Нечистые трагики и прокажённые драматурги";
        book2.Release = 1884;
        book2.BAut = author2;

        book1.Vivod();
        book2.Vivod();
        
    }
}