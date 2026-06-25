using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Antlr4.Runtime.Misc;

namespace Auditai.Model;

public class GetRefIntervalsListener : FormulaDisplayParserBaseListener
{
	private static readonly Color[] _formulaRefRectColors = new Color[7]
	{
		Color.Blue,
		Color.Red,
		Color.Magenta,
		Color.Green,
		Color.DeepPink,
		Color.Orange,
		Color.DarkCyan
	};

	private FormulaContext _context;

	private int _colorIndex;

	private bool _inColName;

	public List<FormulaDisplayRef> Refs { get; } = new List<FormulaDisplayRef>();


	public Color GetNextColor()
	{
		return _formulaRefRectColors[_colorIndex % _formulaRefRectColors.Length];
	}

	public GetRefIntervalsListener(FormulaContext context)
	{
		_context = context;
	}

	public override void ExitTableCell([NotNull] FormulaDisplayParser.TableCellContext context)
	{
		string text = context.TableName().GetText();
		string text2 = context.columnName().GetText();
		int row = int.Parse(context.Int().GetText());
		Table table = _context.Project.GetTableByCanonicalName(text).LoadAndReturn();
		Cell cell = table.ResolveCell(text2, row);
		if (cell != null)
		{
			int item = context.Stop.StopIndex - context.Start.StartIndex + 1;
			Tuple<int, int> item2 = Tuple.Create(context.Start.StartIndex, item);
			FormulaDisplayRef formulaDisplayRef = Refs.FirstOrDefault((FormulaDisplayRef r) => r.Kind == FormulaDisplayRefKind.Cell && r.Cell == cell);
			if (formulaDisplayRef == null)
			{
				Refs.Add(new FormulaDisplayRef
				{
					Kind = FormulaDisplayRefKind.Cell,
					Table = table,
					Cell = cell,
					Color = _formulaRefRectColors[_colorIndex % _formulaRefRectColors.Length],
					Intervals = { item2 }
				});
				_colorIndex++;
			}
			else
			{
				formulaDisplayRef.Intervals.Add(item2);
			}
		}
	}

	public override void ExitColumnCell([NotNull] FormulaDisplayParser.ColumnCellContext context)
	{
		string text = context.columnName().GetText();
		int row = int.Parse(context.Int().GetText());
		Cell cell = _context.Table.ResolveCell(text, row);
		if (cell != null)
		{
			int item = context.Stop.StopIndex - context.Start.StartIndex + 1;
			Tuple<int, int> item2 = Tuple.Create(context.Start.StartIndex, item);
			FormulaDisplayRef formulaDisplayRef = Refs.FirstOrDefault((FormulaDisplayRef r) => r.Kind == FormulaDisplayRefKind.Cell && r.Cell == cell);
			if (formulaDisplayRef == null)
			{
				Refs.Add(new FormulaDisplayRef
				{
					Kind = FormulaDisplayRefKind.Cell,
					Table = _context.Table,
					Cell = cell,
					Color = _formulaRefRectColors[_colorIndex % _formulaRefRectColors.Length],
					Intervals = { item2 }
				});
				_colorIndex++;
			}
			else
			{
				formulaDisplayRef.Intervals.Add(item2);
			}
		}
	}

