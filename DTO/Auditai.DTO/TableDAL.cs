using System.Collections.Generic;
using System.Data.SQLite;
using Dapper;

namespace Auditai.DTO;

public class TableDAL
{
	private readonly SQLiteConnectionStringBuilder connectionStringBuilder = new SQLiteConnectionStringBuilder();

	static TableDAL()
	{
		SqlMapper.AddTypeHandler(new BinaryValueDapperHandler());
		SqlMapper.AddTypeHandler(new Id64DapperHandler());
	}

	public TableDAL(string fileName)
	{
		connectionStringBuilder.JournalMode = SQLiteJournalModeEnum.Wal;
		connectionStringBuilder.SyncMode = SynchronizationModes.Off;
		connectionStringBuilder.DataSource = fileName;
		SetPragma();
		CreateConfig();
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
		cnn.Execute("CREATE TABLE IF NOT EXISTS `Table`(\r\n`Id` INTEGER PRIMARY KEY,\r\n`Title` TEXT NOT NULL,\r\n`PageSetup` TEXT NOT NULL,\r\n`Dirty` INTEGER NOT NULL,\r\n`Note` TEXT NOT NULL,\r\n`HeaderHeights` TEXT,\r\n`DefaultStyleId` INTEGER NOT NULL,\r\n`ConsolidateSettings` TEXT,\r\n`BorderStyle` INTEGER NOT NULL DEFAULT 0,\r\n`FrozenCols` INTEGER NOT NULL DEFAULT 0,\r\n`HeaderMode` INTEGER NOT NULL DEFAULT 0,\r\n`CollectSource` TEXT,\r\n`Locker` INTEGER,\r\n`FilterInfo` TEXT,\r\n`Foot` TEXT,\r\n`RowOwnerExclusive` INTEGER NOT NULL DEFAULT 0,\r\n`RowOwnerLoad` INTEGER NOT NULL DEFAULT 0,\r\n`RowOwnerLoadShare` BLOB NOT NULL DEFAULT '',\r\n`Ticket` TEXT NOT NULL DEFAULT '',\r\n`ControlFormula` TEXT NOT NULL DEFAULT '');\r\n\r\nCREATE TABLE IF NOT EXISTS `Column`(\r\n`Id` INTEGER PRIMARY KEY,\r\n`TableId` INTEGER NOT NULL,\r\n`Index` INTEGER NOT NULL,\r\n`ServerIndex` INTEGER NOT NULL,\r\n`Caption` TEXT NOT NULL,\r\n`CaptionStyle` TEXT NOT NULL,\r\n`Width` INTEGER NOT NULL,\r\n`Visible` INTEGER NOT NULL,\r\n`Dirty` INTEGER NOT NULL,\r\n`Status` INTEGER NOT NULL,\r\n`ConsolidateAttribs` TEXT,\r\n`SubtotalAttribs` INTEGER NOT NULL DEFAULT 0,\r\n`Formula` TEXT,\r\n`StyleId` INTEGER,\r\n`Permissions` TEXT,\r\n`CaptionFormula` TEXT DEFAULT '',\r\n`CrossAttributes` BLOB NOT NULL DEFAULT '');\r\n\r\nCREATE TABLE IF NOT EXISTS `Row`(\r\n`Id` INTEGER PRIMARY KEY,\r\n`TableId` INTEGER NOT NULL,\r\n`Index` INTEGER NOT NULL,\r\n`ServerIndex` INTEGER NOT NULL,\r\n`Height` INTEGER NOT NULL,\r\n`Visible` INTEGER NOT NULL,\r\n`Dirty` INTEGER NOT NULL,\r\n`Status` INTEGER NOT NULL,\r\n`Locked` INTEGER,\r\n`Role` INTEGER,\r\n`Permissions` TEXT,\r\n`Creator` INTEGER NOT NULL DEFAULT 0);\r\n\r\nCREATE TABLE IF NOT EXISTS `Cell`(\r\n`Id` INTEGER NOT NULL,\r\n`RowId` INTEGER NOT NULL,\r\n`ColumnId` INTEGER NOT NULL,\r\n`Value` BLOB NOT NULL,\r\n`StyleId` INTEGER,\r\n`Dirty` INTEGER NOT NULL,\r\n`Status` INTEGER NOT NULL,\r\n`Formula` TEXT,\r\n`CollectSource` TEXT,\r\n`HeaderFormula` TEXT DEFAULT '');\r\n\r\nCREATE TABLE IF NOT EXISTS `Merge`(\r\n`Id` INTEGER PRIMARY KEY,\r\n`TableId` INTEGER NOT NULL,\r\n`TopLeft` INTEGER NOT NULL,\r\n`BottomRight` INTEGER NOT NULL,\r\n`Status` INTEGER NOT NULL);\r\n\r\nCREATE TABLE IF NOT EXISTS `CellStyle`(\r\n`Id` INTEGER PRIMARY KEY,\r\n`TableId` INTEGER NOT NULL,\r\n`FontFamily` TEXT,\r\n`FontSize` INTEGER,\r\n`ForeColor` INTEGER,\r\n`BackColor` INTEGER,\r\n`Align` INTEGER,\r\n`Margin` INTEGER,\r\n`Bold` INTEGER,\r\n`Italic` INTEGER,\r\n`Underline` INTEGER,\r\n`DataType` INTEGER,\r\n`Format` TEXT,\r\n`Status` INTEGER NOT NULL,\r\n`Locked` INTEGER,\r\n`DefaultValue` TEXT,\r\n`Comment` TEXT);\r\n\r\nCREATE TABLE IF NOT EXISTS `CellProp` (\r\n`TableId` INTEGER NOT NULL,\r\n`CellId` INTEGER NOT NULL,\r\n`Dirty` INTEGER NOT NULL,\r\n`Status` INTEGER NOT NULL,\r\n`Attachments` BLOB NOT NULL DEFAULT '',\r\nPRIMARY KEY (`TableId`,`CellId`));\r\n\r\nCREATE TABLE IF NOT EXISTS `ValidationFormula`(\r\n`Id` INTEGER PRIMARY KEY,\r\n`LeftExpr` TEXT NOT NULL,\r\n`Operator` INT NOT NULL,\r\n`RightExpr` TEXT NOT NULL,\r\n`Note` TEXT NOT NULL,\r\n`Status` INTEGER NOT NULL,\r\n`Dirty` INTEGER NOT NULL,\r\n`TableId` INTEGER);");
	}

