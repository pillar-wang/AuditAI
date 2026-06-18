namespace DbAccess;

public static class SQLServerHelper
{
	public static string GetSqlServerConnectionString(string address, string db)
	{
		return "Data Source=" + address.Trim() + ";Initial Catalog=" + db.Trim() + ";Integrated Security=SSPI;";
	}

	public static string GetSqlServerConnectionString(string address, string db, string user, string pass)
	{
		return "Data Source=" + address.Trim() + ";Initial Catalog=" + db.Trim() + ";User ID=" + user.Trim() + ";Password=" + pass.Trim();
	}
}
