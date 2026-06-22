﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Leqisoft.DTO;

namespace Leqisoft.Model;

/// <summary>
/// 跨项目数据引用同步通知器
/// 负责在来源数据变更时记录通知，供目标项目查询
/// </summary>
public class CrossProjectRefSyncNotifier
{
    /// <summary>
    /// 当项目中的某张表数据被保存时，检查是否有其他项目引用它，若有则记录通知
    /// 此方法应在保存成功后调用
    /// </summary>
    public static async Task NotifyOnDataChanged(Guid projectId, Id64 tableId)
    {
        try
        {
            string dbPath = GetProjectDbPath(projectId);
            if (!File.Exists(dbPath)) return;

            var store = new CrossProjectDataRefStore(null);
            // 直接使用 dbPath 操作
            using var conn = new SQLiteConnection($"Data Source={dbPath};Version=3;");
            await conn.OpenAsync();

            // 检查是否有其他项目引用此表
            using var checkCmd = new SQLiteCommand(
                "SELECT COUNT(*) FROM CrossProjectDataRef WHERE SourceProjectId = @SourceProjectId AND SourceTableId = @SourceTableId AND Enabled = 1", conn);
            checkCmd.Parameters.AddWithValue("@SourceProjectId", projectId.ToString());
            checkCmd.Parameters.AddWithValue("@SourceTableId", tableId.Value);
            var count = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());
            if (count == 0) return;

            // 获取所有目标项目
            using var targetCmd = new SQLiteCommand(
                "SELECT DISTINCT TargetProjectId FROM CrossProjectDataRef WHERE SourceProjectId = @SourceProjectId2 AND SourceTableId = @SourceTableId2 AND Enabled = 1", conn);
            targetCmd.Parameters.AddWithValue("@SourceProjectId2", projectId.ToString());
            targetCmd.Parameters.AddWithValue("@SourceTableId2", tableId.Value);
            using var reader = await targetCmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var targetProjectId = reader.GetString(0);
                // 在目标项目的数据库中记录通知
                NotifyTargetProject(projectId, tableId, targetProjectId);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.Message);
        }
    }

    private static void NotifyTargetProject(Guid sourceProjectId, Id64 sourceTableId, string targetProjectIdStr)
    {
        try
        {
            var targetProjectId = Guid.Parse(targetProjectIdStr);
            string targetDbPath = GetProjectDbPath(targetProjectId);
            if (!File.Exists(targetDbPath)) return;

            using var conn = new SQLiteConnection($"Data Source={targetDbPath};Version=3;");
            conn.Open();

            // 确保通知表存在
            using var ensureCmd = new SQLiteCommand(@"
                CREATE TABLE IF NOT EXISTS CrossProjectRefNotify (
                    NotifyId INTEGER PRIMARY KEY AUTOINCREMENT,
                    SourceProjectId TEXT NOT NULL,
                    SourceTableId INTEGER NOT NULL,
                    TargetProjectId TEXT NOT NULL,
                    NotifiedAt TEXT NOT NULL,
                    IsAcknowledged INTEGER NOT NULL DEFAULT 0,
                    AcknowledgedAt TEXT
                )", conn);
            ensureCmd.ExecuteNonQuery();

            using var insertCmd = new SQLiteCommand(@"
                INSERT INTO CrossProjectRefNotify (SourceProjectId, SourceTableId, TargetProjectId, NotifiedAt)
                VALUES (@SourceProjectId, @SourceTableId, @TargetProjectId, @NotifiedAt)", conn);
            insertCmd.Parameters.AddWithValue("@SourceProjectId", sourceProjectId.ToString());
            insertCmd.Parameters.AddWithValue("@SourceTableId", sourceTableId.Value);
            insertCmd.Parameters.AddWithValue("@TargetProjectId", targetProjectIdStr);
            insertCmd.Parameters.AddWithValue("@NotifiedAt", DateTime.Now.ToString("o"));
            insertCmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.Message);
        }
    }

    private static string GetProjectDbPath(Guid projectId)
    {
        long userId = User.Current?.Id ?? 1;
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        return Path.Combine(baseDir, "data", userId.ToString(), $"{projectId}.db");
    }

    /// <summary>
    /// 检查目标项目是否有待处理的通知
    /// </summary>
    public static int GetPendingNotificationCount(Guid targetProjectId)
    {
        try
        {
            string dbPath = GetProjectDbPath(targetProjectId);
            if (!File.Exists(dbPath)) return 0;

            using var conn = new SQLiteConnection($"Data Source={dbPath};Version=3;");
            conn.Open();

            using var ensureCmd = new SQLiteCommand(@"
                CREATE TABLE IF NOT EXISTS CrossProjectRefNotify (
                    NotifyId INTEGER PRIMARY KEY AUTOINCREMENT,
                    SourceProjectId TEXT NOT NULL,
                    SourceTableId INTEGER NOT NULL,
                    TargetProjectId TEXT NOT NULL,
                    NotifiedAt TEXT NOT NULL,
                    IsAcknowledged INTEGER NOT NULL DEFAULT 0,
                    AcknowledgedAt TEXT
                )", conn);
            ensureCmd.ExecuteNonQuery();

            using var cmd = new SQLiteCommand(
                "SELECT COUNT(*) FROM CrossProjectRefNotify WHERE IsAcknowledged = 0", conn);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }
        catch { return 0; }
    }
}