	public override void ExitTableColumn([NotNull] FormulaDisplayParser.TableColumnContext context)
	{
		if (_inColName)
		{
			return;
		}
		string text = context.TableName().GetText();
		string text2 = context.columnName().GetText();
		if (_context.Kind == FormulaContextKind.LedgerCollectFormulaEdit)
		{
			if (StringComparer.OrdinalIgnoreCase.Equals(text, _context.LegderVirtualTableSetting.GetBalanceVirtualTableName()))
			{
				Column virtualTableCol2 = _context.LegderVirtualTableSetting.GetBalanceEmptyVirtualTableColumn(text2);
				if (virtualTableCol2 != null)
				{
					LedgerVirtualTable balanceEmptyVirtualTable = _context.LegderVirtualTableSetting.GetBalanceEmptyVirtualTable();
					int item = context.Stop.StopIndex - context.Start.StartIndex + 1;
					Tuple<int, int> item2 = Tuple.Create(context.Start.StartIndex, item);
					FormulaDisplayRef formulaDisplayRef = Refs.FirstOrDefault((FormulaDisplayRef r) => r.Kind == FormulaDisplayRefKind.Column && r.Column == virtualTableCol2);
					if (formulaDisplayRef == null)
					{
						Refs.Add(new FormulaDisplayRef
						{
							Kind = FormulaDisplayRefKind.Column,
							Table = balanceEmptyVirtualTable,
							Column = virtualTableCol2,
							Color = _formulaRefRectColors[_colorIndex % _formulaRefRectColors.Length],
							Intervals = { item2 }
						});
						_colorIndex++;
					}
					else
					{
						formulaDisplayRef.Intervals.Add(item2);
					}
				}
				return;
			}
			if (StringComparer.OrdinalIgnoreCase.Equals(text, _context.LegderVirtualTableSetting.GetVoucherVirtualTableName()))
			{
				Column virtualTableCol = _context.LegderVirtualTableSetting.GetVoucherEmptyVirtualTableColumn(text2);
				if (virtualTableCol != null)
				{
					LedgerVirtualTable voucherEmptyVirtualTable = _context.LegderVirtualTableSetting.GetVoucherEmptyVirtualTable();
					int item3 = context.Stop.StopIndex - context.Start.StartIndex + 1;
					Tuple<int, int> item4 = Tuple.Create(context.Start.StartIndex, item3);
					FormulaDisplayRef formulaDisplayRef2 = Refs.FirstOrDefault((FormulaDisplayRef r) => r.Kind == FormulaDisplayRefKind.Column && r.Column == virtualTableCol);
					if (formulaDisplayRef2 == null)
					{
						Refs.Add(new FormulaDisplayRef
						{
							Kind = FormulaDisplayRefKind.Column,
							Table = voucherEmptyVirtualTable,
							Column = virtualTableCol,
							Color = _formulaRefRectColors[_colorIndex % _formulaRefRectColors.Length],
							Intervals = { item4 }
						});
						_colorIndex++;
					}
					else
					{
						formulaDisplayRef2.Intervals.Add(item4);
					}
				}
				return;
			}
		}
		Table table = _context.Project.GetTableByCanonicalName(text).LoadAndReturn();
		Column col = table.Columns.GetByCaption(text2);
		if (col != null)
		{
			int item5 = context.Stop.StopIndex - context.Start.StartIndex + 1;
			Tuple<int, int> item6 = Tuple.Create(context.Start.StartIndex, item5);
			FormulaDisplayRef formulaDisplayRef3 = Refs.FirstOrDefault((FormulaDisplayRef r) => r.Kind == FormulaDisplayRefKind.Column && r.Column == col);
			if (formulaDisplayRef3 == null)
			{
				Refs.Add(new FormulaDisplayRef
				{
					Kind = FormulaDisplayRefKind.Column,
					Table = table,
					Column = col,
					Color = _formulaRefRectColors[_colorIndex % _formulaRefRectColors.Length],
					Intervals = { item6 }
				});
				_colorIndex++;
			}
			else
			{
				formulaDisplayRef3.Intervals.Add(item6);
			}
			return;
		}
		Cell cell = table.Cells.GetByCaption(text2);
		if (cell != null)
		{
			int item7 = context.Stop.StopIndex - context.Start.StartIndex + 1;
			Tuple<int, int> item8 = Tuple.Create(context.Start.StartIndex, item7);
			FormulaDisplayRef formulaDisplayRef4 = Refs.FirstOrDefault((FormulaDisplayRef r) => r.Kind == FormulaDisplayRefKind.HeaderCell && r.Cell == cell);
			if (formulaDisplayRef4 == null)
			{
				Refs.Add(new FormulaDisplayRef
				{
					Kind = FormulaDisplayRefKind.HeaderCell,
					Table = table,
					Cell = cell,
					Color = _formulaRefRectColors[_colorIndex % _formulaRefRectColors.Length],
					Intervals = { item8 }
				});
				_colorIndex++;
			}
			else
			{
				formulaDisplayRef4.Intervals.Add(item8);
			}
		}
	}

