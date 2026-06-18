using System.Collections.Generic;
using System.Data.SQLite;
using Dapper;

namespace Leqisoft.DTO;

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
		sQLiteConnection.Execute("INSERT INTO `Paragraph`(`Id`,`Index`,`Stream`,`Section`,`Comment`) VALUES(@Id,@Index,@Stream,@Section,@Comment)", dtos, sQLiteTransaction);
		sQLiteTransaction.Commit();
	}
}
