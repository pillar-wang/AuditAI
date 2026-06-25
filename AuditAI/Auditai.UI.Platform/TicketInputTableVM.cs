﻿﻿﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Auditai.DTO;
using Auditai.Model;
using Auditai.UI.Controls;

namespace Auditai.UI.Platform;

public class TicketInputTableVM
{
	private class TicketFormulaManager
	{
		private readonly TicketInputTableVM _ticket;

		private readonly List<TicketFormulaDependency> _dep;

		public TicketFormulaManager(TicketInputTableVM ticket)
		{
			_ticket = ticket;
			_dep = new List<TicketFormulaDependency>();
			for (int i = 0; i < _ticket.GetRowsCount(); i++)
			{
				for (int j = 0; j < _ticket._columns.Count; j++)
				{
					TicketInputCellVM cellVM = _ticket.GetCellVM(i, j);
					if (cellVM.IsFormula)
					{
						FormulaEvaluator formulaEvaluator = new FormulaEvaluator(cellVM.Formula);
						TicketReferences ticketReferences = formulaEvaluator.GetTicketReferences();
						TicketCellReference host = new TicketCellReference
						{
							Row = i,
							Col = j
						};
						_dep.Add(new TicketFormulaDependency(_ticket)
						{
							Host = host,
							Refs = ticketReferences
						});
					}
				}
			}
		}

		public void CalculateAllHosts()
		{
			HashSet<TicketCellReference> hosts = new HashSet<TicketCellReference>(_dep.Select((TicketFormulaDependency d) => d.Host));
			EvalHostSet(hosts);
		}

		public void CalculateLeaf(TicketCellReference leaf)
		{
			HashSet<TicketCellReference> marking = new HashSet<TicketCellReference>();
			HashSet<TicketCellReference> marked = new HashSet<TicketCellReference>();
			Mark(leaf);
			EvalHostSet(marked);
			void Mark(TicketCellReference c)
			{
				if (!marking.Contains(c))
				{
					marking.Add(c);
					foreach (TicketCellReference item in from d in _dep
						where d.DependsOn(c)
						select d.Host)
					{
						Mark(item);
					}
					marking.Remove(c);
					if (_dep.Any((TicketFormulaDependency d) => d.Host.Equals(c)))
					{
						marked.Add(c);
					}
				}
			}
		}

		public void EvalHostSet(IEnumerable<TicketCellReference> hosts)
		{
			HashSet<TicketCellReference> evaling = new HashSet<TicketCellReference>();
			HashSet<TicketCellReference> evaled = new HashSet<TicketCellReference>();
			foreach (TicketCellReference host in hosts)
			{
				Eval(host);
			}
			void Eval(TicketCellReference c)
			{
				if (!evaling.Contains(c))
				{
					evaling.Add(c);
					TicketFormulaDependency dep = _dep.First((TicketFormulaDependency d) => d.Host.Equals(c));
					foreach (TicketCellReference item in from d in _dep
						where !evaled.Contains(c) && dep.DependsOn(d.Host)
						select d.Host)
					{
						Eval(item);
					}
					EvalImpl();
					evaling.Remove(c);
					evaled.Add(c);
				}
				void EvalImpl()
				{
					TicketInputCellVM cellVM = _ticket.GetCellVM(c.Row, c.Col);
					if (cellVM.IsFormula)
					{
						FormulaEvaluator formulaEvaluator = new FormulaEvaluator(cellVM.Formula)
						{
							Env = _ticket.GetEvalEnv(c.Row, cellVM)
						};
						try
						{
							if (!IsExistFillFormula(formulaEvaluator))
							{
								Operand operand = formulaEvaluator.EvaluateToOperandTicket(_ticket.GetTicketEvalContext());
								cellVM.Value = operand.Evaluate();
								if (cellVM.TempCell != null)
								{
									cellVM.TempCell.Value = cellVM.Value;
								}
							}
						}
						catch (FormulaException)
						{
							cellVM.Value = "(公式出错)";
							if (cellVM.TempCell != null)
							{
								cellVM.TempCell.Value = cellVM.Value;
							}
						}
					}
				}
			}
		}
	}

	private class TicketFormulaDependency
	{
		private readonly TicketInputTableVM _ticket;

		public TicketCellReference Host { get; set; }

		public TicketReferences Refs { get; set; }

		public TicketFormulaDependency(TicketInputTableVM ticket)
		{
			_ticket = ticket;
		}

		public bool DependsOn(TicketCellReference cell)
		{
			if (!Refs.Cell.Any((TicketCellReference c) => c.Equals(cell)) && !Refs.Range.Any((TicketRangeReference r) => r.Contains(cell)))
			{
				return Refs.Column.Any((TicketColumnReference c) => c.Contains(cell, _ticket._ticket.DataRowStart, _ticket._ticket.DataRowStart + _ticket.DataRowsCount - 1));
			}
			return true;
		}

		public override string ToString()
		{
			return $"{Host}=>{Refs}";
		}
	}

	protected class TicketInputDataResolver : FormulaReferenceTicketInputDataResolver
	{
		protected TicketInputTableVM _parent;

		public TicketInputDataResolver(TicketInputTableVM parent)
		{
			_parent = parent;
		}

		public override Auditai.Model.Cell GetTicketTitleCell(int row, int col)
		{
			TicketInputCellVM cellVM;
			try
			{
				cellVM = _parent.Title.GetCellVM(row, col);
			}
			catch (ArgumentOutOfRangeException)
			{
				throw new FormulaBadReferenceException();
			}
			if (cellVM.Column == null)
			{
				return new Auditai.Model.Cell
				{
					Value = cellVM.Value,
					Row = _parent.DummyMR
				};
			}
			return cellVM.TempCell;
		}

		public override Auditai.Model.Cell GetTicketFooterCell(int row, int col)
		{
			TicketInputCellVM cellVM;
			try
			{
				cellVM = _parent.Footer.GetCellVM(row, col);
			}
			catch (ArgumentOutOfRangeException)
			{
				throw new FormulaBadReferenceException();
			}
			if (cellVM.Column == null)
			{
				return new Auditai.Model.Cell
				{
					Value = cellVM.Value,
					Row = _parent.DummyMR
				};
			}
			return cellVM.TempCell;
		}
	}

	private class CalculateTimeWatcher
	{
		private Stopwatch watchForParser = new Stopwatch();

		private Stopwatch watchForBuildEnv = new Stopwatch();

		private Stopwatch watchForEval = new Stopwatch();

		#pragma warning disable CS0414
		private long parseTotalTime;

		private long buldTotalTime;

		private long evalTotalTime;
		#pragma warning restore CS0414

		private long loopTime;

		private TicketInputTableVM _owner;

		public CalculateTimeWatcher(TicketInputTableVM owner)
		{
			_owner = owner;
		}

		public void Reset()
		{
			watchForParser.Reset();
			watchForBuildEnv.Reset();
			watchForEval.Reset();
			parseTotalTime = 0L;
			buldTotalTime = 0L;
			evalTotalTime = 0L;
			loopTime = 0L;
		}

		public void ParseStart()
		{
			watchForParser.Start();
		}

		public void ParseStop()
		{
			watchForParser.Stop();
		}

		public void BuildEnvStart()
		{
			watchForBuildEnv.Start();
		}

		public void BuildEnvStop()
		{
			watchForBuildEnv.Stop();
		}

		public void EvalStart()
		{
			watchForEval.Start();
		}

		public void EvalStop()
		{
			watchForEval.Stop();
		}

		public void InCreaseLoopTime()
		{
			loopTime++;
		}

		public void PrintLog()
		{
		}
	}

	protected class CellFormulaParseData
	{
		public FormulaEvaluator _formulaEvaluator;

		public TicketEvalContext _ticketEvalContext;

		public bool _isExistFillFormula;
	}

	private readonly TicketTable _ticket;

	private readonly TicketRecord _record;

	private readonly List<TicketInputColumnVM> _columns = new List<TicketInputColumnVM>();

	private readonly List<TicketInputRowVM> _rows = new List<TicketInputRowVM>();

	private readonly List<TicketInputCellVM> _cells = new List<TicketInputCellVM>();

	private readonly TicketFormulaManager _fm;

	private readonly Auditai.Model.Row DummyMR = new Auditai.Model.Row
	{
		Creator = Auditai.Model.User.Current.Id
	};

	private readonly Auditai.Model.Column DummyColumn;

	private readonly bool _hasFillingFormula;

	protected HashSet<Auditai.Model.Row> DynamicRowRefTableDataRows = new HashSet<Auditai.Model.Row>();

	public TicketInputTitleFooterVM Title;

	public TicketInputTitleFooterVM Footer;

	public bool IsExistTicketFormulaResultWriteToTableCellFormula;

	protected bool _isBuildTableCellForAllTicketCell;

	public bool IsInShowingVirtualNode;

	protected List<Tuple<TicketInputCellVM, int, int, string>> _inShowingVirtualValueBodyCellList;

	protected List<Tuple<TicketInputCellVM, int, int, string>> _inShowingVirtualValueTitleFooterCellList;

	protected List<Tuple<TicketInputCellVM, int, int, string>> _inShowingVirtualFormulaConstResultBodyCellList;

	protected List<Tuple<TicketInputCellVM, int, int, string>> _inShowingVirtualFormulaConstResultTitleFooterCellList;

	protected Dictionary<int, List<Auditai.Model.Row>> MixRangeDataRowGroupDic;

	protected Dictionary<int, TicketInputRowVM> MixRangeTemplateRowPreFixedRowDic;

	protected HashSet<Auditai.Model.Row> MixTicketDataRowsRefTableRows = new HashSet<Auditai.Model.Row>();

	protected Dictionary<int, TicketTableMixRangeTemplateRow> MixTicketExistMergeTemplateRows;

	protected Dictionary<int, int> MixTicketFixTickRowIndexMapperToVMRowIndexDic;

	protected Dictionary<long, int> MixTicketColumnIdMapperToTemplateRowIdDic;

	protected List<Auditai.Model.Row> _newAddedTableRowsList;

	protected bool _isNeedRecordNewAddedTableRow;

	protected List<TicketInputCellVM> _ticketCellWhichNeedWriteFormulaResultToTableInMixTicket;

	private int _batchingUpdateValueDepth;

	private CalculateTimeWatcher _watcher;

	public HashSet<Auditai.Model.Row> RemovedRows { get; } = new HashSet<Auditai.Model.Row>();


	public TicketRecord TicketRecord => _record;

	public int DataRowsCount { get; private set; }

	public List<TicketMerge> Merges { get; } = new List<TicketMerge>();


	public Auditai.Model.Table Table => _ticket.Table;

	public List<TicketFixedMultiRowVM> FixedMultiRowVMs { get; } = new List<TicketFixedMultiRowVM>();


	public List<TicketInputCellVM> FixedCells { get; } = new List<TicketInputCellVM>();


	protected List<TicketInputCellVM> DynamicRowKeyCells { get; } = new List<TicketInputCellVM>();


	protected Auditai.Model.Row DynamicRowKeyCellsRefEmptyTableRow { get; private set; }

	protected bool _isFixedOneRowTicketRefTableRowBeNewCreated { get; set; }

	public bool IsHasFillingFormula => _hasFillingFormula;

	protected List<TicketInputCellVM> MixTicketKeyCells { get; } = new List<TicketInputCellVM>();


	protected Auditai.Model.Row MixTicketKeyCellsRefEmptyTableRow { get; private set; }

	public bool IsExistLedgerCollectFormula { get; protected set; }

	public TicketInputTableVM(TicketTable ticket, TicketRecord record)
		: this(ticket, record, ticket.Table.Columns.Where((Auditai.Model.Column c) => c.HasFormula).Any(delegate(Auditai.Model.Column c)
		{
			FormulaEvaluator formulaEval = new FormulaEvaluator(c.Formula);
			return IsExistFillFormula(formulaEval);
		}))
	{
	}

