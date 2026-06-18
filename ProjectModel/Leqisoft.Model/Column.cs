﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using Leqisoft.DTO;

namespace Leqisoft.Model;

public class Column
{
	private const int MIN_COLUMN_WIDTH = 0;

	private const int MAX_COLUMN_WIDTH = 9999;

	private int _width;

	public static Regex reInvalidChars = new Regex("[:,=\\+\\-\\*/\\(\\)\\[\\]\\{\\}\\n\\r<>\"'，（）＝ \u3000]", RegexOptions.Compiled);

	public static Regex reBeginWithDigit = new Regex("^\\d", RegexOptions.Compiled);

	public ColumnDirtyMask Dirty;

	private bool _isExistFillFormulaDirty = true;

	private bool _isExistFillFormula;

	private static Stopwatch _stopWatcher;

	public Id64 Id { get; set; }

	public SyncStatus Status { get; set; }

	public int Index { get; set; }

	public int ServerIndex { get; set; }

	public int Width
	{
		get
		{
			return _width;
		}
		set
		{
			if (value <= 0)
			{
				value = 1;
			}
			_width = value;
		}
	}

	public string Caption { get; set; }

	public string CaptionDisplay => Table.HeaderMode switch
	{
		TableHeaderMode.Custom => Caption, 
		TableHeaderMode.Fixed => GetFixedCaption(), 
		_ => throw new ArgumentOutOfRangeException(), 
	};

	public string CaptionFormula { get; set; }

	public CellStyle CaptionStyle { get; internal set; } = new CellStyle();


	public bool IsFormulaRefSelfTableColumnOrCell { get; private set; }

	public bool Visible { get; set; }

	public CellStyle Style { get; set; }

	public Permissions Permissions { get; } = new Permissions();


	public Table Table { get; internal set; }

	public ConsolidateAttributes ConsolidateAttributes { get; set; }

	public ColumnSubtotal SubtotalAttributes { get; set; }

	public string Formula { get; set; }

	public bool HasFormula => !string.IsNullOrEmpty(Formula);

	public bool IsLocked
	{
		get
		{
			long? num = Style?.Locker;
			if (num.HasValue)
			{
				return num != 0;
			}
			return false;
		}
	}

	public CrossAttributes CrossAttributes { get; set; } = new CrossAttributes();


	public bool IsExistFillFormula
	{
		get
		{
			if (!HasFormula)
			{
				return false;
			}
			if (!_isExistFillFormulaDirty)
			{
				return _isExistFillFormula;
			}
			FormulaEvaluator formulaEvaluator = new FormulaEvaluator(Formula);
			IsFillFormula isFillFormula = formulaEvaluator.IsFill();
			_isExistFillFormula = isFillFormula.IsFill || isFillFormula.IsLqCollect;
			_isExistFillFormulaDirty = false;
			return _isExistFillFormula;
		}
	}

	public bool HasLedgerCollectFormula => !string.IsNullOrWhiteSpace(GetFormat().LedgerCollectFormula);

	public string LedgerCollectFormula => GetFormat().LedgerCollectFormula;

	internal bool IsIndexDirty => ServerIndex != Index;

	public bool IsAllowManualInputValueFormulaColumn
	{
		get
		{
			if (!HasFormula)
			{
				return false;
			}
			return true;
		}
	}

	public Font GetCaptionFont()
	{
		return new Font(CaptionStyle.FontFamily, CaptionStyle.FontSize.Value, GetFontStyle());
	}

	private FontStyle GetFontStyle()
	{
		FontStyle fontStyle = FontStyle.Regular;
		if (CaptionStyle.Bold.Value)
		{
			fontStyle |= FontStyle.Bold;
		}
		if (CaptionStyle.Italic.Value)
		{
			fontStyle |= FontStyle.Italic;
		}
		if (CaptionStyle.Underline.Value)
		{
			fontStyle |= FontStyle.Underline;
		}
		return fontStyle;
	}

	public string SerializeConsolidateAttributes()
	{
		return ConsolidateAttributes?.Serialize() ?? string.Empty;
	}

	public IEnumerable<Cell> GetCells(bool excludeFixedRows = false)
	{
		IEnumerable<Cell> enumerable = Table.Cells.Where((Cell c) => c.Column == this);
		if (excludeFixedRows)
		{
			enumerable = enumerable.Where((Cell c) => c.ShouldApplyColumnFormula());
		}
		return enumerable;
	}

	internal Column()
	{
	}

	public void UpdateCaption(string caption)
	{
		if (!(Caption == caption))
		{
			Caption = caption;
			if (Status == SyncStatus.Synced)
			{
				Dirty.IsCaptionDirty = true;
			}
			Table.NeedSave = true;
			Table.AdjustHeaderHeights();
		}
	}

