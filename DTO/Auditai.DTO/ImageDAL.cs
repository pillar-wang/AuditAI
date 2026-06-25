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
		connectionStringBuilder.SyncMode = SynchronizationModes.Off;
		connectionStringBuilder.DataSource = fileName;
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
		sQLiteConnection.Execute("INSERT INTO `Image`(`FileId`,`CenterX`,`CenterY`,`ZoomFactor`,`PageSetup`,`RotateFlip`) VALUES(@FileId,@CenterX,@CenterY,@ZoomFactor,@PageSetup,@RotateFlip)", dto, sQLiteTransaction);
		sQLiteTransaction.Commit();
	}
}
