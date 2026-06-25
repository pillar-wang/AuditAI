using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using C1.Win.C1FlexGrid;
using C1.Win.C1Input;
using Auditai.DTO;
using Auditai.Model;
using Newtonsoft.Json.Linq;

namespace Auditai.UI.Controls;

public class AdvanceGrid : C1FlexGrid
{
	public const string CN_INDEX = "RowIndex";

	public const string CN_RELATION = "Relation";

	public const string CN_COLS = "Col";

	public const string CN_OPT = "Type";

	public const string CN_ARGS = "Args";

	private readonly Dictionary<FilterKind, string> _dicNumber = new Dictionary<FilterKind, string>
	{
		[FilterKind.Eq] = "等于",
		[FilterKind.Ne] = "不等于",
		[FilterKind.Lt] = "小于",
		[FilterKind.Gt] = "大于",
		[FilterKind.Lte] = "小于等于",
		[FilterKind.Gte] = "大于等于",
		[FilterKind.Min] = "最小值",
		[FilterKind.Max] = "最大值",
		[FilterKind.Random] = "随机抽取",
		[FilterKind.Equidistance] = "等距抽取",
		[FilterKind.Pps] = "PPS取样"
	};

	private readonly Dictionary<FilterKind, string> _dicText = new Dictionary<FilterKind, string>
	{
		[FilterKind.TextEq] = "等于",
		[FilterKind.TextNe] = "不等于",
		[FilterKind.StartsWith] = "开头是",
		[FilterKind.NotStartsWith] = "开头不是",
		[FilterKind.Contains] = "包含",
		[FilterKind.NotContains] = "不包含",
		[FilterKind.EndsWith] = "结尾是",
		[FilterKind.NotEndsWith] = "结尾不是",
		[FilterKind.Random] = "随机抽取",
		[FilterKind.Equidistance] = "等距抽取"
	};

	private readonly Dictionary<FilterKind, string> _dicDate = new Dictionary<FilterKind, string>
	{
		[FilterKind.DateEq] = "等于",
		[FilterKind.DateNe] = "不等于",
		[FilterKind.Before] = "早于",
		[FilterKind.After] = "晚于",
		[FilterKind.BeforeEq] = "早于等于",
		[FilterKind.AfterEq] = "晚于等于",
		[FilterKind.DateMin] = "最小值",
		[FilterKind.DateMax] = "最大值",
		[FilterKind.Random] = "随机抽取",
		[FilterKind.Equidistance] = "等距抽取"
	};

	private readonly Dictionary<FilterKind, string> _dicYearMonth;

	private readonly Dictionary<FilterKind, string> _dicTime;

	private readonly Dictionary<FilterKind, string> _dicBool;

	private readonly Dictionary<FilterRelation, string> _dicRelations;

	private List<ColumnInfo> _columnInfos;

	private bool _attachedEvent;

	private readonly C1TextBoxEx _dateEdit;

	private readonly C1TextBoxEx _timeEdit;

	private readonly C1TextBoxEx _yearMonthEdit;

	public List<ColumnInfo> ColumnInfos
	{
		get
		{
			return _columnInfos;
		}
		set
		{
			_columnInfos = value;
			Initialize();
		}
	}

