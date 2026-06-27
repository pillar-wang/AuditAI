using System;
using System.Data.SQLite;
using System.IO;

class Program
{
    static void Main()
    {
        var dbPaths = new[]
        {
            @"e:\lq\Data\1\00000000-0000-0000-0000-000000000000.db",
            @"e:\lq\Data\leqi_audit.db",
            @"e:\lq\LeqiAudit\data\23855\f8862957-a5b2-4c54-b1d3-4be08905a388.db",
            @"e:\lq\LeqiAudit\演示账套\ABC有限公司_2023.db"
        };

        foreach (var dbPath in dbPaths)
        {
            if (!File.Exists(dbPath))
            {
                Console.WriteLine($"NOT FOUND: {dbPath}");
                continue;
            }

            Console.WriteLine($"\n=== {dbPath} ===");
            var connStr = $"Data Source={dbPath};Version=3;";
            using var conn = new SQLiteConnection(connStr);
            conn.Open();

            using var cmd = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type='table' ORDER BY name", conn);
            using var reader = cmd.ExecuteReader();
            var tables = new System.Collections.Generic.List<string>();
            while (reader.Read())
            {
                tables.Add(reader.GetString(0));
            }
            Console.WriteLine($"Tables: {string.Join(", ", tables)}");

            if (tables.Contains("Table"))
            {
                using var cmd2 = new SQLiteCommand("SELECT Id, Title, length(Ticket) as TicketLen FROM `Table` WHERE Title LIKE '%应收账款%' OR Title LIKE '%测试表%' LIMIT 10", conn);
                using var reader2 = cmd2.ExecuteReader();
                while (reader2.Read())
                {
                    Console.WriteLine($"  Id={reader2.GetInt64(0)}, Title={reader2.GetString(1)}, TicketLen={reader2.GetInt64(2)}");
                }
            }
        }
    }
}
