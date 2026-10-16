using System;

interface IStudent
{
    double Avg();
    string Course();
}

class First : IStudent
{
    double[] grades;

    public First(double[] grades)
    {
        this.grades = grades;
    }

    public double Avg()
    {
        double sum = 0;

        foreach (double grade in grades)
            sum += grade;

        return sum / grades.Length;
    }

    public string Course()
    {
        return "1 курс";
    }
}

class Second : IStudent
{
    double[] grades;

    public Second(double[] grades)
    {
        this.grades = grades;
    }

    public double Avg()
    {
        double sum = 0;

        foreach (double grade in grades)
            sum += grade;

        return sum / grades.Length;
    }

    public string Course()
    {
        return "2 курс";
    }
}

class Third : IStudent
{
    double[] grades;

    public Third(double[] grades)
    {
        this.grades = grades;
    }

    public double Avg()
    {
        double sum = 0;

        foreach (double grade in grades)
            sum += grade;

        return sum / grades.Length;
    }

    public string Course()
    {
        return "3 курс";
    }
}

class Program
{
    static void Main()
    {
        static void ShowStudent(string name, IStudent student)
        {
            Console.WriteLine(
                $"{name}: {student.Course()}, средний балл: {student.Avg():F2}");
        }

        IStudent student1 = new First(new double[] { 8, 9, 10 });
        IStudent student2 = new Second(new double[] { 7, 8, 9 });
        IStudent student3 = new Third(new double[] { 10, 9, 10 });

        ShowStudent("Иван", student1);
        ShowStudent("Алексей", student2);
        ShowStudent("Максим", student3);
    }

    
}