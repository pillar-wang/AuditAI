using Microsoft.Data.Sqlite;

namespace AuditApiServer.Infra;

/// <summary>
/// SQLite 并发重试策略。阶段 1 Task 1.2 实现。
/// 高频写入（PushTable/PushDocument/SaveProject）在 WAL 模式下偶发 SQLITE_BUSY/LOCKED，
/// 此处统一捕获后按 200ms/500ms/1000ms 退避重试 3 次，仍失败则抛出原异常。
/// </summary>
public static class SqliteRetryPolicy
{
    /// <summary>
    /// 重试退避间隔（毫秒）：200ms / 500ms / 1000ms。
    /// </summary>
    private static readonly int[] BackoffMilliseconds = { 200, 500, 1000 };

    // SQLite 原生错误码：SQLITE_BUSY=5（数据库文件被其他连接锁住）、SQLITE_LOCKED=6（表级锁冲突）。
    // Microsoft.Data.Sqlite 8.x 的 SqliteException.SqliteErrorCode 返回 int，无独立枚举。
    private const int SqliteBusy = 5;
    private const int SqliteLocked = 6;

    /// <summary>
    /// 判断异常是否为可重试的 SQLite 并发冲突（Busy / Locked）。
    /// </summary>
    private static bool IsTransient(SqliteException ex)
    {
        return ex.SqliteErrorCode == SqliteBusy
            || ex.SqliteErrorCode == SqliteLocked;
    }

    /// <summary>
    /// 执行带返回值的异步操作，遇到 SQLITE_BUSY/LOCKED 自动重试 3 次。
    /// </summary>
    public static async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> action)
    {
        Exception? last = null;
        for (var attempt = 0; attempt <= BackoffMilliseconds.Length; attempt++)
        {
            try
            {
                return await action();
            }
            catch (SqliteException ex) when (IsTransient(ex))
            {
                last = ex;
                if (attempt >= BackoffMilliseconds.Length) break;
                await Task.Delay(BackoffMilliseconds[attempt]);
            }
        }
        throw last!;
    }

    /// <summary>
    /// 执行无返回值的异步操作，遇到 SQLITE_BUSY/LOCKED 自动重试 3 次。
    /// </summary>
    public static async Task ExecuteWithRetryAsync(Func<Task> action)
    {
        Exception? last = null;
        for (var attempt = 0; attempt <= BackoffMilliseconds.Length; attempt++)
        {
            try
            {
                await action();
                return;
            }
            catch (SqliteException ex) when (IsTransient(ex))
            {
                last = ex;
                if (attempt >= BackoffMilliseconds.Length) break;
                await Task.Delay(BackoffMilliseconds[attempt]);
            }
        }
        throw last!;
    }
}
