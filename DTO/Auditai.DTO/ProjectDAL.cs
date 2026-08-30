using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Dapper;
using Newtonsoft.Json;

namespace Auditai.DTO;

public class ProjectDAL : IDisposable
{
	private readonly SQLiteConnectionStringBuilder connectionStringBuilder = new SQLiteConnectionStringBuilder();

	private int _transactionDepth;

	private SQLiteTransaction _transaction;

	// M2: 事务状态（_transaction/_transactionDepth/_transactionAborted）的读写锁保护
	private readonly object _syncRoot = new object();

	// M3: 嵌套事务中内层已回滚的标记，外层 Commit 时据此整体回滚并抛出
	private bool _transactionAborted;

	private bool _disposed;

	public static readonly string DefaultPermissions;

	static ProjectDAL()
	{
		DefaultPermissions = JsonConvert.SerializeObject(new
		{
			Read = new
			{
				GrantAll = true
			},
			Write = new
			{
				GrantAll = true
			},
			Schema = new
			{
				GrantAll = true
			}
		});
		SqlMapper.AddTypeHandler(new BinaryValueDapperHandler());
		SqlMapper.AddTypeHandler(new Id64DapperHandler());
		SqlMapper.AddTypeHandler(new GuidDapperHandler());
	}

	public ProjectDAL(string fileName)
	{
		connectionStringBuilder.JournalMode = SQLiteJournalModeEnum.Wal;
		// 关键修复：必须使用 Normal 而非 Off。
		// Off 模式下 SQLite 不调用 fsync，程序异常终止（调试器停止、断电、kill 进程）时
		// 未刷写的数据会丢失，-wal 文件可能损坏，下次打开会导致整个 .db 不可读
		// （表现为"一个表损坏，其他表也打不开"，因为它们共享同一个 .db 文件）。
		// Normal 模式在关键检查点刷盘，兼顾性能与安全。
		connectionStringBuilder.SyncMode = SynchronizationModes.Normal;
		connectionStringBuilder.DataSource = fileName;
		// 关键修复：设置 busy_timeout 为 15 秒。
		// 默认为 0，SQLite 遇到锁立即返回 SQLITE_BUSY 错误（"database is locked"）。
		// 在多连接并发场景（同步循环 + Saved 事件 AutoPush 并发访问同一 Dal）下会频繁失败。
		// 设置后 SQLite 会在内部自动重试 15 秒，大幅降低并发锁冲突导致的异常与卡死。
		// 通过连接字符串设置，确保 GetConnection() 创建的每个新连接都生效。
		connectionStringBuilder.BusyTimeout = 15000;
		SetPragma();
		CreateConfig();
		UpdateSchema();
		// 启动时执行 WAL checkpoint，合并上次异常退出可能遗留的 -wal 文件，
		// 避免下次打开时因 -wal 损坏导致整个数据库不可读。
		RecoverWalIfNeeded();
	}

	/// <summary>
	/// 启动时恢复 WAL：将可能遗留的 -wal 合并到主数据库，并清理 -shm/-wal 文件。
	/// 若 checkpoint 失败（说明 -wal 已损坏），尝试重置 WAL 以挽救主数据库。
	/// </summary>
	private void RecoverWalIfNeeded()
	{
		try
		{
			using SQLiteConnection cnn = GetConnection();
			// TRUNCATE 模式：合并后将 -wal 截断为 0 字节
			int result = cnn.Execute("PRAGMA wal_checkpoint(TRUNCATE);");
			// result: 0=ok, 1=busy(有读者), 2=lock-error
			// 在启动场景下一般返回 0；若非 0 再尝试一次 PASSIVE
			if (result != 0)
			{
				try { cnn.Execute("PRAGMA wal_checkpoint(PASSIVE);"); } catch (Exception ex) { LogError("RecoverWalIfNeeded: PASSIVE checkpoint 失败", ex); }
			}
		}
		catch (Exception ex)
		{
			// checkpoint 失败通常意味着 -wal 文件已损坏。
			// 尝试禁用再启用 WAL，强制 SQLite 重建 -wal 文件。
			LogError("RecoverWalIfNeeded: WAL checkpoint 失败，尝试重建 WAL", ex);
			try
			{
				using SQLiteConnection cnn2 = GetConnection();
				cnn2.Execute("PRAGMA journal_mode=DELETE;");
				cnn2.Execute("PRAGMA journal_mode=WAL;");
			}
			catch (Exception ex2)
			{
				// 若仍失败，记录但不抛出 —— 后续查询若失败会在业务层被捕获
				LogError("RecoverWalIfNeeded: 重建 WAL 失败", ex2);
			}
		}
	}

	/// <summary>
	/// 执行 WAL checkpoint，将 -wal 数据合并到主 .db 文件。
	/// 应在程序正常退出前调用，避免异常终止后 -wal 残留导致数据库损坏。
	/// </summary>
	public void CheckpointAndClose()
	{
		if (_disposed) return;
		try
		{
			// 先回滚任何未提交的事务，避免悬挂
			lock (_syncRoot)
			{
				if (_transaction != null)
				{
					try { _transaction.Rollback(); } catch (Exception ex) { LogError("CheckpointAndClose: 回滚残留事务失败", ex); }
					try { _transaction.Connection?.Close(); } catch (Exception ex2) { LogError("CheckpointAndClose: 关闭残留事务连接失败", ex2); }
					_transaction = null;
					_transactionDepth = 0;
					_transactionAborted = false;
				}
			}
			using SQLiteConnection cnn = GetConnection();
			try
			{
				cnn.Execute("PRAGMA wal_checkpoint(TRUNCATE);");
			}
			catch (Exception ex) { LogError("CheckpointAndClose: WAL checkpoint 失败", ex); /* checkpoint 失败不阻断退出 */ }
		}
		catch (Exception ex) { LogError("CheckpointAndClose: 退出清理失败", ex); /* 退出清理失败不应抛出 */ }
	}

	public void Dispose()
	{
		if (_disposed) return;
		_disposed = true;
		CheckpointAndClose();
	}

	public void BeginTransaction()
	{
		lock (_syncRoot)
		{
			if (_transactionDepth == 0)
			{
				// M1: BeginTransaction 抛异常时释放已打开的连接，避免连接泄漏
				var cnn = GetConnection();
				try
				{
					_transaction = cnn.BeginTransaction();
				}
				catch
				{
					cnn.Dispose();
					throw;
				}
				// M3: 新事务开始时重置中止标志
				_transactionAborted = false;
			}
			_transactionDepth++;
		}
	}

	public void Execute(string sql, object param = null)
	{
		bool useTransaction;
		// M2: 事务状态读取在锁内完成，与 BeginTransaction/Commit/Rollback 状态互斥
		lock (_syncRoot)
		{
			// 防御：当 _transaction 被并发清理或异常清理置为 null，
			// 但 _transactionDepth 仍 > 0 时，原代码会直接访问 _transaction.Connection 抛 NRE。
			// 这种状态损坏通常出现在 Save() 的 catch 块调用 Rollback 半途失败、
			// 或多上下文（如 SignalR 事件 / UI 消息循环在 await 期间触发的并发 Dal 操作）中。
			// 此处按非事务模式执行并重置深度计数器，保证不抛 NRE，数据仍能落盘。
			if (_transactionDepth == 0 || _transaction == null)
			{
				if (_transactionDepth != 0)
				{
					_transactionDepth = 0;
				}
				useTransaction = false;
			}
			else
			{
				useTransaction = true;
			}
			if (useTransaction)
			{
				// 事务路径：在锁内执行，保证对共享事务的访问串行化
				// （SQLiteConnection 非线程安全，且与 Commit/Rollback 并发会损坏事务状态）
				_transaction.Connection.Execute(sql, param, _transaction);
				return;
			}
		}
		// 非事务路径：先在锁内读取状态快照，再在锁外执行 SQL，避免长时间持锁
		using (SQLiteConnection sQLiteConnection = GetConnection())
		{
			using SQLiteTransaction sQLiteTransaction = sQLiteConnection.BeginTransaction();
			sQLiteTransaction.Connection.Execute(sql, param, sQLiteTransaction);
			sQLiteTransaction.Commit();
		}
	}

