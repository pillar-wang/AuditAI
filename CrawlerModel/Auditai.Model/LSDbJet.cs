using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;

namespace Auditai.Model;

public class LSDbJet : LSDb
{
	private string _SystemDatabase;

	private string _UserID;

	private string _systemDatabasePassword;

	private OleDbConnectionStringBuilder _csBuilder = new OleDbConnectionStringBuilder();

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
			_csBuilder["Jet OLEDB:Database Password"] = value;
		}
	}

	public override string SystemDatabase
	{
		get
		{
			return _SystemDatabase;
		}
		set
		{
			_SystemDatabase = value;
			_csBuilder["Jet OLEDB:System Database"] = value;
		}
	}

	public override string UserId
	{
		get
		{
			return _UserID;
		}
		set
		{
			_UserID = value;
			_csBuilder["User ID"] = value;
		}
	}

	public override string SystemDatabasePassword
	{
		get
		{
			return _systemDatabasePassword;
		}
		set
		{
			_systemDatabasePassword = value;
			_csBuilder["Password"] = value;
		}
	}

	public override void TryConnectAsync()
	{
		throw new NotImplementedException();
	}

	public override int ExecuteNonQuery(string command)
	{
		throw new NotImplementedException();
	}

	public override T ExecuteScalar<T>(string command)
	{
		using OleDbConnection oleDbConnection = new OleDbConnection(_csBuilder.ConnectionString);
		oleDbConnection.Open();
		using OleDbCommand oleDbCommand = new OleDbCommand(command, oleDbConnection);
		object value = oleDbCommand.ExecuteScalar();
		return Convert.IsDBNull(value) ? default(T) : ((T)Convert.ChangeType(value, typeof(T)));
	}

	public override DataTable GetDataTable(string command)
	{
		DataTable dataTable = new DataTable();
		using OleDbConnection oleDbConnection = new OleDbConnection(_csBuilder.ConnectionString);
		oleDbConnection.Open();
		using OleDbDataAdapter oleDbDataAdapter = new OleDbDataAdapter(command, oleDbConnection);
		oleDbDataAdapter.Fill(dataTable);
		return dataTable;
	}

	public override bool TableExists(string tableName)
	{
		throw new NotImplementedException();
	}

	public LSDbJet()
	{
		_csBuilder.Provider = "Microsoft.Jet.OLEDB.4.0";
	}

	public override DataSet GetDataSet(IEnumerable<string> commands)
	{
		DataSet dataSet = new DataSet();
		using OleDbConnection oleDbConnection = new OleDbConnection(_csBuilder.ConnectionString);
		oleDbConnection.Open();
		foreach (string command in commands)
		{
			using OleDbDataAdapter oleDbDataAdapter = new OleDbDataAdapter(command, oleDbConnection);
			DataTable dataTable = new DataTable();
			oleDbDataAdapter.Fill(dataTable);
			dataSet.Tables.Add(dataTable);
		}
		return dataSet;
	}

	public override int ExecuteNonQueryTransaction(IEnumerable<string> commands)
	{
		int num = 0;
		using OleDbConnection oleDbConnection = new OleDbConnection(_csBuilder.ConnectionString);
		oleDbConnection.Open();
		using OleDbTransaction oleDbTransaction = oleDbConnection.BeginTransaction();
		foreach (string command in commands)
		{
			using OleDbCommand oleDbCommand = new OleDbCommand(command, oleDbConnection, oleDbTransaction);
			try
			{
				num += oleDbCommand.ExecuteNonQuery();
				oleDbTransaction.Commit();
			}
			catch (OleDbException)
			{
				oleDbTransaction.Rollback();
			}
		}
		return num;
	}

	public override bool Insert(string tableName, IEnumerable<IEnumerable<object>> @params)
	{
		if (@params.Count() <= 0)
		{
			return false;
		}
		int num = @params.First().Count();
		using OleDbConnection oleDbConnection = new OleDbConnection(_csBuilder.ConnectionString);
		oleDbConnection.Open();
		using OleDbTransaction oleDbTransaction = oleDbConnection.BeginTransaction();
		using (OleDbCommand oleDbCommand = new OleDbCommand(string.Concat("INSERT INTO ", tableName, " VALUES (", string.Concat(Enumerable.Repeat("?,", num - 1).ToArray()), "?)"), oleDbConnection))
		{
			OleDbParameter[] array = new OleDbParameter[num - 1 + 1];
			for (int i = 0; i <= num - 1; i++)
			{
				array[i] = new OleDbParameter();
				oleDbCommand.Parameters.Add(array[i]);
			}
			foreach (IEnumerable<object> param in @params)
			{
				for (int j = 0; j <= num - 1; j++)
				{
					array[j].Value = param.ElementAt(j);
				}
				oleDbCommand.ExecuteNonQuery();
			}
		}
		oleDbTransaction.Commit();
		return true;
	}

	public override bool InsertOne(string tableName, IEnumerable<object> @params)
	{
		int num = @params.Count();
		using OleDbConnection oleDbConnection = new OleDbConnection(_csBuilder.ConnectionString);
		oleDbConnection.Open();
		using (OleDbCommand oleDbCommand = new OleDbCommand(string.Concat("INSERT INTO ", tableName, " VALUES (", string.Concat(Enumerable.Repeat("?,", num - 1).ToArray()), "?)"), oleDbConnection))
		{
			OleDbParameter[] array = new OleDbParameter[num - 1 + 1];
			for (int i = 0; i <= num - 1; i++)
			{
				array[i] = new OleDbParameter();
				oleDbCommand.Parameters.Add(array[i]);
			}
			for (int j = 0; j <= num - 1; j++)
			{
				array[j].Value = @params.ElementAt(j);
			}
			oleDbCommand.ExecuteNonQuery();
		}
		return true;
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
		DataTable dataTable = new DataTable();
		using OleDbConnection oleDbConnection = new OleDbConnection(_csBuilder.ConnectionString);
		oleDbConnection.Open();
		using OleDbCommand oleDbCommand = new OleDbCommand(procedure, oleDbConnection);
		oleDbCommand.CommandType = CommandType.StoredProcedure;
		OleDbCommandBuilder.DeriveParameters(oleDbCommand);
		foreach (KeyValuePair<string, object> param in @params)
		{
			oleDbCommand.Parameters[param.Key].Value = param.Value;
		}
		using OleDbDataAdapter oleDbDataAdapter = new OleDbDataAdapter(oleDbCommand);
		oleDbDataAdapter.Fill(dataTable);
		return dataTable;
	}
}
