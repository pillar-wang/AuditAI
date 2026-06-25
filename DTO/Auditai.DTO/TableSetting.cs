using System;
using System.Collections.Generic;
using System.Drawing;
using Newtonsoft.Json;

namespace Auditai.DTO;

public class TableSetting
{
	private int _subTitleRows;

	private int _mainTitleHeight;

	private int _subTitleHeight;

	private int _tableRows;

	private int _tableCols;

	private int _tableRowHeight;

	[JsonProperty(PropertyName = "FontStyle")]
	public FontSetting FontStyle { get; set; }

	[JsonProperty(PropertyName = "TitleStyle")]
	public FontSetting TitleStyle { get; set; }

	[JsonProperty(PropertyName = "SubTitleStyle")]
	public FontSetting SubTitleStyle { get; set; }

	[JsonProperty(PropertyName = "SubTitleRows")]
	public int SubTitleRows
	{
		get
		{
			if (_subTitleRows < 0)
			{
				return 0;
			}
			if (_subTitleRows > 10)
			{
				return 10;
			}
			return _subTitleRows;
		}
		set
		{
			_subTitleRows = value;
		}
	}

	[JsonProperty(PropertyName = "MainTitleHeight")]
	public int MainTitleHeight
	{
		get
		{
			if (_mainTitleHeight < 10)
			{
				return 10;
			}
			if (_mainTitleHeight > 2000)
			{
				return 2000;
			}
			return _mainTitleHeight;
		}
		set
		{
			_mainTitleHeight = value;
		}
	}

	[JsonProperty(PropertyName = "SubTitleHeight")]
	public int SubTitleHeight
	{
		get
		{
			if (_subTitleHeight < 10)
			{
				return 10;
			}
			if (_subTitleHeight > 2000)
			{
				return 2000;
			}
			return _subTitleHeight;
		}
		set
		{
			_subTitleHeight = value;
		}
	}

	[JsonProperty(PropertyName = "SubTitleContent")]
	public List<Tuple<string, string, string>> SubTitleContent { get; set; }

	[JsonProperty(PropertyName = "TableRows")]
	public int TableRows
	{
		get
		{
			if (_tableRows > 0)
			{
				return _tableRows;
			}
			return 10;
		}
		set
		{
			_tableRows = value;
		}
	}

	[JsonProperty(PropertyName = "TableCols")]
	public int TableCols
	{
		get
		{
			if (_tableCols > 0)
			{
				return _tableCols;
			}
			return 5;
		}
		set
		{
			_tableCols = value;
		}
	}

	[JsonProperty(PropertyName = "TableRowHeight")]
	public int TableRowHeight
	{
		get
		{
			if (_tableRowHeight < 10)
			{
				return 10;
			}
			if (_tableRowHeight > 2000)
			{
				return 2000;
			}
			return _tableRowHeight;
		}
		set
		{
			_tableRowHeight = value;
		}
	}

	[JsonProperty(PropertyName = "LockAreaColor")]
	public Color LockAreaColor { get; set; }

	[JsonProperty(PropertyName = "FormalaColor")]
	public Color FormalaColor { get; set; }

	[JsonProperty(PropertyName = "CheckPassColor")]
	public Color CheckPassColor { get; set; }

	[JsonProperty(PropertyName = "CheckFailColor")]
	public Color CheckFailColor { get; set; }

	[JsonProperty(PropertyName = "RowTotalColor")]
	public Color RowTotalColor { get; set; }

	[JsonProperty(PropertyName = "RowMinusColor")]
	public Color RowMinusColor { get; set; }

	[JsonProperty(PropertyName = "RowAmoungColor")]
	public Color RowAmongColor { get; set; }

	[JsonProperty(PropertyName = "RowFixedColor")]
	public Color RowFixedColor { get; set; }

	[JsonIgnore]
	public Color AllowManualInputFormulaColor => FormalaColor;

	public TableSetting()
	{
		FontStyle = new FontSetting();
		TitleStyle = new FontSetting
		{
			FontSize = 14f
		};
		SubTitleStyle = new FontSetting();
		SubTitleRows = 2;
		MainTitleHeight = 40;
		SubTitleHeight = 30;
		SubTitleContent = new List<Tuple<string, string, string>>();
		for (int i = 0; i < SubTitleRows; i++)
		{
			SubTitleContent.Add(Tuple.Create(string.Empty, string.Empty, string.Empty));
		}
		TableRows = 10;
		TableCols = 5;
		TableRowHeight = 30;
		LockAreaColor = Color.WhiteSmoke;
		FormalaColor = Color.LightYellow;
		CheckPassColor = Color.PaleGreen;
		CheckFailColor = Color.LightCoral;
		RowTotalColor = Color.Wheat;
		RowMinusColor = Color.FromArgb(242, 220, 219);
		RowAmongColor = Color.FromArgb(220, 230, 242);
		RowFixedColor = Color.FromArgb(238, 236, 225);
	}
}