	public void Commit()
	{
		lock (_syncRoot)
		{
			// 防御：没有活动事务（深度已为 0 或 _transaction 已被清理）时直接返回，
			// 避免深度计数器越界减为负数，也避免访问 _transaction.Connection 抛 NRE。
			if (_transactionDepth <= 0 || _transaction == null)
			{
				_transactionDepth = 0;
				_transaction = null;
				return;
			}
			// M3: 内层事务已回滚时，外层事务不能提交——
			// 执行整体回滚并抛出异常，避免"半回滚"的数据被静默提交
			if (_transactionAborted)
			{
				try
				{
					_transaction.Rollback();
				}
				finally
				{
					try { _transaction.Connection?.Close(); } catch { }
					_transaction = null;
					_transactionDepth = 0;
					_transactionAborted = false;
				}
				throw new InvalidOperationException("内层事务已回滚，外层事务不能提交");
			}
			_transactionDepth--;
			if (_transactionDepth == 0)
			{
				try
				{
					_transaction.Commit();
				}
				finally
				{
					// 即使 Commit 抛异常（如 SQLite 锁错误），也要重置状态并关闭连接，
					// 避免悬挂事务导致后续 Execute 走错路径（depth=0 但 _transaction 非 null）。
					try { _transaction.Connection?.Close(); } catch { }
					_transaction = null;
				}
			}
		}
	}

	public void Rollback()
	{
		lock (_syncRoot)
		{
			// 防御：没有活动事务时直接返回，避免深度计数器越界减为负数。
			// 这在 Save() 的 catch 块中很关键：如果异常发生在 BeginTransaction 之前
			// （如 TreeNode/Project 空值检查、ThrowIfMaxSizeExceeded 等），
			// 此时 depth 仍为 0，原代码会把 depth 减到 -1，后续 Save 调用的状态全部错乱。
			if (_transactionDepth <= 0 || _transaction == null)
			{
				_transactionDepth = 0;
				_transaction = null;
				return;
			}
			_transactionDepth--;
			if (_transactionDepth == 0)
			{
				try
				{
					_transaction.Rollback();
				}
				finally
				{
					try { _transaction.Connection?.Close(); } catch { }
					_transaction = null;
				}
				// M3: 回滚到最外层（depth==0）时重置中止标志
				_transactionAborted = false;
			}
			else
			{
				// M3: 嵌套回滚（depth>1）不产生实际 SQL（底层事务仍存活），仅标记中止，
				// 由最外层 Commit 检测该标志后执行整体回滚并抛出异常
				_transactionAborted = true;
			}
		}
	}

	/// <summary>
	/// 在事务中执行一段写操作（H1）：任何异常都会回滚事务后原样抛出，
	/// 避免 Execute 中途抛异常后事务悬挂、部分数据静默丢失。
	/// </summary>
	private void RunInTransaction(Action action)
	{
		BeginTransaction();
		try
		{
			action();
			Commit();
		}
		catch
		{
			// 回滚失败（如事务已被 SQLite 自动终止）不能吞掉原始异常，保证调用方看到真正原因
			try { Rollback(); } catch { }
			throw;
		}
	}

	/// <summary>
	/// 将 DAL 底层异常最小化记录到库文件同目录的 dal_error.log（M6），
	/// 日志本身用 try-catch 包裹，避免产生二次异常影响主流程。
	/// </summary>
	private void LogError(string context, Exception ex)
	{
		try
		{
			string message = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {context}: {ex}";
			Debug.WriteLine(message);
			string logFile = Path.Combine(Path.GetDirectoryName(connectionStringBuilder.DataSource) ?? string.Empty, "dal_error.log");
			File.AppendAllText(logFile, message + Environment.NewLine);
		}
		catch
		{
			// 日志写入失败时静默忽略，不影响主流程
		}
	}

	private SQLiteConnection GetConnection()
	{
		return new SQLiteConnection(connectionStringBuilder.ConnectionString).OpenAndReturn();
	}

	private void SetPragma()
	{
		using SQLiteConnection cnn = GetConnection();
		cnn.Execute("PRAGMA locking_mode=EXCLUSIVE;");
	}

	public void CreateConfig()
	{
		using SQLiteConnection cnn = GetConnection();
		cnn.Execute("CREATE TABLE IF NOT EXISTS `Project`(\r\n`Id` GUID PRIMARY KEY,\r\n`Name` TEXT NOT NULL,\r\n`Parent` GUID,\r\n`Version` INTEGER NOT NULL DEFAULT 0,\r\n`Number` TEXT NOT NULL,\r\n`Category` TEXT NOT NULL,\r\n`Note` TEXT NOT NULL);\r\n\r\nCREATE TABLE IF NOT EXISTS `TreeGroup`(\r\n`Id` INTEGER PRIMARY KEY,\r\n`Name` TEXT NOT NULL,\r\n`Index` INTEGER NOT NULL,\r\n`ServerIndex` INTEGER NOT NULL,\r\n`Status` INTEGER NOT NULL,\r\n`Dirty` INTEGER NOT NULL);\r\n\r\nCREATE TABLE IF NOT EXISTS `TreeNode`(\r\n`Id` INTEGER PRIMARY KEY,\r\n`GroupId` INTEGER NOT NULL,\r\n`ParentId` INTEGER,\r\n`Name` TEXT NOT NULL,\r\n`Status` INTEGER NOT NULL,\r\n`Dirty` INTEGER NOT NULL,\r\n`Index` INTEGER NOT NULL,\r\n`ServerIndex` INTEGER NOT NULL,\r\n`Type` INTEGER NOT NULL,\r\n`Level` INTEGER NOT NULL,\r\n`Version` INTEGER NOT NULL);\r\n\r\nCREATE TABLE IF NOT EXISTS `Table`(\r\n`Id` INTEGER PRIMARY KEY,\r\n`Title` TEXT NOT NULL,\r\n`PageSetup` TEXT NOT NULL,\r\n`Dirty` INTEGER NOT NULL,\r\n`Note` TEXT NOT NULL,\r\n`HeaderHeights` TEXT,\r\n`DefaultStyleId` INTEGER NOT NULL,\r\n`ConsolidateSettings` TEXT);\r\n\r\nCREATE TABLE IF NOT EXISTS `Column`(\r\n`Id` INTEGER PRIMARY KEY,\r\n`TableId` INTEGER NOT NULL,\r\n`Index` INTEGER NOT NULL,\r\n`ServerIndex` INTEGER NOT NULL,\r\n`Caption` TEXT NOT NULL,\r\n`CaptionStyle` TEXT NOT NULL,\r\n`Width` INTEGER NOT NULL,\r\n`Visible` INTEGER NOT NULL,\r\n`Dirty` INTEGER NOT NULL,\r\n`Status` INTEGER NOT NULL,\r\n`ConsolidateAttribs` TEXT,\r\n`SubtotalAttribs` INTEGER NOT NULL DEFAULT 0,\r\n`Formula` TEXT,\r\n`StyleId` INTEGER);\r\n\r\nCREATE TABLE IF NOT EXISTS `Row`(\r\n`Id` INTEGER PRIMARY KEY,\r\n`TableId` INTEGER NOT NULL,\r\n`Index` INTEGER NOT NULL,\r\n`ServerIndex` INTEGER NOT NULL,\r\n`Height` INTEGER NOT NULL,\r\n`Visible` INTEGER NOT NULL,\r\n`Dirty` INTEGER NOT NULL,\r\n`Status` INTEGER NOT NULL);\r\n\r\nCREATE TABLE IF NOT EXISTS `Cell`(\r\n`Id` INTEGER PRIMARY KEY,\r\n`RowId` INTEGER NOT NULL,\r\n`ColumnId` INTEGER NOT NULL,\r\n`Value` BLOB NOT NULL,\r\n`StyleId` INTEGER,\r\n`Dirty` INTEGER NOT NULL,\r\n`Status` INTEGER NOT NULL,\r\n`Formula` TEXT);\r\n\r\nCREATE TABLE IF NOT EXISTS `CellStyle`(\r\n`Id` INTEGER PRIMARY KEY,\r\n`TableId` INTEGER NOT NULL,\r\n`FontFamily` TEXT,\r\n`FontSize` INTEGER,\r\n`ForeColor` INTEGER,\r\n`BackColor` INTEGER,\r\n`Align` INTEGER,\r\n`Margin` INTEGER,\r\n`Bold` INTEGER,\r\n`Italic` INTEGER,\r\n`Underline` INTEGER,\r\n`DataType` INTEGER,\r\n`Format` TEXT,\r\n`Status` INTEGER NOT NULL);\r\n\r\nCREATE TABLE IF NOT EXISTS `Document`(\r\n`Id` INTEGER PRIMARY KEY);\r\n\r\nCREATE TABLE IF NOT EXISTS `Paragraph`(\r\n`Id` INTEGER PRIMARY KEY,\r\n`DocumentId` INTEGER NOT NULL,\r\n`Index` INTEGER NOT NULL,\r\n`Stream` BLOB NOT NULL,\r\n`ServerIndex` INTEGER NOT NULL,\r\n`Dirty` INTEGER NOT NULL,\r\n`Status` INTEGER NOT NULL);\r\n\r\nCREATE TABLE IF NOT EXISTS `Merge`(\r\n`Id` INTEGER PRIMARY KEY,\r\n`TableId` INTEGER NOT NULL,\r\n`TopLeft` INTEGER NOT NULL,\r\n`BottomRight` INTEGER NOT NULL,\r\n`Status` INTEGER NOT NULL);\r\n\r\nCREATE TABLE IF NOT EXISTS `DataReference`(\r\n`Id` INTEGER PRIMARY KEY,\r\n`Key` TEXT NOT NULL,\r\n`Value` TEXT,\r\n`Status` INTEGER NOT NULL,\r\n`Dirty` INTEGER NOT NULL);\r\n\r\nCREATE TABLE IF NOT EXISTS `ValidationFormula`(\r\n`Id` INTEGER PRIMARY KEY,\r\n`LeftExpr` TEXT NOT NULL,\r\n`Operator` INT NOT NULL,\r\n`RightExpr` TEXT NOT NULL,\r\n`Note` TEXT NOT NULL,\r\n`Status` INTEGER NOT NULL,\r\n`Dirty` INTEGER NOT NULL)");
	}

