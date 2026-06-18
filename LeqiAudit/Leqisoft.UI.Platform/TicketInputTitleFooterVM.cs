using System.Collections.Generic;
using System.Linq;
using Leqisoft.Model;

namespace Leqisoft.UI.Platform;

public class TicketInputTitleFooterVM
{
	public List<TicketInputColumnVM> Columns = new List<TicketInputColumnVM>();

	public List<TicketInputRowVM> Rows = new List<TicketInputRowVM>();

	public List<TicketInputCellVM> Cells = new List<TicketInputCellVM>();

	public TicketRecord _ticketRecord;

	protected TicketTable _ticket;

	protected TicketTitleFooter _setting;

	protected Row _ticketEmptyDataRow;

	public List<TicketMerge> Merges { get; } = new List<TicketMerge>();


	public TicketInputTitleFooterVM(TicketTable ticket, TicketRecord ticketRecord, TicketTitleFooter setting, Row ticketEmptyDataRow)
	{
		_ticket = ticket;
		_ticketRecord = ticketRecord;
		_setting = setting;
		_ticketEmptyDataRow = ticketEmptyDataRow;
		Init();
	}

	protected void Init()
	{
		for (int i = 0; i < _setting.Columns.Count; i++)
		{
			TicketColumn ticketColumn = _setting.Columns[i];
			TicketInputColumnVM ticketInputColumnVM = new TicketInputColumnVM
			{
				TicketColumn = ticketColumn,
				TableColumn = _ticket.GetFieldColumn(ticketColumn.Field)
			};
			if (ticketColumn.HasFormula())
			{
				ticketInputColumnVM.Formula = ticketColumn.Formula;
			}
			else if (ticketInputColumnVM.TableColumn != null && ticketInputColumnVM.TableColumn.HasFormula)
			{
				ticketInputColumnVM.Formula = ticketInputColumnVM.TableColumn.Formula;
			}
			else
			{
				ticketInputColumnVM.Formula = "";
			}
			Columns.Add(ticketInputColumnVM);
		}
		for (int j = 0; j < _setting.Rows.Count; j++)
		{
			TicketInputRowVM item = new TicketInputRowVM
			{
				TicketRow = _setting.Rows[j]
			};
			Rows.Add(item);
			for (int k = 0; k < _setting.Columns.Count; k++)
			{
				TicketCell cell = _setting.GetCell(j, k);
				TicketInputCellVM ticketInputCellVM = new TicketInputCellVM
				{
					Value = cell.Text,
					IsDynamicTicketDataRow = false,
					TicketCell = cell
				};
				if (cell.HasField())
				{
					ticketInputCellVM.IsField = true;
					ticketInputCellVM.Column = _ticket.GetFieldColumn(cell.Field);
					if (ticketInputCellVM.Column != null)
					{
						if (ticketInputCellVM.Column.HasFormula)
						{
							ticketInputCellVM.IsFormula = true;
							ticketInputCellVM.Formula = ticketInputCellVM.Column.Formula;
							ticketInputCellVM.IsFormulaFromTicket = false;
						}
						if (_ticketRecord == null)
						{
							ticketInputCellVM.TempCell = new Cell
							{
								Column = ticketInputCellVM.Column,
								Row = _ticketEmptyDataRow,
								Value = ""
							};
							ticketInputCellVM.Value = "";
						}
						else
						{
							Cell cell4 = (ticketInputCellVM.TableCell = (ticketInputCellVM.TempCell = _ticket.Table[_ticketRecord.Rows[0].Index, ticketInputCellVM.Column.Index]));
							ticketInputCellVM.Value = cell4.Value;
							if (_ticket.Table.CellPropManager.TryGetAttachments(ticketInputCellVM.TempCell, out var attachments))
							{
								ticketInputCellVM.Attachments = attachments;
							}
						}
					}
				}
				if (cell.HasFormula())
				{
					ticketInputCellVM.IsFormula = true;
					ticketInputCellVM.Formula = cell.Formula;
					ticketInputCellVM.IsFormulaFromTicket = true;
				}
				if (ticketInputCellVM.TempCell == null)
				{
					ticketInputCellVM.TempCell = new Cell
					{
						Column = ticketInputCellVM.Column,
						Row = _ticketEmptyDataRow,
						Value = ""
					};
				}
				ticketInputCellVM.IsFixedMultiRowValue = false;
				ticketInputCellVM.IsFixedMultiRowKey = false;
				ticketInputCellVM.IsDynamicTicketDataRow = false;
				Cells.Add(ticketInputCellVM);
			}
		}
		foreach (TicketMerge merge in _setting.Merges)
		{
			Merges.Add(new TicketMerge
			{
				TopRow = merge.TopRow,
				BottomRow = merge.BottomRow,
				LeftColumn = merge.LeftColumn,
				RightColumn = merge.RightColumn
			});
		}
		if (_ticket.Kind != TicketKind.FixedMultiRow)
		{
			return;
		}
		foreach (TicketInputCellVM cell5 in Cells)
		{
			if (cell5.Column != null)
			{
				cell5.IsFixedMultiFixedCell = true;
			}
		}
	}

