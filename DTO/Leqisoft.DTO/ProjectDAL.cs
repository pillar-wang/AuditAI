﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;
using Dapper;
using Newtonsoft.Json;

namespace Leqisoft.DTO;

public class ProjectDAL
{
	private readonly SQLiteConnectionStringBuilder connectionStringBuilder = new SQLiteConnectionStringBuilder();

	private int _transactionDepth;

	private SQLiteTransaction _transaction;

	public static readonly string DefaultPermissions;

	static ProjectDAL()
	{
		DefaultPermissions = JsonConvert.SerializeObject(new
		{
			Read = new
			{
				GrantAll = true
			},
			Write = new
			{
				GrantAll = true
			},
			Schema = new
			{
				GrantAll = true
			}
		});
		SqlMapper.AddTypeHandler(new BinaryValueDapperHandler());
		SqlMapper.AddTypeHandler(new Id64DapperHandler());
	}

	public ProjectDAL(string fileName)
	{
		connectionStringBuilder.JournalMode = SQLiteJournalModeEnum.Wal;
		connectionStringBuilder.SyncMode = SynchronizationModes.Off;
		connectionStringBuilder.DataSource = fileName;
		SetPragma();
		CreateConfig();
		UpdateSchema();
	}

	public void BeginTransaction()
	{
		if (_transactionDepth == 0)
		{
			_transaction = GetConnection().BeginTransaction();
		}
		_transactionDepth++;
	}

	public void Execute(string sql, object param = null)
	{
		if (_transactionDepth == 0)
		{
			using (SQLiteConnection sQLiteConnection = GetConnection())
			{
				using SQLiteTransaction sQLiteTransaction = sQLiteConnection.BeginTransaction();
				sQLiteTransaction.Connection.Execute(sql, param, sQLiteTransaction);
				sQLiteTransaction.Commit();
				return;
			}
		}
		_transaction.Connection.Execute(sql, param, _transaction);
	}

	public void Commit()
	{
		_transactionDepth--;
		if (_transactionDepth == 0)
		{
			SQLiteConnection connection = _transaction.Connection;
			_transaction.Commit();
			connection.Close();
			_transaction = null;
		}
	}