	public void UpdateCaptionStyle(CellStyle style)
	{
		CaptionStyle = style;
		if (Status == SyncStatus.Synced)
		{
			Dirty.IsCaptionStyleDirty = true;
		}
		Table.NeedSave = true;
	}

	public void UpdateWidth(int width)
	{
		if (width < 0)
		{
			throw new ArgumentOutOfRangeException("width", $"列宽不能小于{0}");
		}
		if (width > 9999)
		{
			throw new ArgumentOutOfRangeException("width", $"列宽不能大于{9999}");
		}
		if (Width != width)
		{
			Width = width;
			if (Status == SyncStatus.Synced)
			{
				Dirty.IsWidthDirty = true;
			}
			Table.NeedSave = true;
		}
	}

	public void UpdateVisible(bool visible)
	{
		if (Visible != visible)
		{
			Visible = visible;
			if (Status == SyncStatus.Synced)
			{
				Dirty.IsVisibleDirty = true;
			}
			Table.NeedSave = true;
		}
	}

	public void UpdateFormula(string text)
	{
		if (HasFormula)
		{
			FormulaEvaluator formulaEvaluator = new FormulaEvaluator(Formula);
			if (formulaEvaluator.HasLqCrossTable())
			{
				if (string.IsNullOrWhiteSpace(text))
				{
					ClearCrossAttributes();
				}
				else
				{
					formulaEvaluator = new FormulaEvaluator(text);
					if (!formulaEvaluator.HasLqCrossTable())
					{
						ClearCrossAttributes();
					}
				}
			}
		}
		string formula = Formula;
		try
		{
			if (string.IsNullOrWhiteSpace(formula))
			{
				int count = Table.Rows.Count;
				int index = Index;
				for (int i = 0; i < count; i++)
				{
					Table[i, index].IsExistManualInputValue = false;
				}
			}
			_isExistFillFormulaDirty = true;
			Formula = text;
			if (Status == SyncStatus.Synced)
			{
				Dirty.IsFormulaDirty = true;
			}
			try
			{
				TryApplyFormula(rethrow: true);
			}
			catch (FormulaColumnWildcardNoRowException)
			{
			}
			UpdateDependencies();
			Table.Project.FormulaManager.ReplaceHostColumn(new FormulaRecord
			{
				Formula = Formula,
				ObjectId = Id,
				TableId = Table.Id
			});
			Table.NeedSave = true;
			Table.Project.FormulaMapDirty = true;
		}
		catch (FormulaException)
		{
			Formula = formula;
			throw;
		}
		void ClearCrossAttributes()
		{
			foreach (Column column in Table.Columns)
			{
				if (column.CrossAttributes.Role != 0)
				{
					column.TagCrossAttributesDirty();
					column.CrossAttributes.Role = CrossRole.None;
					column.CrossAttributes.SrcColumn = default(Id64);
					column.CrossAttributes.Caption = string.Empty;
				}
			}
		}
	}

	public void UpdateStyle(CellStyle style)
	{
		if (Style != style)
		{
			Table.NeedSave = true;
			Style = style;
			if (Status == SyncStatus.Synced)
			{
				Dirty.IsStyleDirty = true;
			}
		}
	}

	public void UpdateConsolidateAttribs(ConsolidateAttributes ca)
	{
		Table.NeedSave = true;
		ConsolidateAttributes = ca;
		if (Status == SyncStatus.Synced)
		{
			Dirty.IsConsolidateAttribsDirty = true;
		}
	}

	public void UpdateSubtotalAttribs(ColumnSubtotal cs)
	{
		if (SubtotalAttributes != cs)
		{
			Table.NeedSave = true;
			SubtotalAttributes = cs;
			if (Status == SyncStatus.Synced)
			{
				Dirty.IsSubtotalAttribDirty = true;
			}
		}
	}

	public void TagPermissionsDirty()
	{
		Table.NeedSave = true;
		if (Status == SyncStatus.Synced)
		{
			Dirty.IsPermissionsDirty = true;
		}
	}

	public void UpdateCaptionFormula(string cf)
	{
		CaptionFormula = cf;
		Table.NeedSave = true;
		if (Status == SyncStatus.Synced)
		{
			Dirty.IsCaptionFormulaDirty = true;
		}
	}

	public void TagCrossAttributesDirty()
	{
		Table.NeedSave = true;
		if (Status == SyncStatus.Synced)
		{
			Dirty.IsCrossAttributesDirty = true;
		}
	}