	public void GetAllAttachments(List<CellAttachment> outList)
	{
		foreach (TicketInputCellVM cell in Cells)
		{
			if (cell.TableCell != null && _ticket.Table.CellPropManager.TryGetAttachments(cell.TableCell, out var attachments))
			{
				outList.AddRange(attachments.Attachments);
			}
		}
	}

	public void GetAllContainsFieldCell(List<TicketInputCellVM> outList)
	{
		foreach (TicketInputCellVM cell in Cells)
		{
			if (cell.Column != null)
			{
				outList.Add(cell);
			}
		}
	}

	public TicketInputRowVM GetRow(int index)
	{
		return Rows[index];
	}

	public int GetRowsCount()
	{
		return Rows.Count;
	}

	public int GetColumnsCount()
	{
		return Columns.Count;
	}

	public int GetCellsCount()
	{
		return Cells.Count;
	}

	public bool IsIndexOutOfRange(int row, int col)
	{
		if (row < 0 || row >= Rows.Count)
		{
			return true;
		}
		if (col < 0 || col >= Columns.Count)
		{
			return true;
		}
		return false;
	}

	public TicketInputCellVM GetCellVM(int row, int col)
	{
		return Cells[row * Columns.Count + col];
	}

	public int GetRowHeight(int index)
	{
		return _setting.Rows[index].Height;
	}

	public int GetColumnWidth(int index)
	{
		return _setting.Columns[index].Width;
	}

	public TicketInputCellVM GetMergeTopLeftCellVM(int row, int col)
	{
		TicketMerge ticketMerge = Merges.FirstOrDefault((TicketMerge m) => m.Contains(row, col));
		if (ticketMerge != null)
		{
			row = ticketMerge.TopRow;
			col = ticketMerge.LeftColumn;
		}
		return GetCellVM(row, col);
	}

	public void RenameAttachment(int cellRowIndex, int cellColIndex, int fileIndex, string newName)
	{
		TicketInputCellVM cellVM = GetCellVM(cellRowIndex, cellColIndex);
		if (cellVM.TableCell != null)
		{
			_ticket.Table.CellPropManager.RenameAttachmentAt(cellVM.TableCell, fileIndex, newName);
		}
	}

	public void RemoveAttachment(int cellRowIndex, int cellColIndex, int fileIndex)
	{
		TicketInputCellVM cellVM = GetCellVM(cellRowIndex, cellColIndex);
		if (cellVM.TableCell != null)
		{
			_ticket.Table.CellPropManager.RemoveAttachmentAt(cellVM.TableCell, fileIndex);
			if (cellVM.Attachments.Attachments.Count == 0)
			{
				cellVM.Attachments = null;
			}
		}
	}
}