	public void Rollback()
	{
		_transactionDepth--;
		if (_transactionDepth == 0)
		{
			SQLiteConnection connection = _transaction.Connection;
			_transaction.Rollback();
			connection.Close();
			_transaction = null;
		}
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

	public void CreateConfig()
	{
		using SQLiteConnection cnn = GetConnection();
		cnn.Execute("CREATE TABLE IF NOT EXISTS `Project`(\r\n`Id` GUID PRIMARY KEY,\r\n`Name` TEXT NOT NULL,\r\n`Parent` GUID,\r\n`Version` INTEGER NOT NULL DEFAULT 0,\r\n`Number` TEXT NOT NULL,\r\n`Category` TEXT NOT NULL,\r\n`Note` TEXT NOT NULL);\r\n\r\nCREATE TABLE IF NOT EXISTS `TreeGroup`(\r\n`Id` INTEGER PRIMARY KEY,\r\n`Name` TEXT NOT NULL,\r\n`Index` INTEGER NOT NULL,\r\n`ServerIndex` INTEGER NOT NULL,\r\n`Status` INTEGER NOT NULL,\r\n`Dirty` INTEGER NOT NULL);\r\n\r\nCREATE TABLE IF NOT EXISTS `TreeNode`(\r\n`Id` INTEGER PRIMARY KEY,\r\n`GroupId` INTEGER NOT NULL,\r\n`ParentId` INTEGER,\r\n`Name` TEXT NOT NULL,\r\n`Status` INTEGER NOT NULL,\r\n`Dirty` INTEGER NOT NULL,\r\n`Index` INTEGER NOT NULL,\r\n`ServerIndex` INTEGER NOT NULL,\r\n`Type` INTEGER NOT NULL,\r\n`Level` INTEGER NOT NULL,\r\n`Version` INTEGER NOT NULL);\r\n\r\nCREATE TABLE IF NOT EXISTS `Table`(\r\n`Id` INTEGER PRIMARY KEY,\r\n`Title` TEXT NOT NULL,\r\n`PageSetup` TEXT NOT NULL,\r\n`Dirty` INTEGER NOT NULL,\r\n`Note` TEXT NOT NULL,\r\n`HeaderHeights` TEXT,\r\n`DefaultStyleId` INTEGER NOT NULL,\r\n`ConsolidateSettings` TEXT);\r\n\r\nCREATE TABLE IF NOT EXISTS `Column`(\r\n`Id` INTEGER PRIMARY KEY,\r\n`TableId` INTEGER NOT NULL,\r\n`Index` INTEGER NOT NULL,\r\n`ServerIndex` INTEGER NOT NULL,\r\n`Caption` TEXT NOT NULL,\r\n`CaptionStyle` TEXT NOT NULL,\r\n`Width` INTEGER NOT NULL,\r\n`Visible` INTEGER NOT NULL,\r\n`Dirty` INTEGER NOT NULL,\r\n`Status` INTEGER NOT NULL,\r\n`ConsolidateAttribs` TEXT,\r\n`SubtotalAttribs` INTEGER NOT NULL DEFAULT 0,\r\n`Formula` TEXT,\r\n`StyleId` INTEGER);\r\n\r\nCREATE TABLE IF NOT EXISTS `Row`(\r\n`Id` INTEGER PRIMARY KEY,\r\n`TableId` INTEGER NOT NULL,\r\n`Index` INTEGER NOT NULL,\r\n`ServerIndex` INTEGER NOT NULL,\r\n`Height` INTEGER NOT NULL,\r\n`Visible` INTEGER NOT NULL,\r\n`Dirty` INTEGER NOT NULL,\r\n`Status` INTEGER NOT NULL);\r\n\r\nCREATE TABLE IF NOT EXISTS `Cell`(\r\n`Id` INTEGER PRIMARY KEY,\r\n`RowId` INTEGER NOT NULL,\r\n`ColumnId` INTEGER NOT NULL,\r\n`Value` BLOB NOT NULL,\r\n`StyleId` INTEGER,\r\n`Dirty` INTEGER NOT NULL,\r\n`Status` INTEGER NOT NULL,\r\n`Formula` TEXT);\r\n\r\nCREATE TABLE IF NOT EXISTS `CellStyle`(\r\n`Id` INTEGER PRIMARY KEY,\r\n`TableId` INTEGER NOT NULL,\r\n`FontFamily` TEXT,\r\n`FontSize` INTEGER,\r\n`ForeColor` INTEGER,\r\n`BackColor` INTEGER,\r\n`Align` INTEGER,\r\n`Margin` INTEGER,\r\n`Bold` INTEGER,\r\n`Italic` INTEGER,\r\n`Underline` INTEGER,\r\n`DataType` INTEGER,\r\n`Format` TEXT,\r\n`Status` INTEGER NOT NULL);\r\n\r\nCREATE TABLE IF NOT EXISTS `Document`(\r\n`Id` INTEGER PRIMARY KEY);\r\n\r\nCREATE TABLE IF NOT EXISTS `Paragraph`(\r\n`Id` INTEGER PRIMARY KEY,\r\n`DocumentId` INTEGER NOT NULL,\r\n`Index` INTEGER NOT NULL,\r\n`Stream` BLOB NOT NULL,\r\n`ServerIndex` INTEGER NOT NULL,\r\n`Dirty` INTEGER NOT NULL,\r\n`Status` INTEGER NOT NULL);\r\n\r\nCREATE TABLE IF NOT EXISTS `Merge`(\r\n`Id` INTEGER PRIMARY KEY,\r\n`TableId` INTEGER NOT NULL,\r\n`TopLeft` INTEGER NOT NULL,\r\n`BottomRight` INTEGER NOT NULL,\r\n`Status` INTEGER NOT NULL);\r\n\r\nCREATE TABLE IF NOT EXISTS `DataReference`(\r\n`Id` INTEGER PRIMARY KEY,\r\n`Key` TEXT NOT NULL,\r\n`Value` TEXT,\r\n`Status` INTEGER NOT NULL,\r\n`Dirty` INTEGER NOT NULL);\r\n\r\nCREATE TABLE IF NOT EXISTS `ValidationFormula`(\r\n`Id` INTEGER PRIMARY KEY,\r\n`LeftExpr` TEXT NOT NULL,\r\n`Operator` INT NOT NULL,\r\n`RightExpr` TEXT NOT NULL,\r\n`Note` TEXT NOT NULL,\r\n`Status` INTEGER NOT NULL,\r\n`Dirty` INTEGER NOT NULL)");
	}

	private void UpdateSchema()
	{
		using SQLiteConnection sQLiteConnection = GetConnection();
		int num = sQLiteConnection.ExecuteScalar<int>("PRAGMA user_version;");
		if (num == 0)
		{
			num = 1;
		}
		if (num == 1)
		{
			num = 2;
			sQLiteConnection.Execute("ALTER TABLE `Table` ADD COLUMN `BorderStyle` INTEGER NOT NULL DEFAULT 0");
		}
		if (num == 2)
		{
			num = 3;
			sQLiteConnection.Execute("ALTER TABLE `Row` ADD COLUMN `Locked` INTEGER");
			sQLiteConnection.Execute("ALTER TABLE `CellStyle` ADD COLUMN `Locked` INTEGER");
		}
		if (num == 3)
		{
			num = 4;
			sQLiteConnection.Execute("ALTER TABLE `Table` ADD COLUMN `FrozenCols` INTEGER NOT NULL DEFAULT 0");
		}
		if (num == 4)
		{
			num = 5;
			sQLiteConnection.Execute("ALTER TABLE `Table` ADD COLUMN `HeaderMode` INTEGER NOT NULL DEFAULT 0");
		}
		if (num == 5)
		{
			num = 6;
			sQLiteConnection.Execute("ALTER TABLE `Cell` ADD COLUMN `CollectSource` TEXT");
		}
		if (num == 6)
		{
			num = 7;
			sQLiteConnection.Execute("ALTER TABLE `Table` ADD COLUMN `CollectSource` TEXT");
		}
		if (num == 7)
		{
			num = 8;
			sQLiteConnection.Execute("ALTER TABLE `TreeNode` ADD COLUMN `Number` TEXT");
		}
		if (num == 8)
		{
			num = 9;
			sQLiteConnection.Execute("ALTER TABLE `Table` ADD COLUMN `Locker` INTEGER");
		}
		if (num == 9)
		{
			num = 10;
			sQLiteConnection.Execute("ALTER TABLE `Document` ADD COLUMN `Locker` INTEGER");
		}
		if (num == 10)
		{
			num = 11;
			sQLiteConnection.Execute("ALTER TABLE `Row` ADD COLUMN `Role` INTEGER");
		}
		if (num == 11)
		{
			num = 12;
			sQLiteConnection.Execute("ALTER TABLE `Document` ADD COLUMN `SectPr` TEXT");
		}
		if (num == 12)
		{
			num = 13;
			sQLiteConnection.Execute("ALTER TABLE `Table` ADD COLUMN `FilterInfo` TEXT");
		}
		if (num == 13)
		{
			num = 14;
			sQLiteConnection.Execute("ALTER TABLE `ValidationFormula` ADD COLUMN `TableId` INTEGER");
		}
		if (num == 14)
		{
			num = 15;
			sQLiteConnection.Execute("ALTER TABLE `Paragraph` ADD COLUMN `Section` BLOB");
		}
		if (num == 15)
		{
			num = 16;
			sQLiteConnection.Execute("ALTER TABLE `Document` ADD COLUMN `MergeTable` INTEGER NOT NULL DEFAULT 0");
		}
		if (num == 16)
		{
			num = 17;
			sQLiteConnection.Execute("ALTER TABLE `DataReference` ADD COLUMN `Kind` INTEGER NOT NULL DEFAULT 2");
		}
		if (num == 17)
		{
			num = 18;
			sQLiteConnection.Execute("ALTER TABLE `CellStyle` ADD COLUMN `DefaultValue` TEXT");
			sQLiteConnection.Execute("ALTER TABLE `CellStyle` ADD COLUMN `Comment` TEXT");
		}
		if (num == 18)
		{
			num = 19;
			sQLiteConnection.Execute("ALTER TABLE `Paragraph` ADD COLUMN `Comment` TEXT");
		}
		if (num == 19)
		{
			num = 20;
			sQLiteConnection.Execute("CREATE TABLE IF NOT EXISTS `CellStyle_temp`(\r\n`Id` INTEGER PRIMARY KEY,\r\n`TableId` INTEGER NOT NULL,\r\n`FontFamily` TEXT,\r\n`FontSize` REAL,\r\n`ForeColor` INTEGER,\r\n`BackColor` INTEGER,\r\n`Align` INTEGER,\r\n`Margin` INTEGER,\r\n`Bold` INTEGER,\r\n`Italic` INTEGER,\r\n`Underline` INTEGER,\r\n`DataType` INTEGER,\r\n`Format` TEXT,\r\n`Status` INTEGER NOT NULL,\r\n`Locked` INTEGER,\r\n`DefaultValue` TEXT,\r\n`Comment` TEXT);");
			sQLiteConnection.Execute("INSERT INTO `CellStyle_temp` SELECT * FROM `CellStyle`");
			sQLiteConnection.Execute("DROP TABLE `CellStyle`");
			sQLiteConnection.Execute("ALTER TABLE `CellStyle_temp` RENAME TO `CellStyle`");
		}
		if (num == 20)
		{
			num = 21;
			sQLiteConnection.Execute("CREATE TABLE IF NOT EXISTS `Image`(\r\n`Id` INTEGER PRIMARY KEY,\r\n`FileId` GUID NOT NULL);");
		}
		if (num == 21)
		{
			num = 22;
			sQLiteConnection.Execute("CREATE TABLE IF NOT EXISTS `Pdf`(\r\n`Id` INTEGER PRIMARY KEY,\r\n`FileId` GUID NOT NULL);");
		}
		if (num == 22)
		{
			num = 23;
			sQLiteConnection.Execute("ALTER TABLE `Document` ADD COLUMN `Dirty` INTEGER NOT NULL DEFAULT 0");
			sQLiteConnection.Execute("ALTER TABLE `Image` ADD COLUMN `Dirty` INTEGER NOT NULL DEFAULT 0");
			sQLiteConnection.Execute("ALTER TABLE `Image` ADD COLUMN `CenterX` REAL NOT NULL DEFAULT 0.5");
			sQLiteConnection.Execute("ALTER TABLE `Image` ADD COLUMN `CenterY` REAL NOT NULL DEFAULT 0.5");
			sQLiteConnection.Execute("ALTER TABLE `Image` ADD COLUMN `ZoomFactor` REAL NOT NULL DEFAULT 1.0");
			sQLiteConnection.Execute("ALTER TABLE `Image` ADD COLUMN `PageSetup` TEXT");
		}
		if (num == 23)
		{
			num = 24;
			sQLiteConnection.Execute("CREATE TABLE IF NOT EXISTS `Snapshot`(\r\n`Id` INTEGER PRIMARY KEY,\r\n`TreeNodeId` INTEGER NOT NULL,\r\n`DateTime` TEXT NOT NULL,\r\n`Size` INTEGER NOT NULL,\r\n`Kind` INTEGER NOT NULL,\r\n`Name` TEXT NOT NULL,\r\n`Deleted` INTEGER NOT NULL); ");
		}
		if (num == 24)
		{
			num = 25;
			sQLiteConnection.Execute("ALTER TABLE `project` ADD COLUMN `CreateTime` TEXT NOT NULL DEFAULT '2000-01-01'");
		}
		if (num == 25)
		{
			num = 26;
			sQLiteConnection.Execute("ALTER TABLE `TreeNode` ADD COLUMN `Permissions` TEXT NOT NULL DEFAULT '" + DefaultPermissions + "'");
			sQLiteConnection.Execute("ALTER TABLE `Column` ADD COLUMN `Permissions` TEXT NOT NULL DEFAULT '" + DefaultPermissions + "'");
			sQLiteConnection.Execute("ALTER TABLE `Row` ADD COLUMN `Permissions` TEXT NOT NULL DEFAULT '" + DefaultPermissions + "'");
		}
		if (num == 26)
		{
			num = 27;
			sQLiteConnection.Execute("ALTER TABLE `TreeNode` ADD COLUMN `Visible` INTEGER NOT NULL DEFAULT 1");
		}
		if (num == 27)
		{
			num = 28;
			sQLiteConnection.Execute("ALTER TABLE `Table` ADD COLUMN `Foot` TEXT NOT NULL DEFAULT '{}'");
		}
		if (num == 28)
		{
			num = 29;
			sQLiteConnection.Execute("CREATE INDEX `idx_Cell_RowId` ON `Cell` (`RowId`)");
			sQLiteConnection.Execute("CREATE INDEX `idx_Row_TableId` ON `Row` (`TableId`)");
		}
		if (num == 29)
		{
			num = 30;
			sQLiteConnection.Execute("ALTER TABLE `Image` ADD COLUMN `RotateFlip` INTEGER NOT NULL DEFAULT 0");
		}
		if (num == 30)
		{
			num = 31;
			sQLiteConnection.Execute("ALTER TABLE `Table` ADD COLUMN `RowOwnerExclusive` INTEGER NOT NULL DEFAULT 0");
		}
		if (num == 31)
		{
			num = 32;
			sQLiteConnection.Execute("ALTER TABLE `Row` ADD COLUMN `Creator` INTEGER NOT NULL DEFAULT 0");
		}
		if (num == 32)
		{
			num = 33;
			sQLiteConnection.Execute("ALTER TABLE `Column` ADD COLUMN `CaptionFormula` TEXT DEFAULT ''");
		}
		if (num == 33)
		{
			num = 34;
			sQLiteConnection.Execute("ALTER TABLE `Cell` ADD COLUMN `HeaderFormula` TEXT DEFAULT ''");
		}
		if (num == 34)
		{
			num = 35;
			sQLiteConnection.Execute("UPDATE `Paragraph` SET `Comment`='' WHERE `Comment` IS NULL");
		}
		if (num == 35)
		{
			num = 36;
			sQLiteConnection.Execute("UPDATE `Document` SET `Locker`=0 WHERE `Locker` IS NULL");
		}
		if (num == 36)
		{
			num = 37;
			sQLiteConnection.Execute("ALTER TABLE `Table` ADD COLUMN `RowOwnerLoad` INTEGER NOT NULL DEFAULT 0");
		}
		if (num == 37)
		{
			num = 38;
			sQLiteConnection.Execute("ALTER TABLE `Table` ADD COLUMN `RowOwnerLoadShare` BLOB NOT NULL DEFAULT ''");
		}
		if (num == 38)
		{
			num = 39;
			sQLiteConnection.Execute("ALTER TABLE `Column` ADD COLUMN `CrossAttributes` BLOB NOT NULL DEFAULT ''");
		}
		if (num == 39)
		{
			num = 40;
			sQLiteConnection.Execute("ALTER TABLE `TreeNode` ADD COLUMN `RowWrite` INTEGER NOT NULL DEFAULT 0");
			sQLiteConnection.Execute("ALTER TABLE `TreeNode` ADD COLUMN `RowRead` INTEGER NOT NULL DEFAULT 0");
			IEnumerable<object> enumerable = sQLiteConnection.Query("SELECT `Id`,`RowOwnerExclusive`,`RowOwnerLoad` FROM `Table`");
			SQLiteTransaction sQLiteTransaction = sQLiteConnection.BeginTransaction();
			foreach (dynamic item in enumerable)
			{
				sQLiteConnection.Execute("UPDATE `TreeNode` SET `RowWrite`=@RowOwnerExclusive,`RowRead`=@RowOwnerLoad WHERE `Id`=@Id", new
				{
					Id = (object)item.Id,
					RowOwnerExclusive = (object)item.RowOwnerExclusive,
					RowOwnerLoad = (object)item.RowOwnerLoad
				}, sQLiteTransaction);
			}
			sQLiteTransaction.Commit();
		}
		if (num == 40)
		{
			num = 41;
			sQLiteConnection.Execute("\r\nCREATE TABLE IF NOT EXISTS `CellProp` (\r\n`TableId` INTEGER NOT NULL,\r\n`CellId` INTEGER NOT NULL,\r\n`Dirty` INTEGER NOT NULL,\r\n`Status` INTEGER NOT NULL,\r\n`Attachments` BLOB NOT NULL DEFAULT '',\r\nPRIMARY KEY (`TableId`,`CellId`))");
		}
		if (num == 41)
		{
			num = 42;
			sQLiteConnection.Execute("ALTER TABLE `table` ADD COLUMN `Ticket` TEXT NOT NULL DEFAULT ''");
		}
		if (num == 42)
		{
			num = 43;
			sQLiteConnection.Execute("ALTER TABLE `table` ADD COLUMN `ControlFormula` TEXT NOT NULL DEFAULT ''");
		}
		// 自定义表格边框样式迁移
		try
		{
			sQLiteConnection.Execute("ALTER TABLE `Table` ADD COLUMN `CustomBorderStyle` TEXT");
		}
		catch { /* 列已存在则忽略 */ }
		// ValidationFormula 文档域绑定迁移
		try
		{
			sQLiteConnection.Execute("ALTER TABLE `ValidationFormula` ADD COLUMN `DocumentFieldId` INTEGER");
		}
		catch { /* 列已存在则忽略 */ }
		sQLiteConnection.Execute($"PRAGMA user_version={num}");
	}

	public Project GetProject()
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.QueryFirstOrDefault<Project>("SELECT `Id`,`Name`,`Parent`,`Version`,`Number`,`Category`,`Note`,`CreateTime` FROM `Project`");
	}

