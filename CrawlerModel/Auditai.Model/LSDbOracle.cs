using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Devart.Data.Oracle;

namespace Auditai.Model;

public class LSDbOracle : LSDb
{
	private OracleConnectionStringBuilder _csBuilder = new OracleConnectionStringBuilder
	{
		Direct = true
	};

	public override string DataSource
	{
		get
		{
			return base.DataSource;
		}
		set
		{
			base.DataSource = value;
			_csBuilder.Server = value;
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
			_csBuilder.Sid = value;
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
			_csBuilder.UserId = value;
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
			base.Password = value;
			_csBuilder.Password = value;
		}
	}

	public override int ExecuteNonQuery(string command)
	{
		using OracleConnection oracleConnection = new OracleConnection(((DbConnectionStringBuilder)(object)_csBuilder).ConnectionString);
		oracleConnection.Open();
		OracleCommand oracleCommand = new OracleCommand(command, oracleConnection);
		try
		{
			return ((DbCommand)(object)oracleCommand).ExecuteNonQuery();
		}
		finally
		{
			((IDisposable)oracleCommand)?.Dispose();
		}
	}

	public override int ExecuteNonQueryTransaction(IEnumerable<string> commands)
	{
		int num = 0;
		using OracleConnection oracleConnection = new OracleConnection(((DbConnectionStringBuilder)(object)_csBuilder).ConnectionString);
		oracleConnection.Open();
		OracleTransaction oracleTransaction = oracleConnection.BeginTransaction();
		try
		{
			foreach (string command in commands)
			{
				OracleCommand oracleCommand = new OracleCommand(command, oracleConnection);
				try
				{
					oracleCommand.Transaction = oracleTransaction;
					try
					{
						num += ((DbCommand)(object)oracleCommand).ExecuteNonQuery();
						((DbTransaction)(object)oracleTransaction).Commit();
					}
					catch (OracleException)
					{
						((DbTransaction)(object)oracleTransaction).Rollback();
						throw;
					}
				}
				finally
				{
					((IDisposable)oracleCommand)?.Dispose();
				}
			}
			return num;
		}
		finally
		{
			((IDisposable)oracleTransaction)?.Dispose();
		}
	}

