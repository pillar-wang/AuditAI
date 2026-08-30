using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;

namespace Auditai.Model;

public class LSDbSqlite : LSDb
{
	private SQLiteConnectionStringBuilder _csBuilder = new SQLiteConnectionStringBuilder();

	public override string DataSource
	{
		get
		{
			return base.DataSource;
		}
		set
		{
			base.DataSource = value;
			_csBuilder.DataSource = value;
		}
	}

	public override string Database
	{
		get
		{
			return base.Database;
		}
		set
		{
			base.Database = value;
			_csBuilder.DataSource = value;
		}
	}

	public override string Password
	{
		get
		{
			return base.Password;
		}
		set
		{
			if (value == null)
			{
				value = string.Empty;
			}
			base.Password = value;
			_csBuilder.Password = value;
		}
	}

	public override void TryConnectAsync()
	{
		throw new NotImplementedException();
	}

	public override T ExecuteScalar<T>(string command)
	{
		using SQLiteConnection connection = new SQLiteConnection(_csBuilder.ConnectionString).OpenAndReturn();
		using SQLiteCommand sQLiteCommand = new SQLiteCommand(command, connection);
		object value = sQLiteCommand.ExecuteScalar();
		// 修复：SQL 无返回行时 ExecuteScalar 返回 null，Convert.IsDBNull(null) 为 false，
		// 原 Convert.ChangeType(null, typeof(T)) 对值类型（如 bool）抛 InvalidCastException。
		if (value == null || Convert.IsDBNull(value))
		{
			return default(T);
		}
		return (T)Convert.ChangeType(value, typeof(T));
	}

	public override int ExecuteNonQuery(string command)
	{
		using SQLiteConnection connection = new SQLiteConnection(_csBuilder.ConnectionString).OpenAndReturn();
		using SQLiteCommand sQLiteCommand = new SQLiteCommand(command, connection);
		return sQLiteCommand.ExecuteNonQuery();
	}

	public override DataSet GetDataSet(IEnumerable<string> commands)
	{
		DataSet dataSet = new DataSet();
		using SQLiteConnection connection = new SQLiteConnection(_csBuilder.ConnectionString).OpenAndReturn();
		foreach (string command in commands)
		{
			using SQLiteDataAdapter sQLiteDataAdapter = new SQLiteDataAdapter(command, connection);
			DataTable dataTable = new DataTable();
			sQLiteDataAdapter.Fill(dataTable);
			dataSet.Tables.Add(dataTable);
		}
		return dataSet;
	}

	public override DataTable GetDataTable(string command)
	{
		DataTable dataTable = new DataTable();
		using SQLiteConnection connection = new SQLiteConnection(_csBuilder.ConnectionString).OpenAndReturn();
		using SQLiteDataAdapter sQLiteDataAdapter = new SQLiteDataAdapter(command, connection);
		sQLiteDataAdapter.Fill(dataTable);
		return dataTable;
	}

	public override bool TableExists(string tableName)
	{
		// 修复：原实现将表名直接拼入单引号 SQL，含单引号表名会报错/注入。改为参数化。
		using SQLiteConnection connection = new SQLiteConnection(_csBuilder.ConnectionString).OpenAndReturn();
		using SQLiteCommand cmd = new SQLiteCommand("SELECT COUNT(1) FROM sqlite_master WHERE type='table' AND name=@name", connection);
		cmd.Parameters.Add(new SQLiteParameter("@name", tableName));
		long count = (long)cmd.ExecuteScalar();
		return count > 0;
	}

	public override bool Insert(string tableName, IEnumerable<IEnumerable<object>> @params)
	{
		if (@params.Count() <= 0)
		{
			return false;
		}
		int num = @params.First().Count();
		using SQLiteConnection sQLiteConnection = new SQLiteConnection(_csBuilder.ConnectionString).OpenAndReturn();
		using SQLiteTransaction sQLiteTransaction = sQLiteConnection.BeginTransaction();
		using (SQLiteCommand sQLiteCommand = new SQLiteCommand(string.Concat("INSERT INTO ", tableName, " VALUES (", string.Concat(Enumerable.Repeat("?,", num - 1).ToArray()), "?)"), sQLiteConnection))
		{
			SQLiteParameter[] array = new SQLiteParameter[num - 1 + 1];
			for (int i = 0; i <= num - 1; i++)
			{
				array[i] = new SQLiteParameter();
				sQLiteCommand.Parameters.Add(array[i]);
			}
			foreach (IEnumerable<object> param in @params)
			{
				for (int j = 0; j <= num - 1; j++)
				{
					array[j].Value = param.ElementAt(j);
				}
				sQLiteCommand.Transaction = sQLiteTransaction;
				sQLiteCommand.ExecuteNonQuery();
			}
		}
		sQLiteTransaction.Commit();
		return true;
	}

	public override bool InsertOne(string tableName, IEnumerable<object> @params)
	{
		int num = @params.Count();
		using SQLiteConnection connection = new SQLiteConnection(_csBuilder.ConnectionString).OpenAndReturn();
		using (SQLiteCommand sQLiteCommand = new SQLiteCommand(string.Concat("INSERT INTO ", tableName, " VALUES (", string.Concat(Enumerable.Repeat("?,", num - 1).ToArray()), "?)"), connection))
		{
			SQLiteParameter[] array = new SQLiteParameter[num - 1 + 1];
			for (int i = 0; i <= num - 1; i++)
			{
				array[i] = new SQLiteParameter();
				sQLiteCommand.Parameters.Add(array[i]);
			}
			for (int j = 0; j <= num - 1; j++)
			{
				array[j].Value = @params.ElementAt(j);
			}
			sQLiteCommand.ExecuteNonQuery();
		}
		return true;
	}

	public override int ExecuteNonQueryTransaction(IEnumerable<string> commands)
	{
		int num = 0;
		using SQLiteConnection sQLiteConnection = new SQLiteConnection(_csBuilder.ConnectionString).OpenAndReturn();
		using SQLiteTransaction sQLiteTransaction = sQLiteConnection.BeginTransaction();
		try
		{
			foreach (string command in commands)
			{
				using SQLiteCommand sQLiteCommand = new SQLiteCommand(command, sQLiteConnection, sQLiteTransaction);
				num += sQLiteCommand.ExecuteNonQuery();
			}
			sQLiteTransaction.Commit();
		}
		catch (SQLiteException)
		{
			// 修复：原实现在循环内 catch 中 Rollback，循环结束后仍无条件 Commit，
			// 对已回滚事务再次 Commit 抛 InvalidOperationException；且回滚后循环继续执行后续命令。
			// 改为：任一命令失败即回滚并中止，与其它 LSDb 实现语义一致。
			sQLiteTransaction.Rollback();
			throw;
		}
		return num;
	}

	public override IEnumerable<string> GetDatabaseNames()
	{
		return new string[1] { DataSource };
	}

	public override IEnumerable<string> GetUserDatabaseNames()
	{
		return GetDatabaseNames();
	}

	public override DataTable ExecuteProcedure(string procedure, Dictionary<string, object> @params)
	{
		throw new NotImplementedException();
	}

	public static string ByteArrayToBlob(byte[] array)
	{
		return "X'" + BitConverter.ToString(array).Replace("-", string.Empty) + "'";
	}
}