	public void EvaluateCaptionFormula()
	{
		if (!string.IsNullOrWhiteSpace(CaptionFormula))
		{
			FormulaEvaluator formulaEvaluator = new FormulaEvaluator(CaptionFormula);
			formulaEvaluator.Env = new FormulaEvaluationEnvironment
			{
				Resolver = new FormulaReferenceModelResolver(Table.Project),
				RefManager = Table.Project.DataReferenceManager,
				RefEvalContext = new DataReferenceEvaluationContext
				{
					Project = Table.Project,
					CurrentTreeNode = Table.TreeNode
				}
			};
			string caption = formulaEvaluator.EvaluateToOperand().ToString();
			UpdateCaption(caption);
		}
	}

	public void Remove()
	{
		Table.Columns.Remove(Index, 1);
	}

	public Leqisoft.DTO.Column ToDto()
	{
		return new Leqisoft.DTO.Column
		{
			Id = Id,
			Width = Width,
			Caption = Caption,
			Dirty = Dirty.ToInt(),
			Index = Index,
			Status = (int)Status,
			TableId = Table.Id,
			Visible = Visible,
			ConsolidateAttribs = SerializeConsolidateAttributes(),
			SubtotalAttribs = (int)SubtotalAttributes,
			Formula = Formula,
			ServerIndex = ServerIndex,
			StyleId = Style?.Id,
			CaptionStyle = CaptionStyle.Serialize(),
			Permissions = Permissions.Serialize(),
			CaptionFormula = CaptionFormula,
			CrossAttributes = CrossAttributes.Serialize()
		};
	}

	public static Column CreateDummyColumn(Table table)
	{
		return new Column
		{
			Table = table
		};
	}

	public static void ResetCostTime()
	{
		_stopWatcher = null;
	}

	public static long GetCostTime()
	{
		if (_stopWatcher == null)
		{
			return -1L;
		}
		return _stopWatcher.ElapsedMilliseconds;
	}