	private void UpdateSchema()
	{
		using SQLiteConnection sQLiteConnection = GetConnection();
		int num = sQLiteConnection.ExecuteScalar<int>("PRAGMA user_version;");
		if (num == 0)
		{
			num = 1;
		}
		// H2: 迁移主体包在显式事务中（SQLite 支持 DDL 事务），任一步骤失败整体回滚，
		// 避免旧实现中途异常留下"半迁移"状态（user_version 未更新但部分结构已变更），
		// 导致下次打开重复执行 ALTER 时抛"duplicate column name"错误。
		// user_version 在事务内写入，仅在成功提交后生效，确保只在成功后写。
		SQLiteTransaction schemaTransaction = sQLiteConnection.BeginTransaction();
		try
		{
			num = MigrateSchema(sQLiteConnection, schemaTransaction, num);
			sQLiteConnection.Execute($"PRAGMA user_version={num}", schemaTransaction);
			schemaTransaction.Commit();
		}
		catch
		{
			try { schemaTransaction.Rollback(); } catch { }
			throw;
		}
	}

	/// <summary>
	/// 依次执行各版本的 Schema 迁移步骤，返回最终版本号。
	/// 调用方已开启事务，全部步骤要么一起提交、要么一起回滚。
	/// </summary>
	private int MigrateSchema(SQLiteConnection sQLiteConnection, SQLiteTransaction schemaTransaction, int num)
	{
		if (num == 1)
		{
			num = 2;
			sQLiteConnection.Execute("ALTER TABLE `Table` ADD COLUMN `BorderStyle` INTEGER NOT NULL DEFAULT 0");
		}
		if (num == 2)
		{
			num = 3;
			sQLiteConnection.Execute("ALTER TABLE `Row` ADD COLUMN `Locked` INTEGER");
			sQLiteConnection.Execute("ALTER TABLE `CellStyle` ADD COLUMN `Locked` INTEGER");
		}
		if (num == 3)
		{
			num = 4;
			sQLiteConnection.Execute("ALTER TABLE `Table` ADD COLUMN `FrozenCols` INTEGER NOT NULL DEFAULT 0");
		}
		if (num == 4)
		{
			num = 5;
			sQLiteConnection.Execute("ALTER TABLE `Table` ADD COLUMN `HeaderMode` INTEGER NOT NULL DEFAULT 0");
		}
		if (num == 5)
		{
			num = 6;
			sQLiteConnection.Execute("ALTER TABLE `Cell` ADD COLUMN `CollectSource` TEXT");
		}
		if (num == 6)
		{
			num = 7;
			sQLiteConnection.Execute("ALTER TABLE `Table` ADD COLUMN `CollectSource` TEXT");
		}
		if (num == 7)
		{
			num = 8;
			sQLiteConnection.Execute("ALTER TABLE `TreeNode` ADD COLUMN `Number` TEXT");
		}
		if (num == 8)
		{
			num = 9;
			sQLiteConnection.Execute("ALTER TABLE `Table` ADD COLUMN `Locker` INTEGER");
		}
		if (num == 9)
		{
			num = 10;
			sQLiteConnection.Execute("ALTER TABLE `Document` ADD COLUMN `Locker` INTEGER");
		}
		if (num == 10)
		{
			num = 11;
			sQLiteConnection.Execute("ALTER TABLE `Row` ADD COLUMN `Role` INTEGER");
		}
		if (num == 11)
		{
			num = 12;
			sQLiteConnection.Execute("ALTER TABLE `Document` ADD COLUMN `SectPr` TEXT");
		}
		if (num == 12)
		{
			num = 13;
			sQLiteConnection.Execute("ALTER TABLE `Table` ADD COLUMN `FilterInfo` TEXT");
		}
		if (num == 13)
		{
			num = 14;
			sQLiteConnection.Execute("ALTER TABLE `ValidationFormula` ADD COLUMN `TableId` INTEGER");
		}
		if (num == 14)
		{
			num = 15;
			sQLiteConnection.Execute("ALTER TABLE `Paragraph` ADD COLUMN `Section` BLOB");
		}
		if (num == 15)
		{
			num = 16;
			sQLiteConnection.Execute("ALTER TABLE `Document` ADD COLUMN `MergeTable` INTEGER NOT NULL DEFAULT 0");
		}
		if (num == 16)
		{
			num = 17;
			sQLiteConnection.Execute("ALTER TABLE `DataReference` ADD COLUMN `Kind` INTEGER NOT NULL DEFAULT 2");
		}
		if (num == 17)
		{
			num = 18;
			sQLiteConnection.Execute("ALTER TABLE `CellStyle` ADD COLUMN `DefaultValue` TEXT");
			sQLiteConnection.Execute("ALTER TABLE `CellStyle` ADD COLUMN `Comment` TEXT");
		}
		if (num == 18)
		{
			num = 19;
			sQLiteConnection.Execute("ALTER TABLE `Paragraph` ADD COLUMN `Comment` TEXT");
		}
		if (num == 19)
		{
			num = 20;
			sQLiteConnection.Execute("CREATE TABLE IF NOT EXISTS `CellStyle_temp`(\r\n`Id` INTEGER PRIMARY KEY,\r\n`TableId` INTEGER NOT NULL,\r\n`FontFamily` TEXT,\r\n`FontSize` REAL,\r\n`ForeColor` INTEGER,\r\n`BackColor` INTEGER,\r\n`Align` INTEGER,\r\n`Margin` INTEGER,\r\n`Bold` INTEGER,\r\n`Italic` INTEGER,\r\n`Underline` INTEGER,\r\n`DataType` INTEGER,\r\n`Format` TEXT,\r\n`Status` INTEGER NOT NULL,\r\n`Locked` INTEGER,\r\n`DefaultValue` TEXT,\r\n`Comment` TEXT);");
			// H2: 可重入保证——上次迁移若在重建中途失败可能残留旧数据，先清空再拷贝
			sQLiteConnection.Execute("DELETE FROM `CellStyle_temp`");
			sQLiteConnection.Execute("INSERT INTO `CellStyle_temp` SELECT * FROM `CellStyle`");
			sQLiteConnection.Execute("DROP TABLE `CellStyle`");
			sQLiteConnection.Execute("ALTER TABLE `CellStyle_temp` RENAME TO `CellStyle`");
		}
		if (num == 20)
		{
			num = 21;
			sQLiteConnection.Execute("CREATE TABLE IF NOT EXISTS `Image`(\r\n`Id` INTEGER PRIMARY KEY,\r\n`FileId` GUID NOT NULL);");
		}
		if (num == 21)
		{
			num = 22;
			sQLiteConnection.Execute("CREATE TABLE IF NOT EXISTS `Pdf`(\r\n`Id` INTEGER PRIMARY KEY,\r\n`FileId` GUID NOT NULL);");
		}
		if (num == 22)
		{
			num = 23;
			sQLiteConnection.Execute("ALTER TABLE `Document` ADD COLUMN `Dirty` INTEGER NOT NULL DEFAULT 0");
			sQLiteConnection.Execute("ALTER TABLE `Image` ADD COLUMN `Dirty` INTEGER NOT NULL DEFAULT 0");
			sQLiteConnection.Execute("ALTER TABLE `Image` ADD COLUMN `CenterX` REAL NOT NULL DEFAULT 0.5");
			sQLiteConnection.Execute("ALTER TABLE `Image` ADD COLUMN `CenterY` REAL NOT NULL DEFAULT 0.5");
			sQLiteConnection.Execute("ALTER TABLE `Image` ADD COLUMN `ZoomFactor` REAL NOT NULL DEFAULT 1.0");
			sQLiteConnection.Execute("ALTER TABLE `Image` ADD COLUMN `PageSetup` TEXT");
		}
		if (num == 23)
		{
			num = 24;
			sQLiteConnection.Execute("CREATE TABLE IF NOT EXISTS `Snapshot`(\r\n`Id` INTEGER PRIMARY KEY,\r\n`TreeNodeId` INTEGER NOT NULL,\r\n`DateTime` TEXT NOT NULL,\r\n`Size` INTEGER NOT NULL,\r\n`Kind` INTEGER NOT NULL,\r\n`Name` TEXT NOT NULL,\r\n`Deleted` INTEGER NOT NULL); ");
		}
		if (num == 24)
		{
			num = 25;
			sQLiteConnection.Execute("ALTER TABLE `project` ADD COLUMN `CreateTime` TEXT NOT NULL DEFAULT '2000-01-01'");
		}
		if (num == 25)
		{
			num = 26;
			sQLiteConnection.Execute("ALTER TABLE `TreeNode` ADD COLUMN `Permissions` TEXT NOT NULL DEFAULT '" + DefaultPermissions + "'");
			sQLiteConnection.Execute("ALTER TABLE `Column` ADD COLUMN `Permissions` TEXT NOT NULL DEFAULT '" + DefaultPermissions + "'");
			sQLiteConnection.Execute("ALTER TABLE `Row` ADD COLUMN `Permissions` TEXT NOT NULL DEFAULT '" + DefaultPermissions + "'");
		}
		if (num == 26)
		{
			num = 27;
			sQLiteConnection.Execute("ALTER TABLE `TreeNode` ADD COLUMN `Visible` INTEGER NOT NULL DEFAULT 1");
		}
		if (num == 27)
		{
			num = 28;
			sQLiteConnection.Execute("ALTER TABLE `Table` ADD COLUMN `Foot` TEXT NOT NULL DEFAULT '{}'");
		}
		if (num == 28)
		{
			num = 29;
			sQLiteConnection.Execute("CREATE INDEX IF NOT EXISTS `idx_Cell_RowId` ON `Cell` (`RowId`)");
			sQLiteConnection.Execute("CREATE INDEX IF NOT EXISTS `idx_Row_TableId` ON `Row` (`TableId`)");
			sQLiteConnection.Execute("CREATE INDEX IF NOT EXISTS `idx_Column_TableId` ON `Column` (`TableId`)");
		}
		if (num == 29)
		{
			num = 30;
			sQLiteConnection.Execute("ALTER TABLE `Image` ADD COLUMN `RotateFlip` INTEGER NOT NULL DEFAULT 0");
		}
		if (num == 30)
		{
			num = 31;
			sQLiteConnection.Execute("ALTER TABLE `Table` ADD COLUMN `RowOwnerExclusive` INTEGER NOT NULL DEFAULT 0");
		}
		if (num == 31)
		{
			num = 32;
			sQLiteConnection.Execute("ALTER TABLE `Row` ADD COLUMN `Creator` INTEGER NOT NULL DEFAULT 0");
		}
		if (num == 32)
		{
			num = 33;
			sQLiteConnection.Execute("ALTER TABLE `Column` ADD COLUMN `CaptionFormula` TEXT DEFAULT ''");
		}
		if (num == 33)
		{
			num = 34;
			sQLiteConnection.Execute("ALTER TABLE `Cell` ADD COLUMN `HeaderFormula` TEXT DEFAULT ''");
		}
		if (num == 34)
		{
			num = 35;
			sQLiteConnection.Execute("UPDATE `Paragraph` SET `Comment`='' WHERE `Comment` IS NULL");
		}
		if (num == 35)
		{
			num = 36;
			sQLiteConnection.Execute("UPDATE `Document` SET `Locker`=0 WHERE `Locker` IS NULL");
		}
		if (num == 36)
		{
			num = 37;
			sQLiteConnection.Execute("ALTER TABLE `Table` ADD COLUMN `RowOwnerLoad` INTEGER NOT NULL DEFAULT 0");
		}
		if (num == 37)
		{
			num = 38;
			sQLiteConnection.Execute("ALTER TABLE `Table` ADD COLUMN `RowOwnerLoadShare` BLOB NOT NULL DEFAULT ''");
		}
		if (num == 38)
		{
			num = 39;
			sQLiteConnection.Execute("ALTER TABLE `Column` ADD COLUMN `CrossAttributes` BLOB NOT NULL DEFAULT ''");
		}
		if (num == 39)
		{
			num = 40;
			sQLiteConnection.Execute("ALTER TABLE `TreeNode` ADD COLUMN `RowWrite` INTEGER NOT NULL DEFAULT 0");
			sQLiteConnection.Execute("ALTER TABLE `TreeNode` ADD COLUMN `RowRead` INTEGER NOT NULL DEFAULT 0");
			IEnumerable<object> enumerable = sQLiteConnection.Query("SELECT `Id`,`RowOwnerExclusive`,`RowOwnerLoad` FROM `Table`");
			// H2: 外层迁移事务已开启，同一连接不能嵌套 BeginTransaction，
			// 这些更新直接并入整体迁移事务（原内层独立事务已移除）
			foreach (dynamic item in enumerable)
			{
				sQLiteConnection.Execute("UPDATE `TreeNode` SET `RowWrite`=@RowOwnerExclusive,`RowRead`=@RowOwnerLoad WHERE `Id`=@Id", new
				{
					Id = (object)item.Id,
					RowOwnerExclusive = (object)item.RowOwnerExclusive,
					RowOwnerLoad = (object)item.RowOwnerLoad
				}, schemaTransaction);
			}
		}
		if (num == 40)
		{
			num = 41;
			sQLiteConnection.Execute("\r\nCREATE TABLE IF NOT EXISTS `CellProp` (\r\n`TableId` INTEGER NOT NULL,\r\n`CellId` INTEGER NOT NULL,\r\n`Dirty` INTEGER NOT NULL,\r\n`Status` INTEGER NOT NULL,\r\n`Attachments` BLOB NOT NULL DEFAULT '',\r\nPRIMARY KEY (`TableId`,`CellId`))");
		}
		if (num == 41)
		{
			num = 42;
			sQLiteConnection.Execute("ALTER TABLE `table` ADD COLUMN `Ticket` TEXT NOT NULL DEFAULT ''");
		}
		if (num == 42)
		{
			num = 43;
			sQLiteConnection.Execute("ALTER TABLE `table` ADD COLUMN `ControlFormula` TEXT NOT NULL DEFAULT ''");
		}
		if (num == 43)
		{
			num = 44;
			sQLiteConnection.Execute("CREATE INDEX IF NOT EXISTS `idx_Column_TableId` ON `Column` (`TableId`)");
		}
		// 自定义表格边框样式迁移
		try
		{
			sQLiteConnection.Execute("ALTER TABLE `Table` ADD COLUMN `CustomBorderStyle` TEXT");
		}
		catch { /* 列已存在则忽略 */ }
		// ValidationFormula 文档域绑定迁移
		try
		{
			sQLiteConnection.Execute("ALTER TABLE `ValidationFormula` ADD COLUMN `DocumentFieldId` INTEGER");
		}
		catch { /* 列已存在则忽略 */ }
		// 项目级自定义填充配置迁移
		try
		{
			sQLiteConnection.Execute("ALTER TABLE `Project` ADD COLUMN `CustomFillConfig` TEXT");
		}
		catch { /* 列已存在则忽略 */ }
		return num;
	}

