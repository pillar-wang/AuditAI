using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Auditai.UI.Platform;

public class SqlitePersistence : IRecordPersistence
{
	private string _connectionString;
	private readonly List<object> _records = new List<object>();

	public void AddRecord(params object[] args)
	{
		try
		{
			if (args.Length > 0 && args[0] != null)
			{
				_records.Add(args[0]);
			}
		}
		catch
		{
			// 添加记录失败时静默处理
		}
	}

	public void Save(params object[] args)
	{
		try
		{
			string dbPath = null;
			if (args.Length > 0 && args[0] is string path)
			{
				dbPath = path;
			}
			else
			{
				dbPath = _connectionString?.Replace("Data Source=", "")?.TrimEnd(';');
			}

			if (string.IsNullOrEmpty(dbPath)) return;

			EnsureTable(dbPath);

			using var conn = new SQLiteConnection($"Data Source={dbPath};Version=3;");
			conn.Open();
			using var cmd = new SQLiteCommand("INSERT OR REPLACE INTO Records (Id, Data) VALUES (@id, @data)", conn);
			foreach (var record in _records)
			{
				var id = record.GetType().GetProperty("Id")?.GetValue(record)?.ToString() ?? Guid.NewGuid().ToString();
				var json = JsonConvert.SerializeObject(record);
				cmd.Parameters.Clear();
				cmd.Parameters.AddWithValue("@id", id);
				cmd.Parameters.AddWithValue("@data", json);
				cmd.ExecuteNonQuery();
			}
		}
		catch
		{
			// 保存失败时静默处理
		}
	}

	public void Load(params object[] args)
	{
		try
		{
			string dbPath = null;
			if (args.Length > 0 && args[0] is string path)
			{
				dbPath = path;
			}

			if (string.IsNullOrEmpty(dbPath) || !File.Exists(dbPath))
			{
				_connectionString = dbPath != null ? $"Data Source={dbPath};Version=3;" : null;
				return;
			}

			_connectionString = $"Data Source={dbPath};Version=3;";

			using var conn = new SQLiteConnection(_connectionString);
			conn.Open();
			using var cmd = new SQLiteCommand("SELECT Data FROM Records", conn);
			using var reader = cmd.ExecuteReader();
			while (reader.Read())
			{
				string json = reader.GetString(0);
				_records.Add(json);
			}
		}
		catch
		{
			// 加载失败时静默处理
		}
	}

	public IEnumerable<object> GetRecords(params object[] args)
	{
		try
		{
			if (_records.Count > 0) return _records.AsEnumerable();
		}
		catch { }
		return Enumerable.Empty<object>();
	}

	private void EnsureTable(string dbPath)
	{
		string dir = Path.GetDirectoryName(dbPath);
		if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
		{
			Directory.CreateDirectory(dir);
		}

		using var conn = new SQLiteConnection($"Data Source={dbPath};Version=3;");
		conn.Open();
		using var cmd = new SQLiteCommand("CREATE TABLE IF NOT EXISTS Records (Id TEXT PRIMARY KEY, Data TEXT)", conn);
		cmd.ExecuteNonQuery();
	}
}