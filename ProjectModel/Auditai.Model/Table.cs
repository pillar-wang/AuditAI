﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Auditai.DTO;
using Auditai.Util;

namespace Auditai.Model;

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
			// Locker != 0：如果锁持有者是当前用户自己，不算锁定（自己获取的锁自己可编辑）
			if (Locker == Auditai.Model.User.Current?.Id)
			{
				return false;
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
						try
						{
							string logPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "leqiaudit_table_load_error.log");
							System.IO.File.AppendAllText(logPath,
								$"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] LOCAL NOT EXISTS: TableId={Id}, Name={TreeNode?.Name}, Version={Version}\r\n" +
								new string('=', 80) + "\r\n");
						}
						catch { }
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
					// 清空撤销/重做栈，防止历史命令引用已不存在的 Cell/Row 对象
					CommandsManager.Clear();
					Auditai.DTO.Table table = Project.Dal.GetTable(Id);
					if (table == null)
					{
						IsCorrupted = true;
						try
						{
							string logPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "leqiaudit_table_load_error.log");
							System.IO.File.AppendAllText(logPath,
								$"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] TABLE NOT FOUND IN DB: TableId={Id}, Name={TreeNode?.Name}\r\n" +
								new string('=', 80) + "\r\n");
						}
						catch { }
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
					foreach (Auditai.DTO.CellStyle cellStyle2 in Project.Dal.GetCellStyles(Id))
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
					if (!dictionary.TryGetValue(table.DefaultStyleId, out var defaultStyle))
					{
						// 防御：DefaultStyleId 指向样式缺失（脏数据）时用第一个样式兜底，
						// 避免 KeyNotFoundException 导致整表被误判为损坏。
						defaultStyle = CellStyles.FirstOrDefault();
					}
					DefaultStyle = defaultStyle;
					Dictionary<Id64, Column> dictionary2 = new Dictionary<Id64, Column>();
					foreach (Auditai.DTO.Column column2 in Project.Dal.GetColumns(Id))
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
						if (column2.StyleId.HasValue && dictionary.TryGetValue(column2.StyleId.Value, out var colStyle))
						{
							column.Style = colStyle;
						}
						Columns._list.Add(column);
						dictionary2.Add(column2.Id, column);
					}
					Columns.ResetIndex();
					Dictionary<Id64, Row> dictionary3 = new Dictionary<Id64, Row>();
					foreach (Auditai.DTO.Row row2 in Project.Dal.GetRows(Id))
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
					List<Auditai.DTO.Cell> list = Project.Dal.GetCells(Id).ToList();
					foreach (Auditai.DTO.Cell item in list)
					{
						// 防御：Cell 引用的 Row/Column 已被删除（本地删除后待同步/同步合并的正常中间态）
						// 时跳过该 Cell 并记入待删列表，避免 KeyNotFoundException 把整表误判为损坏。
						if (!dictionary3.TryGetValue(item.RowId, out var cellRow) || !dictionary2.TryGetValue(item.ColumnId, out var cellCol))
						{
							if (!item.Id.IsZero())
							{
								CellsToDelete.Add(item.Id);
							}
							continue;
						}
						Cell cell = new Cell
						{
							Row = cellRow,
							Column = cellCol,
							Id = item.Id,
							Value = item.Value.Value,
							Dirty = new CellDirtyMask(item.Dirty),
							Status = (SyncStatus)item.Status,
							Formula = item.Formula,
							CollectSource = item.CollectSource,
							HeaderFormula = item.HeaderFormula
						};
						if (item.StyleId.HasValue && dictionary.TryGetValue(item.StyleId.Value, out var cellStyle))
						{
							cell.Style = cellStyle;
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
					// 注意：这里不再做 Rows*Cols != Cells 的一刀切损坏判定。
					// 原判定位置在 RemovedXxx 待删列表加载之前，且公式不含待删项，
					// 会把"本地已删除行列、待删 Cells 尚未落盘"的正常同步中间态误判为损坏，
					// 进而触发 RepairFromCloudAsync（重置 Version=0 全量推送）造成连锁误报。
					// 一致性校验移到 RemovedXxx 加载完成后，用宽容公式判定（见下方）。
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
					// 单元格一致性：加载时不做"损坏"判定，直接自动修复。
				// 增删行列、改公式等正常编辑（尤其本地已改而云端未同步的中间态）会让
				// Rows*Cols != Cells 天然成立；任何数量公式都无法覆盖复合编辑场景，
				// 一旦据此判定"损坏"就会误触发云端全量重建（RepairFromCloudAsync）。
				// 因此这里只负责把内存模型修复为一致：清理孤儿 Cells + 补全缺失 Cells，
				// 结构性真错误由保存时 ThrowIfCellCountError / 加载异常兜底。
				if (Rows.Count * Columns.Count != Cells._list.Count)
					{
						try
						{
							// a) 清理孤儿 Cells（Row/Column 已不在当前 Rows/Columns 中）
							var validRowIds = new HashSet<Id64>(Rows.Select(r => r.Id));
							var validColIds = new HashSet<Id64>(Columns.Select(c => c.Id));
							var orphanCells = Cells.Where(c => c.Row == null || c.Column == null
								|| !validRowIds.Contains(c.Row.Id) || !validColIds.Contains(c.Column.Id)).ToList();
							foreach (var orphan in orphanCells)
							{
								if (!orphan.Id.IsZero())
								{
									CellsToDelete.Add(orphan.Id);
								}
								Cells._list.Remove(orphan);
							}
							// b) 按 Rows × Columns 笛卡尔积补全缺失 Cells
						if (Rows.Count * Columns.Count > Cells._list.Count)
						{
							EnsureAllCellsExist();
						}
						// 注意：不设置表级 NeedSave——孤儿已记入 CellsToDelete、补全格自带
						// Cell.NeedSave，Save() 会无条件处理；表级标记只会触发多余的保存流程。
						string logPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "leqiaudit_table_load_error.log");
							System.IO.File.AppendAllText(logPath,
								$"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] LOAD AUTO-REPAIR (not corruption): TableId={Id}, Name={TreeNode?.Name}\r\n" +
								$"  Rows={Rows.Count}, Cols={Columns.Count}, Cells={Cells._list.Count}, OrphansRemoved={orphanCells.Count}\r\n" +
								new string('=', 80) + "\r\n");
						}
						catch
						{
							// 自动修复失败不阻断加载，交由保存时校验兜底
						}
					}
					_loaded = true;
					IsCorrupted = false;
					EvalControlFormula();
				}
			}
			catch (Exception ex)
			{
				IsCorrupted = true;
				// Debug.WriteLine 在 Release 构建中被编译器完全移除（[Conditional("DEBUG")]），
				// 导致异常被彻底静默吞掉，无法诊断。改为写文件日志，确保 Release 构建也能捕获。
				try
				{
					string logPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "leqiaudit_table_load_error.log");
					string logContent = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] LoadAndReturn EXCEPTION\r\n" +
						$"  TableId={Id}, Name={TreeNode?.Name}, Version={Version}, LocalExists={LocalExists}\r\n" +
						$"  Exception: {ex.GetType().FullName}: {ex.Message}\r\n" +
						$"  StackTrace:\r\n{ex.StackTrace}\r\n" +
						new string('=', 80) + "\r\n";
					System.IO.File.AppendAllText(logPath, logContent);
				}
				catch { }
				System.Diagnostics.Debug.WriteLine($"[Table.LoadAndReturn] TableId={Id}, Name={TreeNode?.Name}, Error: {ex}");
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
		// 修复：原实现用静态 Project.Current（全局当前项目）而非本表所属项目（this.Project）
		// 判断管理员身份，多项目场景下判断的是错误的项目成员表。优先用本表所属项目。
		Project project = Project ?? Project.Current;
		if (project == null || User.Current == null)
		{
			return false;
		}
		if (!project.Users.Any((KeyValuePair<Auditai.DTO.User, UserRole> u) => u.Key.Id == User.Current.Id))
		{
			return false;
		}
		return project.Users.First((KeyValuePair<Auditai.DTO.User, UserRole> u) => u.Key.Id == User.Current.Id).Value == UserRole.Manager;
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

	public async Task<bool> RepairFromCloudAsync(TaskProgressValueReportCallback reportCallback = null)
	{
		if (Syncer.Disabled || Auditai.LocalDataStore.StorageRouter.IsLocalMode)
		{
			return false;
		}
		try
		{
			System.Diagnostics.Debug.WriteLine($"[RepairFromCloudAsync] 开始修复表格: Id={Id}, Name={TreeNode?.Name}");
			
			_loaded = false;
			IsCorrupted = false;
			
			try
			{
				LoadAndReturn();
			}
			catch (Exception loadEx)
			{
				System.Diagnostics.Debug.WriteLine($"[RepairFromCloudAsync] 本地加载异常（忽略，继续从云端修复）: {loadEx.Message}");
			}
			
			IsCorrupted = false;
			TreeNode.Version = 0;
			_loaded = true;
			
			System.Diagnostics.Debug.WriteLine($"[RepairFromCloudAsync] 本地加载后: Rows={Rows.Count}, Cols={Columns.Count}, Cells={Cells.Count}, IsCorrupted={IsCorrupted}");
			System.Diagnostics.Debug.WriteLine($"[RepairFromCloudAsync] 开始从云端拉取全量数据");
			
			PullResult result = await Syncer.Pull(this, reportCallback).ConfigureAwait(continueOnCapturedContext: false);
			System.Diagnostics.Debug.WriteLine($"[RepairFromCloudAsync] 拉取结果: {result}");
			
			if (result == PullResult.Success || result == PullResult.AlreadyLatest)
			{
				System.Diagnostics.Debug.WriteLine($"[RepairFromCloudAsync] Merge完成: Rows={Rows.Count}, Cols={Columns.Count}, Cells={Cells.Count}");
				
				if (DefaultStyle == null && CellStyles.Any())
				{
					DefaultStyle = CellStyles.First();
					System.Diagnostics.Debug.WriteLine($"[RepairFromCloudAsync] DefaultStyle 为 null，已自动设置为第一个样式");
				}
				
				if (Rows.Count * Columns.Count != Cells.Count)
				{
					System.Diagnostics.Debug.WriteLine($"[RepairFromCloudAsync] 单元格数量不匹配，期望 {Rows.Count * Columns.Count}，实际 {Cells.Count}，开始补全");
					EnsureAllCellsExist();
					System.Diagnostics.Debug.WriteLine($"[RepairFromCloudAsync] 补全后单元格数量: {Cells.Count}");
				}
				
				Save();
				_loaded = false;
				IsCorrupted = false;
				LoadAndReturn();
				System.Diagnostics.Debug.WriteLine($"[RepairFromCloudAsync] 修复完成: IsCorrupted={IsCorrupted}, Rows={Rows.Count}, Cols={Columns.Count}, Cells={Cells.Count}");
				return !IsCorrupted;
			}
			
			if (result == PullResult.NotExist)
			{
				// 云端没有这张表：把修复后的本地数据推回云端重建。
				// 这是"云端修复失败"死锁的关键解药——服务器端没有数据时，
				// 与其直接报失败，不如把本地（可修复的）数据推送上去创建表格。
				System.Diagnostics.Debug.WriteLine($"[RepairFromCloudAsync] 服务端不存在此表格，改为将本地数据推送回云端重建");

				// 1) 确保本地数据已完整加载且一致（加载异常时 Rows/Columns/Cells 可能为空或部分）
				IsCorrupted = false;
				_loaded = false;
				try
				{
					LoadAndReturn();
				}
				catch (Exception loadEx2)
				{
					System.Diagnostics.Debug.WriteLine($"[RepairFromCloudAsync] 推送前本地加载异常（忽略，尽量用已加载的数据推送）: {loadEx2.Message}");
				}
				if (Rows.Count * Columns.Count != Cells.Count)
				{
					EnsureAllCellsExist();
				}
				if (DefaultStyle == null && CellStyles.Any())
				{
					DefaultStyle = CellStyles.First();
				}

				// 2) 整表以 New 状态推送，确保 Synced 且未变更的行列也会被发送（Version=0 全量重建）
				foreach (Column column in Columns) column.Status = SyncStatus.New;
				foreach (Row row in Rows) row.Status = SyncStatus.New;
				foreach (Cell cell in Cells) cell.Status = SyncStatus.New;
				foreach (CellStyle cellStyle in CellStyles) cellStyle.Status = SyncStatus.New;
				foreach (CellMerge cellMerge in MergedCells) cellMerge.Status = SyncStatus.New;
				TreeNode.IsEntityDirty = true;

				// 3) 推送回云端创建表格
				PushResult pushResult = await Syncer.Push(this, reportCallback).ConfigureAwait(continueOnCapturedContext: false);
				System.Diagnostics.Debug.WriteLine($"[RepairFromCloudAsync] 推送结果: {pushResult}");
				if (pushResult == PushResult.Success)
				{
					Save();
					_loaded = false;
					IsCorrupted = false;
					LoadAndReturn();
					System.Diagnostics.Debug.WriteLine($"[RepairFromCloudAsync] 推送重建完成: IsCorrupted={IsCorrupted}, Rows={Rows.Count}, Cols={Columns.Count}, Cells={Cells.Count}");
					return !IsCorrupted;
				}
				return false;
			}
			return false;
		}
		catch (Exception ex)
		{
			IsCorrupted = true;
			System.Diagnostics.Debug.WriteLine($"[RepairFromCloudAsync] 修复异常: {ex}");
			return false;
		}
	}

	/// <summary>
	/// 采用云端版本：丢弃本地未同步修改，从云端全量重建表格。
	/// 供冲突解决对话框"采用云端"分支使用。
	/// </summary>
	public async Task<bool> AdoptServerStateAsync()
	{
		if (Syncer.Disabled || Auditai.LocalDataStore.StorageRouter.IsLocalMode)
		{
			return false;
		}
		try
		{
			TreeNode.IsEntityDirty = false;
			TreeNode.Version = 0;
			_loaded = false;
			// 清空本地内存中的全部数据，让 Pull/Merge 以云端数据完全重建（等价于放弃本地修改）
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
			CellPropManager.DicCellAttachments.Clear();
			CommandsManager.Clear();
			_isFormulaDependenciesLoaded = false;
			_formulaTriggers.Clear();
			PullResult result = await Syncer.Pull(this).ConfigureAwait(continueOnCapturedContext: false);
			if (result != PullResult.Success && result != PullResult.AlreadyLatest)
			{
				return false;
			}
			TreeNode.IsEntityDirty = false;
			Save();
			_loaded = false;
			IsCorrupted = false;
			return true;
		}
		catch
		{
			return false;
		}
	}

	/// <summary>
	/// 同一 (Row, Column) 位置出现多个 Cell 时，用于决定保留哪一个的评分。
	/// 规则：优先保留用户编辑过的格（Dirty 有设置或 Status=New），其次保留有内容/公式的格，
	/// 防止"自动补全出来的空白格"挤掉真实数据。
	/// 本策略同时被 EnsureAllCellsExist 与 TryRepairCellCountBeforeSave 使用，必须保持单一实现。
	/// </summary>
	private static int CellKeepScore(Cell c)
	{
		return (((c.Dirty.AnySet() || c.Status == SyncStatus.New) ? 2 : 0)
			+ ((((c.Value is string sv) ? !string.IsNullOrEmpty(sv) : c.Value != null) || !string.IsNullOrEmpty(c.Formula)) ? 1 : 0));
	}

	internal void EnsureAllCellsExist()
	{
		// 位置索引不变式：Cells._list[i] 必须对应
		//   (Rows[i / Columns.Count], Columns[i % Columns.Count])
		// 因为 CellCollection.GetCollectionIndex(row,col) = row * Columns.Count + col，
		// 且 Table.this[row,col] => Cells.Get(row,col) => _list[row * Columns.Count + col]。
		//
		// 原实现：foreach(Rows × Columns) 发现缺格就 Cells._list.Add(cell) —— 追加到列表末尾。
		// 当缺格位于中间（例如某个中间行的格未建全）时，补出来的格落到尾部会让其后所有格的
		// 位置索引整体错位，table[row,col] 会静默读到"另一个格"的内容（越界原本返回 null，
		// 是响亮的失败；错位返回别的格则是静默错误）。故改为按行主序整体重建列表。
		//
		// 重建顺带丢弃"同一 (Row, Column) 位置重复的 Cell"与孤儿 Cell（Row/Column 已不存在），
		// 因为二者都无法满足上述位置公式；保留哪一个复用 CellKeepScore 策略，
		// 与 TryRepairCellCountBeforeSave 完全一致，避免空白格挤掉用户编辑过的真实格。
		var posCellMap = new Dictionary<Tuple<Id64, Id64>, Cell>();
		foreach (Cell cell in Cells)
		{
			if (cell.Row == null || cell.Column == null)
			{
				continue;   // 孤儿，不进 posCellMap，下面统一登记待删
			}
			var key = Tuple.Create(cell.Row.Id, cell.Column.Id);
			Cell keep;
			if (!posCellMap.TryGetValue(key, out keep))
			{
				posCellMap[key] = cell;
				continue;
			}
			Cell drop = ((CellKeepScore(keep) >= CellKeepScore(cell)) ? cell : keep);
			if (drop == keep)
			{
				posCellMap[key] = cell;
			}
		}

		var ordered = new List<Cell>(Rows.Count * Columns.Count);
		foreach (Row row in Rows)
		{
			foreach (Column col in Columns)
			{
				var key = Tuple.Create(row.Id, col.Id);
				Cell cell;
				if (!posCellMap.TryGetValue(key, out cell))
				{
					cell = MakeNewCell();
					cell.Row = row;
					cell.Column = col;
					cell.Status = SyncStatus.Synced;
					posCellMap[key] = cell;
				}
				ordered.Add(cell);
			}
		}

		// 登记落选者（重复格中未保留的那个、以及孤儿格）为待删，避免它们在服务端长期残留
		var kept = new HashSet<Cell>(ordered);
		foreach (Cell cell in Cells)
		{
			if (kept.Contains(cell))
			{
				continue;
			}
			if (!cell.Id.IsZero())
			{
				CellsToDelete.Add(cell.Id);
			}
		}

		Cells._list.Clear();
		Cells._list.AddRange(ordered);
	}

	/// <summary>
	/// 表格保存成功后触发，供订阅方执行自动 Push 等协同逻辑。
	/// </summary>
	public event EventHandler Saved;

	protected virtual void OnSaved()
	{
		Saved?.Invoke(this, EventArgs.Empty);
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
			List<Auditai.DTO.Row> list = new List<Auditai.DTO.Row>();
			foreach (Row row2 in Rows)
			{
				if (row2.NeedSave)
				{
					Auditai.DTO.Row row = row2.ToDto();
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
		OnSaved();
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

	internal Auditai.DTO.Table ToDto()
	{
		Auditai.DTO.Table table = new Auditai.DTO.Table();
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
		// 防御：容忍历史数据中的损坏/非数字项（int.Parse 抛 FormatException 会
		// 导致整个表格加载失败并被标记为损坏）。逐项 TryParse，跳过非法项，
		// 仅当全部非法时回退为空数组，最大限度保留可用数据。注意保留 -1
		// 哨兵值（SetHeaderHeight 用它表示"未设置"），不做符号过滤以免索引错位。
		List<int> list = null;
		foreach (string h in s.Split(','))
		{
			if (int.TryParse(h, out var height))
			{
				(list ??= new List<int>()).Add(height);
			}
		}
		return list?.ToArray() ?? new int[0];
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
			// 先尝试自动修复：补全缺失单元格、清理多余单元格
			TryRepairCellCountBeforeSave();
			if (Rows.Count * Columns.Count != Cells._list.Count)
			{
				throw new TableModelException("表格 " + TreeNode.Name + " 已损坏，请尝试重新载入表格或者删除本表格。");
			}
		}
	}

	/// <summary>
	/// 保存前尝试自动修复 Rows.Count * Columns.Count != Cells.Count 的不一致。
	/// 1) 清理 Row 或 Column 已不存在导致的多余 Cells（加入 CellsToDelete，从 Cells._list 移除）
	/// 2) 补全缺失的 Cells（调用 EnsureAllCellsExist）
	/// 修复后由调用方再次校验；仍不一致才抛异常。
	/// </summary>
	private void TryRepairCellCountBeforeSave()
	{
		try
		{
			// 0) 同一 (Row, Column) 位置出现重复 Cell 时去重。
			// 重复来源：自动补全的空白格（新本地 Id）与 Pull 下发的真实格（云端 Id）
			// 在 Merge 中按 Id 合并导致同位置共存。去重规则优先保留用户编辑过的格
			// （Dirty 有设置或 Status=New），其次保留有内容/公式的格，防止空白格挤掉真实数据。
			var posCellMap = new Dictionary<Tuple<Id64, Id64>, Cell>();
			var duplicateCells = new List<Cell>();
			foreach (Cell c in Cells)
			{
				if (c.Row == null || c.Column == null)
				{
					continue; // 孤儿由步骤 1 处理
				}
				var key = Tuple.Create(c.Row.Id, c.Column.Id);
				if (!posCellMap.TryGetValue(key, out var keep))
				{
					posCellMap[key] = c;
					continue;
				}
				Cell drop = (CellKeepScore(keep) >= CellKeepScore(c)) ? c : keep;
				if (drop == c)
				{
					duplicateCells.Add(c);
				}
				else
				{
					duplicateCells.Add(keep);
					posCellMap[key] = c;
				}
			}
			foreach (Cell dup in duplicateCells)
			{
				if (!dup.Id.IsZero())
				{
					CellsToDelete.Add(dup.Id);
				}
				Cells._list.Remove(dup);
			}

			// 1) 清理多余 Cells（其 Row 或 Column 已不在当前 Rows/Columns 中）
			var validRowIds = new HashSet<Id64>(Rows.Select(r => r.Id));
			var validColIds = new HashSet<Id64>(Columns.Select(c => c.Id));
			var orphanCells = Cells.Where(c => c.Row == null || c.Column == null
				|| !validRowIds.Contains(c.Row.Id) || !validColIds.Contains(c.Column.Id)).ToList();
			if (orphanCells.Count > 0)
			{
				foreach (var orphan in orphanCells)
				{
					// Cell.Id 是 Id64 结构体（非可空），直接加入待删除集合
					if (!orphan.Id.IsZero())
					{
						CellsToDelete.Add(orphan.Id);
					}
					Cells._list.Remove(orphan);
				}
				NeedSave = true;
			}

			// 2) 补全缺失 Cells（基于现有 Rows × Columns 笛卡尔积）
			if (Rows.Count * Columns.Count > Cells._list.Count)
			{
				EnsureAllCellsExist();
				NeedSave = true;
			}
		}
		catch
		{
			// 修复失败不抛出，由调用方 ThrowIfCellCountError 兜底
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
