﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;

namespace Auditai.Model;

public class CrossProjectRefVersionTracker
{
    private readonly string _dbPath;

    public CrossProjectRefVersionTracker(string dbPath)
    {
        _dbPath = dbPath;
        EnsureTable();
    }

    private void EnsureTable()
    {
        using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
        conn.Open();
        using var cmd = new SQLiteCommand(@"
            CREATE TABLE IF NOT EXISTS CrossProjectRefVersion (
                ProjectId TEXT NOT NULL,
                TableId INTEGER NOT NULL,
                CurrentVersion INTEGER NOT NULL DEFAULT 1,
                LastModifiedAt TEXT NOT NULL,
                PRIMARY KEY (ProjectId, TableId)
            )", conn);
        cmd.ExecuteNonQuery();
    }

    public int GetCurrentVersion(string projectId, long tableId)
    {
        using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
        conn.Open();
        using var cmd = new SQLiteCommand(
            "SELECT CurrentVersion FROM CrossProjectRefVersion WHERE ProjectId = @ProjectId AND TableId = @TableId", conn);
        cmd.Parameters.AddWithValue("@ProjectId", projectId);
        cmd.Parameters.AddWithValue("@TableId", tableId);
        var val = cmd.ExecuteScalar();
        return val != null ? Convert.ToInt32(val) : 1;
    }

    public void IncrementVersion(string projectId, long tableId)
    {
        using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
        conn.Open();
        using var cmd = new SQLiteCommand(@"
            INSERT OR REPLACE INTO CrossProjectRefVersion (ProjectId, TableId, CurrentVersion, LastModifiedAt)
            VALUES (@ProjectId, @TableId, 
                COALESCE((SELECT CurrentVersion + 1 FROM CrossProjectRefVersion WHERE ProjectId = @ProjectId AND TableId = @TableId), 1),
                @LastModifiedAt)", conn);
        cmd.Parameters.AddWithValue("@ProjectId", projectId);
        cmd.Parameters.AddWithValue("@TableId", tableId);
        cmd.Parameters.AddWithValue("@LastModifiedAt", DateTime.Now.ToString("o"));
        cmd.ExecuteNonQuery();
    }

    public string GetVersionDiff(string projectId, long tableId, int fromVersion, int toVersion)
    {
        var current = GetCurrentVersion(projectId, tableId);
        if (fromVersion >= current) return "无差异（已是最新）";
        return $"来源数据已从版本{fromVersion}更新至版本{current}（差异{current - fromVersion}个版本）";
    }
}