	public IEnumerable<Row> TryApplyFormula(bool rethrow = false, bool evalLqDistinct = true, bool isIgnoreColSheetBadRef = true)
	{
		if (!HasFormula)
		{
			return Enumerable.Empty<Row>();
		}
		FormulaReferenceModelResolver resolver = new FormulaReferenceModelResolver(Table.Project);
		try
		{
			FormulaEvaluationEnvironment formulaEvaluationEnvironment = new FormulaEvaluationEnvironment
			{
				HostTable = Table,
				Resolver = resolver,
				RefManager = Table.Project.DataReferenceManager,
				IsIgnoreColSheetFunBadRefrence = isIgnoreColSheetBadRef,
				RefEvalContext = new DataReferenceEvaluationContext
				{
					Project = Table.Project,
					CurrentTreeNode = Table.TreeNode
				}
			};
			FormulaEvaluator formulaEvaluator = new FormulaEvaluator(Formula)
			{
				Env = formulaEvaluationEnvironment
			};
			IsFillFormula isFillFormula = formulaEvaluator.IsFill();
			if (isFillFormula.IsFill || isFillFormula.IsLqCollect)
			{
				foreach (Cell cell5 in Table.Cells)
				{
					cell5.IsExistManualInputValue = false;
				}
			}
			if (isFillFormula.IsFill)
			{
				if (evalLqDistinct)
				{
					ThrowIfOtherColumnsHaveFillingFormula();
					Operand operand = formulaEvaluator.EvaluateToOperand();
					if (operand is ValueSetOperand { Set: var set })
					{
						if (set != null)
						{
							List<Row> list = new List<Row>();
							HashSet<object> hashSet = new HashSet<object>();
							for (int num = Table.Rows.Count - 1; num >= 0; num--)
							{
								Row row = Table.Rows[num];
								if (row.Role != RowRole.Subtotal && row.Role != RowRole.Total && row.Role != RowRole.Fixed)
								{
									object value = Table[num, Index].Value;
									if (hashSet.Contains(value))
									{
										list.Add(row);
									}
									else
									{
										hashSet.Add(value);
									}
								}
							}
							Table.RemoveRows(list);
							List<Tuple<Row, ValueOperand>> list2 = (from c in GetCells()
								where c.Row.Role == RowRole.Normal || c.Row.Role == RowRole.Among || c.Row.Role == RowRole.Minus
								select Tuple.Create(c.Row, ValueOperand.FromObject(c.Value))).ToList();
							HashSet<Tuple<Row, ValueOperand>> hashSet2 = new HashSet<Tuple<Row, ValueOperand>>(list2, Tuple2Item2Comparer.Instance);
							HashSet<object> nowValues2 = new HashSet<object>(set.Select((Tuple<Row, ValueOperand> tup) => tup.Item2));
							IEnumerable<Tuple<Row, ValueOperand>> source = list2.Where((Tuple<Row, ValueOperand> tup) => !nowValues2.Contains(tup.Item2));
							List<Row> list3 = new List<Row>();
							if (set.Count == 0)
							{
								set.Add(Tuple.Create<Row, ValueOperand>(null, ValueOperand.FromObject(string.Empty)));
							}
							else
							{
								set.ExceptWith(list2);
							}
							int num2 = GetInsertIndex();
							List<Tuple<Row, ValueOperand>> list4 = set.ToList();
							if (list4.Count > 0)
							{
								int num3 = num2;
								Table.Rows.Insert(num2, list4.Count);
								for (int j = 0; j < list4.Count; j++)
								{
									int num4 = num3 + j;
									list3.Add(Table.Rows[num4]);
									Tuple<Row, ValueOperand> tuple = list4[j];
									if (tuple.Item1 != null)
									{
										Table.Rows[num4].UpdateRole(tuple.Item1.Role);
									}
									Table[num4, Index].UpdateValue(tuple.Item2.Evaluate());
								}
							}
							Table table = null;
							list = new List<Row>();
							foreach (Tuple<Row, ValueOperand> item2 in source.Reverse())
							{
								list.Add(item2.Item1);
								table = item2.Item1.Table;
							}
							table?.RemoveRows(list);
							if (isFillFormula.IsLqAsc)
							{
								SortAscending();
							}
							else if (isFillFormula.IsLqDesc)
							{
								SortDescending();
							}
							return list3;
						}
					}
					else
					{
						if (!(operand is CrossTableOperand crossTableOperand))
						{
							throw new FormulaTypeMismatchException();
						}
						if (Table.Rows.Any((Row r) => r.Role == RowRole.Header))
						{
							throw new FormulaNotApplicableException("表格中存在列头类行，不能使用CrossTable函数");
						}
						DataTable dt = crossTableOperand.DataTables[0].Item2;
						List<Row> list5 = new List<Row>();
						HashSet<object> hashSet3 = new HashSet<object>();
						for (int num5 = Table.Rows.Count - 1; num5 >= 0; num5--)
						{
							Row row2 = Table.Rows[num5];
							if (row2.Role != RowRole.Subtotal && row2.Role != RowRole.Total && row2.Role != RowRole.Fixed)
							{
								string item = Table[num5, Index].GetDisplayValue().Trim();
								if (hashSet3.Contains(item))
								{
									list5.Add(row2);
								}
								else
								{
									hashSet3.Add(item);
								}
							}
						}
						Table.RemoveRows(list5);
						IEnumerable<Tuple<Row, ValueOperand>> collection = from c in GetCells()
							where c.Row.Role == RowRole.Normal || c.Row.Role == RowRole.Among || c.Row.Role == RowRole.Minus
							select Tuple.Create(c.Row, ValueOperand.FromObject(c.Value));
						HashSet<Tuple<Row, ValueOperand>> source2 = new HashSet<Tuple<Row, ValueOperand>>(collection, Tuple2Item2Comparer.Instance);
						List<object> collection2 = (from i in Enumerable.Range(0, dt.Rows.Count - 1)
							select dt.Rows[i][0]).ToList();
						HashSet<object> nowValues = new HashSet<object>(collection2);
						IEnumerable<Tuple<Row, ValueOperand>> source3 = source2.Where((Tuple<Row, ValueOperand> tup) => !nowValues.Contains(tup.Item2.Object));
						Table table2 = null;
						list5 = new List<Row>();
						foreach (Tuple<Row, ValueOperand> item3 in source3.Reverse())
						{
							list5.Add(item3.Item1);
							table2 = item3.Item1.Table;
						}
						table2?.RemoveRows(list5);
						List<Row> list6 = new List<Row>();
						nowValues.ExceptWith(source2.Select((Tuple<Row, ValueOperand> tup) => tup.Item2.Object));
						int num6 = Table.Rows.FirstOrDefault((Row r) => r.Role == RowRole.Fixed || r.Role == RowRole.Header || r.Role == RowRole.Subtotal || r.Role == RowRole.Total)?.Index ?? 0;
						List<object> list7 = nowValues.ToList();
						if (list7.Count > 0)
						{
							int num7 = num6;
							Table.Rows.Insert(num6, list7.Count);
							for (int k = 0; k < list7.Count; k++)
							{
								int num8 = num7 + k;
								list6.Add(Table.Rows[num8]);
								object value2 = list7[k];
								Table[num8, Index].UpdateValue(value2);
							}
							num6 += list7.Count;
						}
						foreach (Column column in Table.Columns.Where((Column c) => c.CrossAttributes.Role != CrossRole.None).ToList())
						{
							if (!crossTableOperand.DataTables.Any((Tuple<Column, DataTable> tup) => tup.Item1.Id == column.CrossAttributes.SrcColumn))
							{
								column.Remove();
							}
						}
						for (int num9 = crossTableOperand.DataTables.Count - 1; num9 >= 0; num9--)
						{
							Column srcColumn = crossTableOperand.DataTables[num9].Item1;
							dt = crossTableOperand.DataTables[num9].Item2;
							List<string> collection3 = (from i in Enumerable.Range(1, dt.Columns.Count - 2)
								select dt.Columns[i].Caption).ToList();
							HashSet<Column> source4 = new HashSet<Column>(Table.Columns.Where((Column c) => c.CrossAttributes.Role == CrossRole.Data && c.CrossAttributes.SrcColumn == srcColumn.Id));
							HashSet<string> nowCaptions = new HashSet<string>(collection3);
							IEnumerable<Column> enumerable = source4.Where((Column c) => !nowCaptions.Contains(c.CrossAttributes.Caption));
							foreach (Column item4 in enumerable)
							{
								item4.Remove();
							}
							nowCaptions.ExceptWith(source4.Select((Column c) => c.CrossAttributes.Caption));
							num6 = Index + 1;
							List<Column> list8 = new List<Column>();
							foreach (string item5 in nowCaptions)
							{
								Table.Columns.Insert(num6, 1);
								Column column2 = Table.Columns[num6];
								list8.Add(column2);
								CellStyle style = Table.CellStyles.MutateAndGet(column2.Style, delegate(CellStyle s)
								{
									s.DataType = typeof(double);
									s.Format = new DataFormat(DataFormatType.Comma)
									{
										DecimalLength = 2
									};
									s.Align = CellTextAlign.MiddleRight;
								});
								column2.UpdateStyle(style);
								column2.UpdateCaption(srcColumn.CaptionDisplay + "_" + item5);
								column2.TagCrossAttributesDirty();
								column2.CrossAttributes.Role = CrossRole.Data;
								column2.CrossAttributes.Caption = item5;
								column2.CrossAttributes.SrcColumn = srcColumn.Id;
								num6++;
							}
							Column column3 = Table.Columns.FirstOrDefault((Column c) => c.CrossAttributes.Role == CrossRole.Sum && c.CrossAttributes.SrcColumn == srcColumn.Id);
							if (column3 == null)
							{
								Table.Columns.Insert(num6, 1);
								column3 = Table.Columns[num6];
								CellStyle style2 = Table.CellStyles.MutateAndGet(column3.Style, delegate(CellStyle s)
								{
									s.DataType = typeof(double);
									s.Format = new DataFormat(DataFormatType.Comma)
									{
										DecimalLength = 2
									};
									s.Align = CellTextAlign.MiddleRight;
								});
								column3.UpdateStyle(style2);
								column3.UpdateCaption(srcColumn.CaptionDisplay + "_" + dt.Columns[dt.Columns.Count - 1].Caption);
								column3.TagCrossAttributesDirty();
								column3.CrossAttributes.Role = CrossRole.Sum;
								column3.CrossAttributes.SrcColumn = srcColumn.Id;
								num6++;
							}
							else
							{
								Row row3 = Table.Rows.FirstOrDefault((Row r) => r.Role == RowRole.Total);
								if (row3 != null)
								{
									Cell cell = Table[row3.Index, column3.Index];
									if (cell.HasFormula)
									{
										FormulaEvaluator formulaEvaluator2 = new FormulaEvaluator(cell.Formula);
										FormulaReferenceModelResolver formulaReferenceModelResolver = new FormulaReferenceModelResolver(Project.Current);
										foreach (Column item6 in list8)
										{
											Cell cell2 = Table[row3.Index, item6.Index];
											string text = formulaEvaluator2.PasteFormula(resolver, cell, cell2);
											cell2.UpdateFormula(text);
										}
									}
								}
							}
							Dictionary<string, int> dictionary = (from c in GetCells()
								where c.Row.Role == RowRole.Normal || c.Row.Role == RowRole.Among || c.Row.Role == RowRole.Minus
								select c).ToDictionary((Cell c) => c.GetDisplayValue().Trim(), (Cell c) => c.Row.Index);
							List<IGrouping<string, Column>> list9 = (from c in Table.Columns
								where c.CrossAttributes.Role == CrossRole.Data && c.CrossAttributes.SrcColumn == srcColumn.Id
								group c by c.CrossAttributes.Caption).ToList();
							foreach (IGrouping<string, Column> item7 in list9)
							{
								foreach (Column item8 in item7.Skip(1).ToList())
								{
									item8.Remove();
								}
							}
							Dictionary<string, int> dictionary2 = list9.Select((IGrouping<string, Column> g) => g.First()).ToDictionary((Column c) => c.CrossAttributes.Caption, (Column c) => c.Index);
							for (int l = 0; l < dt.Rows.Count - 1; l++)
							{
								for (int m = 1; m <= dt.Columns.Count - 2; m++)
								{
									Table[dictionary[(string)dt.Rows[l][0]], dictionary2[dt.Columns[m].Caption]].UpdateValue(ZeroDBNull(dt.Rows[l][m]));
								}
							}
							for (int n = 0; n < dt.Rows.Count - 1; n++)
							{
								Table[dictionary[(string)dt.Rows[n][0]], column3.Index].UpdateValue(dt.Rows[n][dt.Columns.Count - 1]);
							}
						}
					}
				}
			}
			else
			{
				Operand operand2 = formulaEvaluator.EvaluateToOperand();
				if (operand2 is CellsOperand { IsCollectFill: not false } cellsOperand)
				{
					if (evalLqDistinct)
					{
						if (cellsOperand.Cells.Count == 0)
						{
							cellsOperand.Cells.Add(new Cell
							{
								Value = string.Empty
							});
						}
						int count = cellsOperand.Cells.Count;
						List<Row> list10 = Table.Rows.Where((Row r) => r.Role == RowRole.Header || r.Role == RowRole.Subtotal || r.Role == RowRole.Total || r.Role == RowRole.Fixed).ToList();
						int num10 = Table.Rows.Count;
						foreach (Row item9 in Enumerable.Reverse(list10))
						{
							Table.Rows.Move(item9.Index, 1, num10);
							num10--;
						}
						int num11 = Table.Rows.Count - list10.Count;
						if (count > num11)
						{
							Table.Rows.Insert(num10, count - num11);
						}
						else if (count < num11)
						{
							Table.Rows.Remove(count, num11 - count);
						}
						for (int num12 = 0; num12 < count; num12++)
						{
							Cell cell3 = Table[num12, Index];
							if (cell3 != null && cell3.IsAllowUseColumnFormulaResultUpdateCellValue)
							{
								cell3.UpdateValue(cellsOperand.Cells[num12].Value);
							}
						}
					}
				}
				else
				{
					int index = Index;
					int count2 = Table.Rows.Count;
					for (int num13 = 0; num13 < count2; num13++)
					{
						Cell cell4 = Table[num13, index];
						if (cell4 != null && cell4.IsAllowUseColumnFormulaResultUpdateCellValue)
						{
							formulaEvaluationEnvironment.RowIndex = num13;
							object value3 = formulaEvaluator.Evaluate();
							cell4.UpdateValue(value3);
						}
					}
			}
			}
		}
		catch (FormulaException)
		{
			if (rethrow)
			{
				throw;
			}
		}
		catch (ArgumentOutOfRangeException)
		{
			if (rethrow)
			{
				throw;
			}
		}
		catch (Exception)
		{
			if (rethrow)
			{
				throw;
			}
		}
		return Enumerable.Empty<Row>();
		int GetInsertIndex()
		{
			Row row4 = Table.Rows.FirstOrDefault((Row r) => r.Role == RowRole.Fixed || r.Role == RowRole.Header || r.Role == RowRole.Subtotal || r.Role == RowRole.Total);
			if (row4 == null)
			{
				return 0;
			}
			if (row4.Index == 0)
			{
				return 1;
			}
			return row4.Index;
		}
		static object ZeroDBNull(object dbnull)
		{
			if (dbnull != DBNull.Value)
			{
				return dbnull;
			}
			return 0.0;
		}
	}

