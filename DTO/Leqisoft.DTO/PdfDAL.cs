using System.Data.SQLite;
using Dapper;

namespace Leqisoft.DTO;

public class PdfDAL
{
	private readonly SQLiteConnectionStringBuilder connectionStringBuilder = new SQLiteConnectionStringBuilder();

	static PdfDAL()
	{
		SqlMapper.AddTypeHandler(new BinaryValueDapperHandler());
		SqlMapper.AddTypeHandler(new Id64DapperHandler());
	}

	public PdfDAL(string fileName)
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
		cnn.Execute("CREATE TABLE IF NOT EXISTS `Pdf`(\r\n`FileId` GUID NOT NULL\r\n);");
	}

	public Pdf GetPdf()
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.QueryFirstOrDefault<Pdf>("SELECT `FileId` FROM `Pdf`");
	}

	public void SavePdf(Pdf dto)
	{
		using SQLiteConnection sQLiteConnection = GetConnection();
		using SQLiteTransaction sQLiteTransaction = sQLiteConnection.BeginTransaction();
		sQLiteConnection.Execute("INSERT INTO `Pdf`(`FileId`) VALUES(@FileId)", dto, sQLiteTransaction);
		sQLiteTransaction.Commit();
	}
}