	public AdvanceGrid()
	{
		Dictionary<FilterKind, string> dictionary = new Dictionary<FilterKind, string>();
		dictionary[FilterKind.DateYearMonthEq] = "等于";
		dictionary[FilterKind.DateYearMonthNe] = "不等于";
		dictionary[FilterKind.DateYearMonthBefore] = "早于";
		dictionary[FilterKind.DateYearMonthAfter] = "晚于";
		dictionary[FilterKind.DateYearMonthAfterEq] = "早于等于";
		dictionary[FilterKind.DateYearMonthAfterEq] = "晚于等于";
		dictionary[FilterKind.DateYearMonthMin] = "最小值";
		dictionary[FilterKind.DateYearMonthMax] = "最大值";
		dictionary[FilterKind.Random] = "随机抽取";
		dictionary[FilterKind.Equidistance] = "等距抽取";
		_dicYearMonth = dictionary;
		_dicTime = new Dictionary<FilterKind, string>
		{
			[FilterKind.TimeEq] = "等于",
			[FilterKind.TimeNe] = "不等于",
			[FilterKind.TimeBefore] = "早于",
			[FilterKind.TimeAfter] = "晚于",
			[FilterKind.TimeBeforeEq] = "早于等于",
			[FilterKind.TimeAfterEq] = "晚于等于",
			[FilterKind.Random] = "随机抽取",
			[FilterKind.Equidistance] = "等距抽取"
		};
		_dicBool = new Dictionary<FilterKind, string>
		{
			[FilterKind.True] = "选中",
			[FilterKind.NotTrue] = "未选中"
		};
		_dicRelations = new Dictionary<FilterRelation, string>
		{
			[FilterRelation.And] = "并且",
			[FilterRelation.Or] = "或者"
		};
		_dateEdit = new C1TextBoxEx
		{
			DataType = typeof(DateTime)
		};
		_timeEdit = new C1TextBoxEx
		{
			DataType = typeof(DateTime)
		};
		_yearMonthEdit = new C1TextBoxEx
		{
			DataType = typeof(DateTime)
		};
		Initialize();
		base.SetupEditor += AdvanceGrid_SetupEditor;
		base.ValidateEdit += AdvanceGrid_ValidateEdit;
		_dateEdit.EditFormat.FormatType = FormatTypeEnum.CustomFormat;
		_dateEdit.CustomFormat = "yyyy-MM-dd";
		_timeEdit.EditFormat.FormatType = FormatTypeEnum.LongTime;
		_yearMonthEdit.EditFormat.FormatType = FormatTypeEnum.CustomFormat;
		_yearMonthEdit.CustomFormat = "yyyy-MM";
	}

	private void AdvanceGrid_ValidateEdit(object sender, ValidateEditEventArgs e)
	{
		if (base.Editor == _timeEdit)
		{
			TimeSpan timeOfDay = ((DateTime)_timeEdit.Value).TimeOfDay;
			base[e.Row, e.Col] = TimeSpan.FromSeconds(Math.Floor(timeOfDay.TotalSeconds));
		}
	}

	private void AdvanceGrid_SetupEditor(object sender, RowColEventArgs e)
	{
		if (base.Editor == _timeEdit)
		{
			_timeEdit.DataType = typeof(DateTime);
		}
		else if (base.Editor == _dateEdit)
		{
			object obj = base[e.Row, e.Col];
			if (obj == null || "".Equals(obj))
			{
				_dateEdit.Value = DateTime.Now;
			}
		}
		else if (base.Editor == _yearMonthEdit)
		{
			object obj2 = base[e.Row, e.Col];
			if (obj2 == null || "".Equals(obj2))
			{
				_yearMonthEdit.Value = DateTime.Now;
			}
		}
	}

	public List<FilterBase> GetFilters()
	{
		List<FilterBase> list = new List<FilterBase>();
		for (int i = base.Rows.Fixed; i < base.Rows.Count; i++)
		{
			C1.Win.C1FlexGrid.Row row = base.Rows[i];
			if (IsCellEmpty(row["Relation"]) || IsCellEmpty(row["Col"]) || IsCellEmpty(row["Type"]))
			{
				continue;
			}
			JObject jObject = new JObject();
			object obj = row["Args"] ?? "";
			ColumnInfo columnInfo = (ColumnInfo)row["Col"];
			jObject.Add("Relation", (int)row["Relation"]);
			jObject.Add("ColumnId", ((ColumnInfo)row["Col"]).Id);
			if (columnInfo.DataType == FilterDataType.Bool)
			{
				try
				{
					switch ((int)row["Type"])
					{
					case 502:
						obj = true;
						break;
					case 503:
						obj = false;
						break;
					}
				}
				catch
				{
				}
			}
			if (obj == null || "".Equals(obj))
			{
				jObject.Add("Kind", 504);
			}
			else
			{
				jObject.Add("Kind", (int)row["Type"]);
				object o = obj;
				if (columnInfo.DataType == FilterDataType.DateYearMonth)
				{
					DateTime date = (DateTime)obj;
					o = new DateYearMonth(date);
				}
				jObject.Add("Value", JToken.FromObject(o));
			}
			FilterBase item = jObject.ToObject<FilterBase>(FilterBase.Serializer);
			list.Add(item);
		}
		return list;
	}

