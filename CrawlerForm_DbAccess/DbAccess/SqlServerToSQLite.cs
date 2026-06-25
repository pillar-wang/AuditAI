#define DEBUG
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace DbAccess;

public class SqlServerToSQLite
{
	private static bool _isActive = false;

	private static bool _cancelled = false;

	private static Regex _keyRx = new Regex("(([a-zA-Z_äöüÄÖÜß0-9\\.]|(\\s+))+)(\\(\\-\\))?");

	private static Regex _defaultValueRx = new Regex("\\(N(\\'.*\\')\\)");

	public static bool IsActive => _isActive;

	public static void CancelConversion()
	{
		_cancelled = true;
	}

	public static void ConvertSqlServerToSQLiteDatabase(string sqlServerConnString, string sqlitePath, ConvertConfig config)
	{
		_cancelled = false;
		try
		{
			_isActive = true;
			ConvertSqlServerDatabaseToSQLiteFile(sqlServerConnString, sqlitePath, config.UsePassword, config.progressHandler, config.tableSelectionHandler, config.viewFailedHandler, config.CreatePrimarykey, config.CreateForignkey, config.CreateCollate, config.CreateIndex, config.CreateTriggers, config.CreateViews);
			_isActive = false;
			config.progressHandler?.Invoke(new DefaultProgressInfo(done: true, success: true, 100, "Finished converting database"));
		}
		catch (Exception ex)
		{
			_isActive = false;
			config.progressHandler?.Invoke(new DefaultProgressInfo(done: true, success: false, 100, ex.Message));
			throw;
		}
	}

	private static void ConvertSqlServerDatabaseToSQLiteFile(string sqlConnString, string sqlitePath, string password, SqlConversionHandler handler, Predicate<string> selectionHandler, FailedViewDefinitionHandler viewFailureHandler, bool createPrimarykey, bool createForignkey, bool createCollate, bool createIndex, bool createTriggers, bool createViews)
	{
		if (File.Exists(sqlitePath))
		{
			File.Delete(sqlitePath);
		}
		DatabaseSchema databaseSchema = ReadSqlServerSchema(sqlConnString, handler, selectionHandler, createPrimarykey, createForignkey, createCollate, createIndex, createViews);
		CreateSQLiteDatabase(sqlitePath, databaseSchema, password, handler, viewFailureHandler, createViews);
		CopySqlServerRowsToSQLiteDB(sqlConnString, sqlitePath, databaseSchema.Tables, password, handler);
		if (createTriggers)
		{
			AddTriggersForForeignKeys(sqlitePath, databaseSchema.Tables, password, handler);
		}
	}

	private static void CopySqlServerRowsToSQLiteDB(string sqlConnString, string sqlitePath, List<TableSchema> schema, string password, SqlConversionHandler progressHandler)
	{
		CheckCancelled();
		progressHandler?.Invoke(new DefaultProgressInfo(done: false, success: true, 0, "Preparing to insert tables..."));
		using SqlConnection sqlConnection = new SqlConnection(sqlConnString);
		sqlConnection.Open();
		string connectionString = CreateSQLiteConnectionString(sqlitePath, password);
		using SQLiteConnection sQLiteConnection = new SQLiteConnection(connectionString);
		sQLiteConnection.Open();
		for (int i = 0; i < schema.Count; i++)
		{
			SQLiteTransaction sQLiteTransaction = sQLiteConnection.BeginTransaction();
			try
			{
				string cmdText = BuildSqlServerTableQuery(schema[i]);
				SqlCommand sqlCommand = new SqlCommand(cmdText, sqlConnection);
				using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
				{
					SQLiteCommand sQLiteCommand = BuildSQLiteInsert(schema[i]);
					int num = 0;
					while (sqlDataReader.Read())
					{
						sQLiteCommand.Connection = sQLiteConnection;
						sQLiteCommand.Transaction = sQLiteTransaction;
						List<string> list = new List<string>();
						for (int j = 0; j < schema[i].Columns.Count; j++)
						{
							string text = "@" + GetNormalizedName(schema[i].Columns[j].ColumnName, list);
							sQLiteCommand.Parameters[text].Value = CastValueForColumn(sqlDataReader[j], schema[i].Columns[j]);
							list.Add(text);
						}
						sQLiteCommand.ExecuteNonQuery();
						num++;
						if (num % 1000 == 0)
						{
							CheckCancelled();
							sQLiteTransaction.Commit();
							progressHandler?.Invoke(new DefaultProgressInfo(done: false, success: true, (int)(100.0 * (double)i / (double)schema.Count), "Added " + num + " rows to table " + schema[i].TableName + " so far"));
							sQLiteTransaction = sQLiteConnection.BeginTransaction();
						}
					}
				}
				CheckCancelled();
				sQLiteTransaction.Commit();
				progressHandler?.Invoke(new DefaultProgressInfo(done: false, success: true, (int)(100.0 * (double)i / (double)schema.Count), "Finished inserting rows for table " + schema[i].TableName));
			}
			catch (Exception)
			{
				sQLiteTransaction.Rollback();
				throw;
			}
		}
	}

