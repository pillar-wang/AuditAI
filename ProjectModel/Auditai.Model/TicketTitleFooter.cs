using System.Collections.Generic;
using Newtonsoft.Json;

namespace Auditai.Model;

[JsonObject]
public class TicketTitleFooter
{
	public List<TicketColumn> Columns { get; } = new List<TicketColumn>();


	public List<TicketRow> Rows { get; } = new List<TicketRow>();


	public List<TicketCell> Cells { get; } = new List<TicketCell>();


	public List<TicketMerge> Merges { get; } = new List<TicketMerge>();


	public void Clear()
	{
		Columns.Clear();
		Rows.Clear();
		Cells.Clear();
		Merges.Clear();
	}

	public bool IsEmpty()
	{
		return Cells.Count == 0;
	}

	public TicketCell GetCell(int row, int col)
	{
		return Cells[row * Columns.Count + col];
	}
}
