using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Leqisoft.DTO;
using Leqisoft.Model;
using Leqisoft.UI.Controls;

namespace Leqisoft.UI.Platform;

public class TicketDesignTableVM
{
	protected class TicketDesignCellData
	{
		public TicketDesignValidation.TempField CellData;

		public int Row;

		public int Column;
	}

	private class DataGroupKeyComparer : IEqualityComparer<TicketTableMixRangeDataGroupKey>
	{
		public bool Equals(TicketTableMixRangeDataGroupKey x, TicketTableMixRangeDataGroupKey y)
		{
			if (x == null && y == null)
			{
				return true;
			}
			if (x == null || y == null)
			{
				return false;
			}
			for (int i = 0; i < x.KeyItems.Count; i++)
			{
				if (!x.KeyItems[i].Equals(y.KeyItems[i]))
				{
					return false;
				}
			}
			return true;
		}

		public int GetHashCode(TicketTableMixRangeDataGroupKey obj)
		{
			return obj.GetHashCode();
		}
	}

	[CompilerGenerated]
	private sealed class _003CEnumerateRange_003Ed__69 : IEnumerable<TicketDesignCellVM>, IEnumerable, IEnumerator<TicketDesignCellVM>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private TicketDesignCellVM _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private int r1;

		public int _003C_003E3__r1;

		private int c1;

		public int _003C_003E3__c1;

		public TicketDesignTableVM _003C_003E4__this;

		private int c2;

		public int _003C_003E3__c2;

		private int r2;

		public int _003C_003E3__r2;

		private int _003Ci_003E5__2;

		private int _003Cj_003E5__3;

