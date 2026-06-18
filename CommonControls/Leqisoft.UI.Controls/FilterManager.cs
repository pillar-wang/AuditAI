using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using C1.Win.C1Command;
using C1.Win.C1FlexGrid;
using Leqisoft.DTO;
using Leqisoft.Model;
using Leqisoft.UI.Controls.Properties;

namespace Leqisoft.UI.Controls;

public class FilterManager
{
	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass157_0
	{
		public FilterValue data;

		public FilterManager _003C_003E4__this;

		internal C1CommandLink _003CGetDataTypeLnks_003Eg__GetNumberLnk_007C0(string text, FilterKind kind)
		{
			C1Command c1Command = new C1Command
			{
				Text = text + "..."
			};
			c1Command.Click += delegate
			{
				decimal? num = InputForm.Numeric("数值筛选", "数值型" + text);
				if (num.HasValue)
				{
					_003C_003E4__this.Add(new NumberFilter
					{
						Kind = kind,
						Value = (double)num.Value
					});
				}
			};
			return new C1CommandLink(c1Command);
		}

		internal C1CommandLink _003CGetDataTypeLnks_003Eg__GetTextLnk_007C1(string text, FilterKind kind)
		{
			C1Command c1Command = new C1Command
			{
				Text = text + "..."
			};
			c1Command.Click += delegate
			{
				string text2 = InputForm.Text("文本筛选", "文本型" + text);
				if (text2 != null)
				{
					text2 = text2.Replace("\r", "").Replace("\n", "");
					_003C_003E4__this.Add(new TextFilter
					{
						Kind = kind,
						Value = text2
					});
				}
			};
			return new C1CommandLink(c1Command);
		}

		internal C1CommandLink _003CGetDataTypeLnks_003Eg__GetDateLnk_007C2(string text, FilterKind kind)
		{
			C1Command c1Command = new C1Command
			{
				Text = text + "..."
			};
			c1Command.Click += delegate
			{
				DateTime? dateTime = InputForm.DateInput("日期筛选", "日期型" + text);
				if (dateTime.HasValue)
				{
					_003C_003E4__this.Add(new DateFilter
					{
						Kind = kind,
						Value = dateTime.Value.Date
					});
				}
			};
			return new C1CommandLink(c1Command);
		}

		internal C1CommandLink _003CGetDataTypeLnks_003Eg__GetDateYearMonthLnk_007C3(string text, FilterKind kind)
		{
			C1Command c1Command = new C1Command
			{
				Text = text + "..."
			};
			c1Command.Click += delegate
			{
				DateYearMonth? dateYearMonth = InputForm.DateYearMonthInput("日期筛选", "日期型" + text);
				if (dateYearMonth.HasValue)
				{
					_003C_003E4__this.Add(new DateYearMonthFilter
					{
						Kind = kind,
						Value = dateYearMonth.Value
					});
				}
			};
			return new C1CommandLink(c1Command);
		}

		internal C1CommandLink _003CGetDataTypeLnks_003Eg__GetTimeLnk_007C4(string text, FilterKind kind)
		{
			C1Command c1Command = new C1Command
			{
				Text = text + "..."
			};
			c1Command.Click += delegate
			{
				TimeSpan? timeSpan = InputForm.Time("时间筛选", "时间型" + text);
				if (timeSpan.HasValue)
				{
					_003C_003E4__this.Add(new TimeFilter
					{
						Kind = kind,
						Value = timeSpan.Value
					});
				}
			};
			return new C1CommandLink(c1Command);
		}
	}

	[CompilerGenerated]
	private sealed class _003CGetDataTypeLnks_003Ed__157 : IEnumerable<C1CommandLink>, IEnumerable, IEnumerator<C1CommandLink>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private C1CommandLink _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		public FilterManager _003C_003E4__this;

		private _003C_003Ec__DisplayClass157_0 _003C_003E8__1;

