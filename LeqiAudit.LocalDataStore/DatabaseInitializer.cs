﻿﻿﻿﻿﻿﻿using System.Collections.Generic;
using System.Data.SQLite;

namespace Leqisoft.LocalDataStore
{
    public static class DatabaseInitializer
    {
        public static void Initialize(string connectionString)
        {
            using var conn = new SQLiteConnection(connectionString);
            conn.Open();

            // 执行建表脚本
            string[] createTableStatements = {
                @"CREATE TABLE IF NOT EXISTS SchemaVersion (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Version INTEGER NOT NULL,
                    AppliedAt TEXT NOT NULL DEFAULT (datetime('now'))
                )",

                @"CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserName TEXT NOT NULL UNIQUE,
                    Name TEXT, Password TEXT, Phone TEXT, TelPhone TEXT,
                    Email TEXT, Company TEXT, Sex TEXT, City TEXT,
                    TeamId TEXT, IsTeamAdmin INTEGER DEFAULT 0,
                    GroupId INTEGER, JobTitle TEXT,
                    Permissions TEXT,
                    CreatedAt TEXT DEFAULT (datetime('now')),
                    UpdatedAt TEXT, IsDeleted INTEGER DEFAULT 0
                )",

                @"CREATE TABLE IF NOT EXISTS Teams (
                    Id TEXT PRIMARY KEY,
                    Name TEXT NOT NULL, Type INTEGER DEFAULT 0,
                    CreatorId INTEGER NOT NULL, LicenseDate TEXT DEFAULT (datetime('now', '+10 years')),
                    CreatedAt TEXT DEFAULT (datetime('now')),
                    UpdatedAt TEXT, IsDeleted INTEGER DEFAULT 0
                )",

                @"CREATE TABLE IF NOT EXISTS Projects (
                    Id TEXT PRIMARY KEY, Name TEXT NOT NULL,
                    Type INTEGER DEFAULT 0, Version INTEGER DEFAULT 1,
                    TeamId TEXT, CreatedBy INTEGER NOT NULL,
                    CreatedAt TEXT DEFAULT (datetime('now')), UpdatedAt TEXT,
                    IsDeleted INTEGER DEFAULT 0, DeletedAt TEXT,
                    IsTemplate INTEGER DEFAULT 0, IsDemo INTEGER DEFAULT 0,
                    Description TEXT, OperationId INTEGER DEFAULT 0
                )",

                @"CREATE TABLE IF NOT EXISTS ProjectMembers (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ProjectId TEXT NOT NULL, UserId INTEGER NOT NULL,
                    Role INTEGER DEFAULT 0, JoinedAt TEXT DEFAULT (datetime('now')),
                    UNIQUE(ProjectId, UserId)
                )",

                @"CREATE TABLE IF NOT EXISTS TableSchemas (
                    Id TEXT NOT NULL, ProjectId TEXT NOT NULL,
                    OperationId INTEGER DEFAULT 0, Name TEXT,
                    Version INTEGER DEFAULT 1, Mask INTEGER DEFAULT 0,
                    ColumnsData BLOB, RowsData BLOB, CellsData BLOB,
                    CellStylesData BLOB, MergesData BLOB, CellPropsData BLOB,
                    UpdatedAt TEXT DEFAULT (datetime('now')), UpdatedBy INTEGER,
                    PRIMARY KEY (Id, ProjectId)
                )",

                @"CREATE TABLE IF NOT EXISTS Documents (
                    Id TEXT NOT NULL, ProjectId TEXT NOT NULL,
                    Name TEXT, Version INTEGER DEFAULT 1,
                    OperationId INTEGER DEFAULT 0, ParagraphsData BLOB,
                    UpdatedAt TEXT DEFAULT (datetime('now')), UpdatedBy INTEGER,
                    PRIMARY KEY (Id, ProjectId)
                )",

                @"CREATE TABLE IF NOT EXISTS DataDictionary (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    DicType TEXT NOT NULL, Version INTEGER NOT NULL,
                    Data TEXT NOT NULL, UpdatedAt TEXT DEFAULT (datetime('now'))
                )",

                @"CREATE TABLE IF NOT EXISTS ProjectFiles (
                    Id TEXT PRIMARY KEY, ProjectId TEXT NOT NULL,
                    FileName TEXT NOT NULL, FileSize INTEGER DEFAULT 0,
                    ContentType TEXT, LocalPath TEXT NOT NULL,
                    UploadedBy INTEGER NOT NULL,
                    UploadedAt TEXT DEFAULT (datetime('now'))
                )",

                @"CREATE TABLE IF NOT EXISTS VersionHistory (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ProjectId TEXT NOT NULL, TargetId TEXT NOT NULL,
                    TargetType TEXT NOT NULL, Version INTEGER NOT NULL,
                    ChangeType TEXT, Snapshot BLOB,
                    CreatedAt TEXT DEFAULT (datetime('now'))
                )"
            };

            foreach (var sql in createTableStatements)
            {
                new SQLiteCommand(sql, conn).ExecuteNonQuery();
            }

            // 创建索引
            string[] indexStatements = {
                "CREATE INDEX IF NOT EXISTS idx_tables_project ON TableSchemas(ProjectId)",
                "CREATE INDEX IF NOT EXISTS idx_docs_project ON Documents(ProjectId)",
                "CREATE INDEX IF NOT EXISTS idx_files_project ON ProjectFiles(ProjectId)",
                "CREATE INDEX IF NOT EXISTS idx_history_project ON VersionHistory(ProjectId)",
                "CREATE INDEX IF NOT EXISTS idx_history_target ON VersionHistory(TargetId, TargetType)"
            };

            foreach (var sql in indexStatements)
            {
                new SQLiteCommand(sql, conn).ExecuteNonQuery();
            }

            // 增量添加列（仅当列不存在时）
            AddColumnsIfNotExists(conn, "Projects", new Dictionary<string, string> {
                { "Number", "TEXT DEFAULT ''" },
                { "Category", "TEXT DEFAULT ''" },
                { "Auditee", "TEXT DEFAULT ''" },
                { "Note", "TEXT DEFAULT ''" },
                { "ParentId", "TEXT DEFAULT NULL" }
            });

            // Users 表增量列：头像图片（BLOB）
            AddColumnsIfNotExists(conn, "Users", new Dictionary<string, string> {
                { "Picture", "BLOB" }
            });
        }

        private static void AddColumnsIfNotExists(SQLiteConnection conn, string tableName, Dictionary<string, string> columns)
        {
            // 查询表现有的列
            var existingColumns = new HashSet<string>();
            using (var cmd = new SQLiteCommand($"PRAGMA table_info({tableName})", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    existingColumns.Add(reader["name"].ToString());
                }
            }

            // 只添加不存在的列
            foreach (var kv in columns)
            {
                if (!existingColumns.Contains(kv.Key))
                {
                    new SQLiteCommand($"ALTER TABLE {tableName} ADD COLUMN {kv.Key} {kv.Value}", conn).ExecuteNonQuery();
                }
            }
        }
    }
}