	private void Grid_LastRowEdit(object sender, RowColEventArgs e)
	{
		if (e.Row + 1 == base.Rows.Count)
		{
			C1.Win.C1FlexGrid.Row row = base.Rows.Add();
			row["RowIndex"] = row.Index - base.Rows.Fixed + 1;
		}
	}

	public void Grid_AfterEdit(object sender, RowColEventArgs e)
	{
		if (base.Cols[e.Col].Name == "Col")
		{
			if (base.Rows[e.Row]["Col"] is ColumnInfo columnInfo)
			{
				C1.Win.C1FlexGrid.CellStyle cellStyle = base.Styles.Add("CnCol" + e.Row);
				C1.Win.C1FlexGrid.CellStyle cellStyle2 = base.Styles.Add("CnValue" + e.Row);
				switch (columnInfo.DataType)
				{
				case FilterDataType.Number:
					cellStyle.DataMap = _dicNumber;
					cellStyle2.DataType = typeof(double);
					break;
				case FilterDataType.Text:
					cellStyle.DataMap = _dicText;
					cellStyle2.DataType = typeof(string);
					break;
				case FilterDataType.Bool:
					cellStyle.DataMap = _dicBool;
					cellStyle2.DataType = typeof(string);
					break;
				case FilterDataType.Date:
					cellStyle.DataMap = _dicDate;
					cellStyle2.DataType = typeof(DateTime);
					cellStyle2.Format = (string.IsNullOrWhiteSpace(columnInfo.DataTypeFormatString) ? "yyyy年MM月dd日" : columnInfo.DataTypeFormatString);
					cellStyle2.Editor = _dateEdit;
					break;
				case FilterDataType.Time:
					cellStyle.DataMap = _dicTime;
					cellStyle2.DataType = typeof(TimeSpan);
					cellStyle2.Editor = _timeEdit;
					break;
				case FilterDataType.DateYearMonth:
					cellStyle.DataMap = _dicYearMonth;
					cellStyle2.DataType = typeof(DateTime);
					cellStyle2.Format = (string.IsNullOrWhiteSpace(columnInfo.DataTypeFormatString) ? "yyyy年MM月" : columnInfo.DataTypeFormatString);
					cellStyle2.Editor = _yearMonthEdit;
					break;
				}
				SetCellStyle(e.Row, "Type", cellStyle);
				SetCellStyle(e.Row, "Args", cellStyle2);
				base.Rows[e.Row]["Type"] = null;
				base.Rows[e.Row]["Args"] = null;
				if (string.IsNullOrWhiteSpace(base.Rows[e.Row]["Relation"]?.ToString()))
				{
					base.Rows[e.Row]["Relation"] = FilterRelation.And;
				}
			}
		}
		else
		{
			if (!(base.Cols[e.Col].Name == "Args"))
			{
				return;
			}
			ColumnInfo columnInfo2 = (ColumnInfo)base[e.Row, "Col"];
			if (columnInfo2 == null)
			{
				return;
			}
			object obj = base[e.Row, e.Col];
			if (columnInfo2.DataType == FilterDataType.Date)
			{
				if (obj != null && !string.IsNullOrWhiteSpace(obj.ToString()))
				{
					base[e.Row, e.Col] = ((DateTime)obj).Date;
				}
				else
				{
					base[e.Row, e.Col] = null;
				}
			}
			else if (columnInfo2.DataType == FilterDataType.DateYearMonth)
			{
				if (obj != null && !string.IsNullOrWhiteSpace(obj.ToString()))
				{
					base[e.Row, e.Col] = ((DateTime)obj).Date;
				}
				else
				{
					base[e.Row, e.Col] = null;
				}
			}
		}
	}

