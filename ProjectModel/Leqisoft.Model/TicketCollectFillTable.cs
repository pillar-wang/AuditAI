using Leqisoft.DTO;

namespace Leqisoft.Model;

public class TicketCollectFillTable : Table
{
	public class TableCellUserData
	{
		public DataFormat? tableColumnDataFormat;
	}

	protected Table _tableTarget;

	protected TableTitle _tableTitle;

	public object CollectPageTitleData { get; set; }

	public object CollectPageTableHeaderData { get; set; }

	public override TableTitle Title => _tableTitle;

	public override string CollectSource
	{
		get
		{
			return _tableTarget.CollectSource;
		}
		set
		{
			_tableTarget.CollectSource = value;
		}
	}

	public TicketCollectFillTable(Table target)
	{
		_tableTarget = target;
		Init(target);
	}

	protected void Init(Table target)
	{
		base.Locker = target.Locker;
		base.TreeNode = target.TreeNode;
		base.HeaderMode = TableHeaderMode.Custom;
		_tableTitle = new TableTitle(this);
		_tableTitle.TitleCell.Value = string.Empty;
	}

	public override Table LoadAndReturn(bool bypassRowOwnerLoad = false)
	{
		return this;
	}

	public void SetMainTitleValue(object value)
	{
		if (value == null)
		{
			value = string.Empty;
		}
		_tableTitle.TitleCell.Value = value;
	}

	public void InitSubTitle(int rowsCount, int colsCount)
	{
		_tableTitle.Columns.Clear();
		_tableTitle.Rows.Clear();
		for (int i = 0; i < colsCount; i++)
		{
			TableTitleColumn item = new TableTitleColumn();
			_tableTitle.Columns.Add(item);
		}
		for (int j = 0; j < rowsCount; j++)
		{
			TableTitleRow tableTitleRow = new TableTitleRow
			{
				_table = this
			};
			_tableTitle.Rows.Add(tableTitleRow);
			for (int k = 0; k < colsCount; k++)
			{
				TableTitleCell tableTitleCell = new TableTitleCell();
				tableTitleRow.Cells.Add(tableTitleCell);
				tableTitleCell._table = this;
				tableTitleCell.Value = string.Empty;
			}
		}
	}

	public void SetSubTitleValue(int rowIndex, int colIndex, object value)
	{
		if (value == null)
		{
			value = string.Empty;
		}
		_tableTitle.Rows[rowIndex].Cells[colIndex].Value = value;
	}

	public void InitColumns(int colsCount)
	{
		base.Columns._list.Clear();
		for (int i = 0; i < colsCount; i++)
		{
			Column column = new Column();
			column.Id = new Id64(i);
			column.Table = this;
			base.Columns._list.Add(column);
		}
	}

	public Column GetColumnByIndex(int colIndex)
	{
		return base.Columns._list[colIndex];
	}

	public void SetColumnCaption(int colIndex, string caption)
	{
		Column column = base.Columns._list[colIndex];
		column.Caption = caption;
	}
}
