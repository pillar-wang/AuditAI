using System;
using System.Collections.Generic;
using System.Linq;
using Auditai.DTO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Auditai.Model;

public class TableTitle
{
	private const int MAX_SUBTITLE_ROWS = 10;

	private const int MIN_COLUMN_WIDTH = 3;

	private const int MAX_TITLE_HEIGHT = 9999;

	private int _titleHeight;

	public List<string> NavTreeCellIdList;

	public int Version { get; set; }

	[JsonIgnore]
	public Table Table { get; }

	public int TitleHeight
	{
		get
		{
			return _titleHeight;
		}
		set
		{
			_titleHeight = Math.Min(9999, value);
		}
	}

	public TableTitleCell TitleCell { get; private set; }

	public List<TableTitleRow> Rows { get; } = new List<TableTitleRow>();


	public List<TableTitleColumn> Columns { get; } = new List<TableTitleColumn>();


	public List<TicketMerge> Merges { get; } = new List<TicketMerge>();


	public bool HasBinaryValue { get; set; }

	[JsonIgnore]
	public bool CanAddRow => Rows.Count < 10;

	internal TableTitle(Table table)
	{
		Table = table;
		TitleCell = new TableTitleCell
		{
			_table = Table
		};
		Version = 1;
	}

	internal void ResetTitleCellInstance(int rowIndex, int colIndex, object value)
	{
		if (rowIndex == 0)
		{
			TitleCell = new TableTitleCell
			{
				_table = Table
			};
			TitleCell.Value = value;
		}
		else
		{
			TableTitleCell tableTitleCell = new TableTitleCell
			{
				_table = Table
			};
			tableTitleCell.Value = value;
			Rows[rowIndex - 1].Cells[colIndex] = tableTitleCell;
		}
	}

	public void InsertRow(int index, bool useNextRowStyle)
	{
		if (!CanAddRow)
		{
			throw new InvalidOperationException($"副标题行不能多于{10}行");
		}
		TableTitleRow tableTitleRow = new TableTitleRow
		{
			_table = Table,
			Height = UserSet.Config.TableStyle.SubTitleHeight
		};
		int count = Columns.Count;
		for (int i = 0; i < count; i++)
		{
			TableTitleCell tableTitleCell = new TableTitleCell();
			tableTitleRow.Cells.Add(tableTitleCell);
			tableTitleCell._table = Table;
			tableTitleCell.InitSubtitleCell();
			tableTitleCell.Value = string.Empty;
			tableTitleCell.Align = CellTextAlign.BottomLeft;
		}
		if (Rows.Count > 0 && useNextRowStyle)
		{
			TableTitleRow tableTitleRow2 = Rows[(index == Rows.Count) ? (index - 1) : index];
			for (int j = 0; j < count; j++)
			{
				TableTitleCell tableTitleCell2 = tableTitleRow2.Cells[j];
				TableTitleCell tableTitleCell3 = tableTitleRow.Cells[j];
				tableTitleCell3.FontFamily = tableTitleCell2.FontFamily;
				tableTitleCell3.FontSize = tableTitleCell2.FontSize;
				tableTitleCell3.ForeColor = tableTitleCell2.ForeColor;
				tableTitleCell3.BackColor = tableTitleCell2.BackColor;
				tableTitleCell3.Bold = tableTitleCell2.Bold;
				tableTitleCell3.Italic = tableTitleCell2.Italic;
				tableTitleCell3.Margin = tableTitleCell2.Margin;
				tableTitleCell3.Align = tableTitleCell2.Align;
			}
		}
		Rows.Insert(index, tableTitleRow);
	}

	public void AppendRow(bool useNextRowStyle)
	{
		InsertRow(Rows.Count, useNextRowStyle);
	}