	private static object CastValueForColumn(object val, ColumnSchema columnSchema)
	{
		if (val is DBNull)
		{
			return null;
		}
		DbType dbTypeOfColumn = GetDbTypeOfColumn(columnSchema);
		switch (dbTypeOfColumn)
		{
		case DbType.Int32:
			if (val is short)
			{
				return (int)(short)val;
			}
			if (val is byte)
			{
				return (int)(byte)val;
			}
			if (val is long)
			{
				return (int)(long)val;
			}
			if (val is decimal)
			{
				return (int)(decimal)val;
			}
			break;
		case DbType.Int16:
			if (val is int)
			{
				return (short)(int)val;
			}
			if (val is byte)
			{
				return (short)(byte)val;
			}
			if (val is long)
			{
				return (short)(long)val;
			}
			if (val is decimal)
			{
				return (short)(decimal)val;
			}
			break;
		case DbType.Int64:
			if (val is int)
			{
				return (long)(int)val;
			}
			if (val is short)
			{
				return (long)(short)val;
			}
			if (val is byte)
			{
				return (long)(byte)val;
			}
			if (val is decimal)
			{
				return (long)(decimal)val;
			}
			break;
		case DbType.Single:
			if (val is double)
			{
				return (float)(double)val;
			}
			if (val is decimal)
			{
				return (float)(decimal)val;
			}
			break;
		case DbType.Double:
			if (val is float)
			{
				return (double)(float)val;
			}
			if (val is double)
			{
				return (double)val;
			}
			if (val is decimal)
			{
				return (double)(decimal)val;
			}
			break;
		case DbType.String:
			if (val is Guid guid)
			{
				return guid.ToString();
			}
			break;
		case DbType.Guid:
			if (val is string)
			{
				return ParseStringAsGuid((string)val);
			}
			if (val is byte[])
			{
				return ParseBlobAsGuid((byte[])val);
			}
			if (val is Guid)
			{
				return val.ToString();
			}
			break;
		default:
			throw new ArgumentException("Illegal database type [" + Enum.GetName(typeof(DbType), dbTypeOfColumn) + "]");
		case DbType.Binary:
		case DbType.Boolean:
		case DbType.DateTime:
			break;
		}
		return val;
	}

	private static Guid ParseBlobAsGuid(byte[] blob)
	{
		byte[] array = blob;
		if (blob.Length > 16)
		{
			array = new byte[16];
			for (int i = 0; i < 16; i++)
			{
				array[i] = blob[i];
			}
		}
		else if (blob.Length < 16)
		{
			array = new byte[16];
			for (int j = 0; j < blob.Length; j++)
			{
				array[j] = blob[j];
			}
		}
		return new Guid(array);
	}

	private static Guid ParseStringAsGuid(string str)
	{
		try
		{
			return new Guid(str);
		}
		catch (Exception)
		{
			return Guid.Empty;
		}
	}

	private static SQLiteCommand BuildSQLiteInsert(TableSchema ts)
	{
		SQLiteCommand sQLiteCommand = new SQLiteCommand();
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("INSERT INTO [" + ts.TableName + "] (");
		for (int i = 0; i < ts.Columns.Count; i++)
		{
			stringBuilder.Append("[" + ts.Columns[i].ColumnName + "]");
			if (i < ts.Columns.Count - 1)
			{
				stringBuilder.Append(", ");
			}
		}
		stringBuilder.Append(") VALUES (");
		List<string> list = new List<string>();
		for (int j = 0; j < ts.Columns.Count; j++)
		{
			string text = "@" + GetNormalizedName(ts.Columns[j].ColumnName, list);
			stringBuilder.Append(text);
			if (j < ts.Columns.Count - 1)
			{
				stringBuilder.Append(", ");
			}
			DbType dbTypeOfColumn = GetDbTypeOfColumn(ts.Columns[j]);
			SQLiteParameter parameter = new SQLiteParameter(text, dbTypeOfColumn, ts.Columns[j].ColumnName);
			sQLiteCommand.Parameters.Add(parameter);
			list.Add(text);
		}
		stringBuilder.Append(")");
		sQLiteCommand.CommandText = stringBuilder.ToString();
		sQLiteCommand.CommandType = CommandType.Text;
		return sQLiteCommand;
	}

