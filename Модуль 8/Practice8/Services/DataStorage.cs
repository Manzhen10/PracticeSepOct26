using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.IO;

namespace Practice8
{
    public static class DataStorage
    {
        private static readonly string FolderPath =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "Practice8");

        private static readonly string DatabasePath =
            Path.Combine(FolderPath, "practice8.db");

        private static readonly string ConnectionString =
            "Data Source=" + DatabasePath + ";Version=3;";

        // Создание файла бд 
        public static void Initialize()
        {
            Directory.CreateDirectory(FolderPath);

            if (!File.Exists(DatabasePath))
            {
                SQLiteConnection.CreateFile(DatabasePath);
            }

            using (SQLiteConnection connection =
                new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                string sql = @"
                    CREATE TABLE IF NOT EXISTS PlannerItems
                    (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Title TEXT NOT NULL,
                        Description TEXT,
                        Category TEXT NOT NULL,
                        Deadline TEXT NOT NULL,
                        IsCompleted INTEGER NOT NULL DEFAULT 0,
                        HasReminder INTEGER NOT NULL DEFAULT 1,
                        ReminderMinutesBefore INTEGER NOT NULL DEFAULT 10,
                        ReminderShown INTEGER NOT NULL DEFAULT 0
                    );";

                using (SQLiteCommand command =
                    new SQLiteCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        // Загрузка списка из базы
        public static List<PlannerItem> Load()
        {
            List<PlannerItem> items = new List<PlannerItem>();

            using (SQLiteConnection connection =
                new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                string sql = @"
                    SELECT Title, Description, Category, Deadline,
                           IsCompleted, HasReminder,
                           ReminderMinutesBefore, ReminderShown
                    FROM PlannerItems
                    ORDER BY Deadline;";

                using (SQLiteCommand command =
                    new SQLiteCommand(sql, connection))
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        PlannerItem item = new PlannerItem();

                        item.Title = reader["Title"].ToString();

                        item.Description =
                            reader["Description"].ToString();

                        item.Category =
                            reader["Category"].ToString();

                        item.Deadline = DateTime.ParseExact(
                            reader["Deadline"].ToString(),
                            "yyyy-MM-dd HH:mm:ss",
                            CultureInfo.InvariantCulture);

                        item.IsCompleted =
                            Convert.ToInt32(reader["IsCompleted"]) == 1;

                        item.HasReminder =
                            Convert.ToInt32(reader["HasReminder"]) == 1;

                        item.ReminderMinutesBefore =
                            Convert.ToInt32(
                                reader["ReminderMinutesBefore"]);

                        item.ReminderShown =
                            Convert.ToInt32(reader["ReminderShown"]) == 1;

                        items.Add(item);
                    }
                }
            }

            return items;
        }

        // Сохранение списка в базу
        public static void Save(IEnumerable<PlannerItem> items)
        {
            using (SQLiteConnection connection =
                new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                using (SQLiteTransaction transaction =
                    connection.BeginTransaction())
                {
                    try
                    {

                        using (SQLiteCommand deleteCommand =
                            new SQLiteCommand(
                                "DELETE FROM PlannerItems;",
                                connection,
                                transaction))
                        {
                            deleteCommand.ExecuteNonQuery();
                        }

                        string sql = @"
                            INSERT INTO PlannerItems
                            (
                                Title,
                                Description,
                                Category,
                                Deadline,
                                IsCompleted,
                                HasReminder,
                                ReminderMinutesBefore,
                                ReminderShown
                            )
                            VALUES
                            (
                                @Title,
                                @Description,
                                @Category,
                                @Deadline,
                                @IsCompleted,
                                @HasReminder,
                                @ReminderMinutesBefore,
                                @ReminderShown
                            );";

                        foreach (PlannerItem item in items)
                        {
                            using (SQLiteCommand command =
                                new SQLiteCommand(
                                    sql, connection, transaction))
                            {
                                command.Parameters.AddWithValue(
                                    "@Title", item.Title ?? "");

                                command.Parameters.AddWithValue(
                                    "@Description",
                                    item.Description ?? "");

                                command.Parameters.AddWithValue(
                                    "@Category",
                                    item.Category ?? "Задача");

                                command.Parameters.AddWithValue(
                                    "@Deadline",
                                    item.Deadline.ToString(
                                        "yyyy-MM-dd HH:mm:ss",
                                        CultureInfo.InvariantCulture));

                                command.Parameters.AddWithValue(
                                    "@IsCompleted",
                                    item.IsCompleted ? 1 : 0);

                                command.Parameters.AddWithValue(
                                    "@HasReminder",
                                    item.HasReminder ? 1 : 0);

                                command.Parameters.AddWithValue(
                                    "@ReminderMinutesBefore",
                                    item.ReminderMinutesBefore);

                                command.Parameters.AddWithValue(
                                    "@ReminderShown",
                                    item.ReminderShown ? 1 : 0);

                                command.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}