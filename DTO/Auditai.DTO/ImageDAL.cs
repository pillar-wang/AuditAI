using System.Data.SQLite;
using Dapper;

namespace Auditai.DTO;

public class ImageDAL
{
	private readonly SQLiteConnectionStringBuilder connectionStringBuilder = new SQLiteConnectionStringBuilder();

	static ImageDAL()
	{
		SqlMapper.AddTypeHandler(new BinaryValueDapperHandler());
		SqlMapper.AddTypeHandler(new Id64DapperHandler());
	}

	public ImageDAL(string fileName)
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
		cnn.Execute("CREATE TABLE IF NOT EXISTS `Image`(\r\n`FileId` GUID NOT NULL,\r\n`CenterX` REAL NOT NULL,\r\n`CenterY` REAL NOT NULL,\r\n`ZoomFactor` REAL NOT NULL,\r\n`PageSetup` TEXT,\r\n`RotateFlip` INTEGER NOT NULL\r\n);");
	}

	public Image GetImage()
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.QueryFirstOrDefault<Image>("SELECT `FileId`,`CenterX`,`CenterY`,`ZoomFactor`,`PageSetup`,`RotateFlip` FROM `Image`");
	}

	public void SaveImage(Image dto)
	{
		using SQLiteConnection sQLiteConnection = GetConnection();
		using SQLiteTransaction sQLiteTransaction = sQLiteConnection.BeginTransaction();
		// 修复：Image 表无主键，纯 INSERT 会累积重复行（GetImage 只取第一条）。先删后插保持单行。
		sQLiteConnection.Execute("DELETE FROM `Image`", null, sQLiteTransaction);
		sQLiteConnection.Execute("INSERT INTO `Image`(`FileId`,`CenterX`,`CenterY`,`ZoomFactor`,`PageSetup`,`RotateFlip`) VALUES(@FileId,@CenterX,@CenterY,@ZoomFactor,@PageSetup,@RotateFlip)", dto, sQLiteTransaction);
		sQLiteTransaction.Commit();
	}
}
