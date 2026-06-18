using System;
using System.Collections.Generic;
using System.Data.SQLite;
using Dapper;

namespace Leqisoft.UI.Platform.Chat;

public class ChatRecordDAL
{
	private readonly SQLiteConnectionStringBuilder connectionStringBuilder;

	private DateTime baseTime = new DateTime(1970, 1, 1);

	public ChatRecordDAL()
	{
		connectionStringBuilder = new SQLiteConnectionStringBuilder();
	}

	public void Insert(ChatRecord record)
	{
		using SQLiteConnection cnn = GetConnection();
		long timestamp = GetTimestamp(record.CreateTime);
		string sql = $"INSERT INTO `ChatRecord`(`ChatId`,`FromId`,`Message`,`CreateTime`) VALUES('{record.ChatId}','{record.FromId}','{record.Message}',{timestamp})";
		cnn.Execute(sql);
	}

	public IEnumerable<ChatRecord> GetRecords(string chatId, DateTime start, DateTime end, int count = 20)
	{
		using SQLiteConnection cnn = GetConnection();
		long timestamp = GetTimestamp(start);
		long timestamp2 = GetTimestamp(end);
		string sql = $"SELECT `ChatId`,`FromId`,`Message`,`CreateTime` FROM `ChatRecord` WHERE `ChatId`='{chatId}' AND `CREATETIME`>{timestamp} AND `CREATETIME`<{timestamp2} ORDER BY `CREATETIME` DESC LIMIT {count};";
		List<ChatRecord> list = new List<ChatRecord>();
		foreach (dynamic item in cnn.Query(sql))
		{
			list.Add(new ChatRecord
			{
				ChatId = (string)item.ChatId,
				FromId = (string)item.FromId,
				CreateTime = baseTime.AddMilliseconds((long)item.CreateTime),
				Message = (string)item.Message
			});
		}
		return list;
	}

	public void Connect(string dbfile)
	{
		connectionStringBuilder.JournalMode = SQLiteJournalModeEnum.Wal;
		connectionStringBuilder.SyncMode = SynchronizationModes.Off;
		connectionStringBuilder.DataSource = dbfile;
		SetPragma();
		CreateConfig();
		UpdateSchema();
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

	private void CreateConfig()
	{
		using SQLiteConnection cnn = GetConnection();
		cnn.Execute("CREATE TABLE IF NOT EXISTS `ChatRecord`(\r\n                            `ChatId` TEXT,\r\n                            `FromId` TEXT,\r\n                            `Message` TEXT,\r\n                            `CreateTime` INTEGER);");
	}

	private void UpdateSchema()
	{
		using SQLiteConnection cnn = GetConnection();
		int num = cnn.ExecuteScalar<int>("PRAGMA user_version;");
		if (num == 0)
		{
			num = 1;
		}
		cnn.Execute($"PRAGMA user_version={num}");
	}

	private long GetTimestamp(DateTime time)
	{
		return (long)(time - baseTime).TotalMilliseconds;
	}
}
