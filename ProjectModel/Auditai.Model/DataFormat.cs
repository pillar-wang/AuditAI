using System;
using System.Collections.Generic;
using Auditai.DTO;
using Newtonsoft.Json;

namespace Auditai.Model;

public struct DataFormat
{
	private int _decimalLength;

	[JsonProperty(PropertyName = "T")]
	public DataFormatType FormatType { get; set; }

	[JsonProperty(PropertyName = "D")]
	public int DecimalLength
	{
		get
		{
			return _decimalLength;
		}
		set
		{
			_decimalLength = ((value >= 0) ? value : 0);
		}
	}

	[JsonProperty(PropertyName = "L")]
	public string ComboList { get; set; }

	public bool MultiComboList { get; set; }

	[JsonIgnore]
	public bool HasComboList => !string.IsNullOrWhiteSpace(ComboList);

	public bool IgnoreComboList { get; set; }

	[JsonProperty(PropertyName = "Z")]
	public ZeroFormat ZeroFormat { get; set; }

	[JsonProperty(PropertyName = "A")]
	public bool IsAllowEditOnExistFormula { get; set; }

	[JsonProperty(PropertyName = "LCF")]
	public string LedgerCollectFormula { get; set; }

	[JsonIgnore]
	public bool HasLedgerCollectFormula => !string.IsNullOrWhiteSpace(LedgerCollectFormula);

	private static Dictionary<bool, string> DicYesNo { get; } = new Dictionary<bool, string>
	{
		{ true, "是" },
		{ false, "否" }
	};


	private static Dictionary<bool, string> DicRightWrong { get; } = new Dictionary<bool, string>
	{
		{ true, "对" },
		{ false, "错" }
	};


	private static Dictionary<bool, string> DicTickCross { get; } = new Dictionary<bool, string>
	{
		{ true, "√" },
		{ false, "×" }
	};


	public DataFormat(DataFormatType formatType)
	{
		IsAllowEditOnExistFormula = false;
		FormatType = formatType;
		_decimalLength = 0;
		ComboList = "";
		LedgerCollectFormula = null;
		ZeroFormat = ZeroFormat.Zero;
		MultiComboList = false;
		IgnoreComboList = false;
	}

