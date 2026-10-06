struct Student
{
    public string Name;        
    public string Grp; 
    public int[] Otsenki;      


    public double sred()
    {
        double sum = 0;
        foreach (int grade in Otsenki)
        {
            sum += grade;
        }
        return sum / Otsenki.Length;
    }
 
    public bool OnlyG()
    {
        foreach (int grade in Otsenki)
        {
  
            if (grade != 4 && grade != 5)
            {
                return false;
            }
        }
        return true;
    }
}

class Program
{
    static void Main()
    {

        Student[] students = new Student[10];
        students[0] = new Student { Name = "Новиков Г.А.", Grp = "101-А", Otsenki = new int[] { 5, 4, 5, 4, 5 } };
        students[1] = new Student { Name = "Балдин А.А.", Grp = "101-А", Otsenki = new int[] { 3, 4, 3, 5, 4 } };
        students[2] = new Student { Name = "Кухоренко К.А.", Grp = "102-Б", Otsenki = new int[] { 5, 5, 5, 5, 5 } };
        students[3] = new Student { Name = "Ивченко А.Д.", Grp = "102-Б", Otsenki = new int[] { 5, 3, 3, 5, 4 } };
        students[4] = new Student { Name = "Козлов А.Д.", Grp = "103-В", Otsenki = new int[] { 1, 3, 1, 1, 2 } };
        students[5] = new Student { Name = "Бобко Д.В.", Grp = "101-А", Otsenki = new int[] { 2, 4, 4, 5, 1 } };
        students[6] = new Student { Name = "Попов М.С.", Grp = "101-А", Otsenki = new int[] { 1, 1, 4, 2, 3 } };
        students[7] = new Student { Name = "Соколов Е.А.", Grp = "102-Б", Otsenki = new int[] { 4, 5, 5, 3, 4 } };
        students[8] = new Student { Name = "Михайлов К.Л.", Grp = "103-В", Otsenki = new int[] { 2, 2, 3, 3, 4 } };
        students[9] = new Student { Name = "Шаров И.М.", Grp = "101-А", Otsenki = new int[] { 5, 5, 4, 5, 5 } };

        students = students.OrderBy(s => s.sred()).ToArray();

        Console.WriteLine("СТУДЕНТЫ");
        foreach (var student in students)
        {
            Console.WriteLine($"{student.Name} | Группа: {student.Grp} | Средний балл: {student.sred():F2}");
        }

        Console.WriteLine("\nСТУДЕНТЫ С ОЦЕНКАМИ ТОЛЬКО 4 И 5");
        bool found = false;

        foreach (var student in students)
        {
            if (student.OnlyG())
            {
                Console.WriteLine($"Фамилия: {student.Name}, Группа: {student.Grp}");
                found = true;
            }
        }

        if (!found)
        {
            Console.WriteLine("не найдено.");
        }
    }
}