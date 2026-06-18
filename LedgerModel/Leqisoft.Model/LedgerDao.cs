using System.Data.SQLite;
using Dapper;

namespace Leqisoft.Model;

public class LedgerDao
{
	private SQLiteConnectionStringBuilder connectionStringBuilder = new SQLiteConnectionStringBuilder();

	internal LedgerDao(string fileName)
	{
		connectionStringBuilder.DataSource = fileName;
	}

	private SQLiteConnection GetConnection()
	{
		return new SQLiteConnection(connectionStringBuilder.ConnectionString).OpenAndReturn();
	}

	public void Get()
	{
		using SQLiteConnection cnn = GetConnection();
		string sql = "SELECT a.code,v.dc,SUM(v.amount) FROM voucher v LEFT JOIN account a ON v.accountId = a.id GROUP BY v.accountId,v.dc";
		cnn.Query(sql);
	}
}
