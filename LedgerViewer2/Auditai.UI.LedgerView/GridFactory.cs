using System;
using System.Drawing;
using C1.Win.C1FlexGrid;
using C1.Win.C1FlexGrid.Util.BaseControls;
using Auditai.UI.Controls;

namespace Auditai.UI.LedgerView;

public class GridFactory
{
	public static C1FlexGridEx Create(string type)
	{
		switch (type)
		{
		case "tree":
		case "Tree":
		{
			C1FlexGridEx c1FlexGridEx = new C1FlexGridEx
			{
				Visible = false,
				AllowEditing = false,
				ExtendLastCol = true,
				Font = new Font("微软雅黑", 9f),
				ColumnInfo = "1,0,0,0,0,100,Columns:",
				SelectionMode = SelectionModeEnum.Row,
				BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None
			};
			c1FlexGridEx.Rows.DefaultSize = 30;
			return c1FlexGridEx;
		}
		case "table":
		case "Table":
			return new C1FlexGridEx();
		default:
			throw new ArgumentOutOfRangeException("本工厂不支持创建该类型的实例" + type);
		}
	}
}