	public Project GetProject()
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.QueryFirstOrDefault<Project>("SELECT `Id`,`Name`,`Parent`,`Version`,`Number`,`Category`,`Note`,`CreateTime`,`CustomFillConfig` FROM `Project`");
	}

	public void SaveProject(Project dto)
	{
		Execute("INSERT OR REPLACE INTO `Project`(`Id`,`Name`,`Parent`,`Version`,`Number`,`Category`,`Note`,`CreateTime`,`CustomFillConfig`) VALUES(@Id,@Name,@ParentId,@Version,@Number,@Category,@Note,@CreateTime,@CustomFillConfig)", dto);
	}

	public IEnumerable<TreeGroup> GetTreeGroups()
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<TreeGroup>("\r\nSELECT `Id`,`Name`,`Status`,`Dirty`,`ServerIndex`\r\nFROM `TreeGroup`\r\nWHERE `Status`<2\r\nORDER BY `Index`");
	}

	public void SaveTreeGroups(IEnumerable<TreeGroup> dto)
	{
		Execute("INSERT OR REPLACE INTO `TreeGroup`(`Id`,`Name`,`Index`,`Status`,`Dirty`,`ServerIndex`) VALUES(@Id,@Name,@Index,@Status,@Dirty,@ServerIndex)", dto);
	}

	public void RemoveTreeGroups(IEnumerable<Id64> ids)
	{
		RunInTransaction(() =>
		{
			foreach (Id64 id in ids)
			{
				Execute("UPDATE `TreeGroup` SET `Status`=2 WHERE `Id`= @id", new { id });
			}
		});
	}

	public void DeleteTreeGroups(IEnumerable<Id64> ids)
	{
		RunInTransaction(() =>
		{
			foreach (Id64 id in ids)
			{
				Execute("DELETE FROM `TreeGroup` WHERE `Id` = @id", new { id });
			}
		});
	}

	public IEnumerable<Id64> GetLocalRemovedTreeGroups()
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<Id64>("SELECT `Id` FROM `TreeGroup` WHERE `Status`=2");
	}

	public IEnumerable<TreeNode> GetTreeNodes()
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<TreeNode>("\r\nSELECT `Id`,`GroupId`,`ParentId`,`Name`,`Status`,`Dirty`,`Type`,`ServerIndex`,`Level`,`Version`,`Number`,`Permissions`,`Visible`,`RowWrite`,`RowRead` FROM `TreeNode` WHERE `Status`<2 ORDER BY `GroupId`,`ParentId`,`Index`");
	}

	public void SaveTreeNodes(IEnumerable<TreeNode> dto)
	{
		Execute("INSERT OR REPLACE INTO `TreeNode`(`Id`,`GroupId`,`ParentId`,`Name`,`Status`,`Dirty`,`Index`,`Type`,`ServerIndex`,`Level`,`Version`,`Number`,`Permissions`,`Visible`,`RowWrite`,`RowRead`) VALUES(@Id,@GroupId,@ParentId,@Name,@Status,@Dirty,@Index,@Type,@ServerIndex,@Level,@Version,@Number,@Permissions,@Visible,@RowWrite,@RowRead)", dto);
	}

	public void RemoveTreeNodes(IEnumerable<Id64> ids)
	{
		RunInTransaction(() =>
		{
			foreach (Id64 id in ids)
			{
				Execute("UPDATE `TreeNode` SET `Status`=2 WHERE `Id` = @id", new { id });
			}
		});
	}

	public void DeleteTreeNodes(IEnumerable<Id64> ids)
	{
		RunInTransaction(() =>
		{
			foreach (Id64 id in ids)
			{
				Execute("DELETE FROM `TreeNode` WHERE `Id` =@id", new { id });
			}
		});
	}

	public IEnumerable<Id64> GetLocalRemovedTreeNodes()
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<Id64>("SELECT `Id` FROM `TreeNode` WHERE `Status`=2");
	}

	public Table GetTable(Id64 id)
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.QueryFirstOrDefault<Table>("SELECT `Id`,`Title`,`PageSetup`,`Dirty`,`HeaderHeights`,`DefaultStyleId`,`ConsolidateSettings`,`BorderStyle`,`CustomBorderStyle`,`FrozenCols`,`HeaderMode`,`CollectSource`,`Locker`,`FilterInfo`,`Foot`,`RowOwnerExclusive`,`RowOwnerLoad`,`RowOwnerLoadShare`,`Ticket`,`ControlFormula` FROM `Table` WHERE `Id`=@Id", new
		{
			Id = id
		});
	}

	public IEnumerable<Column> GetColumns(Id64 tableId)
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<Column>("\r\nSELECT `Id`,`Index`,`Caption`,`CaptionStyle`,`Width`,`Visible`,`Dirty`,`Status`,`ConsolidateAttribs`,`SubtotalAttribs`,`Formula`,`ServerIndex`,`StyleId`,`Permissions`,`CaptionFormula`,`CrossAttributes`\r\nFROM `Column` \r\nWHERE `TableId`=@tableId AND `Status`<2\r\nORDER BY `Index`", new { tableId });
	}

	public async Task<List<Column>> GetTableColumns(long tableId)
	{
		var columns = new List<Column>();
		using var conn = new SQLiteConnection(connectionStringBuilder.ConnectionString);
		await conn.OpenAsync();
		using var cmd = new SQLiteCommand("SELECT Id, Caption FROM [Column] WHERE TableId = @TableId ORDER BY [Index]", conn);
		cmd.Parameters.AddWithValue("@TableId", tableId);
		using var reader = await cmd.ExecuteReaderAsync();
		while (await reader.ReadAsync())
		{
			columns.Add(new Column
			{
				Id = new Id64(reader.GetInt64(0)),
				Caption = reader.GetString(1)
			});
		}
		return columns;
	}

	public IEnumerable<Id64> GetLocalRemovedColumns(Id64 tableId)
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<Id64>("SELECT `Id` FROM `Column` WHERE `TableId`=@tableId AND `Status`=2", new { tableId });
	}

	public IEnumerable<Row> GetRows(Id64 tableId)
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<Row>("\r\nSELECT `Id`,`Index`,`Height`,`Visible`,`Locked`,`Dirty`,`Status`,`ServerIndex`,`Role`,`Permissions`,`Creator`\r\nFROM `Row` \r\nWHERE `TableId`=@tableId AND `Status`<2\r\nORDER BY `Index`,`Creator`", new { tableId });
	}

	public IEnumerable<Id64> GetLocalRemovedRows(Id64 tableId)
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<Id64>("SELECT `Id` FROM `Row` WHERE `TableId`=@tableId AND `Status`=2", new { tableId });
	}

	public IEnumerable<Cell> GetCells(Id64 tableId)
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<Cell>("\r\nSELECT c.`Id`,c.`RowId`,c.`ColumnId`,r.`Index`,l.`Index`,c.`Value`,c.`Dirty`,c.`Status`,c.`Formula`,c.`StyleId`,c.`CollectSource`,c.`HeaderFormula`\r\nFROM `Cell` AS c \r\nJOIN `Row` r ON c.`RowId`=r.`Id`\r\nJOIN `Column` l ON c.`ColumnId`=l.`Id`\r\nWHERE c.`Status`<2 AND r.`TableId`=@tableId\r\nORDER BY r.`Index`,r.`Creator`,l.`Index`", new { tableId });
	}

	public IEnumerable<CellProp> GetCellProps(Id64 tableId)
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<CellProp>("SELECT `CellId`,`Dirty`,`Status`,`Attachments` FROM `CellProp` WHERE `TableId`=@tableId", new { tableId });
	}

	public void SaveCellProps(IEnumerable<CellProp> dto)
	{
		RunInTransaction(() =>
		{
			Execute("INSERT OR REPLACE INTO `CellProp`(`TableId`,`CellId`,`Dirty`,`Status`,`Attachments`) VALUES(@TableId,@CellId,@Dirty,@Status,@Attachments)", dto);
		});
	}

	public IEnumerable<CellStyle> GetCellStyles(Id64 tableId)
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<CellStyle>("\r\nSELECT `Id`,`FontSize`,`ForeColor`,`BackColor`,`FontFamily`,`Margin`,`Align`,`Bold`,`Italic`,`Underline`,`DataType`,`Format`,`Locked`,`Status`,`DefaultValue`,`Comment`\r\nFROM `CellStyle`\r\nWHERE `TableId`=@tableId", new { tableId });
	}

	public IEnumerable<Merge> GetMerges(Id64 tableId)
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<Merge>("\r\nSELECT `Id`,`TopLeft`,`BottomRight`,`Status`\r\nFROM `Merge`\r\nWHERE `TableId`=@tableId AND `Status`<2", new { tableId });
	}

	public IEnumerable<Id64> GetLocalRemovedCells(Id64 tableId)
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<Id64>("SELECT `Id` FROM `Cell` WHERE `ColumnId` IN (SELECT `Id` FROM `Column` WHERE `TableId`=@tableId) AND `Status`=2", new { tableId });
	}

	public IEnumerable<Id64> GetLocalRemovedMerges(Id64 tableId)
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<Id64>("SELECT `Id` FROM `Merge` WHERE `TableId`=@tableId AND `Status`=2", new { tableId });
	}

	public void SaveTable(Table dto)
	{
		RunInTransaction(() =>
		{
			Execute("INSERT OR REPLACE INTO `Table`(`Id`,`Title`,`PageSetup`,`Note`,`Dirty`,`HeaderHeights`,`DefaultStyleId`,`ConsolidateSettings`,`BorderStyle`,`CustomBorderStyle`,`FrozenCols`,`HeaderMode`,`CollectSource`,`Locker`,`FilterInfo`,`Foot`,`RowOwnerExclusive`,`RowOwnerLoad`,`RowOwnerLoadShare`,`Ticket`,`ControlFormula`) VALUES(@Id,@Title,@PageSetup,'',@Dirty,@HeaderHeights,@DefaultStyleId,@ConsolidateSettings,@BorderStyle,@CustomBorderStyle,@FrozenCols,@HeaderMode,@CollectSource,@Locker,@FilterInfo,@Foot,@RowOwnerExclusive,@RowOwnerLoad,@RowOwnerLoadShare,@Ticket,@ControlFormula)", dto);
			Execute("UPDATE `TreeNode` SET `Version`=@Version WHERE `Id`=@Id", dto);
		});
	}

	public void SaveColumns(IEnumerable<Column> dto)
	{
		Execute("INSERT OR REPLACE INTO `Column`(`Id`,`TableId`,`Index`,`Caption`,`CaptionStyle`,`Width`,`Visible`,`Dirty`,`Status`,`ConsolidateAttribs`,`SubtotalAttribs`,`Formula`,`ServerIndex`,`StyleId`,`Permissions`,`CaptionFormula`,`CrossAttributes`) VALUES(@Id,@TableId,@Index,@Caption,@CaptionStyle,@Width,@Visible,@Dirty,@Status,@ConsolidateAttribs,@SubtotalAttribs,@Formula,@ServerIndex,@StyleId,@Permissions,@CaptionFormula,@CrossAttributes)", dto);
	}

	public void RemoveColumns(IEnumerable<Id64> ids)
	{
		RunInTransaction(() =>
		{
			foreach (Id64 id in ids)
			{
				Execute("UPDATE `Column` SET `Status`=2 WHERE `Id` = @id", new { id });
			}
		});
	}

	public void DeleteColumns(IEnumerable<Id64> ids)
	{
		RunInTransaction(() =>
		{
			foreach (Id64 id in ids)
			{
				Execute("DELETE FROM `Column` WHERE `Id` = @id", new { id });
			}
		});
	}

	public void SaveRows(IEnumerable<Row> dto)
	{
		Execute("INSERT OR REPLACE INTO `Row`(`Id`,`TableId`,`Index`,`Height`,`Visible`,`Locked`,`Dirty`,`Status`,`ServerIndex`,`Role`,`Permissions`,`Creator`) VALUES(@Id,@TableId,@Index,@Height,@Visible,@Locked,@Dirty,@Status,@ServerIndex,@Role,@Permissions,@Creator)", dto);
	}

	public void RemoveRows(IEnumerable<Id64> ids)
	{
		RunInTransaction(() =>
		{
			foreach (Id64 id in ids)
			{
				Execute("UPDATE `Row` SET `Status`=2 WHERE `Id` = @id", new { id });
			}
		});
	}

	public void DeleteRows(IEnumerable<Id64> ids)
	{
		RunInTransaction(() =>
		{
			foreach (Id64 id in ids)
			{
				Execute("DELETE FROM `Row` WHERE `Id` = @id", new { id });
			}
		});
	}

	public void SaveCells(IEnumerable<Cell> dto)
	{
		Execute("INSERT OR REPLACE INTO `Cell`(`Id`,`RowId`,`ColumnId`,`Value`,`Dirty`,`Status`,`Formula`,`StyleId`,`CollectSource`,`HeaderFormula`) VALUES(@Id,@RowId,@ColumnId,@Value,@Dirty,@Status,@Formula,@StyleId,@CollectSource,@HeaderFormula)", dto);
	}

	public void RemoveCells(IEnumerable<Id64> ids)
	{
		RunInTransaction(() =>
		{
			foreach (Id64 id in ids)
			{
				Execute("UPDATE `Cell` SET `Status`=2 WHERE `Id` = @id", new { id });
			}
		});
	}

	public void RemoveMerges(IEnumerable<Id64> ids)
	{
		RunInTransaction(() =>
		{
			foreach (Id64 id in ids)
			{
				Execute("UPDATE `Merge` SET `Status`=2 WHERE `Id` = @id", new { id });
			}
		});
	}

	public void DeleteCells(IEnumerable<Id64> ids)
	{
		RunInTransaction(() =>
		{
			foreach (Id64 id in ids)
			{
				Execute("DELETE FROM `Cell` WHERE `Id` = @id", new { id });
			}
		});
	}

	public void DeleteMerges(IEnumerable<Id64> ids)
	{
		RunInTransaction(() =>
		{
			foreach (Id64 id in ids)
			{
				Execute("DELETE FROM `Merge` WHERE `Id` = @id", new { id });
			}
		});
	}

	public void SaveCellStyles(IEnumerable<CellStyle> dto)
	{
		Execute("INSERT OR REPLACE INTO `CellStyle`(`Id`,`TableId`,`FontSize`,`ForeColor`,`BackColor`,`FontFamily`,`Status`,`Margin`,`Align`,`Bold`,`Italic`,`Underline`,`DataType`,`Format`,`Locked`,`DefaultValue`,`Comment`) VALUES(@Id,@TableId,@FontSize,@ForeColor,@BackColor,@FontFamily,@Status,@Margin,@Align,@Bold,@Italic,@Underline,@DataType,@Format,@Locked,@DefaultValue,@Comment)", dto);
	}

	public void SaveMerges(IEnumerable<Merge> dto)
	{
		Execute("INSERT OR REPLACE INTO `Merge`(`Id`,`TableId`,`TopLeft`,`BottomRight`,`Status`) VALUES(@Id,@TableId,@TopLeft,@BottomRight,@Status)", dto);
	}

	public Document GetDocument(Id64 id)
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.QueryFirstOrDefault<Document>("SELECT `Id`,`Locker`,`SectPr`,`MergeTable`,`Dirty` FROM `Document` WHERE `Id`=@id", new { id });
	}

	public void SaveDocument(Document dto)
	{
		RunInTransaction(() =>
		{
			Execute("INSERT OR REPLACE INTO `Document`(`Id`,`Locker`,`SectPr`,`MergeTable`,`Dirty`) VALUES(@Id,@Locker,@SectPr,@MergeTable,@Dirty)", dto);
			Execute("UPDATE `TreeNode` SET `Version`=@Version WHERE `Id`=@Id", dto);
		});
	}

	public Image GetImage(Id64 id)
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.QueryFirstOrDefault<Image>("SELECT `Id`,`FileId`,`Dirty`,`CenterX`,`CenterY`,`ZoomFactor`,`PageSetup`,`RotateFlip` FROM `Image` WHERE `Id`=@id", new { id });
	}

	public void SaveImage(Image dto)
	{
		RunInTransaction(() =>
		{
			Execute("INSERT OR REPLACE INTO `Image`(`Id`,`FileId`,`Dirty`,`CenterX`,`CenterY`,`ZoomFactor`,`PageSetup`,`RotateFlip`) VALUES(@Id,@FileId,@Dirty,@CenterX,@CenterY,@ZoomFactor,@PageSetup,@RotateFlip)", dto);
			Execute("UPDATE `TreeNode` SET `Version`=@Version WHERE `Id`=@Id", dto);
		});
	}

	public Pdf GetPdf(Id64 id)
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.QueryFirstOrDefault<Pdf>("SELECT `Id`,`FileId` FROM `Pdf` WHERE `Id`=@id", new { id });
	}

	public void SavePdf(Pdf dto)
	{
		RunInTransaction(() =>
		{
			Execute("INSERT OR REPLACE INTO `Pdf`(`Id`,`FileId`) VALUES(@Id,@FileId)", dto);
			Execute("UPDATE `TreeNode` SET `Version`=@Version WHERE `Id`=@Id", dto);
		});
	}

	public IEnumerable<Paragraph> GetParagraphs(Id64 docId)
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<Paragraph>("SELECT `Id`,`Index`,`Stream`,`ServerIndex`,`Dirty`,`Status`,`Section`,`Comment` FROM `Paragraph` WHERE `DocumentId`=@docId AND `Status`<2 ORDER BY `Index`", new { docId });
	}

	public IEnumerable<Id64> GetLocalRemovedParagraphs(Id64 docId)
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<Id64>("SELECT `Id` FROM `Paragraph` WHERE `DocumentId`=@docId AND `Status`=2", new { docId });
	}

	public void SaveParagraphs(IEnumerable<Paragraph> dtos)
	{
		Execute("INSERT OR REPLACE INTO `Paragraph`(`Id`,`DocumentId`,`Index`,`Stream`,`ServerIndex`,`Dirty`,`Status`,`Section`,`Comment`) VALUES(@Id,@DocumentId,@Index,@Stream,@ServerIndex,@Dirty,@Status,@Section,@Comment)", dtos);
	}

	public void RemoveParagraphs(IEnumerable<Id64> ids)
	{
		RunInTransaction(() =>
		{
			foreach (Id64 id in ids)
			{
				Execute("UPDATE `Paragraph` SET `Status`=2 WHERE `Id` = @id", new { id });
			}
		});
	}

	public void DeleteParagraphs(IEnumerable<Id64> ids)
	{
		RunInTransaction(() =>
		{
			foreach (Id64 id in ids)
			{
				Execute("DELETE FROM `Paragraph` WHERE `Id` = @id", new { id });
			}
		});
	}

	public IEnumerable<DataReference> GetDataReferences()
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<DataReference>("SELECT `Id`,`Key`,`Value`,`Status`,`Dirty`,`Kind` FROM `DataReference` WHERE `Status`<2");
	}

	public IEnumerable<Id64> GetLocalRemovedDataReferences()
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<Id64>("SELECT `Id` FROM `DataReference` WHERE `Status`=2");
	}

	public void SaveDataReferences(IEnumerable<DataReference> dtos)
	{
		Execute("INSERT OR REPLACE INTO `DataReference`(`Id`,`Key`,`Value`,`Status`,`Dirty`,`Kind`) VALUES(@Id,@Key,@Value,@Status,@Dirty,@Kind)", dtos);
	}

	public void RemoveDataReferences(IEnumerable<Id64> ids)
	{
		RunInTransaction(() =>
		{
			foreach (Id64 id in ids)
			{
				Execute("UPDATE `DataReference` SET `Status`=2 WHERE `Id` = @id", new { id });
			}
		});
	}

	public void DeleteDataReferences(IEnumerable<Id64> ids)
	{
		RunInTransaction(() =>
		{
			foreach (Id64 id in ids)
			{
				Execute("DELETE FROM `DataReference` WHERE `Id` = @id", new { id });
			}
		});
	}

	public IEnumerable<ValidationFormula> GetValidationFormulas()
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<ValidationFormula>("SELECT `Id`,`LeftExpr`,`Operator`,`RightExpr`,`Note`,`Status`,`Dirty`,`TableId`,`DocumentFieldId` FROM `ValidationFormula` WHERE `Status`<2");
	}

	public IEnumerable<Id64> GetLocalRemovedValidationFormulas()
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<Id64>("SELECT `Id` FROM `ValidationFormula` WHERE `Status`=2");
	}

	public void SaveValidationFormulas(IEnumerable<ValidationFormula> dtos)
	{
		Execute("INSERT OR REPLACE INTO `ValidationFormula`(`Id`,`LeftExpr`,`Operator`,`RightExpr`,`Note`,`Status`,`Dirty`,`TableId`,`DocumentFieldId`) VALUES(@Id,@LeftExpr,@Operator,@RightExpr,@Note,@Status,@Dirty,@TableId,@DocumentFieldId)", dtos);
	}

	public void RemoveValidationFormulas(IEnumerable<Id64> ids)
	{
		RunInTransaction(() =>
		{
			foreach (Id64 id in ids)
			{
				Execute("UPDATE `ValidationFormula` SET `Status`=2 WHERE `Id` = @id", new { id });
			}
		});
	}

	public void DeleteValidationFormulas(IEnumerable<Id64> ids)
	{
		RunInTransaction(() =>
		{
			foreach (Id64 id in ids)
			{
				Execute("DELETE FROM `ValidationFormula` WHERE `Id` = @id", new { id });
			}
		});
	}

	public void EnsureFormatComplianceRuleTable()
	{
		using SQLiteConnection cnn = GetConnection();
		cnn.Execute("CREATE TABLE IF NOT EXISTS `FormatComplianceRule`(\r\n`Id` INTEGER,\r\n`RuleType` INTEGER,\r\n`Pattern` TEXT,\r\n`Note` TEXT,\r\n`Status` INTEGER,\r\n`Dirty` INTEGER)");
	}

	public IEnumerable<FormatComplianceRule> GetFormatComplianceRules()
	{
		EnsureFormatComplianceRuleTable();
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<FormatComplianceRule>("SELECT `Id`,`RuleType`,`Pattern`,`Note`,`Status`,`Dirty` FROM `FormatComplianceRule` WHERE `Status`<2");
	}

	public IEnumerable<Id64> GetLocalRemovedFormatComplianceRules()
	{
		EnsureFormatComplianceRuleTable();
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<Id64>("SELECT `Id` FROM `FormatComplianceRule` WHERE `Status`=2");
	}

	public void SaveFormatComplianceRules(IEnumerable<FormatComplianceRule> dtos)
	{
		EnsureFormatComplianceRuleTable();
		Execute("INSERT OR REPLACE INTO `FormatComplianceRule`(`Id`,`RuleType`,`Pattern`,`Note`,`Status`,`Dirty`) VALUES(@Id,@RuleType,@Pattern,@Note,@Status,@Dirty)", dtos);
	}

	public void RemoveFormatComplianceRules(IEnumerable<Id64> ids)
	{
		EnsureFormatComplianceRuleTable();
		RunInTransaction(() =>
		{
			foreach (Id64 id in ids)
			{
				Execute("UPDATE `FormatComplianceRule` SET `Status`=2 WHERE `Id` = @id", new { id });
			}
		});
	}

	public void DeleteFormatComplianceRules(IEnumerable<Id64> ids)
	{
		EnsureFormatComplianceRuleTable();
		RunInTransaction(() =>
		{
			foreach (Id64 id in ids)
			{
				Execute("DELETE FROM `FormatComplianceRule` WHERE `Id` = @id", new { id });
			}
		});
	}

	public void EnsureCrossDocumentValidationRuleTable()
	{
		using SQLiteConnection cnn = GetConnection();
		cnn.Execute("CREATE TABLE IF NOT EXISTS `CrossDocumentValidationRule`(\r\n`Id` INTEGER,\r\n`SourceDocumentId` INTEGER,\r\n`SourceFieldId` INTEGER,\r\n`TargetDocumentId` INTEGER,\r\n`TargetFieldId` INTEGER,\r\n`Operator` INTEGER,\r\n`Note` TEXT,\r\n`Status` INTEGER,\r\n`Dirty` INTEGER);");
	}

	public IEnumerable<Auditai.DTO.CrossDocumentValidationRule> GetCrossDocumentValidationRules()
	{
		EnsureCrossDocumentValidationRuleTable();
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<Auditai.DTO.CrossDocumentValidationRule>("SELECT `Id`, `SourceDocumentId`, `SourceFieldId`, `TargetDocumentId`, `TargetFieldId`, `Operator`, `Note`, `Status`, `Dirty` FROM `CrossDocumentValidationRule` WHERE `Status`<2");
	}

	public IEnumerable<Id64> GetLocalRemovedCrossDocumentValidationRules()
	{
		EnsureCrossDocumentValidationRuleTable();
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<Id64>("SELECT `Id` FROM `CrossDocumentValidationRule` WHERE `Status`=2");
	}

	public void SaveCrossDocumentValidationRules(IEnumerable<Auditai.DTO.CrossDocumentValidationRule> dtos)
	{
		EnsureCrossDocumentValidationRuleTable();
		Execute("INSERT OR REPLACE INTO `CrossDocumentValidationRule`(`Id`, `SourceDocumentId`, `SourceFieldId`, `TargetDocumentId`, `TargetFieldId`, `Operator`, `Note`, `Status`, `Dirty`) VALUES (@Id, @SourceDocumentId, @SourceFieldId, @TargetDocumentId, @TargetFieldId, @Operator, @Note, @Status, @Dirty)", dtos);
	}

	public void RemoveCrossDocumentValidationRules(IEnumerable<Id64> ids)
	{
		EnsureCrossDocumentValidationRuleTable();
		RunInTransaction(() =>
		{
			foreach (Id64 id in ids)
			{
				Execute("UPDATE `CrossDocumentValidationRule` SET `Status`=2 WHERE `Id`=@Id", new { Id = id });
			}
		});
	}

	public void DeleteCrossDocumentValidationRules(IEnumerable<Id64> ids)
	{
		EnsureCrossDocumentValidationRuleTable();
		RunInTransaction(() =>
		{
			foreach (Id64 id in ids)
			{
				Execute("DELETE FROM `CrossDocumentValidationRule` WHERE `Id`=@Id", new { Id = id });
			}
		});
	}

	public object GetCellValueById(Id64 id)
	{
		using SQLiteConnection cnn = GetConnection();
		BinaryValue? v = cnn.QueryFirstOrDefault<BinaryValue?>("SELECT `Value` FROM `Cell` WHERE `Id`=@id", new { id });
		return v.HasValue ? v.Value.Value : null;
	}

	public IEnumerable<SnapshotInfo> GetSnapshots(Id64 nodeId)
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<SnapshotInfo>("SELECT `Id`,`TreeNodeId`,`DateTime`,`Size`,`Kind`,`Name`,`Deleted` FROM `Snapshot` WHERE `TreeNodeId`=@nodeId", new { nodeId });
	}

	public void SaveSnapshot(SnapshotInfo si)
	{
		using SQLiteConnection cnn = GetConnection();
		cnn.Execute("INSERT INTO `Snapshot` (`Id`,`TreeNodeId`,`DateTime`,`Size`,`Kind`,`Name`,`Deleted`) VALUES(@Id,@TreeNodeId,@DateTime,@Size,@Kind,@Name,@Deleted)", si);
	}

	public void DeleteSnapshot(SnapshotInfo si)
	{
		using SQLiteConnection cnn = GetConnection();
		cnn.Execute("DELETE FROM `Snapshot` WHERE `Id`=@Id AND `Kind`=@Kind", si);
	}

	public void DeleteSnapshots(Id64 nodeId)
	{
		using SQLiteConnection cnn = GetConnection();
		cnn.Execute("DELETE FROM `Snapshot` WHERE `TreeNodeId`=@nodeId", new { nodeId });
	}

	public int GetLastSnapshotId()
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.QueryFirstOrDefault<int?>("SELECT MAX(`Id`) FROM `Snapshot`").GetValueOrDefault();
	}

	public IEnumerable<SnapshotInfo> GetRecycleList()
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<SnapshotInfo>("SELECT `Id`,`TreeNodeId`,`DateTime`,`Size`,`Kind`,`Name`,`Deleted`\r\nFROM `Snapshot` WHERE `Deleted`=1");
	}

	public IEnumerable<FormulaRecord> GetColumnFormulas()
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<FormulaRecord>("SELECT c.`TableId`,c.`Id` AS `ObjectId`,c.`Formula` FROM `Column` c inner join `treenode` n on c.`tableid`=n.`id` WHERE c.`Formula`<>'' AND c.`Status`<2");
	}

	public IEnumerable<FormulaRecord> GetCellFormulas()
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<FormulaRecord>("SELECT l.`TableId`,c.`Id` AS `ObjectId`,c.`Formula` FROM `Cell` c inner JOIN `Column` l ON c.`ColumnId`=l.`Id` inner join `treenode` n on l.`tableid`=n.`id` WHERE c.`Formula`<>'' AND c.`Status`<2");
	}

	public IEnumerable<FormulaRecord> GetHeaderCellFormulas()
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<FormulaRecord>("SELECT l.`TableId`,c.`Id` AS `ObjectId`,c.`HeaderFormula` FROM `Cell` c inner JOIN `Column` l ON c.`ColumnId`=l.`Id` inner join `treenode` n on l.`tableid`=n.`id` WHERE c.`HeaderFormula`<>'' AND c.`Status`<2");
	}

	public void DeleteTable(Id64 tableId)
	{
		// M7: 多条 DELETE 包入事务保证原子性，并补充 CellProp 的级联清理
		RunInTransaction(() =>
		{
			Execute("delete from `cell` where `columnId` in (select `id` from `column` where `tableId`=@tableId);\r\ndelete from `column` where `tableId`=@tableId;\r\ndelete from `row` where `tableId`=@tableId;\r\ndelete from `merge` where `tableId`=@tableId;", new { tableId });
			Execute("DELETE FROM `CellProp` WHERE `TableId`=@tableId", new { tableId });
		});
	}
}