	public void SaveProject(Project dto)
	{
		Execute("INSERT OR REPLACE INTO `Project`(`Id`,`Name`,`Parent`,`Version`,`Number`,`Category`,`Note`,`CreateTime`) VALUES(@Id,@Name,@ParentId,@Version,@Number,@Category,@Note,@CreateTime)", dto);
	}

	public IEnumerable<TreeGroup> GetTreeGroups()
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<TreeGroup>("\r\nSELECT `Id`,`Name`,`Status`,`Dirty`,`ServerIndex`\r\nFROM `TreeGroup`\r\nWHERE `Status`<2\r\nORDER BY `Index`");
	}

	public void SaveTreeGroups(IEnumerable<TreeGroup> dto)
	{
		Execute("INSERT OR REPLACE INTO `TreeGroup`(`Id`,`Name`,`Index`,`Status`,`Dirty`,`ServerIndex`) VALUES(@Id,@Name,@Index,@Status,@Dirty,@ServerIndex)", dto);
	}

	public void RemoveTreeGroups(IEnumerable<Id64> ids)
	{
		BeginTransaction();
		foreach (Id64 id in ids)
		{
			Execute("UPDATE `TreeGroup` SET `Status`=2 WHERE `Id`= @id", new { id });
		}
		Commit();
	}

	public void DeleteTreeGroups(IEnumerable<Id64> ids)
	{
		BeginTransaction();
		foreach (Id64 id in ids)
		{
			Execute("DELETE FROM `TreeGroup` WHERE `Id` = @id", new { id });
		}
		Commit();
	}

	public IEnumerable<Id64> GetLocalRemovedTreeGroups()
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<Id64>("SELECT `Id` FROM `TreeGroup` WHERE `Status`=2");
	}

	public IEnumerable<TreeNode> GetTreeNodes()
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<TreeNode>("\r\nSELECT `Id`,`GroupId`,`ParentId`,`Name`,`Status`,`Dirty`,`Type`,`ServerIndex`,`Level`,`Version`,`Number`,`Permissions`,`Visible`,`RowWrite`,`RowRead` FROM `TreeNode` WHERE `Status`<2 ORDER BY `GroupId`,`ParentId`,`Index`");
	}

	public void SaveTreeNodes(IEnumerable<TreeNode> dto)
	{
		Execute("INSERT OR REPLACE INTO `TreeNode`(`Id`,`GroupId`,`ParentId`,`Name`,`Status`,`Dirty`,`Index`,`Type`,`ServerIndex`,`Level`,`Version`,`Number`,`Permissions`,`Visible`,`RowWrite`,`RowRead`) VALUES(@Id,@GroupId,@ParentId,@Name,@Status,@Dirty,@Index,@Type,@ServerIndex,@Level,@Version,@Number,@Permissions,@Visible,@RowWrite,@RowRead)", dto);
	}

	public void RemoveTreeNodes(IEnumerable<Id64> ids)
	{
		BeginTransaction();
		foreach (Id64 id in ids)
		{
			Execute("UPDATE `TreeNode` SET `Status`=2 WHERE `Id` = @id", new { id });
		}
		Commit();
	}

	public void DeleteTreeNodes(IEnumerable<Id64> ids)
	{
		BeginTransaction();
		foreach (Id64 id in ids)
		{
			Execute("DELETE FROM `TreeNode` WHERE `Id` =@id", new { id });
		}
		Commit();
	}

	public IEnumerable<Id64> GetLocalRemovedTreeNodes()
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<Id64>("SELECT `Id` FROM `TreeNode` WHERE `Status`=2");
	}

	public Table GetTable(Id64 id)
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.QueryFirstOrDefault<Table>("SELECT `Id`,`Title`,`PageSetup`,`Dirty`,`HeaderHeights`,`DefaultStyleId`,`ConsolidateSettings`,`BorderStyle`,`CustomBorderStyle`,`FrozenCols`,`HeaderMode`,`CollectSource`,`Locker`,`FilterInfo`,`Foot`,`RowOwnerExclusive`,`RowOwnerLoad`,`RowOwnerLoadShare`,`Ticket`,`ControlFormula` FROM `Table` WHERE `Id`=@Id", new
		{
			Id = id
		});
	}

	public IEnumerable<Column> GetColumns(Id64 tableId)
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<Column>("\r\nSELECT `Id`,`Index`,`Caption`,`CaptionStyle`,`Width`,`Visible`,`Dirty`,`Status`,`ConsolidateAttribs`,`SubtotalAttribs`,`Formula`,`ServerIndex`,`StyleId`,`Permissions`,`CaptionFormula`,`CrossAttributes`\r\nFROM `Column` \r\nWHERE `TableId`=@tableId AND `Status`<2\r\nORDER BY `Index`", new { tableId });
	}

	public async Task<List<Column>> GetTableColumns(long tableId)
	{
		var columns = new List<Column>();
		using var conn = new SQLiteConnection(connectionStringBuilder.ConnectionString);
		await conn.OpenAsync();
		using var cmd = new SQLiteCommand("SELECT Id, Caption FROM [Column] WHERE TableId = @TableId ORDER BY [Index]", conn);
		cmd.Parameters.AddWithValue("@TableId", tableId);
		using var reader = await cmd.ExecuteReaderAsync();
		while (await reader.ReadAsync())
		{
			columns.Add(new Column
			{
				Id = new Id64(reader.GetInt64(0)),
				Caption = reader.GetString(1)
			});
		}
		return columns;
	}

	public IEnumerable<Id64> GetLocalRemovedColumns(Id64 tableId)
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<Id64>("SELECT `Id` FROM `Column` WHERE `TableId`=@tableId AND `Status`=2", new { tableId });
	}

	public IEnumerable<Row> GetRows(Id64 tableId)
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<Row>("\r\nSELECT `Id`,`Index`,`Height`,`Visible`,`Locked`,`Dirty`,`Status`,`ServerIndex`,`Role`,`Permissions`,`Creator`\r\nFROM `Row` \r\nWHERE `TableId`=@tableId AND `Status`<2\r\nORDER BY `Index`,`Creator`", new { tableId });
	}

	public IEnumerable<Id64> GetLocalRemovedRows(Id64 tableId)
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<Id64>("SELECT `Id` FROM `Row` WHERE `TableId`=@tableId AND `Status`=2", new { tableId });
	}

	public IEnumerable<Cell> GetCells(Id64 tableId)
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<Cell>("\r\nSELECT c.`Id`,c.`RowId`,c.`ColumnId`,r.`Index`,l.`Index`,c.`Value`,c.`Dirty`,c.`Status`,c.`Formula`,c.`StyleId`,c.`CollectSource`,c.`HeaderFormula`\r\nFROM `Cell` AS c \r\nJOIN `Row` r ON c.`RowId`=r.`Id`\r\nJOIN `Column` l ON c.`ColumnId`=l.`Id`\r\nWHERE c.`Status`<2 AND r.`TableId`=@tableId\r\nORDER BY r.`Index`,r.`Creator`,l.`Index`", new { tableId });
	}

	public IEnumerable<CellProp> GetCellProps(Id64 tableId)
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<CellProp>("SELECT `CellId`,`Dirty`,`Status`,`Attachments` FROM `CellProp` WHERE `TableId`=@tableId", new { tableId });
	}

	public void SaveCellProps(IEnumerable<CellProp> dto)
	{
		BeginTransaction();
		Execute("INSERT OR REPLACE INTO `CellProp`(`TableId`,`CellId`,`Dirty`,`Status`,`Attachments`) VALUES(@TableId,@CellId,@Dirty,@Status,@Attachments)", dto);
		Commit();
	}

	public IEnumerable<CellStyle> GetCellStyles(Id64 tableId)
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<CellStyle>("\r\nSELECT `Id`,`FontSize`,`ForeColor`,`BackColor`,`FontFamily`,`Margin`,`Align`,`Bold`,`Italic`,`Underline`,`DataType`,`Format`,`Locked`,`Status`,`DefaultValue`,`Comment`\r\nFROM `CellStyle`\r\nWHERE `TableId`=@tableId", new { tableId });
	}

	public IEnumerable<Merge> GetMerges(Id64 tableId)
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<Merge>("\r\nSELECT `Id`,`TopLeft`,`BottomRight`,`Status`\r\nFROM `Merge`\r\nWHERE `TableId`=@tableId AND `Status`<2", new { tableId });
	}

	public IEnumerable<Id64> GetLocalRemovedCells(Id64 tableId)
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<Id64>("SELECT `Id` FROM `Cell` WHERE `ColumnId` IN (SELECT `Id` FROM `Column` WHERE `TableId`=@tableId) AND `Status`=2", new { tableId });
	}

	public IEnumerable<Id64> GetLocalRemovedMerges(Id64 tableId)
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<Id64>("SELECT `Id` FROM `Merge` WHERE `TableId`=@tableId AND `Status`=2", new { tableId });
	}

	public void SaveTable(Table dto)
	{
		BeginTransaction();
		Execute("INSERT OR REPLACE INTO `Table`(`Id`,`Title`,`PageSetup`,`Note`,`Dirty`,`HeaderHeights`,`DefaultStyleId`,`ConsolidateSettings`,`BorderStyle`,`CustomBorderStyle`,`FrozenCols`,`HeaderMode`,`CollectSource`,`Locker`,`FilterInfo`,`Foot`,`RowOwnerExclusive`,`RowOwnerLoad`,`RowOwnerLoadShare`,`Ticket`,`ControlFormula`) VALUES(@Id,@Title,@PageSetup,'',@Dirty,@HeaderHeights,@DefaultStyleId,@ConsolidateSettings,@BorderStyle,@CustomBorderStyle,@FrozenCols,@HeaderMode,@CollectSource,@Locker,@FilterInfo,@Foot,@RowOwnerExclusive,@RowOwnerLoad,@RowOwnerLoadShare,@Ticket,@ControlFormula)", dto);
		Execute("UPDATE `TreeNode` SET `Version`=@Version WHERE `Id`=@Id", dto);
		Commit();
	}

	public void SaveColumns(IEnumerable<Column> dto)
	{
		Execute("INSERT OR REPLACE INTO `Column`(`Id`,`TableId`,`Index`,`Caption`,`CaptionStyle`,`Width`,`Visible`,`Dirty`,`Status`,`ConsolidateAttribs`,`SubtotalAttribs`,`Formula`,`ServerIndex`,`StyleId`,`Permissions`,`CaptionFormula`,`CrossAttributes`) VALUES(@Id,@TableId,@Index,@Caption,@CaptionStyle,@Width,@Visible,@Dirty,@Status,@ConsolidateAttribs,@SubtotalAttribs,@Formula,@ServerIndex,@StyleId,@Permissions,@CaptionFormula,@CrossAttributes)", dto);
	}

	public void RemoveColumns(IEnumerable<Id64> ids)
	{
		BeginTransaction();
		foreach (Id64 id in ids)
		{
			Execute("UPDATE `Column` SET `Status`=2 WHERE `Id` = @id", new { id });
		}
		Commit();
	}

	public void DeleteColumns(IEnumerable<Id64> ids)
	{
		BeginTransaction();
		foreach (Id64 id in ids)
		{
			Execute("DELETE FROM `Column` WHERE `Id` = @id", new { id });
		}
		Commit();
	}

	public void SaveRows(IEnumerable<Row> dto)
	{
		Execute("INSERT OR REPLACE INTO `Row`(`Id`,`TableId`,`Index`,`Height`,`Visible`,`Locked`,`Dirty`,`Status`,`ServerIndex`,`Role`,`Permissions`,`Creator`) VALUES(@Id,@TableId,@Index,@Height,@Visible,@Locked,@Dirty,@Status,@ServerIndex,@Role,@Permissions,@Creator)", dto);
	}

	public void RemoveRows(IEnumerable<Id64> ids)
	{
		BeginTransaction();
		foreach (Id64 id in ids)
		{
			Execute("UPDATE `Row` SET `Status`=2 WHERE `Id` = @id", new { id });
		}
		Commit();
	}

	public void DeleteRows(IEnumerable<Id64> ids)
	{
		BeginTransaction();
		foreach (Id64 id in ids)
		{
			Execute("DELETE FROM `Row` WHERE `Id` = @id", new { id });
		}
		Commit();
	}

	public void SaveCells(IEnumerable<Cell> dto)
	{
		Execute("INSERT OR REPLACE INTO `Cell`(`Id`,`RowId`,`ColumnId`,`Value`,`Dirty`,`Status`,`Formula`,`StyleId`,`CollectSource`,`HeaderFormula`) VALUES(@Id,@RowId,@ColumnId,@Value,@Dirty,@Status,@Formula,@StyleId,@CollectSource,@HeaderFormula)", dto);
	}

	public void RemoveCells(IEnumerable<Id64> ids)
	{
		BeginTransaction();
		foreach (Id64 id in ids)
		{
			Execute("UPDATE `Cell` SET `Status`=2 WHERE `Id` = @id", new { id });
		}
		Commit();
	}

	public void RemoveMerges(IEnumerable<Id64> ids)
	{
		BeginTransaction();
		foreach (Id64 id in ids)
		{
			Execute("UPDATE `Merge` SET `Status`=2 WHERE `Id` = @id", new { id });
		}
		Commit();
	}

	public void DeleteCells(IEnumerable<Id64> ids)
	{
		BeginTransaction();
		foreach (Id64 id in ids)
		{
			Execute("DELETE FROM `Cell` WHERE `Id` = @id", new { id });
		}
		Commit();
	}

	public void DeleteMerges(IEnumerable<Id64> ids)
	{
		BeginTransaction();
		foreach (Id64 id in ids)
		{
			Execute("DELETE FROM `Merge` WHERE `Id` = @id", new { id });
		}
		Commit();
	}

	public void SaveCellStyles(IEnumerable<CellStyle> dto)
	{
		Execute("INSERT OR REPLACE INTO `CellStyle`(`Id`,`TableId`,`FontSize`,`ForeColor`,`BackColor`,`FontFamily`,`Status`,`Margin`,`Align`,`Bold`,`Italic`,`Underline`,`DataType`,`Format`,`Locked`,`DefaultValue`,`Comment`) VALUES(@Id,@TableId,@FontSize,@ForeColor,@BackColor,@FontFamily,@Status,@Margin,@Align,@Bold,@Italic,@Underline,@DataType,@Format,@Locked,@DefaultValue,@Comment)", dto);
	}

	public void SaveMerges(IEnumerable<Merge> dto)
	{
		Execute("INSERT OR REPLACE INTO `Merge`(`Id`,`TableId`,`TopLeft`,`BottomRight`,`Status`) VALUES(@Id,@TableId,@TopLeft,@BottomRight,@Status)", dto);
	}

	public Document GetDocument(Id64 id)
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.QueryFirstOrDefault<Document>("SELECT `Id`,`Locker`,`SectPr`,`MergeTable`,`Dirty` FROM `Document` WHERE `Id`=@id", new { id });
	}

	public void SaveDocument(Document dto)
	{
		BeginTransaction();
		Execute("INSERT OR REPLACE INTO `Document`(`Id`,`Locker`,`SectPr`,`MergeTable`,`Dirty`) VALUES(@Id,@Locker,@SectPr,@MergeTable,@Dirty)", dto);
		Execute("UPDATE `TreeNode` SET `Version`=@Version WHERE `Id`=@Id", dto);
		Commit();
	}

	public Image GetImage(Id64 id)
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.QueryFirstOrDefault<Image>("SELECT `Id`,`FileId`,`Dirty`,`CenterX`,`CenterY`,`ZoomFactor`,`PageSetup`,`RotateFlip` FROM `Image` WHERE `Id`=@id", new { id });
	}

	public void SaveImage(Image dto)
	{
		BeginTransaction();
		Execute("INSERT OR REPLACE INTO `Image`(`Id`,`FileId`,`Dirty`,`CenterX`,`CenterY`,`ZoomFactor`,`PageSetup`,`RotateFlip`) VALUES(@Id,@FileId,@Dirty,@CenterX,@CenterY,@ZoomFactor,@PageSetup,@RotateFlip)", dto);
		Execute("UPDATE `TreeNode` SET `Version`=@Version WHERE `Id`=@Id", dto);
		Commit();
	}

	public Pdf GetPdf(Id64 id)
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.QueryFirstOrDefault<Pdf>("SELECT `Id`,`FileId` FROM `Pdf` WHERE `Id`=@id", new { id });
	}

	public void SavePdf(Pdf dto)
	{
		BeginTransaction();
		Execute("INSERT OR REPLACE INTO `Pdf`(`Id`,`FileId`) VALUES(@Id,@FileId)", dto);
		Execute("UPDATE `TreeNode` SET `Version`=@Version WHERE `Id`=@Id", dto);
		Commit();
	}

	public IEnumerable<Paragraph> GetParagraphs(Id64 docId)
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<Paragraph>("SELECT `Id`,`Index`,`Stream`,`ServerIndex`,`Dirty`,`Status`,`Section`,`Comment` FROM `Paragraph` WHERE `DocumentId`=@docId AND `Status`<2 ORDER BY `Index`", new { docId });
	}

	public IEnumerable<Id64> GetLocalRemovedParagraphs(Id64 docId)
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<Id64>("SELECT `Id` FROM `Paragraph` WHERE `DocumentId`=@docId AND `Status`=2", new { docId });
	}

	public void SaveParagraphs(IEnumerable<Paragraph> dtos)
	{
		Execute("INSERT OR REPLACE INTO `Paragraph`(`Id`,`DocumentId`,`Index`,`Stream`,`ServerIndex`,`Dirty`,`Status`,`Section`,`Comment`) VALUES(@Id,@DocumentId,@Index,@Stream,@ServerIndex,@Dirty,@Status,@Section,@Comment)", dtos);
	}

	public void RemoveParagraphs(IEnumerable<Id64> ids)
	{
		BeginTransaction();
		foreach (Id64 id in ids)
		{
			Execute("UPDATE `Paragraph` SET `Status`=2 WHERE `Id` = @id", new { id });
		}
		Commit();
	}

	public void DeleteParagraphs(IEnumerable<Id64> ids)
	{
		BeginTransaction();
		foreach (Id64 id in ids)
		{
			Execute("DELETE FROM `Paragraph` WHERE `Id` = @id", new { id });
		}
		Commit();
	}

	public IEnumerable<DataReference> GetDataReferences()
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<DataReference>("SELECT `Id`,`Key`,`Value`,`Status`,`Dirty`,`Kind` FROM `DataReference` WHERE `Status`<2");
	}

	public IEnumerable<Id64> GetLocalRemovedDataReferences()
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<Id64>("SELECT `Id` FROM `DataReference` WHERE `Status`=2");
	}

	public void SaveDataReferences(IEnumerable<DataReference> dtos)
	{
		Execute("INSERT OR REPLACE INTO `DataReference`(`Id`,`Key`,`Value`,`Status`,`Dirty`,`Kind`) VALUES(@Id,@Key,@Value,@Status,@Dirty,@Kind)", dtos);
	}

	public void RemoveDataReferences(IEnumerable<Id64> ids)
	{
		BeginTransaction();
		foreach (Id64 id in ids)
		{
			Execute("UPDATE `DataReference` SET `Status`=2 WHERE `Id` = @id", new { id });
		}
		Commit();
	}

	public void DeleteDataReferences(IEnumerable<Id64> ids)
	{
		BeginTransaction();
		foreach (Id64 id in ids)
		{
			Execute("DELETE FROM `DataReference` WHERE `Id` = @id", new { id });
		}
		Commit();
	}

	public IEnumerable<ValidationFormula> GetValidationFormulas()
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<ValidationFormula>("SELECT `Id`,`LeftExpr`,`Operator`,`RightExpr`,`Note`,`Status`,`Dirty`,`TableId`,`DocumentFieldId` FROM `ValidationFormula` WHERE `Status`<2");
	}

	public IEnumerable<Id64> GetLocalRemovedValidationFormulas()
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<Id64>("SELECT `Id` FROM `ValidationFormula` WHERE `Status`=2");
	}

	public void SaveValidationFormulas(IEnumerable<ValidationFormula> dtos)
	{
		Execute("INSERT OR REPLACE INTO `ValidationFormula`(`Id`,`LeftExpr`,`Operator`,`RightExpr`,`Note`,`Status`,`Dirty`,`TableId`,`DocumentFieldId`) VALUES(@Id,@LeftExpr,@Operator,@RightExpr,@Note,@Status,@Dirty,@TableId,@DocumentFieldId)", dtos);
	}

	public void RemoveValidationFormulas(IEnumerable<Id64> ids)
	{
		BeginTransaction();
		foreach (Id64 id in ids)
		{
			Execute("UPDATE `ValidationFormula` SET `Status`=2 WHERE `Id` = @id", new { id });
		}
		Commit();
	}

	public void DeleteValidationFormulas(IEnumerable<Id64> ids)
	{
		BeginTransaction();
		foreach (Id64 id in ids)
		{
			Execute("DELETE FROM `ValidationFormula` WHERE `Id` = @id", new { id });
		}
		Commit();
	}

	public void EnsureFormatComplianceRuleTable()
	{
		using SQLiteConnection cnn = GetConnection();
		cnn.Execute("CREATE TABLE IF NOT EXISTS `FormatComplianceRule`(\r\n`Id` INTEGER,\r\n`RuleType` INTEGER,\r\n`Pattern` TEXT,\r\n`Note` TEXT,\r\n`Status` INTEGER,\r\n`Dirty` INTEGER)");
	}

	public IEnumerable<FormatComplianceRule> GetFormatComplianceRules()
	{
		EnsureFormatComplianceRuleTable();
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<FormatComplianceRule>("SELECT `Id`,`RuleType`,`Pattern`,`Note`,`Status`,`Dirty` FROM `FormatComplianceRule` WHERE `Status`<2");
	}

	public IEnumerable<Id64> GetLocalRemovedFormatComplianceRules()
	{
		EnsureFormatComplianceRuleTable();
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<Id64>("SELECT `Id` FROM `FormatComplianceRule` WHERE `Status`=2");
	}

	public void SaveFormatComplianceRules(IEnumerable<FormatComplianceRule> dtos)
	{
		EnsureFormatComplianceRuleTable();
		Execute("INSERT OR REPLACE INTO `FormatComplianceRule`(`Id`,`RuleType`,`Pattern`,`Note`,`Status`,`Dirty`) VALUES(@Id,@RuleType,@Pattern,@Note,@Status,@Dirty)", dtos);
	}

	public void RemoveFormatComplianceRules(IEnumerable<Id64> ids)
	{
		EnsureFormatComplianceRuleTable();
		BeginTransaction();
		foreach (Id64 id in ids)
		{
			Execute("UPDATE `FormatComplianceRule` SET `Status`=2 WHERE `Id` = @id", new { id });
		}
		Commit();
	}

	public void DeleteFormatComplianceRules(IEnumerable<Id64> ids)
	{
		EnsureFormatComplianceRuleTable();
		BeginTransaction();
		foreach (Id64 id in ids)
		{
			Execute("DELETE FROM `FormatComplianceRule` WHERE `Id` = @id", new { id });
		}
		Commit();
	}

	public void EnsureCrossDocumentValidationRuleTable()
	{
		using SQLiteConnection cnn = GetConnection();
		cnn.Execute("CREATE TABLE IF NOT EXISTS `CrossDocumentValidationRule`(\r\n`Id` INTEGER,\r\n`SourceDocumentId` INTEGER,\r\n`SourceFieldId` INTEGER,\r\n`TargetDocumentId` INTEGER,\r\n`TargetFieldId` INTEGER,\r\n`Operator` INTEGER,\r\n`Note` TEXT,\r\n`Status` INTEGER,\r\n`Dirty` INTEGER);");
	}

	public IEnumerable<Leqisoft.DTO.CrossDocumentValidationRule> GetCrossDocumentValidationRules()
	{
		EnsureCrossDocumentValidationRuleTable();
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<Leqisoft.DTO.CrossDocumentValidationRule>("SELECT `Id`, `SourceDocumentId`, `SourceFieldId`, `TargetDocumentId`, `TargetFieldId`, `Operator`, `Note`, `Status`, `Dirty` FROM `CrossDocumentValidationRule` WHERE `Status`<2");
	}

	public IEnumerable<Id64> GetLocalRemovedCrossDocumentValidationRules()
	{
		EnsureCrossDocumentValidationRuleTable();
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<Id64>("SELECT `Id` FROM `CrossDocumentValidationRule` WHERE `Status`=2");
	}

	public void SaveCrossDocumentValidationRules(IEnumerable<Leqisoft.DTO.CrossDocumentValidationRule> dtos)
	{
		EnsureCrossDocumentValidationRuleTable();
		Execute("INSERT OR REPLACE INTO `CrossDocumentValidationRule`(`Id`, `SourceDocumentId`, `SourceFieldId`, `TargetDocumentId`, `TargetFieldId`, `Operator`, `Note`, `Status`, `Dirty`) VALUES (@Id, @SourceDocumentId, @SourceFieldId, @TargetDocumentId, @TargetFieldId, @Operator, @Note, @Status, @Dirty)", dtos);
	}

	public void RemoveCrossDocumentValidationRules(IEnumerable<Id64> ids)
	{
		EnsureCrossDocumentValidationRuleTable();
		BeginTransaction();
		foreach (Id64 id in ids)
		{
			Execute("UPDATE `CrossDocumentValidationRule` SET `Status`=2 WHERE `Id`=@Id", new { Id = id });
		}
		Commit();
	}

	public void DeleteCrossDocumentValidationRules(IEnumerable<Id64> ids)
	{
		EnsureCrossDocumentValidationRuleTable();
		BeginTransaction();
		foreach (Id64 id in ids)
		{
			Execute("DELETE FROM `CrossDocumentValidationRule` WHERE `Id`=@Id", new { Id = id });
		}
		Commit();
	}

	public object GetCellValueById(Id64 id)
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.QueryFirstOrDefault<BinaryValue>("SELECT `Value` FROM `Cell` WHERE `Id`=@id", new { id }).Value;
	}

	public IEnumerable<SnapshotInfo> GetSnapshots(Id64 nodeId)
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<SnapshotInfo>("SELECT `Id`,`TreeNodeId`,`DateTime`,`Size`,`Kind`,`Name`,`Deleted` FROM `Snapshot` WHERE `TreeNodeId`=@nodeId", new { nodeId });
	}

	public void SaveSnapshot(SnapshotInfo si)
	{
		using SQLiteConnection cnn = GetConnection();
		cnn.Execute("INSERT INTO `Snapshot` (`Id`,`TreeNodeId`,`DateTime`,`Size`,`Kind`,`Name`,`Deleted`) VALUES(@Id,@TreeNodeId,@DateTime,@Size,@Kind,@Name,@Deleted)", si);
	}

	public void DeleteSnapshot(SnapshotInfo si)
	{
		using SQLiteConnection cnn = GetConnection();
		cnn.Execute("DELETE FROM `Snapshot` WHERE `Id`=@Id AND `Kind`=@Kind", si);
	}

	public void DeleteSnapshots(Id64 nodeId)
	{
		using SQLiteConnection cnn = GetConnection();
		cnn.Execute("DELETE FROM `Snapshot` WHERE `TreeNodeId`=@nodeId", new { nodeId });
	}

	public int GetLastSnapshotId()
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.QueryFirstOrDefault<int?>("SELECT MAX(`Id`) FROM `Snapshot`").GetValueOrDefault();
	}

	public IEnumerable<SnapshotInfo> GetRecycleList()
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<SnapshotInfo>("SELECT `Id`,`TreeNodeId`,`DateTime`,`Size`,`Kind`,`Name`,`Deleted`\r\nFROM `Snapshot` WHERE `Deleted`=1");
	}

	public IEnumerable<FormulaRecord> GetColumnFormulas()
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<FormulaRecord>("SELECT c.`TableId`,c.`Id` AS `ObjectId`,c.`Formula` FROM `Column` c inner join `treenode` n on c.`tableid`=n.`id` WHERE c.`Formula`<>'' AND c.`Status`<2");
	}

	public IEnumerable<FormulaRecord> GetCellFormulas()
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<FormulaRecord>("SELECT l.`TableId`,c.`Id` AS `ObjectId`,c.`Formula` FROM `Cell` c inner JOIN `Column` l ON c.`ColumnId`=l.`Id` inner join `treenode` n on l.`tableid`=n.`id` WHERE c.`Formula`<>'' AND c.`Status`<2");
	}

	public IEnumerable<FormulaRecord> GetHeaderCellFormulas()
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<FormulaRecord>("SELECT l.`TableId`,c.`Id` AS `ObjectId`,c.`HeaderFormula` FROM `Cell` c inner JOIN `Column` l ON c.`ColumnId`=l.`Id` inner join `treenode` n on l.`tableid`=n.`id` WHERE c.`HeaderFormula`<>'' AND c.`Status`<2");
	}

	public void DeleteTable(Id64 tableId)
	{
		using SQLiteConnection cnn = GetConnection();
		cnn.Execute("delete from `cell` where `columnId` in (select `id` from `column` where `tableId`=@tableId);\r\ndelete from `column` where `tableId`=@tableId;\r\ndelete from `row` where `tableId`=@tableId;\r\ndelete from `merge` where `tableId`=@tableId;", new { tableId });
	}
}