	private void Grid_CnFilterChanged(object sender, RowColEventArgs e)
	{
		if (base.Cols[e.Col].Name != "Type")
		{
			return;
		}
		if (string.IsNullOrWhiteSpace(base.Rows[e.Row]["Col"]?.ToString()))
		{
			MessageBox.Show(MessageBoxIcon.None, "请先设置操作列");
			base.Rows[e.Row][e.Col] = null;
			return;
		}
		base.Rows[e.Row]["Args"] = null;
		object obj = base.Rows[e.Row]["Type"];
		if ("随机抽取".Equals(obj) || "等距抽取".Equals(obj) || "PPS取样".Equals(obj))
		{
			C1.Win.C1FlexGrid.CellStyle cellStyle = base.Styles.Add("cnValue" + e.Row);
			cellStyle.DataType = typeof(decimal);
			SetCellStyle(e.Row, "Args", cellStyle);
		}
	}

	private void AdvanceGrid_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Delete)
		{
			for (int i = Selection.TopRow; i <= Selection.BottomRow; i++)
			{
				C1.Win.C1FlexGrid.Row row = base.Rows[i];
				row.Clear(ClearFlags.Content | ClearFlags.UserData);
				row["RowIndex"] = row.Index - base.Rows.Fixed + 1;
			}
		}
	}

	private void AdvanceGrid_AfterDeleteRow(object sender, RowColEventArgs e)
	{
		for (int i = base.Rows.Fixed; i < base.Rows.Count; i++)
		{
			base.Rows[i]["RowIndex"] = i - base.Rows.Fixed + 1;
		}
	}

	private void AdvanceGrid_AfterAddRow(object sender, RowColEventArgs e)
	{
		base.Rows[e.Row]["RowIndex"] = e.Row - base.Rows.Fixed + 1;
	}

	public void Initialize()
	{
		DettachEvent();
		base.Rows.Count = 0;
		base.Cols.Count = 0;
		base.Rows.DefaultSize = 30;
		base.Cols.Add(5);
		base.Rows.Add(6);
		base.Cols[0].Name = "RowIndex";
		base.Cols[1].Name = "Relation";
		base.Cols[2].Name = "Col";
		base.Cols[3].Name = "Type";
		base.Cols[4].Name = "Args";
		base.Rows[0]["RowIndex"] = "序号";
		base.Rows[0]["Relation"] = "关系";
		base.Rows[0]["Col"] = "操作列";
		base.Rows[0]["Type"] = "筛选类型";
		base.Rows[0]["Args"] = "参数";
		base.Rows.Fixed = 1;
		base.Cols.Fixed = 1;
		base.AutoResize = true;
		base.AllowDragging = AllowDraggingEnum.Rows;
		base.AllowFiltering = false;
		base.Rows[0].TextAlign = TextAlignEnum.CenterCenter;
		base.Cols[0].TextAlign = TextAlignEnum.CenterCenter;
		for (int i = base.Rows.Fixed; i < base.Rows.Count; i++)
		{
			base.Rows[i]["RowIndex"] = base.Rows[i].Index - base.Rows.Fixed + 1;
		}
		base.Cols["Relation"].DataMap = _dicRelations;
		if (_columnInfos != null)
		{
			base.Cols["Col"].DataMap = _columnInfos.ToDictionary((ColumnInfo ci) => ci, (ColumnInfo ci) => ci.Caption);
			base.Cols["Col"].AllowEditing = _columnInfos.Count > 0;
		}
		AttachEvent();
	}

	private void AttachEvent()
	{
		if (!_attachedEvent)
		{
			base.AfterEdit += Grid_LastRowEdit;
			base.AfterEdit += Grid_AfterEdit;
			base.AfterEdit += Grid_CnFilterChanged;
			base.KeyDown += AdvanceGrid_KeyDown;
			base.AfterAddRow += AdvanceGrid_AfterAddRow;
			base.AfterDeleteRow += AdvanceGrid_AfterDeleteRow;
			_attachedEvent = true;
		}
	}

	private void DettachEvent()
	{
		if (_attachedEvent)
		{
			base.AfterEdit -= Grid_LastRowEdit;
			base.AfterEdit -= Grid_AfterEdit;
			base.AfterEdit -= Grid_CnFilterChanged;
			base.KeyDown -= AdvanceGrid_KeyDown;
			base.AfterAddRow -= AdvanceGrid_AfterAddRow;
			base.AfterDeleteRow -= AdvanceGrid_AfterDeleteRow;
			_attachedEvent = false;
		}
	}

	private bool IsCellEmpty(object cell)
	{
		return string.IsNullOrEmpty(cell?.ToString());
	}
}