	public override void ExitColumn([NotNull] FormulaDisplayParser.ColumnContext context)
	{
		if (_inColName)
		{
			return;
		}
		string text = context.columnName().GetText();
		Column col = _context.Table.Columns.GetByCaption(text);
		if (col != null)
		{
			int item = context.Stop.StopIndex - context.Start.StartIndex + 1;
			Tuple<int, int> item2 = Tuple.Create(context.Start.StartIndex, item);
			FormulaDisplayRef formulaDisplayRef = Refs.FirstOrDefault((FormulaDisplayRef r) => r.Kind == FormulaDisplayRefKind.Column && r.Column == col);
			if (formulaDisplayRef == null)
			{
				Refs.Add(new FormulaDisplayRef
				{
					Kind = FormulaDisplayRefKind.Column,
					Table = _context.Table,
					Column = col,
					Color = _formulaRefRectColors[_colorIndex % _formulaRefRectColors.Length],
					Intervals = { item2 }
				});
				_colorIndex++;
			}
			else
			{
				formulaDisplayRef.Intervals.Add(item2);
			}
			return;
		}
		Cell cell = _context.Table.Cells.GetByCaption(text);
		if (cell != null)
		{
			int item3 = context.Stop.StopIndex - context.Start.StartIndex + 1;
			Tuple<int, int> item4 = Tuple.Create(context.Start.StartIndex, item3);
			FormulaDisplayRef formulaDisplayRef2 = Refs.FirstOrDefault((FormulaDisplayRef r) => r.Kind == FormulaDisplayRefKind.HeaderCell && r.Cell == cell);
			if (formulaDisplayRef2 == null)
			{
				Refs.Add(new FormulaDisplayRef
				{
					Kind = FormulaDisplayRefKind.HeaderCell,
					Table = _context.Table,
					Cell = cell,
					Color = _formulaRefRectColors[_colorIndex % _formulaRefRectColors.Length],
					Intervals = { item4 }
				});
				_colorIndex++;
			}
			else
			{
				formulaDisplayRef2.Intervals.Add(item4);
			}
		}
	}

	public override void ExitTableColumnWildcard([NotNull] FormulaDisplayParser.TableColumnWildcardContext context)
	{
		string text = context.TableName().GetText();
		string text2 = context.columnName().GetText();
		Table table = _context.Project.GetTableByCanonicalName(text).LoadAndReturn();
		Column col = table.Columns.GetByCaption(text2);
		if (col != null)
		{
			int item = context.Stop.StopIndex - context.Start.StartIndex + 1;
			Tuple<int, int> item2 = Tuple.Create(context.Start.StartIndex, item);
			FormulaDisplayRef formulaDisplayRef = Refs.FirstOrDefault((FormulaDisplayRef r) => r.Kind == FormulaDisplayRefKind.ColumnWildcard && r.Column == col);
			if (formulaDisplayRef == null)
			{
				Refs.Add(new FormulaDisplayRef
				{
					Kind = FormulaDisplayRefKind.ColumnWildcard,
					Table = table,
					Column = col,
					Color = _formulaRefRectColors[_colorIndex % _formulaRefRectColors.Length],
					Intervals = { item2 }
				});
				_colorIndex++;
			}
			else
			{
				formulaDisplayRef.Intervals.Add(item2);
			}
			return;
		}
		Cell cell = table.Cells.GetByCaption(text2);
		if (cell != null)
		{
			int item3 = context.Stop.StopIndex - context.Start.StartIndex + 1;
			Tuple<int, int> item4 = Tuple.Create(context.Start.StartIndex, item3);
			FormulaDisplayRef formulaDisplayRef2 = Refs.FirstOrDefault((FormulaDisplayRef r) => r.Kind == FormulaDisplayRefKind.HeaderCellWildcard && r.Cell == cell);
			if (formulaDisplayRef2 == null)
			{
				Refs.Add(new FormulaDisplayRef
				{
					Kind = FormulaDisplayRefKind.HeaderCellWildcard,
					Table = table,
					Cell = cell,
					Color = _formulaRefRectColors[_colorIndex % _formulaRefRectColors.Length],
					Intervals = { item4 }
				});
				_colorIndex++;
			}
			else
			{
				formulaDisplayRef2.Intervals.Add(item4);
			}
		}
	}