		C1CommandLink IEnumerator<C1CommandLink>.Current
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
		public _003CGetDataTypeLnks_003Ed__157(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E8__1 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			FilterManager filterManager = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E8__1 = new _003C_003Ec__DisplayClass157_0();
				_003C_003E8__1._003C_003E4__this = _003C_003E4__this;
				_003C_003E8__1.data = filterManager.GetCurrentData();
				switch (_003C_003E8__1.data.DataType)
				{
				case FilterDataType.Number:
					_003C_003E2__current = _003C_003E8__1._003CGetDataTypeLnks_003Eg__GetNumberLnk_007C0("等于", FilterKind.Eq);
					_003C_003E1__state = 1;
					return true;
				case FilterDataType.Text:
					break;
				case FilterDataType.Date:
					_003C_003E2__current = _003C_003E8__1._003CGetDataTypeLnks_003Eg__GetDateLnk_007C2("等于", FilterKind.DateEq);
					_003C_003E1__state = 5;
					return true;
				case FilterDataType.DateYearMonth:
					_003C_003E2__current = _003C_003E8__1._003CGetDataTypeLnks_003Eg__GetDateYearMonthLnk_007C3("等于", FilterKind.DateYearMonthEq);
					_003C_003E1__state = 14;
					return true;
				case FilterDataType.Bool:
					return false;
				case FilterDataType.Time:
					_003C_003E2__current = _003C_003E8__1._003CGetDataTypeLnks_003Eg__GetTimeLnk_007C4("等于", FilterKind.TimeEq);
					_003C_003E1__state = 23;
					return true;
				default:
					throw new ArgumentOutOfRangeException();
				}
				if (!string.IsNullOrEmpty(_003C_003E8__1.data.Text))
				{
					_003C_003E2__current = _003C_003E8__1._003CGetDataTypeLnks_003Eg__GetTextLnk_007C1("等于", FilterKind.TextEq);
					_003C_003E1__state = 3;
					return true;
				}
				goto IL_01ab;
			case 1:
				_003C_003E1__state = -1;
				_003C_003E2__current = _003C_003E8__1._003CGetDataTypeLnks_003Eg__GetNumberLnk_007C0("不等于", FilterKind.Ne);
				_003C_003E1__state = 2;
				return true;
			case 2:
				_003C_003E1__state = -1;
				return false;
			case 3:
				_003C_003E1__state = -1;
				_003C_003E2__current = _003C_003E8__1._003CGetDataTypeLnks_003Eg__GetTextLnk_007C1("不等于", FilterKind.TextNe);
				_003C_003E1__state = 4;
				return true;
			case 4:
				_003C_003E1__state = -1;
				goto IL_01ab;
			case 5:
				_003C_003E1__state = -1;
				_003C_003E2__current = _003C_003E8__1._003CGetDataTypeLnks_003Eg__GetDateLnk_007C2("早于", FilterKind.Before);
				_003C_003E1__state = 6;
				return true;
			case 6:
				_003C_003E1__state = -1;
				_003C_003E2__current = _003C_003E8__1._003CGetDataTypeLnks_003Eg__GetDateLnk_007C2("晚于", FilterKind.After);
				_003C_003E1__state = 7;
				return true;
			case 7:
				_003C_003E1__state = -1;
				_003C_003E2__current = _003C_003E8__1._003CGetDataTypeLnks_003Eg__GetDateLnk_007C2("不等于", FilterKind.DateNe);
				_003C_003E1__state = 8;
				return true;
			case 8:
				_003C_003E1__state = -1;
				_003C_003E2__current = _003C_003E8__1._003CGetDataTypeLnks_003Eg__GetDateLnk_007C2("早于等于", FilterKind.BeforeEq);
				_003C_003E1__state = 9;
				return true;
			case 9:
				_003C_003E1__state = -1;
				_003C_003E2__current = _003C_003E8__1._003CGetDataTypeLnks_003Eg__GetDateLnk_007C2("晚于等于", FilterKind.AfterEq);
				_003C_003E1__state = 10;
				return true;
			case 10:
				_003C_003E1__state = -1;
				_003C_003E2__current = filterManager._lnkToday;
				_003C_003E1__state = 11;
				return true;
			case 11:
				_003C_003E1__state = -1;
				_003C_003E2__current = filterManager._lnkDateSpan;
				_003C_003E1__state = 12;
				return true;
			case 12:
				_003C_003E1__state = -1;
				_003C_003E2__current = filterManager._lnkSamePeriod;
				_003C_003E1__state = 13;
				return true;
			case 13:
				_003C_003E1__state = -1;
				return false;
			case 14:
				_003C_003E1__state = -1;
				_003C_003E2__current = _003C_003E8__1._003CGetDataTypeLnks_003Eg__GetDateYearMonthLnk_007C3("早于", FilterKind.DateYearMonthBefore);
				_003C_003E1__state = 15;
				return true;
			case 15:
				_003C_003E1__state = -1;
				_003C_003E2__current = _003C_003E8__1._003CGetDataTypeLnks_003Eg__GetDateYearMonthLnk_007C3("晚于", FilterKind.DateYearMonthAfter);
				_003C_003E1__state = 16;
				return true;
			case 16:
				_003C_003E1__state = -1;
				_003C_003E2__current = _003C_003E8__1._003CGetDataTypeLnks_003Eg__GetDateYearMonthLnk_007C3("不等于", FilterKind.DateYearMonthNe);
				_003C_003E1__state = 17;
				return true;
			case 17:
				_003C_003E1__state = -1;
				_003C_003E2__current = _003C_003E8__1._003CGetDataTypeLnks_003Eg__GetDateYearMonthLnk_007C3("早于等于", FilterKind.DateYearMonthBeforeEq);
				_003C_003E1__state = 18;
				return true;
			case 18:
				_003C_003E1__state = -1;
				_003C_003E2__current = _003C_003E8__1._003CGetDataTypeLnks_003Eg__GetDateYearMonthLnk_007C3("晚于等于", FilterKind.DateYearMonthAfterEq);
				_003C_003E1__state = 19;
				return true;
			case 19:
				_003C_003E1__state = -1;
				_003C_003E2__current = filterManager._lnkCurrentMonth;
				_003C_003E1__state = 20;
				return true;
			case 20:
				_003C_003E1__state = -1;
				_003C_003E2__current = filterManager._lnkDateYearMonthSpan;
				_003C_003E1__state = 21;
				return true;
			case 21:
				_003C_003E1__state = -1;
				_003C_003E2__current = filterManager._lnkDateYearMonthSamePeriod;
				_003C_003E1__state = 22;
				return true;
			case 22:
				_003C_003E1__state = -1;
				return false;
			case 23:
				_003C_003E1__state = -1;
				_003C_003E2__current = _003C_003E8__1._003CGetDataTypeLnks_003Eg__GetTimeLnk_007C4("早于", FilterKind.TimeBefore);
				_003C_003E1__state = 24;
				return true;
			case 24:
				_003C_003E1__state = -1;
				_003C_003E2__current = _003C_003E8__1._003CGetDataTypeLnks_003Eg__GetTimeLnk_007C4("晚于", FilterKind.TimeAfter);
				_003C_003E1__state = 25;
				return true;
			case 25:
				_003C_003E1__state = -1;
				_003C_003E2__current = _003C_003E8__1._003CGetDataTypeLnks_003Eg__GetTimeLnk_007C4("不等于", FilterKind.TimeNe);
				_003C_003E1__state = 26;
				return true;
			case 26:
				_003C_003E1__state = -1;
				_003C_003E2__current = _003C_003E8__1._003CGetDataTypeLnks_003Eg__GetTimeLnk_007C4("早于等于", FilterKind.TimeBeforeEq);
				_003C_003E1__state = 27;
				return true;
			case 27:
				_003C_003E1__state = -1;
				_003C_003E2__current = _003C_003E8__1._003CGetDataTypeLnks_003Eg__GetTimeLnk_007C4("晚于等于", FilterKind.TimeAfterEq);
				_003C_003E1__state = 28;
				return true;
			case 28:
				{
					_003C_003E1__state = -1;
					return false;
				}
				IL_01ab:
				return false;
			}
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
		IEnumerator<C1CommandLink> IEnumerable<C1CommandLink>.GetEnumerator()
		{
			_003CGetDataTypeLnks_003Ed__157 result;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				result = this;
			}
			else
			{
				result = new _003CGetDataTypeLnks_003Ed__157(0)
				{
					_003C_003E4__this = _003C_003E4__this
				};
			}
			return result;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<C1CommandLink>)this).GetEnumerator();
		}
	}

	public const int INVALID_COL_INDEX = -1;

	private static readonly string[] _chineseOrdinals = new string[13]
	{
		"零", "一", "二", "三", "四", "五", "六", "七", "八", "九",
		"十", "十一", "十二"
	};

	private readonly C1FlexGridEx _grid;

	private readonly Dictionary<int, List<FilterBase>> _dicFilterCols = new Dictionary<int, List<FilterBase>>();

	private readonly C1Command _cmdCancelColumnHeader;

	private readonly C1CommandLink _lnkCancelColumnHeader;

	private readonly C1Command _cmdCancelCurrentColumn;

	private readonly C1Command _cmdCancelAll;

	private readonly C1CommandMenu _cmdFilter;

	private readonly C1Command _cmdAdvanced;

	private readonly C1CommandLink _lnkAdvanced;

	private readonly C1CommandMenu _cmdNumber;

	private readonly C1CommandLink _lnkNumber;

	private readonly C1Command _cmdMax;

	private readonly C1CommandLink _lnkMax;

	private readonly C1Command _cmdMin;

	private readonly C1CommandLink _lnkMin;

	private readonly C1CommandMenu _cmdText;

	private readonly C1CommandLink _lnkText;

	private readonly C1CommandMenu _cmdDate;

	private readonly C1CommandLink _lnkDate;

	private readonly C1CommandMenu _cmdDateYearMonth;

	private readonly C1CommandLink _lnkDateYearMonth;

	private readonly C1Command _cmdCurrentMonth;

	private readonly C1CommandLink _lnkCurrentMonth;

	private readonly C1CommandMenu _cmdTime;

	private readonly C1CommandLink _lnkTime;

	private readonly C1Command _cmdTrue;

	private readonly C1CommandLink _lnkTrue;

	private readonly C1Command _cmdNotTrue;

	private readonly C1CommandLink _lnkNotTrue;

	private readonly C1Command _cmdEmpty;

	private readonly C1CommandLink _lnkEmpty;

	private readonly C1Command _cmdNotEmpty;

	private readonly C1CommandLink _lnkNotEmpty;

	private readonly C1Command _cmdToday;

	private readonly C1CommandLink _lnkToday;

	private readonly C1CommandMenu _cmdDateSpan;

	private readonly C1CommandLink _lnkDateSpan;

	private readonly C1CommandMenu _cmdDateYearMonthSpan;

	private readonly C1CommandLink _lnkDateYearMonthSpan;

	private readonly C1CommandMenu _cmdSamePeriod;

	private readonly C1CommandLink _lnkSamePeriod;

	private readonly C1CommandMenu _cmdDateYearMonthSamePeriod;

	private readonly C1CommandLink _lnkDateYearMonthSamePeriod;

	private readonly C1ContextMenu _ctxFilter;

	private readonly C1CommandLink _lnkCancel;

	private readonly C1CommandMenu _cmdSample;

	private readonly C1Command _cmdSelect;

	private readonly C1Command _cmdRandom;

	private readonly C1CommandLink _lnkRandom;

	private readonly C1Command _cmdEquidistance;

	private readonly C1CommandLink _lnkEquidistance;

	private readonly C1Command _cmdPps;

	private readonly C1CommandLink _lnkPps;

	private readonly C1CommandMenu _cmdRepeat;

	private readonly C1CommandLink _lnkRepeat;

	private readonly C1ContextMenu _ctxTopLeft;

	private readonly SolidBrush _brush;

	private Dictionary<int, List<int>> _colMergeData = new Dictionary<int, List<int>>();

	public bool IsFilteredExternally { get; set; }

	public bool IsEditingFormula { get; set; }

	public bool IsLocked { get; set; }

	public bool IsEditingColHeader { get; set; }

	public bool IsDrawCancelAllFilterIcon { get; set; } = true;


	public bool IsDrawCancelAllFilterOnLeft { get; set; }

	public bool IsFilterOnGridColumnHeader { get; set; }

	public FilterCollection Filters { get; } = new FilterCollection();


	public bool IsFiltering
	{
		get
		{
			if (!Filters.Any())
			{
				return IsFilteredExternally;
			}
			return true;
		}
	}

	public int ResultCount { get; private set; }

	public FilterContext Context { get; set; }

	public event EventHandler Changed;

	public event EventHandler AfterFilterExecute;

	public C1CommandLink GenLnkFilter()
	{
		return new C1CommandLink(_cmdFilter);
	}

	public C1CommandLink GenLnkSample()
	{
		return new C1CommandLink(_cmdSample);
	}

	public C1CommandLink GenLnkSelect()
	{
		return new C1CommandLink(_cmdSelect);
	}

	public C1CommandLink GenLnkCancelAll()
	{
		return new C1CommandLink(_cmdCancelAll);
	}

	public C1CommandLink GenLnkCancelCurrentColumn()
	{
		return new C1CommandLink(_cmdCancelCurrentColumn);
	}

	public FilterManager(C1FlexGridEx owner)
	{
		_grid = owner;
		_grid.OwnerDrawCell += _grid_OwnerDrawCell;
		_grid.Paint += _grid_Paint;
		_grid.BeforeMouseDown += _grid_BeforeMouseDown;
		_cmdCancelColumnHeader = new C1Command
		{
			Text = "取消本列筛选"
		};
		_cmdCancelColumnHeader.Click += _cmdCancelColumnHeader_Click;
		_lnkCancelColumnHeader = new C1CommandLink(_cmdCancelColumnHeader);
		_cmdCancelCurrentColumn = new C1Command
		{
			Text = "取消本列筛选"
		};
		_cmdCancelCurrentColumn.Click += _cmdCancelCurrentColumn_Click;
		_cmdCancelCurrentColumn.CommandStateQuery += _cmdCancelCurrentColumn_CommandStateQuery;
		_cmdCancelAll = new C1Command
		{
			Text = "取消全部筛选"
		};
		_cmdCancelAll.Click += _cmdCancel_Click;
		_lnkCancel = new C1CommandLink(_cmdCancelAll);
		_cmdAdvanced = new C1Command
		{
			Text = "高级筛选"
		};
		_cmdAdvanced.Click += _cmdAdvanced_Click;
		_lnkAdvanced = new C1CommandLink(_cmdAdvanced);
		_cmdNumber = new C1CommandMenu
		{
			Text = "数值筛选"
		};
		AddNumberLnk("等于", FilterKind.Eq);
		AddNumberLnk("大于", FilterKind.Gt);
		AddNumberLnk("小于", FilterKind.Lt);
		AddNumberLnk("不等于", FilterKind.Ne);
		AddNumberLnk("大于等于", FilterKind.Gte);
		AddNumberLnk("小于等于", FilterKind.Lte);
		AddNumber2Lnk("范围之内", FilterKind.Between);
		AddNumber2Lnk("范围之外", FilterKind.Outside);
		_cmdMax = new C1Command
		{
			Text = "最大前..."
		};
		_cmdMax.Click += _cmdMax_Click;
		_lnkMax = new C1CommandLink(_cmdMax);
		_cmdNumber.CommandLinks.Add(_lnkMax);
		_cmdMin = new C1Command
		{
			Text = "最小前..."
		};
		_cmdMin.Click += _cmdMin_Click;
		_lnkMin = new C1CommandLink(_cmdMin);
		_cmdNumber.CommandLinks.Add(_lnkMin);
		_lnkNumber = new C1CommandLink(_cmdNumber);
		_cmdText = new C1CommandMenu
		{
			Text = "文本筛选"
		};
		AddTextLnk("等于", FilterKind.TextEq);
		AddTextLnk("包含", FilterKind.Contains);
		AddTextLnk("开头是", FilterKind.StartsWith);
		AddTextLnk("结尾是", FilterKind.EndsWith);
		AddTextLnk("不等于", FilterKind.TextNe);
		AddTextLnk("不包含", FilterKind.NotContains);
		AddTextLnk("开头不是", FilterKind.NotStartsWith);
		AddTextLnk("结尾不是", FilterKind.NotEndsWith);
		_lnkText = new C1CommandLink(_cmdText);
		_cmdDate = new C1CommandMenu
		{
			Text = "日期筛选"
		};
		AddDateLnk("等于", FilterKind.DateEq);
		AddDateLnk("早于", FilterKind.Before);
		AddDateLnk("晚于", FilterKind.After);
		AddDateLnk("不等于", FilterKind.DateNe);
		AddDateLnk("早于等于", FilterKind.BeforeEq);
		AddDateLnk("晚于等于", FilterKind.AfterEq);
		AddDate2Lnk("范围之内", FilterKind.DateBetween);
		AddDate2Lnk("范围之外", FilterKind.DateOutside);
		_lnkDate = new C1CommandLink(_cmdDate);
		_cmdDateYearMonth = new C1CommandMenu
		{
			Text = "日期筛选"
		};
		AddDateYearMonthLnk("等于", FilterKind.DateYearMonthEq);
		AddDateYearMonthLnk("早于", FilterKind.DateYearMonthBefore);
		AddDateYearMonthLnk("晚于", FilterKind.DateYearMonthAfter);
		AddDateYearMonthLnk("不等于", FilterKind.DateYearMonthNe);
		AddDateYearMonthLnk("早于等于", FilterKind.DateYearMonthBeforeEq);
		AddDateYearMonthLnk("晚于等于", FilterKind.DateYearMonthAfterEq);
		AddDateYearMonth2Lnk("范围之内", FilterKind.DateYearMonthBetween);
		AddDateYearMonth2Lnk("范围之外", FilterKind.DateYearMonthOutside);
		_lnkDateYearMonth = new C1CommandLink(_cmdDateYearMonth);
		_cmdTime = new C1CommandMenu
		{
			Text = "时间筛选"
		};
		AddTimeLnk("等于", FilterKind.TimeEq);
		AddTimeLnk("早于", FilterKind.TimeBefore);
		AddTimeLnk("晚于", FilterKind.TimeAfter);
		AddTimeLnk("不等于", FilterKind.TimeNe);
		AddTimeLnk("早于等于", FilterKind.TimeBeforeEq);
		AddTimeLnk("晚于等于", FilterKind.TimeAfterEq);
		AddTime2Lnk("范围之内", FilterKind.TimeBetween);
		AddTime2Lnk("范围之外", FilterKind.TimeOutside);
		_lnkTime = new C1CommandLink(_cmdTime);
		_cmdEmpty = new C1Command
		{
			Text = "等于空白"
		};
		_cmdEmpty.Click += _cmdBlank_Click;
		_lnkEmpty = new C1CommandLink(_cmdEmpty);
		_cmdNotEmpty = new C1Command
		{
			Text = "不等于空白"
		};
		_cmdNotEmpty.Click += _cmdNotEmpty_Click;
		_lnkNotEmpty = new C1CommandLink(_cmdNotEmpty);
		_cmdToday = new C1Command
		{
			Text = "等于今日"
		};
		_cmdToday.Click += _cmdToday_Click;
		_lnkToday = new C1CommandLink(_cmdToday);
		_cmdCurrentMonth = new C1Command
		{
			Text = "等于当前月"
		};
		_cmdCurrentMonth.Click += _cmdCurrentMonth_Click;
		_lnkCurrentMonth = new C1CommandLink(_cmdCurrentMonth);
		_cmdDateSpan = new C1CommandMenu
		{
			Text = "指定时段"
		};
		_cmdDateSpan.Popup += _cmdDateSpan_Popup;
		_cmdDateSpan.CommandLinks.Add(new C1CommandLink());
		_lnkDateSpan = new C1CommandLink(_cmdDateSpan);
		_cmdDateYearMonthSpan = new C1CommandMenu
		{
			Text = "指定时段"
		};
		_cmdDateYearMonthSpan.Popup += _cmdDateYearMonthSpan_Popup;
		_cmdDateYearMonthSpan.CommandLinks.Add(new C1CommandLink());
		_lnkDateYearMonthSpan = new C1CommandLink(_cmdDateYearMonthSpan);
		_cmdSamePeriod = new C1CommandMenu
		{
			Text = "相同时段"
		};
		_cmdSamePeriod.Popup += _cmdSamePeriod_Popup;
		_cmdSamePeriod.CommandLinks.Add(new C1CommandLink());
		_lnkSamePeriod = new C1CommandLink(_cmdSamePeriod);
		_cmdDateYearMonthSamePeriod = new C1CommandMenu
		{
			Text = "相同时段"
		};
		_cmdDateYearMonthSamePeriod.Popup += _cmdDateYearMonthSamePeriod_Popup;
		_cmdDateYearMonthSamePeriod.CommandLinks.Add(new C1CommandLink());
		_lnkDateYearMonthSamePeriod = new C1CommandLink(_cmdDateYearMonthSamePeriod);
		_cmdTrue = new C1Command
		{
			Text = "选中"
		};
		_cmdTrue.Click += _cmdTrue_Click;
		_lnkTrue = new C1CommandLink(_cmdTrue);
		_cmdNotTrue = new C1Command
		{
			Text = "未选中"
		};
		_cmdNotTrue.Click += _cmdFalseOrEmpty_Click;
		_lnkNotTrue = new C1CommandLink(_cmdNotTrue);
		_cmdFilter = new C1CommandMenu
		{
			Text = "筛选",
			Image = Resources.ctxFilter
		};
		_cmdFilter.CommandStateQuery += _cmdFilter_CommandStateQuery;
		_cmdFilter.Popup += _cmdFilter_Popup;
		_cmdFilter.CommandLinks.Add(_lnkAdvanced);
		_ctxFilter = new C1ContextMenu();
		_ctxFilter.CommandLinks.Add(_lnkCancelColumnHeader);
		_cmdSample = new C1CommandMenu
		{
			Text = "抽样",
			Image = Resources.ctxSample
		};
		_cmdSelect = new C1Command
		{
			Text = "选择"
		};
		_cmdSelect.Click += _cmdSelect_Click;
		_cmdSelect.CommandStateQuery += _cmdSelect_CommandStateQuery;
		_cmdSample.CommandLinks.Add(new C1CommandLink());
		_cmdSample.Popup += _cmdSample_Popup;
		_cmdSample.CommandStateQuery += _cmdSample_CommandStateQuery;
		_cmdRandom = new C1Command
		{
			Text = "随机抽样"
		};
		_cmdRandom.Click += _cmdRandom_Click;
		_lnkRandom = new C1CommandLink(_cmdRandom);
		_cmdEquidistance = new C1Command
		{
			Text = "等距抽样"
		};
		_cmdEquidistance.Click += _cmdEquidistance_Click;
		_lnkEquidistance = new C1CommandLink(_cmdEquidistance);
		_cmdPps = new C1Command
		{
			Text = "PPS抽样"
		};
		_cmdPps.Click += _cmdPps_Click;
		_lnkPps = new C1CommandLink(_cmdPps);
		_cmdRepeat = new C1CommandMenu
		{
			Text = "重复筛选"
		};
		_cmdRepeat.CommandLinks.Add(new C1CommandLink());
		_cmdRepeat.Popup += _cmdRepeat_Popup;
		_lnkRepeat = new C1CommandLink(_cmdRepeat);
		_brush = new SolidBrush(Color.Black);
		_ctxTopLeft = new C1ContextMenu();
		_lnkCancel = new C1CommandLink(_cmdCancelAll);
		_ctxTopLeft.CommandLinks.Add(_lnkCancel);
	}

	private void _cmdSample_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		bool visible = true;
		if (_grid.Selection.RightCol - _grid.Selection.LeftCol >= 1)
		{
			visible = false;
		}
		e.Visible = visible;
	}

	private void _cmdCancelCurrentColumn_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		bool visible = true;
		if (_grid.Selection.RightCol - _grid.Selection.LeftCol >= 1)
		{
			visible = false;
		}
		e.Visible = visible;
	}

	private void _cmdSelect_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		bool visible = true;
		if (_grid.Selection.RightCol - _grid.Selection.LeftCol >= 1)
		{
			visible = false;
		}
		e.Visible = visible;
	}

	private void _cmdFilter_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		bool visible = true;
		if (_grid.Selection.RightCol - _grid.Selection.LeftCol >= 1)
		{
			visible = false;
		}
		e.Visible = visible;
	}

	private void _grid_OwnerDrawCell(object sender, OwnerDrawCellEventArgs e)
	{
		if (IsDrawCancelAllFilterIcon && e.Col == 0 && e.Row == 0 && IsFiltering)
		{
			e.Text = "";
		}
	}

	private void _cmdCancelCurrentColumn_Click(object sender, ClickEventArgs e)
	{
		Filters.RemoveAll((FilterBase f) => Context.GetColumnIndex(f.ColumnId) == _grid.BodyCol);
		Execute();
		OnChanged();
	}

	private void _cmdSamePeriod_Popup(object sender, EventArgs e)
	{
		_cmdSamePeriod.CommandLinks.Clear();
		DateTime date = GetCurrentData().Date;
		AddSamePeriod("同年", new DateFilter
		{
			Kind = FilterKind.Year,
			Value = date
		});
		AddSamePeriod("同季", new DateFilter
		{
			Kind = FilterKind.Season,
			Value = date
		});
		AddSamePeriod("同月", new DateFilter
		{
			Kind = FilterKind.Month,
			Value = date
		});
		AddSamePeriod("同周", new DateFilter
		{
			Kind = FilterKind.Week,
			Value = date
		});
		void AddSamePeriod(string text, FilterBase filter)
		{
			C1Command c1Command = new C1Command
			{
				Text = text
			};
			c1Command.Click += delegate
			{
				Add(filter);
			};
			C1CommandLink value = new C1CommandLink(c1Command);
			_cmdSamePeriod.CommandLinks.Add(value);
		}
	}

	private void _cmdDateYearMonthSamePeriod_Popup(object sender, EventArgs e)
	{
		_cmdDateYearMonthSamePeriod.CommandLinks.Clear();
		DateYearMonth dateYearMonth = GetCurrentData().DateYearMonth;
		AddSamePeriod("同年", new DateYearMonthFilter
		{
			Kind = FilterKind.DateYearMonthYear,
			Value = dateYearMonth
		});
		AddSamePeriod("同季", new DateYearMonthFilter
		{
			Kind = FilterKind.DateYearMonthSeason,
			Value = dateYearMonth
		});
		AddSamePeriod("同月", new DateYearMonthFilter
		{
			Kind = FilterKind.DateYearMonthMonth,
			Value = dateYearMonth
		});
		void AddSamePeriod(string text, FilterBase filter)
		{
			C1Command c1Command = new C1Command
			{
				Text = text
			};
			c1Command.Click += delegate
			{
				Add(filter);
			};
			C1CommandLink value = new C1CommandLink(c1Command);
			_cmdDateYearMonthSamePeriod.CommandLinks.Add(value);
		}
	}

	private void _cmdFalseOrEmpty_Click(object sender, ClickEventArgs e)
	{
		Add(new NotTrueFilter
		{
			Kind = FilterKind.NotTrue
		});
	}

	private void _cmdTrue_Click(object sender, ClickEventArgs e)
	{
		Add(new TrueFilter
		{
			Kind = FilterKind.True
		});
	}

	private void _cmdSelect_Click(object sender, ClickEventArgs e)
	{
		C1.Win.C1FlexGrid.Column col = _grid.Cols[_grid.Col];
		HashSet<int> rows = ExecuteAllFilters();
		IEnumerable<string> values = from fv in Context.GetColumnData(Context.GetColumnId(col)).Where((FilterValue fv, int i) => rows.Contains(i))
			select fv.Display;
		SelectBoxForm selectBoxForm = new SelectBoxForm(values);
		if (selectBoxForm.ShowDialog() == DialogResult.OK)
		{
			Add(new SelectFilter
			{
				Kind = FilterKind.Select,
				Values = new HashSet<string>(selectBoxForm.SelectedValues)
			});
		}
	}

	private void _cmdRepeat_Popup(object sender, EventArgs e)
	{
		_cmdRepeat.CommandLinks.Clear();
		AddRepeat("显示重复值", new DuplicateFilter
		{
			Kind = FilterKind.Duplicate
		});
		AddRepeat("显示冗余值", new ExcessFilter
		{
			Kind = FilterKind.Excess
		});
		AddRepeat("排除冗余值", new ExceptExcessFilter
		{
			Kind = FilterKind.ExceptExcess
		});
		AddRepeat("显示唯一值", new UniqueFilter
		{
			Kind = FilterKind.Unique
		});
		void AddRepeat(string text, FilterBase filter)
		{
			C1Command c1Command = new C1Command
			{
				Text = text
			};
			c1Command.Click += delegate
			{
				Add(filter);
			};
			C1CommandLink value = new C1CommandLink(c1Command);
			_cmdRepeat.CommandLinks.Add(value);
		}
	}

	private void _cmdPps_Click(object sender, ClickEventArgs e)
	{
		decimal? num = InputForm.Numeric("PPS抽样", "PPS抽样数量");
		if (num.HasValue)
		{
			Add(new PpsFilter
			{
				Kind = FilterKind.Pps,
				Count = (int)num.Value
			});
		}
	}

	private void _cmdEquidistance_Click(object sender, ClickEventArgs e)
	{
		decimal? num = InputForm.Numeric("等距抽样", "等距抽样距离n（大于0，每n个样本取样，即第1个，第1+n个，第1+2n个，...）");
		if (num.HasValue)
		{
			Add(new EquidistanceFilter
			{
				Kind = FilterKind.Equidistance,
				Count = (int)num.Value
			});
		}
	}

	private void _cmdRandom_Click(object sender, ClickEventArgs e)
	{
		decimal? num = InputForm.Numeric("随机抽样", "随机抽样数量");
		if (num.HasValue)
		{
			Add(new RandomFilter
			{
				Kind = FilterKind.Random,
				Count = (int)num.Value
			});
		}
	}

	private FilterValue GetCurrentData()
	{
		if (IsFilterOnGridColumnHeader)
		{
			return Context.GetDataFilterOnColumnHeader(_grid.BodyCol);
		}
		return Context.GetData(_grid.BodyRow, _grid.BodyCol);
	}

	private void _cmdSample_Popup(object sender, EventArgs e)
	{
		_cmdSample.CommandLinks.Clear();
		_cmdSample.CommandLinks.Add(_lnkRandom);
		_cmdSample.CommandLinks.Add(_lnkEquidistance);
		if (GetCurrentData().DataType == FilterDataType.Number)
		{
			_cmdSample.CommandLinks.Add(_lnkPps);
		}
	}

	private void _cmdCancelColumnHeader_Click(object sender, ClickEventArgs e)
	{
		Filters.RemoveAll((FilterBase f) => Context.GetColumnIndex(f.ColumnId) == (int)e.CallerLink.Command.UserData);
		Execute();
		OnChanged();
	}

	private void _cmdCancelMulColumnHeader_Click(object sender, ClickEventArgs e)
	{
		List<int> colsList = (List<int>)e.CallerLink.Command.UserData;
		Filters.RemoveAll((FilterBase f) => colsList.Contains(Context.GetColumnIndex(f.ColumnId)));
		Execute();
		OnChanged();
	}

	private void _cmdMax_Click(object sender, ClickEventArgs e)
	{
		decimal? num = InputForm.Numeric("最大前", "筛选最大前几个数值");
		if (num.HasValue)
		{
			Add(new MaxFilter
			{
				Kind = FilterKind.Max,
				Count = (int)num.Value
			});
		}
	}

	private void _cmdMin_Click(object sender, ClickEventArgs e)
	{
		decimal? num = InputForm.Numeric("最小前", "筛选最小前几个数值");
		if (num.HasValue)
		{
			Add(new MinFilter
			{
				Kind = FilterKind.Max,
				Count = (int)num.Value
			});
		}
	}

	private void _grid_BeforeMouseDown(object sender, BeforeMouseDownEventArgs e)
	{
		if (IsEditingFormula || IsEditingColHeader || IsLocked || e.Button != MouseButtons.Left)
		{
			return;
		}
		Point pt = new Point(e.X, e.Y);
		HitTestInfo hitTestInfo = _grid.HitTest(pt);
		if (hitTestInfo.Row >= _grid.Rows.Fixed)
		{
			return;
		}
		int num = hitTestInfo.Column - _grid.Cols.Fixed;
		if (IsFiltering && hitTestInfo.Column == 0 && IsDrawCancelAllFilterIcon)
		{
			Rectangle drawImageRectangle = GetDrawImageRectangle(hitTestInfo.Column, isCancelAllIcon: true);
			if (drawImageRectangle.Contains(pt))
			{
				e.Cancel = true;
				_grid.Invalidate();
				_ctxTopLeft.ShowContextMenu(_grid, new Point(drawImageRectangle.Left, drawImageRectangle.Bottom + 2));
				return;
			}
		}
		bool flag = false;
		List<int> list = null;
		if (_colMergeData.Count > 0 && _colMergeData.TryGetValue(hitTestInfo.Column, out var value))
		{
			foreach (int item in value)
			{
				int num2 = item - _grid.Cols.Fixed;
				if (_dicFilterCols.ContainsKey(num2))
				{
					flag = true;
					if (list == null)
					{
						list = new List<int>();
					}
					list.Add(num2);
				}
			}
		}
		else
		{
			flag = _dicFilterCols.ContainsKey(num);
		}
		if (!flag)
		{
			return;
		}
		Rectangle drawImageRectangle2 = GetDrawImageRectangle(hitTestInfo.Column);
		if (drawImageRectangle2.Contains(pt))
		{
			e.Cancel = true;
			_grid.Invalidate();
			_cmdCancelColumnHeader.UserData = num;
			_ctxFilter.CommandLinks.Clear();
			if (list == null)
			{
				_ctxFilter.CommandLinks.Add(_lnkCancelColumnHeader);
			}
			else
			{
				C1Command c1Command = new C1Command();
				c1Command.UserData = list;
				c1Command.Click += _cmdCancelMulColumnHeader_Click;
				c1Command.Text = "取消本列筛选";
				_ctxFilter.CommandLinks.Add(new C1CommandLink(c1Command));
			}
			_ctxFilter.ShowContextMenu(_grid, new Point(drawImageRectangle2.Left, drawImageRectangle2.Bottom + 2));
		}
	}

	private Rectangle GetDrawImageRectangle(int col, bool isCancelAllIcon = false)
	{
		Rectangle cellRect = _grid.GetCellRect(_grid.Rows.Fixed - 1, col);
		int x = ((!isCancelAllIcon) ? (cellRect.Right - Resources.Filter12.Width - 2) : ((!IsDrawCancelAllFilterOnLeft) ? ((cellRect.Width - Resources.Filter12.Width) / 2) : (cellRect.Left + 2)));
		int y = cellRect.Top + (cellRect.Height - Resources.Filter12.Height) / 2;
		return new Rectangle(new Point(x, y), Resources.Filter12.Size);
	}

	public void ResetGridColumnMergeRange()
	{
		_colMergeData.Clear();
		if (_grid.Rows.Fixed <= 0)
		{
			return;
		}
		bool flag = false;
		int count = _grid.Cols.Count;
		int row = _grid.Rows.Fixed - 1;
		for (int i = 0; i < count; i++)
		{
			C1.Win.C1FlexGrid.CellRange mergedRange = _grid.GetMergedRange(row, i);
			if (mergedRange.LeftCol == mergedRange.RightCol && mergedRange.TopRow == mergedRange.BottomRow)
			{
				_colMergeData.Add(i, new List<int> { i });
				continue;
			}
			flag = true;
			List<int> value = null;
			if (!_colMergeData.TryGetValue(mergedRange.LeftCol, out value))
			{
				value = new List<int>();
				for (int j = mergedRange.LeftCol; j <= mergedRange.RightCol; j++)
				{
					value.Add(j);
				}
			}
			_colMergeData.Add(i, value);
		}
		if (!flag)
		{
			_colMergeData.Clear();
		}
	}

	public bool IsColumnInFilting(int gridColIndex)
	{
		if (!IsFiltering)
		{
			return false;
		}
		return _dicFilterCols.ContainsKey(gridColIndex - _grid.Cols.Fixed);
	}

	private void _grid_Paint(object sender, PaintEventArgs e)
	{
		Region region = null;
		Point p = _grid.PointToClient(Control.MousePosition);
		HitTestInfo hitTestInfo = _grid.HitTest();
		if (!_grid.IsResizingColumn && !_grid.IsResizingRow && !IsLocked && !IsEditingColHeader && !IsEditingFormula)
		{
			if (IsDrawCancelAllFilterIcon && hitTestInfo.Column == 0 && hitTestInfo.Type == HitTestTypeEnum.ColumnHeader && hitTestInfo.Row < _grid.Rows.Fixed && IsPointInFilterImageRect(p, hitTestInfo.Column, isCancelAllIcon: true))
			{
				Rectangle drawImageRectangle = GetDrawImageRectangle(hitTestInfo.Column, isCancelAllIcon: true);
				if (_colMergeData.Count > 0 && region == null)
				{
					region = e.Graphics.Clip;
					e.Graphics.ResetClip();
				}
				_brush.Color = Util.DarkenColor(_grid.Styles.SelectedColumnHeader.BackColor, Control.MouseButtons.HasFlag(MouseButtons.Left) ? 0.2 : 0.1);
				e.Graphics.FillRectangle(_brush, drawImageRectangle);
			}
			if (hitTestInfo.Type == HitTestTypeEnum.ColumnHeader && hitTestInfo.Row < _grid.Rows.Fixed && IsPointInFilterImageRect(p, hitTestInfo.Column))
			{
				Rectangle drawImageRectangle2 = GetDrawImageRectangle(hitTestInfo.Column);
				if (_colMergeData.Count > 0 && region == null)
				{
					region = e.Graphics.Clip;
					e.Graphics.ResetClip();
				}
				_brush.Color = Util.DarkenColor(_grid.Styles.SelectedColumnHeader.BackColor, Control.MouseButtons.HasFlag(MouseButtons.Left) ? 0.2 : 0.1);
				e.Graphics.FillRectangle(_brush, drawImageRectangle2);
			}
		}
		if (IsFiltering && !IsLocked && !IsEditingColHeader && IsDrawCancelAllFilterIcon)
		{
			Rectangle drawImageRectangle3 = GetDrawImageRectangle(0, isCancelAllIcon: true);
			if (_colMergeData.Count > 0 && region == null)
			{
				region = e.Graphics.Clip;
				e.Graphics.ResetClip();
			}
			e.Graphics.DrawImage(Resources.Filter12, drawImageRectangle3.Location);
		}
		if (!IsEditingColHeader)
		{
			foreach (KeyValuePair<int, List<FilterBase>> dicFilterCol in _dicFilterCols)
			{
				Rectangle drawImageRectangle4 = GetDrawImageRectangle(dicFilterCol.Key + _grid.Cols.Fixed);
				if (_colMergeData.Count > 0 && region == null)
				{
					region = e.Graphics.Clip;
					e.Graphics.ResetClip();
				}
				e.Graphics.DrawImage(Resources.Filter12, drawImageRectangle4.Location);
			}
		}
		if (region != null)
		{
			e.Graphics.Clip = region;
			region.Dispose();
		}
	}

	public bool IsPointInFilterImageRect(Point p, int col, bool isCancelAllIcon = false)
	{
		if (isCancelAllIcon)
		{
			if (IsFiltering && col == 0 && IsDrawCancelAllFilterIcon)
			{
				return GetDrawImageRectangle(col, isCancelAllIcon).Contains(p);
			}
			return false;
		}
		bool flag = false;
		if (_colMergeData.Count > 0 && _colMergeData.TryGetValue(col, out var value))
		{
			foreach (int item in value)
			{
				if (_dicFilterCols.ContainsKey(item - _grid.Cols.Fixed))
				{
					flag = true;
					break;
				}
			}
		}
		else
		{
			flag = _dicFilterCols.ContainsKey(col - _grid.Cols.Fixed);
		}
		if (flag)
		{
			return GetDrawImageRectangle(col, isCancelAllIcon).Contains(p);
		}
		return false;
	}

	private void _cmdDateSpan_Popup(object sender, EventArgs e)
	{
		_cmdDateSpan.CommandLinks.Clear();
		DateTime data = GetCurrentData().Date;
		Enumerable.Range(1, 4).ToList().ForEach(delegate(int i)
		{
			AddDateSpanSeason(i);
		});
		Enumerable.Range(1, 12).ToList().ForEach(delegate(int i)
		{
			AddDateSpanMonth(i);
		});
		void AddDateSpanMonth(int month)
		{
			C1Command c1Command = new C1Command
			{
				Text = _chineseOrdinals[month] + "月"
			};
			c1Command.Click += delegate
			{
				Add(new DateFilter
				{
					Kind = FilterKind.Month,
					Value = new DateTime(data.Year, month, 1)
				});
			};
			C1CommandLink value = new C1CommandLink(c1Command);
			_cmdDateSpan.CommandLinks.Add(value);
		}
		void AddDateSpanSeason(int season)
		{
			C1Command c1Command2 = new C1Command
			{
				Text = _chineseOrdinals[season] + "季度"
			};
			c1Command2.Click += delegate
			{
				Add(new DateFilter
				{
					Kind = FilterKind.Season,
					Value = new DateTime(data.Year, season * 3, 1)
				});
			};
			C1CommandLink value2 = new C1CommandLink(c1Command2);
			_cmdDateSpan.CommandLinks.Add(value2);
		}
	}

	private void _cmdDateYearMonthSpan_Popup(object sender, EventArgs e)
	{
		_cmdDateYearMonthSpan.CommandLinks.Clear();
		DateYearMonth data = GetCurrentData().DateYearMonth;
		Enumerable.Range(1, 4).ToList().ForEach(delegate(int i)
		{
			AddDateYearMonthSpanSeason(i);
		});
		Enumerable.Range(1, 12).ToList().ForEach(delegate(int i)
		{
			AddDateYearMonthSpanMonth(i);
		});
		void AddDateYearMonthSpanMonth(int month)
		{
			C1Command c1Command = new C1Command
			{
				Text = _chineseOrdinals[month] + "月"
			};
			c1Command.Click += delegate
			{
				Add(new DateYearMonthFilter
				{
					Kind = FilterKind.DateYearMonthMonth,
					Value = new DateYearMonth(new DateTime(data.Date.Year, month, 1))
				});
			};
			C1CommandLink value = new C1CommandLink(c1Command);
			_cmdDateYearMonthSpan.CommandLinks.Add(value);
		}
		void AddDateYearMonthSpanSeason(int season)
		{
			C1Command c1Command2 = new C1Command
			{
				Text = _chineseOrdinals[season] + "季度"
			};
			c1Command2.Click += delegate
			{
				Add(new DateYearMonthFilter
				{
					Kind = FilterKind.DateYearMonthSeason,
					Value = new DateYearMonth(new DateTime(data.Date.Year, season * 3, 1))
				});
			};
			C1CommandLink value2 = new C1CommandLink(c1Command2);
			_cmdDateYearMonthSpan.CommandLinks.Add(value2);
		}
	}

	private void _cmdToday_Click(object sender, ClickEventArgs e)
	{
		Add(new DateFilter
		{
			Kind = FilterKind.Today,
			Value = DateTime.Now.Date
		});
	}

	private void _cmdCurrentMonth_Click(object sender, ClickEventArgs e)
	{
		Add(new DateYearMonthFilter
		{
			Kind = FilterKind.DateYearMonthCurrentMonth,
			Value = new DateYearMonth(DateTime.Now.Date)
		});
	}

	private void _cmdAdvanced_Click(object sender, ClickEventArgs e)
	{
		AdvanceFilterBox advanceFilterBox = new AdvanceFilterBox();
		List<ColumnInfo> list = new List<ColumnInfo>();
		for (int i = 0; i < _grid.BodyColsCount; i++)
		{
			C1.Win.C1FlexGrid.Column col = _grid.BodyGetCol(i);
			string columnId = Context.GetColumnId(col);
			if (columnId != null)
			{
				list.Add(new ColumnInfo
				{
					Id = columnId,
					Caption = Context.GetColumnCaption(columnId),
					DataType = TypeToFilterDataType(Context.GetColumnDataType(columnId)),
					DataTypeFormatString = Context.GetColumnDataTypeFormatString(columnId)
				});
			}
		}
		advanceFilterBox.AdvanceGrid.ColumnInfos = list;
		if (advanceFilterBox.ShowDialog() == DialogResult.OK)
		{
			List<FilterBase> filters = advanceFilterBox.AdvanceGrid.GetFilters();
			Filters.Clear();
			Filters.AddRange(filters);
			Execute();
			OnChanged();
		}
	}

	private void _cmdNotEmpty_Click(object sender, ClickEventArgs e)
	{
		Add(new NotBlankFilter
		{
			Kind = FilterKind.NotBlank
		});
	}

	private void _cmdBlank_Click(object sender, ClickEventArgs e)
	{
		Add(new BlankFilter
		{
			Kind = FilterKind.Blank
		});
	}

	private void AddNumberLnk(string text, FilterKind kind)
	{
		C1Command c1Command = new C1Command
		{
			Text = text + "..."
		};
		c1Command.Click += delegate
		{
			decimal? num = InputForm.Numeric("数值筛选", "数值型" + text);
			if (num.HasValue)
			{
				Add(new NumberFilter
				{
					Kind = kind,
					Value = (double)num.Value
				});
			}
		};
		C1CommandLink value = new C1CommandLink(c1Command);
		_cmdNumber.CommandLinks.Add(value);
	}

	private void AddNumber2Lnk(string text, FilterKind kind)
	{
		C1Command c1Command = new C1Command
		{
			Text = text + "..."
		};
		c1Command.Click += delegate
		{
			if (InputForm.NumRange("数值筛选", "数值型" + text, out var min, out var max).HasValue)
			{
				Add(new NumberFilter
				{
					Kind = kind,
					Value = (double)min,
					Value2 = (double)max
				});
			}
		};
		C1CommandLink value = new C1CommandLink(c1Command);
		_cmdNumber.CommandLinks.Add(value);
	}

	private void AddTextLnk(string text, FilterKind kind)
	{
		C1Command c1Command = new C1Command
		{
			Text = text + "..."
		};
		c1Command.Click += delegate
		{
			string text2 = InputForm.Text("文本筛选", "文本型" + text);
			if (text2 != null)
			{
				text2 = text2.Replace("\r", "").Replace("\n", "");
				Add(new TextFilter
				{
					Kind = kind,
					Value = text2
				});
			}
		};
		C1CommandLink value = new C1CommandLink(c1Command);
		_cmdText.CommandLinks.Add(value);
	}

	private void AddDateLnk(string text, FilterKind kind)
	{
		C1Command c1Command = new C1Command
		{
			Text = text + "..."
		};
		c1Command.Click += delegate
		{
			DateTime? dateTime = InputForm.DateInput("日期筛选", "日期型" + text);
			if (dateTime.HasValue)
			{
				Add(new DateFilter
				{
					Kind = kind,
					Value = dateTime.Value.Date
				});
			}
		};
		C1CommandLink value = new C1CommandLink(c1Command);
		_cmdDate.CommandLinks.Add(value);
	}

	private void AddDate2Lnk(string text, FilterKind kind)
	{
		C1Command c1Command = new C1Command
		{
			Text = text + "..."
		};
		c1Command.Click += delegate
		{
			if (InputForm.DateRange("日期筛选", "日期型" + text, out var min, out var max).HasValue)
			{
				Add(new DateFilter
				{
					Kind = kind,
					Value = min.Date,
					Value2 = max.Date
				});
			}
		};
		C1CommandLink value = new C1CommandLink(c1Command);
		_cmdDate.CommandLinks.Add(value);
	}

	private void AddDateYearMonthLnk(string text, FilterKind kind)
	{
		C1Command c1Command = new C1Command
		{
			Text = text + "..."
		};
		c1Command.Click += delegate
		{
			DateYearMonth? dateYearMonth = InputForm.DateYearMonthInput("日期筛选", "日期型" + text);
			if (dateYearMonth.HasValue)
			{
				Add(new DateYearMonthFilter
				{
					Kind = kind,
					Value = dateYearMonth.Value
				});
			}
		};
		C1CommandLink value = new C1CommandLink(c1Command);
		_cmdDateYearMonth.CommandLinks.Add(value);
	}

	private void AddDateYearMonth2Lnk(string text, FilterKind kind)
	{
		C1Command c1Command = new C1Command
		{
			Text = text + "..."
		};
		c1Command.Click += delegate
		{
			if (InputForm.DateYearMonthRange("日期筛选", "日期型" + text, out var min, out var max).HasValue)
			{
				Add(new DateYearMonthFilter
				{
					Kind = kind,
					Value = min,
					Value2 = max
				});
			}
		};
		C1CommandLink value = new C1CommandLink(c1Command);
		_cmdDateYearMonth.CommandLinks.Add(value);
	}

	private void AddTimeLnk(string text, FilterKind kind)
	{
		C1Command c1Command = new C1Command
		{
			Text = text + "..."
		};
		c1Command.Click += delegate
		{
			TimeSpan? timeSpan = InputForm.Time("时间筛选", "时间型" + text);
			if (timeSpan.HasValue)
			{
				Add(new TimeFilter
				{
					Kind = kind,
					Value = timeSpan.Value
				});
			}
		};
		C1CommandLink value = new C1CommandLink(c1Command);
		_cmdTime.CommandLinks.Add(value);
	}

	private void AddTime2Lnk(string text, FilterKind kind)
	{
		C1Command c1Command = new C1Command
		{
			Text = text + "..."
		};
		c1Command.Click += delegate
		{
			Tuple<TimeSpan, TimeSpan> tuple = InputForm.TimeRange("时间筛选", "时间型" + text);
			if (tuple != null)
			{
				Add(new TimeFilter
				{
					Kind = kind,
					Value = tuple.Item1,
					Value2 = tuple.Item2
				});
			}
		};
		C1CommandLink value = new C1CommandLink(c1Command);
		_cmdTime.CommandLinks.Add(value);
	}

	private void _cmdFilter_Popup(object sender, EventArgs e)
	{
		_cmdFilter.CommandLinks.Clear();
		if (IsFilterOnGridColumnHeader)
		{
			Tuple<bool, string, string> tuple = Context.IsCheckBoxFilterOnColumnHeader(_grid.BodyCol);
			if (tuple.Item1)
			{
				_lnkTrue.Text = tuple.Item2;
				_lnkNotTrue.Text = tuple.Item3;
				_cmdFilter.CommandLinks.Add(_lnkTrue);
				_cmdFilter.CommandLinks.Add(_lnkNotTrue);
			}
			else
			{
				_cmdFilter.CommandLinks.Add(_lnkEmpty);
				_cmdFilter.CommandLinks.Add(_lnkNotEmpty);
				_cmdFilter.CommandLinks.Add(GetDataTypeLnk());
				_cmdFilter.CommandLinks.Add(_lnkRepeat);
			}
		}
		else
		{
			Tuple<bool, string, string> tuple2 = Context.IsCheckBox(_grid.BodyRow, _grid.BodyCol);
			if (tuple2.Item1)
			{
				_lnkTrue.Text = tuple2.Item2;
				_lnkNotTrue.Text = tuple2.Item3;
				_cmdFilter.CommandLinks.Add(_lnkTrue);
				_cmdFilter.CommandLinks.Add(_lnkNotTrue);
			}
			else
			{
				_cmdFilter.CommandLinks.AddRange(GetDataTypeLnks().ToList());
				_cmdFilter.CommandLinks.Add(_lnkEmpty);
				_cmdFilter.CommandLinks.Add(_lnkNotEmpty);
				_cmdFilter.CommandLinks.Add(GetDataTypeLnk());
				_cmdFilter.CommandLinks.Add(_lnkRepeat);
			}
		}
		_cmdFilter.CommandLinks.Add(_lnkAdvanced);
	}

	[IteratorStateMachine(typeof(_003CGetDataTypeLnks_003Ed__157))]
	private IEnumerable<C1CommandLink> GetDataTypeLnks()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGetDataTypeLnks_003Ed__157(-2)
		{
			_003C_003E4__this = this
		};
	}

	private C1CommandLink GetDataTypeLnk()
	{
		FilterValue currentData = GetCurrentData();
		return currentData.DataType switch
		{
			FilterDataType.Number => _lnkNumber, 
			FilterDataType.Text => _lnkText, 
			FilterDataType.Date => _lnkDate, 
			FilterDataType.Time => _lnkTime, 
			FilterDataType.DateYearMonth => _lnkDateYearMonth, 
			_ => _lnkText, 
		};
	}

	private void _cmdCancel_Click(object sender, ClickEventArgs e)
	{
		Clear();
		OnChanged();
	}

	public void Clear()
	{
		Filters.Clear();
		Execute();
	}

	public void Add(FilterBase filter)
	{
		C1.Win.C1FlexGrid.CellRange mergedRange = _grid.GetMergedRange(_grid.Row, _grid.Col, clip: false);
		if (mergedRange.IsValid)
		{
			int leftCol = mergedRange.LeftCol;
			string columnId = Context.GetColumnId(_grid.Cols[leftCol]);
			if (columnId != null)
			{
				filter.ColumnId = Context.GetColumnId(_grid.Cols[leftCol]);
				filter.Relation = FilterRelation.And;
				Filters.Add(filter);
				Execute();
				OnChanged();
			}
		}
	}

	private HashSet<int> GetFilteredRows(FilterBase filter)
	{
		List<FilterValue> columnData = Context.GetColumnData(filter.ColumnId);
		return filter.Execute(columnData);
	}

	private HashSet<int> ExecuteAllFilters()
	{
		if (Filters.Count > 0)
		{
			return Filters.Skip(1).Aggregate(GetFilteredRows(Filters.First()), delegate(HashSet<int> agg, FilterBase next)
			{
				if (next.Relation == FilterRelation.And)
				{
					agg.IntersectWith(GetFilteredRows(next));
					return agg;
				}
				if (next.Relation == FilterRelation.Or)
				{
					agg.UnionWith(GetFilteredRows(next));
					return agg;
				}
				throw new ArgumentOutOfRangeException();
			});
		}
		return new HashSet<int>(Enumerable.Range(0, _grid.BodyRowsCount));
	}

	public void Execute()
	{
		HashSet<int> hashSet = ExecuteAllFilters();
		ResultCount = hashSet.Count;
		_grid.BeginUpdate();
		for (int i = 0; i < _grid.BodyRowsCount; i++)
		{
			C1.Win.C1FlexGrid.Row row = _grid.BodyGetRow(i);
			row.Visible = hashSet.Contains(i);
		}
		_grid.EndUpdate();
		IsFilteredExternally = false;
		Populate();
		this.AfterFilterExecute?.Invoke(this, EventArgs.Empty);
	}

	private void OnChanged()
	{
		this.Changed?.Invoke(this, EventArgs.Empty);
	}

	public void Populate()
	{
		_dicFilterCols.Clear();
		foreach (IGrouping<int, FilterBase> item in from f in Filters
			group f by Context.GetColumnIndex(f.ColumnId) into g
			where g.Key != -1
			select g)
		{
			_dicFilterCols.Add(item.Key, item.ToList());
		}
		_grid.Invalidate();
	}

	public static FilterDataType TypeToFilterDataType(Type type)
	{
		if (type == typeof(decimal) || type == typeof(double) || type == typeof(float) || type == typeof(long) || type == typeof(int))
		{
			return FilterDataType.Number;
		}
		if (type == typeof(string))
		{
			return FilterDataType.Text;
		}
		if (type == typeof(DateTime))
		{
			return FilterDataType.Date;
		}
		if (type == typeof(bool))
		{
			return FilterDataType.Bool;
		}
		if (type == typeof(TimeSpan))
		{
			return FilterDataType.Time;
		}
		if (type == typeof(DateYearMonth))
		{
			return FilterDataType.DateYearMonth;
		}
		throw new ArgumentOutOfRangeException();
	}
}
