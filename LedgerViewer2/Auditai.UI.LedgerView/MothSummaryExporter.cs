using System;
using System.Collections;
using System.Collections.Generic;
using C1.C1Excel;
using C1.Win.C1FlexGrid;
using Auditai.UI.Controls;

namespace Auditai.UI.LedgerView;

internal class MothSummaryExporter : LedgerExporter
{
	protected SummaryEditor _summaryEditor;

	public MothSummaryExporter(SummaryEditor summaryEditor)
	{
		_summaryEditor = summaryEditor;
	}

	public override void Build()
	{
		XLSheet xLSheet = xlBook.Sheets[0];
		int num = 0;
		C1FlexGridEx grdMonthSummary = _summaryEditor.grdMonthSummary;
		int num2 = grdMonthSummary.Cols.Count - grdMonthSummary.Cols.Fixed;
		int num3 = grdMonthSummary.Rows.Count - grdMonthSummary.Rows.Fixed;
		int num4 = grdMonthSummary.Cols.Fixed;
		if (grdMonthSummary.Cols.Fixed > 0)
		{
			Column column = grdMonthSummary.Cols[0];
			if (column.Caption != "序号")
			{
				num4 = 0;
				num2 = grdMonthSummary.Cols.Count;
			}
		}
		List<Type> list = new List<Type>();
		for (int i = 0; i < num2; i++)
		{
			Column column2 = grdMonthSummary.Cols[num4 + i];
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
					CellStyle cellStyleDisplay = grdMonthSummary.GetCellStyleDisplay(grdMonthSummary.Rows.Fixed + j, num4 + k);
					if (cellStyleDisplay != null && cellStyleDisplay.TextAlign == TextAlignEnum.CenterCenter)
					{
						style = styleHCenter;
					}
				}
				object obj = grdMonthSummary[grdMonthSummary.Rows.Fixed + j, num4 + k];
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