	public void TryApplyFormulaToRows(List<int> rows)
	{
		if (rows.Count == 0 || !HasFormula)
		{
			return;
		}
		FormulaReferenceModelResolver resolver = new FormulaReferenceModelResolver(Table.Project);
		FormulaEvaluationEnvironment formulaEvaluationEnvironment = new FormulaEvaluationEnvironment
		{
			Resolver = resolver,
			RefManager = Table.Project.DataReferenceManager,
			RefEvalContext = new DataReferenceEvaluationContext
			{
				Project = Table.Project,
				CurrentTreeNode = Table.TreeNode
			}
		};
		try
		{
			int index = Index;
			int count = rows.Count;
			FormulaEvaluator formulaEvaluator = new FormulaEvaluator(Formula)
			{
				Env = formulaEvaluationEnvironment
			};
			for (int i = 0; i < count; i++)
			{
				Cell cell = Table[rows[i], index];
				if (cell != null && cell.IsAllowUseColumnFormulaResultUpdateCellValue)
				{
					formulaEvaluationEnvironment.RowIndex = rows[i];
					formulaEvaluationEnvironment.HostTable = Table;
					cell.UpdateValue(formulaEvaluator.Evaluate());
				}
			}
		}
		catch (FormulaException)
		{
		}
		catch (ArgumentOutOfRangeException)
		{
		}
		catch (Exception)
		{
		}
	}