	public override void EnterColumnWildcard([NotNull] FormulaDisplayParser.ColumnWildcardContext context)
	{
		string text = context.columnName().GetText();
		Column col = _context.Table.Columns.GetByCaption(text);
		if (col != null)
		{
			int item = context.Stop.StopIndex - context.Start.StartIndex + 1;
			Tuple<int, int> item2 = Tuple.Create(context.Start.StartIndex, item);
			FormulaDisplayRef formulaDisplayRef = Refs.FirstOrDefault((FormulaDisplayRef r) => r.Kind == FormulaDisplayRefKind.ColumnWildcard && r.Column == col);
			if (formulaDisplayRef == null)
			{
				Refs.Add(new FormulaDisplayRef
				{
					Kind = FormulaDisplayRefKind.ColumnWildcard,
					Table = _context.Table,
					Column = col,
					Color = _formulaRefRectColors[_colorIndex % _formulaRefRectColors.Length],
					Intervals = { item2 }
				});
				_colorIndex++;
			}
			else
			{
				formulaDisplayRef.Intervals.Add(item2);
			}
			return;
		}
		Cell cell = _context.Table.Cells.GetByCaption(text);
		if (cell != null)
		{
			int item3 = context.Stop.StopIndex - context.Start.StartIndex + 1;
			Tuple<int, int> item4 = Tuple.Create(context.Start.StartIndex, item3);
			FormulaDisplayRef formulaDisplayRef2 = Refs.FirstOrDefault((FormulaDisplayRef r) => r.Kind == FormulaDisplayRefKind.HeaderCellWildcard && r.Cell == cell);
			if (formulaDisplayRef2 == null)
			{
				Refs.Add(new FormulaDisplayRef
				{
					Kind = FormulaDisplayRefKind.HeaderCellWildcard,
					Table = _context.Table,
					Cell = cell,
					Color = _formulaRefRectColors[_colorIndex % _formulaRefRectColors.Length],
					Intervals = { item4 }
				});
				_colorIndex++;
			}
			else
			{
				formulaDisplayRef2.Intervals.Add(item4);
			}
		}
	}

	public override void ExitTableRange([NotNull] FormulaDisplayParser.TableRangeContext context)
	{
		string text = context.TableName().GetText();
		Table table = _context.Project.GetTableByCanonicalName(text).LoadAndReturn();
		string text2 = context.columnName(0).GetText();
		int row = int.Parse(context.Int(0).GetText());
		Cell cell1 = table.ResolveCell(text2, row);
		string text3 = context.columnName(1).GetText();
		int row2 = int.Parse(context.Int(1).GetText());
		Cell cell2 = table.ResolveCell(text3, row2);
		if (cell1 != null && cell2 != null)
		{
			int item = context.Stop.StopIndex - context.Start.StartIndex + 1;
			Tuple<int, int> item2 = Tuple.Create(context.Start.StartIndex, item);
			FormulaDisplayRef formulaDisplayRef = Refs.FirstOrDefault((FormulaDisplayRef r) => r.Kind == FormulaDisplayRefKind.Range && r.Cell == cell1 && r.Cell2 == cell2);
			if (formulaDisplayRef == null)
			{
				Refs.Add(new FormulaDisplayRef
				{
					Kind = FormulaDisplayRefKind.Range,
					Table = table,
					Cell = cell1,
					Cell2 = cell2,
					Color = _formulaRefRectColors[_colorIndex % _formulaRefRectColors.Length],
					Intervals = { item2 }
				});
				_colorIndex++;
			}
			else
			{
				formulaDisplayRef.Intervals.Add(item2);
			}
		}
	}

