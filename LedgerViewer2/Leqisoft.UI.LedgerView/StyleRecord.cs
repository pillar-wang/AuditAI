using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using C1.Win.C1FlexGrid;
using Leqisoft.UI.Controls;

namespace Leqisoft.UI.LedgerView;

public class StyleRecord
{
	public ViewStyle ViewStyle { get; private set; } = new ViewStyle();


	public void Load(ViewStyle style)
	{
		ViewStyle = style;
	}

	public void RecordWidth(string tableName, string colName, int width)
	{
	}

	public void RecordVisible(string tableName, string colName, bool Visible)
	{
		GridStyle gridStyle = ViewStyle[tableName];
		if (gridStyle == null)
		{
			gridStyle = new GridStyle(tableName);
			ViewStyle.GridStyleCollection.Add(gridStyle);
		}
		ColStyle colStyle = gridStyle[colName];
		if (colStyle == null)
		{
			colStyle = new ColStyle(colName, Visible);
			gridStyle.ColStyleCollection.Add(colStyle);
		}
		else
		{
			colStyle.Visible = Visible;
		}
	}

	public void RecordOrder(string tableName, IEnumerable<string> colList)
	{
		GridStyle gridStyle = ViewStyle[tableName];
		if (gridStyle == null)
		{
			gridStyle = new GridStyle(tableName);
			ViewStyle.GridStyleCollection.Add(gridStyle);
		}
		gridStyle.ColOrder.Clear();
		gridStyle.ColOrder.AddRange(colList);
	}

	public void RecordHeight(C1FlexGridEx grid, int row)
	{
		int height = grid.Rows[row].Height;
		grid.BeginUpdate();
		try
		{
			if (height <= 0)
			{
				return;
			}
			ViewStyle.Height = height;
			foreach (Row item in (IEnumerable)grid.Rows)
			{
				item.Height = height;
			}
		}
		finally
		{
			grid.EndUpdate();
		}
	}

	public void ResumeVisible(C1FlexGridEx flex)
	{
		flex.BeginUpdate();
		try
		{
			foreach (Column item in (IEnumerable)flex.Cols)
			{
				item.Visible = true;
			}
			GridStyle gridStyle = ViewStyle[flex.Name];
			if (gridStyle == null)
			{
				return;
			}
			foreach (ColStyle item2 in gridStyle.ColStyleCollection)
			{
				item2.Visible = true;
			}
		}
		catch
		{
		}
		finally
		{
			flex.EndUpdate();
		}
	}

	public void ResumeWidth(C1FlexGrid flex)
	{
		flex.BeginUpdate();
		try
		{
			GridStyle gridStyle = ViewStyle[flex.Name];
			if (gridStyle == null)
			{
				return;
			}
			foreach (ColStyle item in gridStyle.ColStyleCollection)
			{
				if (flex.Cols.Contains(item.Name))
				{
					flex.Cols[item.Name].Visible = item.Visible;
				}
			}
		}
		catch
		{
		}
		finally
		{
			flex.EndUpdate();
		}
	}

	public void ResumeHeight(C1FlexGrid flex)
	{
		flex.BeginUpdate();
		try
		{
			int num = Convert.ToInt32(ViewStyle.Height);
			if (num <= 0)
			{
				return;
			}
			foreach (Row item in (IEnumerable)flex.Rows)
			{
				if (item.Height != num)
				{
					item.Height = num;
				}
			}
		}
		catch
		{
		}
		finally
		{
			flex.EndUpdate();
		}
	}

	public void ResumeFont(C1FlexGrid flex)
	{
		if (!(ViewStyle.FontSize <= 0f))
		{
			Font font = flex.Font;
			if (!(font.FontFamily.Name == ViewStyle.FamilyName) || font.Size != ViewStyle.FontSize)
			{
				flex.BeginUpdate();
				flex.Font = new Font(ViewStyle.FamilyName, ViewStyle.FontSize);
				flex.EndUpdate();
			}
		}
	}

	public void RestoreOrder(C1FlexGrid flex)
	{
		flex.BeginUpdate();
		try
		{
			GridStyle gridStyle = ViewStyle[flex.Name];
			if (gridStyle == null)
			{
				return;
			}
			foreach (string item in gridStyle.ColOrder)
			{
				if (flex.Cols.Contains(item))
				{
					flex.Cols.Move(flex.Cols.IndexOf(item), gridStyle.ColOrder.IndexOf(item));
				}
			}
		}
		catch
		{
		}
		finally
		{
			flex.EndUpdate();
		}
	}

	public void ResumeStyle(C1FlexGrid flex)
	{
		try
		{
			ResumeFont(flex);
			RestoreOrder(flex);
			ResumeWidth(flex);
			ResumeHeight(flex);
		}
		catch
		{
		}
	}
}
