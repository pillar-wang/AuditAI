using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using C1.Win.C1Command;
using C1.Win.C1FlexGrid;
using Auditai.Model;
using Auditai.UI.Controls;

namespace Auditai.UI.Platform;

public class TicketNavGrid : UserControl
{
	public class NavNode
	{
		public TicketRecord Record { get; set; }
		public int ValidationCheckFailedCount { get; set; }
		public NavNode AddLastCol(params object[] args) { return this; }
		public NavNode AddOrGet(params object[] args) { return this; }
		public NavNode[] Children(params object[] args) { return Array.Empty<NavNode>(); }
		public string Text { get; set; }
	}

	public Table Table { get; set; }
	public TicketRecord CurrentTicket { get; set; }
	public C1FlexGrid Grid { get; }
	public object NavTreeID { get; set; }
	public object NavTreeName { get; set; }
	public dynamic View { get; set; }
	public List<Auditai.Model.Column> Nav { get; set; }
	public object Ticket { get; set; }
	public object NavSetting { get; set; }
	public bool IsHasFillingFormula { get; set; }
	public bool IsAllowModifyTableRowOrder { get; set; }
	public dynamic Ctx { get; set; }

	public event EventHandler RecordSelected;
	public event EventHandler VirtualNodeSelected;

	public TicketNavGrid()
	{
		Grid = new C1FlexGrid();
		View = Grid;
	}

	public void Populate(TicketRecord ticket)
	{
		CurrentTicket = ticket;
		Populate();
	}

	public void Populate()
	{
		try
		{
			Grid.BeginUpdate();
			Grid.Rows.Fixed = 1;
			Grid.Rows.Count = 1;
			Grid.Cols.Fixed = 0;
			Grid.Cols.Count = 1;

			if (Nav == null || Nav.Count == 0)
			{
				// No navigation columns configured, just show a simple list
				Grid.Cols[0].Name = "记录";
				Grid.Cols[0].Width = 200;
				if (Table?.Ticket?.Records != null)
				{
					foreach (var record in Table.Ticket.Records)
					{
						var row = Grid.Rows.Add();
						Grid[row.Index, 0] = record?.ToString() ?? "记录 " + row.Index;
					}
				}
			}
			else
			{
				// Set up columns based on navigation settings
				Grid.Cols.Count = Nav.Count;
				for (int i = 0; i < Nav.Count; i++)
				{
					Grid.Cols[i].Name = Nav[i].Caption ?? "列" + (i + 1);
					Grid.Cols[i].Width = 120;
				}

				// Populate rows from table records
				if (Table?.Ticket?.Records != null)
				{
					foreach (var record in Table.Ticket.Records)
					{
						var row = Grid.Rows.Add();
						for (int col = 0; col < Nav.Count; col++)
						{
							try
							{
								string cellValue = "";
								if (record.Rows != null)
								{
									foreach (var r in record.Rows)
									{
										var cell = r.GetCells().FirstOrDefault(c => c.Column?.Id == Nav[col].Id);
										if (cell != null)
										{
											cellValue = cell.GetDisplayValue() ?? "";
											break;
										}
									}
								}
								Grid[row.Index, col] = cellValue;
							}
							catch { Grid[row.Index, col] = ""; }
						}
					}
				}
			}

			// Update RecordList
			var list = new List<object>();
			if (Table?.Ticket?.Records != null)
				list.AddRange(Table.Ticket.Records);
			RecordList = list;
		}
		catch (Exception ex) { ex.Log("TicketNavGrid.Populate"); }
		finally { Grid.EndUpdate(); }
	}

	public void Reset()
	{
		try
		{
			Grid.BeginUpdate();
			Grid.Rows.Count = 1;
			Grid.Rows.Fixed = 1;
			RecordList = new List<object>();
			SelectedRecord = null;
			CurrentTicket = null;
		}
		catch { }
		finally { Grid.EndUpdate(); }
	}

	public void CmdDeleteTicket_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = true;
		e.Enabled = SelectedRecord != null && !IsHasFillingFormula;
	}

	public void CmdAddTicket_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = true;
		e.Enabled = Table != null;
	}

	public IList RecordList { get; private set; } = new List<object>();

	public void TryToSelectFirstAvailableNode()
	{
		try
		{
			if (Grid.Rows.Count > 1)
			{
				Grid.Select(1, 0);
				Grid.Row = 1;
			}
		}
		catch { }
	}

	public void SetTheme() { SetTheme(null); }
	public void SetTheme(params object[] args)
	{
		try
		{
			Grid.VisualStyle = C1.Win.C1FlexGrid.VisualStyle.Office2010Blue;
			Grid.Styles.Normal.BackColor = Color.White;
			Grid.Styles.Alternate.BackColor = Color.FromArgb(240, 245, 250);
		}
		catch { }
	}

	public void FindAndSelectRecord(params object[] args)
	{
		try
		{
			if (args == null || args.Length == 0) return;
			var target = args[0];
			for (int r = 1; r < Grid.Rows.Count; r++)
			{
				if (Grid.Rows[r].UserData == target || Grid[r, 0]?.ToString() == target?.ToString())
				{
					Grid.Select(r, 0);
					Grid.Row = r;
					return;
				}
			}
		}
		catch { }
	}

	public int GetCurrentIndex(params object[] args)
	{
		return Grid.Row >= 1 ? Grid.Row - 1 : 0;
	}

	public object SelectedRecord { get; set; }
	public int SelectedVirtualNodeRowIndex { get; set; }
	public object SelectedVirtualNode { get; set; }

	public string GetTreeNodePath(params object[] args)
	{
		try
		{
			if (Grid.Row >= 1 && Grid.Row < Grid.Rows.Count)
				return "\\" + (Grid.Row - 1);
		}
		catch { }
		return "";
	}

	public void FindAndSelectTreeNodePath(params object[] args)
	{
		try
		{
			if (args?.Length > 0 && args[0] is string path)
			{
				var parts = path.Trim('\\').Split('\\');
				if (parts.Length > 0 && int.TryParse(parts[parts.Length - 1], out int idx))
				{
					if (idx + 1 < Grid.Rows.Count)
					{
						Grid.Select(idx + 1, 0);
						Grid.Row = idx + 1;
					}
				}
			}
		}
		catch { }
	}

	public string GetRecordNavTreeNodeOpenPath(params object[] args)
	{
		return GetTreeNodePath(args);
	}

	public bool IsNavTreeNodeOpenPathMatchedCurrentNavTree(string navTreeNodePath)
	{
		return GetTreeNodePath() == navTreeNodePath;
	}

	public void ClickToShowRow(params object[] args)
	{
		try
		{
			if (args?.Length > 0 && args[0] is int row && row >= 1 && row < Grid.Rows.Count)
			{
				Grid.Select(row, 0);
				Grid.Row = row;
				Grid.ShowCell(row, 0);
			}
		}
		catch { }
	}

	public static Color VirtualNodeTextColor { get; set; } = Color.Blue;

	public object GetTableRowByOpenPath(params object[] args)
	{
		try
		{
			if (args?.Length > 0 && args[0] is string path && Table?.Ticket?.Records != null)
			{
				var parts = path.Trim('\\').Split('\\');
				if (parts.Length > 0 && int.TryParse(parts[parts.Length - 1], out int idx) && idx < Table.Ticket.Records.Count)
					return Table.Ticket.Records[idx];
			}
		}
		catch { }
		return null;
	}
}