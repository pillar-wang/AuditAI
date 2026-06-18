using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Leqisoft.Model;

public abstract class LSDb
{
	public delegate void TryConnectDoneEventHandler(LSDb sender, TryConnectEventArgs e);

	public enum DbProvider
	{
		Jet,
		SqlServer,
		Oracle,
		MySql,
		Sqlite,
		Paradox
	}

	public virtual string DataSource { get; set; }

	public virtual string Database { get; set; }

	public virtual string UserId { get; set; }

	public virtual string Password { get; set; }

	public virtual string SystemDatabase { get; set; }

	public virtual string SystemDatabasePassword { get; set; }

	public virtual bool IntegratedSecurity { get; set; }

	public virtual int ConnectionTimeout { get; set; }

	public virtual bool IsBusy { get; }

	public event TryConnectDoneEventHandler TryConnectDone;

	public abstract DataTable GetDataTable(string command);

	public abstract DataSet GetDataSet(IEnumerable<string> commands);

	public abstract T ExecuteScalar<T>(string command);

	public abstract int ExecuteNonQuery(string command);

	public abstract int ExecuteNonQueryTransaction(IEnumerable<string> commands);

	public abstract bool Insert(string tableName, IEnumerable<IEnumerable<object>> @params);

	public abstract bool InsertOne(string tableName, IEnumerable<object> @params);

	public abstract IEnumerable<string> GetDatabaseNames();

	public abstract IEnumerable<string> GetUserDatabaseNames();

	public abstract bool TableExists(string tableName);

	public abstract DataTable ExecuteProcedure(string procedure, Dictionary<string, object> @params);

	public int GetTablesAllRecordsCount(IEnumerable<string> tableName)
	{
		return ExecuteScalar<int>("SELECT " + string.Join("+", tableName.Select((string x) => "(SELECT COUNT(1) FROM " + x + ")").ToArray()));
	}

	public abstract void TryConnectAsync();

	protected void OnTryConnectDone(TryConnectEventArgs e)
	{
		this.TryConnectDone?.Invoke(this, e);
	}

	public static LSDb Create(DbProvider provider)
	{
		switch (provider)
		{
		case DbProvider.Jet:
		case DbProvider.Paradox:
			return new LSDbJet();
		case DbProvider.SqlServer:
			return new LSDbSql();
		case DbProvider.Sqlite:
			return new LSDbSqlite();
		case DbProvider.Oracle:
			return new LSDbOracle();
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public static LSDb FromDatabaseInfo(DatabaseInfo dbInfo)
	{
		LSDb lSDb = Create(dbInfo.DatabaseType);
		if (dbInfo.DataSource != null)
		{
			lSDb.DataSource = dbInfo.DataSource;
		}
		if (dbInfo.Name != null)
		{
			lSDb.Database = dbInfo.Name;
		}
		if (dbInfo.User != null)
		{
			lSDb.UserId = dbInfo.User;
		}
		if (dbInfo.Password != null)
		{
			lSDb.Password = dbInfo.Password;
		}
		lSDb.IntegratedSecurity = dbInfo.IntegratedSecurity;
		return lSDb;
	}
}