		TicketDesignCellVM IEnumerator<TicketDesignCellVM>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CEnumerateRange_003Ed__69(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			TicketDesignTableVM ticketDesignTableVM = _003C_003E4__this;
			if (num != 0)
			{
				if (num != 1)
				{
					return false;
				}
				_003C_003E1__state = -1;
				_003Cj_003E5__3++;
				goto IL_0072;
			}
			_003C_003E1__state = -1;
			_003Ci_003E5__2 = r1;
			goto IL_0090;
			IL_0072:
			if (_003Cj_003E5__3 <= c2)
			{
				_003C_003E2__current = ticketDesignTableVM.GetCell(_003Ci_003E5__2, _003Cj_003E5__3);
				_003C_003E1__state = 1;
				return true;
			}
			_003Ci_003E5__2++;
			goto IL_0090;
			IL_0090:
			if (_003Ci_003E5__2 <= r2)
			{
				_003Cj_003E5__3 = c1;
				goto IL_0072;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<TicketDesignCellVM> IEnumerable<TicketDesignCellVM>.GetEnumerator()
		{
			_003CEnumerateRange_003Ed__69 _003CEnumerateRange_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CEnumerateRange_003Ed__ = this;
			}
			else
			{
				_003CEnumerateRange_003Ed__ = new _003CEnumerateRange_003Ed__69(0)
				{
					_003C_003E4__this = _003C_003E4__this
				};
			}
			_003CEnumerateRange_003Ed__.r1 = _003C_003E3__r1;
			_003CEnumerateRange_003Ed__.c1 = _003C_003E3__c1;
			_003CEnumerateRange_003Ed__.r2 = _003C_003E3__r2;
			_003CEnumerateRange_003Ed__.c2 = _003C_003E3__c2;
			return _003CEnumerateRange_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<TicketDesignCellVM>)this).GetEnumerator();
		}
	}

	private readonly TicketTable _ticket;

	private static readonly Regex _rxValidate = new Regex("(.*)\\[(.+)](=\"(.+)\")?$", RegexOptions.Singleline);

	private int _nextCellId = 1;

	public List<TicketDesignCellVM> Cells { get; } = new List<TicketDesignCellVM>();


	public List<TicketDesignColumnVM> Columns { get; } = new List<TicketDesignColumnVM>();


	public List<TicketDesignRowVM> Rows { get; } = new List<TicketDesignRowVM>();


	public List<TicketMerge> Merges { get; } = new List<TicketMerge>();


	public TicketDesignTitleFooterVM Title { get; private set; }

	public TicketDesignTitleFooterVM Footer { get; private set; }

	public int TableRowsFrozenCount { get; set; }

	public int TableColsFrozenCount { get; set; }

	public int ColumnHeaderRowsCount { get; set; }

	public TicketDesignTableVM()
	{
	}

	public TicketDesignTableVM(TicketTable ticket)
	{
		TicketTable ticket2 = ticket;
		TicketDesignTableVM ticketDesignTableVM = this;
		_ticket = ticket2;
		if (ticket2.IsEmpty())
		{
			AppendColumns(8);
			AppendRows(10);
			Title = new TicketDesignTitleFooterVM(ticket2);
			Footer = new TicketDesignTitleFooterVM(ticket2);
			Title.AppendColumns(3);
			Title.AppendRows(2);
			Title.MergeCells(0, 0, 0, 2);
			for (int i = 0; i < Title.Columns.Count; i++)
			{
				TicketDesignCellVM cell = Title.GetCell(0, i);
				cell.FontSize = 14f;
				cell.Bold = true;
			}
			return;
		}
		TableRowsFrozenCount = ticket2.TableRowsFrozenCount;
		TableColsFrozenCount = ticket2.TableColsFrozenCount;
		ColumnHeaderRowsCount = ticket2.ColumnHeaderRowsCount;
		Title = new TicketDesignTitleFooterVM(ticket2, ticket2.Title);
		Footer = new TicketDesignTitleFooterVM(ticket2, ticket2.Footer);
		foreach (TicketColumn column in ticket2.Columns)
		{
			Columns.Add(new TicketDesignColumnVM
			{
				Width = column.Width,
				IsHiddenColumn = column.IsHiddenColumn
			});
		}
		if (ticket2.Kind == TicketKind.DynamicRow)
		{
			for (int j = 0; j < ticket2.DataRowStart; j++)
			{
				Rows.Add(new TicketDesignRowVM
				{
					Height = ticket2.Rows[j].Height
				});
				for (int k = 0; k < ticket2.Columns.Count; k++)
				{
					TicketCell cell2 = ticket2.GetCell(j, k);
					TicketDesignCellVM cellVM = GetCellVM(cell2);
					Cells.Add(cellVM);
				}
			}
			for (int l = 0; l < ticket2.DataRowCount; l++)
			{
				Rows.Add(new TicketDesignRowVM
				{
					Height = ticket2.DataRowHeight
				});
				for (int m = 0; m < ticket2.Columns.Count; m++)
				{
					TicketColumn ticketColumn = ticket2.Columns[m];
					TicketDesignCellVM ticketDesignCellVM = new TicketDesignCellVM
					{
						Text = "",
						FontFamily = ticketColumn.FontFamily,
						FontSize = ticketColumn.FontSize,
						ForeColor = ticketColumn.ForeColor,
						Bold = ticketColumn.Bold,
						Italic = ticketColumn.Italic,
						BackColor = ticketColumn.BackColor,
						Align = ticketColumn.Align,
						Top = ((l == 0) ? ticketColumn.Top.Clone() : ticketColumn.Middle.Clone()),
						Right = ticketColumn.Right.Clone(),
						Bottom = ((l == ticket2.DataRowCount - 1) ? ticketColumn.Bottom.Clone() : ticketColumn.Middle.Clone()),
						Left = ticketColumn.Left.Clone(),
						Formula = ticketColumn.Formula,
						DataFormat = ticketColumn.DataFormat
					};
					if (ticketColumn.HasField())
					{
						Leqisoft.Model.Column byId = _ticket.Table.Columns.GetById(ticketColumn.Field);
						if (byId != null)
						{
							ticketDesignCellVM.Text = "[" + byId.GetUniqueFormulaName() + "]";
						}
					}
					Cells.Add(ticketDesignCellVM);
				}
			}
			for (int n = ticket2.DataRowStart; n < ticket2.Rows.Count; n++)
			{
				Rows.Add(new TicketDesignRowVM
				{
					Height = ticket2.Rows[n].Height
				});
				for (int num = 0; num < ticket2.Columns.Count; num++)
				{
					TicketCell cell3 = ticket2.GetCell(n, num);
					TicketDesignCellVM cellVM2 = GetCellVM(cell3);
					Cells.Add(cellVM2);
				}
			}
		}
		else
		{
			for (int num2 = 0; num2 < ticket2.Rows.Count; num2++)
			{
				Rows.Add(new TicketDesignRowVM
				{
					Height = ticket2.Rows[num2].Height
				});
				for (int num3 = 0; num3 < ticket2.Columns.Count; num3++)
				{
					TicketCell cell4 = ticket2.GetCell(num2, num3);
					TicketDesignCellVM cellVM3 = GetCellVM(cell4);
					Cells.Add(cellVM3);
				}
			}
		}
		foreach (TicketMerge merge in ticket2.Merges)
		{
			Merges.Add(new TicketMerge
			{
				TopRow = TicketRowToVMRow(merge.TopRow),
				BottomRow = TicketRowToVMRow(merge.BottomRow),
				LeftColumn = merge.LeftColumn,
				RightColumn = merge.RightColumn
			});
		}
		foreach (TicketMerge dataRowMerge in ticket2.DataRowMerges)
		{
			for (int num4 = 0; num4 < ticket2.DataRowCount; num4++)
			{
				Merges.Add(new TicketMerge
				{
					TopRow = ticket2.DataRowStart + num4,
					BottomRow = ticket2.DataRowStart + num4,
					LeftColumn = dataRowMerge.LeftColumn,
					RightColumn = dataRowMerge.RightColumn
				});
			}
		}
		if (ticket2.FixedAndDynamicMixRange != null && ticket2.FixedAndDynamicMixRange.DynamicDataRowTemplateRows != null)
		{
			foreach (TicketTableMixRangeTemplateRow dynamicDataRowTemplateRow in ticket2.FixedAndDynamicMixRange.DynamicDataRowTemplateRows)
			{
				if (dynamicDataRowTemplateRow.Merges == null || dynamicDataRowTemplateRow.Merges.Count == 0)
				{
					continue;
				}
				for (int num5 = 0; num5 < ticket2.Rows.Count; num5++)
				{
					TicketRow ticketRow = ticket2.Rows[num5];
					if (!ticketRow.IsMixRangeDynamicDataRow || ticketRow.MixRangeDynamicDataRowTemplateId != dynamicDataRowTemplateRow.TemplateId)
					{
						continue;
					}
					foreach (TicketMerge merge2 in dynamicDataRowTemplateRow.Merges)
					{
						Merges.Add(new TicketMerge
						{
							TopRow = num5,
							BottomRow = num5,
							LeftColumn = merge2.LeftColumn,
							RightColumn = merge2.RightColumn
						});
					}
				}
			}
		}
		if (ticket2.Kind != TicketKind.FixedDataRowMixDynamicDataRow)
		{
			return;
		}
		foreach (TicketTableMixRangeTemplateRow dynamicDataRowTemplateRow2 in ticket2.FixedAndDynamicMixRange.DynamicDataRowTemplateRows)
		{
			for (int num6 = dynamicDataRowTemplateRow2.RefTicketTableRowIndex + 1; num6 <= dynamicDataRowTemplateRow2.BottomBorderRefTicketTableRowIndex; num6++)
			{
				Rows[num6].Height = Rows[dynamicDataRowTemplateRow2.RefTicketTableRowIndex].Height;
			}
		}
		foreach (TicketDesignCellVM cell5 in Cells)
		{
			if (cell5.HasFormula())
			{
				FormulaEvaluator formulaEvaluator = new FormulaEvaluator(cell5.Formula);
				cell5.UpdateFormula(formulaEvaluator.RewriteMixTicketFormulaToDesignFormula(ConvertFixedRowIndexToVMRowIndex));
			}
		}
		int TicketRowToVMRow(int row)
		{
			if (ticket2.Kind == TicketKind.FixedDataRowMixDynamicDataRow)
			{
				return ConvertFixedRowIndexToVMRowIndex(row);
			}
			if (row < ticket2.DataRowStart)
			{
				return row;
			}
			return row + ticket2.DataRowCount;
		}
	}

	private int ConvertFixedRowIndexToVMRowIndex(int rowIndex)
	{
		int num = -1;
		for (int i = 0; i < _ticket.Rows.Count; i++)
		{
			if (!_ticket.Rows[i].IsMixRangeDynamicDataRow)
			{
				num++;
				if (num == rowIndex)
				{
					return i;
				}
			}
		}
		if (num == -1)
		{
			return rowIndex;
		}
		return num;
	}

	public TicketDesignCellVM GetCell(int row, int col)
	{
		return Cells[row * Columns.Count + col];
	}

	public int GetRowsCount()
	{
		return Rows.Count;
	}

	public int GetColumnsCount()
	{
		return Columns.Count;
	}

	public TicketDesignRowVM GetRow(int index)
	{
		return Rows[index];
	}

	public TicketDesignColumnVM GetColumn(int index)
	{
		return Columns[index];
	}

	private string GetCellFormulaError()
	{
		int rowsCount = GetRowsCount();
		int columnsCount = GetColumnsCount();
		for (int i = 0; i < rowsCount; i++)
		{
			for (int j = 0; j < columnsCount; j++)
			{
				TicketDesignCellVM cell = GetCell(i, j);
				if (cell.HasFormula())
				{
					try
					{
						FormulaEvaluator formulaEvaluator = new FormulaEvaluator(cell.Formula);
					}
					catch (FormulaSyntaxException)
					{
						return "表体区单元格[" + Leqisoft.Model.Column.GetExcelColumnName(j) + ":" + (i + 1) + "]的单元格公式不正确!";
					}
				}
			}
		}
		return null;
	}

	public void InsertColumns(int index, int count)
	{
		foreach (TicketDesignCellVM cell in Cells)
		{
			if (cell.HasFormula())
			{
				try
				{
					FormulaEvaluator formulaEvaluator = new FormulaEvaluator(cell.Formula);
					cell.UpdateFormula(formulaEvaluator.OffsetTicket(int.MaxValue, 0, index, count));
				}
				catch (FormulaSyntaxException)
				{
				}
			}
		}
		int i;
		for (i = Rows.Count - 1; i >= 0; i--)
		{
			Cells.InsertRange(i * Columns.Count + index, Enumerable.Range(index, count).Select((Func<int, TicketDesignCellVM>)delegate
			{
				if (index == Columns.Count)
				{
					return new TicketDesignCellVM
					{
						Text = "",
						Align = CellTextAlign.MiddleCenter,
						Bottom = new TicketBorder
						{
							Width = 1
						},
						FontFamily = "微软雅黑",
						FontSize = 10.5f,
						ForeColor = Color.Black,
						BackColor = Color.White,
						Formula = "",
						Indent = 0,
						Bold = false,
						Italic = false,
						Left = new TicketBorder
						{
							Width = 1
						},
						Right = new TicketBorder
						{
							Width = 1
						},
						Top = new TicketBorder
						{
							Width = 1
						},
						DataFormat = null
					};
				}
				TicketDesignCellVM ticketDesignCellVM = Cells[i * Columns.Count + index];
				return new TicketDesignCellVM
				{
					Text = "",
					Align = ticketDesignCellVM.Align,
					Bottom = ticketDesignCellVM.Bottom.Clone(),
					FontFamily = ticketDesignCellVM.FontFamily,
					FontSize = ticketDesignCellVM.FontSize,
					ForeColor = ticketDesignCellVM.ForeColor,
					BackColor = ticketDesignCellVM.BackColor,
					Formula = "",
					Indent = ticketDesignCellVM.Indent,
					Bold = ticketDesignCellVM.Bold,
					Italic = ticketDesignCellVM.Italic,
					Left = ticketDesignCellVM.Left.Clone(),
					Right = ticketDesignCellVM.Right.Clone(),
					Top = ticketDesignCellVM.Top.Clone(),
					DataFormat = ticketDesignCellVM.DataFormat
				};
			}).ToList());
		}
		Columns.InsertRange(index, from _ in Enumerable.Range(0, count)
			select new TicketDesignColumnVM
			{
				Width = 100
			});
		foreach (TicketMerge merge in Merges)
		{
			if (index <= merge.LeftColumn)
			{
				merge.LeftColumn += count;
			}
			if (index <= merge.RightColumn)
			{
				merge.RightColumn += count;
			}
		}
	}

	public void AppendColumns(int count)
	{
		InsertColumns(Columns.Count, count);
	}

	public void RemoveColumns(int index, int count)
	{
		for (int num = Rows.Count - 1; num >= 0; num--)
		{
			Cells.RemoveRange(num * Columns.Count + index, count);
		}
		Columns.RemoveRange(index, count);
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
		foreach (TicketDesignCellVM cell in Cells)
		{
			if (cell.HasFormula())
			{
				try
				{
					FormulaEvaluator formulaEvaluator = new FormulaEvaluator(cell.Formula);
					cell.UpdateFormula(formulaEvaluator.OffsetTicket(int.MaxValue, 0, index, -count));
				}
				catch (FormulaSyntaxException)
				{
				}
			}
		}
	}

	public void InsertRows(int index, int count)
	{
		foreach (TicketDesignCellVM cell in Cells)
		{
			if (cell.HasFormula())
			{
				try
				{
					FormulaEvaluator formulaEvaluator = new FormulaEvaluator(cell.Formula);
					cell.UpdateFormula(formulaEvaluator.OffsetTicket(index, count, int.MaxValue, 0));
				}
				catch (FormulaSyntaxException)
				{
				}
			}
		}
		Cells.InsertRange(index * Columns.Count, Enumerable.Range(0, count * Columns.Count).Select(delegate(int i)
		{
			if (index == Rows.Count)
			{
				return new TicketDesignCellVM
				{
					Text = "",
					Align = CellTextAlign.MiddleCenter,
					Bottom = new TicketBorder
					{
						Width = 1
					},
					FontFamily = "微软雅黑",
					FontSize = 10.5f,
					ForeColor = Color.Black,
					BackColor = Color.White,
					Formula = "",
					Indent = 0,
					Bold = false,
					Italic = false,
					Left = new TicketBorder
					{
						Width = 1
					},
					Right = new TicketBorder
					{
						Width = 1
					},
					Top = new TicketBorder
					{
						Width = 1
					},
					DataFormat = null
				};
			}
			TicketDesignCellVM ticketDesignCellVM = Cells[index * Columns.Count + i % Columns.Count];
			return new TicketDesignCellVM
			{
				Text = "",
				Align = ticketDesignCellVM.Align,
				Bottom = ticketDesignCellVM.Bottom.Clone(),
				FontFamily = ticketDesignCellVM.FontFamily,
				FontSize = ticketDesignCellVM.FontSize,
				ForeColor = ticketDesignCellVM.ForeColor,
				BackColor = ticketDesignCellVM.BackColor,
				Formula = "",
				Indent = ticketDesignCellVM.Indent,
				Bold = ticketDesignCellVM.Bold,
				Italic = ticketDesignCellVM.Italic,
				Left = ticketDesignCellVM.Left.Clone(),
				Right = ticketDesignCellVM.Right.Clone(),
				Top = ticketDesignCellVM.Top.Clone(),
				DataFormat = ticketDesignCellVM.DataFormat
			};
		}).ToList());
		Rows.InsertRange(index, from _ in Enumerable.Range(0, count)
			select new TicketDesignRowVM
			{
				Height = 30
			});
		foreach (TicketMerge merge in Merges)
		{
			if (index <= merge.TopRow)
			{
				merge.TopRow += count;
			}
			if (index <= merge.BottomRow)
			{
				merge.BottomRow += count;
			}
		}
		foreach (TicketMerge j in Merges.Where((TicketMerge m) => m.TopRow == m.BottomRow && m.TopRow == index + count).ToList())
		{
			Merges.AddRange(from i in Enumerable.Range(index, count)
				select new TicketMerge
				{
					TopRow = i,
					BottomRow = i,
					LeftColumn = j.LeftColumn,
					RightColumn = j.RightColumn
				});
		}
	}

	public void AppendRows(int count)
	{
		InsertRows(Rows.Count, count);
	}

	public void RemoveRows(int index, int count)
	{
		Cells.RemoveRange(index * Columns.Count, count * Columns.Count);
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
		foreach (TicketDesignCellVM cell in Cells)
		{
			if (cell.HasFormula())
			{
				try
				{
					FormulaEvaluator formulaEvaluator = new FormulaEvaluator(cell.Formula);
					cell.UpdateFormula(formulaEvaluator.OffsetTicket(index, -count, int.MaxValue, 0));
				}
				catch (FormulaSyntaxException)
				{
				}
			}
		}
	}

	protected bool ParseFixedAndDynamicRowMixRange(int rangeStartRowIndex, int rangeRowsCount, List<TicketDesignCellData> fieldCellList, TicketDesignValidation outTarget)
	{
		HashSet<TicketDesignCellVM> hashSet = new HashSet<TicketDesignCellVM>(fieldCellList.Select((TicketDesignCellData u) => GetCell(u.Row, u.Column)));
		for (int i = rangeStartRowIndex; i < rangeStartRowIndex + rangeRowsCount; i++)
		{
			for (int j = 0; j < Columns.Count; j++)
			{
				TicketDesignCellVM cell = GetCell(i, j);
				if (outTarget.DicField.TryGetValue(cell, out var _) && !hashSet.Contains(cell))
				{
					outTarget.FailureReason = TicketDesignFailureReason.DataRowContainsKeyCell;
					return false;
				}
			}
		}
		List<int> list = (from u in fieldCellList
			group u by u.Column into t
			select t.Key).ToList();
		HashSet<Id64> hashSet2 = new HashSet<Id64>();
		for (int k = 0; k < list.Count; k++)
		{
			int col = list[k];
			TicketDesignCellVM cell2 = GetCell(rangeStartRowIndex, col);
			if (!outTarget.DicField.TryGetValue(cell2, out var value2))
			{
				outTarget.FailureReason = TicketDesignFailureReason.RowNotContinuous;
				return false;
			}
			if (hashSet2.Contains(value2.Field))
			{
				outTarget.FailureReason = TicketDesignFailureReason.InvalidCol;
				return false;
			}
			hashSet2.Add(value2.Field);
			for (int l = 1; l < rangeRowsCount; l++)
			{
				int row = rangeStartRowIndex + l;
				TicketDesignCellVM cell3 = GetCell(row, col);
				if (!outTarget.DicField.TryGetValue(cell3, out var value3) || value3.Field != value2.Field)
				{
					outTarget.FailureReason = TicketDesignFailureReason.RowNotContinuous;
					return false;
				}
			}
		}
		HashSet<int> hashSet3 = new HashSet<int>();
		HashSet<int> hashSet4 = new HashSet<int>();
		for (int m = rangeStartRowIndex; m < rangeStartRowIndex + rangeRowsCount; m++)
		{
			bool flag = false;
			foreach (int item in list)
			{
				TicketDesignCellVM cell4 = GetCell(m, item);
				if (outTarget.DicField.TryGetValue(cell4, out var value4) && !string.IsNullOrEmpty(value4.Text))
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				hashSet4.Add(m);
			}
			else
			{
				hashSet3.Add(m);
			}
		}
		TicketDesignValidation.FixedAndDynamicRowMixRange fixedAndDynamicRowMixRange = new TicketDesignValidation.FixedAndDynamicRowMixRange();
		outTarget.MixRangeList.Add(fixedAndDynamicRowMixRange);
		fixedAndDynamicRowMixRange.RangeStartRowIndex = rangeStartRowIndex;
		fixedAndDynamicRowMixRange.RangeEndRowIndex = rangeStartRowIndex + rangeRowsCount - 1;
		fixedAndDynamicRowMixRange.RangeRowsCount = rangeRowsCount;
		fixedAndDynamicRowMixRange.FixedRowsList = hashSet4.ToList();
		fixedAndDynamicRowMixRange.FixedRowsList.Sort();
		foreach (int fixedRows in fixedAndDynamicRowMixRange.FixedRowsList)
		{
			for (int n = 0; n < Columns.Count; n++)
			{
				TicketDesignValidation.FieldCellSetting fieldCellSetting = outTarget.MixTicketCellSettingList[fixedRows, n];
				if (fieldCellSetting != null)
				{
					fieldCellSetting.IsInFixedDataRow = true;
				}
			}
		}
		List<int> list2 = hashSet3.ToList();
		list2.Sort();
		if (list2.Count > 0)
		{
			List<Tuple<int, int>> list3 = new List<Tuple<int, int>>();
			int num = list2[0];
			int num2 = 1;
			int num3 = num;
			for (int num4 = 1; num4 < list2.Count; num4++)
			{
				int num5 = list2[num4];
				if (num3 != num5 - 1)
				{
					list3.Add(Tuple.Create(num, num2));
					num = num5;
					num3 = num5;
					num2 = 1;
				}
				else
				{
					num2++;
					num3 = num5;
				}
			}
			list3.Add(Tuple.Create(num, num2));
			fixedAndDynamicRowMixRange.DynamicRowsList = list3;
			foreach (int item2 in list2)
			{
				for (int num6 = 0; num6 < Columns.Count; num6++)
				{
					TicketDesignValidation.FieldCellSetting fieldCellSetting2 = outTarget.MixTicketCellSettingList[item2, num6];
					if (fieldCellSetting2 != null)
					{
						fieldCellSetting2.IsInFixedDataRow = true;
					}
				}
			}
		}
		else
		{
			fixedAndDynamicRowMixRange.DynamicRowsList = new List<Tuple<int, int>>();
		}
		return true;
	}

	private bool IsMergeRangeValidInFixedRowMixDynamicRowTicket(TicketDesignValidation outTarget)
	{
		foreach (TicketMerge merge in Merges)
		{
			bool flag = IsTicketRangeExistNoDataRowCell(merge, outTarget);
			bool flag2 = IsTicketRangeExistFixedDataRowCell(merge, outTarget);
			bool flag3 = IsTicketRangeExistDynamicDataRowCell(merge, outTarget);
			if (flag && flag3)
			{
				outTarget.FailureReason = TicketDesignFailureReason.MergeRangeCrossDynamicRowAndFixedRow;
				return false;
			}
			if (flag2 && flag3)
			{
				outTarget.FailureReason = TicketDesignFailureReason.MergeRangeCrossDynamicRowAndFixedRow;
				return false;
			}
			if (flag3 && merge.TopRow != merge.BottomRow)
			{
				outTarget.FailureReason = TicketDesignFailureReason.InvalidDataRowVerticalMerge;
				return false;
			}
			for (int i = merge.TopRow; i <= merge.BottomRow; i++)
			{
				for (int j = merge.LeftColumn; j <= merge.RightColumn; j++)
				{
					TicketDesignValidation.FieldCellSetting fieldCellSetting = outTarget.MixTicketCellSettingList[i, j];
					if (fieldCellSetting != null)
					{
						fieldCellSetting.TicketMergeRange = merge;
					}
				}
			}
		}
		foreach (TicketDesignValidation.FixedAndDynamicRowMixRange mixRange in outTarget.MixRangeList)
		{
			foreach (Tuple<int, int> dynamicRows in mixRange.DynamicRowsList)
			{
				for (int k = 0; k < Columns.Count; k++)
				{
					for (int l = 1; l < dynamicRows.Item2; l++)
					{
						int num = dynamicRows.Item1 + l;
						int num2 = num - 1;
						TicketDesignValidation.FieldCellSetting fieldCellSetting2 = outTarget.MixTicketCellSettingList[num2, k];
						TicketDesignValidation.FieldCellSetting fieldCellSetting3 = outTarget.MixTicketCellSettingList[num, k];
						if (fieldCellSetting2.TicketMergeRange != null || fieldCellSetting3.TicketMergeRange != null)
						{
							if (fieldCellSetting2.TicketMergeRange == null || fieldCellSetting3.TicketMergeRange == null)
							{
								outTarget.FailureReason = TicketDesignFailureReason.InvalidCol;
								return false;
							}
							if (((dynamic)fieldCellSetting2.TicketMergeRange).LeftColumn != ((dynamic)fieldCellSetting3.TicketMergeRange).LeftColumn)
							{
								outTarget.FailureReason = TicketDesignFailureReason.DataRowMergeInvalidLeftCol;
								return false;
							}
							if (((dynamic)fieldCellSetting2.TicketMergeRange).RightColumn != ((dynamic)fieldCellSetting3.TicketMergeRange).RightColumn)
							{
								outTarget.FailureReason = TicketDesignFailureReason.DataRowMergeInvalidRightCol;
								return false;
							}
						}
					}
				}
			}
		}
		return true;
		static bool IsTicketRangeExistDynamicDataRowCell(TicketMerge mergeRange, TicketDesignValidation outTarget)
		{
			for (int m = mergeRange.TopRow; m <= mergeRange.BottomRow; m++)
			{
				for (int n = mergeRange.LeftColumn; n <= mergeRange.RightColumn; n++)
				{
					TicketDesignValidation.FieldCellSetting fieldCellSetting4 = outTarget.MixTicketCellSettingList[m, n];
					if (fieldCellSetting4 != null && fieldCellSetting4.IsInDynamicDataRow)
					{
						return true;
					}
				}
			}
			return false;
		}
		static bool IsTicketRangeExistFixedDataRowCell(TicketMerge mergeRange, TicketDesignValidation outTarget)
		{
			for (int num3 = mergeRange.TopRow; num3 <= mergeRange.BottomRow; num3++)
			{
				for (int num4 = mergeRange.LeftColumn; num4 <= mergeRange.RightColumn; num4++)
				{
					TicketDesignValidation.FieldCellSetting fieldCellSetting5 = outTarget.MixTicketCellSettingList[num3, num4];
					if (fieldCellSetting5 != null && fieldCellSetting5.IsInFixedDataRow)
					{
						return true;
					}
				}
			}
			return false;
		}
		static bool IsTicketRangeExistNoDataRowCell(TicketMerge mergeRange, TicketDesignValidation outTarget)
		{
			for (int num5 = mergeRange.TopRow; num5 <= mergeRange.BottomRow; num5++)
			{
				for (int num6 = mergeRange.LeftColumn; num6 <= mergeRange.RightColumn; num6++)
				{
					TicketDesignValidation.FieldCellSetting fieldCellSetting6 = outTarget.MixTicketCellSettingList[num5, num6];
					if (fieldCellSetting6 == null || fieldCellSetting6.TempField == null)
					{
						return true;
					}
					if (!fieldCellSetting6.IsInDynamicDataRow && !fieldCellSetting6.IsInFixedDataRow)
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	private bool IsMixRangeRefrenceTableColumnValid(TicketDesignValidation outTarget)
	{
		foreach (TicketDesignValidation.FixedAndDynamicRowMixRange mixRange in outTarget.MixRangeList)
		{
			for (int i = mixRange.RangeStartRowIndex; i <= mixRange.RangeEndRowIndex; i++)
			{
				for (int j = 0; j < Columns.Count; j++)
				{
					TicketDesignValidation.FieldCellSetting fieldCellSetting = outTarget.MixTicketCellSettingList[i, j];
					if (fieldCellSetting.TempField != null && fieldCellSetting.IsTicketKey && (fieldCellSetting.IsInDynamicDataRow || fieldCellSetting.IsInFixedDataRow))
					{
						outTarget.FailureReason = TicketDesignFailureReason.InvalidCol;
						return false;
					}
				}
			}
		}
		foreach (TicketDesignValidation.FixedAndDynamicRowMixRange mixRange2 in outTarget.MixRangeList)
		{
			for (int k = 0; k < Columns.Count; k++)
			{
				for (int l = mixRange2.RangeStartRowIndex; l <= mixRange2.RangeEndRowIndex; l++)
				{
					TicketDesignValidation.FieldCellSetting fieldCellSetting2 = outTarget.MixTicketCellSettingList[l, k];
					if (l == mixRange2.RangeStartRowIndex)
					{
						continue;
					}
					TicketDesignValidation.FieldCellSetting fieldCellSetting3 = outTarget.MixTicketCellSettingList[l - 1, k];
					if (fieldCellSetting3.TempField != null || fieldCellSetting2.TempField != null)
					{
						if (fieldCellSetting3.TempField == null || fieldCellSetting2.TempField == null)
						{
							outTarget.FailureReason = TicketDesignFailureReason.InvalidCol;
							return false;
						}
						if (fieldCellSetting3.TempField.Field != fieldCellSetting2.TempField.Field)
						{
							outTarget.FailureReason = TicketDesignFailureReason.InvalidCol;
							return false;
						}
					}
				}
			}
			HashSet<long> hashSet = new HashSet<long>();
			for (int m = 0; m < Columns.Count; m++)
			{
				TicketDesignValidation.FieldCellSetting fieldCellSetting4 = outTarget.MixTicketCellSettingList[mixRange2.RangeStartRowIndex, m];
				if (fieldCellSetting4.TempField != null && !fieldCellSetting4.IsTicketKey)
				{
					if (hashSet.Contains(fieldCellSetting4.TempField.Field.Value))
					{
						outTarget.FailureReason = TicketDesignFailureReason.InvalidCol;
						return false;
					}
					hashSet.Add(fieldCellSetting4.TempField.Field.Value);
				}
			}
			foreach (Tuple<int, int> dynamicRows in mixRange2.DynamicRowsList)
			{
				for (int n = 0; n < Columns.Count; n++)
				{
					for (int num = 1; num < dynamicRows.Item2; num++)
					{
						TicketDesignCellVM cell = GetCell(dynamicRows.Item1 + num, n);
						TicketDesignCellVM cell2 = GetCell(dynamicRows.Item1 + num - 1, n);
						if (cell.Text != cell2.Text)
						{
							outTarget.FailureReason = TicketDesignFailureReason.InvalidCol;
							return false;
						}
					}
				}
			}
		}
		return true;
	}

	private bool IsMixRangeCellFormulaValid(TicketDesignValidation outTarget)
	{
		foreach (TicketDesignValidation.FixedAndDynamicRowMixRange mixRange in outTarget.MixRangeList)
		{
			foreach (Tuple<int, int> dynamicRows in mixRange.DynamicRowsList)
			{
				for (int i = 0; i < Columns.Count; i++)
				{
					for (int j = 1; j < dynamicRows.Item2; j++)
					{
						int num = dynamicRows.Item1 + j;
						TicketDesignCellVM cell = GetCell(num, i);
						TicketDesignCellVM cell2 = GetCell(num - 1, i);
						if (cell.Formula != cell2.Formula)
						{
							outTarget.FailureReason = TicketDesignFailureReason.InvalidFormula;
							return false;
						}
					}
				}
			}
		}
		return true;
	}

	protected void PostValidate(TicketDesignValidation validation)
	{
		if (!validation.Success || validation.Kind == (int)TicketKind.FixedMultiRow)
		{
			return;
		}
		foreach (TicketDesignCellVM key in validation.TitleDicField.Keys)
		{
			if (key.HasFormula())
			{
				validation.FailureReason = TicketDesignFailureReason.WriteTicketFormulaDataToTableCell;
				return;
			}
		}
		foreach (TicketDesignCellVM key2 in validation.FooterDicField.Keys)
		{
			if (key2.HasFormula())
			{
				validation.FailureReason = TicketDesignFailureReason.WriteTicketFormulaDataToTableCell;
				return;
			}
		}
		foreach (TicketDesignCellVM key3 in validation.DicField.Keys)
		{
			if (!key3.HasFormula())
			{
				continue;
			}
			if (validation.Kind != (int)TicketKind.FixedDataRowMixDynamicDataRow)
			{
				validation.FailureReason = TicketDesignFailureReason.WriteTicketFormulaDataToTableCell;
				break;
			}
			if (validation.MixTicketCellSettingList == null)
			{
				validation.FailureReason = TicketDesignFailureReason.WriteTicketFormulaDataToTableCell;
				break;
			}
			TicketDesignValidation.FieldCellSetting fieldCellSetting = null;
			int length = validation.MixTicketCellSettingList.GetLength(0);
			int length2 = validation.MixTicketCellSettingList.GetLength(1);
			for (int i = 0; i < length; i++)
			{
				for (int j = 0; j < length2; j++)
				{
					TicketDesignValidation.FieldCellSetting fieldCellSetting2 = validation.MixTicketCellSettingList[i, j];
					if (fieldCellSetting2.ticketDesignCellVM == key3)
					{
						fieldCellSetting = fieldCellSetting2;
						break;
					}
				}
				if (fieldCellSetting != null)
				{
					break;
				}
			}
			if (fieldCellSetting == null)
			{
				validation.FailureReason = TicketDesignFailureReason.WriteTicketFormulaDataToTableCell;
				break;
			}
			if (!fieldCellSetting.IsInFixedDataRow)
			{
				validation.FailureReason = TicketDesignFailureReason.WriteTicketFormulaDataToTableCell;
				break;
			}
			if (fieldCellSetting.TempField == null)
			{
				validation.FailureReason = TicketDesignFailureReason.WriteTicketFormulaDataToTableCell;
				break;
			}
			if (!string.IsNullOrEmpty(fieldCellSetting.TempField.Text) || !string.IsNullOrEmpty(fieldCellSetting.TempField.InputValue))
			{
				validation.FailureReason = TicketDesignFailureReason.WriteTicketFormulaDataToTableCell;
				break;
			}
		}
	}

	public TicketDesignValidation Validate()
	{
		TicketDesignValidation ret = new TicketDesignValidation
		{
			Kind = (int)TicketKind.FixedOneRow,
			FailureReason = TicketDesignFailureReason.None,
			GroupStartRow = -1,
			GroupEndRow = -1
		};
		ClearDataAreaMergeCellText();
		CelarTitleFooterMergeCellText(Title);
		CelarTitleFooterMergeCellText(Footer);
		if (Cells.All((TicketDesignCellVM c) => c.Text == "") && Title.Cells.All((TicketDesignCellVM c) => c.Text == "") && Footer.Cells.All((TicketDesignCellVM c) => c.Text == ""))
		{
			ret.Kind = (int)TicketKind.None;
			return ret;
		}
		Dictionary<string, Leqisoft.Model.Column> dictionary = _ticket.Table.Columns.ToDictionary((Leqisoft.Model.Column c) => c.GetUniqueFormulaName() ?? "", (Leqisoft.Model.Column c) => c);
		Dictionary<Id64, Leqisoft.Model.Column> dictionary2 = new Dictionary<Id64, Leqisoft.Model.Column>();
		foreach (TicketDesignCellVM cell5 in Cells)
		{
			Match match = _rxValidate.Match(cell5.Text);
			if (!match.Success)
			{
				continue;
			}
			string value = match.Groups[2].Value;
			if (dictionary.TryGetValue(value, out var value2))
			{
				TicketDesignValidation.TempField tempField = new TicketDesignValidation.TempField();
				tempField.Field = value2.Id;
				tempField.Text = match.Groups[1].Value;
				if (match.Groups[4].Success)
				{
					tempField.InputValue = match.Groups[4].Value;
				}
				ret.DicField.Add(cell5, tempField);
				if (!dictionary2.ContainsKey(tempField.Field))
				{
					dictionary2.Add(tempField.Field, value2);
				}
			}
		}
		if (ret.DicField.Count == 0)
		{
			ret.FailureReason = TicketDesignFailureReason.NoField;
			return ret;
		}
		if (!GetTitleFooterInputField(dictionary, dictionary2))
		{
			return ret;
		}
		var enumerable = from tup in Cells.Select((TicketDesignCellVM c, int i) => new
			{
				c = c,
				row = i / Columns.Count,
				col = i % Columns.Count
			})
			where ret.DicField.ContainsKey(tup.c)
			select new
			{
				c = ret.DicField[tup.c],
				row = tup.row,
				col = tup.col
			} into tup
			group tup by tup.c.Field into g
			where g.Count() > 1
			select g;
		if (!enumerable.Any())
		{
			return ret;
		}
		if (enumerable.SelectMany(g => g).Any(tup => string.IsNullOrEmpty(tup.c.Text) && string.IsNullOrEmpty(tup.c.InputValue)) && enumerable.SelectMany(g => g).Any(tup => !string.IsNullOrEmpty(tup.c.Text) || !string.IsNullOrEmpty(tup.c.InputValue)) && (from tup in enumerable.SelectMany(g => g)
			group tup by tup.row).All(c => (from tup in c
			group tup by tup.c.Field into g
			where g.Count() > 1
			select g).Count() == 0))
		{
			ret.Kind = (int)TicketKind.FixedDataRowMixDynamicDataRow;
			HashSet<TicketDesignValidation.TempField> hashSet = new HashSet<TicketDesignValidation.TempField>(from u in enumerable.SelectMany(g => g)
				select u.c);
			ret.MixTicketCellSettingList = new TicketDesignValidation.FieldCellSetting[Rows.Count, Columns.Count];
			for (int j = 0; j < Rows.Count; j++)
			{
				for (int k = 0; k < Columns.Count; k++)
				{
					TicketDesignCellVM cell = GetCell(j, k);
					TicketDesignValidation.FieldCellSetting fieldCellSetting = new TicketDesignValidation.FieldCellSetting();
					fieldCellSetting.ticketDesignCellVM = cell;
					ret.MixTicketCellSettingList[j, k] = fieldCellSetting;
					if (ret.DicField.TryGetValue(cell, out var value3))
					{
						fieldCellSetting.TempField = value3;
						if (hashSet.Contains(value3))
						{
							fieldCellSetting.IsTicketKey = false;
						}
						else
						{
							fieldCellSetting.IsTicketKey = true;
						}
					}
				}
			}
			List<Tuple<int, List<TicketDesignCellData>>> list = new List<Tuple<int, List<TicketDesignCellData>>>();
			var list2 = (from tup in enumerable.SelectMany(g => g)
				group tup by tup.row).ToList();
			list2.Sort((left, right) => (left.Key != right.Key) ? ((left.Key >= right.Key) ? 1 : (-1)) : 0);
			foreach (var item2 in list2)
			{
				list.Add(Tuple.Create(item2.Key, item2.Select(u => new TicketDesignCellData
				{
					CellData = u.c,
					Row = u.row,
					Column = u.col
				}).ToList()));
			}
			List<TicketDesignCellData> list3 = new List<TicketDesignCellData>();
			int num = list[0].Item1;
			int num2 = 1;
			int num3 = num;
			list3.AddRange(list[0].Item2);
			for (int l = 1; l < list.Count; l++)
			{
				int item = list[l].Item1;
				if (item != num3 + 1)
				{
					if (!ParseFixedAndDynamicRowMixRange(num, num2, list3, ret))
					{
						return ret;
					}
					num = item;
					num3 = num;
					num2 = 1;
					list3 = new List<TicketDesignCellData>();
					list3.AddRange(list[l].Item2);
				}
				else
				{
					num2++;
					list3.AddRange(list[l].Item2);
					num3 = item;
				}
			}
			if (!ParseFixedAndDynamicRowMixRange(num, num2, list3, ret))
			{
				return ret;
			}
			if (!IsMergeRangeValidInFixedRowMixDynamicRowTicket(ret))
			{
				return ret;
			}
			if (!IsMixRangeRefrenceTableColumnValid(ret))
			{
				return ret;
			}
			if (!IsMixRangeCellFormulaValid(ret))
			{
				return ret;
			}
			bool flag = false;
			foreach (TicketDesignValidation.FixedAndDynamicRowMixRange mixRange in ret.MixRangeList)
			{
				if (mixRange.DynamicRowsList != null && mixRange.DynamicRowsList.Count > 0)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				return ret;
			}
			ret.Kind = (int)TicketKind.FixedOneRow;
			ret.FailureReason = TicketDesignFailureReason.None;
			ret.GroupStartRow = -1;
			ret.GroupEndRow = -1;
			ret.MixRangeList.Clear();
			ret.MixTicketCellSettingList = null;
		}
		if (enumerable.SelectMany(g => g).Any(tup => tup.c.Text != ""))
		{
			ret.Kind = (int)TicketKind.FixedMultiRow;
			if (enumerable.Select(g => g.Count()).Distinct().Count() != 1)
			{
				ret.FailureReason = TicketDesignFailureReason.FieldCountNotEqual;
				return ret;
			}
			foreach (var item3 in enumerable)
			{
				if (item3.First().c.Text == "")
				{
					if (item3.Any(tup => tup.c.Text != ""))
					{
						ret.FailureReason = TicketDesignFailureReason.FieldCellsNotSameKind;
						return ret;
					}
				}
				else if (item3.Any(tup => tup.c.Text == ""))
				{
					ret.FailureReason = TicketDesignFailureReason.FieldCellsNotSameKind;
					return ret;
				}
			}
		}
		else
		{
			ret.Kind = (int)TicketKind.DynamicRow;
			ret.GroupStartRow = enumerable.First().First().row;
			ret.GroupEndRow = enumerable.First().Last().row;
			foreach (var item4 in enumerable)
			{
				int row = item4.First().row;
				if (row != ret.GroupStartRow)
				{
					ret.FailureReason = TicketDesignFailureReason.InvalidStartRow;
					return ret;
				}
				int col = item4.First().col;
				foreach (var item5 in item4.Skip(1))
				{
					if (item5.row != row + 1)
					{
						ret.FailureReason = TicketDesignFailureReason.RowNotContinuous;
						return ret;
					}
					row = item5.row;
					if (item5.col != col)
					{
						ret.FailureReason = TicketDesignFailureReason.InvalidCol;
						return ret;
					}
				}
				if (row != ret.GroupEndRow)
				{
					ret.FailureReason = TicketDesignFailureReason.InvalidEndRow;
					return ret;
				}
			}
			if (Merges.Any((TicketMerge m) => m.BottomRow != m.TopRow && ((m.BottomRow >= ret.GroupStartRow && m.BottomRow <= ret.GroupEndRow) || (m.TopRow >= ret.GroupStartRow && m.TopRow <= ret.GroupEndRow))))
			{
				ret.FailureReason = TicketDesignFailureReason.InvalidDataRowVerticalMerge;
				return ret;
			}
			IEnumerable<IGrouping<int, TicketMerge>> enumerable2 = from m in Merges
				where m.TopRow == m.BottomRow && m.TopRow >= ret.GroupStartRow && m.BottomRow <= ret.GroupEndRow
				group m by m.LeftColumn;
			foreach (IGrouping<int, TicketMerge> item6 in enumerable2)
			{
				List<TicketMerge> source = item6.OrderBy((TicketMerge m) => m.TopRow).ToList();
				int topRow = source.First().TopRow;
				int rightColumn = source.First().RightColumn;
				if (topRow != ret.GroupStartRow)
				{
					ret.FailureReason = TicketDesignFailureReason.InvalidDataRowMergeStartRow;
					return ret;
				}
				foreach (TicketMerge item7 in source.Skip(1))
				{
					if (item7.TopRow != topRow + 1)
					{
						ret.FailureReason = TicketDesignFailureReason.DataRowMergeNotContinuous;
						return ret;
					}
					topRow = item7.TopRow;
					if (item7.RightColumn != rightColumn)
					{
						ret.FailureReason = TicketDesignFailureReason.DataRowMergeInvalidRightCol;
						return ret;
					}
				}
				if (topRow != ret.GroupEndRow)
				{
					ret.FailureReason = TicketDesignFailureReason.InvalidDataRowMergeStartRow;
					return ret;
				}
				ret.DataMergeCols.Add(Tuple.Create(source.First().LeftColumn, rightColumn));
			}
			for (int n = 0; n < Columns.Count; n++)
			{
				string formula = GetCell(ret.GroupStartRow, n).Formula;
				for (int num4 = ret.GroupStartRow; num4 <= ret.GroupEndRow; num4++)
				{
					if (GetCell(num4, n).Formula != formula)
					{
						ret.FailureReason = TicketDesignFailureReason.InvalidFormula;
						return ret;
					}
				}
			}
		}
		ColumnHeaderRowsCount = Math.Min(ColumnHeaderRowsCount, Rows.Count);
		ColumnHeaderRowsCount = Math.Max(ColumnHeaderRowsCount, 0);
		if (ColumnHeaderRowsCount > 0)
		{
			for (int num5 = 0; num5 < ColumnHeaderRowsCount; num5++)
			{
				for (int num6 = 0; num6 < Columns.Count; num6++)
				{
					TicketDesignCellVM cell2 = GetCell(num5, num6);
					if (ret.DicField.ContainsKey(cell2))
					{
						ret.FailureReason = TicketDesignFailureReason.ColumnHeaderRowIncludeField;
						return ret;
					}
					if (cell2.HasFormula())
					{
						ret.FailureReason = TicketDesignFailureReason.ColumnHeaderRowExistFormula;
						return ret;
					}
				}
			}
			foreach (TicketMerge merge in Merges)
			{
				if (merge.TopRow < ColumnHeaderRowsCount && merge.BottomRow >= ColumnHeaderRowsCount)
				{
					ret.FailureReason = TicketDesignFailureReason.MergeRangeCrossColumnHeaderAndDataArea;
					return ret;
				}
			}
		}
		return ret;
		static void CelarTitleFooterMergeCellText(TicketDesignTitleFooterVM target)
		{
			foreach (TicketMerge merge2 in target.Merges)
			{
				for (int num7 = merge2.TopRow; num7 <= merge2.BottomRow; num7++)
				{
					for (int num8 = merge2.LeftColumn; num8 <= merge2.RightColumn; num8++)
					{
						if (num7 != merge2.TopRow || num8 != merge2.LeftColumn)
						{
							TicketDesignCellVM cell3 = target.GetCell(num7, num8);
							cell3.Text = "";
						}
					}
				}
			}
		}
		void ClearDataAreaMergeCellText()
		{
			foreach (TicketMerge merge3 in Merges)
			{
				for (int num9 = merge3.TopRow; num9 <= merge3.BottomRow; num9++)
				{
					for (int num10 = merge3.LeftColumn; num10 <= merge3.RightColumn; num10++)
					{
						if (num9 != merge3.TopRow || num10 != merge3.LeftColumn)
						{
							TicketDesignCellVM cell4 = GetCell(num9, num10);
							cell4.Text = "";
						}
					}
				}
			}
		}
		bool GetTitleFooterInputField(Dictionary<string, Leqisoft.Model.Column> columnDic, Dictionary<Id64, Leqisoft.Model.Column> dataAreaUsedField)
		{
			Dictionary<Id64, Leqisoft.Model.Column> dictionary3 = new Dictionary<Id64, Leqisoft.Model.Column>();
			Dictionary<Id64, Leqisoft.Model.Column> dictionary4 = new Dictionary<Id64, Leqisoft.Model.Column>();
			if (!GetTitleOrFooterInputField(columnDic, Title, isTitleArea: true, dataAreaUsedField, dictionary3, ret.TitleDicField))
			{
				return false;
			}
			if (!GetTitleOrFooterInputField(columnDic, Footer, isTitleArea: false, dataAreaUsedField, dictionary4, ret.FooterDicField))
			{
				return false;
			}
			foreach (Id64 key in dictionary4.Keys)
			{
				if (dictionary3.ContainsKey(key))
				{
					ret.FailureReason = TicketDesignFailureReason.FooterPartExistTitleRepeatField;
					return false;
				}
			}
			return true;
		}
		bool GetTitleOrFooterInputField(Dictionary<string, Leqisoft.Model.Column> columnDic, TicketDesignTitleFooterVM target, bool isTitleArea, Dictionary<Id64, Leqisoft.Model.Column> dataAreaUsedField, Dictionary<Id64, Leqisoft.Model.Column> outUsedFieldMap, Dictionary<object, TicketDesignValidation.TempField> outDic)
		{
			foreach (TicketDesignCellVM cell6 in target.Cells)
			{
				Match match2 = _rxValidate.Match(cell6.Text);
				if (match2.Success)
				{
					string value4 = match2.Groups[2].Value;
					if (columnDic.TryGetValue(value4, out var value5))
					{
						TicketDesignValidation.TempField tempField2 = new TicketDesignValidation.TempField
						{
							Field = value5.Id,
							Text = match2.Groups[1].Value
						};
						if (match2.Groups[4].Success)
						{
							tempField2.InputValue = match2.Groups[4].Value;
						}
						if (outUsedFieldMap.ContainsKey(tempField2.Field))
						{
							ret.FailureReason = (isTitleArea ? TicketDesignFailureReason.TitlePartExistRepeatField : TicketDesignFailureReason.FooterPartExistRepeatField);
							return false;
						}
						if (dataAreaUsedField.ContainsKey(tempField2.Field))
						{
							ret.FailureReason = (isTitleArea ? TicketDesignFailureReason.TitlePartIncludeDataPartField : TicketDesignFailureReason.FooterPartIncludeDataPartField);
							return false;
						}
						outUsedFieldMap.Add(tempField2.Field, value5);
						outDic.Add(cell6, tempField2);
					}
				}
			}
			return true;
		}
	}

	private bool CheckCellFormulaIsValid()
	{
		string text = null;
		text = GetCellFormulaError();
		if (text == null)
		{
			text = Title.GetCellFormulaError("标题区");
			if (text == null)
			{
				text = Footer.GetCellFormulaError("表底区");
			}
		}
		if (text != null)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, text);
			return false;
		}
		return true;
	}

	protected void GenerateCellId()
	{
		foreach (TicketDesignCellVM cell in Cells)
		{
			if (cell.CellId == 0)
			{
				cell.CellId = _nextCellId++;
			}
		}
	}

	protected List<Tuple<int, int>> GetTicketMergeInCellId()
	{
		if (Merges.Count == 0)
		{
			return new List<Tuple<int, int>>();
		}
		GenerateCellId();
		List<Tuple<int, int>> list = new List<Tuple<int, int>>();
		foreach (TicketMerge merge in Merges)
		{
			TicketDesignCellVM cell = GetCell(merge.TopRow, merge.LeftColumn);
			TicketDesignCellVM cell2 = GetCell(merge.BottomRow, merge.RightColumn);
			if (cell != null && cell2 != null)
			{
				list.Add(Tuple.Create(cell.CellId, cell2.CellId));
			}
		}
		return list;
	}

	protected void RebuildTicketMergeByCellId(List<Tuple<int, int>> mergeList)
	{
		Merges.Clear();
		if (mergeList.Count == 0)
		{
			return;
		}
		GenerateCellId();
		Dictionary<int, Tuple<int, int>> dictionary = new Dictionary<int, Tuple<int, int>>();
		for (int i = 0; i < Rows.Count; i++)
		{
			for (int j = 0; j < Columns.Count; j++)
			{
				TicketDesignCellVM cell = GetCell(i, j);
				dictionary[cell.CellId] = Tuple.Create(i, j);
			}
		}
		foreach (Tuple<int, int> merge in mergeList)
		{
			if (dictionary.TryGetValue(merge.Item1, out var value) && dictionary.TryGetValue(merge.Item2, out var value2))
			{
				int topRow = Math.Min(value.Item1, value2.Item1);
				int bottomRow = Math.Max(value.Item1, value2.Item1);
				int leftColumn = Math.Min(value.Item2, value2.Item2);
				int rightColumn = Math.Max(value.Item2, value2.Item2);
				TicketMerge item = new TicketMerge
				{
					TopRow = topRow,
					LeftColumn = leftColumn,
					BottomRow = bottomRow,
					RightColumn = rightColumn
				};
				Merges.Add(item);
			}
		}
	}

	public int MoveColumnLeft(int vmColStartIndex, int moveCount, out int afterMoveVMColStartIndex)
	{
		afterMoveVMColStartIndex = vmColStartIndex;
		if (moveCount == 0)
		{
			return 0;
		}
		if (vmColStartIndex <= 0 || vmColStartIndex >= Columns.Count)
		{
			return 0;
		}
		if (vmColStartIndex + moveCount > Columns.Count)
		{
			return 0;
		}
		List<Tuple<int, int>> ticketMergeInCellId = GetTicketMergeInCellId();
		int count = Cells.Count;
		int count2 = Columns.Count;
		List<TicketDesignCellVM> list = new List<TicketDesignCellVM>(Cells);
		List<TicketDesignColumnVM> list2 = new List<TicketDesignColumnVM>(Columns);
		Cells.Clear();
		Cells.AddRange(new TicketDesignCellVM[count]);
		Columns.Clear();
		Columns.AddRange(new TicketDesignColumnVM[count2]);
		for (int i = 0; i < count2; i++)
		{
			int index = GetBeforeMoveColIndex(i);
			Columns[i] = list2[index];
		}
		for (int j = 0; j < Rows.Count; j++)
		{
			for (int k = 0; k < count2; k++)
			{
				int index2 = j * Columns.Count + k;
				int num = GetBeforeMoveColIndex(k);
				int index3 = j * Columns.Count + num;
				Cells[index2] = list[index3];
			}
		}
		afterMoveVMColStartIndex = vmColStartIndex - 1;
		RebuildTicketMergeByCellId(ticketMergeInCellId);
		return moveCount;
		int GetBeforeMoveColIndex(int afterMoveColIndex)
		{
			int num2 = afterMoveColIndex;
			if (afterMoveColIndex < vmColStartIndex - 1)
			{
				return afterMoveColIndex;
			}
			if (afterMoveColIndex < vmColStartIndex + moveCount - 1)
			{
				return afterMoveColIndex + 1;
			}
			if (afterMoveColIndex == vmColStartIndex + moveCount - 1)
			{
				return vmColStartIndex - 1;
			}
			return afterMoveColIndex;
		}
	}

	public int MoveColumnRight(int vmColStartIndex, int moveCount, out int afterMoveVMColStartIndex)
	{
		afterMoveVMColStartIndex = vmColStartIndex;
		if (moveCount == 0)
		{
			return 0;
		}
		int num = vmColStartIndex + moveCount - 1;
		if (vmColStartIndex < 0 || num >= Columns.Count - 1)
		{
			return 0;
		}
		List<Tuple<int, int>> ticketMergeInCellId = GetTicketMergeInCellId();
		int count = Cells.Count;
		int count2 = Columns.Count;
		List<TicketDesignCellVM> list = new List<TicketDesignCellVM>(Cells);
		List<TicketDesignColumnVM> list2 = new List<TicketDesignColumnVM>(Columns);
		Cells.Clear();
		Cells.AddRange(new TicketDesignCellVM[count]);
		Columns.Clear();
		Columns.AddRange(new TicketDesignColumnVM[count2]);
		for (int i = 0; i < count2; i++)
		{
			int index = GetBeforeMoveColIndex(i);
			Columns[i] = list2[index];
		}
		for (int j = 0; j < Rows.Count; j++)
		{
			for (int k = 0; k < count2; k++)
			{
				int index2 = j * Columns.Count + k;
				int num2 = GetBeforeMoveColIndex(k);
				int index3 = j * Columns.Count + num2;
				Cells[index2] = list[index3];
			}
		}
		afterMoveVMColStartIndex = vmColStartIndex + 1;
		RebuildTicketMergeByCellId(ticketMergeInCellId);
		return moveCount;
		int GetBeforeMoveColIndex(int afterMoveColIndex)
		{
			int num3 = afterMoveColIndex;
			if (afterMoveColIndex < vmColStartIndex)
			{
				return afterMoveColIndex;
			}
			if (afterMoveColIndex == vmColStartIndex)
			{
				return vmColStartIndex + moveCount;
			}
			if (afterMoveColIndex <= vmColStartIndex + moveCount)
			{
				return afterMoveColIndex - 1;
			}
			return afterMoveColIndex;
		}
	}

	public TicketDesignValidation Save()
	{
		if (!CheckCellFormulaIsValid())
		{
			return new TicketDesignValidation
			{
				Kind = (int)TicketKind.None,
				FailureReason = TicketDesignFailureReason.InvalidFormula
			};
		}
		TicketDesignValidation ret = Validate();
		PostValidate(ret);
		if (ret.Success)
		{
			_ticket.Columns.Clear();
			_ticket.Rows.Clear();
			_ticket.Cells.Clear();
			_ticket.Merges.Clear();
			_ticket.DataRowMerges.Clear();
			_ticket.Kind = (TicketKind)ret.Kind;
			_ticket.Title.Clear();
			_ticket.Footer.Clear();
			_ticket.TableRowsFrozenCount = 0;
			_ticket.TableColsFrozenCount = 0;
			_ticket.ColumnHeaderRowsCount = 0;
			_ticket.FixedAndDynamicMixRange = null;
			if (ret.Kind != 0)
			{
				SaveTitleFooter(Title, _ticket.Title, ret.TitleDicField);
				SaveTitleFooter(Footer, _ticket.Footer, ret.FooterDicField);
				_ticket.TableRowsFrozenCount = Math.Min(Rows.Count, TableRowsFrozenCount);
				_ticket.TableColsFrozenCount = Math.Min(Columns.Count, TableColsFrozenCount);
				_ticket.ColumnHeaderRowsCount = ColumnHeaderRowsCount;
			}
			if (ret.Kind != 0)
			{
				if (ret.Kind == (int)TicketKind.DynamicRow)
				{
					for (int i = 0; i < Columns.Count; i++)
					{
						TicketDesignCellVM cell = GetCell(ret.GroupStartRow, i);
						TicketDesignCellVM cell2 = GetCell(ret.GroupEndRow, i);
						TicketColumn ticketColumn = new TicketColumn
						{
							Width = Columns[i].Width,
							FontFamily = cell.FontFamily,
							FontSize = cell.FontSize,
							ForeColor = cell.ForeColor,
							Bold = cell.Bold,
							Italic = cell.Italic,
							BackColor = cell.BackColor,
							Align = cell.Align,
							Top = cell.Top.Clone(),
							Right = cell.Right.Clone(),
							Middle = cell.Bottom.Clone(),
							Left = cell.Left.Clone(),
							Bottom = cell2.Bottom.Clone(),
							Formula = (cell.Formula ?? ""),
							DataFormat = cell.DataFormat,
							IsHiddenColumn = Columns[i].IsHiddenColumn
						};
						if (ret.DicField.TryGetValue(cell, out var value))
						{
							ticketColumn.Field = value.Field;
						}
						_ticket.Columns.Add(ticketColumn);
					}
					for (int j = 0; j < ret.GroupStartRow; j++)
					{
						_ticket.Rows.Add(new TicketRow
						{
							Height = Rows[j].Height
						});
						for (int k = 0; k < Columns.Count; k++)
						{
							TicketDesignCellVM cell3 = GetCell(j, k);
							TicketCell ticketCell = cell3.ToModel();
							if (ret.DicField.TryGetValue(cell3, out var value2))
							{
								value2.WriteTo(ticketCell);
							}
							_ticket.Cells.Add(ticketCell);
						}
					}
					_ticket.DataRowStart = ret.GroupStartRow;
					_ticket.DataRowCount = ret.GroupEndRow - ret.GroupStartRow + 1;
					_ticket.DataRowHeight = Rows[ret.GroupStartRow].Height;
					for (int l = ret.GroupEndRow + 1; l < Rows.Count; l++)
					{
						_ticket.Rows.Add(new TicketRow
						{
							Height = Rows[l].Height
						});
						for (int n = 0; n < Columns.Count; n++)
						{
							TicketDesignCellVM cell4 = GetCell(l, n);
							TicketCell ticketCell2 = cell4.ToModel();
							if (ret.DicField.TryGetValue(cell4, out var value3))
							{
								value3.WriteTo(ticketCell2);
							}
							_ticket.Cells.Add(ticketCell2);
						}
					}
					foreach (TicketMerge item in Merges.Where((TicketMerge m) => m.TopRow < ret.GroupStartRow || m.TopRow > ret.GroupEndRow))
					{
						_ticket.Merges.Add(new TicketMerge
						{
							TopRow = VMRowToTicketRow(item.TopRow),
							BottomRow = VMRowToTicketRow(item.BottomRow),
							LeftColumn = item.LeftColumn,
							RightColumn = item.RightColumn
						});
					}
					foreach (Tuple<int, int> dataMergeCol in ret.DataMergeCols)
					{
						_ticket.DataRowMerges.Add(new TicketMerge
						{
							TopRow = 0,
							BottomRow = 0,
							LeftColumn = dataMergeCol.Item1,
							RightColumn = dataMergeCol.Item2
						});
					}
					foreach (TicketCell cell10 in _ticket.Cells)
					{
						if (cell10.HasFormula())
						{
							FormulaEvaluator formulaEvaluator = new FormulaEvaluator(cell10.Formula);
							cell10.Formula = formulaEvaluator.RewriteDynamicRowTicket(_ticket.DataRowStart, _ticket.DataRowCount);
						}
					}
					foreach (TicketCell cell11 in _ticket.Title.Cells)
					{
						if (cell11.HasFormula())
						{
							FormulaEvaluator formulaEvaluator2 = new FormulaEvaluator(cell11.Formula);
							cell11.Formula = formulaEvaluator2.RewriteDynamicRowTicket(_ticket.DataRowStart, _ticket.DataRowCount);
						}
					}
					foreach (TicketCell cell12 in _ticket.Footer.Cells)
					{
						if (cell12.HasFormula())
						{
							FormulaEvaluator formulaEvaluator3 = new FormulaEvaluator(cell12.Formula);
							cell12.Formula = formulaEvaluator3.RewriteDynamicRowTicket(_ticket.DataRowStart, _ticket.DataRowCount);
						}
					}
				}
				else if (ret.Kind == (int)TicketKind.FixedOneRow || ret.Kind == (int)TicketKind.FixedMultiRow)
				{
					_ticket.DataRowStart = -1;
					_ticket.DataRowCount = 0;
					_ticket.DataRowHeight = 0;
					for (int num = 0; num < Columns.Count; num++)
					{
						_ticket.Columns.Add(new TicketColumn
						{
							Width = Columns[num].Width,
							IsHiddenColumn = Columns[num].IsHiddenColumn
						});
					}
					for (int num2 = 0; num2 < Rows.Count; num2++)
					{
						_ticket.Rows.Add(new TicketRow
						{
							Height = Rows[num2].Height
						});
						for (int num3 = 0; num3 < Columns.Count; num3++)
						{
							TicketDesignCellVM cell5 = GetCell(num2, num3);
							TicketCell ticketCell3 = cell5.ToModel();
							if (ret.DicField.TryGetValue(cell5, out var value4))
							{
								value4.WriteTo(ticketCell3);
							}
							_ticket.Cells.Add(ticketCell3);
						}
					}
					foreach (TicketMerge merge in Merges)
					{
						_ticket.Merges.Add(new TicketMerge
						{
							TopRow = merge.TopRow,
							BottomRow = merge.BottomRow,
							LeftColumn = merge.LeftColumn,
							RightColumn = merge.RightColumn
						});
						TicketBorder top = _ticket.GetCell(merge.TopRow, merge.LeftColumn).Top;
						TicketBorder bottom = _ticket.GetCell(merge.TopRow, merge.LeftColumn).Bottom;
						for (int num4 = merge.LeftColumn + 1; num4 <= merge.RightColumn; num4++)
						{
							_ticket.GetCell(merge.TopRow, num4).Top = top.Clone();
							_ticket.GetCell(merge.TopRow, num4).Bottom = bottom.Clone();
						}
					}
				}
				else if (ret.Kind == (int)TicketKind.FixedDataRowMixDynamicDataRow)
				{
					_ticket.DataRowStart = -1;
					_ticket.DataRowCount = 0;
					_ticket.DataRowHeight = 0;
					_ticket.FixedAndDynamicMixRange = new TicketTableFixedAndDynamicMixRange();
					for (int num5 = 0; num5 < Columns.Count; num5++)
					{
						_ticket.Columns.Add(new TicketColumn
						{
							Width = Columns[num5].Width,
							IsHiddenColumn = Columns[num5].IsHiddenColumn
						});
					}
					for (int num6 = 0; num6 < Rows.Count; num6++)
					{
						_ticket.Rows.Add(new TicketRow
						{
							Height = Rows[num6].Height
						});
						for (int num7 = 0; num7 < Columns.Count; num7++)
						{
							TicketDesignCellVM cell6 = GetCell(num6, num7);
							TicketCell ticketCell4 = cell6.ToModel();
							if (ret.DicField.TryGetValue(cell6, out var value5))
							{
								value5.WriteTo(ticketCell4);
							}
							_ticket.Cells.Add(ticketCell4);
							TicketDesignValidation.FieldCellSetting fieldCellSetting = ret.MixTicketCellSettingList[num6, num7];
							ticketCell4.IsInMixRangeDynamicDataRow = fieldCellSetting.IsInDynamicDataRow;
							ticketCell4.IsInMixRangeFixedDataRow = fieldCellSetting.IsInFixedDataRow;
							ticketCell4.IsMixRangeTicketKey = fieldCellSetting.IsTicketKey;
						}
					}
					List<TicketTableMixRangeTemplateRow> list = new List<TicketTableMixRangeTemplateRow>();
					_ticket.FixedAndDynamicMixRange.DynamicDataRowTemplateRows = list;
					foreach (TicketDesignValidation.FixedAndDynamicRowMixRange mixRange in ret.MixRangeList)
					{
						foreach (Tuple<int, int> dynamicRows in mixRange.DynamicRowsList)
						{
							TicketTableMixRangeTemplateRow ticketTableMixRangeTemplateRow = new TicketTableMixRangeTemplateRow();
							ticketTableMixRangeTemplateRow.RefTicketTableRowIndex = dynamicRows.Item1;
							ticketTableMixRangeTemplateRow.BottomBorderRefTicketTableRowIndex = dynamicRows.Item1 + dynamicRows.Item2 - 1;
							ticketTableMixRangeTemplateRow.TemplateId = list.Count + 1;
							list.Add(ticketTableMixRangeTemplateRow);
							for (int num8 = 0; num8 < dynamicRows.Item2; num8++)
							{
								int index = dynamicRows.Item1 + num8;
								_ticket.Rows[index].MixRangeDynamicDataRowTemplateId = ticketTableMixRangeTemplateRow.TemplateId;
								_ticket.Rows[index].IsMixRangeDynamicDataRow = true;
							}
						}
						foreach (int fixedRows in mixRange.FixedRowsList)
						{
							_ticket.Rows[fixedRows].IsMixRangeFixedDataRow = true;
						}
					}
					int num9 = 1;
					Dictionary<TicketTableMixRangeDataGroupKey, TicketTableMixRangeDataGroupKey> dictionary = new Dictionary<TicketTableMixRangeDataGroupKey, TicketTableMixRangeDataGroupKey>(new DataGroupKeyComparer());
					foreach (TicketDesignValidation.FixedAndDynamicRowMixRange mixRange2 in ret.MixRangeList)
					{
						foreach (int fixedRows2 in mixRange2.FixedRowsList)
						{
							List<TicketTableMixRangeDataGroupKeyItem> list2 = new List<TicketTableMixRangeDataGroupKeyItem>();
							for (int num10 = 0; num10 < Columns.Count; num10++)
							{
								TicketDesignCellVM cell7 = GetCell(fixedRows2, num10);
								if (ret.DicField.TryGetValue(cell7, out var value6))
								{
									string text = "";
									if (!string.IsNullOrEmpty(value6.InputValue))
									{
										text = value6.InputValue;
									}
									else if (!string.IsNullOrEmpty(value6.Text))
									{
										text = value6.Text;
									}
									if (!string.IsNullOrEmpty(text))
									{
										list2.Add(new TicketTableMixRangeDataGroupKeyItem
										{
											TableColumnId = value6.Field,
											TableColumnValue = text
										});
									}
								}
							}
							TicketTableMixRangeDataGroupKey ticketTableMixRangeDataGroupKey = GenerateMixRangeDataGroupKey(list2);
							if (!dictionary.TryGetValue(ticketTableMixRangeDataGroupKey, out var value7))
							{
								ticketTableMixRangeDataGroupKey.KeyId = num9++;
								ticketTableMixRangeDataGroupKey.TickeRowIndex = fixedRows2;
								dictionary.Add(ticketTableMixRangeDataGroupKey, ticketTableMixRangeDataGroupKey);
							}
							else
							{
								if (value7.TickeRowIndex > fixedRows2)
								{
									value7.TickeRowIndex = fixedRows2;
								}
								ticketTableMixRangeDataGroupKey = value7;
							}
							_ticket.Rows[fixedRows2].MixRangeDataKeyId = ticketTableMixRangeDataGroupKey.KeyId;
						}
					}
					_ticket.FixedAndDynamicMixRange.DataGroupKeyListForFixedDataRow = dictionary.Values.ToList();
					_ticket.FixedAndDynamicMixRange.DataGroupKeyListForFixedDataRow.Sort(TicketTableMixRangeDataGroupKeyCompare);
					Dictionary<TicketTableMixRangeDataGroupKey, TicketTableMixRangeDataGroupKey> dictionary2 = new Dictionary<TicketTableMixRangeDataGroupKey, TicketTableMixRangeDataGroupKey>(new DataGroupKeyComparer());
					foreach (TicketTableMixRangeTemplateRow dynamicDataRowTemplateRow in _ticket.FixedAndDynamicMixRange.DynamicDataRowTemplateRows)
					{
						int refTicketTableRowIndex = dynamicDataRowTemplateRow.RefTicketTableRowIndex;
						List<TicketTableMixRangeDataGroupKeyItem> list3 = new List<TicketTableMixRangeDataGroupKeyItem>();
						for (int num11 = 0; num11 < Columns.Count; num11++)
						{
							TicketDesignCellVM cell8 = GetCell(refTicketTableRowIndex, num11);
							if (ret.DicField.TryGetValue(cell8, out var value8))
							{
								string text2 = "";
								if (!string.IsNullOrEmpty(value8.InputValue))
								{
									text2 = value8.InputValue;
								}
								else if (!string.IsNullOrEmpty(value8.Text))
								{
									text2 = value8.Text;
								}
								if (!string.IsNullOrEmpty(text2))
								{
									list3.Add(new TicketTableMixRangeDataGroupKeyItem
									{
										TableColumnId = value8.Field,
										TableColumnValue = text2
									});
								}
							}
						}
						TicketTableMixRangeDataGroupKey ticketTableMixRangeDataGroupKey2 = GenerateMixRangeDataGroupKey(list3);
						if (!dictionary2.TryGetValue(ticketTableMixRangeDataGroupKey2, out var value9))
						{
							ticketTableMixRangeDataGroupKey2.KeyId = num9++;
							ticketTableMixRangeDataGroupKey2.TickeRowIndex = refTicketTableRowIndex;
							dictionary2.Add(ticketTableMixRangeDataGroupKey2, ticketTableMixRangeDataGroupKey2);
						}
						else
						{
							if (value9.TickeRowIndex > refTicketTableRowIndex)
							{
								value9.TickeRowIndex = refTicketTableRowIndex;
							}
							ticketTableMixRangeDataGroupKey2 = value9;
						}
						_ticket.Rows[refTicketTableRowIndex].MixRangeDataKeyId = ticketTableMixRangeDataGroupKey2.KeyId;
						_ticket.Rows[refTicketTableRowIndex].IsMixRangeTemplateRow = true;
						dynamicDataRowTemplateRow.DataGroupKeyId = ticketTableMixRangeDataGroupKey2.KeyId;
					}
					_ticket.FixedAndDynamicMixRange.DataGroupKeyListForDynamicDataRow = dictionary2.Values.ToList();
					_ticket.FixedAndDynamicMixRange.DataGroupKeyListForDynamicDataRow.Sort(TicketTableMixRangeDataGroupKeyCompare);
					foreach (TicketMerge merge2 in Merges)
					{
						if (ContainDynamicDataRow(merge2, out var dynamicDataRowIndex2))
						{
							TicketRow dynamicRow = _ticket.Rows[dynamicDataRowIndex2];
							TicketTableMixRangeTemplateRow templateRow = _ticket.FixedAndDynamicMixRange.DynamicDataRowTemplateRows.FirstOrDefault((TicketTableMixRangeTemplateRow u) => u.TemplateId == dynamicRow.MixRangeDynamicDataRowTemplateId);
							if (templateRow == null || (templateRow.Merges != null && templateRow.Merges.Count != 0))
							{
								continue;
							}
							templateRow.Merges = new List<TicketMerge>();
							foreach (TicketMerge item2 in Merges.Where((TicketMerge u) => _ticket.Rows[u.TopRow].MixRangeDynamicDataRowTemplateId == templateRow.TemplateId))
							{
								TicketMerge newMergeArea = new TicketMerge
								{
									TopRow = dynamicDataRowIndex2,
									BottomRow = dynamicDataRowIndex2,
									LeftColumn = merge2.LeftColumn,
									RightColumn = merge2.RightColumn
								};
								if (!templateRow.Merges.Any((TicketMerge u) => u.IntersectsWith(newMergeArea)))
								{
									templateRow.Merges.Add(newMergeArea);
								}
							}
						}
						else
						{
							_ticket.Merges.Add(new TicketMerge
							{
								TopRow = GetRowIndexExcludeDynamicRow(merge2.TopRow),
								BottomRow = GetRowIndexExcludeDynamicRow(merge2.BottomRow),
								LeftColumn = merge2.LeftColumn,
								RightColumn = merge2.RightColumn
							});
						}
						TicketBorder top2 = _ticket.GetCell(merge2.TopRow, merge2.LeftColumn).Top;
						TicketBorder bottom2 = _ticket.GetCell(merge2.TopRow, merge2.LeftColumn).Bottom;
						for (int num12 = merge2.LeftColumn + 1; num12 <= merge2.RightColumn; num12++)
						{
							_ticket.GetCell(merge2.TopRow, num12).Top = top2.Clone();
							_ticket.GetCell(merge2.TopRow, num12).Bottom = bottom2.Clone();
						}
					}
					int num13 = 0;
					foreach (TicketTableMixRangeTemplateRow dynamicDataRowTemplateRow2 in _ticket.FixedAndDynamicMixRange.DynamicDataRowTemplateRows)
					{
						List<long> list4 = new List<long>();
						for (int num14 = 0; num14 < _ticket.Columns.Count; num14++)
						{
							list4.Add(num13++);
						}
						dynamicDataRowTemplateRow2.TicketColumnIdList = list4;
					}
					foreach (TicketCell cell13 in _ticket.Cells)
					{
						if (cell13.HasFormula())
						{
							FormulaEvaluator formulaEvaluator4 = new FormulaEvaluator(cell13.Formula);
							cell13.Formula = formulaEvaluator4.RewriteMixTicket(IsMixRangeDynamicDataRow, GetMixRangeTicketColumnId, GetRowIndexExcludeDynamicRow);
						}
					}
					foreach (TicketCell cell14 in _ticket.Title.Cells)
					{
						if (cell14.HasFormula())
						{
							FormulaEvaluator formulaEvaluator5 = new FormulaEvaluator(cell14.Formula);
							cell14.Formula = formulaEvaluator5.RewriteMixTicket(IsMixRangeDynamicDataRow, GetMixRangeTicketColumnId, GetRowIndexExcludeDynamicRow);
						}
					}
					foreach (TicketCell cell15 in _ticket.Footer.Cells)
					{
						if (cell15.HasFormula())
						{
							FormulaEvaluator formulaEvaluator6 = new FormulaEvaluator(cell15.Formula);
							cell15.Formula = formulaEvaluator6.RewriteMixTicket(IsMixRangeDynamicDataRow, GetMixRangeTicketColumnId, GetRowIndexExcludeDynamicRow);
						}
					}
				}
			}
			_ticket.Table.TagTicketDirty();
			_ticket.IsCacheExpired = true;
		}
		else
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, TicketDesignFailureReasonMessage(ret.FailureReason));
		}
		return ret;
		bool ContainDynamicDataRow(TicketMerge mergeRange, out int dynamicDataRowIndex)
		{
			for (int num17 = mergeRange.TopRow; num17 <= mergeRange.BottomRow; num17++)
			{
				if (_ticket.Rows[num17].IsMixRangeDynamicDataRow)
				{
					dynamicDataRowIndex = num17;
					return true;
				}
			}
			dynamicDataRowIndex = -1;
			return false;
		}
		long GetMixRangeTicketColumnId(int rowIndex, int colIndex)
		{
			int ticketRowTemplateId = _ticket.Rows[rowIndex].MixRangeDynamicDataRowTemplateId;
			TicketTableMixRangeTemplateRow ticketTableMixRangeTemplateRow2 = _ticket.FixedAndDynamicMixRange.DynamicDataRowTemplateRows.Find((TicketTableMixRangeTemplateRow u) => u.TemplateId == ticketRowTemplateId);
			return ticketTableMixRangeTemplateRow2.TicketColumnIdList[colIndex];
		}
		int GetRowIndexExcludeDynamicRow(int rowIndex)
		{
			int num15 = -1;
			for (int num16 = 0; num16 <= rowIndex; num16++)
			{
				if (!_ticket.Rows[num16].IsMixRangeDynamicDataRow)
				{
					num15++;
				}
			}
			return num15;
		}
		bool IsMixRangeDynamicDataRow(int rowIndex)
		{
			if (_ticket.Rows[rowIndex].IsMixRangeDynamicDataRow)
			{
				return true;
			}
			return false;
		}
		static void SaveTitleFooter(TicketDesignTitleFooterVM src, TicketTitleFooter dst, Dictionary<object, TicketDesignValidation.TempField> fieldDic)
		{
			for (int num18 = 0; num18 < src.Columns.Count; num18++)
			{
				dst.Columns.Add(new TicketColumn
				{
					Width = src.Columns[num18].Width
				});
			}
			for (int num19 = 0; num19 < src.Rows.Count; num19++)
			{
				dst.Rows.Add(new TicketRow
				{
					Height = src.Rows[num19].Height
				});
				for (int num20 = 0; num20 < src.Columns.Count; num20++)
				{
					TicketDesignCellVM cell9 = src.GetCell(num19, num20);
					TicketCell ticketCell5 = cell9.ToModel();
					if (fieldDic.TryGetValue(cell9, out var value10))
					{
						value10.WriteTo(ticketCell5);
					}
					dst.Cells.Add(ticketCell5);
				}
			}
			foreach (TicketMerge merge3 in src.Merges)
			{
				dst.Merges.Add(new TicketMerge
				{
					TopRow = merge3.TopRow,
					BottomRow = merge3.BottomRow,
					LeftColumn = merge3.LeftColumn,
					RightColumn = merge3.RightColumn
				});
			}
		}
		int VMRowToTicketRow(int row)
		{
			if (row < ret.GroupStartRow)
			{
				return row;
			}
			if (row < ret.GroupStartRow + _ticket.DataRowCount)
			{
				return ret.GroupStartRow;
			}
			return row - _ticket.DataRowCount;
		}
	}

	private int TicketTableMixRangeDataGroupKeyCompare(TicketTableMixRangeDataGroupKey left, TicketTableMixRangeDataGroupKey right)
	{
		if (left.KeyItems.Count < right.KeyItems.Count)
		{
			return 1;
		}
		if (left.KeyItems.Count > right.KeyItems.Count)
		{
			return -1;
		}
		return left.TickeRowIndex.CompareTo(right.TickeRowIndex);
	}

	private TicketTableMixRangeDataGroupKey GenerateMixRangeDataGroupKey(List<TicketTableMixRangeDataGroupKeyItem> keyItems)
	{
		List<TicketTableMixRangeDataGroupKeyItem> list = new List<TicketTableMixRangeDataGroupKeyItem>(keyItems);
		list.Sort((TicketTableMixRangeDataGroupKeyItem left, TicketTableMixRangeDataGroupKeyItem right) => left.TableColumnId.Value.CompareTo(right.TableColumnId.Value));
		TicketTableMixRangeDataGroupKey ticketTableMixRangeDataGroupKey = new TicketTableMixRangeDataGroupKey();
		ticketTableMixRangeDataGroupKey.KeyItems = list;
		return ticketTableMixRangeDataGroupKey;
	}

	public void MergeCells(int r1, int c1, int r2, int c2)
	{
		Merges.RemoveAll((TicketMerge m) => m.IntersectsWith(r1, c1, r2, c2));
		Merges.Add(new TicketMerge
		{
			TopRow = r1,
			BottomRow = r2,
			LeftColumn = c1,
			RightColumn = c2
		});
		TicketDesignCellVM ticketDesignCellVM = EnumerateRange(r1, c1, r2, c2).FirstOrDefault((TicketDesignCellVM c) => c.Text != "");
		GetCell(r1, c1).Text = ticketDesignCellVM?.Text ?? "";
		foreach (TicketDesignCellVM item in EnumerateRange(r1, c1, r2, c2).Skip(1))
		{
			item.Text = "";
		}
	}

	public void SanitizeImportTable()
	{
		foreach (TicketDesignCellVM cell in Cells)
		{
			Match match = _rxValidate.Match(cell.Text);
			System.Text.RegularExpressions.Group group = match.Groups[2];
			if (match.Success && group.Success)
			{
				int startIndex = group.Index - 1;
				int num = group.Length + 2;
				System.Text.RegularExpressions.Group group2 = match.Groups[3];
				if (group2.Success)
				{
					num += group2.Length;
				}
				cell.Text = cell.Text.Remove(startIndex, num);
			}
		}
		Title.SanitizeImportTable();
		Footer.SanitizeImportTable();
	}

	[IteratorStateMachine(typeof(_003CEnumerateRange_003Ed__69))]
	private IEnumerable<TicketDesignCellVM> EnumerateRange(int r1, int c1, int r2, int c2)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CEnumerateRange_003Ed__69(-2)
		{
			_003C_003E4__this = this,
			_003C_003E3__r1 = r1,
			_003C_003E3__c1 = c1,
			_003C_003E3__r2 = r2,
			_003C_003E3__c2 = c2
		};
	}

	public void Clear()
	{
		Columns.Clear();
		Rows.Clear();
		Cells.Clear();
		Merges.Clear();
	}

	public void CopyFrom(TicketDesignTableVM rhs)
	{
		Clear();
		Columns.AddRange(rhs.Columns);
		Rows.AddRange(rhs.Rows);
		Cells.AddRange(rhs.Cells);
		Merges.AddRange(rhs.Merges);
		Title.CopyFrom(rhs.Title);
		Footer.CopyFrom(rhs.Footer);
	}

	private TicketDesignCellVM GetCellVM(TicketCell tc)
	{
		TicketDesignCellVM ticketDesignCellVM = new TicketDesignCellVM
		{
			Text = tc.Text,
			FontFamily = tc.FontFamily,
			FontSize = tc.FontSize,
			ForeColor = tc.ForeColor,
			Bold = tc.Bold,
			Italic = tc.Italic,
			BackColor = tc.BackColor,
			Align = tc.Align,
			Top = tc.Top.Clone(),
			Right = tc.Right.Clone(),
			Bottom = tc.Bottom.Clone(),
			Left = tc.Left.Clone(),
			Formula = tc.Formula,
			Indent = tc.Indent,
			DataFormat = tc.DataFormat
		};
		if (tc.HasField())
		{
			Leqisoft.Model.Column byId = _ticket.Table.Columns.GetById(tc.Field);
			if (byId != null)
			{
				ticketDesignCellVM.Text = ticketDesignCellVM.Text + "[" + byId.GetUniqueFormulaName() + "]";
			}
			if (!string.IsNullOrEmpty(tc.InputValue))
			{
				ticketDesignCellVM.Text = ticketDesignCellVM.Text + "=\"" + tc.InputValue + "\"";
			}
		}
		return ticketDesignCellVM;
	}

	private string TicketDesignFailureReasonMessage(TicketDesignFailureReason r)
	{
		switch (r)
		{
		case TicketDesignFailureReason.NoField:
			return "表单的表体区中未定义对应的表格列，请定义后再保存表单样式！";
		case TicketDesignFailureReason.InvalidStartRow:
		case TicketDesignFailureReason.RowNotContinuous:
		case TicketDesignFailureReason.InvalidCol:
		case TicketDesignFailureReason.InvalidEndRow:
		case TicketDesignFailureReason.InvalidDataRowVerticalMerge:
		case TicketDesignFailureReason.InvalidDataRowMergeStartRow:
		case TicketDesignFailureReason.DataRowMergeNotContinuous:
		case TicketDesignFailureReason.DataRowMergeInvalidRightCol:
		case TicketDesignFailureReason.InvalidDataRowMergeEndRow:
		case TicketDesignFailureReason.InvalidFormula:
		case TicketDesignFailureReason.DataRowContainsKeyCell:
		case TicketDesignFailureReason.DataRowMergeInvalidLeftCol:
			return "变动行中定义的对应表格列不符合规范";
		case TicketDesignFailureReason.FieldCountNotEqual:
		case TicketDesignFailureReason.FieldCellsNotSameKind:
			return "报表设计对应表格列的定义不符合规范";
		case TicketDesignFailureReason.LevelNotSupported:
			return "表单设计模式下不支持设计报表";
		case TicketDesignFailureReason.DataPartNothing:
			return "表体区部分不允许为空";
		case TicketDesignFailureReason.TitlePartIncludeDataPartField:
			return "标题区不允许使用表体区已经占用了的表格列";
		case TicketDesignFailureReason.FooterPartIncludeDataPartField:
			return "表底区不允许使用表体区已经占用了的表格列";
		case TicketDesignFailureReason.TitlePartExistRepeatField:
			return "标题区存在重复的表格列";
		case TicketDesignFailureReason.FooterPartExistRepeatField:
			return "表底区存在重复的表格列";
		case TicketDesignFailureReason.FooterPartExistTitleRepeatField:
			return "标题区和表底区存在重复的表格列";
		case TicketDesignFailureReason.ColumnHeaderRowIncludeField:
			return "被设置为列头的区域内不允许使用表格列";
		case TicketDesignFailureReason.ColumnHeaderRowExistFormula:
			return "被设置为列头的区域内不允许使用公式";
		case TicketDesignFailureReason.MergeRangeCrossColumnHeaderAndDataArea:
			return "合并单元格不允许同时包含列头区域和非列头区域";
		case TicketDesignFailureReason.MergeRangeCrossDynamicRowAndFixedRow:
			return "合并单元格不允许同时包含变动行和非变动行";
		case TicketDesignFailureReason.WriteTicketFormulaDataToTableCell:
			return "表单公式的运算结果不允许写入到表格列";
		default:
			return "其他错误";
		}
	}
}