	public override void ExitRange([NotNull] FormulaDisplayParser.RangeContext context)
	{
		string text = context.columnName(0).GetText();
		int row = int.Parse(context.Int(0).GetText());
		Cell cell1 = _context.Table.ResolveCell(text, row);
		string text2 = context.columnName(1).GetText();
		int row2 = int.Parse(context.Int(1).GetText());
		Cell cell2 = _context.Table.ResolveCell(text2, row2);
		if (cell1 != null && cell2 != null)
		{
			int item = context.Stop.StopIndex - context.Start.StartIndex + 1;
			Tuple<int, int> item2 = Tuple.Create(context.Start.StartIndex, item);
			FormulaDisplayRef formulaDisplayRef = Refs.FirstOrDefault((FormulaDisplayRef r) => r.Kind == FormulaDisplayRefKind.Range && r.Cell == cell1 && r.Cell2 == cell2);
			if (formulaDisplayRef == null)
			{
				Refs.Add(new FormulaDisplayRef
				{
					Kind = FormulaDisplayRefKind.Range,
					Table = _context.Table,
					Cell = cell1,
					Cell2 = cell2,
					Color = _formulaRefRectColors[_colorIndex % _formulaRefRectColors.Length],
					Intervals = { item2 }
				});
				_colorIndex++;
			}
			else
			{
				formulaDisplayRef.Intervals.Add(item2);
			}
		}
	}

	public override void EnterFunc([NotNull] FormulaDisplayParser.FuncContext context)
	{
		string text = context.funcName().GetText();
		if (!text.Equals("ColName", StringComparison.OrdinalIgnoreCase))
		{
			return;
		}
		_inColName = true;
		if (context.expr(0) is FormulaDisplayParser.ColumnContext columnContext)
		{
			string text2 = columnContext.columnName().GetText();
			Column col2 = _context.Table.Columns.GetByCaption(text2);
			if (col2 != null)
			{
				int item = context.Stop.StopIndex - context.Start.StartIndex + 1;
				Tuple<int, int> item2 = Tuple.Create(context.Start.StartIndex, item);
				FormulaDisplayRef formulaDisplayRef = Refs.FirstOrDefault((FormulaDisplayRef r) => r.Kind == FormulaDisplayRefKind.ColumnHeader && r.Column == col2);
				if (formulaDisplayRef == null)
				{
					Refs.Add(new FormulaDisplayRef
					{
						Kind = FormulaDisplayRefKind.ColumnHeader,
						Table = _context.Table,
						Column = col2,
						Color = _formulaRefRectColors[_colorIndex % _formulaRefRectColors.Length],
						Intervals = { item2 }
					});
					_colorIndex++;
				}
				else
				{
					formulaDisplayRef.Intervals.Add(item2);
				}
			}
		}
		else
		{
			if (!(context.expr(0) is FormulaDisplayParser.TableColumnContext tableColumnContext))
			{
				return;
			}
			string text3 = tableColumnContext.TableName().GetText();
			string text4 = tableColumnContext.columnName().GetText();
			Table table = _context.Project.GetTableByCanonicalName(text3).LoadAndReturn();
			Column col = table.Columns.GetByCaption(text4);
			if (col != null)
			{
				int item3 = context.Stop.StopIndex - context.Start.StartIndex + 1;
				Tuple<int, int> item4 = Tuple.Create(context.Start.StartIndex, item3);
				FormulaDisplayRef formulaDisplayRef2 = Refs.FirstOrDefault((FormulaDisplayRef r) => r.Kind == FormulaDisplayRefKind.ColumnHeader && r.Column == col);
				if (formulaDisplayRef2 == null)
				{
					Refs.Add(new FormulaDisplayRef
					{
						Kind = FormulaDisplayRefKind.ColumnHeader,
						Table = table,
						Column = col,
						Color = _formulaRefRectColors[_colorIndex % _formulaRefRectColors.Length],
						Intervals = { item4 }
					});
					_colorIndex++;
				}
				else
				{
					formulaDisplayRef2.Intervals.Add(item4);
				}
			}
		}
	}