	public void RemoveRow(int index, int count)
	{
		Rows.RemoveRange(index, count);
		HashSet<TicketMerge> hashSet = new HashSet<TicketMerge>();
		foreach (TicketMerge merge in Merges)
		{
			if (index + count - 1 < merge.TopRow)
			{
				merge.TopRow -= count;
			}
			else if (index < merge.TopRow)
			{
				merge.TopRow = index;
			}
			if (index + count - 1 < merge.BottomRow)
			{
				merge.BottomRow -= count;
			}
			else if (index <= merge.BottomRow)
			{
				merge.BottomRow = index - 1;
			}
			if (merge.BottomRow < merge.TopRow)
			{
				hashSet.Add(merge);
			}
		}
		foreach (TicketMerge item in hashSet)
		{
			Merges.Remove(item);
		}
	}

	public bool InsertColumn(int index, int maxWidth, int bodyExtraWidth)
	{
		for (int i = 0; i < Rows.Count; i++)
		{
			TableTitleCell tableTitleCell = new TableTitleCell();
			TableTitleRow tableTitleRow = Rows[i];
			tableTitleRow.Cells.Insert(index, tableTitleCell);
			tableTitleCell._table = Table;
			tableTitleCell.InitSubtitleCell();
			tableTitleCell.Value = string.Empty;
			TableTitleCell tableTitleCell2 = tableTitleRow.Cells[(index == Columns.Count) ? (index - 1) : index];
			tableTitleCell.FontFamily = tableTitleCell2.FontFamily;
			tableTitleCell.FontSize = tableTitleCell2.FontSize;
			tableTitleCell.ForeColor = tableTitleCell2.ForeColor;
		}
		int num = SumWidthDisplay();
		if (num >= maxWidth)
		{
			TableTitleColumn item = new TableTitleColumn
			{
				Width = Columns[(index == Columns.Count) ? (index - 1) : index].Width
			};
			Columns.Insert(index, item);
			return false;
		}
		TableTitleColumn tableTitleColumn = new TableTitleColumn();
		TableTitleColumn tableTitleColumn2 = Columns[(index == Columns.Count) ? (index - 1) : index];
		tableTitleColumn.WidthDisplay = Math.Min(tableTitleColumn2.WidthDisplay, maxWidth - num);
		Columns.Insert(index, tableTitleColumn);
		DisplayToModel();
		Table.Columns.Resize(SumWidthDisplay() - bodyExtraWidth);
		return true;
	}

	public void RemoveColumn(int index, int count, int maxWidth, int bodyExtraWidth)
	{
		if (index < 0 || index >= Columns.Count || (index == 0 && Columns.Count == 1))
		{
			return;
		}
		if (index == 0 && count == Columns.Count)
		{
			index = 1;
			count--;
		}
		Columns.RemoveRange(index, count);
		for (int i = 0; i < Rows.Count; i++)
		{
			Rows[i].Cells.RemoveRange(index, count);
		}
		HashSet<TicketMerge> hashSet = new HashSet<TicketMerge>();
		foreach (TicketMerge merge in Merges)
		{
			if (index + count - 1 < merge.LeftColumn)
			{
				merge.LeftColumn -= count;
			}
			else if (index < merge.LeftColumn)
			{
				merge.LeftColumn = index;
			}
			if (index + count - 1 < merge.RightColumn)
			{
				merge.RightColumn -= count;
			}
			else if (index <= merge.RightColumn)
			{
				merge.RightColumn = index - 1;
			}
			if (merge.RightColumn < merge.LeftColumn)
			{
				hashSet.Add(merge);
			}
		}
		foreach (TicketMerge item in hashSet)
		{
			Merges.Remove(item);
		}
		int num = SumWidthDisplay();
		if (num < maxWidth)
		{
			Table.Columns.Resize(SumWidthDisplay() - bodyExtraWidth);
		}
	}

	public void Resize(int width)
	{
		float num = Columns.Sum((TableTitleColumn c) => c.Width);
		int num2 = width;
		for (int i = 0; i < Columns.Count - 1; i++)
		{
			Columns[i].WidthDisplay = (int)Math.Round(Columns[i].Width / num * (float)width);
			num2 -= Columns[i].WidthDisplay;
		}
		Columns[Columns.Count - 1].WidthDisplay = num2;
	}

	public int SumWidthDisplay()
	{
		return Columns.Sum((TableTitleColumn c) => c.WidthDisplay);
	}

