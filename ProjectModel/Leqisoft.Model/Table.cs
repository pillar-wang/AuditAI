﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Leqisoft.DTO;
using Leqisoft.Util;

namespace Leqisoft.Model;

public class Table
{
	[CompilerGenerated]
	private sealed class _003CEnumerateCellRange_003Ed__188 : IEnumerable<Cell>, IEnumerable, IEnumerator<Cell>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private Cell _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private int topRow;

		public int _003C_003E3__topRow;

		private int leftCol;

		public int _003C_003E3__leftCol;

		public Table _003C_003E4__this;

		private int rightCol;

		public int _003C_003E3__rightCol;

		private int bottomRow;

		public int _003C_003E3__bottomRow;

		private int _003Ci_003E5__2;

		private int _003Cj_003E5__3;

		Cell IEnumerator<Cell>.Current
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
		public _003CEnumerateCellRange_003Ed__188(int _003C_003E1__state)
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
			Table table = _003C_003E4__this;
			if (num != 0)
			{
				if (num != 1)
				{
					return false;
				}
				_003C_003E1__state = -1;
				_003Cj_003E5__3++;
				goto IL_0077;
			}
			_003C_003E1__state = -1;
			_003Ci_003E5__2 = topRow;
			goto IL_0095;
			IL_0077:
			if (_003Cj_003E5__3 <= rightCol)
			{
				_003C_003E2__current = table.Cells.Get(_003Ci_003E5__2, _003Cj_003E5__3);
				_003C_003E1__state = 1;
				return true;
			}
			_003Ci_003E5__2++;
			goto IL_0095;
			IL_0095:
			if (_003Ci_003E5__2 <= bottomRow)
			{
				_003Cj_003E5__3 = leftCol;
				goto IL_0077;
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
		IEnumerator<Cell> IEnumerable<Cell>.GetEnumerator()
		{
			_003CEnumerateCellRange_003Ed__188 _003CEnumerateCellRange_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CEnumerateCellRange_003Ed__ = this;
			}
			else
			{
				_003CEnumerateCellRange_003Ed__ = new _003CEnumerateCellRange_003Ed__188(0)
				{
					_003C_003E4__this = _003C_003E4__this
				};
			}
			_003CEnumerateCellRange_003Ed__.topRow = _003C_003E3__topRow;
			_003CEnumerateCellRange_003Ed__.leftCol = _003C_003E3__leftCol;
			_003CEnumerateCellRange_003Ed__.bottomRow = _003C_003E3__bottomRow;
			_003CEnumerateCellRange_003Ed__.rightCol = _003C_003E3__rightCol;
			return _003CEnumerateCellRange_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<Cell>)this).GetEnumerator();
		}
	}

	private readonly object _syncLock = new object();

	private const int MAX_TABLE_SIZE = 1000000000;

	public const int MAX_ROWS_COUNT = 1000000;

	public bool _loaded;

	protected string _collectSource;

	internal bool _isFormulaDependenciesLoaded;

	internal HashSet<FormulaTrigger> _formulaTriggers = new HashSet<FormulaTrigger>();

	internal bool _isBatchUpdating;

	internal HashSet<Cell> _batchUpdatingCells = new HashSet<Cell>();

	internal HashSet<object> _formulaExecuted = new HashSet<object>();

	internal List<int> _dbRowSlots = new List<int>();

	public TableDirtyMask Dirty;

	protected HashSet<Row> AllowEditRows;

	internal HashSet<Id64> RemovedColumns { get; } = new HashSet<Id64>();


	internal HashSet<Id64> RemovedRows { get; } = new HashSet<Id64>();


	internal HashSet<Id64> RemovedCells { get; } = new HashSet<Id64>();


	internal HashSet<Id64> ColumnsToDelete { get; } = new HashSet<Id64>();


	internal HashSet<Id64> RowsToDelete { get; } = new HashSet<Id64>();


	internal HashSet<Id64> CellsToDelete { get; } = new HashSet<Id64>();


	internal int DbRowsCount { get; private set; }

	public virtual Id64 Id => TreeNode.Id;

	public ColumnCollection Columns { get; }

	public RowCollection Rows { get; }

	public CellCollection Cells { get; }

	public CellStylePool CellStyles { get; }

	public virtual string CollectSource
	{
		get
		{
			return _collectSource;
		}
		set
		{
			_collectSource = value ?? string.Empty;
		}
	}

	public Project Project => TreeNode.Project;

	public virtual TableTitle Title { get; }

	public TableFoot Foot { get; }

	public string Note { get; set; }

	public TableCommandsManager CommandsManager { get; }

	public PageSetup PageSetup { get; } = new PageSetup();


	public TicketTable Ticket { get; }

	public int Version => TreeNode.Version;

	public bool LocalExists => Version != -1;

	public TreeTableNode TreeNode { get; set; }

	public int FrozenCols { get; set; }

	public TableBorderStyle BorderStyle { get; set; }

	/// <summary>自定义表格边框样式的 JSON（仅当使用自定义样式时填充）</summary>
	public string CustomBorderStyle { get; set; }

	public Cell this[int row, int col] => Cells.Get(row, col);

	public SubTotal SubTotal { get; }

	public bool NeedSave { get; set; }

	public HashSet<CellMerge> MergedCells { get; } = new HashSet<CellMerge>();


	public HashSet<Id64> RemovedMerges { get; } = new HashSet<Id64>();


	public HashSet<Id64> MergesToDelete { get; } = new HashSet<Id64>();


	public HashSet<Row> HeaderRowCache { get; } = new HashSet<Row>();


	public int[] HeaderHeights { get; set; } = Enumerable.Empty<int>().ToArray();


	public ConsolidateSettings ConsolidateSettings { get; internal set; } = new ConsolidateSettings();


	public CellStyle DefaultStyle { get; internal set; }

	public TableHeaderMode HeaderMode { get; set; }

	public long Locker { get; set; }

	public string FilterInfo { get; set; }

	public bool RowOwnerExclusive => TreeNode.RowWrite;

	public bool RowOwnerLoad => TreeNode.RowRead;

	public RowOwnerLoadShare RowOwnerLoadShare { get; } = new RowOwnerLoadShare();


	public CellPropManager CellPropManager { get; private set; }

	public string ControlFormula { get; set; } = "";


	public bool HasControlFormula => !string.IsNullOrEmpty(ControlFormula);

	public bool CanReload => Project.Dal.GetTable(Id) != null;

	public bool IsCorrupted { get; private set; }

	public bool EnableFormulaTrigger { get; set; } = true;


	public bool IsLocked
	{
		get
		{
			if (Locker == 0L)
			{
				return !TreeNode.HasWritePermission();
			}
			return true;
		}
	}

	public HashSet<Cell> ControlRemindCells { get; } = new HashSet<Cell>();


	public HashSet<Cell> ControlWarningCells { get; } = new HashSet<Cell>();


	public HashSet<Cell> ControlLockCells { get; } = new HashSet<Cell>();


	public HashSet<Row> ControlLockRows { get; } = new HashSet<Row>();


	public Dictionary<Cell, Color> ControlForeColorCells { get; } = new Dictionary<Cell, Color>();


	public Dictionary<Cell, Color> ControlBackColorCells { get; } = new Dictionary<Cell, Color>();


	public Table()
	{
		Title = new TableTitle(this);
		Foot = new TableFoot(this);
		Columns = new ColumnCollection(this);
		Rows = new RowCollection(this);
		Cells = new CellCollection(this);
		SubTotal = new SubTotal(this);
		CellStyles = new CellStylePool(this);
		CommandsManager = new TableCommandsManager(this);
		CellPropManager = new CellPropManager(this);
		Ticket = new TicketTable(this);
	}

	public string GetCanonicalName()
	{
		return TreeNode.FormulaUniqueName;
	}

	public int GetHeaderHeight(int index)
	{
		if (index >= HeaderHeights.Length)
		{
			return UserSet.Config.TableStyle.SubTitleHeight;
		}
		return HeaderHeights[index];
	}

	public int SumHeaderHeight(int count)
	{
		return Enumerable.Range(0, count).Select(GetHeaderHeight).Sum();
	}

	public void SetHeaderHeight(int index, int value)
	{
		if (index >= HeaderHeights.Length)
		{
			int[] array = Enumerable.Repeat(-1, index + 1).ToArray();
			Array.Copy(HeaderHeights, array, HeaderHeights.Length);
			HeaderHeights = array;
		}
		HeaderHeights[index] = value;
	}

	public void LoadRowOwnerLoadView()
	{
		if (!IsManager())
		{
			Rows._list.RemoveAll((Row r) => !CanLoad(r));
			HeaderRowCache.RemoveWhere((Row r) => !CanLoad(r));
		}
		_dbRowSlots.Clear();
		_dbRowSlots.AddRange(Rows.Select((Row r) => r.Index));
		Rows.ResetIndex();
		HashSet<Id64> rowSet = new HashSet<Id64>(Rows.Select((Row r) => r.Id));
		List<Cell> collection = (from c in Cells
			where rowSet.Contains(c.Row.Id)
			orderby c.Row.Index, c.Column.Index
			select c).ToList();
		Cells._list.Clear();
		Cells._list.AddRange(collection);
	}

	protected void ResetTitleCellInstance(int rowIndex, int colIndex, object value)
	{
		Title.ResetTitleCellInstance(rowIndex, colIndex, value);
	}

	public virtual Table LoadAndReturn(bool bypassRowOwnerLoad = false)
	{
		lock (_syncLock)
		{
			try
			{
				if (!_loaded && !IsCorrupted)
				{
					if (!LocalExists)
					{
						_loaded = true;
						return this;
					}
					CellStyles.Clear();
					Columns.Clear();
					Rows.Clear();
					Cells.Clear();
					MergedCells.Clear();
					RemovedMerges.Clear();
					RemovedColumns.Clear();
					RemovedRows.Clear();
					RemovedCells.Clear();
					ColumnsToDelete.Clear();
					RowsToDelete.Clear();
					CellsToDelete.Clear();
					MergesToDelete.Clear();
					HeaderRowCache.Clear();
					_isFormulaDependenciesLoaded = false;
					_formulaTriggers.Clear();
					CellPropManager.DicCellAttachments.Clear();
					Leqisoft.DTO.Table table = Project.Dal.GetTable(Id);
					if (table == null)
					{
						IsCorrupted = true;
						return this;
					}
					Dirty = new TableDirtyMask(table.Dirty);
					Title.Deserialize(table.Title);
					PageSetup.Deserialize(table.PageSetup);
					HeaderHeights = DeserializeHeaderHeights(table.HeaderHeights);
					ConsolidateSettings = ConsolidateSettings.Deserialize(table.ConsolidateSettings);
					BorderStyle = TableBorderStyles.FromNumber(table.BorderStyle);
					if (!string.IsNullOrEmpty(table.CustomBorderStyle))
					{
						BorderStyle = TableBorderStyle.FromJson(table.CustomBorderStyle);
						CustomBorderStyle = table.CustomBorderStyle;
					}
					else
					{
						CustomBorderStyle = null;
					}
					FrozenCols = table.FrozenCols;
					HeaderMode = (TableHeaderMode)table.HeaderMode;
					CollectSource = table.CollectSource;
					Locker = table.Locker;
					FilterInfo = table.FilterInfo;
					Foot.Deserialize(table.Foot);
					RowOwnerLoadShare.Deserialize(table.RowOwnerLoadShare);
					Ticket.Deserialize(table.Ticket);
					Ticket.IsCacheExpired = true;
					ControlFormula = table.ControlFormula;
					Dictionary<Id64, CellStyle> dictionary = new Dictionary<Id64, CellStyle>();
					foreach (Leqisoft.DTO.CellStyle cellStyle2 in Project.Dal.GetCellStyles(Id))
					{
						CellStyle cellStyle = new CellStyle
						{
							Id = cellStyle2.Id,
							FontSize = cellStyle2.FontSize,
							BackColor = cellStyle2.BackColor.ToNullableColor(),
							ForeColor = cellStyle2.ForeColor.ToNullableColor(),
							FontFamily = cellStyle2.FontFamily,
							Align = (CellTextAlign?)cellStyle2.Align,
							Margin = cellStyle2.Margin,
							Bold = cellStyle2.Bold,
							Italic = cellStyle2.Italic,
							Underline = cellStyle2.Underline,
							Format = DataFormat.Parse(cellStyle2.Format),
							Locker = cellStyle2.Locked,
							Status = (SyncStatus)cellStyle2.Status,
							DataType = Util.NullableIntToDataType(cellStyle2.DataType),
							DefaultValue = cellStyle2.DefaultValue,
							Comment = cellStyle2.Comment
						};
						dictionary.Add(cellStyle.Id, cellStyle);
						CellStyles.Add(cellStyle);
					}
					DefaultStyle = dictionary[table.DefaultStyleId];
					Dictionary<Id64, Column> dictionary2 = new Dictionary<Id64, Column>();
					foreach (Leqisoft.DTO.Column column2 in Project.Dal.GetColumns(Id))
					{
						Column column = new Column
						{
							Table = this,
							Id = column2.Id,
							ServerIndex = column2.ServerIndex,
							Status = (SyncStatus)column2.Status,
							Caption = column2.Caption,
							Width = column2.Width,
							Visible = column2.Visible,
							Formula = column2.Formula,
							ConsolidateAttributes = ConsolidateAttributes.Deserialize(column2.ConsolidateAttribs),
							SubtotalAttributes = (ColumnSubtotal)column2.SubtotalAttribs,
							CaptionFormula = column2.CaptionFormula,
							Dirty = new ColumnDirtyMask(column2.Dirty)
						};
						column.Permissions.Deserialize(column2.Permissions);
						column.CaptionStyle.Deserialize(column2.CaptionStyle);
						column.CrossAttributes.Deserialize(column2.CrossAttributes);
						if (column2.StyleId.HasValue)
						{
							column.Style = dictionary[column2.StyleId.Value];
						}
						Columns._list.Add(column);
						dictionary2.Add(column2.Id, column);
					}
					Columns.ResetIndex();
					Dictionary<Id64, Row> dictionary3 = new Dictionary<Id64, Row>();
					foreach (Leqisoft.DTO.Row row2 in Project.Dal.GetRows(Id))
					{
						Row row = new Row
						{
							Table = this,
							Id = row2.Id,
							ServerIndex = row2.ServerIndex,
							Status = (SyncStatus)row2.Status,
							Visible = row2.Visible,
							Height = row2.Height,
							Locker = row2.Locked,
							Role = (RowRole)row2.Role,
							Creator = row2.Creator,
							NeedSave = false,
							Dirty = new RowDirtyMask(row2.Dirty)
						};
						row.Permissions.Deserialize(row2.Permissions);
						Rows._list.Add(row);
						dictionary3.Add(row2.Id, row);
						if (row.Role == RowRole.Header || row.Role == RowRole.Fixed)
						{
							HeaderRowCache.Add(row);
						}
					}
					DbRowsCount = Rows.Count;
					Rows.ResetIndex();
					List<Leqisoft.DTO.Cell> list = Project.Dal.GetCells(Id).ToList();
					foreach (Leqisoft.DTO.Cell item in list)
					{
						Cell cell = new Cell
						{
							Row = dictionary3[item.RowId],
							Column = dictionary2[item.ColumnId],
							Id = item.Id,
							Value = item.Value.Value,
							Dirty = new CellDirtyMask(item.Dirty),
							Status = (SyncStatus)item.Status,
							Formula = item.Formula,
							CollectSource = item.CollectSource,
							HeaderFormula = item.HeaderFormula
						};
						if (item.StyleId.HasValue)
						{
							cell.Style = dictionary[item.StyleId.Value];
						}
						cell.DeserializeCellPrivateData(item.Value.AdditionalData);
						Cells._list.Add(cell);
					}
					foreach (CellProp cellProp in Project.Dal.GetCellProps(Id))
					{
						CellAttachments cellAttachments = new CellAttachments();
						cellAttachments.Deserialize(cellProp.Attachments);
						cellAttachments.Dirty = cellProp.Dirty == 1;
						cellAttachments.Status = (SyncStatus)cellProp.Status;
						CellPropManager.DicCellAttachments.Add(cellProp.CellId, cellAttachments);
					}
					if (!bypassRowOwnerLoad && ShouldRowOwnerLoad())
					{
						LoadRowOwnerLoadView();
					}
					if (Rows.Count * Columns.Count != Cells.Count)
					{
						IsCorrupted = true;
						return this;
					}
					foreach (Merge dtoM in Project.Dal.GetMerges(Id))
					{
						CellMerge cellMerge = new CellMerge();
						cellMerge.Id = new Id64(dtoM.Id);
						cellMerge.Status = (SyncStatus)dtoM.Status;
						cellMerge.TopLeft = Cells.FirstOrDefault((Cell c) => c.Id.Value == dtoM.TopLeft);
						cellMerge.BottomRight = Cells.FirstOrDefault((Cell c) => c.Id.Value == dtoM.BottomRight);
						if (cellMerge.TopLeft != null && cellMerge.BottomRight != null)
						{
							MergedCells.Add(cellMerge);
						}
					}
					foreach (Id64 localRemovedMerge in Project.Dal.GetLocalRemovedMerges(Id))
					{
						RemovedMerges.Add(localRemovedMerge);
					}
					foreach (Id64 localRemovedColumn in Project.Dal.GetLocalRemovedColumns(Id))
					{
						RemovedColumns.Add(localRemovedColumn);
					}
					foreach (Id64 localRemovedRow in Project.Dal.GetLocalRemovedRows(Id))
					{
						RemovedRows.Add(localRemovedRow);
					}
					foreach (Id64 localRemovedCell in Project.Dal.GetLocalRemovedCells(Id))
					{
						RemovedCells.Add(localRemovedCell);
					}
					_loaded = true;
					IsCorrupted = false;
					EvalControlFormula();
				}
			}
			catch
			{
				IsCorrupted = true;
			}
			return this;
		}
	}

	private bool ShouldRowOwnerLoad()
	{
		if (!AnyColCrossTable())
		{
			return RowOwnerLoad;
		}
		return false;
	}

	private bool AnyColCrossTable()
	{
		return Columns.Any(delegate(Column c)
		{
			if (!c.HasFormula)
			{
				return false;
			}
			try
			{
				return new FormulaEvaluator(c.Formula).HasLqCrossTable();
			}
			catch (FormulaException)
			{
				return false;
			}
		});
	}

	public bool IsManager()
	{
		if (!Project.Current.Users.Any((KeyValuePair<Leqisoft.DTO.User, UserRole> u) => u.Key.Id == User.Current.Id))
		{
			return false;
		}
		return Project.Current.Users.First((KeyValuePair<Leqisoft.DTO.User, UserRole> u) => u.Key.Id == User.Current.Id).Value == UserRole.Manager;
	}

	private bool CanLoad(Row row)
	{
		if (User.Current.Id != row.Creator && !RowOwnerLoadShare.Exists(row.Creator, User.Current.Id) && row.Role != RowRole.Fixed && row.Role != RowRole.Header)
		{
			return row.Role == RowRole.Total;
		}
		return true;
	}

	public void LoadFormulaDependencies()
	{
		if (!_loaded || IsCorrupted || _isFormulaDependenciesLoaded)
		{
			return;
		}
		foreach (Column column in Columns)
		{
			if (column.HasFormula)
			{
				column.UpdateDependencies();
			}
		}
		foreach (Cell cell in Cells)
		{
			if (cell.HasHeaderFormula)
			{
				cell.UpdateHeaderCellDependencies();
			}
			if (cell.HasFormula)
			{
				cell.UpdateDependencies();
			}
		}
		_isFormulaDependenciesLoaded = true;
	}

	public List<Row> TryApplyFormula(bool evalLqDistinct)
	{
		EnableFormulaTrigger = false;
		List<Row> list = new List<Row>();
		int num = 0;
		do
		{
			num++;
			Cell.UpdateValueSuccessFlag = false;
			foreach (Column item in Columns.ToList())
			{
				list.AddRange(item.TryApplyFormula(rethrow: false, evalLqDistinct));
			}
			for (int i = 0; i < Rows.Count; i++)
			{
				for (int j = 0; j < Columns.Count; j++)
				{
					this[i, j]?.TryApplyHeaderFormula();
				}
			}
			for (int k = 0; k < Rows.Count; k++)
			{
				for (int l = 0; l < Columns.Count; l++)
				{
					this[k, l]?.TryApplyFormula();
				}
			}
			TryApplyTitleFootFormula();
		}
		while (Cell.UpdateValueSuccessFlag && num <= 10);
		EvalControlFormula();
		EnableFormulaTrigger = true;
		FormulaEvaluator.ClearCache();
		return list;
	}

	public void TryApplyTitleFootFormula()
	{
		try
		{
			Title.TitleCell.EvaluateFormula();
		}
		catch (FormulaException)
		{
		}
		foreach (TableTitleRow row in Title.Rows)
		{
			foreach (TableTitleCell cell in row.Cells)
			{
				try
				{
					cell.EvaluateFormula();
				}
				catch (FormulaException)
				{
				}
			}
		}
		foreach (TableTitleRow row2 in Foot.Rows)
		{
			foreach (TableTitleCell cell2 in row2.Cells)
			{
				try
				{
					cell2.EvaluateFormula();
				}
				catch (FormulaException)
				{
				}
			}
		}
		foreach (Column column in Columns)
		{
			try
			{
				column.EvaluateCaptionFormula();
			}
			catch (FormulaException)
			{
			}
		}
	}

	public void ReloadFromDb()
	{
		_loaded = false;
		CellStyles.Clear();
		Columns.Clear();
		Rows.Clear();
		Cells.Clear();
		MergedCells.Clear();
		RemovedMerges.Clear();
		RemovedColumns.Clear();
		RemovedRows.Clear();
		RemovedCells.Clear();
		LoadAndReturn();
	}

	public void Save(IProgress<ProgressInfo> progress = null, bool bypassMapRowIndex = false, TaskProgressValueUpdater taskProgressValueUpdater = null)
	{
		bool flag = false;
		try
		{
			// 调试检查：定位 NullReferenceException 的确切位置
			if (TreeNode == null) throw new InvalidOperationException("[Save调试] TreeNode 为 null");
			if (!LocalExists)
			{
				return;
			}
			if (Project == null) throw new InvalidOperationException("[Save调试] Project 为 null");
			if (Project.Dal == null) throw new InvalidOperationException("[Save调试] Project.Dal 为 null");
			
			ThrowIfMaxSizeExceeded();
			ThrowIfCellCountError();
			Project.Dal.BeginTransaction();
			progress?.Report(new ProgressInfo
			{
				MainCaption = "正在保存表格信息",
				MainProgress = 0
			});
			Project.Dal.SaveTable(ToDto());
			progress?.Report(new ProgressInfo
			{
				MainCaption = "正在保存列信息",
				MainProgress = 10
			});
			taskProgressValueUpdater?.UpdateProgress(10L, 100L);
			Project.Dal.SaveColumns(Columns.Select((Column c) => c.ToDto()));
			progress?.Report(new ProgressInfo
			{
				MainCaption = "正在保存行信息",
				MainProgress = 20
			});
			taskProgressValueUpdater?.UpdateProgress(20L, 100L);
			List<Leqisoft.DTO.Row> list = new List<Leqisoft.DTO.Row>();
			foreach (Row row2 in Rows)
			{
				if (row2.NeedSave)
				{
					Leqisoft.DTO.Row row = row2.ToDto();
					if (!bypassMapRowIndex && RowOwnerLoad)
					{
						row.Index = row2.GetMappedIndex();
					}
					list.Add(row);
				}
			}
			Project.Dal.SaveRows(list);
			progress?.Report(new ProgressInfo
			{
				MainCaption = "正在保存单元格信息",
				MainProgress = 30
			});
			taskProgressValueUpdater?.UpdateProgress(30L, 100L);
			List<Cell> list2 = Cells.Where((Cell c) => c.NeedSave).ToList();
			Project.Dal.SaveCells(list2.Select((Cell c) => c.ToDto()));
			List<CellProp> dto = CellPropManager.DicCellAttachments.Select((KeyValuePair<Id64, CellAttachments> kv) => new CellProp
			{
				TableId = Id,
				CellId = kv.Key,
				Dirty = (kv.Value.Dirty ? 1 : 0),
				Status = (int)kv.Value.Status,
				Attachments = kv.Value.Serialize()
			}).ToList();
			Project.Dal.SaveCellProps(dto);
			Project.Dal.SaveCellStyles(CellStyles.Select((CellStyle cs) => cs.ToDto()));
			taskProgressValueUpdater?.UpdateProgress(60L, 100L);
			progress?.Report(new ProgressInfo
			{
				MainCaption = "正在保存合并单元格信息",
				MainProgress = 80
			});
			Project.Dal.SaveMerges(MergedCells.Select((CellMerge m) => new Merge
			{
				Id = m.Id.Value,
				TableId = Id.Value,
				TopLeft = m.TopLeft.Id.Value,
				BottomRight = m.BottomRight.Id.Value,
				Status = (int)m.Status
			}));
			progress?.Report(new ProgressInfo
			{
				MainCaption = "正在清理数据...",
				MainProgress = 90
			});
			Project.Dal.RemoveColumns(RemovedColumns);
			Project.Dal.RemoveRows(RemovedRows);
			Project.Dal.RemoveCells(RemovedCells);
			taskProgressValueUpdater?.UpdateProgress(80L, 100L);
			Project.Dal.RemoveMerges(RemovedMerges);
			Project.Dal.DeleteColumns(ColumnsToDelete);
			ColumnsToDelete.Clear();
			Project.Dal.DeleteRows(RowsToDelete);
			RowsToDelete.Clear();
			Project.Dal.DeleteCells(CellsToDelete);
			CellsToDelete.Clear();
			Project.Dal.DeleteMerges(MergesToDelete);
			MergesToDelete.Clear();
			taskProgressValueUpdater?.UpdateProgress(90L, 100L);
			Project.Dal.Commit();
			flag = true;
			foreach (Row row3 in Rows)
			{
				row3.NeedSave = false;
			}
			foreach (Cell item in list2)
			{
				item.NeedSave = false;
			}
			progress?.Report(new ProgressInfo
			{
				MainProgress = 100
			});
			NeedSave = false;
		}
		catch
		{
			if (!flag)
			{
				try { Project.Dal.Rollback(); } catch { }
			}
			throw;
		}
	}

	public void TagTitleDirty()
	{
		Dirty.IsTitleDirty = true;
		NeedSave = true;
	}

	public void TagFootDirty()
	{
		Dirty.IsFootDirty = true;
		NeedSave = true;
	}

	public void TagPageSetupDirty()
	{
		Dirty.IsPageSetupDirty = true;
		NeedSave = true;
	}

	public void TagRowOwnerLoadShareDirty()
	{
		Dirty.IsRowOwnerLoadShareDirty = true;
		NeedSave = true;
	}

	public void TagTicketDirty(bool isCanOverrideByServerData = false)
	{
		if (!isCanOverrideByServerData)
		{
			Ticket.IsDirtyDataOnlyIncludeCanOverrideByServerData = false;
		}
		Dirty.IsTicketDirty = true;
		NeedSave = true;
	}

	public void UpdateDefaultStyle(CellStyle style)
	{
		DefaultStyle = style;
		Dirty.IsDefaultStyleDirty = true;
		NeedSave = true;
	}

	public void UpdateBorderStyle(TableBorderStyle bs)
	{
		BorderStyle = bs;
		if (bs != null && bs.IsCustomStyle)
		{
			CustomBorderStyle = bs.ToJson();
		}
		else
		{
			CustomBorderStyle = null;
		}
		Dirty.IsBorderStyleDirty = true;
		NeedSave = true;
	}

	public void UpdateFrozenCols(int fc)
	{
		FrozenCols = fc;
		Dirty.IsFrozenColsDirty = true;
		NeedSave = true;
	}

	public void UpdateHeaderMode(TableHeaderMode hm)
	{
		HeaderMode = hm;
		Dirty.IsHeaderModeDirty = true;
		NeedSave = true;
	}

	public void UpdateCollectSource(string cs)
	{
		CollectSource = cs;
		Dirty.IsCollectSourceDirty = true;
		NeedSave = true;
	}

	public void UpdateLocker(long l)
	{
		Locker = l;
		Dirty.IsLockerDirty = true;
		NeedSave = true;
	}

	public void UpdateFilterInfo(string fi)
	{
		FilterInfo = fi;
		Dirty.IsFilterDirty = true;
		NeedSave = true;
	}

	public void UpdateHeaderRowHeight(int row, int height)
	{
		SetHeaderHeight(row, height);
		Dirty.IsHeaderHeightsDirty = true;
		NeedSave = true;
	}

	public void TagConsolidateSettingsDirty()
	{
		Dirty.IsConsolidateSettingsDirty = true;
		NeedSave = true;
	}

	public void UpdateControlFormula(string f)
	{
		ControlFormula = f;
		Dirty.IsControlFormulaDirty = true;
		NeedSave = true;
	}

	[IteratorStateMachine(typeof(_003CEnumerateCellRange_003Ed__188))]
	public IEnumerable<Cell> EnumerateCellRange(int topRow, int leftCol, int bottomRow, int rightCol)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CEnumerateCellRange_003Ed__188(-2)
		{
			_003C_003E4__this = this,
			_003C_003E3__topRow = topRow,
			_003C_003E3__leftCol = leftCol,
			_003C_003E3__bottomRow = bottomRow,
			_003C_003E3__rightCol = rightCol
		};
	}

	public Cell ResolveCell(string column, int row)
	{
		LoadAndReturn();
		Column byCaption = Columns.GetByCaption(column);
		if (byCaption == null)
		{
			return null;
		}
		try
		{
			return this[row - 1, byCaption.Index];
		}
		catch (ArgumentOutOfRangeException)
		{
			return null;
		}
	}

	public bool WillMergeEraseValue(int topRow, int leftCol, int bottomRow, int rightCol)
	{
		try
		{
			if (topRow == bottomRow && leftCol == rightCol)
			{
				return false;
			}
			return EnumerateCellRange(topRow, leftCol, bottomRow, rightCol).Skip(1).Any((Cell c) => !c.Value.Equals(string.Empty));
		}
		catch (ArgumentOutOfRangeException)
		{
			return true;
		}
	}

	public void MergeCells(int topRow, int leftCol, int bottomRow, int rightCol)
	{
		if (topRow == bottomRow && leftCol == rightCol)
		{
			return;
		}
		object value = EnumerateCellRange(topRow, leftCol, bottomRow, rightCol).FirstOrDefault((Cell c) => !"".Equals(c.Value))?.Value ?? "";
		this[topRow, leftCol]?.UpdateValue(value);
		foreach (Cell item2 in EnumerateCellRange(topRow, leftCol, bottomRow, rightCol).Skip(1))
		{
			item2.UpdateValue(string.Empty);
		}
		CellMerge item = new CellMerge
		{
			Id = Project.Current.GetNextId(),
			Status = SyncStatus.New,
			TopLeft = this[topRow, leftCol],
			BottomRight = this[bottomRow, rightCol]
		};
		MergedCells.Add(item);
		NeedSave = true;
	}

	public void UnmergeCells(int row, int col)
	{
		CellMerge cellMerge = MergedCells.FirstOrDefault((CellMerge c) => c.TopLeft.Row.Index == row && c.TopLeft.Column.Index == col);
		if (cellMerge != null)
		{
			RemoveMerge(cellMerge);
			NeedSave = true;
		}
	}

	public Cell GetCellById(Id64 id)
	{
		return Cells.FirstOrDefault((Cell c) => c.Id == id);
	}

	public int GetNumCaptionRows()
	{
		if (Columns.Count == 0)
		{
			return 1;
		}
		return Columns.Select((Column c) => c.CaptionDisplay.Count((char s) => s == '_')).Max() + 1;
	}

	public int GetNumVisibleCaptionRows()
	{
		if (Columns.VisibleCount == 0)
		{
			return 1;
		}
		return Columns.WhereVisible.Select((Column c) => c.CaptionDisplay.Count((char s) => s == '_')).Max() + 1;
	}

	public List<CellRange> GetMergeInfo(bool visibleOnly)
	{
		if (HeaderMode == TableHeaderMode.Custom)
		{
			if (visibleOnly)
			{
				return TableHeaderMergeHelper.GetHeaderMergeInfoVisibleOnly(this);
			}
			return TableHeaderMergeHelper.GetHeaderMergeInfo(this);
		}
		return new List<CellRange>();
	}

	public void ExecuteBatchUpdateCellTriggers()
	{
		if (_batchUpdatingCells == null || _batchUpdatingCells.Count == 0)
		{
			return;
		}
		bool isBatchUpdating = _isBatchUpdating;
		try
		{
			EndBatchUpdateValue();
		}
		catch
		{
		}
		finally
		{
			_isBatchUpdating = isBatchUpdating;
		}
	}

	public void BeginBatchUpdateValue()
	{
		_isBatchUpdating = true;
	}

	public void EndBatchUpdateValue()
	{
		HashSet<FormulaTrigger> hashSet = new HashSet<FormulaTrigger>();
		foreach (FormulaTrigger formulaTrigger in _formulaTriggers)
		{
			try
			{
				formulaTrigger.Execute(_batchUpdatingCells);
			}
			catch (FormulaBadReferenceException)
			{
				hashSet.Add(formulaTrigger);
			}
		}
		foreach (FormulaTrigger item in hashSet)
		{
			_formulaTriggers.Remove(item);
		}
		EvalControlFormula();
		_batchUpdatingCells.Clear();
		_formulaExecuted.Clear();
		FormulaEvaluator.ClearCache();
		_isBatchUpdating = false;
	}

	public void CloneTo(Table ret)
	{
		ret.TreeNode = TreeNode;
		ret._loaded = true;
		ret.HeaderHeights = (int[])HeaderHeights.Clone();
		ret.BorderStyle = BorderStyle;
		ret.CustomBorderStyle = CustomBorderStyle;
		ret.FilterInfo = FilterInfo;
		ret.CollectSource = CollectSource;
		ret.DefaultStyle = DefaultStyle;
		ret.Title.Deserialize(Title.Serialize());
		ret.Foot.Deserialize(Foot.Serialize());
		ret.Ticket.Deserialize(Ticket.Serialize());
		ret.ControlFormula = ControlFormula;
		Dictionary<Id64, CellStyle> dictionary = new Dictionary<Id64, CellStyle>();
		foreach (CellStyle cellStyle2 in CellStyles)
		{
			CellStyle cellStyle = cellStyle2.Clone();
			cellStyle._pool = ret.CellStyles;
			dictionary.Add(cellStyle.Id, cellStyle);
			ret.CellStyles.Add(cellStyle);
		}
		foreach (Column column2 in Columns)
		{
			Column column = column2.Clone();
			column.Table = ret;
			column.CaptionStyle = column2.CaptionStyle.Clone();
			if (column2.Style != null)
			{
				column.Style = dictionary[column2.Style.Id];
			}
			ret.Columns._list.Add(column);
		}
		foreach (Row row2 in Rows)
		{
			Row row = row2.Clone();
			row.Table = ret;
			ret.Rows._list.Add(row);
		}
		foreach (Cell cell2 in Cells)
		{
			Cell cell = cell2.Clone();
			cell.Column = ret.Columns[cell2.Column.Index];
			cell.Row = ret.Rows[cell2.Row.Index];
			if (cell2.Style != null)
			{
				cell.Style = dictionary[cell2.Style.Id];
			}
			ret.Cells._list.Add(cell);
		}
		foreach (CellMerge i in MergedCells)
		{
			ret.MergedCells.Add(new CellMerge
			{
				Id = i.Id,
				TopLeft = ret.Cells.First((Cell c) => c.Id == i.TopLeft.Id),
				BottomRight = ret.Cells.First((Cell c) => c.Id == i.BottomRight.Id),
				Status = i.Status
			});
		}
	}

	public Table TemporaryClone()
	{
		Table table = new Table();
		CloneTo(table);
		return table;
	}

	public void SumColumns(Column sumLiteralCol)
	{
		BeginBatchUpdateValue();
		Row row = Rows.LastOrDefault((Row r) => r.Role == RowRole.Total);
		Row row2 = Rows.LastOrDefault((Row r) => r.Role == RowRole.Normal || r.Role == RowRole.Among || r.Role == RowRole.Minus);
		if (row == null)
		{
			int index = ((row2 != null) ? (row2.Index + 1) : 0);
			Rows.Insert(index, 1);
			row = Rows[index];
			row.UpdateRole(RowRole.Total);
		}
		if (row2 != null)
		{
			for (int i = 0; i < Columns.Count; i++)
			{
				Cell cell = this[row.Index, i];
				if (cell == null)
				{
					continue;
				}
				if (this[row2.Index, i] != null && this[row2.Index, i].DisplayFormat.IsNumericFormat())
				{
					if (!cell.HasFormula)
					{
						string text = null;
						text = ((!RowOwnerLoad || Project.IsCurrentUserManager()) ? $"SUM([2:{Id}:{Columns[i].Id}])" : $"SUM([3:{Id}:{this[0, i]?.Id}:{this[row2.Index, i]?.Id}])");
						cell.UpdateFormula(text);
					}
				}
				else if (!cell.HasFormula || cell.Formula == "\"合计\"")
				{
					cell.UpdateFormula("\"\"");
				}
			}
		}
		Cell cell2 = this[row.Index, sumLiteralCol.Index];
		if (cell2 != null)
		{
			cell2.UpdateFormula("\"合计\"");
			cell2.UpdateStyle(CellStyles.MutateAndGet(DefaultStyle, delegate(CellStyle cs)
			{
				cs.Align = CellTextAlign.MiddleCenter;
			}));
		}
		EndBatchUpdateValue();
	}

	public void CancelSumColumns()
	{
		foreach (Row item in Rows.ToList())
		{
			if (item.Role == RowRole.Total)
			{
				item.Remove();
			}
		}
	}

	public void RemoveRows(List<Row> rowsList)
	{
		if (rowsList.Count == 0)
		{
			return;
		}
		rowsList.Sort((Row left, Row right) => left.Index.CompareTo(right.Index));
		List<List<Row>> list = new List<List<Row>>();
		list.Add(new List<Row> { rowsList[0] });
		for (int i = 1; i < rowsList.Count; i++)
		{
			if (rowsList[i].Index != rowsList[i - 1].Index + 1)
			{
				list.Add(new List<Row> { rowsList[i] });
			}
			else
			{
				list[list.Count - 1].Add(rowsList[i]);
			}
		}
		for (int num = list.Count - 1; num >= 0; num--)
		{
			int index = list[num][0].Index;
			int count = list[num].Count;
			Rows.Remove(index, count);
		}
	}

	public bool IsControlFormulaAllowEditRow(Row r)
	{
		if (AllowEditRows == null)
		{
			return true;
		}
		return AllowEditRows.Contains(r);
	}

	public void CalculateRecursive()
	{
		FormulaManagerTransitional formulaManagerTransitional = new FormulaManagerTransitional(Project);
		formulaManagerTransitional.CalculateTableRecursive(this);
	}

	public void EvalControlFormula()
	{
		ControlLockCells.Clear();
		ControlLockRows.Clear();
		ControlWarningCells.Clear();
		ControlRemindCells.Clear();
		ControlForeColorCells.Clear();
		ControlBackColorCells.Clear();
		AllowEditRows = null;
		if (!HasControlFormula)
		{
			return;
		}
		FormulaEvaluator.ClearCache(this);
		ControlFormulaEvaluator controlFormulaEvaluator = new ControlFormulaEvaluator(ControlFormula);
		FormulaReferenceModelResolver resolver = new FormulaReferenceModelResolver(Project);
		controlFormulaEvaluator.Env = new FormulaEvaluationEnvironment
		{
			Resolver = resolver,
			RefManager = Project.DataReferenceManager,
			RefEvalContext = new DataReferenceEvaluationContext
			{
				Project = Project,
				CurrentTreeNode = TreeNode
			},
			ControlFormulaContext = new ControlFormulaContext()
		};
		try
		{
			controlFormulaEvaluator.Evaluate();
			ControlLockCells.UnionWith(controlFormulaEvaluator.Env.ControlFormulaContext.Lock);
			ControlLockRows.UnionWith(ControlLockCells.Select((Cell c) => c.Row));
			ControlWarningCells.UnionWith(controlFormulaEvaluator.Env.ControlFormulaContext.Warning);
			ControlRemindCells.UnionWith(controlFormulaEvaluator.Env.ControlFormulaContext.Remind);
			foreach (Cell key in controlFormulaEvaluator.Env.ControlFormulaContext.ForeColor.Keys)
			{
				ControlForeColorCells[key] = controlFormulaEvaluator.Env.ControlFormulaContext.ForeColor[key];
			}
			foreach (Cell key2 in controlFormulaEvaluator.Env.ControlFormulaContext.BackColor.Keys)
			{
				ControlBackColorCells[key2] = controlFormulaEvaluator.Env.ControlFormulaContext.BackColor[key2];
			}
			AllowEditRows = controlFormulaEvaluator.Env.ControlFormulaContext.AllowEditRow;
		}
		catch (FormulaException)
		{
		}
	}

	public bool HasSchemaPermission()
	{
		if (RowOwnerLoad || RowOwnerExclusive)
		{
			return IsManager();
		}
		return TreeNode.HasSchemaPermission();
	}

	public bool ContainsRow(Row row)
	{
		if (row.Table != this)
		{
			return false;
		}
		if (RemovedRows.Contains(row.Id) || RowsToDelete.Contains(row.Id))
		{
			return false;
		}
		return true;
	}

	internal void SetSynced()
	{
		TreeNode.IsEntityDirty = false;
		Dirty = default(TableDirtyMask);
		foreach (Column column in Columns)
		{
			column.SetSynced();
		}
		foreach (Id64 removedColumn in RemovedColumns)
		{
			ColumnsToDelete.Add(removedColumn);
		}
		RemovedColumns.Clear();
		foreach (Row row in Rows)
		{
			row.SetSynced();
		}
		foreach (Id64 removedRow in RemovedRows)
		{
			RowsToDelete.Add(removedRow);
		}
		RemovedRows.Clear();
		foreach (Cell cell in Cells)
		{
			cell.SetSynced();
		}
		foreach (Id64 removedCell in RemovedCells)
		{
			CellsToDelete.Add(removedCell);
		}
		foreach (CellStyle cellStyle in CellStyles)
		{
			cellStyle.SetSynced();
		}
		foreach (CellMerge mergedCell in MergedCells)
		{
			mergedCell.SetSynced();
		}
		foreach (Id64 removedMerge in RemovedMerges)
		{
			MergesToDelete.Add(removedMerge);
		}
		RemovedMerges.Clear();
		RemovedCells.Clear();
		CellPropManager.SetSynced();
		Ticket.SetSynced();
	}

	internal Cell MakeNewCell()
	{
		Project project = Project;
		if (project == null)
		{
			project = Project.Current;
		}
		return new Cell
		{
			NeedSave = true,
			Id = project.GetNextId(),
			Value = string.Empty,
			Formula = string.Empty,
			CollectSource = string.Empty,
			HeaderFormula = string.Empty
		};
	}

	internal Leqisoft.DTO.Table ToDto()
	{
		Leqisoft.DTO.Table table = new Leqisoft.DTO.Table();
		table.Id = Id;
		table.Dirty = Dirty.ToInt();
		table.Title = Title.Serialize();
		table.PageSetup = PageSetup.Serialize();
		table.HeaderHeights = SerializeHeaderHeights();
		table.DefaultStyleId = DefaultStyle?.Id ?? default;
		table.ConsolidateSettings = ConsolidateSettings.Serialize();
		table.BorderStyle = BorderStyle?.InternalNumber ?? 0;
		table.CustomBorderStyle = CustomBorderStyle;
		table.FrozenCols = FrozenCols;
		table.HeaderMode = (int)HeaderMode;
		table.CollectSource = CollectSource;
		table.Locker = Locker;
		table.Version = Version;
		table.FilterInfo = FilterInfo;
		table.Foot = Foot.Serialize();
		table.RowOwnerLoadShare = RowOwnerLoadShare.Serialize();
		table.Ticket = Ticket.Serialize();
		table.ControlFormula = ControlFormula;
		return table;
	}

	public string GetDebugInfo()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine($"Id={Id}");
		stringBuilder.AppendLine($"Title==null?{Title == null}");
		if (Title != null)
		{
			stringBuilder.AppendLine(Title.Serialize());
		}
		stringBuilder.AppendLine($"PageSetup==null?{PageSetup == null}");
		if (PageSetup != null)
		{
			stringBuilder.AppendLine(PageSetup.Serialize());
		}
		stringBuilder.AppendLine($"DefaultStyle==null?{DefaultStyle == null}");
		if (DefaultStyle != null)
		{
			stringBuilder.AppendLine(DefaultStyle.Id.ToString());
		}
		stringBuilder.AppendLine($"ConsolidateSettings==null?{ConsolidateSettings == null}");
		if (ConsolidateSettings != null)
		{
			stringBuilder.AppendLine(ConsolidateSettings.Serialize());
		}
		stringBuilder.AppendLine($"BorderStyle==null?{BorderStyle == null}");
		if (BorderStyle != null)
		{
			stringBuilder.AppendLine(BorderStyle.InternalNumber.ToString());
		}
		stringBuilder.AppendLine($"Foot==null?{Foot == null}");
		if (Foot != null)
		{
			stringBuilder.AppendLine(Foot.Serialize());
		}
		stringBuilder.AppendLine($"Version={Version}");
		stringBuilder.AppendLine($"# Rows={Rows.Count}");
		stringBuilder.AppendLine($"# Columns={Columns.Count}");
		stringBuilder.AppendLine($"# Cells={Cells.Count}");
		stringBuilder.AppendLine(string.Format("# RemovedRows={0}; {1}", RemovedRows.Count, string.Join(",", RemovedRows)));
		stringBuilder.AppendLine($"# RemovedColumns={RemovedColumns.Count}");
		stringBuilder.AppendLine($"# RemovedCells={RemovedCells.Count}");
		stringBuilder.AppendLine(string.Format("# RowsToDelete={0}; {1}", RowsToDelete.Count, string.Join(",", RowsToDelete)));
		stringBuilder.AppendLine($"# ColumnsToDelete={ColumnsToDelete.Count}");
		stringBuilder.AppendLine($"# CellsToDelete={CellsToDelete.Count}");
		return stringBuilder.ToString();
	}

	public List<CellRange> GetCellMergesVisible()
	{
		int[] map = new int[Columns.Count];
		int num = 0;
		for (int i = 0; i < Columns.Count; i++)
		{
			map[i] = num;
			if (Columns[i].Visible)
			{
				num++;
			}
		}
		return MergedCells.Select((CellMerge m) => new CellRange(m.TopLeft.Row.Index, map[m.TopLeft.Column.Index], m.BottomRight.Row.Index, map[m.BottomRight.Column.Index])).ToList();
	}

	public void Reset()
	{
	}

	internal string SerializeHeaderHeights()
	{
		return string.Join(",", HeaderHeights);
	}

	internal int[] DeserializeHeaderHeights(string s)
	{
		if (string.IsNullOrWhiteSpace(s))
		{
			return new int[0];
		}
		return (from h in s.Split(',')
			select int.Parse(h)).ToArray();
	}

	internal void InitTableForCreate(InitTableMode mode)
	{
		_loaded = true;
		NeedSave = true;
		Title.TitleCell.InitTitleCell();
		DefaultStyle = CellStyles.GetDefault();
		BorderStyle = TableBorderStyles.Grid;
		FilterInfo = string.Empty;
		CollectSource = string.Empty;
		for (int i = 0; i < 3; i++)
		{
			Title.Columns.Add(new TableTitleColumn
			{
				Width = 1f
			});
			Foot.Columns.Add(new TableTitleColumn
			{
				Width = 1f
			});
		}
		switch (mode)
		{
		case InitTableMode.Default:
		{
			Title.TitleCell.Value = "表格标题";
			Title.TitleCell.Align = CellTextAlign.MiddleCenter;
			Title.TitleHeight = UserSet.Config.TableStyle.MainTitleHeight;
			for (int j = 0; j < UserSet.Config.TableStyle.SubTitleRows; j++)
			{
				Title.AppendRow(useNextRowStyle: false);
				try
				{
					Title.Rows[j].Cells[0].Value = UserSet.Config.TableStyle.SubTitleContent[j].Item1;
					Title.Rows[j].Cells[1].Value = UserSet.Config.TableStyle.SubTitleContent[j].Item2;
					Title.Rows[j].Cells[2].Value = UserSet.Config.TableStyle.SubTitleContent[j].Item3;
				}
				catch
				{
				}
			}
			HeaderHeights = new int[1] { Rows.DefaultHeight };
			Rows.Append(UserSet.Config.TableStyle.TableRows);
			Columns.Append(UserSet.Config.TableStyle.TableCols);
			break;
		}
		case InitTableMode.Empty:
			Title.TitleCell.Value = string.Empty;
			Title.TitleCell.Align = CellTextAlign.MiddleCenter;
			Title.TitleHeight = UserSet.Config.TableStyle.MainTitleHeight;
			break;
		}
	}

	internal void RemoveInvalidMerges()
	{
		List<CellMerge> list = new List<CellMerge>();
		List<CellMerge> list2 = MergedCells.ToList();
		for (int i = 0; i < list2.Count; i++)
		{
			for (int j = i + 1; j < list2.Count; j++)
			{
				CellMerge cellMerge = list2[i];
				CellMerge cellMerge2 = list2[j];
				if (AreMergesConflict(cellMerge.TopLeft.Row.Index, cellMerge.TopLeft.Column.Index, cellMerge.BottomRight.Row.Index, cellMerge.BottomRight.Column.Index, cellMerge2.TopLeft.Row.Index, cellMerge2.TopLeft.Column.Index, cellMerge2.BottomRight.Row.Index, cellMerge2.BottomRight.Column.Index))
				{
					list.Add(cellMerge);
				}
			}
		}
		foreach (CellMerge item in list)
		{
			RemoveMerge(item);
		}
	}

	public bool AreMergesConflict(int m1tlr, int m1tlc, int m1brr, int m1brc, int m2tlr, int m2tlc, int m2brr, int m2brc)
	{
		if ((m1tlr <= m2tlr && m2tlr <= m1brr) || (m1tlr <= m2brr && m2brr <= m1brr) || (m2tlr < m1tlr && m2brr > m1brr))
		{
			if ((m1tlc > m2tlc || m2tlc > m1brc) && (m1tlc > m2brc || m2brc > m1brc))
			{
				if (m2tlc < m1tlc)
				{
					return m2brc > m1brc;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	internal void RemoveMerge(CellMerge merge)
	{
		MergedCells.Remove(merge);
		RemovedMerges.Add(merge.Id);
	}

	internal void AdjustHeaderHeights()
	{
		int numCaptionRows = GetNumCaptionRows();
		int[] array = Enumerable.Repeat(Rows.DefaultHeight, numCaptionRows).ToArray();
		if (numCaptionRows < HeaderHeights.Length)
		{
			Array.Copy(HeaderHeights, array, array.Length);
		}
		else
		{
			HeaderHeights.CopyTo(array, 0);
		}
		HeaderHeights = array;
		NeedSave = true;
		Dirty.IsHeaderHeightsDirty = true;
	}

	private int GetRawSize()
	{
		return Cells.Where((Cell c) => c.Value is string).Sum((Cell c) => ((string)c.Value).Length);
	}

	private void ThrowIfMaxSizeExceeded()
	{
		if (GetRawSize() > 1000000000)
		{
			throw new TableModelException("表格内容过大，需减少行列的数量或者缩减单元格内容");
		}
	}

	private void ThrowIfCellCountError()
	{
		if (Rows.Count * Columns.Count != Cells._list.Count)
		{
			throw new TableModelException("表格 " + TreeNode.Name + " 已损坏，请尝试重新载入表格或者删除本表格。");
		}
	}

	public void ThrowIfDelCellCountError()
	{
		if ((Rows.Count + RemovedRows.Count + RowsToDelete.Count) * (Columns.Count + RemovedColumns.Count + ColumnsToDelete.Count) != Cells.Count + RemovedCells.Count + CellsToDelete.Count)
		{
			throw new TableModelException("表格 " + TreeNode.Name + " 保存时数据出现异常，请联系官方客服联系支持！");
		}
	}
}