	public TicketInputTableVM(TicketTable ticket, TicketRecord record, bool isExistFillingColumnFormula, bool isCalculateTicket = true)
	{
		TicketInputTableVM ticketInputTableVM = this;
		DummyColumn = Auditai.Model.Column.CreateDummyColumn(ticket.Table);
		_hasFillingFormula = isExistFillingColumnFormula;
		_ticket = ticket;
		_record = record;
		_columns.Clear();
		_rows.Clear();
		_cells.Clear();
		Title = new TicketInputTitleFooterVM(ticket, record, ticket.Title, DummyMR);
		Footer = new TicketInputTitleFooterVM(ticket, record, ticket.Footer, DummyMR);
		dynamic keyCellsList = null;
		if (Table != null)
		{
			foreach (Auditai.Model.Column column in Table.Columns)
			{
				if (column.HasLedgerCollectFormula)
				{
					IsExistLedgerCollectFormula = true;
				}
			}
		}
		if (_ticket.Kind == TicketKind.FixedDataRowMixDynamicDataRow)
		{
			InitFixedDataRowMixDynamicDataRowTicket(isCalculateTicket);
			return;
		}
		for (int j = 0; j < _ticket.Columns.Count; j++)
		{
			TicketColumn ticketColumn = _ticket.Columns[j];
			TicketInputColumnVM ticketInputColumnVM = new TicketInputColumnVM
			{
				TicketColumn = ticketColumn,
				TableColumn = _ticket.GetFieldColumn(ticketColumn.Field),
				IsHiddenColumn = ticketColumn.IsHiddenColumn
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
			_columns.Add(ticketInputColumnVM);
		}
		for (int k = 0; k < ticket.Rows.Count; k++)
		{
			TicketInputRowVM item = new TicketInputRowVM
			{
				TicketRow = ticket.Rows[k]
			};
			_rows.Add(item);
			for (int l = 0; l < ticket.Columns.Count; l++)
			{
				TicketCell cell = ticket.GetCell(k, l);
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
						if (record == null)
						{
							ticketInputCellVM.TempCell = new Auditai.Model.Cell
							{
								Column = ticketInputCellVM.Column,
								Row = DummyMR,
								Value = ""
							};
							ticketInputCellVM.Value = "";
						}
						else
						{
							Auditai.Model.Cell cell4 = (ticketInputCellVM.TableCell = (ticketInputCellVM.TempCell = Table[record.Rows[0].Index, ticketInputCellVM.Column.Index]));
							if (cell4 == null) continue;
							ticketInputCellVM.Value = cell4.Value;
							if (Table.CellPropManager.TryGetAttachments(ticketInputCellVM.TableCell, out var attachments))
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
					ticketInputCellVM.TempCell = new Auditai.Model.Cell
					{
						Column = ticketInputCellVM.Column,
						Row = DummyMR,
						Value = ""
					};
				}
				_cells.Add(ticketInputCellVM);
			}
		}
		int i;
		if (_ticket.Kind == TicketKind.FixedMultiRow)
		{
			HashSet<Auditai.Model.Column> _keyColumns = new HashSet<Auditai.Model.Column>(from c in ticket.Cells
				where c.HasText() && c.HasField()
				select ticket.GetFieldColumn(c.Field) into c
				where c != null
				select c);
			HashSet<Auditai.Model.Column> source = new HashSet<Auditai.Model.Column>(from c in ticket.Cells
				where c.HasField()
				group c by c.Field into g
				where g.Count() > 1
				select ticket.GetFieldColumn(g.Key) into c
				where c != null && !_keyColumns.Contains(c)
				select c);
			FixedCells.AddRange(from c in _cells
				where c.Column != null
				group c by c.Column into g
				where g.Count() == 1
				select g.Single());
			keyCellsList = _keyColumns.Select((Auditai.Model.Column kc) => new
			{
				Column = kc,
				Cells = ticketInputTableVM._cells.Where((TicketInputCellVM c) => c.Column == kc).ToList()
			}).ToList();
			int num = ((keyCellsList != null && keyCellsList.Count != 0) ? keyCellsList[0].Cells.Count : 0);
			var list = source.Select((Auditai.Model.Column vc) => new
			{
				Column = vc,
				Cells = ticketInputTableVM._cells.Where((TicketInputCellVM c) => c.Column == vc).ToList()
			}).ToList();
			Title.GetAllContainsFieldCell(FixedCells);
			Footer.GetAllContainsFieldCell(FixedCells);
			foreach (TicketInputCellVM fixedCell in FixedCells)
			{
				fixedCell.IsFixedMultiFixedCell = true;
			}
			for (i = 0; i < num; i++)
			{
				TicketFixedMultiRowVM ticketFixedMultiRowVM = new TicketFixedMultiRowVM();
				FixedMultiRowVMs.Add(ticketFixedMultiRowVM);
				Auditai.Model.Row row3 = (ticketFixedMultiRowVM.Row = record?.Rows.FirstOrDefault((Auditai.Model.Row r) => RowMatches(r)));
				foreach (var item4 in keyCellsList)
				{
					TicketInputCellVM ticketInputCellVM2 = item4.Cells[i];
					ticketInputCellVM2.IsFixedMultiRowKey = true;
					ticketInputCellVM2.Value = ticketInputCellVM2.TicketCell.GetInputValue();
					ticketInputCellVM2.FixedMultiRowKeyCellDisplayValue = ((ticketInputCellVM2.TicketCell.Text == null) ? string.Empty : ticketInputCellVM2.TicketCell.Text);
					ticketFixedMultiRowVM.KeyCells.Add(ticketInputCellVM2);
					if (row3 == null)
					{
						ticketInputCellVM2.TempCell = new Auditai.Model.Cell
						{
							Column = item4.Column,
							Row = DummyMR,
							Value = ticketInputCellVM2.TicketCell.GetInputValue()
						};
						ticketInputCellVM2.TableCell = null;
					}
					else
					{
						Auditai.Model.Cell tableCell = (ticketInputCellVM2.TempCell = Table[row3.Index, item4.Column.Index]);
						if (tableCell == null) continue;
						ticketInputCellVM2.TableCell = tableCell;
					}
				}
				foreach (var item5 in list)
				{
					TicketInputCellVM ticketInputCellVM3 = item5.Cells[i];
					ticketInputCellVM3.IsFixedMultiRowValue = true;
					ticketFixedMultiRowVM.ValueCells.Add(ticketInputCellVM3);
					if (row3 == null)
					{
						ticketInputCellVM3.TempCell = new Auditai.Model.Cell
						{
							Column = item5.Column,
							Row = DummyMR,
							Value = ""
						};
						ticketInputCellVM3.Value = "";
						ticketInputCellVM3.TableCell = null;
					}
					else
					{
						Auditai.Model.Cell cell6 = Table[row3.Index, item5.Column.Index];
						if (cell6 == null) continue;
						ticketInputCellVM3.Value = cell6.Value;
						ticketInputCellVM3.TempCell = cell6;
						ticketInputCellVM3.TableCell = cell6;
					}
					if (ticketInputCellVM3.TicketCell.HasFormula())
					{
						ticketInputCellVM3.IsFormula = true;
						ticketInputCellVM3.Formula = ticketInputCellVM3.TicketCell.Formula;
						ticketInputCellVM3.IsFormulaFromTicket = true;
					}
				}
			}
			if (ticket.Table.RowOwnerExclusive && _record != null && FixedMultiRowVMs.Where((TicketFixedMultiRowVM r) => r.Row != null).All((TicketFixedMultiRowVM r) => r.Row.Creator != Auditai.Model.User.Current.Id))
			{
				DummyMR.Creator = 0L;
			}
		}
		else if (ticket.Kind == TicketKind.DynamicRow)
		{
			DynamicRowKeyCells.AddRange(_cells.Where((TicketInputCellVM u) => u.Column != null));
			Title.GetAllContainsFieldCell(DynamicRowKeyCells);
			Footer.GetAllContainsFieldCell(DynamicRowKeyCells);
			foreach (TicketInputCellVM dynamicRowKeyCell in DynamicRowKeyCells)
			{
				dynamicRowKeyCell.IsDynamicRowKeyCell = true;
			}
			List<TicketInputRowVM> list2 = new List<TicketInputRowVM>();
			List<TicketInputCellVM> list3 = new List<TicketInputCellVM>();
			if (record == null)
			{
				DataRowsCount = ticket.DataRowCount;
				if (_hasFillingFormula)
				{
					DataRowsCount = 1;
				}
			}
			else
			{
				DataRowsCount = Math.Max(record.Rows.Count, ticket.DataRowCount);
				if (_hasFillingFormula)
				{
					DataRowsCount = record.Rows.Count;
				}
				for (int m = 0; m < record.Rows.Count; m++)
				{
					Auditai.Model.Row row4 = record.Rows[m];
					TicketInputRowVM item2 = new TicketInputRowVM
					{
						IsNew = false,
						TempRow = row4,
						TableRow = row4,
						IsDynamicRowTicketDataRow = true
					};
					DynamicRowRefTableDataRows.Add(row4);
					list2.Add(item2);
					for (int n = 0; n < ticket.Columns.Count; n++)
					{
						TicketColumn ticketColumn2 = ticket.Columns[n];
						TicketInputCellVM ticketInputCellVM4 = new TicketInputCellVM
						{
							Value = "",
							IsDynamicTicketDataRow = true,
							TicketColumn = ticketColumn2
						};
						ticketInputCellVM4.Value = "";
						ticketInputCellVM4.IsDynamicRowDataCell = true;
						if (ticketColumn2.HasField())
						{
							ticketInputCellVM4.IsField = true;
							ticketInputCellVM4.Column = _ticket.GetFieldColumn(ticketColumn2.Field);
							if (ticketInputCellVM4.Column != null)
							{
								Auditai.Model.Cell cell9 = (ticketInputCellVM4.TableCell = (ticketInputCellVM4.TempCell = Table[row4.Index, ticketInputCellVM4.Column.Index]));
								if (cell9 == null) continue;
								ticketInputCellVM4.Value = cell9.Value;
								if (ticketInputCellVM4.Column.HasFormula)
								{
									ticketInputCellVM4.IsFormula = true;
									ticketInputCellVM4.Formula = ticketInputCellVM4.Column.Formula;
									ticketInputCellVM4.IsFormulaFromTicket = false;
								}
								if (Table.CellPropManager.TryGetAttachments(cell9, out var attachments2))
								{
									ticketInputCellVM4.Attachments = attachments2;
								}
							}
						}
						if (ticketColumn2.HasFormula())
						{
							ticketInputCellVM4.IsFormula = true;
							ticketInputCellVM4.Formula = ticketColumn2.Formula;
							ticketInputCellVM4.IsFormulaFromTicket = true;
						}
						if (ticketInputCellVM4.TempCell == null)
						{
							ticketInputCellVM4.TempCell = new Auditai.Model.Cell
							{
								Column = ticketInputCellVM4.Column,
								Row = DummyMR,
								Value = ""
							};
						}
						list3.Add(ticketInputCellVM4);
					}
				}
			}
			for (int num2 = 0; num2 < ((record == null) ? DataRowsCount : (DataRowsCount - record.Rows.Count)); num2++)
			{
				TicketInputRowVM item3 = new TicketInputRowVM
				{
					IsNew = true,
					TempRow = DummyMR,
					IsDynamicRowTicketDataRow = true
				};
				list2.Add(item3);
				for (int num3 = 0; num3 < ticket.Columns.Count; num3++)
				{
					TicketColumn ticketColumn3 = ticket.Columns[num3];
					TicketInputCellVM ticketInputCellVM5 = new TicketInputCellVM
					{
						Value = "",
						IsDynamicTicketDataRow = true,
						TicketColumn = ticketColumn3
					};
					ticketInputCellVM5.IsDynamicRowDataCell = true;
					if (ticketColumn3.HasField())
					{
						ticketInputCellVM5.IsField = true;
						ticketInputCellVM5.Column = _ticket.GetFieldColumn(ticketColumn3.Field);
						if (ticketInputCellVM5.Column != null)
						{
							ticketInputCellVM5.TempCell = new Auditai.Model.Cell
							{
								Column = ticketInputCellVM5.Column,
								Row = DummyMR,
								Value = ""
							};
							if (ticketInputCellVM5.Column.HasFormula)
							{
								ticketInputCellVM5.IsFormula = true;
								ticketInputCellVM5.Formula = ticketInputCellVM5.Column.Formula;
								ticketInputCellVM5.IsFormulaFromTicket = false;
							}
						}
					}
					if (ticketColumn3.HasFormula())
					{
						ticketInputCellVM5.IsFormula = true;
						ticketInputCellVM5.Formula = ticketColumn3.Formula;
						ticketInputCellVM5.IsFormulaFromTicket = true;
					}
					if (ticketInputCellVM5.TempCell == null)
					{
						ticketInputCellVM5.TempCell = new Auditai.Model.Cell
						{
							Column = ticketInputCellVM5.Column,
							Row = DummyMR,
							Value = ""
						};
					}
					list3.Add(ticketInputCellVM5);
				}
			}
			_rows.InsertRange(ticket.DataRowStart, list2);
			_cells.InsertRange(ticket.DataRowStart * ticket.Columns.Count, list3);
		}
		else
		{
			_ = ticket.Kind;
			_ = 1;
		}
		PopulateMerges();
		if (isCalculateTicket)
		{
			CalculateTicket();
		}
		bool RowMatches(Auditai.Model.Row row)
		{
			foreach (var item6 in keyCellsList)
			{
				if (!ticketInputTableVM.Table[row.Index, item6.Column.Index].Value.Equals(item6.Cells[i].TicketCell.GetInputValue()))
				{
					return false;
				}
			}
			return true;
		}
	}

	private void FindOutTicketCellWhichNeedWriteFormulaResultToTable_MixTicket()
	{
		if (_ticket.Kind != TicketKind.FixedDataRowMixDynamicDataRow)
		{
			return;
		}
		_ticketCellWhichNeedWriteFormulaResultToTableInMixTicket = null;
		int count = _rows.Count;
		int count2 = _columns.Count;
		for (int i = 0; i < count; i++)
		{
			if (!_rows[i].IsMixTicketFixedDataRow)
			{
				continue;
			}
			for (int j = 0; j < count2; j++)
			{
				TicketInputCellVM cellVM = GetCellVM(i, j);
				if (cellVM.IsField && cellVM.IsFormula && cellVM.IsFormulaFromTicket)
				{
					if (_ticketCellWhichNeedWriteFormulaResultToTableInMixTicket == null)
					{
						_ticketCellWhichNeedWriteFormulaResultToTableInMixTicket = new List<TicketInputCellVM>();
					}
					_ticketCellWhichNeedWriteFormulaResultToTableInMixTicket.Add(cellVM);
					_rows[i].IsMixTicketExistWriteTicketFormulaResultToTableRow = true;
				}
			}
		}
	}

	private void BuildTableCellTicektCellForWhichNeedWriteFormulaResultToTable_MixTicket()
	{
		if (_ticketCellWhichNeedWriteFormulaResultToTableInMixTicket == null || _ticketCellWhichNeedWriteFormulaResultToTableInMixTicket.Count == 0)
		{
			return;
		}
		HashSet<TicketInputCellVM> hashSet = new HashSet<TicketInputCellVM>(_ticketCellWhichNeedWriteFormulaResultToTableInMixTicket);
		Dictionary<TicketInputCellVM, Tuple<int, int>> dictionary = new Dictionary<TicketInputCellVM, Tuple<int, int>>();
		int count = _rows.Count;
		int count2 = _columns.Count;
		for (int i = 0; i < count; i++)
		{
			if (!_rows[i].IsMixTicketFixedDataRow)
			{
				continue;
			}
			for (int j = 0; j < count2; j++)
			{
				TicketInputCellVM cellVM = GetCellVM(i, j);
				if (hashSet.Contains(cellVM))
				{
					dictionary[cellVM] = Tuple.Create(i, j);
				}
			}
		}
		BeginBatchUpdateValue();
		foreach (TicketInputCellVM item in _ticketCellWhichNeedWriteFormulaResultToTableInMixTicket)
		{
			if (dictionary.TryGetValue(item, out var value))
			{
				BuildTableCellForTicketCell_MixTicket(value.Item1, value.Item2);
			}
		}
		EndBatchUpdateValue();
	}

	private void InitFixedDataRowMixDynamicDataRowTicket(bool isCalculateTicket)
	{
		GroupRecordDataForMixRange();
		for (int i = 0; i < _ticket.Columns.Count; i++)
		{
			TicketColumn ticketColumn = _ticket.Columns[i];
			TicketInputColumnVM ticketInputColumnVM = new TicketInputColumnVM
			{
				TicketColumn = ticketColumn,
				TableColumn = _ticket.GetFieldColumn(ticketColumn.Field),
				IsHiddenColumn = ticketColumn.IsHiddenColumn
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
			_columns.Add(ticketInputColumnVM);
		}
		MixRangeTemplateRowPreFixedRowDic = new Dictionary<int, TicketInputRowVM>();
		if (_record != null && _record.Rows != null)
		{
			foreach (Auditai.Model.Row row3 in _record.Rows)
			{
				MixTicketDataRowsRefTableRows.Add(row3);
			}
		}
		for (int j = 0; j < _ticket.Rows.Count; j++)
		{
			TicketRow ticketRow = _ticket.Rows[j];
			if (ticketRow.IsMixRangeDynamicDataRow)
			{
				if (ticketRow.IsMixRangeTemplateRow)
				{
					if (_rows.Count == 0)
					{
						MixRangeTemplateRowPreFixedRowDic.Add(ticketRow.MixRangeDynamicDataRowTemplateId, null);
					}
					else
					{
						MixRangeTemplateRowPreFixedRowDic.Add(ticketRow.MixRangeDynamicDataRowTemplateId, _rows[_rows.Count - 1]);
					}
				}
				continue;
			}
			TicketInputRowVM ticketInputRowVM = new TicketInputRowVM
			{
				TicketRow = ticketRow
			};
			_rows.Add(ticketInputRowVM);
			Auditai.Model.Row row = null;
			if (ticketRow.IsMixRangeFixedDataRow)
			{
				if (MixRangeDataRowGroupDic.TryGetValue(ticketRow.MixRangeDataKeyId, out var value) && value.Count > 0)
				{
					row = value[0];
				}
				ticketInputRowVM.IsMixTicketFixedDataRow = true;
				ticketInputRowVM.TableRow = row;
			}
			else if (_record != null)
			{
				row = _record.Rows[0];
			}
			for (int k = 0; k < _ticket.Columns.Count; k++)
			{
				TicketCell cell = _ticket.GetCell(j, k);
				TicketInputCellVM ticketInputCellVM = new TicketInputCellVM
				{
					Value = cell.Text,
					IsDynamicTicketDataRow = false,
					TicketCell = cell,
					IsMixTicketFixedDataRow = ticketInputRowVM.IsMixTicketFixedDataRow
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
						if (row == null)
						{
							ticketInputCellVM.TempCell = new Auditai.Model.Cell
							{
								Column = ticketInputCellVM.Column,
								Row = DummyMR,
								Value = ""
							};
							ticketInputCellVM.Value = "";
						}
						else
						{
							Auditai.Model.Cell cell4 = (ticketInputCellVM.TableCell = (ticketInputCellVM.TempCell = Table[row.Index, ticketInputCellVM.Column.Index]));
							if (cell4 == null) continue;
							ticketInputCellVM.Value = cell4.Value;
							if (Table.CellPropManager.TryGetAttachments(ticketInputCellVM.TableCell, out var attachments))
							{
								ticketInputCellVM.Attachments = attachments;
							}
						}
						if (ticketInputRowVM.IsMixTicketFixedDataRow && (!string.IsNullOrEmpty(cell.Text) || !string.IsNullOrEmpty(cell.InputValue)))
						{
							ticketInputCellVM.IsMixTicketExistDesignInputValue = true;
							ticketInputCellVM.MixTicketExistDesignInputValueShouldDisplayValue = (string.IsNullOrEmpty(cell.Text) ? cell.InputValue : cell.Text);
							ticketInputCellVM.Value = ticketInputCellVM.MixTicketExistDesignInputValueShouldDisplayValue;
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
					ticketInputCellVM.TempCell = new Auditai.Model.Cell
					{
						Column = ticketInputCellVM.Column,
						Row = DummyMR,
						Value = ""
					};
				}
				_cells.Add(ticketInputCellVM);
			}
		}
		FindOutTicketCellWhichNeedWriteFormulaResultToTable_MixTicket();
		MixTicketKeyCells.AddRange(_cells.Where((TicketInputCellVM u) => u.TicketCell.IsMixRangeTicketKey));
		Title.GetAllContainsFieldCell(MixTicketKeyCells);
		Footer.GetAllContainsFieldCell(MixTicketKeyCells);
		foreach (TicketInputCellVM mixTicketKeyCell in MixTicketKeyCells)
		{
			mixTicketKeyCell.IsMixTicketKeyCell = true;
		}
		MixTicketExistMergeTemplateRows = null;
		MixTicketColumnIdMapperToTemplateRowIdDic = new Dictionary<long, int>();
		foreach (TicketTableMixRangeTemplateRow dynamicDataRowTemplateRow in _ticket.FixedAndDynamicMixRange.DynamicDataRowTemplateRows)
		{
			if (dynamicDataRowTemplateRow.Merges != null && dynamicDataRowTemplateRow.Merges.Count > 0)
			{
				if (MixTicketExistMergeTemplateRows == null)
				{
					MixTicketExistMergeTemplateRows = new Dictionary<int, TicketTableMixRangeTemplateRow>();
				}
				MixTicketExistMergeTemplateRows.Add(dynamicDataRowTemplateRow.TemplateId, dynamicDataRowTemplateRow);
			}
			foreach (long ticketColumnId in dynamicDataRowTemplateRow.TicketColumnIdList)
			{
				MixTicketColumnIdMapperToTemplateRowIdDic[ticketColumnId] = dynamicDataRowTemplateRow.TemplateId;
			}
		}
		foreach (TicketTableMixRangeTemplateRow dynamicDataRowTemplateRow2 in _ticket.FixedAndDynamicMixRange.DynamicDataRowTemplateRows)
		{
			if (!MixRangeDataRowGroupDic.TryGetValue(dynamicDataRowTemplateRow2.DataGroupKeyId, out var value2))
			{
				value2 = new List<Auditai.Model.Row>();
			}
			int num = dynamicDataRowTemplateRow2.GetTicketTableRowsCount();
			List<TicketInputRowVM> list = new List<TicketInputRowVM>();
			List<TicketInputCellVM> list2 = new List<TicketInputCellVM>();
			for (int l = 0; l < value2.Count; l++)
			{
				Auditai.Model.Row row2 = value2[l];
				TicketRow ticketRow2 = _ticket.Rows[dynamicDataRowTemplateRow2.RefTicketTableRowIndex];
				TicketInputRowVM item = new TicketInputRowVM
				{
					TicketRow = ticketRow2,
					IsMixTicketDynamicDataRow = true,
					TableRow = row2,
					TempRow = row2,
					MixTicketDynamicDataRowTemplate = dynamicDataRowTemplateRow2
				};
				list.Add(item);
				for (int m = 0; m < _ticket.Columns.Count; m++)
				{
					TicketCell cell5 = _ticket.GetCell(dynamicDataRowTemplateRow2.RefTicketTableRowIndex, m);
					TicketInputCellVM ticketInputCellVM2 = new TicketInputCellVM
					{
						Value = cell5.GetInputValue(),
						IsDynamicTicketDataRow = false,
						TicketCell = cell5,
						IsMixTicketDynamicDataRow = true
					};
					if (cell5.HasField())
					{
						ticketInputCellVM2.IsField = true;
						ticketInputCellVM2.Column = _ticket.GetFieldColumn(cell5.Field);
						if (ticketInputCellVM2.Column != null)
						{
							if (ticketInputCellVM2.Column.HasFormula)
							{
								ticketInputCellVM2.IsFormula = true;
								ticketInputCellVM2.Formula = ticketInputCellVM2.Column.Formula;
								ticketInputCellVM2.IsFormulaFromTicket = false;
							}
							Auditai.Model.Cell cell8 = (ticketInputCellVM2.TableCell = (ticketInputCellVM2.TempCell = Table[row2.Index, ticketInputCellVM2.Column.Index]));
							if (cell8 == null) continue;
							ticketInputCellVM2.Value = cell8.Value;
							if (Table.CellPropManager.TryGetAttachments(ticketInputCellVM2.TableCell, out var attachments2))
							{
								ticketInputCellVM2.Attachments = attachments2;
							}
							if (!string.IsNullOrEmpty(cell5.InputValue))
							{
								ticketInputCellVM2.IsMixTicketExistDesignInputValue = true;
								ticketInputCellVM2.MixTicketExistDesignInputValueShouldDisplayValue = cell5.GetInputValue();
								ticketInputCellVM2.Value = ticketInputCellVM2.MixTicketExistDesignInputValueShouldDisplayValue;
							}
						}
					}
					if (cell5.HasFormula())
					{
						ticketInputCellVM2.IsFormula = true;
						ticketInputCellVM2.Formula = cell5.Formula;
						ticketInputCellVM2.IsFormulaFromTicket = true;
					}
					if (ticketInputCellVM2.TempCell == null)
					{
						ticketInputCellVM2.TempCell = new Auditai.Model.Cell
						{
							Column = ticketInputCellVM2.Column,
							Row = DummyMR,
							Value = ""
						};
					}
					list2.Add(ticketInputCellVM2);
				}
			}
			if (_hasFillingFormula)
			{
				num = ((value2.Count == 0) ? 1 : value2.Count);
			}
			for (int n = value2.Count; n < num; n++)
			{
				TicketRow ticketRow3 = _ticket.Rows[dynamicDataRowTemplateRow2.RefTicketTableRowIndex];
				TicketInputRowVM item2 = new TicketInputRowVM
				{
					IsNew = true,
					TicketRow = ticketRow3,
					IsMixTicketDynamicDataRow = true,
					MixTicketDynamicDataRowTemplate = dynamicDataRowTemplateRow2
				};
				list.Add(item2);
				for (int num2 = 0; num2 < _ticket.Columns.Count; num2++)
				{
					TicketCell cell9 = _ticket.GetCell(dynamicDataRowTemplateRow2.RefTicketTableRowIndex, num2);
					TicketInputCellVM ticketInputCellVM3 = new TicketInputCellVM
					{
						Value = cell9.GetInputValue(),
						IsDynamicTicketDataRow = false,
						TicketCell = cell9,
						IsMixTicketDynamicDataRow = true
					};
					if (cell9.HasField())
					{
						ticketInputCellVM3.IsField = true;
						ticketInputCellVM3.Column = _ticket.GetFieldColumn(cell9.Field);
						if (ticketInputCellVM3.Column != null)
						{
							if (ticketInputCellVM3.Column.HasFormula)
							{
								ticketInputCellVM3.IsFormula = true;
								ticketInputCellVM3.Formula = ticketInputCellVM3.Column.Formula;
								ticketInputCellVM3.IsFormulaFromTicket = false;
							}
							ticketInputCellVM3.TempCell = new Auditai.Model.Cell
							{
								Column = ticketInputCellVM3.Column,
								Row = DummyMR,
								Value = ""
							};
							ticketInputCellVM3.Value = "";
							if (!string.IsNullOrEmpty(cell9.InputValue))
							{
								ticketInputCellVM3.IsMixTicketExistDesignInputValue = true;
								ticketInputCellVM3.MixTicketExistDesignInputValueShouldDisplayValue = cell9.GetInputValue();
								ticketInputCellVM3.Value = ticketInputCellVM3.MixTicketExistDesignInputValueShouldDisplayValue;
							}
						}
					}
					if (cell9.HasFormula())
					{
						ticketInputCellVM3.IsFormula = true;
						ticketInputCellVM3.Formula = cell9.Formula;
						ticketInputCellVM3.IsFormulaFromTicket = true;
					}
					if (ticketInputCellVM3.TempCell == null)
					{
						ticketInputCellVM3.TempCell = new Auditai.Model.Cell
						{
							Column = ticketInputCellVM3.Column,
							Row = DummyMR,
							Value = ""
						};
					}
					list2.Add(ticketInputCellVM3);
				}
			}
			if (list.Count > 0)
			{
				int mixRangeUseTemplateDataRowInsertStartIndex = GetMixRangeUseTemplateDataRowInsertStartIndex(dynamicDataRowTemplateRow2.TemplateId);
				if (mixRangeUseTemplateDataRowInsertStartIndex >= 0)
				{
					_rows.InsertRange(mixRangeUseTemplateDataRowInsertStartIndex, list);
					_cells.InsertRange(mixRangeUseTemplateDataRowInsertStartIndex * _columns.Count, list2);
				}
			}
		}
		PopulateMerges();
		if (isCalculateTicket)
		{
			CalculateTicket();
		}
	}

	public void StartRecordNewAddTableRows()
	{
		_isNeedRecordNewAddedTableRow = true;
	}

	private void RecordTableRowsToNewAddRow(int startIndex, int count)
	{
		if (!_isNeedRecordNewAddedTableRow)
		{
			return;
		}
		if (_newAddedTableRowsList == null)
		{
			_newAddedTableRowsList = new List<Auditai.Model.Row>();
		}
		if (Table == null)
		{
			return;
		}
		int count2 = Table.Rows.Count;
		for (int i = 0; i < count; i++)
		{
			int num = startIndex + i;
			if (num >= 0 && num < count2)
			{
				_newAddedTableRowsList.Add(Table.Rows[num]);
			}
		}
	}

	private void RecordTableRowsToNewAddRow(Auditai.Model.Row row)
	{
		if (_isNeedRecordNewAddedTableRow)
		{
			if (_newAddedTableRowsList == null)
			{
				_newAddedTableRowsList = new List<Auditai.Model.Row>();
			}
			if (Table != null && row != null)
			{
				_newAddedTableRowsList.Add(row);
			}
		}
	}

	public List<CellAttachment> GetAllAttachments()
	{
		List<CellAttachment> list = new List<CellAttachment>();
		Title.GetAllAttachments(list);
		Footer.GetAllAttachments(list);
		foreach (TicketInputCellVM cell in _cells)
		{
			if (cell.TableCell != null && Table.CellPropManager.TryGetAttachments(cell.TableCell, out var attachments))
			{
				list.AddRange(attachments.Attachments);
			}
		}
		return list;
	}

	protected int GetMixRangeUseTemplateDataRowInsertStartIndex(int templateRowId)
	{
		if (MixRangeTemplateRowPreFixedRowDic == null)
		{
			return -1;
		}
		if (!MixRangeTemplateRowPreFixedRowDic.TryGetValue(templateRowId, out var value))
		{
			return -1;
		}
		int num = _rows.IndexOf(value);
		if (num == -1)
		{
			return -1;
		}
		return num + 1;
	}

	protected int GetMixRangeDynamicDataRowIndex(int vmRowIndex)
	{
		TicketInputRowVM ticketInputRowVM = _rows[vmRowIndex];
		if (!ticketInputRowVM.IsMixTicketDynamicDataRow)
		{
			return -1;
		}
		int num = 0;
		int mixRangeDynamicDataRowTemplateId = ticketInputRowVM.TicketRow.MixRangeDynamicDataRowTemplateId;
		for (int num2 = vmRowIndex - 1; num2 >= 0; num2--)
		{
			TicketInputRowVM ticketInputRowVM2 = _rows[num2];
			if (!ticketInputRowVM2.TicketRow.IsMixRangeDynamicDataRow || ticketInputRowVM2.TicketRow.MixRangeDynamicDataRowTemplateId != mixRangeDynamicDataRowTemplateId)
			{
				break;
			}
			num++;
		}
		return num;
	}

	private void GroupRecordDataForMixRange()
	{
		MixRangeDataRowGroupDic = new Dictionary<int, List<Auditai.Model.Row>>();
		if (_ticket.FixedAndDynamicMixRange == null)
		{
			_ticket.FixedAndDynamicMixRange = new TicketTableFixedAndDynamicMixRange();
			_ticket.FixedAndDynamicMixRange.DataGroupKeyListForDynamicDataRow = new List<TicketTableMixRangeDataGroupKey>();
			_ticket.FixedAndDynamicMixRange.DataGroupKeyListForFixedDataRow = new List<TicketTableMixRangeDataGroupKey>();
			_ticket.FixedAndDynamicMixRange.DynamicDataRowTemplateRows = new List<TicketTableMixRangeTemplateRow>();
		}
		if (_record == null)
		{
			return;
		}
		List<Auditai.Model.Row> list = new List<Auditai.Model.Row>(_record.Rows);
		if (_ticket.FixedAndDynamicMixRange.DataGroupKeyListForFixedDataRow != null)
		{
			foreach (TicketTableMixRangeDataGroupKey item in _ticket.FixedAndDynamicMixRange.DataGroupKeyListForFixedDataRow)
			{
				List<Auditai.Model.Row> list2 = GetDataKeyMatchedRows(item, list, isOnlyGetFirstRow: true);
				if (list2.Count > 0)
				{
					MixRangeDataRowGroupDic[item.KeyId] = list2;
				}
			}
		}
		if (_ticket.FixedAndDynamicMixRange.DataGroupKeyListForDynamicDataRow == null)
		{
			return;
		}
		foreach (TicketTableMixRangeDataGroupKey item2 in _ticket.FixedAndDynamicMixRange.DataGroupKeyListForDynamicDataRow)
		{
			if (item2.KeyItems == null || item2.KeyItems.Count == 0)
			{
				MixRangeDataRowGroupDic[item2.KeyId] = new List<Auditai.Model.Row>(list.Where((Auditai.Model.Row u) => u != null));
				list = new List<Auditai.Model.Row>();
				break;
			}
			List<Auditai.Model.Row> list3 = GetDataKeyMatchedRows(item2, list, isOnlyGetFirstRow: false);
			if (list3.Count > 0)
			{
				MixRangeDataRowGroupDic[item2.KeyId] = list3;
			}
		}
		List<Auditai.Model.Row> GetDataKeyMatchedRows(TicketTableMixRangeDataGroupKey dataGroupKey, List<Auditai.Model.Row> rowsList, bool isOnlyGetFirstRow)
		{
			List<Tuple<Auditai.Model.Column, ValueOperand>> list4 = new List<Tuple<Auditai.Model.Column, ValueOperand>>();
			foreach (TicketTableMixRangeDataGroupKeyItem keyItem in dataGroupKey.KeyItems)
			{
				Auditai.Model.Column byId = _ticket.Table.Columns.GetById(keyItem.TableColumnId);
				if (byId == null)
				{
					return new List<Auditai.Model.Row>();
				}
				list4.Add(Tuple.Create(byId, Auditai.Model.Cell.ChangeToValueOperand(keyItem.TableColumnValue, byId.GetDataType())));
			}
			if (list4.Count == 0)
			{
				return new List<Auditai.Model.Row>();
			}
			List<Auditai.Model.Row> list5 = new List<Auditai.Model.Row>();
			for (int i = 0; i < rowsList.Count; i++)
			{
				Auditai.Model.Row row = rowsList[i];
				if (row != null)
				{
					bool flag = true;
					foreach (Tuple<Auditai.Model.Column, ValueOperand> item3 in list4)
					{
						ValueOperand valueOperand = ValueOperand.FromCellValue(Table[row.Index, item3.Item1.Index]);
						if (!valueOperand.Equal(item3.Item2).ToBool().Value)
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						list5.Add(row);
						rowsList[i] = null;
						if (isOnlyGetFirstRow)
						{
							return list5;
						}
					}
				}
			}
			return list5;
		}
	}

	public void InitCombolistForNewRecord(Dictionary<Id64, string> initValue = null)
	{
		List<Tuple<TicketInputCellVM, int, int, string>> list = new List<Tuple<TicketInputCellVM, int, int, string>>();
		List<Tuple<TicketInputCellVM, int, int, string>> list2 = new List<Tuple<TicketInputCellVM, int, int, string>>();
		if (initValue != null)
		{
			GetBodyHasCombolistCellInitValue(list, initValue);
			GetTitleOrFooterCombolistCellInitValue(Title, list2, initValue);
			GetTitleOrFooterCombolistCellInitValue(Footer, list2, initValue);
		}
		else
		{
			GetBodyOnlyHasOneCombolistValueCell(list);
			GetTitleOrFooterOnlyHasOneCombolistValueCell(Title, list2);
			GetTitleOrFooterOnlyHasOneCombolistValueCell(Footer, list2);
		}
		if (IsInShowingVirtualNode)
		{
			ShowFormulaConstValueInBodyCell();
			ShowFormulaConstValueInTitleOrFooterCell(Title);
			ShowFormulaConstValueInTitleOrFooterCell(Footer);
		}
		if (list.Count == 0 && list2.Count == 0)
		{
			return;
		}
		if (IsInShowingVirtualNode)
		{
			_inShowingVirtualValueBodyCellList = new List<Tuple<TicketInputCellVM, int, int, string>>();
			_inShowingVirtualValueTitleFooterCellList = new List<Tuple<TicketInputCellVM, int, int, string>>();
			foreach (Tuple<TicketInputCellVM, int, int, string> item in list)
			{
				item.Item1.IsShowVirtualValue = true;
				item.Item1.VirtualValue = item.Item4;
				_inShowingVirtualValueBodyCellList.Add(item);
			}
			{
				foreach (Tuple<TicketInputCellVM, int, int, string> item2 in list2)
				{
					item2.Item1.IsShowVirtualValue = true;
					item2.Item1.VirtualValue = item2.Item4;
					_inShowingVirtualValueTitleFooterCellList.Add(item2);
				}
				return;
			}
		}
		BeginBatchUpdateValue();
		foreach (Tuple<TicketInputCellVM, int, int, string> item3 in list)
		{
			BuildTableCellForTicketCell(item3.Item2, item3.Item3);
		}
		foreach (Tuple<TicketInputCellVM, int, int, string> item4 in list2)
		{
			BuildTableCellForTicketTitleFooterCell(item4.Item1);
		}
		foreach (Tuple<TicketInputCellVM, int, int, string> item5 in list)
		{
			UpdateTicketCellValue(newValue: (item5.Item1.TableCell == null) ? item5.Item4 : Auditai.Model.Cell.ChangeDataTypeImpl(item5.Item4, item5.Item1.TableCell.DisplayDataType), ticketCell: item5.Item1, isFormulaExistManualInputValue: true);
		}
		foreach (Tuple<TicketInputCellVM, int, int, string> item6 in list2)
		{
			UpdateTicketCellValue(newValue: (item6.Item1.TableCell == null) ? item6.Item4 : Auditai.Model.Cell.ChangeDataTypeImpl(item6.Item4, item6.Item1.TableCell.DisplayDataType), ticketCell: item6.Item1, isFormulaExistManualInputValue: true);
		}
		EndBatchUpdateValue();
		void GetBodyHasCombolistCellInitValue(List<Tuple<TicketInputCellVM, int, int, string>> outList, Dictionary<Id64, string> initValue)
		{
			for (int num5 = 0; num5 < _rows.Count; num5++)
			{
				if (!_rows[num5].IsDynamicRowTicketDataRow)
				{
					for (int num6 = 0; num6 < _columns.Count; num6++)
					{
						TicketInputCellVM cellVM6 = GetCellVM(num5, num6);
						if (cellVM6.Column != null && CanEditColumn(cellVM6.Column) && cellVM6.Column.GetFormat().HasComboList && initValue.TryGetValue(cellVM6.Column.Id, out var value4))
						{
							outList.Add(Tuple.Create(cellVM6, num5, num6, value4));
						}
					}
				}
			}
		}
		void GetBodyOnlyHasOneCombolistValueCell(List<Tuple<TicketInputCellVM, int, int, string>> outList)
		{
			for (int num = 0; num < _rows.Count; num++)
			{
				if (!_rows[num].IsDynamicRowTicketDataRow)
				{
					for (int num2 = 0; num2 < _columns.Count; num2++)
					{
						TicketInputCellVM cellVM4 = GetCellVM(num, num2);
						if (cellVM4.Column != null && CanEditColumn(cellVM4.Column))
						{
							DataFormat format2 = cellVM4.Column.GetFormat();
							if (format2.HasComboList)
							{
								Operand comboList = GetComboList(num, num2, format2.ComboList);
								if (IsCombolistOnlyOneValue(comboList, out var theValue2))
								{
									outList.Add(Tuple.Create(cellVM4, num, num2, theValue2));
								}
							}
						}
					}
				}
			}
		}
		void GetTitleOrFooterCombolistCellInitValue(TicketInputTitleFooterVM target, List<Tuple<TicketInputCellVM, int, int, string>> outList, Dictionary<Id64, string> initValue)
		{
			for (int num3 = 0; num3 < target.Rows.Count; num3++)
			{
				for (int num4 = 0; num4 < target.Columns.Count; num4++)
				{
					TicketInputCellVM cellVM5 = target.GetCellVM(num3, num4);
					if (cellVM5.Column != null && CanEditColumn(cellVM5.Column) && cellVM5.Column.GetFormat().HasComboList && initValue.TryGetValue(cellVM5.Column.Id, out var value3))
					{
						outList.Add(Tuple.Create(cellVM5, num3, num4, value3));
					}
				}
			}
		}
		void GetTitleOrFooterOnlyHasOneCombolistValueCell(TicketInputTitleFooterVM target, List<Tuple<TicketInputCellVM, int, int, string>> outList)
		{
			for (int m = 0; m < target.Rows.Count; m++)
			{
				for (int n = 0; n < target.Columns.Count; n++)
				{
					TicketInputCellVM cellVM3 = target.GetCellVM(m, n);
					if (cellVM3.Column != null && CanEditColumn(cellVM3.Column))
					{
						DataFormat format = cellVM3.Column.GetFormat();
						if (format.HasComboList)
						{
							Operand cellValueList = GetCellValueList(target, m, n, format.ComboList);
							if (IsCombolistOnlyOneValue(cellValueList, out var theValue))
							{
								outList.Add(Tuple.Create(cellVM3, m, n, theValue));
							}
						}
					}
				}
			}
		}
		void ShowFormulaConstValueInBodyCell()
		{
			FormulaReferenceModelResolver resolver2 = new FormulaReferenceModelResolver(Table.Project);
			FormulaEvaluationEnvironment env2 = new FormulaEvaluationEnvironment
			{
				Resolver = resolver2,
				RefManager = Table.Project.DataReferenceManager,
				RefEvalContext = new DataReferenceEvaluationContext
				{
					Project = Table.Project,
					CurrentTreeNode = Table.TreeNode
				},
				RowIndex = -1,
				HostTable = Table
			};
			for (int k = 0; k < _rows.Count; k++)
			{
				if (!_rows[k].IsDynamicRowTicketDataRow && !_rows[k].IsMixTicketDynamicDataRow && !_rows[k].IsMixTicketFixedDataRow)
				{
					for (int l = 0; l < _columns.Count; l++)
					{
						TicketInputCellVM cellVM2 = GetCellVM(k, l);
						if (cellVM2.Column != null && cellVM2.IsFormula && !cellVM2.IsFormulaFromTicket && !(cellVM2.Formula != cellVM2.Column.Formula) && !cellVM2.Column.IsFormulaRefSelfTableColumnOrCell)
						{
							try
							{
								FormulaEvaluator formulaEvaluator2 = new FormulaEvaluator(cellVM2.Formula)
								{
									Env = env2
								};
								object value2 = formulaEvaluator2.Evaluate();
								object obj3 = Auditai.Model.Cell.ChangeDataTypeImpl(value2, cellVM2.Column.GetDataType());
								string displayValueImpl2 = Auditai.Model.Cell.GetDisplayValueImpl(value2, cellVM2.Column.GetFormat());
								if (!string.IsNullOrWhiteSpace(displayValueImpl2))
								{
									cellVM2.IsShowVirtualValue = true;
									cellVM2.VirtualValue = displayValueImpl2;
									if (_inShowingVirtualFormulaConstResultBodyCellList == null)
									{
										_inShowingVirtualFormulaConstResultBodyCellList = new List<Tuple<TicketInputCellVM, int, int, string>>();
									}
									_inShowingVirtualFormulaConstResultBodyCellList.Add(Tuple.Create(cellVM2, k, l, displayValueImpl2));
								}
							}
							catch
							{
							}
						}
					}
				}
			}
		}
		void ShowFormulaConstValueInTitleOrFooterCell(TicketInputTitleFooterVM target)
		{
			if (target.Rows.Count != 0)
			{
				FormulaReferenceModelResolver resolver = new FormulaReferenceModelResolver(Table.Project);
				FormulaEvaluationEnvironment env = new FormulaEvaluationEnvironment
				{
					Resolver = resolver,
					RefManager = Table.Project.DataReferenceManager,
					RefEvalContext = new DataReferenceEvaluationContext
					{
						Project = Table.Project,
						CurrentTreeNode = Table.TreeNode
					},
					RowIndex = -1,
					HostTable = Table
				};
				for (int i = 0; i < target.Rows.Count; i++)
				{
					for (int j = 0; j < target.Columns.Count; j++)
					{
						TicketInputCellVM cellVM = target.GetCellVM(i, j);
						if (cellVM.Column != null && cellVM.IsFormula && !cellVM.IsFormulaFromTicket && !(cellVM.Formula != cellVM.Column.Formula) && !cellVM.Column.IsFormulaRefSelfTableColumnOrCell)
						{
							try
							{
								FormulaEvaluator formulaEvaluator = new FormulaEvaluator(cellVM.Formula)
								{
									Env = env
								};
								object value = formulaEvaluator.Evaluate();
								object obj = Auditai.Model.Cell.ChangeDataTypeImpl(value, cellVM.Column.GetDataType());
								string displayValueImpl = Auditai.Model.Cell.GetDisplayValueImpl(value, cellVM.Column.GetFormat());
								if (!string.IsNullOrWhiteSpace(displayValueImpl))
								{
									cellVM.IsShowVirtualValue = true;
									cellVM.VirtualValue = displayValueImpl;
									if (_inShowingVirtualFormulaConstResultTitleFooterCellList == null)
									{
										_inShowingVirtualFormulaConstResultTitleFooterCellList = new List<Tuple<TicketInputCellVM, int, int, string>>();
									}
									_inShowingVirtualFormulaConstResultTitleFooterCellList.Add(Tuple.Create(cellVM, i, j, displayValueImpl));
								}
							}
							catch
							{
							}
						}
					}
				}
			}
		}
	}

	public void ChangeVirtualValueToRealValue()
	{
		if (!IsInShowingVirtualNode)
		{
			return;
		}
		IsInShowingVirtualNode = false;
		if (_inShowingVirtualValueBodyCellList == null)
		{
			_inShowingVirtualValueBodyCellList = new List<Tuple<TicketInputCellVM, int, int, string>>();
		}
		if (_inShowingVirtualValueTitleFooterCellList == null)
		{
			_inShowingVirtualValueTitleFooterCellList = new List<Tuple<TicketInputCellVM, int, int, string>>();
		}
		FindOutTicketCellWhichNeedWriteFormulaResultToTable_MixTicket();
		StartRecordNewAddTableRows();
		BeginBatchUpdateValue();
		foreach (Tuple<TicketInputCellVM, int, int, string> inShowingVirtualValueBodyCell in _inShowingVirtualValueBodyCellList)
		{
			BuildTableCellForTicketCell(inShowingVirtualValueBodyCell.Item2, inShowingVirtualValueBodyCell.Item3);
			inShowingVirtualValueBodyCell.Item1.IsShowVirtualValue = false;
		}
		foreach (Tuple<TicketInputCellVM, int, int, string> inShowingVirtualValueTitleFooterCell in _inShowingVirtualValueTitleFooterCellList)
		{
			BuildTableCellForTicketTitleFooterCell(inShowingVirtualValueTitleFooterCell.Item1);
			inShowingVirtualValueTitleFooterCell.Item1.IsShowVirtualValue = false;
		}
		bool flag = false;
		if (_inShowingVirtualFormulaConstResultBodyCellList != null)
		{
			foreach (Tuple<TicketInputCellVM, int, int, string> inShowingVirtualFormulaConstResultBodyCell in _inShowingVirtualFormulaConstResultBodyCellList)
			{
				BuildTableCellForTicketCell(inShowingVirtualFormulaConstResultBodyCell.Item2, inShowingVirtualFormulaConstResultBodyCell.Item3);
				inShowingVirtualFormulaConstResultBodyCell.Item1.IsShowVirtualValue = false;
				flag = true;
			}
			_inShowingVirtualFormulaConstResultBodyCellList = null;
		}
		if (_inShowingVirtualFormulaConstResultTitleFooterCellList != null)
		{
			foreach (Tuple<TicketInputCellVM, int, int, string> inShowingVirtualFormulaConstResultTitleFooterCell in _inShowingVirtualFormulaConstResultTitleFooterCellList)
			{
				BuildTableCellForTicketTitleFooterCell(inShowingVirtualFormulaConstResultTitleFooterCell.Item1);
				inShowingVirtualFormulaConstResultTitleFooterCell.Item1.IsShowVirtualValue = false;
				flag = true;
			}
			_inShowingVirtualFormulaConstResultTitleFooterCellList = null;
		}
		BuildTableCellTicektCellForWhichNeedWriteFormulaResultToTable_MixTicket();
		foreach (Tuple<TicketInputCellVM, int, int, string> inShowingVirtualValueBodyCell2 in _inShowingVirtualValueBodyCellList)
		{
			UpdateTicketCellValue(newValue: (inShowingVirtualValueBodyCell2.Item1.TableCell == null) ? inShowingVirtualValueBodyCell2.Item4 : Auditai.Model.Cell.ChangeDataTypeImpl(inShowingVirtualValueBodyCell2.Item4, inShowingVirtualValueBodyCell2.Item1.TableCell.DisplayDataType), ticketCell: inShowingVirtualValueBodyCell2.Item1, isFormulaExistManualInputValue: true);
		}
		foreach (Tuple<TicketInputCellVM, int, int, string> inShowingVirtualValueTitleFooterCell2 in _inShowingVirtualValueTitleFooterCellList)
		{
			UpdateTicketCellValue(newValue: (inShowingVirtualValueTitleFooterCell2.Item1.TableCell == null) ? inShowingVirtualValueTitleFooterCell2.Item4 : Auditai.Model.Cell.ChangeDataTypeImpl(inShowingVirtualValueTitleFooterCell2.Item4, inShowingVirtualValueTitleFooterCell2.Item1.TableCell.DisplayDataType), ticketCell: inShowingVirtualValueTitleFooterCell2.Item1, isFormulaExistManualInputValue: true);
		}
		EndBatchUpdateValue();
		if (flag && _newAddedTableRowsList != null && _newAddedTableRowsList.Count > 0)
		{
			ExecuteNewAddedTableRowsWhichNotAbleAutoTriggerFormula();
		}
	}

	public int GetMixTicketColumnWhichRefTableColumn(int rowIndex, Auditai.Model.Column tableColumn)
	{
		if (tableColumn == null)
		{
			return -1;
		}
		if (rowIndex < 0 || rowIndex >= _rows.Count)
		{
			return -1;
		}
		for (int i = 0; i < _columns.Count; i++)
		{
			TicketInputCellVM cellVM = GetCellVM(rowIndex, i);
			if (cellVM.Column == tableColumn)
			{
				return i;
			}
		}
		return -1;
	}

	public List<int> GetMixTicketMixRangeFixedRowsIndex(int rowIndex)
	{
		List<int> list = new List<int>();
		int count = _rows.Count;
		if (rowIndex < 0 || rowIndex >= count)
		{
			return list;
		}
		TicketInputRowVM ticketInputRowVM = _rows[rowIndex];
		if (ticketInputRowVM.IsMixTicketDynamicDataRow || ticketInputRowVM.IsMixTicketFixedDataRow)
		{
			for (int i = rowIndex; i < count; i++)
			{
				if (_rows[i].IsMixTicketFixedDataRow)
				{
					list.Add(i);
				}
				else if (!_rows[i].IsMixTicketDynamicDataRow)
				{
					break;
				}
			}
			for (int num = rowIndex - 1; num >= 0; num--)
			{
				if (_rows[num].IsMixTicketFixedDataRow)
				{
					list.Add(num);
				}
				else if (!_rows[num].IsMixTicketDynamicDataRow)
				{
					break;
				}
			}
			list.Sort();
			return list;
		}
		return list;
	}

	public bool IsRowIndexOutOfRange(int rowIndex)
	{
		if (rowIndex < 0 || rowIndex >= _rows.Count)
		{
			return true;
		}
		return false;
	}

	public bool IsIndexOutOfRange(int rowIndex, int colIndex)
	{
		if (rowIndex < 0 || rowIndex >= _rows.Count)
		{
			return true;
		}
		if (colIndex < 0 || colIndex >= _columns.Count)
		{
			return true;
		}
		return false;
	}

	public void AddAttachment(int cellRowIndex, int cellColIndex, Guid fileId, string fileName)
	{
		TicketInputCellVM ticketInputCellVM = PrepareAttachmentsUpdateConditionForTableCell(cellRowIndex, cellColIndex);
		if (ticketInputCellVM.TableCell != null)
		{
			Table.CellPropManager.AddAttachment(ticketInputCellVM.TableCell, fileId, fileName);
			ticketInputCellVM.Attachments = null;
			if (Table.CellPropManager.TryGetAttachments(ticketInputCellVM.TableCell, out var attachments))
			{
				ticketInputCellVM.Attachments = attachments;
			}
		}
	}

	public void RenameAttachment(int cellRowIndex, int cellColIndex, int fileIndex, string newName)
	{
		TicketInputCellVM ticketInputCellVM = PrepareAttachmentsUpdateConditionForTableCell(cellRowIndex, cellColIndex);
		if (ticketInputCellVM.TableCell != null)
		{
			Table.CellPropManager.RenameAttachmentAt(ticketInputCellVM.TableCell, fileIndex, newName);
		}
	}

	public void RemoveAllAttachment(int cellRowIndex, int cellColIndex)
	{
		TicketInputCellVM ticketInputCellVM = PrepareAttachmentsUpdateConditionForTableCell(cellRowIndex, cellColIndex);
		if (ticketInputCellVM.TableCell != null)
		{
			Table.CellPropManager.RemoveAllAttachment(ticketInputCellVM.TableCell);
			ticketInputCellVM.Attachments = null;
		}
	}

	public void RemoveAttachment(int cellRowIndex, int cellColIndex, int fileIndex)
	{
		TicketInputCellVM ticketInputCellVM = PrepareAttachmentsUpdateConditionForTableCell(cellRowIndex, cellColIndex);
		if (ticketInputCellVM.TableCell != null)
			{
				Table.CellPropManager.RemoveAttachmentAt(ticketInputCellVM.TableCell, fileIndex);
				if (ticketInputCellVM.Attachments != null && ticketInputCellVM.Attachments.Attachments.Count == 0)
				{
					ticketInputCellVM.Attachments = null;
				}
			}
	}

	private TicketInputCellVM PrepareAttachmentsUpdateConditionForTableCell(int cellRowIndex, int cellColIndex)
	{
		TicketInputCellVM cellVM = GetCellVM(cellRowIndex, cellColIndex);
		if (cellVM.TableCell == null)
		{
			BuildTableCellForTicketCell(cellRowIndex, cellColIndex);
		}
		return cellVM;
	}

	public void BuildTableCellForTicketTitleFooterCell(TicketInputCellVM cell)
	{
		if (_ticket.Kind == TicketKind.DynamicRow)
		{
			BuildTableCellForTicketKeyCell_DynamicRow(cell);
		}
		else if (_ticket.Kind == TicketKind.FixedMultiRow)
		{
			BuildTableCellForAllTicketCell_FixedMultiRow();
		}
		else if (_ticket.Kind == TicketKind.FixedOneRow)
		{
			BuildTableCellForAllTicketCell_FixedOneRow();
		}
		else if (_ticket.Kind == TicketKind.FixedDataRowMixDynamicDataRow)
		{
			BuildTableCellForTicketKeyCell_MixTicket(cell);
			BuildTableCellTicektCellForWhichNeedWriteFormulaResultToTable_MixTicket();
		}
	}

	public void BuildTableCellForTicketCell(int cellRowIndex, int cellColIndex)
	{
		if (_ticket.Kind == TicketKind.DynamicRow)
		{
			BuildTableCellForTicketCell_DynamicRow(cellRowIndex, cellColIndex);
		}
		else if (_ticket.Kind == TicketKind.FixedMultiRow)
		{
			BuildTableCellForAllTicketCell_FixedMultiRow();
		}
		else if (_ticket.Kind == TicketKind.FixedOneRow)
		{
			BuildTableCellForAllTicketCell_FixedOneRow();
		}
		else if (_ticket.Kind == TicketKind.FixedDataRowMixDynamicDataRow)
		{
			BuildTableCellForTicketCell_MixTicket(cellRowIndex, cellColIndex);
			BuildTableCellTicektCellForWhichNeedWriteFormulaResultToTable_MixTicket();
		}
	}

	public void BuildTableRowsForTicketDataRows_DynamicRow(int ticketStartRowIndex, int count)
	{
		if (count == 0 || _rows.Count == 0)
		{
			return;
		}
		BeginBatchUpdateValue();
		int num = ticketStartRowIndex + count;
		if (num > _rows.Count)
		{
			num = _rows.Count;
		}
		int num2 = ticketStartRowIndex;
		Auditai.Model.Row row = null;
		while (num2 < num)
		{
			TicketInputRowVM ticketInputRowVM = _rows[num2];
			if (ticketInputRowVM.TableRow != null)
			{
				num2++;
				continue;
			}
			int num3 = GetNextTicketRowWhichExistTableRow(num2 + 1);
			if (num3 == -1)
			{
				int count2 = Table.Rows.Count;
				int count3 = num - num2;
				Table.Rows.Append(count3);
				RecordTableRowsToNewAddRow(count2, count3);
				if (row == null)
				{
					row = Table.Rows[count2];
				}
				BindTableRowsToTicketDataRow_DynamicRow(num2, count2, count3);
				break;
			}
			if (num3 >= num)
			{
				int index = _rows[num3].TableRow.Index;
				int count4 = num - num2;
				Table.Rows.Insert(index, count4);
				RecordTableRowsToNewAddRow(index, count4);
				if (row == null)
				{
					row = Table.Rows[index];
				}
				BindTableRowsToTicketDataRow_DynamicRow(num2, index, count4);
				break;
			}
			int index2 = _rows[num3].TableRow.Index;
			int num4 = num3 - num2;
			Table.Rows.Insert(index2, num4);
			RecordTableRowsToNewAddRow(index2, num4);
			if (row == null)
			{
				row = Table.Rows[index2];
			}
			BindTableRowsToTicketDataRow_DynamicRow(num2, index2, num4);
			num2 += num4;
		}
		if (DynamicRowKeyCells.Count > 0 && DynamicRowKeyCells[0].TableCell == null && row != null)
		{
			foreach (TicketInputCellVM dynamicRowKeyCell in DynamicRowKeyCells)
			{
				BindTableNewCreatedCellToTicketCell(dynamicRowKeyCell, row);
			}
		}
		EndBatchUpdateValue();
		int GetNextTicketRowWhichExistTableRow(int startRowIndex)
		{
			int count5 = _rows.Count;
			for (int i = startRowIndex; i < count5; i++)
			{
				TicketInputRowVM ticketInputRowVM2 = _rows[i];
				if (ticketInputRowVM2.TableRow != null)
				{
					return i;
				}
			}
			return -1;
		}
	}

	private void BindTableRowsToTicketDataRow_DynamicRow(int ticketStartIndex, int tableRowStartIndex, int count)
	{
		int count2 = _columns.Count;
		for (int i = 0; i < count; i++)
		{
			int num = ticketStartIndex + i;
			int index = tableRowStartIndex + i;
			Auditai.Model.Row row = Table.Rows[index];
			foreach (TicketInputCellVM dynamicRowKeyCell in DynamicRowKeyCells)
			{
				Auditai.Model.Cell tableCell = Table[row.Index, dynamicRowKeyCell.Column.Index];
				bool isFormulaExistManualInputValue = false;
				if (dynamicRowKeyCell.TableCell != null)
				{
					isFormulaExistManualInputValue = dynamicRowKeyCell.IsExistManualInputValue;
				}
				UpdateTableCellValue(tableCell, dynamicRowKeyCell.Value, isFormulaExistManualInputValue);
			}
			for (int j = 0; j < count2; j++)
			{
				TicketInputCellVM cellVM = GetCellVM(num, j);
				if (cellVM.Column != null)
				{
					BindTableNewCreatedCellToTicketCell(cellVM, row);
				}
			}
			DynamicRowRefTableDataRows.Add(row);
			_rows[num].TableRow = row;
		}
	}

	public void BuildTableRowsForTicketDataRows_MixTicket(int ticketStartRowIndex, int count)
	{
		if (count == 0 || _rows.Count == 0)
		{
			return;
		}
		BeginBatchUpdateValue();
		int num = ticketStartRowIndex + count;
		if (num > _rows.Count)
		{
			num = _rows.Count;
		}
		int num2 = ticketStartRowIndex;
		Auditai.Model.Row row = null;
		while (num2 < num)
		{
			TicketInputRowVM ticketInputRowVM = _rows[num2];
			if (ticketInputRowVM.TableRow != null)
			{
				num2++;
			}
			else if (!ticketInputRowVM.TicketRow.IsMixRangeDynamicDataRow && !ticketInputRowVM.TicketRow.IsMixRangeFixedDataRow)
			{
				num2++;
			}
			else if (ticketInputRowVM.TicketRow.IsMixRangeFixedDataRow)
			{
				int num3 = GetTableRowInsertIndex(num2 + 1);
				Table.Rows.Insert(num3, 1);
				RecordTableRowsToNewAddRow(num3, 1);
				if (row == null)
				{
					row = Table.Rows[num3];
				}
				BindTableRowsToTicketDataRow_MixTicket(num2, num3);
				num2++;
			}
			else
			{
				if (!ticketInputRowVM.TicketRow.IsMixRangeDynamicDataRow)
				{
					continue;
				}
				int num4 = GetNextTicketRowWhichExistTableRowInSameMixRange(num2 + 1, ticketInputRowVM.TicketRow.MixRangeDynamicDataRowTemplateId);
				if (num4 == -1)
				{
					int num5 = GetAfterRowsDataRowsCountIndexSameMixRange(num2, ticketInputRowVM.TicketRow.MixRangeDynamicDataRowTemplateId);
					if (num2 + num5 >= num)
					{
						num5 = num - num2;
					}
					int num6 = GetTableRowInsertIndex(num2 + 1);
					Table.Rows.Insert(num6, num5);
					RecordTableRowsToNewAddRow(num6, num5);
					if (row == null)
					{
						row = Table.Rows[num6];
					}
					BindTableRowsToTicketDynamicDataRow_MixTicket(num2, num6, num5);
					num2 += num5;
					continue;
				}
				if (num4 >= num)
				{
					int index = _rows[num4].TableRow.Index;
					int num7 = num - num2;
					Table.Rows.Insert(index, num7);
					RecordTableRowsToNewAddRow(index, num7);
					if (row == null)
					{
						row = Table.Rows[index];
					}
					BindTableRowsToTicketDynamicDataRow_MixTicket(num2, index, num7);
					break;
				}
				int index2 = _rows[num4].TableRow.Index;
				int num8 = num4 - num2;
				Table.Rows.Insert(index2, num8);
				RecordTableRowsToNewAddRow(index2, num8);
				if (row == null)
				{
					row = Table.Rows[index2];
				}
				BindTableRowsToTicketDynamicDataRow_MixTicket(num2, index2, num8);
				num2 += num8;
			}
		}
		if (MixTicketKeyCells.Count > 0 && MixTicketKeyCells[0].TableCell == null && row != null)
		{
			foreach (TicketInputCellVM mixTicketKeyCell in MixTicketKeyCells)
			{
				BindTableNewCreatedCellToTicketCell(mixTicketKeyCell, row);
			}
		}
		BuildTableCellTicektCellForWhichNeedWriteFormulaResultToTable_MixTicket();
		EndBatchUpdateValue();
		int GetAfterRowsDataRowsCountIndexSameMixRange(int startRowIndex, int templateRowId)
		{
			int num9 = 0;
			int count2 = _rows.Count;
			for (int i = startRowIndex; i < count2; i++)
			{
				TicketInputRowVM ticketInputRowVM2 = _rows[i];
				if (!ticketInputRowVM2.IsMixTicketDynamicDataRow || ticketInputRowVM2.TicketRow.MixRangeDynamicDataRowTemplateId != templateRowId)
				{
					break;
				}
				num9++;
			}
			return num9;
		}
		int GetNextTicketRowWhichExistTableRowInSameMixRange(int startRowIndex, int templateRowId)
		{
			int count3 = _rows.Count;
			for (int j = startRowIndex; j < count3; j++)
			{
				TicketInputRowVM ticketInputRowVM3 = _rows[j];
				if (!ticketInputRowVM3.IsMixTicketDynamicDataRow)
				{
					return -1;
				}
				if (ticketInputRowVM3.TicketRow.MixRangeDynamicDataRowTemplateId != templateRowId)
				{
					return -1;
				}
				if (ticketInputRowVM3.TableRow != null)
				{
					return j;
				}
			}
			return -1;
		}
		int GetTableRowInsertIndex(int vmStartRowIndex)
		{
			int count4 = _rows.Count;
			for (int k = vmStartRowIndex; k < count4; k++)
			{
				TicketInputRowVM ticketInputRowVM4 = _rows[k];
				if (ticketInputRowVM4.TableRow != null)
				{
					return ticketInputRowVM4.TableRow.Index;
				}
			}
			return Table.Rows.Count;
		}
	}

	private void BindTableRowsToTicketDataRow_MixTicket(int ticketRowIndex, int tableRowIndex)
	{
		Auditai.Model.Row row = Table.Rows[tableRowIndex];
		foreach (TicketInputCellVM mixTicketKeyCell in MixTicketKeyCells)
		{
			Auditai.Model.Cell tableCell = Table[row.Index, mixTicketKeyCell.Column.Index];
			bool isFormulaExistManualInputValue = false;
			if (mixTicketKeyCell.TableCell != null)
			{
				isFormulaExistManualInputValue = mixTicketKeyCell.IsExistManualInputValue;
			}
			UpdateTableCellValue(tableCell, mixTicketKeyCell.Value, isFormulaExistManualInputValue);
		}
		for (int i = 0; i < _ticket.Columns.Count; i++)
		{
			TicketInputCellVM cellVM = GetCellVM(ticketRowIndex, i);
			if (cellVM.IsMixTicketExistDesignInputValue)
			{
				Auditai.Model.Column column = cellVM.Column;
				if (column != null)
				{
					Auditai.Model.Cell tableCell2 = Table[row.Index, column.Index];
					string inputValue = cellVM.TicketCell.GetInputValue();
					object value = Auditai.Model.Cell.ChangeDataTypeImpl(inputValue, column.GetDataType());
					UpdateTableCellValue(tableCell2, value, isFormulaExistManualInputValue: false);
				}
			}
		}
		for (int j = 0; j < _ticket.Columns.Count; j++)
		{
			TicketInputCellVM cellVM2 = GetCellVM(ticketRowIndex, j);
			if (cellVM2.Column != null)
			{
				BindTableNewCreatedCellToTicketCell(cellVM2, row);
			}
		}
		MixTicketDataRowsRefTableRows.Add(row);
		_rows[ticketRowIndex].TableRow = row;
	}

	private void BindTableRowsToTicketDynamicDataRow_MixTicket(int ticketRowIndex, int tableRowIndex, int rowsCount)
	{
		for (int i = 0; i < rowsCount; i++)
		{
			BindTableRowsToTicketDataRow_MixTicket(ticketRowIndex + i, tableRowIndex + i);
		}
	}

	private int GetDataRowInsertBeforeWhichTableRow_DynamicRow(int ticketRowIndex)
	{
		int count = _rows.Count;
		for (int i = ticketRowIndex + 1; i < count; i++)
		{
			TicketInputRowVM ticketInputRowVM = _rows[i];
			if (ticketInputRowVM.IsDynamicRowTicketDataRow && ticketInputRowVM.TableRow != null)
			{
				return ticketInputRowVM.TableRow.Index;
			}
		}
		return -1;
	}

	private int GetDataRowInsertBeforeWhichTableRow_MixTicket(int ticketRowIndex)
	{
		int mixRangeDynamicDataRowTemplateId = _rows[ticketRowIndex].TicketRow.MixRangeDynamicDataRowTemplateId;
		int count = _rows.Count;
		for (int i = ticketRowIndex + 1; i < count; i++)
		{
			TicketInputRowVM ticketInputRowVM = _rows[i];
			if (ticketInputRowVM.TicketRow == null || !ticketInputRowVM.TicketRow.IsMixRangeDynamicDataRow || ticketInputRowVM.TicketRow.MixRangeDynamicDataRowTemplateId != mixRangeDynamicDataRowTemplateId)
			{
				break;
			}
			if (ticketInputRowVM.TableRow != null)
			{
				return ticketInputRowVM.TableRow.Index;
			}
		}
		return -1;
	}

	private void BuildTableCellForTicketKeyCell_DynamicRow(TicketInputCellVM cell)
	{
		if (cell.TableCell != null)
		{
			return;
		}
		Table.Rows.Append(1);
		Auditai.Model.Row row = Table.Rows[Table.Rows.Count - 1];
		RecordTableRowsToNewAddRow(row);
		DynamicRowKeyCellsRefEmptyTableRow = row;
		foreach (TicketInputCellVM dynamicRowKeyCell in DynamicRowKeyCells)
		{
			if (dynamicRowKeyCell.Column != null)
			{
				BindTableNewCreatedCellToTicketCell(dynamicRowKeyCell, row);
			}
		}
	}

	private void BuildTableCellForTicketKeyCell_MixTicket(TicketInputCellVM cell)
	{
		if (cell.TableCell != null)
		{
			return;
		}
		Table.Rows.Append(1);
		Auditai.Model.Row row = Table.Rows[Table.Rows.Count - 1];
		RecordTableRowsToNewAddRow(row);
		MixTicketKeyCellsRefEmptyTableRow = row;
		foreach (TicketInputCellVM mixTicketKeyCell in MixTicketKeyCells)
		{
			if (mixTicketKeyCell.Column != null)
			{
				BindTableNewCreatedCellToTicketCell(mixTicketKeyCell, row);
			}
		}
	}

	private void BuildTableCellForTicketCell_DynamicRow(int cellRowIndex, int cellColIndex)
	{
		TicketInputRowVM ticketInputRowVM = _rows[cellRowIndex];
		TicketInputCellVM cellVM = GetCellVM(cellRowIndex, cellColIndex);
		if (!ticketInputRowVM.IsDynamicRowTicketDataRow)
		{
			if (cellVM.IsDynamicRowKeyCell)
			{
				BuildTableCellForTicketKeyCell_DynamicRow(cellVM);
			}
		}
		else
		{
			if (cellVM.TableCell != null)
			{
				return;
			}
			if (cellVM.IsDynamicRowKeyCell)
			{
				BuildTableCellForTicketKeyCell_DynamicRow(cellVM);
				return;
			}
			BeginBatchUpdateValue();
			Auditai.Model.Row row = null;
			int dataRowInsertBeforeWhichTableRow_DynamicRow = GetDataRowInsertBeforeWhichTableRow_DynamicRow(cellRowIndex);
			if (dataRowInsertBeforeWhichTableRow_DynamicRow < 0)
			{
				Table.Rows.Append(1);
				row = Table.Rows[Table.Rows.Count - 1];
			}
			else
			{
				Table.Rows.Insert(dataRowInsertBeforeWhichTableRow_DynamicRow, 1);
				row = Table.Rows[dataRowInsertBeforeWhichTableRow_DynamicRow];
			}
			RecordTableRowsToNewAddRow(row);
			DynamicRowRefTableDataRows.Add(row);
			ticketInputRowVM.TempRow = row;
			ticketInputRowVM.TableRow = row;
			ticketInputRowVM.IsNew = true;
			if (DynamicRowKeyCells.Count > 0 && DynamicRowKeyCells[0].TableCell == null)
			{
				foreach (TicketInputCellVM dynamicRowKeyCell in DynamicRowKeyCells)
				{
					BindTableNewCreatedCellToTicketCell(dynamicRowKeyCell, row);
				}
				DynamicRowKeyCellsRefEmptyTableRow = row;
			}
			foreach (TicketInputCellVM dynamicRowKeyCell2 in DynamicRowKeyCells)
			{
				Auditai.Model.Cell tableCell = Table[row.Index, dynamicRowKeyCell2.Column.Index];
				bool isFormulaExistManualInputValue = false;
				if (dynamicRowKeyCell2.TableCell != null)
				{
					isFormulaExistManualInputValue = dynamicRowKeyCell2.TableCell.IsExistManualInputValue;
				}
				UpdateTableCellValue(tableCell, dynamicRowKeyCell2.Value, isFormulaExistManualInputValue);
			}
			for (int i = 0; i < _columns.Count; i++)
			{
				TicketInputCellVM cellVM2 = GetCellVM(cellRowIndex, i);
				if (cellVM2.Column != null)
				{
					BindTableNewCreatedCellToTicketCell(cellVM2, row);
				}
			}
			EndBatchUpdateValue();
		}
	}

	private void BuildTableCellForTicketCell_MixTicket(int cellRowIndex, int cellColIndex)
	{
		TicketInputRowVM ticketRow = _rows[cellRowIndex];
		TicketInputCellVM cellVM = GetCellVM(cellRowIndex, cellColIndex);
		if (ticketRow.TicketRow == null || (!ticketRow.TicketRow.IsMixRangeDynamicDataRow && !ticketRow.TicketRow.IsMixRangeFixedDataRow))
		{
			if (cellVM.IsMixTicketKeyCell)
			{
				BuildTableCellForTicketKeyCell_MixTicket(cellVM);
			}
		}
		else
		{
			if (cellVM.TableCell != null)
			{
				return;
			}
			if (cellVM.IsMixTicketKeyCell)
			{
				BuildTableCellForTicketKeyCell_MixTicket(cellVM);
				return;
			}
			BeginBatchUpdateValue();
			Auditai.Model.Row row = null;
			int dataRowInsertBeforeWhichTableRow_MixTicket = GetDataRowInsertBeforeWhichTableRow_MixTicket(cellRowIndex);
			if (dataRowInsertBeforeWhichTableRow_MixTicket < 0)
			{
				Table.Rows.Append(1);
				row = Table.Rows[Table.Rows.Count - 1];
			}
			else
			{
				Table.Rows.Insert(dataRowInsertBeforeWhichTableRow_MixTicket, 1);
				row = Table.Rows[dataRowInsertBeforeWhichTableRow_MixTicket];
			}
			RecordTableRowsToNewAddRow(row);
			MixTicketDataRowsRefTableRows.Add(row);
			ticketRow.TempRow = row;
			ticketRow.TableRow = row;
			ticketRow.IsNew = true;
			if (MixTicketKeyCells.Count > 0 && MixTicketKeyCells[0].TableCell == null)
			{
				foreach (TicketInputCellVM mixTicketKeyCell in MixTicketKeyCells)
				{
					BindTableNewCreatedCellToTicketCell(mixTicketKeyCell, row);
				}
				MixTicketKeyCellsRefEmptyTableRow = row;
			}
			foreach (TicketInputCellVM mixTicketKeyCell2 in MixTicketKeyCells)
			{
				if (mixTicketKeyCell2.Column == null) continue;
				Auditai.Model.Cell tableCell = Table[row.Index, mixTicketKeyCell2.Column.Index];
				if (tableCell == null) continue;
				bool isFormulaExistManualInputValue = false;
				if (mixTicketKeyCell2.TableCell != null)
				{
					isFormulaExistManualInputValue = mixTicketKeyCell2.TableCell.IsExistManualInputValue;
				}
				UpdateTableCellValue(tableCell, mixTicketKeyCell2.Value, isFormulaExistManualInputValue);
			}
			if (ticketRow.IsMixTicketFixedDataRow)
			{
				for (int i = 0; i < _ticket.Columns.Count; i++)
				{
					TicketInputCellVM cellVM2 = GetCellVM(cellRowIndex, i);
					if (cellVM2.IsMixTicketExistDesignInputValue)
					{
						Auditai.Model.Column column = cellVM2.Column;
						if (column != null)
						{
							Auditai.Model.Cell tableCell2 = Table[row.Index, column.Index];
							string inputValue = cellVM2.TicketCell.GetInputValue();
							object value = Auditai.Model.Cell.ChangeDataTypeImpl(inputValue, column.GetDataType());
							UpdateTableCellValue(tableCell2, value, isFormulaExistManualInputValue: false);
						}
					}
				}
			}
			else if (ticketRow.IsMixTicketDynamicDataRow)
			{
				TicketTableMixRangeTemplateRow ticketTableMixRangeTemplateRow = _ticket.FixedAndDynamicMixRange.DynamicDataRowTemplateRows.Find((TicketTableMixRangeTemplateRow u) => u.TemplateId == ticketRow.TicketRow.MixRangeDynamicDataRowTemplateId);
				if (ticketTableMixRangeTemplateRow != null)
				{
					for (int j = 0; j < _ticket.Columns.Count; j++)
					{
						TicketInputCellVM cellVM3 = GetCellVM(cellRowIndex, j);
						if (cellVM3.IsMixTicketExistDesignInputValue)
						{
							Auditai.Model.Column column2 = cellVM3.Column;
							if (column2 != null)
							{
								Auditai.Model.Cell tableCell3 = Table[row.Index, column2.Index];
								string inputValue2 = cellVM3.TicketCell.GetInputValue();
								object value2 = Auditai.Model.Cell.ChangeDataTypeImpl(inputValue2, column2.GetDataType());
								UpdateTableCellValue(tableCell3, value2, isFormulaExistManualInputValue: false);
							}
						}
					}
				}
			}
			for (int k = 0; k < _columns.Count; k++)
			{
				TicketInputCellVM cellVM4 = GetCellVM(cellRowIndex, k);
				if (cellVM4.Column != null)
				{
					BindTableNewCreatedCellToTicketCell(cellVM4, row);
				}
			}
			EndBatchUpdateValue();
		}
	}

	public int GetLastDataRowIndex()
	{
		if (DataRowsCount == 0)
		{
			return -1;
		}
		return _ticket.DataRowStart + DataRowsCount;
	}

	public TicketInputRowVM InsertDataRow(int ticketRowIndex)
	{
		return InsertDataRow_DynamicRowTicket(ticketRowIndex);
	}

	public TicketInputRowVM AppendDataRow()
	{
		return InsertDataRow_DynamicRowTicket(_ticket.DataRowStart + DataRowsCount);
	}

	public void InsertDataRows(int ticketRowIndex, int count)
	{
		InsertDataRows_DynamicRowTicket(ticketRowIndex, count);
	}

	public void AppendDataRows(int count)
	{
		InsertDataRows_DynamicRowTicket(_ticket.DataRowStart + DataRowsCount, count);
	}

	public void RemoveDataRow(int ticketRowIndex)
	{
		TicketInputRowVM ticketInputRowVM = _rows[ticketRowIndex];
		if (ticketInputRowVM.TableRow != null)
		{
			if (_ticket.Kind == TicketKind.DynamicRow)
			{
				DynamicRowRefTableDataRows.Remove(ticketInputRowVM.TableRow);
			}
			else if (_ticket.Kind == TicketKind.FixedDataRowMixDynamicDataRow)
			{
				MixTicketDataRowsRefTableRows.Remove(ticketInputRowVM.TableRow);
			}
			RemovedRows.Add(ticketInputRowVM.TableRow);
		}
		RemoveRow(ticketRowIndex);
	}

	public void RemoveDataRows(int ticketRowIndex, int count)
	{
		if (count == 0)
		{
			return;
		}
		for (int i = 0; i < count; i++)
		{
			TicketInputRowVM ticketInputRowVM = _rows[ticketRowIndex + i];
			if (ticketInputRowVM.TableRow != null)
			{
				if (_ticket.Kind == TicketKind.DynamicRow)
				{
					DynamicRowRefTableDataRows.Remove(ticketInputRowVM.TableRow);
				}
				else if (_ticket.Kind == TicketKind.FixedDataRowMixDynamicDataRow)
				{
					MixTicketDataRowsRefTableRows.Remove(ticketInputRowVM.TableRow);
				}
				RemovedRows.Add(ticketInputRowVM.TableRow);
			}
		}
		RemoveRows(ticketRowIndex, count);
	}

	public TicketInputRowVM InsertDataRow_MixTicket(int ticketRowIndex, int templateRowId)
	{
		return InsertDynamicDataRowForMixTicket(ticketRowIndex, templateRowId);
	}

	public void InsertDataRows_MixTicket(int ticketRowIndex, int count, int templateRowId)
	{
		InsertDynamicDataRowsForMixTicket(ticketRowIndex, count, templateRowId);
	}

	public void BuildTableCellForAllTicketCell()
	{
		if (!IsInShowingVirtualNode && _ticket.Kind != TicketKind.DynamicRow)
		{
			if (_ticket.Kind == TicketKind.FixedMultiRow)
			{
				BuildTableCellForAllTicketCell_FixedMultiRow();
				return;
			}
			if (_ticket.Kind == TicketKind.FixedOneRow)
			{
				BuildTableCellForAllTicketCell_FixedOneRow();
				return;
			}
			_ = _ticket.Kind;
			_ = 4;
		}
	}

	private Auditai.Model.Cell BindTableNewCreatedCellToTicketCell(TicketInputCellVM ticketCell, Auditai.Model.Row tableRow)
	{
		return ticketCell.TableCell = (ticketCell.TempCell = Table[tableRow.Index, ticketCell.Column.Index]);
	}

	private void BuildTableCellForAllTicketCell_DynamicRow()
	{
		if (DynamicRowKeyCells.Count == 0 || DynamicRowKeyCells[0].TableCell != null)
		{
			return;
		}
		Table.Rows.Append(1);
		Auditai.Model.Row row = Table.Rows[Table.Rows.Count - 1];
		RecordTableRowsToNewAddRow(row);
		DynamicRowKeyCellsRefEmptyTableRow = row;
		foreach (TicketInputCellVM dynamicRowKeyCell in DynamicRowKeyCells)
		{
			if (dynamicRowKeyCell.Column != null)
			{
				BindTableNewCreatedCellToTicketCell(dynamicRowKeyCell, row);
			}
		}
	}

	private void BuildTableCellForAllTicketCell_FixedOneRow()
	{
		if (_isBuildTableCellForAllTicketCell)
		{
			return;
		}
		_isBuildTableCellForAllTicketCell = true;
		IEnumerable<TicketInputCellVM> enumerable = _cells.Where((TicketInputCellVM u) => u.IsField);
		TicketInputCellVM ticketInputCellVM = null;
		foreach (TicketInputCellVM item in enumerable)
		{
			if (item.Column != null)
			{
				ticketInputCellVM = item;
				break;
			}
		}
		if (ticketInputCellVM == null || ticketInputCellVM.TableRow != null)
		{
			return;
		}
		Table.Rows.Append(1);
		Auditai.Model.Row row = Table.Rows[Table.Rows.Count - 1];
		RecordTableRowsToNewAddRow(row);
		_isFixedOneRowTicketRefTableRowBeNewCreated = true;
		foreach (TicketInputCellVM cell in _cells)
		{
			if (cell.IsField && cell.Column != null)
			{
				BindTableNewCreatedCellToTicketCell(cell, row);
			}
		}
		foreach (TicketInputCellVM cell2 in Title.Cells)
		{
			if (cell2.IsField && cell2.Column != null)
			{
				BindTableNewCreatedCellToTicketCell(cell2, row);
			}
		}
		foreach (TicketInputCellVM cell3 in Footer.Cells)
		{
			if (cell3.IsField && cell3.Column != null)
			{
				BindTableNewCreatedCellToTicketCell(cell3, row);
			}
		}
	}

	private void BuildTableCellForAllTicketCell_FixedMultiRow()
	{
		if (_isBuildTableCellForAllTicketCell)
		{
			return;
		}
		_isBuildTableCellForAllTicketCell = true;
		int num = FixedMultiRowVMs.Count((TicketFixedMultiRowVM u) => u.Row == null);
		if (num == 0)
		{
			return;
		}
		List<TicketInputCellVM> list = FixedCells.ToList();
		Queue<Auditai.Model.Row> queue = new Queue<Auditai.Model.Row>();
		if (num == FixedMultiRowVMs.Count)
		{
			TicketInputCellVM ticketInputCellVM = list.FirstOrDefault((TicketInputCellVM u) => u.TableRow != null);
			if (ticketInputCellVM != null)
			{
				num--;
				queue.Enqueue(ticketInputCellVM.TableRow);
			}
		}
		BeginBatchUpdateValue();
		if (num > 0)
		{
			int count = Table.Rows.Count;
			Table.Rows.Append(num);
			for (int i = 0; i < num; i++)
			{
				queue.Enqueue(Table.Rows[count + i]);
			}
			RecordTableRowsToNewAddRow(count, num);
		}
		Auditai.Model.Row row = null;
		foreach (TicketFixedMultiRowVM fixedMultiRowVM in FixedMultiRowVMs)
		{
			if (fixedMultiRowVM.Row != null)
			{
				continue;
			}
			Auditai.Model.Row row3 = (fixedMultiRowVM.Row = queue.Dequeue());
			if (row == null)
			{
				row = row3;
			}
			foreach (TicketInputCellVM keyCell in fixedMultiRowVM.KeyCells)
			{
				Auditai.Model.Cell tableCell = BindTableNewCreatedCellToTicketCell(keyCell, row3);
				UpdateTableCellValue(tableCell, keyCell.Value, isFormulaExistManualInputValue: true);
			}
			foreach (TicketInputCellVM valueCell in fixedMultiRowVM.ValueCells)
			{
				BindTableNewCreatedCellToTicketCell(valueCell, row3);
			}
			foreach (TicketInputCellVM item in list)
			{
				Auditai.Model.Cell tableCell2 = Table[row3.Index, item.Column.Index];
				bool isFormulaExistManualInputValue = false;
				if (item.TableCell != null)
				{
					isFormulaExistManualInputValue = item.TableCell.IsExistManualInputValue;
				}
				UpdateTableCellValue(tableCell2, item.Value, isFormulaExistManualInputValue);
			}
		}
		Auditai.Model.Row row4 = FixedMultiRowVMs[0].Row;
		foreach (TicketInputCellVM item2 in list)
		{
			if (item2.TableRow == null)
			{
				Auditai.Model.Cell tableCell3 = (item2.TempCell = Table[row4.Index, item2.Column.Index]);
				item2.TableCell = tableCell3;
			}
		}
		EndBatchUpdateValue();
	}

	private void BuildTableCellForAllTicketCell_MixTicket()
	{
		if (MixTicketKeyCells.Count == 0 || MixTicketKeyCells[0].TableCell != null)
		{
			return;
		}
		Table.Rows.Append(1);
		Auditai.Model.Row row = Table.Rows[Table.Rows.Count - 1];
		RecordTableRowsToNewAddRow(row);
		MixTicketKeyCellsRefEmptyTableRow = row;
		foreach (TicketInputCellVM mixTicketKeyCell in MixTicketKeyCells)
		{
			if (mixTicketKeyCell.Column != null)
			{
				BindTableNewCreatedCellToTicketCell(mixTicketKeyCell, row);
			}
		}
	}

	private void UpdateTableCellValue(Auditai.Model.Cell tableCell, object value, bool isFormulaExistManualInputValue)
	{
		if (!tableCell.HasCellFormulaOrColumnFormula)
		{
			tableCell.UpdateValue(value);
			tableCell.IsExistManualInputValue = false;
		}
		else if (tableCell.IsAllowManualInputOnFormula)
		{
			tableCell.UpdateValue(value);
			tableCell.IsExistManualInputValue = isFormulaExistManualInputValue;
		}
	}

	public void UpdateTicketCellValue(TicketInputCellVM ticketCell, object newValue, bool isFormulaExistManualInputValue)
	{
		switch (_ticket.Kind)
		{
		case TicketKind.FixedOneRow:
			UpdateTicketCellValue_FixedOneRow(ticketCell, newValue, isFormulaExistManualInputValue);
			break;
		case TicketKind.FixedMultiRow:
			UpdateTicketCellValue_FixedMultiRow(ticketCell, newValue, isFormulaExistManualInputValue);
			break;
		case TicketKind.DynamicRow:
			UpdateTicketCellValue_DynamicRow(ticketCell, newValue, isFormulaExistManualInputValue);
			break;
		case TicketKind.FixedDataRowMixDynamicDataRow:
			UpdateTicketCellValue_MixTicket(ticketCell, newValue, isFormulaExistManualInputValue);
			break;
		}
	}

	public void UpdateTicketCellValue_DynamicRowDataRow(TicketInputCellVM ticketCell, object newValue)
	{
		ticketCell.Value = newValue;
		if (ticketCell.TableCell != null)
		{
			UpdateTableCellValue(ticketCell.TableCell, newValue, isFormulaExistManualInputValue: true);
		}
	}

	private void UpdateTicketCellValue_FixedOneRow(TicketInputCellVM ticketCell, object newValue, bool isFormulaExistManualInputValue)
	{
		ticketCell.Value = newValue;
		if (ticketCell.TableCell != null)
		{
			UpdateTableCellValue(ticketCell.TableCell, newValue, isFormulaExistManualInputValue);
		}
	}

	private void UpdateTicketCellValue_FixedMultiRow(TicketInputCellVM ticketCell, object newValue, bool isFormulaExistManualInputValue)
	{
		ticketCell.Value = newValue;
		if (ticketCell.Column == null)
		{
			return;
		}
		if (ticketCell.IsFixedMultiFixedCell)
		{
			BeginBatchUpdateValue();
			foreach (TicketFixedMultiRowVM fixedMultiRowVM in FixedMultiRowVMs)
			{
				if (fixedMultiRowVM.Row != null)
				{
					Auditai.Model.Cell tableCell = Table[fixedMultiRowVM.Row.Index, ticketCell.Column.Index];
					UpdateTableCellValue(tableCell, newValue, isFormulaExistManualInputValue);
				}
			}
			EndBatchUpdateValue();
		}
		else if ((ticketCell.IsFixedMultiRowKey || ticketCell.IsFixedMultiRowValue) && ticketCell.TableCell != null)
		{
			UpdateTableCellValue(ticketCell.TableCell, newValue, isFormulaExistManualInputValue);
		}
	}

	protected void UpdateTicketCellValue_DynamicRow(TicketInputCellVM ticketCell, object value, bool isFormulaExistManualInputValue)
	{
		ticketCell.Value = value;
		if (ticketCell.TableCell == null)
		{
			return;
		}
		if (ticketCell.IsDynamicRowKeyCell)
		{
			BeginBatchUpdateValue();
			bool flag = false;
			foreach (Auditai.Model.Row dynamicRowRefTableDataRow in DynamicRowRefTableDataRows)
			{
				Auditai.Model.Cell cell = Table[dynamicRowRefTableDataRow.Index, ticketCell.Column.Index];
				UpdateTableCellValue(cell, value, isFormulaExistManualInputValue);
				if (cell == ticketCell.TableCell)
				{
					flag = true;
				}
			}
			if (DynamicRowKeyCellsRefEmptyTableRow != null)
			{
				Auditai.Model.Cell tableCell = Table[DynamicRowKeyCellsRefEmptyTableRow.Index, ticketCell.Column.Index];
				UpdateTableCellValue(tableCell, value, isFormulaExistManualInputValue);
			}
			if (!flag)
			{
				UpdateTableCellValue(ticketCell.TableCell, value, isFormulaExistManualInputValue);
			}
			EndBatchUpdateValue();
		}
		else if (ticketCell.IsDynamicRowDataCell)
		{
			UpdateTableCellValue(ticketCell.TableCell, value, isFormulaExistManualInputValue);
		}
	}

	protected void UpdateTicketCellValue_MixTicket(TicketInputCellVM ticketCell, object value, bool isFormulaExistManualInputValue)
	{
		ticketCell.Value = value;
		if (ticketCell.TableCell == null)
		{
			return;
		}
		if (ticketCell.IsMixTicketKeyCell)
		{
			BeginBatchUpdateValue();
			bool flag = false;
			foreach (Auditai.Model.Row mixTicketDataRowsRefTableRow in MixTicketDataRowsRefTableRows)
			{
				Auditai.Model.Cell cell = Table[mixTicketDataRowsRefTableRow.Index, ticketCell.Column.Index];
				UpdateTableCellValue(cell, value, isFormulaExistManualInputValue);
				if (cell == ticketCell.TableCell)
				{
					flag = true;
				}
			}
			if (MixTicketKeyCellsRefEmptyTableRow != null)
			{
				Auditai.Model.Cell tableCell = Table[MixTicketKeyCellsRefEmptyTableRow.Index, ticketCell.Column.Index];
				UpdateTableCellValue(tableCell, value, isFormulaExistManualInputValue);
			}
			if (!flag)
			{
				UpdateTableCellValue(ticketCell.TableCell, value, isFormulaExistManualInputValue);
			}
			EndBatchUpdateValue();
		}
		else if (ticketCell.IsMixTicketFixedDataRow || ticketCell.IsMixTicketDynamicDataRow)
		{
			UpdateTableCellValue(ticketCell.TableCell, value, isFormulaExistManualInputValue);
		}
	}

	public void BeginBatchUpdateValue()
	{
		_batchingUpdateValueDepth++;
		if (_batchingUpdateValueDepth == 1)
		{
			Table.BeginBatchUpdateValue();
		}
	}

	public void EndBatchUpdateValue()
	{
		_batchingUpdateValueDepth--;
		if (_batchingUpdateValueDepth == 0)
		{
			ExecuteNewAddTableRowsFormula();
			Table.EndBatchUpdateValue();
		}
	}

	public void ExecuteNewAddedTableRowsWhichNotAbleAutoTriggerFormula()
	{
		ExecuteNewAddTableRowsFormula();
		Table.ExecuteBatchUpdateCellTriggers();
	}

	private void ExecuteNewAddTableRowsFormula()
	{
		_isNeedRecordNewAddedTableRow = false;
		if (_newAddedTableRowsList == null)
		{
			return;
		}
		if (_newAddedTableRowsList.Count == 0)
		{
			_newAddedTableRowsList = null;
			return;
		}
		try
		{
			List<Auditai.Model.Column> list = null;
			foreach (Auditai.Model.Column column in Table.Columns)
			{
				if (column.HasFormula && !column.IsFormulaRefSelfTableColumnOrCell)
				{
					if (list == null)
					{
						list = new List<Auditai.Model.Column>();
					}
					list.Add(column);
				}
			}
			if (list == null)
			{
				return;
			}
			HashSet<int> hashSet = new HashSet<int>();
			foreach (Auditai.Model.Row newAddedTableRows in _newAddedTableRowsList)
			{
				if (newAddedTableRows.Status != SyncStatus.LocalDeleted)
				{
					hashSet.Add(newAddedTableRows.Index);
				}
			}
			List<int> list2 = hashSet.ToList();
			list2.Sort((int left, int right) => left.CompareTo(right));
			foreach (Auditai.Model.Column item in list)
			{
				try
				{
					item.TryApplyFormulaToRows(list2);
				}
				catch
				{
				}
			}
		}
		catch (Exception exception)
		{
			exception.Log();
		}
		finally
		{
			_newAddedTableRowsList = null;
		}
	}

	public static bool IsExistFillFormula(FormulaEvaluator formulaEval)
	{
		IsFillFormula isFillFormula = formulaEval.IsFill();
		if (!isFillFormula.IsFill)
		{
			return isFillFormula.IsLqCollect;
		}
		return true;
	}

	public static bool IsExistFillFormula(FormulaEvaluator formulaEval, out bool isAllowModifyTableRowOrder)
	{
		IsFillFormula isFillFormula = formulaEval.IsFill();
		isAllowModifyTableRowOrder = true;
		if (isFillFormula.IsLqAsc || isFillFormula.IsLqDesc || isFillFormula.IsLqCollect)
		{
			isAllowModifyTableRowOrder = false;
		}
		if (!isFillFormula.IsFill)
		{
			return isFillFormula.IsLqCollect;
		}
		return true;
	}

	private TicketInputCellVM BuildCellVMForNewInsertRow(TicketInputRowVM row, TicketColumn column)
	{
		TicketInputCellVM ticketInputCellVM = new TicketInputCellVM
		{
			Value = "",
			IsDynamicTicketDataRow = true,
			TicketColumn = column
		};
		ticketInputCellVM.IsDynamicRowDataCell = true;
		if (column.HasField())
		{
			ticketInputCellVM.IsField = true;
			ticketInputCellVM.Column = _ticket.GetFieldColumn(column.Field);
			if (ticketInputCellVM.Column != null)
			{
				ticketInputCellVM.TempCell = new Auditai.Model.Cell
				{
					Column = ticketInputCellVM.Column,
					Row = row.TempRow,
					Value = ""
				};
				if (ticketInputCellVM.Column.HasFormula)
				{
					ticketInputCellVM.IsFormula = true;
					ticketInputCellVM.Formula = ticketInputCellVM.Column.Formula;
					ticketInputCellVM.IsFormulaFromTicket = false;
				}
			}
		}
		if (column.HasFormula())
		{
			ticketInputCellVM.IsFormula = true;
			ticketInputCellVM.Formula = column.Formula;
			ticketInputCellVM.IsFormulaFromTicket = true;
		}
		if (ticketInputCellVM.TempCell == null)
		{
			ticketInputCellVM.TempCell = new Auditai.Model.Cell
			{
				Column = ticketInputCellVM.Column,
				Row = DummyMR,
				Value = ""
			};
		}
		return ticketInputCellVM;
	}

	protected TicketInputRowVM InsertDataRow_DynamicRowTicket(int index)
	{
		TicketInputRowVM ticketInputRowVM = new TicketInputRowVM
		{
			TempRow = new Auditai.Model.Row
			{
				Creator = Auditai.Model.User.Current.Id
			},
			IsNew = true,
			IsDynamicRowTicketDataRow = true
		};
		_rows.Insert(index, ticketInputRowVM);
		List<TicketInputCellVM> list = new List<TicketInputCellVM>();
		for (int i = 0; i < _ticket.Columns.Count; i++)
		{
			list.Add(BuildCellVMForNewInsertRow(ticketInputRowVM, _ticket.Columns[i]));
		}
		_cells.InsertRange(index * _columns.Count, list);
		DataRowsCount++;
		PopulateMerges();
		return ticketInputRowVM;
	}

	protected void InsertDataRows_DynamicRowTicket(int index, int count)
	{
		List<TicketInputRowVM> list = (from i in Enumerable.Range(0, count)
			select new TicketInputRowVM
			{
				TempRow = new Auditai.Model.Row
				{
					Creator = Auditai.Model.User.Current.Id
				},
				IsNew = true,
				IsDynamicRowTicketDataRow = true
			}).ToList();
		_rows.InsertRange(index, list);
		int count2 = _ticket.Columns.Count;
		List<TicketInputCellVM> list2 = new List<TicketInputCellVM>();
		for (int j = 0; j < count; j++)
		{
			for (int k = 0; k < count2; k++)
			{
				list2.Add(BuildCellVMForNewInsertRow(list[j], _ticket.Columns[k]));
			}
		}
		_cells.InsertRange(index * count2, list2);
		DataRowsCount += count;
		PopulateMerges();
	}

	protected void RemoveRow(int index)
	{
		MixTicketFixTickRowIndexMapperToVMRowIndexDic = null;
		_rows.RemoveAt(index);
		_cells.RemoveRange(index * _columns.Count, _columns.Count);
		if (_ticket.Kind == TicketKind.DynamicRow)
		{
			DataRowsCount--;
		}
		PopulateMerges();
	}

	protected void RemoveRows(int index, int count)
	{
		MixTicketFixTickRowIndexMapperToVMRowIndexDic = null;
		_rows.RemoveRange(index, count);
		_cells.RemoveRange(index * _columns.Count, _columns.Count * count);
		if (_ticket.Kind == TicketKind.DynamicRow)
		{
			DataRowsCount -= count;
		}
		PopulateMerges();
	}

	public bool IsRecordDataRow_DynamicRowTicket(int index)
	{
		if (_ticket.Kind != TicketKind.DynamicRow)
		{
			return false;
		}
		if (index >= _ticket.DataRowStart)
		{
			return index < _ticket.DataRowStart + DataRowsCount;
		}
		return false;
	}

	private TicketInputCellVM BuildCellVMForNewInsertRowForMixTicket(TicketInputRowVM row, TicketColumn column, TicketCell templateCell)
	{
		TicketInputCellVM ticketInputCellVM = new TicketInputCellVM
		{
			Value = templateCell.Text,
			TicketColumn = column,
			IsDynamicTicketDataRow = false,
			TicketCell = templateCell
		};
		ticketInputCellVM.IsMixTicketDynamicDataRow = true;
		if (templateCell.HasField())
		{
			ticketInputCellVM.IsField = true;
			ticketInputCellVM.Column = _ticket.GetFieldColumn(templateCell.Field);
			if (ticketInputCellVM.Column != null)
			{
				ticketInputCellVM.TempCell = new Auditai.Model.Cell
				{
					Column = ticketInputCellVM.Column,
					Row = row.TempRow,
					Value = ""
				};
				if (ticketInputCellVM.Column.HasFormula)
				{
					ticketInputCellVM.IsFormula = true;
					ticketInputCellVM.Formula = ticketInputCellVM.Column.Formula;
					ticketInputCellVM.IsFormulaFromTicket = false;
				}
				if (!string.IsNullOrEmpty(templateCell.InputValue))
				{
					ticketInputCellVM.IsMixTicketExistDesignInputValue = true;
					ticketInputCellVM.MixTicketExistDesignInputValueShouldDisplayValue = templateCell.InputValue;
					ticketInputCellVM.Value = ticketInputCellVM.MixTicketExistDesignInputValueShouldDisplayValue;
				}
			}
		}
		if (templateCell.HasFormula())
		{
			ticketInputCellVM.IsFormula = true;
			ticketInputCellVM.Formula = templateCell.Formula;
			ticketInputCellVM.IsFormulaFromTicket = true;
		}
		if (ticketInputCellVM.TempCell == null)
		{
			ticketInputCellVM.TempCell = new Auditai.Model.Cell
			{
				Column = ticketInputCellVM.Column,
				Row = DummyMR,
				Value = ""
			};
		}
		return ticketInputCellVM;
	}

	protected void InsertDynamicDataRowsForMixTicket(int index, int count, int templateId)
	{
		MixTicketFixTickRowIndexMapperToVMRowIndexDic = null;
		TicketTableMixRangeTemplateRow templateRow = _ticket.FixedAndDynamicMixRange.DynamicDataRowTemplateRows.Find((TicketTableMixRangeTemplateRow u) => u.TemplateId == templateId);
		TicketRow ticketRow = _ticket.Rows[templateRow.RefTicketTableRowIndex];
		List<TicketInputRowVM> list = (from i in Enumerable.Range(0, count)
			select new TicketInputRowVM
			{
				TempRow = new Auditai.Model.Row
				{
					Creator = Auditai.Model.User.Current.Id
				},
				IsNew = true,
				TicketRow = ticketRow,
				IsMixTicketDynamicDataRow = true,
				MixTicketDynamicDataRowTemplate = templateRow
			}).ToList();
		_rows.InsertRange(index, list);
		int count2 = _ticket.Columns.Count;
		List<TicketInputCellVM> list2 = new List<TicketInputCellVM>();
		for (int j = 0; j < count; j++)
		{
			for (int k = 0; k < count2; k++)
			{
				list2.Add(BuildCellVMForNewInsertRowForMixTicket(list[j], _ticket.Columns[k], _ticket.GetCell(templateRow.RefTicketTableRowIndex, k)));
			}
		}
		_cells.InsertRange(index * count2, list2);
		PopulateMerges();
	}

	protected TicketInputRowVM InsertDynamicDataRowForMixTicket(int index, int templateId)
	{
		MixTicketFixTickRowIndexMapperToVMRowIndexDic = null;
		TicketTableMixRangeTemplateRow ticketTableMixRangeTemplateRow = _ticket.FixedAndDynamicMixRange.DynamicDataRowTemplateRows.Find((TicketTableMixRangeTemplateRow u) => u.TemplateId == templateId);
		TicketRow ticketRow = _ticket.Rows[ticketTableMixRangeTemplateRow.RefTicketTableRowIndex];
		TicketInputRowVM ticketInputRowVM = new TicketInputRowVM
		{
			TempRow = new Auditai.Model.Row
			{
				Creator = Auditai.Model.User.Current.Id
			},
			IsNew = true,
			TicketRow = ticketRow,
			IsMixTicketDynamicDataRow = true,
			MixTicketDynamicDataRowTemplate = ticketTableMixRangeTemplateRow
		};
		_rows.Insert(index, ticketInputRowVM);
		List<TicketInputCellVM> list = new List<TicketInputCellVM>();
		for (int i = 0; i < _ticket.Columns.Count; i++)
		{
			list.Add(BuildCellVMForNewInsertRowForMixTicket(ticketInputRowVM, _ticket.Columns[i], _ticket.GetCell(ticketTableMixRangeTemplateRow.RefTicketTableRowIndex, i)));
		}
		_cells.InsertRange(index * _columns.Count, list);
		PopulateMerges();
		return ticketInputRowVM;
	}

	public TicketInputRowVM GetRow(int index)
	{
		return _rows[index];
	}

	public TicketInputColumnVM GetColumn(int index)
	{
		return _columns[index];
	}

	public int MoveUpDataRow_DynamicRowTicket(int vmStartRowIndex, int rowsCount, out int afterMoveFistRowIndex)
	{
		afterMoveFistRowIndex = vmStartRowIndex;
		if (_ticket.Kind != TicketKind.DynamicRow)
		{
			return 0;
		}
		int num = rowsCount;
		int num2 = vmStartRowIndex;
		for (int i = 0; i < rowsCount; i++)
		{
			if (IsRowIndexOutOfRange(num2))
			{
				return 0;
			}
			if (IsRecordDataRow_DynamicRowTicket(num2))
			{
				break;
			}
			num2++;
			num--;
		}
		vmStartRowIndex = num2;
		rowsCount = num;
		if (IsRowIndexOutOfRange(vmStartRowIndex))
		{
			return 0;
		}
		if (rowsCount <= 0)
		{
			return 0;
		}
		if (!IsRecordDataRow_DynamicRowTicket(vmStartRowIndex))
		{
			return 0;
		}
		if (vmStartRowIndex <= _ticket.DataRowStart)
		{
			return 0;
		}
		if (vmStartRowIndex + rowsCount >= _ticket.DataRowStart + DataRowsCount)
		{
			rowsCount = _ticket.DataRowStart + DataRowsCount - vmStartRowIndex;
		}
		if (rowsCount <= 0)
		{
			return 0;
		}
		int num3 = vmStartRowIndex - 1;
		int num4 = vmStartRowIndex + rowsCount;
		Auditai.Model.Row row = null;
		Auditai.Model.Row row2 = null;
		bool flag = false;
		for (int num5 = num4 - 1; num5 > num3; num5--)
		{
			Auditai.Model.Row tableRow = _rows[num5].TableRow;
			if (tableRow != null)
			{
				row2 = tableRow;
				break;
			}
		}
		if (row2 != null)
		{
			if (row2.Index < Table.Rows.Count - 1)
			{
				row2 = Table.Rows[row2.Index + 1];
				flag = false;
			}
			else
			{
				row2 = null;
				flag = true;
			}
		}
		InsertDataRows_DynamicRowTicket(num4, 1);
		TicketInputRowVM ticketInputRowVM = _rows[num3];
		TicketInputRowVM value = _rows[num4];
		_rows[num3] = value;
		_rows[num4] = ticketInputRowVM;
		row = ticketInputRowVM.TableRow;
		int count = _columns.Count;
		for (int j = 0; j < count; j++)
		{
			int index = num3 * count + j;
			int index2 = num4 * count + j;
			TicketInputCellVM value2 = _cells[index];
			TicketInputCellVM value3 = _cells[index2];
			_cells[index] = value3;
			_cells[index2] = value2;
		}
		if (row != null)
		{
			if (flag)
			{
				Table.Rows.Move(row.Index, 1, Table.Rows.Count);
			}
			else if (row2 != null)
			{
				Table.Rows.Move(row.Index, 1, row2.Index);
			}
		}
		RemoveDataRows(num3, 1);
		afterMoveFistRowIndex = vmStartRowIndex - 1;
		return rowsCount;
	}

	public int MoveDownDataRow_DynamicRowTicket(int vmStartRowIndex, int rowsCount, out int afterMoveFistRowIndex)
	{
		afterMoveFistRowIndex = vmStartRowIndex;
		if (_ticket.Kind != TicketKind.DynamicRow)
		{
			return 0;
		}
		int num = rowsCount;
		int num2 = vmStartRowIndex;
		for (int i = 0; i < rowsCount; i++)
		{
			if (IsRowIndexOutOfRange(num2))
			{
				return 0;
			}
			if (IsRecordDataRow_DynamicRowTicket(num2))
			{
				break;
			}
			num2++;
			num--;
		}
		vmStartRowIndex = num2;
		rowsCount = num;
		if (IsRowIndexOutOfRange(vmStartRowIndex))
		{
			return 0;
		}
		if (rowsCount <= 0)
		{
			return 0;
		}
		int num3 = vmStartRowIndex + rowsCount - 1;
		if (IsRowIndexOutOfRange(num3))
		{
			return 0;
		}
		if (!IsRecordDataRow_DynamicRowTicket(num3))
		{
			return 0;
		}
		if (num3 >= _ticket.DataRowStart + DataRowsCount - 1)
		{
			return 0;
		}
		if (vmStartRowIndex < _ticket.DataRowStart)
		{
			vmStartRowIndex = _ticket.DataRowStart;
			rowsCount = num3 - _ticket.DataRowStart + 1;
		}
		if (rowsCount <= 0)
		{
			return 0;
		}
		int num4 = num3 + 1;
		int num5 = vmStartRowIndex;
		Auditai.Model.Row row = null;
		Auditai.Model.Row row2 = null;
		for (int j = vmStartRowIndex; j <= num3; j++)
		{
			Auditai.Model.Row tableRow = _rows[j].TableRow;
			if (tableRow != null)
			{
				row2 = tableRow;
				break;
			}
		}
		InsertDataRows_DynamicRowTicket(vmStartRowIndex, 1);
		num4++;
		TicketInputRowVM ticketInputRowVM = _rows[num4];
		TicketInputRowVM value = _rows[num5];
		_rows[num4] = value;
		_rows[num5] = ticketInputRowVM;
		row = ticketInputRowVM.TableRow;
		int count = _columns.Count;
		for (int k = 0; k < count; k++)
		{
			int index = num4 * count + k;
			int index2 = num5 * count + k;
			TicketInputCellVM value2 = _cells[index];
			TicketInputCellVM value3 = _cells[index2];
			_cells[index] = value3;
			_cells[index2] = value2;
		}
		if (row != null && row2 != null)
		{
			Table.Rows.Move(row.Index, 1, row2.Index);
		}
		RemoveDataRows(num4, 1);
		afterMoveFistRowIndex = vmStartRowIndex + 1;
		return rowsCount;
	}

	public int MoveDataRowToTop_DynamicRowTicket(int vmStartRowIndex, int rowsCount, out int afterMoveFistRowIndex)
	{
		afterMoveFistRowIndex = vmStartRowIndex;
		if (_ticket.Kind != TicketKind.DynamicRow)
		{
			return 0;
		}
		int num = rowsCount;
		int num2 = vmStartRowIndex;
		for (int i = 0; i < rowsCount; i++)
		{
			if (IsRowIndexOutOfRange(num2))
			{
				return 0;
			}
			if (IsRecordDataRow_DynamicRowTicket(num2))
			{
				break;
			}
			num2++;
			num--;
		}
		vmStartRowIndex = num2;
		rowsCount = num;
		if (IsRowIndexOutOfRange(vmStartRowIndex))
		{
			return 0;
		}
		if (rowsCount <= 0)
		{
			return 0;
		}
		if (!IsRecordDataRow_DynamicRowTicket(vmStartRowIndex))
		{
			return 0;
		}
		if (vmStartRowIndex <= _ticket.DataRowStart)
		{
			return 0;
		}
		if (vmStartRowIndex + rowsCount >= _ticket.DataRowStart + DataRowsCount)
		{
			rowsCount = _ticket.DataRowStart + DataRowsCount - vmStartRowIndex;
		}
		if (rowsCount <= 0)
		{
			return 0;
		}
		int num3 = vmStartRowIndex;
		Auditai.Model.Row row = null;
		for (int j = _ticket.DataRowStart; j < DataRowsCount; j++)
		{
			Auditai.Model.Row tableRow = _rows[j].TableRow;
			if (tableRow != null)
			{
				row = tableRow;
				break;
			}
		}
		List<Auditai.Model.Row> list = new List<Auditai.Model.Row>();
		for (int k = 0; k < rowsCount; k++)
		{
			Auditai.Model.Row tableRow2 = _rows[num3 + k].TableRow;
			if (tableRow2 != null)
			{
				list.Add(tableRow2);
			}
		}
		InsertDataRows_DynamicRowTicket(_ticket.DataRowStart, rowsCount);
		num3 += rowsCount;
		for (int l = 0; l < rowsCount; l++)
		{
			int index = num3 + l;
			int index2 = _ticket.DataRowStart + l;
			TicketInputRowVM value = _rows[index];
			TicketInputRowVM value2 = _rows[index2];
			_rows[index] = value2;
			_rows[index2] = value;
		}
		int count = _columns.Count;
		for (int m = 0; m < rowsCount; m++)
		{
			int num4 = num3 + m;
			int num5 = _ticket.DataRowStart + m;
			for (int n = 0; n < count; n++)
			{
				int index3 = num4 * count + n;
				int index4 = num5 * count + n;
				TicketInputCellVM value3 = _cells[index3];
				TicketInputCellVM value4 = _cells[index4];
				_cells[index3] = value4;
				_cells[index4] = value3;
			}
		}
		if (row != null)
		{
			for (int num6 = 0; num6 < list.Count; num6++)
			{
				Auditai.Model.Row row2 = list[num6];
				if (row2 != row)
				{
					Table.Rows.Move(row2.Index, 1, row.Index);
				}
			}
		}
		RemoveDataRows(num3, rowsCount);
		afterMoveFistRowIndex = _ticket.DataRowStart;
		return rowsCount;
	}

	public int MoveDataRowToBottom_DynamicRowTicket(int vmStartRowIndex, int rowsCount, out int afterMoveFistRowIndex)
	{
		afterMoveFistRowIndex = vmStartRowIndex;
		if (_ticket.Kind != TicketKind.DynamicRow)
		{
			return 0;
		}
		int num = rowsCount;
		int num2 = vmStartRowIndex;
		for (int i = 0; i < rowsCount; i++)
		{
			if (IsRowIndexOutOfRange(num2))
			{
				return 0;
			}
			if (IsRecordDataRow_DynamicRowTicket(num2))
			{
				break;
			}
			num2++;
			num--;
		}
		vmStartRowIndex = num2;
		rowsCount = num;
		if (IsRowIndexOutOfRange(vmStartRowIndex))
		{
			return 0;
		}
		if (rowsCount <= 0)
		{
			return 0;
		}
		if (!IsRecordDataRow_DynamicRowTicket(vmStartRowIndex))
		{
			return 0;
		}
		int num3 = vmStartRowIndex + rowsCount - 1;
		if (num3 >= _ticket.DataRowStart + DataRowsCount - 1)
		{
			return 0;
		}
		if (vmStartRowIndex < _ticket.DataRowStart)
		{
			vmStartRowIndex = _ticket.DataRowStart;
			rowsCount = num3 - vmStartRowIndex + 1;
		}
		if (rowsCount <= 0)
		{
			return 0;
		}
		int num4 = vmStartRowIndex;
		Auditai.Model.Row row = null;
		int num5 = _ticket.DataRowStart + DataRowsCount;
		bool flag = false;
		for (int num6 = _ticket.DataRowStart + DataRowsCount - 1; num6 >= _ticket.DataRowStart; num6--)
		{
			Auditai.Model.Row tableRow = _rows[num6].TableRow;
			if (tableRow != null)
			{
				row = tableRow;
				break;
			}
		}
		if (row == null)
		{
			flag = true;
		}
		else if (row.Index >= Table.Rows.Count - 1)
		{
			flag = true;
			row = null;
		}
		else
		{
			row = Table.Rows[row.Index + 1];
		}
		List<Auditai.Model.Row> list = new List<Auditai.Model.Row>();
		for (int j = 0; j < rowsCount; j++)
		{
			Auditai.Model.Row tableRow2 = _rows[num4 + j].TableRow;
			if (tableRow2 != null)
			{
				list.Add(tableRow2);
			}
		}
		InsertDataRows_DynamicRowTicket(_ticket.DataRowStart + DataRowsCount, rowsCount);
		for (int k = 0; k < rowsCount; k++)
		{
			int index = num4 + k;
			int index2 = num5 + k;
			TicketInputRowVM value = _rows[index];
			TicketInputRowVM value2 = _rows[index2];
			_rows[index] = value2;
			_rows[index2] = value;
		}
		int count = _columns.Count;
		for (int l = 0; l < rowsCount; l++)
		{
			int num7 = num4 + l;
			int num8 = num5 + l;
			for (int m = 0; m < count; m++)
			{
				int index3 = num7 * count + m;
				int index4 = num8 * count + m;
				TicketInputCellVM value3 = _cells[index3];
				TicketInputCellVM value4 = _cells[index4];
				_cells[index3] = value4;
				_cells[index4] = value3;
			}
		}
		if (flag)
		{
			for (int n = 0; n < list.Count; n++)
			{
				Auditai.Model.Row row2 = list[n];
				if (row2 != row)
				{
					Table.Rows.Move(row2.Index, 1, Table.Rows.Count);
				}
			}
		}
		else if (row != null)
		{
			for (int num9 = 0; num9 < list.Count; num9++)
			{
				Auditai.Model.Row row3 = list[num9];
				if (row3 != row)
				{
					Table.Rows.Move(row3.Index, 1, row.Index);
				}
			}
		}
		RemoveDataRows(num4, rowsCount);
		afterMoveFistRowIndex = _ticket.DataRowStart + DataRowsCount - rowsCount;
		return rowsCount;
	}

	public int MoveUpDataRow_MixTicket(int vmStartRowIndex, int rowsCount, out int afterMoveFistRowIndex)
	{
		afterMoveFistRowIndex = vmStartRowIndex;
		if (_ticket.Kind != TicketKind.FixedDataRowMixDynamicDataRow)
		{
			return 0;
		}
		int num = rowsCount;
		int num2 = vmStartRowIndex;
		for (int i = 0; i < rowsCount; i++)
		{
			if (IsRowIndexOutOfRange(num2))
			{
				return 0;
			}
			if (_rows[num2].IsMixTicketDynamicDataRow)
			{
				break;
			}
			num2++;
			num--;
		}
		vmStartRowIndex = num2;
		rowsCount = num;
		if (IsRowIndexOutOfRange(vmStartRowIndex))
		{
			return 0;
		}
		if (rowsCount <= 0)
		{
			return 0;
		}
		if (!_rows[vmStartRowIndex].IsMixTicketDynamicDataRow)
		{
			return 0;
		}
		int num3 = 0;
		int vmStartRowIndex2 = 0;
		int vmEndRowIndex = 0;
		num3 = GetMixTicketDynamicDataRowsRange(vmStartRowIndex, out vmStartRowIndex2, out vmEndRowIndex);
		if (num3 <= 0)
		{
			return 0;
		}
		if (vmStartRowIndex <= vmStartRowIndex2)
		{
			return 0;
		}
		if (vmStartRowIndex + rowsCount >= vmEndRowIndex + 1)
		{
			rowsCount = vmEndRowIndex - vmStartRowIndex + 1;
		}
		if (rowsCount <= 0)
		{
			return 0;
		}
		int num4 = vmStartRowIndex - 1;
		int num5 = vmStartRowIndex + rowsCount;
		Auditai.Model.Row row = null;
		Auditai.Model.Row row2 = null;
		bool flag = false;
		for (int num6 = num5 - 1; num6 > num4; num6--)
		{
			Auditai.Model.Row tableRow = _rows[num6].TableRow;
			if (tableRow != null)
			{
				row2 = tableRow;
				break;
			}
		}
		if (row2 != null)
		{
			if (row2.Index < Table.Rows.Count - 1)
			{
				row2 = Table.Rows[row2.Index + 1];
				flag = false;
			}
			else
			{
				row2 = null;
				flag = true;
			}
		}
		InsertDataRows_MixTicket(num5, 1, _rows[vmStartRowIndex2].MixTicketDynamicDataRowTemplate.TemplateId);
		TicketInputRowVM ticketInputRowVM = _rows[num4];
		TicketInputRowVM value = _rows[num5];
		_rows[num4] = value;
		_rows[num5] = ticketInputRowVM;
		row = ticketInputRowVM.TableRow;
		int count = _columns.Count;
		for (int j = 0; j < count; j++)
		{
			int index = num4 * count + j;
			int index2 = num5 * count + j;
			TicketInputCellVM value2 = _cells[index];
			TicketInputCellVM value3 = _cells[index2];
			_cells[index] = value3;
			_cells[index2] = value2;
		}
		if (row != null)
		{
			if (flag)
			{
				Table.Rows.Move(row.Index, 1, Table.Rows.Count);
			}
			else if (row2 != null)
			{
				Table.Rows.Move(row.Index, 1, row2.Index);
			}
		}
		RemoveDataRows(num4, 1);
		afterMoveFistRowIndex = vmStartRowIndex - 1;
		return rowsCount;
	}

	public int MoveDownDataRow_MixTicket(int vmStartRowIndex, int rowsCount, out int afterMoveFistRowIndex)
	{
		afterMoveFistRowIndex = vmStartRowIndex;
		if (_ticket.Kind != TicketKind.FixedDataRowMixDynamicDataRow)
		{
			return 0;
		}
		int num = rowsCount;
		int num2 = vmStartRowIndex;
		for (int i = 0; i < rowsCount; i++)
		{
			if (IsRowIndexOutOfRange(num2))
			{
				return 0;
			}
			if (_rows[num2].IsMixTicketDynamicDataRow)
			{
				break;
			}
			num2++;
			num--;
		}
		vmStartRowIndex = num2;
		rowsCount = num;
		if (IsRowIndexOutOfRange(vmStartRowIndex))
		{
			return 0;
		}
		if (rowsCount <= 0)
		{
			return 0;
		}
		int num3 = vmStartRowIndex + rowsCount - 1;
		if (IsRowIndexOutOfRange(num3))
		{
			return 0;
		}
		if (!_rows[num3].IsMixTicketDynamicDataRow)
		{
			return 0;
		}
		int num4 = 0;
		int vmStartRowIndex2 = 0;
		int vmEndRowIndex = 0;
		num4 = GetMixTicketDynamicDataRowsRange(vmStartRowIndex, out vmStartRowIndex2, out vmEndRowIndex);
		if (num4 <= 0)
		{
			return 0;
		}
		if (num3 >= vmEndRowIndex)
		{
			return 0;
		}
		if (vmStartRowIndex < vmStartRowIndex2)
		{
			vmStartRowIndex = vmStartRowIndex2;
			rowsCount = num3 - vmStartRowIndex2 + 1;
		}
		if (rowsCount <= 0)
		{
			return 0;
		}
		int num5 = num3 + 1;
		int num6 = vmStartRowIndex;
		Auditai.Model.Row row = null;
		Auditai.Model.Row row2 = null;
		for (int j = vmStartRowIndex; j <= num3; j++)
		{
			Auditai.Model.Row tableRow = _rows[j].TableRow;
			if (tableRow != null)
			{
				row2 = tableRow;
				break;
			}
		}
		InsertDataRows_MixTicket(vmStartRowIndex, 1, _rows[num3].MixTicketDynamicDataRowTemplate.TemplateId);
		num5++;
		TicketInputRowVM ticketInputRowVM = _rows[num5];
		TicketInputRowVM value = _rows[num6];
		_rows[num5] = value;
		_rows[num6] = ticketInputRowVM;
		row = ticketInputRowVM.TableRow;
		int count = _columns.Count;
		for (int k = 0; k < count; k++)
		{
			int index = num5 * count + k;
			int index2 = num6 * count + k;
			TicketInputCellVM value2 = _cells[index];
			TicketInputCellVM value3 = _cells[index2];
			_cells[index] = value3;
			_cells[index2] = value2;
		}
		if (row != null && row2 != null)
		{
			Table.Rows.Move(row.Index, 1, row2.Index);
		}
		RemoveDataRows(num5, 1);
		afterMoveFistRowIndex = vmStartRowIndex + 1;
		return rowsCount;
	}

	public int MoveDataRowToTop_MixTicket(int vmStartRowIndex, int rowsCount, out int afterMoveFistRowIndex)
	{
		afterMoveFistRowIndex = vmStartRowIndex;
		if (_ticket.Kind != TicketKind.FixedDataRowMixDynamicDataRow)
		{
			return 0;
		}
		int num = rowsCount;
		int num2 = vmStartRowIndex;
		for (int i = 0; i < rowsCount; i++)
		{
			if (IsRowIndexOutOfRange(num2))
			{
				return 0;
			}
			if (_rows[num2].IsMixTicketDynamicDataRow)
			{
				break;
			}
			num2++;
			num--;
		}
		vmStartRowIndex = num2;
		rowsCount = num;
		if (IsRowIndexOutOfRange(vmStartRowIndex))
		{
			return 0;
		}
		if (rowsCount <= 0)
		{
			return 0;
		}
		if (IsRowIndexOutOfRange(vmStartRowIndex))
		{
			return 0;
		}
		if (!_rows[vmStartRowIndex].IsMixTicketDynamicDataRow)
		{
			return 0;
		}
		int num3 = 0;
		int vmStartRowIndex2 = 0;
		int vmEndRowIndex = 0;
		num3 = GetMixTicketDynamicDataRowsRange(vmStartRowIndex, out vmStartRowIndex2, out vmEndRowIndex);
		if (num3 <= 0)
		{
			return 0;
		}
		if (vmStartRowIndex <= vmStartRowIndex2)
		{
			return 0;
		}
		if (vmStartRowIndex + rowsCount > vmEndRowIndex + 1)
		{
			rowsCount = vmEndRowIndex - vmStartRowIndex + 1;
		}
		if (rowsCount <= 0)
		{
			return 0;
		}
		int num4 = vmStartRowIndex;
		Auditai.Model.Row row = null;
		for (int j = vmStartRowIndex2; j <= vmEndRowIndex; j++)
		{
			Auditai.Model.Row tableRow = _rows[j].TableRow;
			if (tableRow != null)
			{
				row = tableRow;
				break;
			}
		}
		List<Auditai.Model.Row> list = new List<Auditai.Model.Row>();
		for (int k = 0; k < rowsCount; k++)
		{
			Auditai.Model.Row tableRow2 = _rows[num4 + k].TableRow;
			if (tableRow2 != null)
			{
				list.Add(tableRow2);
			}
		}
		InsertDataRows_MixTicket(vmStartRowIndex2, rowsCount, _rows[vmStartRowIndex2].MixTicketDynamicDataRowTemplate.TemplateId);
		num4 += rowsCount;
		for (int l = 0; l < rowsCount; l++)
		{
			int index = num4 + l;
			int index2 = vmStartRowIndex2 + l;
			TicketInputRowVM value = _rows[index];
			TicketInputRowVM value2 = _rows[index2];
			_rows[index] = value2;
			_rows[index2] = value;
		}
		int count = _columns.Count;
		for (int m = 0; m < rowsCount; m++)
		{
			int num5 = num4 + m;
			int num6 = vmStartRowIndex2 + m;
			for (int n = 0; n < count; n++)
			{
				int index3 = num5 * count + n;
				int index4 = num6 * count + n;
				TicketInputCellVM value3 = _cells[index3];
				TicketInputCellVM value4 = _cells[index4];
				_cells[index3] = value4;
				_cells[index4] = value3;
			}
		}
		if (row != null)
		{
			for (int num7 = 0; num7 < list.Count; num7++)
			{
				Auditai.Model.Row row2 = list[num7];
				if (row2 != row)
				{
					Table.Rows.Move(row2.Index, 1, row.Index);
				}
			}
		}
		RemoveDataRows(num4, rowsCount);
		afterMoveFistRowIndex = vmStartRowIndex2;
		return rowsCount;
	}

	public int MoveDataRowToBottom_MixTicket(int vmStartRowIndex, int rowsCount, out int afterMoveFistRowIndex)
	{
		afterMoveFistRowIndex = vmStartRowIndex;
		if (_ticket.Kind != TicketKind.FixedDataRowMixDynamicDataRow)
		{
			return 0;
		}
		int num = rowsCount;
		int num2 = vmStartRowIndex;
		for (int i = 0; i < rowsCount; i++)
		{
			if (IsRowIndexOutOfRange(num2))
			{
				return 0;
			}
			if (_rows[num2].IsMixTicketDynamicDataRow)
			{
				break;
			}
			num2++;
			num--;
		}
		vmStartRowIndex = num2;
		rowsCount = num;
		if (IsRowIndexOutOfRange(vmStartRowIndex))
		{
			return 0;
		}
		if (rowsCount <= 0)
		{
			return 0;
		}
		if (IsRowIndexOutOfRange(vmStartRowIndex))
		{
			return 0;
		}
		if (!_rows[vmStartRowIndex].IsMixTicketDynamicDataRow)
		{
			return 0;
		}
		int num3 = 0;
		int vmStartRowIndex2 = 0;
		int vmEndRowIndex = 0;
		num3 = GetMixTicketDynamicDataRowsRange(vmStartRowIndex, out vmStartRowIndex2, out vmEndRowIndex);
		if (num3 <= 0)
		{
			return 0;
		}
		int num4 = vmStartRowIndex + rowsCount - 1;
		if (num4 >= vmEndRowIndex)
		{
			return 0;
		}
		if (vmStartRowIndex < vmStartRowIndex2)
		{
			vmStartRowIndex = vmStartRowIndex2;
			rowsCount = num4 - vmStartRowIndex + 1;
		}
		if (rowsCount <= 0)
		{
			return 0;
		}
		int num5 = vmStartRowIndex;
		Auditai.Model.Row row = null;
		int num6 = vmEndRowIndex + 1;
		bool flag = false;
		for (int num7 = vmStartRowIndex2; num7 >= vmEndRowIndex; num7--)
		{
			Auditai.Model.Row tableRow = _rows[num7].TableRow;
			if (tableRow != null)
			{
				row = tableRow;
				break;
			}
		}
		if (row == null)
		{
			flag = true;
		}
		else if (row.Index >= Table.Rows.Count - 1)
		{
			flag = true;
			row = null;
		}
		else
		{
			row = Table.Rows[row.Index + 1];
		}
		List<Auditai.Model.Row> list = new List<Auditai.Model.Row>();
		for (int j = 0; j < rowsCount; j++)
		{
			Auditai.Model.Row tableRow2 = _rows[num5 + j].TableRow;
			if (tableRow2 != null)
			{
				list.Add(tableRow2);
			}
		}
		InsertDataRows_DynamicRowTicket(vmEndRowIndex + 1, rowsCount);
		for (int k = 0; k < rowsCount; k++)
		{
			int index = num5 + k;
			int index2 = num6 + k;
			TicketInputRowVM value = _rows[index];
			TicketInputRowVM value2 = _rows[index2];
			_rows[index] = value2;
			_rows[index2] = value;
		}
		int count = _columns.Count;
		for (int l = 0; l < rowsCount; l++)
		{
			int num8 = num5 + l;
			int num9 = num6 + l;
			for (int m = 0; m < count; m++)
			{
				int index3 = num8 * count + m;
				int index4 = num9 * count + m;
				TicketInputCellVM value3 = _cells[index3];
				TicketInputCellVM value4 = _cells[index4];
				_cells[index3] = value4;
				_cells[index4] = value3;
			}
		}
		if (flag)
		{
			for (int n = 0; n < list.Count; n++)
			{
				Auditai.Model.Row row2 = list[n];
				if (row2 != row)
				{
					Table.Rows.Move(row2.Index, 1, Table.Rows.Count);
				}
			}
		}
		else if (row != null)
		{
			for (int num10 = 0; num10 < list.Count; num10++)
			{
				Auditai.Model.Row row3 = list[num10];
				if (row3 != row)
				{
					Table.Rows.Move(row3.Index, 1, row.Index);
				}
			}
		}
		RemoveDataRows(num5, rowsCount);
		afterMoveFistRowIndex = vmEndRowIndex - rowsCount + 1;
		return rowsCount;
	}

	protected int GetMixTicketDynamicDataRowsRange(int dynamicRowIndex, out int vmStartRowIndex, out int vmEndRowIndex)
	{
		int result = 0;
		vmStartRowIndex = 0;
		vmEndRowIndex = 0;
		if (_ticket.Kind != TicketKind.FixedDataRowMixDynamicDataRow || IsRowIndexOutOfRange(dynamicRowIndex) || !_rows[dynamicRowIndex].IsMixTicketDynamicDataRow)
		{
			return result;
		}
		TicketTableMixRangeTemplateRow mixTicketDynamicDataRowTemplate = _rows[dynamicRowIndex].MixTicketDynamicDataRowTemplate;
		vmStartRowIndex = dynamicRowIndex;
		vmEndRowIndex = dynamicRowIndex;
		int num = dynamicRowIndex - 1;
		while (num >= 0 && _rows[num].IsMixTicketDynamicDataRow && _rows[num].MixTicketDynamicDataRowTemplate == mixTicketDynamicDataRowTemplate)
		{
			vmStartRowIndex = num;
			num--;
		}
		for (int i = dynamicRowIndex + 1; i < _rows.Count && _rows[i].IsMixTicketDynamicDataRow && _rows[i].MixTicketDynamicDataRowTemplate == mixTicketDynamicDataRowTemplate; i++)
		{
			vmEndRowIndex = i;
		}
		return vmEndRowIndex - vmStartRowIndex + 1;
	}

	protected int VMIndexToModel_ExcludeMixTicket(int index)
	{
		if (_ticket.DataRowStart == -1)
		{
			return index;
		}
		if (index < _ticket.DataRowStart)
		{
			return index;
		}
		if (index < _ticket.DataRowStart + DataRowsCount)
		{
			return index - _ticket.DataRowStart;
		}
		return index - DataRowsCount;
	}

	public int GetRowsCount()
	{
		return _rows.Count;
	}

	public int GetColumnsCount()
	{
		return _columns.Count;
	}

	public int GetCellsCount()
	{
		return _cells.Count;
	}

	public TicketBorder GetCellTopBorder(TicketInputCellVM cell, int rowIndex, int colIndex)
	{
		if (_ticket.Kind == TicketKind.FixedDataRowMixDynamicDataRow)
		{
			if (!cell.IsMixTicketDynamicDataRow)
			{
				return cell.TicketCell.Top;
			}
			if (rowIndex == 0)
			{
				return cell.TicketCell.Top;
			}
			TicketInputRowVM ticketInputRowVM = _rows[rowIndex - 1];
			if (!ticketInputRowVM.TicketRow.IsMixRangeDynamicDataRow)
			{
				return cell.TicketCell.Top;
			}
			TicketInputRowVM ticketInputRowVM2 = _rows[rowIndex];
			if (ticketInputRowVM2.TicketRow.MixRangeDynamicDataRowTemplateId != ticketInputRowVM.TicketRow.MixRangeDynamicDataRowTemplateId)
			{
				return cell.TicketCell.Top;
			}
			return _ticket.GetCell(ticketInputRowVM2.MixTicketDynamicDataRowTemplate.BottomBorderRefTicketTableRowIndex, colIndex).Top;
		}
		if (_ticket.Kind == TicketKind.DynamicRow)
		{
			return cell.GetTop(rowIndex == _ticket.DataRowStart);
		}
		return cell.GetTop(isFirstDataRow: false);
	}

	public TicketBorder GetCellBottomBorder(TicketInputCellVM cell, int rowIndex, int colIndex)
	{
		if (_ticket.Kind == TicketKind.FixedDataRowMixDynamicDataRow)
		{
			if (!cell.IsMixTicketDynamicDataRow)
			{
				return cell.TicketCell.Bottom;
			}
			if (rowIndex + 1 >= _rows.Count)
			{
				return cell.TicketCell.Bottom;
			}
			TicketInputRowVM ticketInputRowVM = _rows[rowIndex + 1];
			TicketInputRowVM ticketInputRowVM2 = _rows[rowIndex];
			if (!ticketInputRowVM.TicketRow.IsMixRangeDynamicDataRow)
			{
				return _ticket.GetCell(ticketInputRowVM2.MixTicketDynamicDataRowTemplate.BottomBorderRefTicketTableRowIndex, colIndex).Bottom;
			}
			if (ticketInputRowVM2.TicketRow.MixRangeDynamicDataRowTemplateId == ticketInputRowVM.TicketRow.MixRangeDynamicDataRowTemplateId)
			{
				return cell.TicketCell.Bottom;
			}
			return _ticket.GetCell(ticketInputRowVM2.MixTicketDynamicDataRowTemplate.BottomBorderRefTicketTableRowIndex, colIndex).Bottom;
		}
		if (_ticket.Kind == TicketKind.DynamicRow)
		{
			return cell.GetBottom(rowIndex == _ticket.DataRowStart + DataRowsCount - 1);
		}
		return cell.GetBottom(isLastDataRow: false);
	}

	public int GetAllRowsTotalHeight()
	{
		if (_rows == null)
		{
			return 0;
		}
		int count = _rows.Count;
		int num = 0;
		for (int i = 0; i < count; i++)
		{
			num += GetRowHeight(i);
		}
		return num;
	}

	public TicketInputCellVM GetCellVM(int row, int col)
	{
		return _cells[row * _columns.Count + col];
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

	public HashSet<Auditai.Model.Column> GetNavColumns()
	{
		HashSet<Auditai.Model.Column> hashSet = new HashSet<Auditai.Model.Column>();
		foreach (TicketInputCellVM cell in _cells)
		{
			if (!cell.IsDynamicTicketDataRow && cell.IsField && cell.Column != null)
			{
				hashSet.Add(cell.Column);
			}
		}
		return hashSet;
	}

	public CellsOperand GetCellsOperand(Id64 columnId)
	{
		return new CellsOperand((from c in _cells
			where c.Column != null && c.Column.Id == columnId
			select c.TempCell).ToList(), Table);
	}

	private FormulaEvaluationEnvironment GetEvalEnv(int row, TicketInputCellVM cell)
	{
		FormulaReferenceModelResolver resolver = new FormulaReferenceModelResolver(Table.Project);
		FormulaEvaluationEnvironment formulaEvaluationEnvironment = new FormulaEvaluationEnvironment
		{
			Resolver = resolver,
			HostTable = Table,
			RefManager = Table.Project.DataReferenceManager,
			IsInTicketEnvironment = true,
			IsFormulaComeFromTable = !cell.IsFormulaFromTicket,
			TicketCellRefTableRow = cell.TableRow,
			CurrentUserId = Auditai.Model.User.Current.Id,
			RefEvalContext = new DataReferenceEvaluationContext
			{
				Project = Table.Project,
				CurrentTreeNode = Table.TreeNode
			}
		};
		if (cell.IsDynamicTicketDataRow)
		{
			formulaEvaluationEnvironment.TicketDataRowIndex = row - _ticket.DataRowStart;
		}
		else if (cell.IsFixedMultiRowValue)
		{
			formulaEvaluationEnvironment.TicketDataRowIndex = FixedMultiRowVMs.FindIndex((TicketFixedMultiRowVM r) => r.ValueCells.Contains(cell));
		}
		else if (cell.IsMixTicketDynamicDataRow)
		{
			formulaEvaluationEnvironment.TicketDataRowIndex = GetMixRangeDynamicDataRowIndex(row);
		}
		if (_record == null)
		{
			formulaEvaluationEnvironment.RowIndex = -1;
		}
		else if (_ticket.Kind == TicketKind.FixedMultiRow || _ticket.Kind == TicketKind.FixedDataRowMixDynamicDataRow)
		{
			if (cell.TempCell == null)
			{
				formulaEvaluationEnvironment.RowIndex = _record.Rows[0].Index;
			}
			else
			{
				formulaEvaluationEnvironment.RowIndex = cell.TempCell.Row.Index;
			}
		}
		else
		{
			formulaEvaluationEnvironment.RowIndex = _record.Rows[0].Index;
		}
		formulaEvaluationEnvironment.TicketInputDataResolver = new TicketInputDataResolver(this);
		return formulaEvaluationEnvironment;
	}

	public void CalculateLeaf(TicketCellReference c)
	{
		_fm.CalculateLeaf(c);
	}

	public void PrintTicketCellValue(string msg)
	{
	}

	private void FindOutTicketFormulaShouldUpdateTableCell(TicketInputCellVM ticketCell, Dictionary<Auditai.Model.Cell, TicketInputCellVM> tableNeedUpdateCellDic)
	{
		if (ticketCell.IsFormulaFromTicket && ticketCell.IsField && ticketCell.TableCell != null)
		{
			if (tableNeedUpdateCellDic.ContainsKey(ticketCell.TableCell))
			{
				tableNeedUpdateCellDic[ticketCell.TableCell] = ticketCell;
			}
			else
			{
				tableNeedUpdateCellDic.Add(ticketCell.TableCell, ticketCell);
			}
		}
	}

	public void CalculateTicket(bool forceCalculate = false)
	{
		if (!forceCalculate && IsInShowingVirtualNode)
		{
			return;
		}
		Dictionary<Auditai.Model.Cell, TicketInputCellVM> tableNeedUpdateCellDic = new Dictionary<Auditai.Model.Cell, TicketInputCellVM>();
		int num = 0;
		bool anyChanged;
		do
		{
			anyChanged = false;
			for (int i = 0; i < GetRowsCount(); i++)
			{
				for (int j = 0; j < _columns.Count; j++)
				{
					TicketInputCellVM cellVM = GetCellVM(i, j);
					if (!cellVM.IsFormula || cellVM.IsFixedMultiRowKey || !cellVM.IsFormulaFromTicket)
					{
						continue;
					}
					FormulaEvaluator formulaEvaluator = new FormulaEvaluator(cellVM.Formula)
					{
						Env = GetEvalEnv(i, cellVM)
					};
					try
					{
						if (!ShouldEval())
						{
							continue;
						}
						Operand operand = formulaEvaluator.EvaluateToOperandTicket(GetTicketEvalContext());
						if (!(operand is CellsOperand { IsCollectFill: not false }))
						{
							object obj = operand.Evaluate();
							if (!obj.Equals(cellVM.Value))
							{
								anyChanged = true;
							}
							cellVM.Value = obj;
							FindOutTicketFormulaShouldUpdateTableCell(cellVM, tableNeedUpdateCellDic);
						}
					}
					catch (FormulaException)
					{
						cellVM.Value = "";
						FindOutTicketFormulaShouldUpdateTableCell(cellVM, tableNeedUpdateCellDic);
					}
				}
			}
			EvalTitleFooter(Title);
			EvalTitleFooter(Footer);
			if (tableNeedUpdateCellDic.Count > 0)
			{
				BeginBatchUpdateValue();
				foreach (Auditai.Model.Cell key in tableNeedUpdateCellDic.Keys)
				{
					TicketInputCellVM ticketInputCellVM = tableNeedUpdateCellDic[key];
					string displayValue = key.GetDisplayValue();
					string text = ticketInputCellVM.ConvertInputValueToDisplayValue(ticketInputCellVM.Value);
					if (displayValue != text)
					{
						UpdateTableCellValue(key, ticketInputCellVM.Value, isFormulaExistManualInputValue: true);
					}
				}
				EndBatchUpdateValue();
				tableNeedUpdateCellDic.Clear();
			}
			num++;
		}
		while (anyChanged && num < 10);
		void EvalTitleFooter(TicketInputTitleFooterVM target)
		{
			foreach (TicketInputCellVM cell in target.Cells)
			{
				if (cell.IsFormula && cell.IsFormulaFromTicket)
				{
					FormulaEvaluator formulaEvaluator2 = new FormulaEvaluator(cell.Formula)
					{
						Env = GetEvalEnv(0, cell)
					};
					try
					{
						if (ShouldEval())
						{
							Operand operand2 = formulaEvaluator2.EvaluateToOperandTicket(GetTicketEvalContext());
							if (!(operand2 is CellsOperand { IsCollectFill: not false }))
							{
								object obj2 = operand2.Evaluate();
								if (!obj2.Equals(cell.Value))
								{
									anyChanged = true;
								}
								cell.Value = obj2;
								FindOutTicketFormulaShouldUpdateTableCell(cell, tableNeedUpdateCellDic);
							}
						}
					}
					catch (FormulaException)
					{
						cell.Value = "";
						FindOutTicketFormulaShouldUpdateTableCell(cell, tableNeedUpdateCellDic);
					}
				}
			}
		}
		static bool ShouldEval()
		{
			return true;
		}
	}

	private string ConvertTicketCellValueToCellFormula(TicketInputCellVM ticketCell)
	{
		string text;
		if (ticketCell.Value is double)
		{
			text = ticketCell.GetDisplayValue().Replace(",", "");
			if (string.IsNullOrEmpty(text))
			{
				text = "0";
			}
		}
		else
		{
			text = ticketCell.GetDisplayValue();
			text = text.Replace("\t", "");
			text = text.Replace("\r", "");
			text = text.Replace("\n", "");
			text = text.Replace("\"", "");
			text = text.Replace(";", "");
		}
		return text;
	}

	public TicketEvalContext GetTicketEvalContext()
	{
		return new TicketEvalContext
		{
			ResolveColumnWildcard = delegate(Id64 colId, int row, int ticketdatarow)
			{
				TicketInputCellVM ticketInputCellVM = _cells.FirstOrDefault((TicketInputCellVM c) => c.Column != null && c.Column.Id == colId);
				if (ticketInputCellVM == null)
				{
					if (Title != null)
					{
						ticketInputCellVM = Title.Cells.FirstOrDefault((TicketInputCellVM c) => c.Column != null && c.Column.Id == colId);
						if (ticketInputCellVM != null)
						{
							return ticketInputCellVM.TempCell;
						}
					}
					if (Footer != null)
					{
						ticketInputCellVM = Footer.Cells.FirstOrDefault((TicketInputCellVM c) => c.Column != null && c.Column.Id == colId);
						if (ticketInputCellVM != null)
						{
							return ticketInputCellVM.TempCell;
						}
					}
					return (Auditai.Model.Cell)null;
				}
				if (ticketInputCellVM.IsDynamicTicketDataRow)
				{
					if (ticketdatarow < 0)
					{
						ticketdatarow = 0;
					}
					return _cells.Where((TicketInputCellVM c) => c.Column != null && c.Column.Id == colId).ElementAt(ticketdatarow).TempCell;
				}
				if (ticketInputCellVM.IsFixedMultiRowKey || ticketInputCellVM.IsFixedMultiRowValue)
				{
					if (ticketdatarow < 0)
					{
						ticketdatarow = 0;
					}
					return _cells.Where((TicketInputCellVM c) => c.Column != null && c.Column.Id == colId).ElementAt(ticketdatarow).TempCell;
				}
				return ticketInputCellVM.TempCell;
			},
			ResolveColumn = (Id64 colId) => (CellsOperand)null,
			ResolveTicketCell = delegate(int row, int col)
			{
				int num2 = 0;
				TicketInputCellVM cellVM4;
				try
				{
					if (_ticket.Kind == TicketKind.DynamicRow)
					{
						if (row >= _ticket.DataRowStart + _ticket.DataRowCount)
						{
							num2 = row - (_ticket.DataRowStart + _ticket.DataRowCount) + (_ticket.DataRowStart + DataRowsCount) - row;
						}
					}
					else if (_ticket.Kind == TicketKind.FixedDataRowMixDynamicDataRow)
					{
						num2 = ConvertFixedRowIndexToVMRowIndex_MixTicket(row) - row;
					}
					cellVM4 = GetCellVM(row + num2, col);
				}
				catch (ArgumentOutOfRangeException)
				{
					throw new FormulaBadReferenceException();
				}
				return (cellVM4.Column == null || cellVM4.TempCell == null) ? new Auditai.Model.Cell
				{
					Value = cellVM4.Value,
					Row = DummyMR,
					Column = DummyColumn
				} : cellVM4.TempCell;
			},
			ResolveTicketRange = delegate(int r1, int c1, int r2, int c2)
			{
				List<Auditai.Model.Cell> list2 = new List<Auditai.Model.Cell>();
				int num = 0;
				if (_ticket.Kind == TicketKind.DynamicRow)
				{
					if (r1 >= _ticket.DataRowStart + _ticket.DataRowCount)
					{
						num = r1 - (_ticket.DataRowStart + _ticket.DataRowCount) + (_ticket.DataRowStart + DataRowsCount) - r1;
					}
				}
				else if (_ticket.Kind == TicketKind.FixedDataRowMixDynamicDataRow)
				{
					num = ConvertFixedRowIndexToVMRowIndex_MixTicket(r1) - r1;
				}
				for (int k = r1; k <= r2; k++)
				{
					for (int l = c1; l <= c2; l++)
					{
						TicketInputCellVM cellVM3;
						try
						{
							cellVM3 = GetCellVM(k + num, l);
						}
						catch (ArgumentOutOfRangeException)
						{
							throw new FormulaBadReferenceException();
						}
						if (cellVM3.Column == null || cellVM3.TempCell == null)
						{
							list2.Add(new Auditai.Model.Cell
							{
								Value = cellVM3.Value,
								Row = DummyMR,
								Column = DummyColumn
							});
						}
						else
						{
							list2.Add(cellVM3.TempCell);
						}
					}
				}
				return new CellsOperand(list2, Table);
			},
			ResolveTicketColumn = delegate(int col)
			{
				List<Auditai.Model.Cell> list = new List<Auditai.Model.Cell>();
				int value;
				if (_ticket.Kind == TicketKind.DynamicRow)
				{
					for (int i = _ticket.DataRowStart; i < _ticket.DataRowStart + DataRowsCount; i++)
					{
						TicketInputCellVM cellVM;
						try
						{
							cellVM = GetCellVM(i, col);
						}
						catch (ArgumentOutOfRangeException)
						{
							throw new FormulaBadReferenceException();
						}
						if (cellVM.Column != null && cellVM.TempCell != null)
						{
							list.Add(cellVM.TempCell);
						}
					}
				}
				else if (_ticket.Kind == TicketKind.FixedDataRowMixDynamicDataRow && MixTicketColumnIdMapperToTemplateRowIdDic.TryGetValue(col, out value))
				{
					try
					{
						for (int j = 0; j < _rows.Count; j++)
						{
							if (_rows[j].IsMixTicketDynamicDataRow && _rows[j].TicketRow.MixRangeDynamicDataRowTemplateId == value)
							{
								int col2 = col % _ticket.Columns.Count;
								TicketInputCellVM cellVM2 = GetCellVM(j, col2);
								if (cellVM2.Column != null && cellVM2.TempCell != null)
								{
									list.Add(cellVM2.TempCell);
								}
							}
						}
					}
					catch (ArgumentOutOfRangeException)
					{
						throw new FormulaBadReferenceException();
					}
				}
				return new CellsOperand(list, Table);
			}
		};
	}

	public int ConvertTicketDesignRowIndexToVMRowIndex(int rowIndex)
	{
		if (_ticket.Kind == TicketKind.DynamicRow)
		{
			if (rowIndex >= _ticket.DataRowStart + _ticket.DataRowCount)
			{
				return rowIndex - (_ticket.DataRowStart + _ticket.DataRowCount) + (_ticket.DataRowStart + DataRowsCount);
			}
			return rowIndex;
		}
		return rowIndex;
	}

	public int ConvertFixedRowIndexToVMRowIndex_MixTicket(int fixeRowIndex)
	{
		int num = -1;
		for (int i = 0; i < _rows.Count; i++)
		{
			if (!_rows[i].TicketRow.IsMixRangeDynamicDataRow)
			{
				num++;
				if (num == fixeRowIndex)
				{
					return i;
				}
			}
		}
		if (num == -1)
		{
			return fixeRowIndex;
		}
		return num;
	}

	public Tuple<int, int, int> GetMixTicketTicketColumnSetting(int colId)
	{
		if (MixTicketColumnIdMapperToTemplateRowIdDic == null)
		{
			return new Tuple<int, int, int>(0, -1, -1);
		}
		int item = colId % _columns.Count;
		int num = -1;
		int item2 = -1;
		try
		{
			if (MixTicketColumnIdMapperToTemplateRowIdDic.TryGetValue(colId, out var value))
			{
				for (int i = 0; i < _rows.Count; i++)
				{
					if (!_rows[i].IsMixTicketDynamicDataRow)
					{
						continue;
					}
					if (_rows[i].TicketRow.MixRangeDynamicDataRowTemplateId != value)
					{
						if (num != -1)
						{
							break;
						}
						continue;
					}
					if (num == -1)
					{
						num = i;
					}
					item2 = i;
				}
			}
		}
		catch (Exception)
		{
		}
		return Tuple.Create(item, num, item2);
	}

	public void PopulateMerges()
	{
		Merges.Clear();
		foreach (TicketMerge merge in _ticket.Merges)
		{
			TicketMerge ticketMerge = new TicketMerge
			{
				TopRow = TicketRowToVMRow(merge.TopRow),
				BottomRow = TicketRowToVMRow(merge.BottomRow),
				LeftColumn = merge.LeftColumn,
				RightColumn = merge.RightColumn
			};
			if (ticketMerge.TopRow >= 0 && ticketMerge.BottomRow >= 0)
			{
				Merges.Add(ticketMerge);
			}
		}
		foreach (TicketMerge dataRowMerge in _ticket.DataRowMerges)
		{
			for (int i = 0; i < DataRowsCount; i++)
			{
				Merges.Add(new TicketMerge
				{
					TopRow = i + _ticket.DataRowStart,
					BottomRow = i + _ticket.DataRowStart,
					LeftColumn = dataRowMerge.LeftColumn,
					RightColumn = dataRowMerge.RightColumn
				});
			}
		}
		if (_ticket.FixedAndDynamicMixRange == null || _ticket.FixedAndDynamicMixRange.DynamicDataRowTemplateRows == null)
		{
			return;
		}
		Dictionary<int, TicketTableMixRangeTemplateRow> dictionary = new Dictionary<int, TicketTableMixRangeTemplateRow>();
		foreach (TicketTableMixRangeTemplateRow dynamicDataRowTemplateRow in _ticket.FixedAndDynamicMixRange.DynamicDataRowTemplateRows)
		{
			if (dynamicDataRowTemplateRow.Merges != null && dynamicDataRowTemplateRow.Merges.Count > 0)
			{
				dictionary.Add(dynamicDataRowTemplateRow.TemplateId, dynamicDataRowTemplateRow);
			}
		}
		if (dictionary.Count <= 0)
		{
			return;
		}
		TicketTableMixRangeTemplateRow ticketTableMixRangeTemplateRow = null;
		for (int j = 0; j < _rows.Count; j++)
		{
			TicketInputRowVM ticketInputRowVM = _rows[j];
			if (!ticketInputRowVM.IsMixTicketDynamicDataRow || ticketInputRowVM.TicketRow == null)
			{
				continue;
			}
			if (ticketTableMixRangeTemplateRow == null || ticketInputRowVM.TicketRow.MixRangeDynamicDataRowTemplateId != ticketTableMixRangeTemplateRow.TemplateId)
			{
				ticketTableMixRangeTemplateRow = null;
				if (!dictionary.TryGetValue(ticketInputRowVM.TicketRow.MixRangeDynamicDataRowTemplateId, out var value))
				{
					continue;
				}
				ticketTableMixRangeTemplateRow = value;
			}
			foreach (TicketMerge merge2 in ticketTableMixRangeTemplateRow.Merges)
			{
				Merges.Add(new TicketMerge
				{
					TopRow = j,
					BottomRow = j,
					LeftColumn = merge2.LeftColumn,
					RightColumn = merge2.RightColumn
				});
			}
		}
		int TicketRowToVMRow(int row)
		{
			if (_ticket.Kind == TicketKind.FixedDataRowMixDynamicDataRow)
			{
				if (MixTicketFixTickRowIndexMapperToVMRowIndexDic == null)
				{
					MixTicketFixTickRowIndexMapperToVMRowIndexDic = new Dictionary<int, int>();
					int num = -1;
					for (int k = 0; k < _rows.Count; k++)
					{
						if (!_rows[k].IsMixTicketDynamicDataRow)
						{
							num++;
							MixTicketFixTickRowIndexMapperToVMRowIndexDic[num] = k;
						}
					}
				}
				if (MixTicketFixTickRowIndexMapperToVMRowIndexDic.TryGetValue(row, out var value2))
				{
					return value2;
				}
				return -1;
			}
			if (!_ticket.HasDataRow())
			{
				return row;
			}
			if (row < _ticket.DataRowStart)
			{
				return row;
			}
			return row + DataRowsCount;
		}
	}

	public void SetRowHeight(int index, int height)
	{
		if (_ticket.Kind == TicketKind.FixedDataRowMixDynamicDataRow)
		{
			TicketRow ticketRow = _rows[index].TicketRow;
			ticketRow.Height = height;
		}
		else if (IsRecordDataRow_DynamicRowTicket(index))
		{
			_ticket.DataRowHeight = height;
		}
		else
		{
			_ticket.Rows[VMIndexToModel_ExcludeMixTicket(index)].Height = height;
		}
	}

	public int GetRowHeight(int index)
	{
		if (_ticket.Kind == TicketKind.FixedDataRowMixDynamicDataRow)
		{
			TicketRow ticketRow = _rows[index].TicketRow;
			return ticketRow.Height;
		}
		if (IsRecordDataRow_DynamicRowTicket(index))
		{
			return _ticket.DataRowHeight;
		}
		return _ticket.Rows[VMIndexToModel_ExcludeMixTicket(index)].Height;
	}

	public int GetColumnWidth(int index)
	{
		return _ticket.Columns[index].Width;
	}

	public bool ChangeEmptyTableRowToAvailableDynamicDataRow_MixTicket(Auditai.Model.Row tableRow)
	{
		if (_ticket.FixedAndDynamicMixRange.DynamicDataRowTemplateRows.Count == 0)
		{
			return false;
		}
		bool result = false;
		TicketTableMixRangeTemplateRow ticketTableMixRangeTemplateRow = _ticket.FixedAndDynamicMixRange.DynamicDataRowTemplateRows[0];
		for (int i = 0; i < _ticket.Columns.Count; i++)
		{
			TicketCell cell = _ticket.GetCell(ticketTableMixRangeTemplateRow.RefTicketTableRowIndex, i);
			if (cell.IsMixRangeTicketKey || !cell.HasField())
			{
				continue;
			}
			string inputValue = cell.GetInputValue();
			if (!string.IsNullOrEmpty(inputValue))
			{
				Auditai.Model.Column byId = Table.Columns.GetById(cell.Field);
				if (byId != null)
				{
					result = true;
					Auditai.Model.Cell tableCell = Table[tableRow.Index, byId.Index];
					object value = Auditai.Model.Cell.ChangeDataTypeImpl(inputValue, byId.GetDataType());
					UpdateTableCellValue(tableCell, value, isFormulaExistManualInputValue: false);
				}
			}
		}
		return result;
	}

	public bool IsMixTicketDataRowEmpty(int index)
	{
		if (!_rows[index].IsMixTicketExistWriteTicketFormulaResultToTableRow && !_rows[index].IsNew)
		{
			return false;
		}
		for (int i = 0; i < _columns.Count; i++)
		{
			TicketInputCellVM cellVM = GetCellVM(index, i);
			if (cellVM.Column != null && cellVM.TableCell != null)
			{
				if (cellVM.Attachments != null && cellVM.Attachments.Attachments.Count > 0)
				{
					return false;
				}
				if (string.IsNullOrEmpty(cellVM.TicketCell.Text) && string.IsNullOrEmpty(cellVM.TicketCell.InputValue) && !cellVM.TableCell.IsEmpty)
				{
					return false;
				}
			}
		}
		return true;
	}

	public Auditai.Model.Row GetKeyCellsRefTableRow_MixTicket()
	{
		if (MixTicketKeyCells.Count == 0)
		{
			return null;
		}
		return MixTicketKeyCells[0].TableRow;
	}

	public bool IsKeyCellsRefTableRowBeNewAddedRow_MixTicket()
	{
		return MixTicketKeyCellsRefEmptyTableRow != null;
	}

	public bool IsTableRowDataColumnEmpty_MixTicket(Auditai.Model.Row mr)
	{
		if (Table.Ticket.FixedAndDynamicMixRange.DynamicDataRowTemplateRows.Count == 0)
		{
			return true;
		}
		int refTicketTableRowIndex = Table.Ticket.FixedAndDynamicMixRange.DynamicDataRowTemplateRows[0].RefTicketTableRowIndex;
		int count = _columns.Count;
		for (int i = 0; i < count; i++)
		{
			TicketCell cell = Table.Ticket.GetCell(refTicketTableRowIndex, i);
			if (!cell.HasField() || !string.IsNullOrEmpty(cell.Text) || !string.IsNullOrEmpty(cell.InputValue))
			{
				continue;
			}
			Auditai.Model.Column byId = Table.Columns.GetById(cell.Field);
			if (byId != null)
			{
				Auditai.Model.Cell cell2 = Table[mr.Index, byId.Index];
				if (!cell2.IsEmpty)
				{
					return false;
				}
				if (Table.CellPropManager.TryGetAttachments(cell2, out var _))
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool IsTableRowDataColumnEmpty_OnlyCheckAutoFillColumn_MixTicket(Auditai.Model.Row mr)
	{
		if (Table.Ticket.FixedAndDynamicMixRange.DynamicDataRowTemplateRows.Count == 0)
		{
			return true;
		}
		int refTicketTableRowIndex = Table.Ticket.FixedAndDynamicMixRange.DynamicDataRowTemplateRows[0].RefTicketTableRowIndex;
		int count = _columns.Count;
		for (int i = 0; i < count; i++)
		{
			TicketCell cell = Table.Ticket.GetCell(refTicketTableRowIndex, i);
			if (!cell.HasField() || (string.IsNullOrEmpty(cell.Text) && string.IsNullOrEmpty(cell.InputValue)))
			{
				continue;
			}
			Auditai.Model.Column byId = Table.Columns.GetById(cell.Field);
			if (byId != null)
			{
				Auditai.Model.Cell cell2 = Table[mr.Index, byId.Index];
				if (!cell2.IsEmpty)
				{
					return false;
				}
				if (Table.CellPropManager.TryGetAttachments(cell2, out var _))
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool IsTableRowKeyColumnEmpty_MixTicket(Auditai.Model.Row mr)
	{
		foreach (TicketInputCellVM mixTicketKeyCell in MixTicketKeyCells)
		{
			Auditai.Model.Column column = mixTicketKeyCell.Column;
			if (column != null)
			{
				Auditai.Model.Cell cell = Table[mr.Index, column.Index];
				if (!cell.IsEmpty)
				{
					return false;
				}
				if (Table.CellPropManager.TryGetAttachments(cell, out var _))
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool IsDataRowEmpty(int index)
	{
		if (!_rows[index].IsNew)
		{
			return false;
		}
		for (int i = 0; i < _columns.Count; i++)
		{
			TicketInputCellVM cellVM = GetCellVM(index, i);
			if (cellVM.Column != null && cellVM.TableCell != null)
			{
				if (cellVM.Attachments != null && cellVM.Attachments.Attachments.Count > 0)
				{
					return false;
				}
				if (!cellVM.TableCell.IsEmpty)
				{
					return false;
				}
			}
		}
		return true;
	}

	public Auditai.Model.Row GetKeyCellsRefTableRow_DynamicRow()
	{
		if (DynamicRowKeyCells.Count == 0)
		{
			return null;
		}
		return DynamicRowKeyCells[0].TableRow;
	}

	public bool IsTableRowDataColumnEmpty_DynamicRowTicket(Auditai.Model.Row mr)
	{
		int count = _columns.Count;
		for (int i = 0; i < count; i++)
		{
			TicketInputColumnVM ticketInputColumnVM = _columns[i];
			if (ticketInputColumnVM.TableColumn != null)
			{
				Auditai.Model.Cell cell = Table[mr.Index, ticketInputColumnVM.TableColumn.Index];
				if (!cell.IsEmpty)
				{
					return false;
				}
				if (Table.CellPropManager.TryGetAttachments(cell, out var _))
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool IsTableRowKeyColumnEmpty_DynamicRowTicket(Auditai.Model.Row mr)
	{
		foreach (TicketInputCellVM dynamicRowKeyCell in DynamicRowKeyCells)
		{
			Auditai.Model.Column column = dynamicRowKeyCell.Column;
			if (column != null)
			{
				Auditai.Model.Cell cell = Table[mr.Index, column.Index];
				if (!cell.IsEmpty)
				{
					return false;
				}
				if (Table.CellPropManager.TryGetAttachments(cell, out var _))
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool IsKeyCellsRefTableRowBeNewAddedRow_DynamicRow()
	{
		return DynamicRowKeyCellsRefEmptyTableRow != null;
	}

	public bool IsFixedOneRowTicketRefTableRowEmpty()
	{
		if (!_isFixedOneRowTicketRefTableRowBeNewCreated)
		{
			return false;
		}
		for (int i = 0; i < _rows.Count; i++)
		{
			for (int j = 0; j < _columns.Count; j++)
			{
				TicketInputCellVM cellVM = GetCellVM(i, j);
				if (cellVM.TableCell != null && !cellVM.TableCell.IsEmpty)
				{
					return false;
				}
			}
		}
		return true;
	}

	public Auditai.Model.Row GetAnyOrDefaultTableRow()
	{
		for (int i = 0; i < _rows.Count; i++)
		{
			for (int j = 0; j < _columns.Count; j++)
			{
				TicketInputCellVM cellVM = GetCellVM(i, j);
				if (cellVM.TableCell != null)
				{
					return cellVM.TableCell.Row;
				}
			}
		}
		return null;
	}

	public Operand GetComboList(int vmRowIndex, int vmColIndex, string comboList)
	{
		if (string.IsNullOrWhiteSpace(comboList))
		{
			return ValueSetOperand.Empty;
		}
		if (vmRowIndex < 0 || vmRowIndex >= _rows.Count)
		{
			return ValueSetOperand.Empty;
		}
		TicketInputCellVM cellVM = GetCellVM(vmRowIndex, vmColIndex);
		if (cellVM == null || cellVM.TableRow == null)
		{
			return ValueSetOperand.Empty;
		}
		FormulaReferenceModelResolver resolver = new FormulaReferenceModelResolver(Table.Project);
		FormulaEvaluationEnvironment env = new FormulaEvaluationEnvironment
		{
			Resolver = resolver,
			RowIndex = cellVM.TableRow.Index,
			HostTable = Table,
			RefManager = Table.Project.DataReferenceManager,
			RefEvalContext = new DataReferenceEvaluationContext
			{
				Project = Table.Project,
				CurrentTreeNode = Table.TreeNode
			}
		};
		try
		{
			FormulaEvaluator formulaEvaluator = new FormulaEvaluator(comboList)
			{
				Env = env
			};
			Operand operand = formulaEvaluator.EvaluateToOperand();
			if (operand is TreeListOperand || operand is TableListOperand || operand is MultiListOperand || operand is InputListOperand)
			{
				return operand;
			}
			return operand.ToValueSetOrderByRowIndex();
		}
		catch (FormulaException)
		{
			return ValueSetOperand.Empty;
		}
	}

	public Operand GetTitleCellValueList(int row, int col, string comboList)
	{
		return GetCellValueList(Title, row, col, comboList);
	}

	public Operand GetFooterCellValueList(int row, int col, string comboList)
	{
		return GetCellValueList(Footer, row, col, comboList);
	}

	protected Operand GetCellValueList(TicketInputTitleFooterVM target, int row, int col, string comboList)
	{
		if (string.IsNullOrWhiteSpace(comboList))
		{
			return ValueSetOperand.Empty;
		}
		if (target.IsIndexOutOfRange(row, col))
		{
			return ValueSetOperand.Empty;
		}
		TicketInputCellVM cellVM = target.GetCellVM(row, col);
		if (cellVM == null || cellVM.TableRow == null)
		{
			return ValueSetOperand.Empty;
		}
		FormulaReferenceModelResolver resolver = new FormulaReferenceModelResolver(Table.Project);
		FormulaEvaluationEnvironment env = new FormulaEvaluationEnvironment
		{
			Resolver = resolver,
			RowIndex = cellVM.TableRow.Index,
			HostTable = Table,
			RefManager = Table.Project.DataReferenceManager,
			RefEvalContext = new DataReferenceEvaluationContext
			{
				Project = Table.Project,
				CurrentTreeNode = Table.TreeNode
			}
		};
		try
		{
			FormulaEvaluator formulaEvaluator = new FormulaEvaluator(comboList)
			{
				Env = env
			};
			Operand operand = formulaEvaluator.EvaluateToOperand();
			if (operand is TreeListOperand || operand is TableListOperand || operand is MultiListOperand || operand is InputListOperand)
			{
				return operand;
			}
			return operand.ToValueSetOrderByRowIndex();
		}
		catch (FormulaException)
		{
			return ValueSetOperand.Empty;
		}
	}

	public bool CanEditColumn(Auditai.Model.Column column)
	{
		if (Auditai.Model.Project.Current.Creator.Id == Auditai.Model.User.Current.Id)
		{
			return true;
		}
		if (!Auditai.Model.Project.Current.Users.Any((KeyValuePair<Auditai.DTO.User, UserRole> kv) => kv.Key.Id == Auditai.Model.User.Current.Id))
		{
			return false;
		}
		if (Auditai.Model.Project.Current.Users.First((KeyValuePair<Auditai.DTO.User, UserRole> kv) => kv.Key.Id == Auditai.Model.User.Current.Id).Value == UserRole.Manager)
		{
			return true;
		}
		return column.Permissions.CanWrite();
	}

	private bool IsCombolistOnlyOneValue(Operand op, out string theValue)
	{
		theValue = "";
		if (op is ValueSetOperand valueSetOperand)
		{
			if (valueSetOperand.Set.Count == 1)
			{
				theValue = valueSetOperand.Set.First().Item2.ToString();
				return true;
			}
			return false;
		}
		if (op is TableListOperand tableListOperand)
		{
			if (tableListOperand.DataTable.Rows.Count == 1)
			{
				theValue = tableListOperand.DataTable.Rows[0][0].ToString();
				return true;
			}
			return false;
		}
		return false;
	}
}
