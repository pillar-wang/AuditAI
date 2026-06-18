using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Leqisoft.Model;

namespace Leqisoft.UI.Platform;

public class TicketDesignTitleFooterVM
{
	[CompilerGenerated]
	private sealed class _003CEnumerateRange_003Ed__31 : IEnumerable<TicketDesignCellVM>, IEnumerable, IEnumerator<TicketDesignCellVM>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private TicketDesignCellVM _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private int r1;

		public int _003C_003E3__r1;

		private int c1;

		public int _003C_003E3__c1;

		public TicketDesignTitleFooterVM _003C_003E4__this;

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
		public _003CEnumerateRange_003Ed__31(int _003C_003E1__state)
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
			TicketDesignTitleFooterVM ticketDesignTitleFooterVM = _003C_003E4__this;
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
				_003C_003E2__current = ticketDesignTitleFooterVM.GetCell(_003Ci_003E5__2, _003Cj_003E5__3);
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
			_003CEnumerateRange_003Ed__31 _003CEnumerateRange_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CEnumerateRange_003Ed__ = this;
			}
			else
			{
				_003CEnumerateRange_003Ed__ = new _003CEnumerateRange_003Ed__31(0)
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

	private static readonly Regex _rxValidate = new Regex("(.*)\\[(.+)](=\"(.+)\")?$", RegexOptions.Singleline);

	protected TicketTable _ticket;

	public List<TicketDesignCellVM> Cells { get; } = new List<TicketDesignCellVM>();


	public List<TicketDesignColumnVM> Columns { get; } = new List<TicketDesignColumnVM>();


	public List<TicketDesignRowVM> Rows { get; } = new List<TicketDesignRowVM>();


	public List<TicketMerge> Merges { get; } = new List<TicketMerge>();


	public TicketDesignTitleFooterVM(TicketTable ticket)
	{
		_ticket = ticket;
		Init(null);
	}

	public TicketDesignTitleFooterVM(TicketTable ticket, TicketTitleFooter setting)
	{
		_ticket = ticket;
		Init(setting);
	}

	private void Init(TicketTitleFooter setting)
	{
		if (setting == null)
		{
			return;
		}
		foreach (TicketColumn column in setting.Columns)
		{
			Columns.Add(new TicketDesignColumnVM
			{
				Width = column.Width
			});
		}
		foreach (TicketRow row in setting.Rows)
		{
			Rows.Add(new TicketDesignRowVM
			{
				Height = row.Height
			});
		}
		foreach (TicketCell cell in setting.Cells)
		{
			TicketDesignCellVM cellVM = GetCellVM(cell);
			Cells.Add(cellVM);
		}
		foreach (TicketMerge merge in setting.Merges)
		{
			Merges.Add(new TicketMerge
			{
				TopRow = merge.TopRow,
				BottomRow = merge.BottomRow,
				LeftColumn = merge.LeftColumn,
				RightColumn = merge.RightColumn
			});
		}
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
							Width = 0
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
							Width = 0
						},
						Right = new TicketBorder
						{
							Width = 0
						},
						Top = new TicketBorder
						{
							Width = 0
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

	public string GetCellFormulaError(string strPrefix)
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
						return $"{strPrefix}第{i + 1}行第{j + 1}列的单元格公式不正确!";
					}
				}
			}
		}
		return null;
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
						Width = 0
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
						Width = 0
					},
					Right = new TicketBorder
					{
						Width = 0
					},
					Top = new TicketBorder
					{
						Width = 0
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
	}

	[IteratorStateMachine(typeof(_003CEnumerateRange_003Ed__31))]
	private IEnumerable<TicketDesignCellVM> EnumerateRange(int r1, int c1, int r2, int c2)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CEnumerateRange_003Ed__31(-2)
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

	public void CopyFrom(TicketDesignTitleFooterVM rhs)
	{
		Clear();
		if (rhs != null)
		{
			Columns.AddRange(rhs.Columns);
			Rows.AddRange(rhs.Rows);
			Cells.AddRange(rhs.Cells);
			Merges.AddRange(rhs.Merges);
		}
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
			Column byId = _ticket.Table.Columns.GetById(tc.Field);
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
}
