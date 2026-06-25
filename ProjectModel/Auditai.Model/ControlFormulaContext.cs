using System.Collections.Generic;
using System.Drawing;

namespace Auditai.Model;

public class ControlFormulaContext
{
	private HashSet<Row> _allowEditRows;

	public HashSet<Cell> Lock { get; } = new HashSet<Cell>();


	public HashSet<Cell> Warning { get; } = new HashSet<Cell>();


	public HashSet<Cell> Remind { get; } = new HashSet<Cell>();


	public Dictionary<Cell, Color> ForeColor { get; } = new Dictionary<Cell, Color>();


	public Dictionary<Cell, Color> BackColor { get; } = new Dictionary<Cell, Color>();


	public HashSet<Row> AllowEditRow => _allowEditRows;

	public void DoLock(CellsOperand o)
	{
		foreach (Cell cell in o.Cells)
		{
			Lock.Add(cell);
		}
	}

	public void DoWarning(CellsOperand o)
	{
		foreach (Cell cell in o.Cells)
		{
			Warning.Add(cell);
		}
	}

	public void DoRemind(CellsOperand o)
	{
		foreach (Cell cell in o.Cells)
		{
			Remind.Add(cell);
		}
	}

	public void DoForeColor(CellsOperand o, Color color)
	{
		foreach (Cell cell in o.Cells)
		{
			ForeColor[cell] = color;
		}
	}

	public void DoBackColor(CellsOperand o, Color color)
	{
		foreach (Cell cell in o.Cells)
		{
			BackColor[cell] = color;
		}
	}

	public void DoAllowEditRow(Row r)
	{
		if (_allowEditRows == null)
		{
			_allowEditRows = new HashSet<Row>();
		}
		if (r != null)
		{
			_allowEditRows.Add(r);
		}
	}
}