	public void SetSynced()
	{
		Status = SyncStatus.Synced;
		Dirty = default(ColumnDirtyMask);
		ServerIndex = Index;
	}

	public string GetUniqueFormulaName()
	{
		int num = 1;
		int num2 = 0;
		string formulaCanonicalCaption = GetFormulaCanonicalCaption(CaptionDisplay);
		foreach (Column column in Table.Columns)
		{
			string formulaCanonicalCaption2 = GetFormulaCanonicalCaption(column.CaptionDisplay);
			if (formulaCanonicalCaption2 == formulaCanonicalCaption)
			{
				num2++;
			}
			if (column == this)
			{
				num = num2;
			}
		}
		foreach (Row item in from r in Table.HeaderRowCache
			where r.Role == RowRole.Header
			orderby r.Index
			select r)
		{
			for (int i = 0; i < Table.Columns.Count; i++)
			{
				string formulaCanonicalCaption3 = GetFormulaCanonicalCaption(Table[item.Index, i].GetDisplayValue());
				if (formulaCanonicalCaption3 == formulaCanonicalCaption)
				{
					num2++;
				}
			}
		}
		if (num2 != 1)
		{
			return $"{formulaCanonicalCaption}_{num}";
		}
		return formulaCanonicalCaption;
	}

	public void SortAscending()
	{
		Row row2 = Table.Rows.FirstOrDefault((Row r) => r.Role == RowRole.Normal || r.Role == RowRole.Minus || r.Role == RowRole.Among);
		if (row2 != null)
		{
			int index = row2.Index;
			int num = Table.Rows.Skip(index + 1).FirstOrDefault((Row r) => r.Role == RowRole.Fixed || r.Role == RowRole.Header || r.Role == RowRole.Subtotal || r.Role == RowRole.Total)?.Index ?? Table.Rows.Count;
			List<int> pickupOrder = (from r in Table.Rows.Skip(index).Take(num - index).OrderByCellValue((Row row) => Table[row.Index, Index].Value)
				select r.Index).ToList();
			Table.Rows.Reorder(index, pickupOrder);
		}
	}

