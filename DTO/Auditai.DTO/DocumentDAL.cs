using System.Collections.Generic;
using System.Data.SQLite;
using Dapper;

namespace Auditai.DTO;

public class DocumentDAL
{
	private readonly SQLiteConnectionStringBuilder connectionStringBuilder = new SQLiteConnectionStringBuilder();

	static DocumentDAL()
	{
		SqlMapper.AddTypeHandler(new BinaryValueDapperHandler());
		SqlMapper.AddTypeHandler(new Id64DapperHandler());
	}

	public DocumentDAL(string fileName)
	{
		connectionStringBuilder.JournalMode = SQLiteJournalModeEnum.Wal;
		// 修复：与 ProjectDAL 保持一致，Off 模式在进程异常终止时丢失数据/损坏 -wal。
		connectionStringBuilder.SyncMode = SynchronizationModes.Normal;
		connectionStringBuilder.DataSource = fileName;
		// M4: 与 ProjectDAL 保持一致，busy_timeout 15 秒，降低并发锁冲突导致的 SQLITE_BUSY 异常
		connectionStringBuilder.BusyTimeout = 15000;
		SetPragma();
		CreateConfig();
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
		cnn.Execute("CREATE TABLE IF NOT EXISTS `Document`(\r\n`Locker` INTEGER,\r\n`SectPr` TEXT,\r\n`MergeTable` INTEGER NOT NULL DEFAULT 0);\r\n\r\nCREATE TABLE IF NOT EXISTS `Paragraph`(\r\n`Id` INTEGER PRIMARY KEY,\r\n`Index` INTEGER NOT NULL,\r\n`Stream` BLOB NOT NULL,\r\n`Section` BLOB,\r\n`Comment` TEXT);");
	}

	public Document GetDocument()
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.QueryFirstOrDefault<Document>("SELECT `Locker`,`SectPr`,`MergeTable` FROM `Document`");
	}

	public void SaveDocument(Document dto)
	{
		using SQLiteConnection sQLiteConnection = GetConnection();
		using SQLiteTransaction sQLiteTransaction = sQLiteConnection.BeginTransaction();
		// 修复：Document 表无主键，纯 INSERT 会累积重复行（GetDocument 只取第一条，行为不可预期）。
		// 该表语义为"每库一条文档配置"，改为先删后插保持单行。
		sQLiteConnection.Execute("DELETE FROM `Document`", null, sQLiteTransaction);
		sQLiteConnection.Execute("INSERT INTO `Document`(`Locker`,`SectPr`,`MergeTable`) VALUES(@Locker,@SectPr,@MergeTable)", dto, sQLiteTransaction);
		sQLiteTransaction.Commit();
	}

	public IEnumerable<Paragraph> GetParagraphs()
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<Paragraph>("SELECT `Id`,`Index`,`Stream`,`Section`,`Comment` FROM `Paragraph` ORDER BY `Index`");
	}

	public void SaveParagraphs(IEnumerable<Paragraph> dtos)
	{
		using SQLiteConnection sQLiteConnection = GetConnection();
		using SQLiteTransaction sQLiteTransaction = sQLiteConnection.BeginTransaction();
		// 修复：Paragraph 有主键，纯 INSERT 重复保存同 Id 抛 UNIQUE 冲突。改为 INSERT OR REPLACE。
		sQLiteConnection.Execute("INSERT OR REPLACE INTO `Paragraph`(`Id`,`Index`,`Stream`,`Section`,`Comment`) VALUES(@Id,@Index,@Stream,@Section,@Comment)", dtos, sQLiteTransaction);
		sQLiteTransaction.Commit();
	}
}
