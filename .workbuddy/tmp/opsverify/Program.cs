using System;
using System.Reflection;
using AuditAI.McpServer.Services;

// ServerOpsService 远端命令构造的 A/B 验证。
//
// 通过反射调用【真实编译产物】里的 internal 方法 QuoteForRemoteShell / IsReadOnlySql，
// 输出两条"远端 shell 会收到的命令串"：
//   OLD = 修复前的写法（把 " 转义成 \" 后用双引号包裹，逐字复刻原实现的两行）
//   NEW = 修复后的写法（调用真实的 QuoteForRemoteShell 单引号包裹）
// 再由本地 bash 分别执行这两条串（把程序名换成 echo），检查注入标记文件是否被创建
// —— 远端 shell 与本地 bash 的解析语义一致，故这是对注入面的直接验证。
internal static class Program
{
	private static int Main(string[] args)
	{
		Console.OutputEncoding = System.Text.Encoding.UTF8;
		string marker = (args.Length > 0) ? args[0] : "INJECTED_MARKER";
		string injection = "$(touch " + marker + ")";
		string sql = "SELECT 1\"" + injection + "\"";
		string dbPath = "/opt/auditapi/Data/auditai_server.db";

		MethodInfo quote = typeof(ServerOpsService).GetMethod("QuoteForRemoteShell",
			BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
		MethodInfo readOnlySql = typeof(ServerOpsService).GetMethod("IsReadOnlySql",
			BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
		if (quote == null || readOnlySql == null)
		{
			Console.Error.WriteLine("找不到目标方法（反射失败）");
			return 2;
		}

		// --- OLD：逐字复刻修复前的实现 ---
		string oldSafeSql = sql.Replace("\"", "\\\"");
		string oldCmd = "sqlite3 \"" + dbPath + "\" \"" + oldSafeSql + "\"";

		// --- NEW：调用真实修复后的实现 ---
		string newCmd = "sqlite3 " + (string)quote.Invoke(null, new object[] { dbPath })
			+ " " + (string)quote.Invoke(null, new object[] { sql });

		Console.WriteLine("OLD::" + oldCmd);
		Console.WriteLine("NEW::" + newCmd);

		// --- 只读判定 ---
		Console.WriteLine("SQLCASE::allowWrite=false::" + Describe(readOnlySql, "SELECT name FROM sqlite_master", false));
		Console.WriteLine("SQLCASE::select-lower::" + Describe(readOnlySql, "  select 1", false));
		Console.WriteLine("SQLCASE::multi-stmt::" + Describe(readOnlySql, "SELECT 1; DROP TABLE Project", false));
		Console.WriteLine("SQLCASE::drop::" + Describe(readOnlySql, "DROP TABLE Project", false));
		Console.WriteLine("SQLCASE::delete::" + Describe(readOnlySql, "DELETE FROM VersionHistory", false));
		Console.WriteLine("SQLCASE::attach::" + Describe(readOnlySql, "ATTACH DATABASE '/tmp/x' AS y", false));
		Console.WriteLine("SQLCASE::with-cte::" + Describe(readOnlySql, "WITH t AS (SELECT 1) SELECT * FROM t", false));
		Console.WriteLine("SQLCASE::trailing-semicolon::" + Describe(readOnlySql, "SELECT 1;", false));
		Console.WriteLine("SQLCASE::write-with-optin::" + Describe(readOnlySql, "DELETE FROM VersionHistory", true));
		Console.WriteLine("SQLCASE::pragma-read::" + Describe(readOnlySql, "PRAGMA table_info(Project)", false));
		Console.WriteLine("SQLCASE::pragma-write::" + Describe(readOnlySql, "PRAGMA user_version=5", false));
		Console.WriteLine("SQLCASE::pragma-writable-schema::" + Describe(readOnlySql, "PRAGMA writable_schema=1", false));
		Console.WriteLine("SQLCASE::with-hiding-delete::" + Describe(readOnlySql, "WITH x AS (SELECT 1) DELETE FROM VersionHistory", false));
		Console.WriteLine("SQLCASE::literal-drop-allowed::" + Describe(readOnlySql, "SELECT 'DROP TABLE x' AS note", false));
		Console.WriteLine("SQLCASE::identifier-insert-allowed::" + Describe(readOnlySql, "SELECT * FROM VersionHistory WHERE note = 'INSERT'", false));
		Console.WriteLine("SQLCASE::create-in-where::" + Describe(readOnlySql, "SELECT * FROM t WHERE action = 'CREATE'", false));
		return 0;
	}

	private static string Describe(MethodInfo m, string sql, bool allowWrite)
	{
		object[] argv = { sql, allowWrite, null };
		bool ok = (bool)m.Invoke(null, argv);
		string err = (string)argv[2];
		return (ok ? "ALLOW" : "REJECT(" + err + ")");
	}
}