	public void UnifyColumnWidth(int column, int count)
	{
		int num = Columns.Skip(column).Take(count).Sum((TableTitleColumn c) => c.WidthDisplay);
		for (int i = column; i < column + count - 1; i++)
		{
			Columns[i].WidthDisplay = num / count;
		}
		Columns[column + count - 1].WidthDisplay = num - Columns[column].WidthDisplay * (count - 1);
		DisplayToModel();
	}

	public bool ResizeColumn(int column, int count, int width, int maxWidth, int bodyExtraWidth)
	{
		int num = SumWidthDisplay();
		if (num >= maxWidth)
		{
			if (count > 1)
			{
				UnifyColumnWidth(column, count);
			}
			else if (column != Columns.Count - 1)
			{
				int num2 = width - Columns[column].WidthDisplay;
				if (num2 < 0)
				{
					num2 = Math.Max(num2, 3 - Columns[column].WidthDisplay);
				}
				else if (num2 > 0)
				{
					num2 = Math.Min(num2, Columns[column + 1].WidthDisplay - 3);
				}
				Columns[column].WidthDisplay += num2;
				Columns[column + 1].WidthDisplay -= num2;
				DisplayToModel();
				return false;
			}
			return false;
		}
		if (width < 3)
		{
			width = 3;
		}
		else
		{
			int num3 = num - Columns.Skip(column).Take(count).Sum((TableTitleColumn c) => c.WidthDisplay);
			int num4 = maxWidth - num3;
			if (width > num4)
			{
				width = num4;
			}
		}
		for (int i = column; i < column + count; i++)
		{
			Columns[i].WidthDisplay = width;
		}
		DisplayToModel();
		num = SumWidthDisplay();
		Table.Columns.Resize(num - bodyExtraWidth);
		return true;
	}

	public void IncreaseColumnWidth(int column, int count, int delta, int maxWidth, int bodyExtraWidth)
	{
		int num = SumWidthDisplay();
		if (num >= maxWidth)
		{
			if (column == 0 && count == Columns.Count)
			{
				return;
			}
			List<int> list = Enumerable.Range(0, column).Concat(Enumerable.Range(column + count, Columns.Count - column - count)).ToList();
			int num2 = maxWidth - 3 * list.Count;
			int num3 = Columns.Skip(column).Take(count).Sum((TableTitleColumn c) => c.WidthDisplay);
			if (num3 + count * delta > num2)
			{
				delta = (maxWidth - num) / count;
				int num4 = num - num3 - delta * count;
				int num5 = num4;
				int num6 = 0;
				foreach (int item in list)
				{
					num6 += Columns[item].WidthDisplay;
				}
				foreach (int item2 in list.Take(list.Count - 1))
				{
					double num7 = (double)Columns[item2].WidthDisplay / (double)num6;
					Columns[item2].WidthDisplay += (int)Math.Round(num7 * (double)num4);
					num5 -= Columns[item2].WidthDisplay;
				}
				Columns[list[list.Count - 1]].WidthDisplay += num5;
			}
			for (int i = column; i < column + count; i++)
			{
				Columns[i].WidthDisplay += delta;
			}
			DisplayToModel();
		}
		else
		{
			if (num + count * delta > maxWidth)
			{
				delta = (maxWidth - num) / count;
				Columns[column + count - 1].WidthDisplay += maxWidth - num - delta * (count - 1);
			}
			else
			{
				Columns[column + count - 1].WidthDisplay += delta;
			}
			for (int j = column; j < column + count - 1; j++)
			{
				Columns[j].WidthDisplay += delta;
			}
			DisplayToModel();
			Table.Columns.Resize(SumWidthDisplay() - bodyExtraWidth);
		}
	}