	public void SortDescending()
	{
		Row row2 = Table.Rows.FirstOrDefault((Row r) => r.Role == RowRole.Normal || r.Role == RowRole.Minus || r.Role == RowRole.Among);
		if (row2 != null)
		{
			int index = row2.Index;
			int num = Table.Rows.Skip(index + 1).FirstOrDefault((Row r) => r.Role == RowRole.Fixed || r.Role == RowRole.Header || r.Role == RowRole.Subtotal || r.Role == RowRole.Total)?.Index ?? Table.Rows.Count;
			List<int> pickupOrder = (from r in Table.Rows.Skip(index).Take(num - index).OrderByCellValueDescending((Row row) => Table[row.Index, Index].Value)
				select r.Index).ToList();
			Table.Rows.Reorder(index, pickupOrder);
		}
	}

	public Type GetDataType()
	{
		return Style?.DataType ?? Table.DefaultStyle.DataType;
	}

	public DataFormat GetFormat()
	{
		return (Style?.Format ?? Table.DefaultStyle.Format).Value;
	}

	public override string ToString()
	{
		return $"Id={Id} Caption={Caption}";
	}

	internal void UpdateDependencies()
	{
		IsFormulaRefSelfTableColumnOrCell = false;
		Table._formulaTriggers.RemoveWhere((FormulaTrigger ft) => ft.DstColumn == this);
		if (!HasFormula)
		{
			return;
		}
		FormulaReferenceModelResolver resolver = new FormulaReferenceModelResolver(Table.Project);
		try
		{
			FormulaEvaluator formulaEvaluator = new FormulaEvaluator(Formula);
			FormulaReferences references = formulaEvaluator.GetReferences(resolver);
			foreach (Cell cellReference in references.CellReferences)
			{
				Table._formulaTriggers.Add(new FormulaTrigger
				{
					Table = Table,
					Kind = FormulaTriggerKind.Cell_Column,
					SrcCell = cellReference,
					DstColumn = this
				});
				if (cellReference._Table == Table)
				{
					IsFormulaRefSelfTableColumnOrCell = true;
				}
			}
			foreach (RangeOperand rangeReference in references.RangeReferences)
			{
				Table._formulaTriggers.Add(new FormulaTrigger
				{
					Table = Table,
					Kind = FormulaTriggerKind.Range_Column,
					SrcRange = rangeReference,
					DstColumn = this
				});
				if (rangeReference.Table == Table)
				{
					IsFormulaRefSelfTableColumnOrCell = true;
				}
			}
			foreach (Column columnWildcardReference in references.ColumnWildcardReferences)
			{
				Table._formulaTriggers.Add(new FormulaTrigger
				{
					Table = Table,
					Kind = FormulaTriggerKind.ColumnWildcard_Column,
					SrcColumn = columnWildcardReference,
					DstColumn = this
				});
				if (columnWildcardReference.Table == Table)
				{
					IsFormulaRefSelfTableColumnOrCell = true;
				}
			}
			foreach (Column columnReference in references.ColumnReferences)
			{
				Table._formulaTriggers.Add(new FormulaTrigger
				{
					Table = Table,
					Kind = FormulaTriggerKind.Column_Column,
					SrcColumn = columnReference,
					DstColumn = this
				});
				if (columnReference.Table == Table)
				{
					IsFormulaRefSelfTableColumnOrCell = true;
				}
			}
			foreach (Cell headerCellReference in references.HeaderCellReferences)
			{
				Table._formulaTriggers.Add(new FormulaTrigger
				{
					Table = Table,
					Kind = FormulaTriggerKind.HeaderCell_Column,
					SrcHeaderCell = headerCellReference,
					DstColumn = this
				});
				if (headerCellReference._Table == Table)
				{
					IsFormulaRefSelfTableColumnOrCell = true;
				}
			}
			foreach (Cell headerCellWildcardReference in references.HeaderCellWildcardReferences)
			{
				Table._formulaTriggers.Add(new FormulaTrigger
				{
					Table = Table,
					Kind = FormulaTriggerKind.HeaderCellWildcard_Column,
					SrcHeaderCell = headerCellWildcardReference,
					DstColumn = this
				});
				if (headerCellWildcardReference._Table == Table)
				{
					IsFormulaRefSelfTableColumnOrCell = true;
				}
			}
		}
		catch (FormulaException)
		{
		}
	}

