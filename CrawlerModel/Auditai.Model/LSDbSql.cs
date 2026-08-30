using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;

namespace Auditai.Model;

public class LSDbSql : LSDb
{
	private bool _IsBusy;

	private SqlConnectionStringBuilder _csBuilder = new SqlConnectionStringBuilder();

	private static IEnumerable<string> SYSTEMDATABASES = new List<string> { "master", "tempdb", "model", "msdb", "pubs", "Northwind" };

	public override string DataSource
	{
		get
		{
			return base.DataSource;
		}
		set
		{
			string dataSource = (base.DataSource = value.Trim(default(char)));
			_csBuilder.DataSource = dataSource;
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
			_csBuilder.InitialCatalog = value;
		}
	}

	public override string UserId
	{
		get
		{
			return base.UserId;
		}
		set
		{
			base.UserId = value;
			_csBuilder.UserID = value;
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

	public override bool IntegratedSecurity
	{
		get
		{
			return base.IntegratedSecurity;
		}
		set
		{
			base.IntegratedSecurity = value;
			_csBuilder.IntegratedSecurity = value;
		}
	}

	public override int ConnectionTimeout
	{
		get
		{
			return base.ConnectionTimeout;
		}
		set
		{
			base.ConnectionTimeout = value;
			_csBuilder.ConnectTimeout = value;
		}
	}

	public override bool IsBusy => _IsBusy;

	internal LSDbSql()
	{
		ConnectionTimeout = 30;
	}

	public override void TryConnectAsync()
	{
		_IsBusy = true;
		using BackgroundWorker backgroundWorker = new BackgroundWorker();
		backgroundWorker.DoWork += delegate(object s, DoWorkEventArgs e)
		{
			using SqlConnection sqlConnection = new SqlConnection(_csBuilder.ConnectionString);
			sqlConnection.Open();
			e.Result = new TryConnectEventArgs(isSuccess: true, null);
		};
		backgroundWorker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs e)
		{
			_IsBusy = false;
			if (e.Error == null)
			{
				OnTryConnectDone((TryConnectEventArgs)e.Result);
			}
			else
			{
				OnTryConnectDone(new TryConnectEventArgs(isSuccess: false, (DbException)e.Error));
			}
		};
		backgroundWorker.RunWorkerAsync();
	}

	public override IEnumerable<string> GetDatabaseNames()
	{
		using SqlConnection sqlConnection = new SqlConnection(_csBuilder.ConnectionString);
		sqlConnection.Open();
		using SqlCommand sqlCommand = new SqlCommand("SELECT name FROM sysdatabases", sqlConnection);
		using SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
		List<string> list = new List<string>();
		while (sqlDataReader.Read())
		{
			list.Add(sqlDataReader.GetString(0));
		}
		return list;
	}

	public override IEnumerable<string> GetUserDatabaseNames()
	{
		return GetDatabaseNames().Except(SYSTEMDATABASES).ToList();
	}

	public override DataSet GetDataSet(IEnumerable<string> commands)
	{
		DataSet dataSet = new DataSet();
		using SqlConnection sqlConnection = new SqlConnection(_csBuilder.ConnectionString);
		sqlConnection.Open();
		foreach (string command in commands)
		{
			using SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(command, sqlConnection);
			DataTable dataTable = new DataTable();
			sqlDataAdapter.Fill(dataTable);
			dataSet.Tables.Add(dataTable);
		}
		return dataSet;
	}

	public override DataTable GetDataTable(string command)
	{
		DataTable dataTable = new DataTable();
		using SqlConnection sqlConnection = new SqlConnection(_csBuilder.ConnectionString);
		sqlConnection.Open();
		using SqlCommand sqlCommand = new SqlCommand(command, sqlConnection);
		using SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
		sqlCommand.CommandTimeout = 0;
		sqlDataAdapter.Fill(dataTable);
		return dataTable;
	}

	public override T ExecuteScalar<T>(string command)
	{
		using SqlConnection sqlConnection = new SqlConnection(_csBuilder.ConnectionString);
		sqlConnection.Open();
		using SqlCommand sqlCommand = new SqlCommand(command, sqlConnection);
		object value = sqlCommand.ExecuteScalar();
		return Convert.IsDBNull(value) ? default(T) : ((T)Convert.ChangeType(value, typeof(T)));
	}

	public override int ExecuteNonQuery(string command)
	{
		using SqlConnection sqlConnection = new SqlConnection(_csBuilder.ConnectionString);
		sqlConnection.Open();
		using SqlCommand sqlCommand = new SqlCommand(command, sqlConnection);
		return sqlCommand.ExecuteNonQuery();
	}

	public override bool TableExists(string tableName)
	{
		// 修复：原实现将 Database/tableName 直接拼入 SQL，Database 为 null 或含 ] 时生成非法 SQL/注入面。
		// 改为参数化查询。
		using SqlConnection sqlConnection = new SqlConnection(_csBuilder.ConnectionString);
		sqlConnection.Open();
		using SqlCommand cmd = new SqlCommand(
			"IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_CATALOG=@db AND TABLE_NAME=@tbl) SELECT 1 ELSE SELECT 0",
			sqlConnection);
		cmd.Parameters.Add(new SqlParameter("@db", (object)Database ?? DBNull.Value));
		cmd.Parameters.Add(new SqlParameter("@tbl", tableName));
		return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
	}

	public override int ExecuteNonQueryTransaction(IEnumerable<string> commands)
	{
		int num = 0;
		using SqlConnection sqlConnection = new SqlConnection(_csBuilder.ConnectionString);
		sqlConnection.Open();
		using SqlTransaction sqlTransaction = sqlConnection.BeginTransaction();
		try
		{
			foreach (string command in commands)
			{
				using SqlCommand sqlCommand = new SqlCommand(command, sqlConnection, sqlTransaction);
				num += sqlCommand.ExecuteNonQuery();
			}
			sqlTransaction.Commit();
		}
		catch (SqlException)
		{
			// 修复：原实现在循环内每条命令后 Commit，下一条命令仍绑定已提交事务
			// 会抛 InvalidOperationException（“此 SqlTransaction 已完成”）。改为循环外统一 Commit。
			sqlTransaction.Rollback();
			throw;
		}
		return num;
	}

	public override bool InsertOne(string tableName, IEnumerable<object> @params)
	{
		int num = @params.Count();
		if (num <= 0)
		{
			return false;
		}
		using SqlConnection sqlConnection = new SqlConnection(_csBuilder.ConnectionString);
		sqlConnection.Open();
		// 修复：SqlClient 不支持 "?" 占位符，原 SQL 会下发字面 "?" 导致语法错误（被 catch 吞掉后返回 false）。
		// 改用 @p0,@p1,... 命名参数。
		using SqlCommand sqlCommand = new SqlCommand(BuildInsertSql(tableName, num), sqlConnection);
		for (int i = 0; i < num; i++)
		{
			sqlCommand.Parameters.Add(new SqlParameter("@p" + i, @params.ElementAt(i)));
		}
		try
		{
			sqlCommand.ExecuteNonQuery();
			return true;
		}
		catch (SqlException)
		{
			return false;
		}
	}

	public override bool Insert(string tableName, IEnumerable<IEnumerable<object>> @params)
	{
		if (@params.Count() <= 0)
		{
			return false;
		}
		int num = @params.First().Count();
		using SqlConnection sqlConnection = new SqlConnection(_csBuilder.ConnectionString);
		sqlConnection.Open();
		using SqlTransaction sqlTransaction = sqlConnection.BeginTransaction();
		// 修复：SqlClient 不支持 "?" 占位符，改用 @p0,@p1,... 命名参数。
		using (SqlCommand sqlCommand = new SqlCommand(BuildInsertSql(tableName, num), sqlConnection, sqlTransaction))
		{
			foreach (IEnumerable<object> param in @params)
			{
				sqlCommand.Parameters.Clear();
				for (int j = 0; j < num; j++)
				{
					sqlCommand.Parameters.Add(new SqlParameter("@p" + j, param.ElementAt(j)));
				}
				sqlCommand.ExecuteNonQuery();
			}
		}
		sqlTransaction.Commit();
		return true;
	}

	private static string BuildInsertSql(string tableName, int count)
	{
		var sb = new System.Text.StringBuilder("INSERT INTO ");
		sb.Append(tableName).Append(" VALUES (");
		for (int i = 0; i < count; i++)
		{
			if (i > 0) sb.Append(',');
			sb.Append("@p").Append(i);
		}
		sb.Append(')');
		return sb.ToString();
	}

	public override DataTable ExecuteProcedure(string procedure, Dictionary<string, object> @params)
	{
		DataTable dataTable = new DataTable();
		using SqlConnection sqlConnection = new SqlConnection(_csBuilder.ConnectionString);
		sqlConnection.Open();
		using SqlCommand sqlCommand = new SqlCommand(procedure, sqlConnection);
		sqlCommand.CommandType = CommandType.StoredProcedure;
		SqlCommandBuilder.DeriveParameters(sqlCommand);
		foreach (KeyValuePair<string, object> param in @params)
		{
			sqlCommand.Parameters[param.Key].Value = param.Value;
		}
		using SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
		sqlDataAdapter.Fill(dataTable);
		return dataTable;
	}
}