	public override void ExitFunc([NotNull] FormulaDisplayParser.FuncContext context)
	{
		_inColName = false;
		string text = context.funcName().GetText();
		if (text.Equals("Title", StringComparison.OrdinalIgnoreCase))
		{
			if (context.expr(0) is FormulaDisplayParser.TreeNodeContext treeNodeContext)
			{
				Table tableByCanonicalName = _context.Project.GetTableByCanonicalName(treeNodeContext.TableName().GetText());
				int row2 = int.Parse(context.expr(1).GetText());
				int col2 = int.Parse(context.expr(2).GetText());
				int item = context.Stop.StopIndex - context.Start.StartIndex + 1;
				Tuple<int, int> item2 = Tuple.Create(context.Start.StartIndex, item);
				FormulaDisplayRef formulaDisplayRef = Refs.FirstOrDefault((FormulaDisplayRef r) => r.Kind == FormulaDisplayRefKind.Title && r.TitleOrFootRow == row2 && r.TitleOrFootCol == col2);
				if (formulaDisplayRef == null)
				{
					Refs.Add(new FormulaDisplayRef
					{
						Kind = FormulaDisplayRefKind.Title,
						Table = tableByCanonicalName,
						TitleOrFootRow = row2,
						TitleOrFootCol = col2,
						Color = _formulaRefRectColors[_colorIndex % _formulaRefRectColors.Length],
						Intervals = { item2 }
					});
					_colorIndex++;
				}
				else
				{
					formulaDisplayRef.Intervals.Add(item2);
				}
			}
		}
		else if (text.Equals("Foot", StringComparison.OrdinalIgnoreCase) && context.expr(0) is FormulaDisplayParser.TreeNodeContext treeNodeContext2)
		{
			Table tableByCanonicalName2 = _context.Project.GetTableByCanonicalName(treeNodeContext2.TableName().GetText());
			int row = int.Parse(context.expr(1).GetText());
			int col = int.Parse(context.expr(2).GetText());
			int item3 = context.Stop.StopIndex - context.Start.StartIndex + 1;
			Tuple<int, int> item4 = Tuple.Create(context.Start.StartIndex, item3);
			FormulaDisplayRef formulaDisplayRef2 = Refs.FirstOrDefault((FormulaDisplayRef r) => r.Kind == FormulaDisplayRefKind.Foot && r.TitleOrFootRow == row && r.TitleOrFootCol == col);
			if (formulaDisplayRef2 == null)
			{
				Refs.Add(new FormulaDisplayRef
				{
					Kind = FormulaDisplayRefKind.Foot,
					Table = tableByCanonicalName2,
					TitleOrFootRow = row,
					TitleOrFootCol = col,
					Color = _formulaRefRectColors[_colorIndex % _formulaRefRectColors.Length],
					Intervals = { item4 }
				});
				_colorIndex++;
			}
			else
			{
				formulaDisplayRef2.Intervals.Add(item4);
			}
		}
	}

	public override void ExitFormula([NotNull] FormulaDisplayParser.FormulaContext context)
	{
		_colorIndex--;
		if (_colorIndex < 0)
		{
			_colorIndex = 0;
		}
	}
}
