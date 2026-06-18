using Leqisoft.DTO;

namespace Leqisoft.Model;

public class VirtualTable : Table
{
	public VirtualTable(int rowCount, int colsCount)
	{
		BuildRows(rowCount);
		BuildCols(colsCount);
		BuildCells(rowCount, colsCount);
	}

	private void BuildRows(int rowsCount)
	{
		for (int i = 0; i < rowsCount; i++)
		{
			VirtualRow virtualRow = new VirtualRow();
			virtualRow.Id = new Id64(i);
			virtualRow.Table = this;
			base.Rows._list.Add(virtualRow);
		}
	}

	private void BuildCols(int colsCount)
	{
		for (int i = 0; i < colsCount; i++)
		{
			VirtualColumn virtualColumn = new VirtualColumn();
			virtualColumn.Id = new Id64(i);
			virtualColumn.Table = this;
			base.Columns._list.Add(virtualColumn);
		}
	}

	private void BuildCells(int rowsCount, int colsCount)
	{
		for (int i = 0; i < rowsCount; i++)
		{
			for (int j = 0; j < colsCount; j++)
			{
				VirtualCell virtualCell = new VirtualCell();
				virtualCell.Id = new Id64(i * colsCount + j);
				virtualCell.Row = base.Rows[i];
				virtualCell.Column = base.Columns[j];
				base.Cells._list.Add(virtualCell);
			}
		}
	}

	public void SetDefaultStyle(CellStyle cellStyle)
	{
		base.DefaultStyle = cellStyle;
	}

	public void ResetCellInstance(int rowIndex, int colInex, object cellValue)
	{
		int num = rowIndex * base.Columns.Count + colInex;
		VirtualCell virtualCell = new VirtualCell();
		virtualCell.Id = new Id64(num);
		virtualCell.Row = base.Rows[rowIndex];
		virtualCell.Column = base.Columns[colInex];
		virtualCell.Value = cellValue;
		base.Cells._list[num] = virtualCell;
	}

	public void BuildTitleCell(int rowsCount, int colsCount)
	{
		Title.Rows.Clear();
		Title.Columns.Clear();
		for (int i = 0; i < colsCount; i++)
		{
			Title.Columns.Add(new TableTitleColumn());
		}
		for (int j = 0; j < rowsCount; j++)
		{
			TableTitleRow tableTitleRow = new TableTitleRow();
			tableTitleRow._table = this;
			Title.Rows.Add(tableTitleRow);
			for (int k = 0; k < colsCount; k++)
			{
				tableTitleRow.Cells.Add(new TableTitleCell
				{
					_table = this
				});
			}
		}
	}

	public void SetTitleCellValue(int rowIndex, int colIndex, object value)
	{
		ResetTitleCellInstance(rowIndex, colIndex, value);
	}
}