	public void DecreaseColumnWidth(int column, int count, int delta, int maxWidth, int bodyExtraWidth)
	{
		int num = SumWidthDisplay();
		if (num >= maxWidth)
		{
			if (column == 0 && count == Columns.Count)
			{
				return;
			}
			List<int> list = Enumerable.Range(0, column).Concat(Enumerable.Range(column + count, Columns.Count - column - count)).ToList();
			int num2 = 0;
			for (int i = column; i < column + count; i++)
			{
				if (Columns[i].WidthDisplay - delta < 3)
				{
					delta = Columns[i].WidthDisplay - 3;
				}
				num2 += delta;
				Columns[i].WidthDisplay -= delta;
			}
			int num3 = num2;
			int num4 = 0;
			foreach (int item in list)
			{
				num4 += Columns[item].WidthDisplay;
			}
			foreach (int item2 in list.Take(list.Count - 1))
			{
				double num5 = (double)Columns[item2].WidthDisplay / (double)num4;
				int num6 = (int)Math.Round(num5 * (double)num2);
				Columns[item2].WidthDisplay += num6;
				num3 -= num6;
			}
			Columns[list[list.Count - 1]].WidthDisplay += num3;
			DisplayToModel();
			return;
		}
		for (int j = column; j < column + count; j++)
		{
			Columns[j].WidthDisplay -= delta;
			if (Columns[j].WidthDisplay < 3)
			{
				Columns[j].WidthDisplay = 3;
			}
		}
		DisplayToModel();
		num = SumWidthDisplay();
		Table.Columns.Resize(num - bodyExtraWidth);
	}

	public void DisplayToModel()
	{
		foreach (TableTitleColumn column in Columns)
		{
			column.Width = column.WidthDisplay;
		}
	}

	public string Serialize()
	{
		TitleCell.BinaryValue = BinaryValue.FromObject(TitleCell.Value);
		foreach (TableTitleRow row in Rows)
		{
			foreach (TableTitleCell cell in row.Cells)
			{
				cell.BinaryValue = BinaryValue.FromObject(cell.Value);
			}
		}
		return JsonConvert.SerializeObject(this);
	}

	public void Deserialize(string s)
	{
		NavTreeCellIdList = null;
		Rows.Clear();
		Columns.Clear();
		Merges.Clear();
		JObject jObject = JObject.Parse(s);
		if (jObject["Version"] == null)
		{
			Version = 0;
		}
		Upgrade_Columns(jObject);
		Upgrade_Formula(jObject);
		Upgrade_Value(jObject);
		JsonReader reader = jObject.CreateReader();
		JsonSerializer.CreateDefault().Populate(reader, this);
		TitleCell.Value = TitleCell.BinaryValue.Value;
		foreach (TableTitleRow row in Rows)
		{
			row._table = Table;
			foreach (TableTitleCell cell in row.Cells)
			{
				cell._table = Table;
				cell.Value = cell.BinaryValue.Value;
			}
		}
	}

	private void Upgrade_Formula(JObject root)
	{
		if (Version != 0)
		{
			return;
		}
		Version = 1;
		Upgrade_ValueFormula((JObject)root["TitleCell"]);
		if (root["Rows"] != null)
		{
			foreach (JObject item in (JArray)root["Rows"])
			{
				foreach (JObject item2 in (IEnumerable<JToken>)item["Cells"])
				{
					Upgrade_ValueFormula(item2);
				}
			}
		}
		Table.TagTitleDirty();
	}

	private void Upgrade_ValueFormula(JObject cell)
	{
		string text = (string)cell["Value"];
		string text2 = "";
		if (string.IsNullOrWhiteSpace(text))
		{
			text2 = text;
		}
		else
		{
			try
			{
				FormulaEvaluator formulaEvaluator = new FormulaEvaluator(text);
				text2 = text;
			}
			catch (FormulaSyntaxException)
			{
				text2 = string.Empty;
			}
			catch (FormulaException)
			{
				text2 = text;
			}
		}
		cell["Formula"] = text2;
		cell.Remove("FormulaDisplay");
	}

	private void Upgrade_Columns(JObject root)
	{
		if (root["Columns"] != null)
		{
			return;
		}
		JArray jArray = new JArray();
		root.Add("Columns", jArray);
		JToken item = JToken.FromObject(new TableTitleColumn
		{
			Width = 1f
		});
		jArray.Add(item);
		jArray.Add(item);
		jArray.Add(item);
		if (root["Rows"] != null)
		{
			foreach (JObject item2 in (JArray)root["Rows"])
			{
				JArray jArray2 = new JArray();
				jArray2.Add(item2["Left"]);
				jArray2.Add(item2["Center"]);
				jArray2.Add(item2["Right"]);
				item2.Add("Cells", jArray2);
				item2.Remove("Left");
				item2.Remove("Center");
				item2.Remove("Right");
			}
		}
		Table.TagTitleDirty();
	}