	public void SaveTable(Table dto)
	{
		using SQLiteConnection sQLiteConnection = GetConnection();
		using SQLiteTransaction sQLiteTransaction = sQLiteConnection.BeginTransaction();
		sQLiteConnection.Execute("INSERT INTO `Table`(`Id`,`Title`,`PageSetup`,`Note`,`Dirty`,`HeaderHeights`,`DefaultStyleId`,`ConsolidateSettings`,`BorderStyle`,`FrozenCols`,`HeaderMode`,`CollectSource`,`Locker`,`FilterInfo`,`Foot`,`RowOwnerExclusive`,`RowOwnerLoad`,`RowOwnerLoadShare`,`Ticket`,`ControlFormula`) VALUES(@Id,@Title,@PageSetup,'',@Dirty,@HeaderHeights,@DefaultStyleId,@ConsolidateSettings,@BorderStyle,@FrozenCols,@HeaderMode,@CollectSource,@Locker,@FilterInfo,@Foot,@RowOwnerExclusive,@RowOwnerLoad,@RowOwnerLoadShare,@Ticket,@ControlFormula)", dto, sQLiteTransaction);
		sQLiteTransaction.Commit();
	}

	public void SaveColumns(IEnumerable<Column> dto)
	{
		using SQLiteConnection sQLiteConnection = GetConnection();
		using SQLiteTransaction sQLiteTransaction = sQLiteConnection.BeginTransaction();
		sQLiteConnection.Execute("INSERT INTO `Column`(`Id`,`TableId`,`Index`,`Caption`,`CaptionStyle`,`Width`,`Visible`,`Dirty`,`Status`,`ConsolidateAttribs`,`SubtotalAttribs`,`Formula`,`ServerIndex`,`StyleId`,`Permissions`,`CaptionFormula`,`CrossAttributes`) VALUES(@Id,@TableId,@Index,@Caption,@CaptionStyle,@Width,@Visible,@Dirty,@Status,@ConsolidateAttribs,@SubtotalAttribs,@Formula,@ServerIndex,@StyleId,@Permissions,@CaptionFormula,@CrossAttributes)", dto, sQLiteTransaction);
		sQLiteTransaction.Commit();
	}