	public string GetFormatString()
	{
		switch (FormatType)
		{
		case DataFormatType.General:
			return string.Empty;
		case DataFormatType.Number:
			return "0." + new string('0', DecimalLength);
		case DataFormatType.Comma:
			return "#,##0." + new string('0', _decimalLength);
		case DataFormatType.Percentage:
			return "0." + new string('0', DecimalLength) + "%";
		case DataFormatType.NumDollar:
			return "$#,##0." + new string('0', DecimalLength);
		case DataFormatType.NumRmb:
			return "￥#,##0." + new string('0', DecimalLength);
		case DataFormatType.DateChinese:
			return "yyyy年M月d日";
		case DataFormatType.DateDash:
			return "yyyy-M-d";
		case DataFormatType.DateSlash:
			return "yyyy'/'M'/'d";
		case DataFormatType.DateDot:
			return "yyyy.M.d";
		case DataFormatType.TimeLong:
			return "hh:mm:ss";
		case DataFormatType.TimeLongChinese:
			return "hh时mm分ss秒";
		case DataFormatType.TimeShort:
			return "hh:mm";
		case DataFormatType.TimeShortChinese:
			return "hh时mm分";
		case DataFormatType.DateYearMonthChinese:
			return "yyyy年M月";
		case DataFormatType.DateYearMonthDash:
			return "yyyy-M";
		case DataFormatType.DateYearMonthSlash:
			return "yyyy'/'M";
		case DataFormatType.DateYearMonthDot:
			return "yyyy.M";
		case DataFormatType.BoolRightWrong:
		case DataFormatType.BoolTickCross:
		case DataFormatType.BoolCheckBox:
		case DataFormatType.BoolOnOff:
			return string.Empty;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public Dictionary<bool, string> GetFormatDictForBool()
	{
		switch (FormatType)
		{
		case DataFormatType.BoolYesNo:
			return DicYesNo;
		case DataFormatType.BoolRightWrong:
			return DicRightWrong;
		case DataFormatType.BoolTickCross:
		case DataFormatType.BoolOnOff:
			return DicTickCross;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	internal string Serialize()
	{
		return JsonConvert.SerializeObject(this);
	}

	internal static DataFormat? Parse(string s)
	{
		if (s == null)
		{
			return null;
		}
		return JsonConvert.DeserializeObject<DataFormat>(s);
	}

	public override bool Equals(object obj)
	{
		if (obj == (object)this)
		{
			return true;
		}
		if (obj is DataFormat dataFormat)
		{
			if (dataFormat.FormatType == FormatType && dataFormat.DecimalLength == DecimalLength && dataFormat.ComboList == ComboList && dataFormat.ZeroFormat == ZeroFormat && dataFormat.IgnoreComboList == IgnoreComboList && dataFormat.MultiComboList == MultiComboList && dataFormat.IsAllowEditOnExistFormula == IsAllowEditOnExistFormula)
			{
				return dataFormat.LedgerCollectFormula == LedgerCollectFormula;
			}
			return false;
		}
		return false;
	}

	public override int GetHashCode()
	{
		int num = -1241600518;
		num = num * -1521134295 + FormatType.GetHashCode();
		num = num * -1521134295 + DecimalLength.GetHashCode();
		if (ComboList != null)
		{
			num = num * -1521134295 + ComboList.GetHashCode();
		}
		num = num * -1521134295 + ZeroFormat.GetHashCode();
		num = num * -1521134295 + MultiComboList.GetHashCode();
		num = num * -1521134295 + IgnoreComboList.GetHashCode();
		if (IsAllowEditOnExistFormula)
		{
			num = num * -1521134295 + IsAllowEditOnExistFormula.GetHashCode();
		}
		if (LedgerCollectFormula != null && LedgerCollectFormula != string.Empty)
		{
			num = num * -1521134295 + LedgerCollectFormula.GetHashCode();
		}
		return num;
	}

	public DataFormat Clone()
	{
		DataFormat result = new DataFormat(FormatType);
		result._decimalLength = _decimalLength;
		result.ComboList = ComboList;
		result.ZeroFormat = ZeroFormat;
		result.MultiComboList = MultiComboList;
		result.IgnoreComboList = IgnoreComboList;
		result.IsAllowEditOnExistFormula = IsAllowEditOnExistFormula;
		result.LedgerCollectFormula = LedgerCollectFormula;
		return result;
	}

	public bool IsNumericFormat()
	{
		return GetDataType() == typeof(double);
	}

	public Type GetDataType()
	{
		switch (FormatType)
		{
		case DataFormatType.Number:
		case DataFormatType.Percentage:
		case DataFormatType.NumDollar:
		case DataFormatType.NumRmb:
		case DataFormatType.Comma:
			return typeof(double);
		case DataFormatType.BoolYesNo:
		case DataFormatType.BoolRightWrong:
		case DataFormatType.BoolTickCross:
		case DataFormatType.BoolCheckBox:
		case DataFormatType.BoolOnOff:
			return typeof(bool);
		default:
			return typeof(string);
		case DataFormatType.TimeLong:
		case DataFormatType.TimeShort:
		case DataFormatType.TimeLongChinese:
		case DataFormatType.TimeShortChinese:
			return typeof(TimeSpan);
		case DataFormatType.DateSlash:
		case DataFormatType.DateDash:
		case DataFormatType.DateChinese:
		case DataFormatType.DateDot:
			return typeof(DateTime);
		case DataFormatType.DateYearMonthChinese:
		case DataFormatType.DateYearMonthDash:
		case DataFormatType.DateYearMonthSlash:
		case DataFormatType.DateYearMonthDot:
			return typeof(DateYearMonth);
		}
	}
}
