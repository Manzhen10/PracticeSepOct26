using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Data.Sqlite;

namespace StudentManager;

public interface IStudentRepository
{
    List<Student> GetAll();
    void Add(Student student);
    void Update(Student student);
    void Delete(int id);
    void ReplaceAll(List<Student> students);
}

public class StudentRepository : IStudentRepository
{
    private readonly string _connectionString;

    public StudentRepository(string databasePath)
    {
        if (string.IsNullOrWhiteSpace(databasePath))
        {
            throw new ArgumentException(
                "Путь к базе данных не указан.",
                nameof(databasePath));
        }

        string fullPath = Path.GetFullPath(databasePath);
        string? directory = Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = fullPath
        };

        _connectionString = builder.ToString();

        InitializeDatabase();
    }

    private SqliteConnection CreateConnection()
    {
        return new SqliteConnection(_connectionString);
    }

    // Создание таблицы 

    private void InitializeDatabase()
    {
        using var connection = CreateConnection();
        connection.Open();

        using (var command = connection.CreateCommand())
        {
            command.CommandText = """
                CREATE TABLE IF NOT EXISTS Students
                (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    FirstName TEXT NOT NULL,
                    LastName TEXT NOT NULL,
                    BirthDate TEXT NOT NULL,
                    GroupName TEXT NOT NULL,
                    AverageGrade REAL NOT NULL
                        CHECK (AverageGrade >= 0 AND AverageGrade <= 10)
                );
                """;

            command.ExecuteNonQuery();
        }

        MigrateGradeConstraint(connection);
    }

    private static void MigrateGradeConstraint(
        SqliteConnection connection)
    {
        string schema;

        using (var command = connection.CreateCommand())
        {
            command.CommandText = """
                SELECT sql
                FROM sqlite_master
                WHERE type = 'table' AND name = 'Students';
                """;

            schema = command.ExecuteScalar()?.ToString() ?? "";
        }

        string normalized = NormalizeSql(schema);

        bool hasGradeCheck =
            normalized.Contains("check(") &&
            normalized.Contains("averagegrade");

        bool hasCorrectGradeRange =
            normalized.Contains("averagegrade>=0") &&
            normalized.Contains("averagegrade<=10");
        if (!hasGradeCheck || hasCorrectGradeRange)
        {
            return;
        }

        using var transaction = connection.BeginTransaction();

        try
        {
            ExecuteNonQuery(
                connection,
                transaction,
                "DROP TABLE IF EXISTS Students_New;");

            ExecuteNonQuery(
                connection,
                transaction,
                """
                CREATE TABLE Students_New
                (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    FirstName TEXT NOT NULL,
                    LastName TEXT NOT NULL,
                    BirthDate TEXT NOT NULL,
                    GroupName TEXT NOT NULL,
                    AverageGrade REAL NOT NULL
                        CHECK (AverageGrade >= 0 AND AverageGrade <= 10)
                );
                """);

            ExecuteNonQuery(
                connection,
                transaction,
                """
                INSERT INTO Students_New
                    (Id, FirstName, LastName, BirthDate, GroupName, AverageGrade)
                SELECT
                    Id, FirstName, LastName, BirthDate, GroupName, AverageGrade
                FROM Students;
                """);

            ExecuteNonQuery(
                connection,
                transaction,
                "DROP TABLE Students;");

            ExecuteNonQuery(
                connection,
                transaction,
                "ALTER TABLE Students_New RENAME TO Students;");

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private static string NormalizeSql(string sql)
    {
        return new string(
            sql.Where(c =>
                !char.IsWhiteSpace(c) &&
                c != '"' &&
                c != '`' &&
                c != '[' &&
                c != ']')
            .ToArray())
            .ToLowerInvariant();
    }

    private static void ExecuteNonQuery(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string sql)
    {
        using var command = connection.CreateCommand();

        command.Transaction = transaction;
        command.CommandText = sql;

        command.ExecuteNonQuery();
    }

    // Получение всех студентов из базы

    public List<Student> GetAll()
    {
        var students = new List<Student>();

        using var connection = CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT
                Id,
                FirstName,
                LastName,
                BirthDate,
                GroupName,
                AverageGrade
            FROM Students
            ORDER BY Id;
            """;

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            students.Add(new Student
            {
                Id = reader.GetInt32(0),
                FirstName = reader.GetString(1),
                LastName = reader.GetString(2),
                BirthDate = DateTime.Parse(
                    reader.GetString(3),
                    CultureInfo.InvariantCulture),
                GroupName = reader.GetString(4),
                AverageGrade = reader.GetDouble(5)
            });
        }

        return students;
    }

    // Добавление студента

    public void Add(Student student)
    {
        ValidateStudent(student);

        using var connection = CreateConnection();
        connection.Open();

        InsertStudent(connection, null, student, false);
    }

    // Редактирование студента

    public void Update(Student student)
    {
        ValidateStudent(student);

        if (student.Id <= 0)
        {
            throw new ArgumentException(
                "ID студента должен быть положительным.");
        }

        using var connection = CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = """
            UPDATE Students
            SET
                FirstName = $firstName,
                LastName = $lastName,
                BirthDate = $birthDate,
                GroupName = $groupName,
                AverageGrade = $averageGrade
            WHERE Id = $id;
            """;

        AddStudentParameters(command, student);
        command.Parameters.AddWithValue("$id", student.Id);

        int rowsAffected = command.ExecuteNonQuery();

        if (rowsAffected == 0)
        {
            throw new InvalidOperationException(
                "Студент с таким ID не найден.");
        }
    }

    // Удаление студента

    public void Delete(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException(
                "ID студента должен быть положительным.");
        }

        using var connection = CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = """
            DELETE FROM Students
            WHERE Id = $id;
            """;

        command.Parameters.AddWithValue("$id", id);

        int rowsAffected = command.ExecuteNonQuery();

        if (rowsAffected == 0)
        {
            throw new InvalidOperationException(
                "Студент с таким ID не найден.");
        }
    }


    public void ReplaceAll(List<Student> students)
    {
        ArgumentNullException.ThrowIfNull(students);

        foreach (Student student in students)
        {
            ValidateStudent(student);
        }

        using var connection = CreateConnection();
        connection.Open();

        using var transaction = connection.BeginTransaction();

        try
        {
            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = "DELETE FROM Students;";
                command.ExecuteNonQuery();
            }

            foreach (Student student in students)
            {
                InsertStudent(
                    connection,
                    transaction,
                    student,
                    student.Id > 0);
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    // Запись одного студента

    private static void InsertStudent(
        SqliteConnection connection,
        SqliteTransaction? transaction,
        Student student,
        bool includeId)
    {
        using var command = connection.CreateCommand();

        command.Transaction = transaction;

        if (includeId)
        {
            command.CommandText = """
                INSERT INTO Students
                    (Id, FirstName, LastName, BirthDate, GroupName, AverageGrade)
                VALUES
                    ($id, $firstName, $lastName, $birthDate, $groupName, $averageGrade);
                """;

            command.Parameters.AddWithValue("$id", student.Id);
        }
        else
        {
            command.CommandText = """
                INSERT INTO Students
                    (FirstName, LastName, BirthDate, GroupName, AverageGrade)
                VALUES
                    ($firstName, $lastName, $birthDate, $groupName, $averageGrade);
                """;
        }

        AddStudentParameters(command, student);

        command.ExecuteNonQuery();
    }

    // Параметры SQL-запроса

    private static void AddStudentParameters(
        SqliteCommand command,
        Student student)
    {
        command.Parameters.AddWithValue(
            "$firstName",
            student.FirstName);

        command.Parameters.AddWithValue(
            "$lastName",
            student.LastName);

        command.Parameters.AddWithValue(
            "$birthDate",
            student.BirthDate.ToString(
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture));

        command.Parameters.AddWithValue(
            "$groupName",
            student.GroupName);

        command.Parameters.AddWithValue(
            "$averageGrade",
            student.AverageGrade);
    }

    // Проверка данных перед сохранением

    private static void ValidateStudent(Student student)
    {
        ArgumentNullException.ThrowIfNull(student);

        if (string.IsNullOrWhiteSpace(student.FirstName))
        {
            throw new ArgumentException("Имя студента не может быть пустым.");
        }

        if (string.IsNullOrWhiteSpace(student.LastName))
        {
            throw new ArgumentException("Фамилия студента не может быть пустой.");
        }

        if (string.IsNullOrWhiteSpace(student.GroupName))
        {
            throw new ArgumentException("Группа студента не может быть пустой.");
        }

        if (student.BirthDate.Date > DateTime.Today)
        {
            throw new ArgumentException(
                "Дата рождения не может быть в будущем.");
        }

        if (!double.IsFinite(student.AverageGrade) ||
            student.AverageGrade < 0 ||
            student.AverageGrade > 10)
        {
            throw new ArgumentOutOfRangeException(
                nameof(student.AverageGrade),
                student.AverageGrade,
                "Средний балл должен быть от 0 до 10.");
        }
    }
}