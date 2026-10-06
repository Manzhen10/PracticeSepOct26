class Automobile
{
    public string marka;
    public string model;
    public int gv;
    public double price;
    public double nalog;
    public double skidka;

    public void raschet()
    {
        nalog = price * 0.05;
        double newprice = price - ((skidka/100)*price) + nalog;
        //Console.WriteLine($"Марка: {marka}");
        //Console.WriteLine($"Модель: {model}");
        //Console.WriteLine($"Год выпуска: {gv}");
        Console.WriteLine($"Цена за {marka} {model} ({gv} года выпуска) = {newprice}BYN, при скидке в {skidka}% ({(skidka / 100) * price}BYN), налоге в 5%({nalog}BYN) и изначальной цене {price}BYN");
    }
}

class Programm
{
    static void Main()
    {
        Automobile Mercedes = new Automobile();
        Mercedes.marka = "Mercedes-Benz";
        Mercedes.model = "GLS X166";
        Mercedes.gv = 2016;
        Mercedes.price = 90319;
        Console.Write("Введите скидку в процентах: ");
        Mercedes.skidka = Convert.ToDouble(Console.ReadLine());
        Mercedes.raschet();

        Automobile Audi = new Automobile();
        Audi.marka = "Audi";
        Audi.model = "Q7";
        Audi.gv = 2021;
        Audi.price = 119318;
        Audi.skidka = Mercedes.skidka;
        Audi.raschet();

    }
}