	internal Column Duplicate()
	{
		Column column = new Column
		{
			Id = Project.Current.GetNextId(),
			Caption = Caption,
			Status = SyncStatus.New,
			Visible = Visible,
			Width = Width,
			CaptionStyle = CaptionStyle.Duplicate(),
			Index = Index,
			SubtotalAttributes = SubtotalAttributes,
			Formula = string.Empty,
			CaptionFormula = string.Empty
		};
		column.CrossAttributes.Deserialize(CrossAttributes.Serialize());
		return column;
	}

	internal Column Clone()
	{
		return (Column)MemberwiseClone();
	}

	internal static string GetFormulaCanonicalCaption(string s)
	{
		string text = reInvalidChars.Replace(s, string.Empty);
		if (text == "")
		{
			text = "_";
		}
		return reBeginWithDigit.Replace(text, "_$&");
	}

	private string GetFixedCaption()
	{
		return GetExcelColumnName(Index);
	}

	public static string GetExcelColumnName(int columnNumber)
	{
		int num = columnNumber + 1;
		string text = string.Empty;
		while (num > 0)
		{
			int num2 = (num - 1) % 26;
			text = Convert.ToChar(65 + num2) + text;
			num = (num - num2) / 26;
		}
		return text;
	}

	public static int ExcelColumnNameToNumber(string columnName)
	{
		if (string.IsNullOrEmpty(columnName))
		{
			throw new ArgumentNullException("columnName");
		}
		columnName = columnName.ToUpperInvariant();
		int num = 0;
		for (int i = 0; i < columnName.Length; i++)
		{
			num *= 26;
			num += columnName[i] - 65 + 1;
		}
		return num - 1;
	}

	private void ThrowIfOtherColumnsHaveFillingFormula()
	{
		foreach (Column column in Table.Columns)
		{
			if (column != this && column.HasFormula)
			{
				bool isFill;
				try
				{
					FormulaEvaluator formulaEvaluator = new FormulaEvaluator(column.Formula);
					isFill = formulaEvaluator.IsFill().IsFill;
				}
				catch (FormulaException)
				{
					break;
				}
				if (isFill)
				{
					throw new FormulaNotApplicableException("同一表格内最多只能有一列填充型公式");
				}
			}
		}
	}
}