	public override DataTable ExecuteProcedure(string procedure, Dictionary<string, object> @params)
	{
		DataTable dataTable = new DataTable();
		using OracleConnection oracleConnection = new OracleConnection(((DbConnectionStringBuilder)(object)_csBuilder).ConnectionString);
		oracleConnection.Open();
		OracleCommand oracleCommand = new OracleCommand(procedure, oracleConnection);
		try
		{
			((DbCommand)(object)oracleCommand).CommandType = CommandType.StoredProcedure;
			OracleCommandBuilder.DeriveParameters(oracleCommand);
			foreach (KeyValuePair<string, object> param in @params)
			{
				((DbParameter)(object)oracleCommand.Parameters[param.Key]).Value = param.Value;
			}
			OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(oracleCommand);
			try
			{
				((DbDataAdapter)(object)oracleDataAdapter).Fill(dataTable);
				return dataTable;
			}
			finally
			{
				((IDisposable)oracleDataAdapter)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)oracleCommand)?.Dispose();
		}
	}

	public override T ExecuteScalar<T>(string command)
	{
		using OracleConnection oracleConnection = new OracleConnection(((DbConnectionStringBuilder)(object)_csBuilder).ConnectionString);
		oracleConnection.Open();
		OracleCommand oracleCommand = new OracleCommand(command, oracleConnection);
		try
		{
			object value = ((DbCommand)(object)oracleCommand).ExecuteScalar();
			return Convert.IsDBNull(value) ? default(T) : ((T)Convert.ChangeType(value, typeof(T)));
		}
		finally
		{
			((IDisposable)oracleCommand)?.Dispose();
		}
	}

	public override IEnumerable<string> GetDatabaseNames()
	{
		throw new NotImplementedException();
	}

	public override DataSet GetDataSet(IEnumerable<string> commands)
	{
		DataSet dataSet = new DataSet();
		using OracleConnection oracleConnection = new OracleConnection(((DbConnectionStringBuilder)(object)_csBuilder).ConnectionString);
		oracleConnection.Open();
		foreach (string command in commands)
		{
			OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(command, oracleConnection);
			try
			{
				DataTable dataTable = new DataTable();
				((DbDataAdapter)(object)oracleDataAdapter).Fill(dataTable);
				dataSet.Tables.Add(dataTable);
			}
			finally
			{
				((IDisposable)oracleDataAdapter)?.Dispose();
			}
		}
		return dataSet;
	}

	public override DataTable GetDataTable(string command)
	{
		DataTable dataTable = new DataTable();
		using OracleConnection oracleConnection = new OracleConnection(((DbConnectionStringBuilder)(object)_csBuilder).ConnectionString);
		oracleConnection.Open();
		OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(command, oracleConnection);
		try
		{
			((DbDataAdapter)(object)oracleDataAdapter).Fill(dataTable);
			return dataTable;
		}
		finally
		{
			((IDisposable)oracleDataAdapter)?.Dispose();
		}
	}

	public override IEnumerable<string> GetUserDatabaseNames()
	{
		throw new NotImplementedException();
	}

	public override bool Insert(string tableName, IEnumerable<IEnumerable<object>> @params)
	{
		if (@params.Count() <= 0)
		{
			return false;
		}
		int num = @params.First().Count();
		using OracleConnection oracleConnection = new OracleConnection(((DbConnectionStringBuilder)(object)_csBuilder).ConnectionString);
		oracleConnection.Open();
		OracleTransaction oracleTransaction = oracleConnection.BeginTransaction();
		try
		{
			OracleCommand oracleCommand = new OracleCommand(string.Concat("INSERT INTO ", tableName, " VALUES (", string.Concat(Enumerable.Repeat("?,", num - 1).ToArray()), "?)"), oracleConnection);
			try
			{
				OracleParameter[] array = new OracleParameter[num - 1 + 1];
				for (int i = 0; i <= num - 1; i++)
				{
					array[i] = new OracleParameter();
					oracleCommand.Parameters.Add(array[i]);
				}
				foreach (IEnumerable<object> param in @params)
				{
					for (int j = 0; j <= num - 1; j++)
					{
						((DbParameter)(object)array[j]).Value = param.ElementAt(j);
					}
					((DbCommand)(object)oracleCommand).ExecuteNonQuery();
				}
			}
			finally
			{
				((IDisposable)oracleCommand)?.Dispose();
			}
			((DbTransaction)(object)oracleTransaction).Commit();
			return true;
		}
		finally
		{
			((IDisposable)oracleTransaction)?.Dispose();
		}
	}

	public override bool InsertOne(string tableName, IEnumerable<object> @params)
	{
		int num = @params.Count();
		using OracleConnection oracleConnection = new OracleConnection(((DbConnectionStringBuilder)(object)_csBuilder).ConnectionString);
		oracleConnection.Open();
		OracleCommand oracleCommand = new OracleCommand(string.Concat("INSERT INTO ", tableName, " VALUES (", string.Concat(Enumerable.Repeat("?,", num - 1).ToArray()), "?)"), oracleConnection);
		try
		{
			OracleParameter[] array = new OracleParameter[num - 1 + 1];
			for (int i = 0; i <= num - 1; i++)
			{
				array[i] = new OracleParameter();
				oracleCommand.Parameters.Add(array[i]);
			}
			for (int j = 0; j <= num - 1; j++)
			{
				((DbParameter)(object)array[j]).Value = @params.ElementAt(j);
			}
			try
			{
				((DbCommand)(object)oracleCommand).ExecuteNonQuery();
				return true;
			}
			catch (OracleException)
			{
				return false;
			}
		}
		finally
		{
			((IDisposable)oracleCommand)?.Dispose();
		}
	}

	public override bool TableExists(string tableName)
	{
		using OracleConnection oracleConnection = new OracleConnection(((DbConnectionStringBuilder)(object)_csBuilder).ConnectionString);
		oracleConnection.Open();
		OracleCommand oracleCommand = new OracleCommand("select 1 from user_tables where upper(table_name)=:tablename", oracleConnection);
		try
		{
			OracleParameter value = new OracleParameter("tablename", tableName.ToUpper());
			oracleCommand.Parameters.Add(value);
			object value2 = ((DbCommand)(object)oracleCommand).ExecuteScalar();
			return 1m.Equals(value2);
		}
		finally
		{
			((IDisposable)oracleCommand)?.Dispose();
		}
	}

	public override void TryConnectAsync()
	{
		throw new NotImplementedException();
	}

	private string GetDataSource()
	{
		return "(DESCRIPTION =\r\n    (ADDRESS = (PROTOCOL = TCP)(HOST = " + DataSource + ")(PORT = 1521))\r\n    (CONNECT_DATA =\r\n      (SERVER = DEDICATED)\r\n      (SERVICE_NAME = " + Database + ")\r\n    )\r\n  )";
	}
}
