using System;
using System.Collections;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using C1.Win.C1Command;
using C1.Win.C1FlexGrid;
using Leqisoft.Model;
using Leqisoft.UI.Controls;

namespace Leqisoft.UI.LedgerView;

internal static class Common
{
	internal static readonly Color MarkBackColor = Color.Red;

	internal static readonly Color MarkForeColor = Color.White;

	private static TooltipBox _ttp = new TooltipBox
	{
		IsBalloon = true
	};

	internal static void ShowTooltipInfo(string title, string body, int dur, Control control, Point point)
	{
		_ttp.DurationElapsed = delegate
		{
			_ttp.Hide();
		};
		_ttp.SetText(title, new XElement("div", new XAttribute("style", "color:red"), body).ToString());
		_ttp.Show(control, point);
	}

	internal static void HideTooltipInfo()
	{
		_ttp.Hide();
	}

	internal static C1CommandLink MakeCommandLink(string text, ClickEventHandler action, object userData = null)
	{
		C1CommandLink c1CommandLink = new C1CommandLink();
		C1Command c1Command = new C1Command
		{
			Text = text,
			UserData = userData
		};
		c1Command.Click += delegate(object s, ClickEventArgs e)
		{
			action(s, e);
		};
		c1CommandLink.Command = c1Command;
		return c1CommandLink;
	}

	internal static string GetSelectionContent(C1FlexGridEx grid, out int rowsCount)
	{
		rowsCount = 0;
		CellRange selection = grid.Selection;
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = selection.r1; i <= selection.r2; i++)
		{
			if (!rowVisible(grid.Rows[i]))
			{
				continue;
			}
			rowsCount++;
			for (int j = selection.c1; j <= selection.c2; j++)
			{
				if (grid.Cols[j].Visible)
				{
					string dataDisplay = grid.GetDataDisplay(i, j);
					if (dataDisplay == null)
					{
						stringBuilder.Append(string.Empty);
					}
					else
					{
						string text = dataDisplay.ToString().Trim();
						stringBuilder.Append(text.Replace("\r", "").Replace("\n", "").Replace("\t", ""));
					}
					if (j < selection.c2)
					{
						stringBuilder.Append("\t");
					}
				}
			}
			stringBuilder.Append("\r\n");
		}
		return stringBuilder.ToString();
		static bool rowVisible(C1.Win.C1FlexGrid.Row row)
		{
			if (!row.Visible)
			{
				return false;
			}
			if (!row.IsNode)
			{
				return true;
			}
			for (Node parent = row.Node.Parent; parent != null; parent = parent.Parent)
			{
				if (parent.Collapsed)
				{
					return false;
				}
			}
			return true;
		}
	}

	internal static void SetSelectionToClipboard(C1FlexGridEx flex)
	{
		try
		{
			if (flex.Row >= 0 && flex.Col >= 0)
			{
				int rowsCount;
				string selectionContent = GetSelectionContent(flex, out rowsCount);
				if ((LedgerViewer.LicenseCheckHandleOnCopyLedgerData == null || LedgerViewer.LicenseCheckHandleOnCopyLedgerData(rowsCount)) && !string.IsNullOrWhiteSpace(selectionContent))
				{
					Clipboard.SetText(selectionContent);
				}
			}
		}
		catch (ExternalException)
		{
		}
	}

	internal static void SetTreeCheck(C1FlexGrid grid, CheckEnum checkEnum)
	{
		foreach (C1.Win.C1FlexGrid.Row item in (IEnumerable)grid.Rows)
		{
			grid.SetCellCheck(item.Index, 0, checkEnum);
		}
		grid.AllowEditing = checkEnum != CheckEnum.None;
	}

	internal static void CheckChildren(C1FlexGrid tree, Node node, CheckEnum checkenum)
	{
		tree.SetCellCheck(node.Row.Index, 0, checkenum);
		if (node.Nodes.Length != 0)
		{
			Node[] nodes = node.Nodes;
			foreach (Node node2 in nodes)
			{
				CheckChildren(tree, node2, checkenum);
			}
		}
	}

	internal static C1.Win.C1FlexGrid.Row FindRow(C1FlexGrid _grid, object userdata)
	{
		if (userdata == null)
		{
			return null;
		}
		foreach (C1.Win.C1FlexGrid.Row item in (IEnumerable)_grid.Rows)
		{
			if (userdata.Equals(item.UserData))
			{
				return item;
			}
		}
		return null;
	}

	internal static C1.Win.C1FlexGrid.Column FindColumn(C1FlexGrid _grid, object userdata)
	{
		if (userdata == null)
		{
			return null;
		}
		foreach (C1.Win.C1FlexGrid.Column item in (IEnumerable)_grid.Cols)
		{
			if (userdata.Equals(item.UserData))
			{
				return item;
			}
		}
		return null;
	}

	internal static object GetDCChar(bool isDebit, decimal balance)
	{
		if (balance == 0m)
		{
			return "平";
		}
		if (balance > 0m)
		{
			if (!isDebit)
			{
				return "贷";
			}
			return "借";
		}
		if (!isDebit)
		{
			return "借";
		}
		return "贷";
	}

	internal static string GetVoucherKey(Voucher voucher)
	{
		return $"{voucher.Day:yyyyMMdd}{voucher.Type.Name}{voucher.Number}";
	}

	internal static string GetFullNameWithCode(Account account, AuxiliaryItem item = null)
	{
		Account account2 = account;
		string code = account2.Code;
		string text = account2.Name;
		int num = 0;
		while ((account2 = account2.Parent) != null)
		{
			if (num++ > 100)
			{
				throw new Exception("数据信息异常，无法打开");
			}
			text = string.Join("-", account2.Name, text);
		}
		if (item != null)
		{
			return "（" + code + "-" + item.Code + "）" + text + "-" + item.Name;
		}
		return "（" + code + "）" + text;
	}
}