	private static string GetNormalizedName(string str, List<string> names)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < str.Length; i++)
		{
			if (char.IsLetterOrDigit(str[i]) || str[i] == '_')
			{
				stringBuilder.Append(str[i]);
			}
			else
			{
				stringBuilder.Append("_");
			}
		}
		if (names.Contains(stringBuilder.ToString()))
		{
			return GetNormalizedName(stringBuilder.ToString() + "_", names);
		}
		return stringBuilder.ToString();
	}

	private static DbType GetDbTypeOfColumn(ColumnSchema cs)
	{
		if (cs.ColumnType == "tinyint")
		{
			return DbType.Byte;
		}
		if (cs.ColumnType == "int")
		{
			return DbType.Int32;
		}
		if (cs.ColumnType == "smallint")
		{
			return DbType.Int16;
		}
		if (cs.ColumnType == "bigint")
		{
			return DbType.Int64;
		}
		if (cs.ColumnType == "bit")
		{
			return DbType.Boolean;
		}
		if (cs.ColumnType == "nvarchar" || cs.ColumnType == "varchar" || cs.ColumnType == "text" || cs.ColumnType == "ntext")
		{
			return DbType.String;
		}
		if (cs.ColumnType == "float")
		{
			return DbType.Double;
		}
		if (cs.ColumnType == "real")
		{
			return DbType.Single;
		}
		if (cs.ColumnType == "blob")
		{
			return DbType.Binary;
		}
		if (cs.ColumnType == "numeric")
		{
			return DbType.Double;
		}
		if (cs.ColumnType == "timestamp" || cs.ColumnType == "datetime" || cs.ColumnType == "datetime2" || cs.ColumnType == "date" || cs.ColumnType == "time")
		{
			return DbType.DateTime;
		}
		if (cs.ColumnType == "nchar" || cs.ColumnType == "char")
		{
			return DbType.String;
		}
		if (cs.ColumnType == "uniqueidentifier" || cs.ColumnType == "guid")
		{
			return DbType.Guid;
		}
		if (cs.ColumnType == "xml")
		{
			return DbType.String;
		}
		if (cs.ColumnType == "sql_variant")
		{
			return DbType.Object;
		}
		if (cs.ColumnType == "integer")
		{
			return DbType.Int64;
		}
		throw new ApplicationException("Illegal DB type found (" + cs.ColumnType + ")");
	}

	private static string BuildSqlServerTableQuery(TableSchema ts)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("SELECT ");
		for (int i = 0; i < ts.Columns.Count; i++)
		{
			stringBuilder.Append("[" + ts.Columns[i].ColumnName + "]");
			if (i < ts.Columns.Count - 1)
			{
				stringBuilder.Append(", ");
			}
		}
		stringBuilder.Append(" FROM " + ts.TableSchemaName + ".[" + ts.TableName + "]");
		return stringBuilder.ToString();
	}

	private static void CreateSQLiteDatabase(string sqlitePath, DatabaseSchema schema, string password, SqlConversionHandler progressHandler, FailedViewDefinitionHandler viewFailureHandler, bool createViews)
	{
		SQLiteConnection.CreateFile(sqlitePath);
		string connectionString = CreateSQLiteConnectionString(sqlitePath, password);
		using SQLiteConnection sQLiteConnection = new SQLiteConnection(connectionString);
		sQLiteConnection.Open();
		int num = 0;
		foreach (TableSchema table in schema.Tables)
		{
			try
			{
				AddSQLiteTable(sQLiteConnection, table);
			}
			catch (Exception)
			{
				throw;
			}
			num++;
			CheckCancelled();
			progressHandler?.Invoke(new DefaultProgressInfo(done: false, success: true, (int)((double)num * 50.0 / (double)schema.Tables.Count), "Added table " + table.TableName + " to the SQLite database"));
		}
		num = 0;
		if (!createViews)
		{
			return;
		}
		foreach (ViewSchema view in schema.Views)
		{
			try
			{
				AddSQLiteView(sQLiteConnection, view, viewFailureHandler);
			}
			catch (Exception)
			{
				throw;
			}
			num++;
			CheckCancelled();
			progressHandler?.Invoke(new DefaultProgressInfo(done: false, success: true, 50 + (int)((double)num * 50.0 / (double)schema.Views.Count), "Added view " + view.ViewName + " to the SQLite database"));
		}
	}

	private static void AddSQLiteView(SQLiteConnection conn, ViewSchema vs, FailedViewDefinitionHandler handler)
	{
		string viewSQL = vs.ViewSQL;
		SQLiteTransaction sQLiteTransaction = conn.BeginTransaction();
		try
		{
			SQLiteCommand sQLiteCommand = new SQLiteCommand(viewSQL, conn, sQLiteTransaction);
			sQLiteCommand.ExecuteNonQuery();
			sQLiteTransaction.Commit();
		}
		catch (SQLiteException)
		{
			sQLiteTransaction.Rollback();
			if (handler != null)
			{
				ViewSchema viewSchema = new ViewSchema();
				viewSchema.ViewName = vs.ViewName;
				viewSchema.ViewSQL = vs.ViewSQL;
				string text = handler(viewSchema);
				if (text != null)
				{
					viewSchema.ViewSQL = text;
					AddSQLiteView(conn, viewSchema, handler);
				}
				return;
			}
			throw;
		}
	}

	private static void AddSQLiteTable(SQLiteConnection conn, TableSchema dt)
	{
		string commandText = BuildCreateTableQuery(dt);
		SQLiteCommand sQLiteCommand = new SQLiteCommand(commandText, conn);
		sQLiteCommand.ExecuteNonQuery();
	}

	private static string BuildCreateTableQuery(TableSchema ts)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("CREATE TABLE [" + ts.TableName + "] (\n");
		bool pkey = false;
		for (int i = 0; i < ts.Columns.Count; i++)
		{
			ColumnSchema col = ts.Columns[i];
			string value = BuildColumnStatement(col, ts, ref pkey);
			stringBuilder.Append(value);
			if (i < ts.Columns.Count - 1)
			{
				stringBuilder.Append(",\n");
			}
		}
		if (ts.PrimaryKey != null && ts.PrimaryKey.Count > 0 && !pkey)
		{
			stringBuilder.Append(",\n");
			stringBuilder.Append("    PRIMARY KEY (");
			for (int j = 0; j < ts.PrimaryKey.Count; j++)
			{
				stringBuilder.Append("[" + ts.PrimaryKey[j] + "]");
				if (j < ts.PrimaryKey.Count - 1)
				{
					stringBuilder.Append(", ");
				}
			}
			stringBuilder.Append(")\n");
		}
		else
		{
			stringBuilder.Append("\n");
		}
		if (ts.ForeignKeys != null && ts.ForeignKeys.Count > 0)
		{
			stringBuilder.Append(",\n");
			for (int k = 0; k < ts.ForeignKeys.Count; k++)
			{
				ForeignKeySchema foreignKeySchema = ts.ForeignKeys[k];
				string value2 = $"    FOREIGN KEY ([{foreignKeySchema.ColumnName}])\n        REFERENCES [{foreignKeySchema.ForeignTableName}]([{foreignKeySchema.ForeignColumnName}])";
				stringBuilder.Append(value2);
				if (k < ts.ForeignKeys.Count - 1)
				{
					stringBuilder.Append(",\n");
				}
			}
		}
		stringBuilder.Append("\n");
		stringBuilder.Append(");\n");
		if (ts.Indexes != null && ts.Indexes.Count > 0)
		{
			for (int l = 0; l < ts.Indexes.Count; l++)
			{
				string text = BuildCreateIndex(ts.TableName, ts.Indexes[l]);
				stringBuilder.Append(text + ";\n");
			}
		}
		return stringBuilder.ToString();
	}

	private static string BuildCreateIndex(string tableName, IndexSchema indexSchema)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("CREATE ");
		if (indexSchema.IsUnique)
		{
			stringBuilder.Append("UNIQUE ");
		}
		stringBuilder.Append("INDEX [" + tableName + "_" + indexSchema.IndexName + "]\n");
		stringBuilder.Append("ON [" + tableName + "]\n");
		stringBuilder.Append("(");
		for (int i = 0; i < indexSchema.Columns.Count; i++)
		{
			stringBuilder.Append("[" + indexSchema.Columns[i].ColumnName + "]");
			if (!indexSchema.Columns[i].IsAscending)
			{
				stringBuilder.Append(" DESC");
			}
			if (i < indexSchema.Columns.Count - 1)
			{
				stringBuilder.Append(", ");
			}
		}
		stringBuilder.Append(")");
		return stringBuilder.ToString();
	}

	private static string BuildColumnStatement(ColumnSchema col, TableSchema ts, ref bool pkey)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("\t[" + col.ColumnName + "]\t");
		if (col.IsIdentity)
		{
			if (ts.PrimaryKey != null && ts.PrimaryKey.Count == 1 && (col.ColumnType == "tinyint" || col.ColumnType == "int" || col.ColumnType == "smallint" || col.ColumnType == "bigint" || col.ColumnType == "integer"))
			{
				stringBuilder.Append("integer PRIMARY KEY AUTOINCREMENT");
				pkey = true;
			}
			else
			{
				stringBuilder.Append("integer");
			}
		}
		else
		{
			if (col.ColumnType == "int")
			{
				stringBuilder.Append("integer");
			}
			else
			{
				stringBuilder.Append(col.ColumnType);
			}
			if (col.Length > 0)
			{
				stringBuilder.Append("(" + col.Length + ")");
			}
		}
		if (!col.IsNullable)
		{
			stringBuilder.Append(" NOT NULL");
		}
		if (col.IsCaseSensitivite.HasValue && !col.IsCaseSensitivite.Value)
		{
			stringBuilder.Append(" COLLATE NOCASE");
		}
		string value = StripParens(col.DefaultValue);
		value = DiscardNational(value);
		if (value != string.Empty && value.ToUpper().Contains("GETDATE"))
		{
			stringBuilder.Append(" DEFAULT (CURRENT_TIMESTAMP)");
		}
		else if (value != string.Empty && IsValidDefaultValue(value))
		{
			stringBuilder.Append(" DEFAULT " + value);
		}
		return stringBuilder.ToString();
	}

	private static string DiscardNational(string value)
	{
		Regex regex = new Regex("N\\'([^\\']*)\\'");
		Match match = regex.Match(value);
		if (match.Success)
		{
			return match.Groups[1].Value;
		}
		return value;
	}

	private static bool IsValidDefaultValue(string value)
	{
		if (IsSingleQuoted(value))
		{
			return true;
		}
		if (!double.TryParse(value, out var _))
		{
			return false;
		}
		return true;
	}

	private static bool IsSingleQuoted(string value)
	{
		value = value.Trim();
		if (value.StartsWith("'") && value.EndsWith("'"))
		{
			return true;
		}
		return false;
	}

	private static string StripParens(string value)
	{
		Regex regex = new Regex("\\(([^\\)]*)\\)");
		Match match = regex.Match(value);
		if (!match.Success)
		{
			return value;
		}
		return StripParens(match.Groups[1].Value);
	}

	private static DatabaseSchema ReadSqlServerSchema(string connString, SqlConversionHandler progressHandler, Predicate<string> tableSelectionHandler, bool createPrimaryKey, bool createForeignKey, bool createCollate, bool createIndex, bool createViews)
	{
		DatabaseSchema databaseSchema = new DatabaseSchema();
		using (SqlConnection sqlConnection = new SqlConnection(connString))
		{
			sqlConnection.Open();
			List<TableSchema> list = new List<TableSchema>();
			List<string> list2 = new List<string>();
			List<string> list3 = new List<string>();
			SqlCommand sqlCommand = new SqlCommand("select * from INFORMATION_SCHEMA.TABLES  where TABLE_TYPE = 'BASE TABLE'", sqlConnection);
			using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
			{
				while (sqlDataReader.Read())
				{
					if (sqlDataReader["TABLE_NAME"] != DBNull.Value && sqlDataReader["TABLE_SCHEMA"] != DBNull.Value)
					{
						list2.Add((string)sqlDataReader["TABLE_NAME"]);
						list3.Add((string)sqlDataReader["TABLE_SCHEMA"]);
					}
				}
			}
			int num = 0;
			for (int i = 0; i < list2.Count; i++)
			{
				string text = list2[i];
				string tschma = list3[i];
				if (tableSelectionHandler == null || tableSelectionHandler(text))
				{
					TableSchema tableSchema = CreateTableSchema(sqlConnection, text, tschma, createPrimaryKey, createCollate, createIndex);
					if (createForeignKey)
					{
						CreateForeignKeySchema(sqlConnection, tableSchema);
					}
					list.Add(tableSchema);
					num++;
					CheckCancelled();
					progressHandler?.Invoke(new DefaultProgressInfo(done: false, success: true, (int)((double)num * 50.0 / (double)list2.Count), "Parsed table " + text));
				}
			}
			databaseSchema.Tables = list;
		}
		Regex regex = new Regex("dbo\\.", RegexOptions.IgnoreCase | RegexOptions.Compiled);
		if (createViews)
		{
			List<ViewSchema> list4 = new List<ViewSchema>();
			using (SqlConnection sqlConnection2 = new SqlConnection(connString))
			{
				sqlConnection2.Open();
				SqlCommand sqlCommand2 = new SqlCommand("SELECT TABLE_NAME, VIEW_DEFINITION  from INFORMATION_SCHEMA.VIEWS", sqlConnection2);
				using SqlDataReader sqlDataReader2 = sqlCommand2.ExecuteReader();
				int num2 = 0;
				while (sqlDataReader2.Read())
				{
					ViewSchema viewSchema = new ViewSchema();
					if (sqlDataReader2["TABLE_NAME"] != DBNull.Value && sqlDataReader2["VIEW_DEFINITION"] != DBNull.Value)
					{
						viewSchema.ViewName = (string)sqlDataReader2["TABLE_NAME"];
						viewSchema.ViewSQL = (string)sqlDataReader2["VIEW_DEFINITION"];
						viewSchema.ViewSQL = regex.Replace(viewSchema.ViewSQL, string.Empty);
						list4.Add(viewSchema);
						num2++;
						CheckCancelled();
						progressHandler?.Invoke(new DefaultProgressInfo(done: false, success: true, 50 + (int)((double)num2 * 50.0 / (double)list4.Count), "Parsed view " + viewSchema.ViewName));
					}
				}
			}
			databaseSchema.Views = list4;
		}
		return databaseSchema;
	}

	private static void CheckCancelled()
	{
		if (_cancelled)
		{
			throw new ApplicationException("User cancelled the conversion");
		}
	}

	private static TableSchema CreateTableSchema(SqlConnection conn, string tableName, string tschma, bool createPrimaryKey, bool createCollate, bool createIndex)
	{
		TableSchema tableSchema = new TableSchema();
		tableSchema.TableName = tableName;
		tableSchema.TableSchemaName = tschma;
		tableSchema.Columns = new List<ColumnSchema>();
		SqlCommand sqlCommand = new SqlCommand("SELECT COLUMN_NAME,COLUMN_DEFAULT,IS_NULLABLE,DATA_TYPE,  (columnproperty(object_id(TABLE_NAME), COLUMN_NAME, 'IsIdentity')) AS [IDENT], CHARACTER_MAXIMUM_LENGTH AS CSIZE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + tableName + "' ORDER BY ORDINAL_POSITION ASC", conn);
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
		{
			while (sqlDataReader.Read())
			{
				object obj = sqlDataReader["COLUMN_NAME"];
				if (obj is DBNull)
				{
					continue;
				}
				string columnName = (string)sqlDataReader["COLUMN_NAME"];
				obj = sqlDataReader["COLUMN_DEFAULT"];
				string text = ((!(obj is DBNull)) ? ((string)obj) : string.Empty);
				obj = sqlDataReader["IS_NULLABLE"];
				bool isNullable = (string)obj == "YES";
				string text2 = (string)sqlDataReader["DATA_TYPE"];
				bool isIdentity = false;
				if (sqlDataReader["IDENT"] != DBNull.Value)
				{
					isIdentity = (int)sqlDataReader["IDENT"] == 1;
				}
				int length = ((sqlDataReader["CSIZE"] != DBNull.Value) ? Convert.ToInt32(sqlDataReader["CSIZE"]) : 0);
				ValidateDataType(text2);
				switch (text2)
				{
				case "timestamp":
					text2 = "blob";
					break;
				default:
					if (!(text2 == "time"))
					{
						if (text2 == "decimal")
						{
							text2 = "numeric";
							break;
						}
						if (text2 == "money" || text2 == "smallmoney")
						{
							text2 = "numeric";
							break;
						}
						if (text2 == "binary" || text2 == "varbinary" || text2 == "image")
						{
							text2 = "blob";
							break;
						}
						switch (text2)
						{
						case "tinyint":
							text2 = "smallint";
							break;
						case "bigint":
							text2 = "bigint";
							break;
						case "sql_variant":
							text2 = "blob";
							break;
						case "xml":
							text2 = "varchar";
							break;
						case "uniqueidentifier":
							text2 = "varchar";
							length = 128;
							break;
						case "ntext":
							text2 = "text";
							break;
						case "nchar":
							text2 = "char";
							break;
						default:
							if (text2.Equals(SqlServerType.Geography) || text2.Equals(SqlServerType.Geometry))
							{
								text2 = "text";
							}
							break;
						}
						break;
					}
					goto case "datetime";
				case "datetime":
				case "smalldatetime":
				case "date":
				case "datetime2":
					text2 = "datetime";
					break;
				}
				if (text2 == "bit" || text2 == "int")
				{
					if (text == "('False')")
					{
						text = "(0)";
					}
					else if (text == "('True')")
					{
						text = "(1)";
					}
				}
				text = FixDefaultValueString(text);
				ColumnSchema columnSchema = new ColumnSchema();
				columnSchema.ColumnName = columnName;
				columnSchema.ColumnType = text2;
				columnSchema.Length = length;
				columnSchema.IsNullable = isNullable;
				columnSchema.IsIdentity = isIdentity;
				columnSchema.DefaultValue = AdjustDefaultValue(text);
				tableSchema.Columns.Add(columnSchema);
			}
		}
		if (createPrimaryKey)
		{
			SqlCommand sqlCommand2 = new SqlCommand("EXEC sp_pkeys '" + tableName + "'", conn);
			using SqlDataReader sqlDataReader2 = sqlCommand2.ExecuteReader();
			tableSchema.PrimaryKey = new List<string>();
			while (sqlDataReader2.Read())
			{
				string item = (string)sqlDataReader2["COLUMN_NAME"];
				tableSchema.PrimaryKey.Add(item);
			}
		}
		if (createCollate)
		{
			SqlCommand sqlCommand3 = new SqlCommand("EXEC sp_tablecollations '" + tschma + "." + tableName + "'", conn);
			using SqlDataReader sqlDataReader3 = sqlCommand3.ExecuteReader();
			while (sqlDataReader3.Read())
			{
				bool? isCaseSensitivite = null;
				string text3 = (string)sqlDataReader3["name"];
				if (sqlDataReader3["tds_collation"] != DBNull.Value)
				{
					byte[] array = (byte[])sqlDataReader3["tds_collation"];
					isCaseSensitivite = (((array[2] & 0x10) == 0) ? new bool?(true) : new bool?(false));
				}
				if (!isCaseSensitivite.HasValue)
				{
					continue;
				}
				foreach (ColumnSchema column in tableSchema.Columns)
				{
					if (column.ColumnName == text3)
					{
						column.IsCaseSensitivite = isCaseSensitivite;
						break;
					}
				}
			}
		}
		if (createIndex)
		{
			try
			{
				SqlCommand sqlCommand4 = new SqlCommand("exec sp_helpindex '" + tschma + "." + tableName + "'", conn);
				using SqlDataReader sqlDataReader4 = sqlCommand4.ExecuteReader();
				tableSchema.Indexes = new List<IndexSchema>();
				while (sqlDataReader4.Read())
				{
					string indexName = (string)sqlDataReader4["index_name"];
					string text4 = (string)sqlDataReader4["index_description"];
					string keys = (string)sqlDataReader4["index_keys"];
					if (!text4.Contains("primary key"))
					{
						IndexSchema item2 = BuildIndexSchema(indexName, text4, keys);
						tableSchema.Indexes.Add(item2);
					}
				}
			}
			catch (Exception)
			{
			}
		}
		return tableSchema;
	}

	private static void ValidateDataType(string dataType)
	{
		switch (dataType)
		{
		case "smallint":
			return;
		case "bit":
			return;
		case "float":
			return;
		case "real":
			return;
		case "nvarchar":
			return;
		case "varchar":
			return;
		case "timestamp":
			return;
		case "varbinary":
			return;
		case "image":
			return;
		case "text":
			return;
		case "ntext":
			return;
		case "bigint":
			return;
		case "char":
			return;
		case "numeric":
			return;
		case "binary":
			return;
		case "smalldatetime":
			return;
		case "smallmoney":
			return;
		case "money":
			return;
		case "tinyint":
			return;
		case "uniqueidentifier":
			return;
		case "xml":
			return;
		case "sql_variant":
			return;
		case "datetime2":
			return;
		case "date":
			return;
		case "time":
			return;
		case "decimal":
			return;
		case "nchar":
			return;
		case "datetime":
			return;
		}
		if (dataType == SqlServerType.Geography || dataType == SqlServerType.Geometry)
		{
			return;
		}
		throw new ApplicationException("Validation failed for data type [" + dataType + "]");
	}

	private static string FixDefaultValueString(string colDefault)
	{
		bool flag = false;
		string text = colDefault.Trim();
		int num = -1;
		int num2 = -1;
		for (int i = 0; i < text.Length; i++)
		{
			if (text[i] == '\'' && num == -1)
			{
				num = i;
			}
			if (text[i] == '\'' && num != -1 && i > num2)
			{
				num2 = i;
			}
		}
		if (num != -1 && num2 > num)
		{
			return text.Substring(num, num2 - num + 1);
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int j = 0; j < text.Length; j++)
		{
			if (text[j] != '(' && text[j] != ')')
			{
				stringBuilder.Append(text[j]);
				flag = true;
			}
		}
		if (flag)
		{
			return "(" + stringBuilder.ToString() + ")";
		}
		return stringBuilder.ToString();
	}

	private static void CreateForeignKeySchema(SqlConnection conn, TableSchema ts)
	{
		ts.ForeignKeys = new List<ForeignKeySchema>();
		SqlCommand sqlCommand = new SqlCommand("SELECT   ColumnName = CU.COLUMN_NAME,   ForeignTableName  = PK.TABLE_NAME,   ForeignColumnName = PT.COLUMN_NAME,   DeleteRule = C.DELETE_RULE,   IsNullable = COL.IS_NULLABLE FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS C INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS FK ON C.CONSTRAINT_NAME = FK.CONSTRAINT_NAME INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS PK ON C.UNIQUE_CONSTRAINT_NAME = PK.CONSTRAINT_NAME INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE CU ON C.CONSTRAINT_NAME = CU.CONSTRAINT_NAME INNER JOIN   (     SELECT i1.TABLE_NAME, i2.COLUMN_NAME     FROM  INFORMATION_SCHEMA.TABLE_CONSTRAINTS i1     INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE i2 ON i1.CONSTRAINT_NAME = i2.CONSTRAINT_NAME     WHERE i1.CONSTRAINT_TYPE = 'PRIMARY KEY'   ) PT ON PT.TABLE_NAME = PK.TABLE_NAME INNER JOIN INFORMATION_SCHEMA.COLUMNS AS COL ON CU.COLUMN_NAME = COL.COLUMN_NAME AND FK.TABLE_NAME = COL.TABLE_NAME WHERE FK.Table_NAME='" + ts.TableName + "'", conn);
		using SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
		while (sqlDataReader.Read())
		{
			ForeignKeySchema foreignKeySchema = new ForeignKeySchema();
			foreignKeySchema.ColumnName = (string)sqlDataReader["ColumnName"];
			foreignKeySchema.ForeignTableName = (string)sqlDataReader["ForeignTableName"];
			foreignKeySchema.ForeignColumnName = (string)sqlDataReader["ForeignColumnName"];
			foreignKeySchema.CascadeOnDelete = (string)sqlDataReader["DeleteRule"] == "CASCADE";
			foreignKeySchema.IsNullable = (string)sqlDataReader["IsNullable"] == "YES";
			foreignKeySchema.TableName = ts.TableName;
			ts.ForeignKeys.Add(foreignKeySchema);
		}
	}

	private static IndexSchema BuildIndexSchema(string indexName, string desc, string keys)
	{
		IndexSchema indexSchema = new IndexSchema();
		indexSchema.IndexName = indexName;
		string[] array = desc.Split(',');
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (text.Trim().Contains("unique"))
			{
				indexSchema.IsUnique = true;
				break;
			}
		}
		indexSchema.Columns = new List<IndexColumn>();
		string[] array3 = keys.Split(',');
		string[] array4 = array3;
		foreach (string text2 in array4)
		{
			Match match = _keyRx.Match(text2.Trim());
			if (!match.Success)
			{
				throw new ApplicationException("Illegal key name [" + text2 + "] in index [" + indexName + "]");
			}
			string value = match.Groups[1].Value;
			IndexColumn indexColumn = new IndexColumn();
			indexColumn.ColumnName = value;
			if (match.Groups[2].Success)
			{
				indexColumn.IsAscending = false;
			}
			else
			{
				indexColumn.IsAscending = true;
			}
			indexSchema.Columns.Add(indexColumn);
		}
		return indexSchema;
	}

	private static string AdjustDefaultValue(string val)
	{
		if (val == null || val == string.Empty)
		{
			return val;
		}
		Match match = _defaultValueRx.Match(val);
		if (match.Success)
		{
			return match.Groups[1].Value;
		}
		return val;
	}

	private static string CreateSQLiteConnectionString(string sqlitePath, string password)
	{
		SQLiteConnectionStringBuilder sQLiteConnectionStringBuilder = new SQLiteConnectionStringBuilder();
		sQLiteConnectionStringBuilder.DataSource = sqlitePath;
		if (password != null)
		{
			sQLiteConnectionStringBuilder.Password = password;
		}
		sQLiteConnectionStringBuilder.PageSize = 4096;
		sQLiteConnectionStringBuilder.UseUTF16Encoding = true;
		return sQLiteConnectionStringBuilder.ConnectionString;
	}

	private static void AddTriggersForForeignKeys(string sqlitePath, IEnumerable<TableSchema> schema, string password, SqlConversionHandler handler)
	{
		string connectionString = CreateSQLiteConnectionString(sqlitePath, password);
		using SQLiteConnection sQLiteConnection = new SQLiteConnection(connectionString);
		sQLiteConnection.Open();
		foreach (TableSchema item in schema)
		{
			try
			{
				AddTableTriggers(sQLiteConnection, item);
			}
			catch (Exception)
			{
				throw;
			}
		}
	}

	private static void AddTableTriggers(SQLiteConnection conn, TableSchema dt)
	{
		IList<TriggerSchema> foreignKeyTriggers = TriggerBuilder.GetForeignKeyTriggers(dt);
		foreach (TriggerSchema item in foreignKeyTriggers)
		{
			SQLiteCommand sQLiteCommand = new SQLiteCommand(WriteTriggerSchema(item), conn);
			sQLiteCommand.ExecuteNonQuery();
		}
	}

	private static string WriteTriggerSchema(TriggerSchema ts)
	{
		return "CREATE TRIGGER [" + ts.Name + "] " + ts.Type.ToString() + " " + ts.Event.ToString() + " ON [" + ts.Table + "] BEGIN " + ts.Body + " END;";
	}
}