	public void SaveRows(IEnumerable<Row> dto)
	{
		using SQLiteConnection sQLiteConnection = GetConnection();
		using SQLiteTransaction sQLiteTransaction = sQLiteConnection.BeginTransaction();
		sQLiteConnection.Execute("INSERT INTO `Row`(`Id`,`TableId`,`Index`,`Height`,`Visible`,`Locked`,`Dirty`,`Status`,`ServerIndex`,`Role`,`Permissions`,`Creator`) VALUES(@Id,@TableId,@Index,@Height,@Visible,@Locked,@Dirty,@Status,@ServerIndex,@Role,@Permissions,@Creator)", dto, sQLiteTransaction);
		sQLiteTransaction.Commit();
	}

	public void SaveCells(IEnumerable<Cell> dto)
	{
		using SQLiteConnection sQLiteConnection = GetConnection();
		using SQLiteTransaction sQLiteTransaction = sQLiteConnection.BeginTransaction();
		sQLiteConnection.Execute("INSERT INTO `Cell`(`Id`,`RowId`,`ColumnId`,`Value`,`Dirty`,`Status`,`Formula`,`StyleId`,`CollectSource`,`HeaderFormula`) VALUES(@Id,@RowId,@ColumnId,@Value,@Dirty,@Status,@Formula,@StyleId,@CollectSource,@HeaderFormula)", dto, sQLiteTransaction);
		sQLiteTransaction.Commit();
	}

	public void SaveMerges(IEnumerable<Merge> dto)
	{
		using SQLiteConnection sQLiteConnection = GetConnection();
		using SQLiteTransaction sQLiteTransaction = sQLiteConnection.BeginTransaction();
		sQLiteConnection.Execute("INSERT INTO `Merge`(`Id`,`TableId`,`TopLeft`,`BottomRight`,`Status`) VALUES(@Id,@TableId,@TopLeft,@BottomRight,@Status)", dto, sQLiteTransaction);
		sQLiteTransaction.Commit();
	}

	public void SaveCellProps(IEnumerable<CellProp> dto)
	{
		using SQLiteConnection sQLiteConnection = GetConnection();
		using SQLiteTransaction sQLiteTransaction = sQLiteConnection.BeginTransaction();
		sQLiteConnection.Execute("INSERT INTO `CellProp`(`TableId`,`CellId`,`Dirty`,`Status`,`Attachments`) VALUES(@TableId,@CellId,@Dirty,@Status,@Attachments)", dto, sQLiteTransaction);
		sQLiteTransaction.Commit();
	}