	public void Upgrade_Value(JObject root)
	{
		if (root["HasBinaryValue"] != null)
		{
			return;
		}
		root.Add("HasBinaryValue", true);
		root["TitleCell"]["BinaryValue"] = BinaryValue.FromObject((string)root["TitleCell"]["Value"]).GetBytes();
		foreach (JObject item in (JArray)root["Rows"])
		{
			foreach (JObject item2 in (JArray)item["Cells"])
			{
				item2["BinaryValue"] = BinaryValue.FromObject((string)item2["Value"]).GetBytes();
			}
		}
		Table.TagTitleDirty();
	}

	public TableTitleCell GetCell(int row, int col)
	{
		if (row == 0)
		{
			return TitleCell;
		}
		TableTitleRow tableTitleRow = Rows[row - 1];
		return tableTitleRow.Cells[col];
	}

	public List<TableTitleCell> GetRowCells(int rowIndex)
	{
		List<TableTitleCell> list = new List<TableTitleCell>();
		if (rowIndex == 0)
		{
			list.Add(TitleCell);
		}
		else
		{
			TableTitleRow tableTitleRow = Rows[rowIndex - 1];
			list.AddRange(tableTitleRow.Cells);
		}
		return list;
	}

	public List<TableTitleCell> GetColumnCells(int colIndex)
	{
		List<TableTitleCell> list = new List<TableTitleCell>();
		int count = Rows.Count;
		for (int i = 0; i < count; i++)
		{
			list.Add(Rows[i].Cells[colIndex]);
		}
		return list;
	}

	public TableTitleCell GetUIRenderCellByCellId(string cellId)
	{
		if (TitleCell.CellId == cellId)
		{
			return TitleCell;
		}
		for (int i = 0; i < Rows.Count; i++)
		{
			TableTitleRow tableTitleRow = Rows[i];
			for (int j = 0; j < Columns.Count; j++)
			{
				if (IsUIRenderCell(i, j) && tableTitleRow.Cells[j].CellId == cellId)
				{
					return tableTitleRow.Cells[j];
				}
			}
		}
		return null;
	}

	public bool GetCellIndex(TableTitleCell cell, out int rowIndex, out int colIndex)
	{
		if (TitleCell == cell)
		{
			rowIndex = 0;
			colIndex = 0;
			return true;
		}
		for (int i = 0; i < Rows.Count; i++)
		{
			TableTitleRow tableTitleRow = Rows[i];
			for (int j = 0; j < Columns.Count; j++)
			{
				if (tableTitleRow.Cells[j] == cell)
				{
					rowIndex = i + 1;
					colIndex = j;
					return true;
				}
			}
		}
		rowIndex = 0;
		colIndex = 0;
		return false;
	}

	public TableTitleCell GetUIRenderCell(int row, int col)
	{
		if (Merges.Count == 0)
		{
			return GetCell(row, col);
		}
		foreach (TicketMerge merge in Merges)
		{
			if (merge.Contains(row - 1, col))
			{
				return GetCell(merge.TopRow + 1, merge.LeftColumn);
			}
		}
		return GetCell(row, col);
	}

	protected bool IsUIRenderCell(int row, int col)
	{
		if (Merges.Count == 0)
		{
			return true;
		}
		foreach (TicketMerge merge in Merges)
		{
			if (merge.Contains(row, col))
			{
				if (merge.TopRow == row && merge.LeftColumn == col)
				{
					return true;
				}
				return false;
			}
		}
		return true;
	}

	public int GetLastNonEmptyRow()
	{
		for (int num = Rows.Count - 1; num >= 0; num--)
		{
			if (!Rows[num].IsEmpty())
			{
				return num;
			}
		}
		return -1;
	}
}
