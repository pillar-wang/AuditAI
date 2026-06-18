using System;
using System.Collections;
using System.Collections.Generic;
using C1.C1Excel;
using C1.Win.C1FlexGrid;
using Leqisoft.UI.Controls;

namespace Leqisoft.UI.LedgerView;

internal class LedgerAgingExporter : LedgerExporter
{
	protected LedgerAgingEditor _agingEditor;

	public LedgerAgingExporter(LedgerAgingEditor agingEditor)
	{
		_agingEditor = agingEditor;
	}

	public override void Build()
	{
		XLSheet xLSheet = xlBook.Sheets[0];
		int num = 0;
		C1FlexGridEx grid = _agingEditor.grid;
		int num2 = grid.Cols.Count - grid.Cols.Fixed;
		int num3 = grid.Rows.Count - grid.Rows.Fixed;
		int num4 = grid.Cols.Fixed;
		if (grid.Cols.Fixed > 0)
		{
			Column column = grid.Cols[0];
			if (column.Caption != "序号")
			{
				num4 = 0;
				num2 = grid.Cols.Count;
			}
		}
		List<Type> list = new List<Type>();
		for (int i = 0; i < num2; i++)
		{
			Column column2 = grid.Cols[num4 + i];
			list.Add(column2.DataType);
			xLSheet[num, i].SetValue(column2.Caption, styleHCenter);
			xLSheet.Columns[i].Width = C1XLBook.PixelsToTwips((column2.Width < 0) ? 90 : column2.Width);
		}
		num++;
		for (int j = 0; j < num3; j++)
		{
			for (int k = 0; k < num2; k++)
			{
				XLStyle style = styleBorder;
				if (list[k] == typeof(decimal))
				{
					style = styleMoney;
				}
				else
				{
					CellStyle cellStyleDisplay = grid.GetCellStyleDisplay(grid.Rows.Fixed + j, num4 + k);
					if (cellStyleDisplay != null && cellStyleDisplay.TextAlign == TextAlignEnum.CenterCenter)
					{
						style = styleHCenter;
					}
				}
				object obj = grid[grid.Rows.Fixed + j, num4 + k];
				if (obj is decimal d)
				{
					xLSheet[num, k].SetValue(LedgerExporter.EmptyIf0(d), styleMoney);
				}
				else
				{
					xLSheet[num, k].SetValue(obj, style);
				}
			}
			num++;
		}
		foreach (XLRow item in (IEnumerable)xLSheet.Rows)
		{
			item.Height = C1XLBook.PixelsToTwips(30.0);
		}
	}
}