	public IEnumerable<CellProp> GetCellProps()
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<CellProp>("SELECT 'TableId',`CellId`,`Dirty`,`Status`,`Attachments` FROM `CellProp`");
	}

	public Table GetTable()
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.QueryFirstOrDefault<Table>("SELECT `Id`,`Title`,`PageSetup`,`Dirty`,`HeaderHeights`,`DefaultStyleId`,`ConsolidateSettings`,`BorderStyle`,`FrozenCols`,`HeaderMode`,`CollectSource`,`Locker`,`FilterInfo`,`Foot`,`RowOwnerExclusive`,`RowOwnerLoad`,`RowOwnerLoadShare`,`Ticket`,`ControlFormula` FROM `Table`");
	}

	public IEnumerable<Column> GetColumns()
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<Column>("SELECT `Id`,`Index`,`Caption`,`CaptionStyle`,`Width`,`Visible`,`Dirty`,`Status`,`ConsolidateAttribs`,`SubtotalAttribs`,`Formula`,`ServerIndex`,`StyleId`,`Permissions`,`CaptionFormula`,`CrossAttributes` FROM `Column` ORDER BY `Index`");
	}

	public IEnumerable<Row> GetRows()
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<Row>("SELECT `Id`,`Index`,`Height`,`Visible`,`Locked`,`Dirty`,`Status`,`ServerIndex`,`Role`,`Permissions`,`Creator` FROM `Row` ORDER BY `Index`");
	}

	public IEnumerable<Cell> GetCells()
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<Cell>("SELECT c.`Id`,c.`RowId`,c.`ColumnId`,c.`Value`,c.`Dirty`,c.`Status`,c.`Formula`,c.`StyleId`,c.`CollectSource`,c.`HeaderFormula`\r\nFROM `Cell` AS c\r\nJOIN `Row` AS r ON c.`RowId`= r.`Id`\r\nJOIN `Column` AS l ON c.`ColumnId`= l.`Id`\r\nORDER BY r.`Index`, l.`Index`");
	}

	public IEnumerable<Merge> GetMerges()
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<Merge>("SELECT `Id`,`TableId`,`TopLeft`,`BottomRight`,`Status` FROM `Merge`");
	}

	public IEnumerable<CellStyle> GetCellStyles()
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<CellStyle>("SELECT `Id`,`TableId`,`FontSize`,`ForeColor`,`BackColor`,`FontFamily`,`Status`,`Margin`,`Align`,`Bold`,`Italic`,`Underline`,`DataType`,`Format`,`Locked`,`DefaultValue`,`Comment` FROM `CellStyle`");
	}

	public void SaveCellStyles(IEnumerable<CellStyle> dto)
	{
		using SQLiteConnection sQLiteConnection = GetConnection();
		using SQLiteTransaction sQLiteTransaction = sQLiteConnection.BeginTransaction();
		sQLiteConnection.Execute("INSERT INTO `CellStyle`(`Id`,`TableId`,`FontSize`,`ForeColor`,`BackColor`,`FontFamily`,`Status`,`Margin`,`Align`,`Bold`,`Italic`,`Underline`,`DataType`,`Format`,`Locked`,`DefaultValue`,`Comment`) VALUES(@Id,@TableId,@FontSize,@ForeColor,@BackColor,@FontFamily,@Status,@Margin,@Align,@Bold,@Italic,@Underline,@DataType,@Format,@Locked,@DefaultValue,@Comment)", dto, sQLiteTransaction);
		sQLiteTransaction.Commit();
	}

	public void SaveValidationFormulas(IEnumerable<ValidationFormula> dto)
	{
		using SQLiteConnection sQLiteConnection = GetConnection();
		using SQLiteTransaction sQLiteTransaction = sQLiteConnection.BeginTransaction();
		sQLiteConnection.Execute("INSERT INTO `ValidationFormula`(`Id`,`LeftExpr`,`Operator`,`RightExpr`,`Note`,`Status`,`Dirty`,`TableId`) VALUES(@Id,@LeftExpr,@Operator,@RightExpr,@Note,@Status,@Dirty,@TableId)", dto, sQLiteTransaction);
		sQLiteTransaction.Commit();
	}

	public IEnumerable<ValidationFormula> GetValidationFormulas()
	{
		using SQLiteConnection cnn = GetConnection();
		return cnn.Query<ValidationFormula>("SELECT `Id`,`LeftExpr`,`Operator`,`RightExpr`,`Note`,`Status`,`Dirty`,`TableId` FROM `ValidationFormula`");
	}
}
