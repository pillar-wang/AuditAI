using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Leqisoft.Model;

public static class CellValueSortExtend
{
	protected class ComparerRuleSetting
	{
		public Regex RegExp;

		public CellValueSortComparerBase Comparator;

		public ComparerRuleSetting(string regExp, CellValueSortComparerBase comparer)
		{
			RegExp = new Regex(regExp);
			Comparator = comparer;
		}
	}

	protected interface ISupportCompareData
	{
		int CompareTo(ISupportCompareData target);
	}

	protected abstract class EnumerableElement<TSource> : ISupportCompareData
	{
		public TSource Data;

		public abstract int CompareTo(ISupportCompareData target);
	}

	protected interface CellValueSortComparerBase : IComparer<object>
	{
		EnumerableElement<TSource> ParseCompareKey<TSource>(TSource data, Regex reg, string keyValue);
	}

	protected class CellValueSortComparer_StringNumber : CellValueSortComparerBase, IComparer<object>
	{
		protected class ElementData_StringNumber<TSource> : EnumerableElement<TSource>
		{
			public string StrValue;

			public double NumberValue;

			public ElementData_StringNumber(TSource data)
			{
				Data = data;
				StrValue = string.Empty;
				NumberValue = 0.0;
			}

			public ElementData_StringNumber(TSource data, string strValue, double numberValue)
			{
				Data = data;
				StrValue = strValue;
				NumberValue = numberValue;
			}

			public override int CompareTo(ISupportCompareData target)
			{
				ElementData_StringNumber<TSource> elementData_StringNumber = target as ElementData_StringNumber<TSource>;
				if (target == null)
				{
					return 1;
				}
				int num = StrValue.CompareTo(elementData_StringNumber.StrValue);
				if (num != 0)
				{
					return num;
				}
				return NumberValue.CompareTo(elementData_StringNumber.NumberValue);
			}
		}

		public static CellValueSortComparer_StringNumber Instance = new CellValueSortComparer_StringNumber();

		public const string RegExp = "^(\\D+)(\\d+)$";

		public EnumerableElement<TSource> ParseCompareKey<TSource>(TSource data, Regex reg, string keyValue)
		{
			if (string.IsNullOrWhiteSpace(keyValue))
			{
				return new ElementData_StringNumber<TSource>(data);
			}
			Match match = reg.Match(keyValue);
			if (!match.Success)
			{
				return null;
			}
			string value = match.Groups[1].Value;
			string value2 = match.Groups[2].Value;
			if (!double.TryParse(value2, out var result))
			{
				return null;
			}
			return new ElementData_StringNumber<TSource>(data, value, result);
		}

		public int Compare(object x, object y)
		{
			ISupportCompareData supportCompareData = x as ISupportCompareData;
			ISupportCompareData supportCompareData2 = y as ISupportCompareData;
			if (supportCompareData == null && supportCompareData2 == null)
			{
				return 0;
			}
			if (supportCompareData == null)
			{
				return -1;
			}
			if (supportCompareData2 == null)
			{
				return 1;
			}
			return supportCompareData.CompareTo(supportCompareData2);
		}
	}

	protected class CellValueSortComparer_NumberString : CellValueSortComparerBase, IComparer<object>
	{
		protected class ElementData_NumberString<TSource> : EnumerableElement<TSource>
		{
			public string StrValue;

			public double NumberValue;

			public ElementData_NumberString(TSource data)
			{
				Data = data;
				StrValue = string.Empty;
				NumberValue = 0.0;
			}

			public ElementData_NumberString(TSource data, string strValue, double numberValue)
			{
				Data = data;
				StrValue = strValue;
				NumberValue = numberValue;
			}

			public override int CompareTo(ISupportCompareData target)
			{
				ElementData_NumberString<TSource> elementData_NumberString = target as ElementData_NumberString<TSource>;
				if (target == null)
				{
					return 1;
				}
				int num = NumberValue.CompareTo(elementData_NumberString.NumberValue);
				if (num != 0)
				{
					return num;
				}
				return StrValue.CompareTo(elementData_NumberString.StrValue);
			}
		}

		public static CellValueSortComparer_NumberString Instance = new CellValueSortComparer_NumberString();

		public const string RegExp = "^(\\d+)(\\D+)$";

		public EnumerableElement<TSource> ParseCompareKey<TSource>(TSource data, Regex reg, string keyValue)
		{
			if (string.IsNullOrWhiteSpace(keyValue))
			{
				return new ElementData_NumberString<TSource>(data);
			}
			Match match = reg.Match(keyValue);
			if (!match.Success)
			{
				return null;
			}
			string value = match.Groups[1].Value;
			string value2 = match.Groups[2].Value;
			if (!double.TryParse(value, out var result))
			{
				return null;
			}
			return new ElementData_NumberString<TSource>(data, value2, result);
		}

		public int Compare(object x, object y)
		{
			ISupportCompareData supportCompareData = x as ISupportCompareData;
			ISupportCompareData supportCompareData2 = y as ISupportCompareData;
			if (supportCompareData == null && supportCompareData2 == null)
			{
				return 0;
			}
			if (supportCompareData == null)
			{
				return -1;
			}
			if (supportCompareData2 == null)
			{
				return 1;
			}
			return supportCompareData.CompareTo(supportCompareData2);
		}
	}

	protected class CellValueSortComparer_DateString : CellValueSortComparerBase, IComparer<object>
	{
		protected class ElementData_DateString<TSource> : EnumerableElement<TSource>
		{
			public DateTime DateValue;

			public string StrValue;

			public ElementData_DateString(TSource data)
			{
				Data = data;
				DateValue = DateTime.MinValue;
				StrValue = string.Empty;
			}

			public ElementData_DateString(TSource data, DateTime dateValue, string strValue)
			{
				Data = data;
				DateValue = dateValue;
				StrValue = strValue;
			}

			public override int CompareTo(ISupportCompareData target)
			{
				ElementData_DateString<TSource> elementData_DateString = target as ElementData_DateString<TSource>;
				if (target == null)
				{
					return 1;
				}
				int num = DateValue.CompareTo(elementData_DateString.DateValue);
				if (num != 0)
				{
					return num;
				}
				return StrValue.CompareTo(elementData_DateString.StrValue);
			}
		}

		public static CellValueSortComparer_DateString Instance = new CellValueSortComparer_DateString();

		public const string RegExp = "^(\\d{1,4}[年|\\-|/|\\\\|\\.]\\d{1,2}[月|\\-|/|\\\\|\\.]\\d{1,2}[日|号]{0,1})[\\-|—|¦|_|\\|]{1}([\\s\\S]*)$";

		public virtual EnumerableElement<TSource> ParseCompareKey<TSource>(TSource data, Regex reg, string keyValue)
		{
			if (string.IsNullOrWhiteSpace(keyValue))
			{
				return new ElementData_DateString<TSource>(data);
			}
			Match match = reg.Match(keyValue);
			if (!match.Success)
			{
				return null;
			}
			string value = match.Groups[1].Value;
			string value2 = match.Groups[2].Value;
			if (!TryParseDataTime(value, out var dateTime))
			{
				return null;
			}
			return new ElementData_DateString<TSource>(data, dateTime, value2);
		}

		protected virtual bool TryParseDataTime(string dayText, out DateTime dateTime)
		{
			if (DateTime.TryParse(dayText, out var result))
			{
				dateTime = result;
				return true;
			}
			try
			{
				DateTime dateTime2 = Convert.ToDateTime(dayText);
				dateTime = dateTime2;
				return true;
			}
			catch
			{
			}
			try
			{
				string text = dayText.Split(' ')[0];
				text = text.Replace('年', '-');
				text = text.Replace('月', '-');
				text = text.Replace('日', '-');
				text = text.Replace('号', '-');
				if (text.EndsWith("-"))
				{
					text = text.Substring(0, text.Length - 1);
				}
				DateTime dateTime3 = Convert.ToDateTime(text);
				dateTime = dateTime3;
				return true;
			}
			catch
			{
			}
			dateTime = DateTime.Now;
			return false;
		}

		public int Compare(object x, object y)
		{
			ISupportCompareData supportCompareData = x as ISupportCompareData;
			ISupportCompareData supportCompareData2 = y as ISupportCompareData;
			if (supportCompareData == null && supportCompareData2 == null)
			{
				return 0;
			}
			if (supportCompareData == null)
			{
				return -1;
			}
			if (supportCompareData2 == null)
			{
				return 1;
			}
			return supportCompareData.CompareTo(supportCompareData2);
		}
	}

	protected class CellValueSortComparer_StringDate : CellValueSortComparerBase, IComparer<object>
	{
		protected class ElementData_StringDate<TSource> : EnumerableElement<TSource>
		{
			public DateTime DateValue;

			public string StrValue;

			public ElementData_StringDate(TSource data)
			{
				Data = data;
				DateValue = DateTime.MinValue;
				StrValue = string.Empty;
			}

			public ElementData_StringDate(TSource data, DateTime dateValue, string strValue)
			{
				Data = data;
				DateValue = dateValue;
				StrValue = strValue;
			}

			public override int CompareTo(ISupportCompareData target)
			{
				ElementData_StringDate<TSource> elementData_StringDate = target as ElementData_StringDate<TSource>;
				if (target == null)
				{
					return 1;
				}
				int num = StrValue.CompareTo(elementData_StringDate.StrValue);
				if (num != 0)
				{
					return num;
				}
				return DateValue.CompareTo(elementData_StringDate.DateValue);
			}
		}

		public static CellValueSortComparer_StringDate Instance = new CellValueSortComparer_StringDate();

		public const string RegExp = "^([\\s\\S]*)[\\-|—|¦|_|\\|]{1}(\\d{1,4}[年|\\-|/|\\\\|\\.]\\d{1,2}[月|\\-|/|\\\\|\\.]\\d{1,2}[日|号]{0,1})$";

		public virtual EnumerableElement<TSource> ParseCompareKey<TSource>(TSource data, Regex reg, string keyValue)
		{
			if (string.IsNullOrWhiteSpace(keyValue))
			{
				return new ElementData_StringDate<TSource>(data);
			}
			Match match = reg.Match(keyValue);
			if (!match.Success)
			{
				return null;
			}
			string value = match.Groups[2].Value;
			string value2 = match.Groups[1].Value;
			if (!TryParseDataTime(value, out var dateTime))
			{
				return null;
			}
			return new ElementData_StringDate<TSource>(data, dateTime, value2);
		}

		protected virtual bool TryParseDataTime(string dayText, out DateTime dateTime)
		{
			if (DateTime.TryParse(dayText, out var result))
			{
				dateTime = result;
				return true;
			}
			try
			{
				DateTime dateTime2 = Convert.ToDateTime(dayText);
				dateTime = dateTime2;
				return true;
			}
			catch
			{
			}
			try
			{
				string text = dayText.Split(' ')[0];
				text = text.Replace('年', '-');
				text = text.Replace('月', '-');
				text = text.Replace('日', '-');
				text = text.Replace('号', '-');
				if (text.EndsWith("-"))
				{
					text = text.Substring(0, text.Length - 1);
				}
				DateTime dateTime3 = Convert.ToDateTime(text);
				dateTime = dateTime3;
				return true;
			}
			catch
			{
			}
			dateTime = DateTime.Now;
			return false;
		}

		public int Compare(object x, object y)
		{
			ISupportCompareData supportCompareData = x as ISupportCompareData;
			ISupportCompareData supportCompareData2 = y as ISupportCompareData;
			if (supportCompareData == null && supportCompareData2 == null)
			{
				return 0;
			}
			if (supportCompareData == null)
			{
				return -1;
			}
			if (supportCompareData2 == null)
			{
				return 1;
			}
			return supportCompareData.CompareTo(supportCompareData2);
		}
	}

	protected class CellValueSortComparer_YearMonthString : CellValueSortComparerBase, IComparer<object>
	{
		protected class ElementData_YearMonthString<TSource> : EnumerableElement<TSource>
		{
			public DateTime DateValue;

			public string StrValue;

			public ElementData_YearMonthString(TSource data)
			{
				Data = data;
				DateValue = DateTime.MinValue;
				StrValue = string.Empty;
			}

			public ElementData_YearMonthString(TSource data, DateTime dateValue, string strValue)
			{
				Data = data;
				DateValue = dateValue;
				StrValue = strValue;
			}

			public override int CompareTo(ISupportCompareData target)
			{
				ElementData_YearMonthString<TSource> elementData_YearMonthString = target as ElementData_YearMonthString<TSource>;
				if (target == null)
				{
					return 1;
				}
				int num = DateValue.CompareTo(elementData_YearMonthString.DateValue);
				if (num != 0)
				{
					return num;
				}
				return StrValue.CompareTo(elementData_YearMonthString.StrValue);
			}
		}

		public static CellValueSortComparer_YearMonthString Instance = new CellValueSortComparer_YearMonthString();

		public const string RegExp = "^(\\d{1,4}[年|\\-|/|\\\\|\\.]\\d{1,2}[月]{0,1})[\\-|—|¦|_|\\|]{1}([\\s\\S]*)$";

		public virtual EnumerableElement<TSource> ParseCompareKey<TSource>(TSource data, Regex reg, string keyValue)
		{
			if (string.IsNullOrWhiteSpace(keyValue))
			{
				return new ElementData_YearMonthString<TSource>(data);
			}
			Match match = reg.Match(keyValue);
			if (!match.Success)
			{
				return null;
			}
			string value = match.Groups[1].Value;
			string value2 = match.Groups[2].Value;
			if (!TryParseYearMonth(value, out var dateTime))
			{
				return null;
			}
			return new ElementData_YearMonthString<TSource>(data, dateTime, value2);
		}

		protected virtual bool TryParseYearMonth(string dayText, out DateTime dateTime)
		{
			if (DateTime.TryParse(dayText, out var result))
			{
				dateTime = new DateTime(result.Year, result.Month, 1);
				return true;
			}
			try
			{
				DateTime dateTime2 = Convert.ToDateTime(dayText);
				dateTime = new DateTime(dateTime2.Year, dateTime2.Month, 1);
				return true;
			}
			catch
			{
			}
			try
			{
				string text = dayText.Split(' ')[0];
				text = text.Replace('年', '-');
				text = text.Replace('月', '-');
				text = text.Replace('日', '-');
				text = text.Replace('号', '-');
				if (text.EndsWith("-"))
				{
					text = text.Substring(0, text.Length - 1);
				}
				if (DateTime.TryParse(text, out var result2))
				{
					dateTime = new DateTime(result2.Year, result2.Month, 1);
					return true;
				}
				DateTime dateTime3 = Convert.ToDateTime(text);
				dateTime = new DateTime(dateTime3.Year, dateTime3.Month, 1);
				return true;
			}
			catch
			{
			}
			dateTime = DateTime.Now;
			return false;
		}

		public int Compare(object x, object y)
		{
			ISupportCompareData supportCompareData = x as ISupportCompareData;
			ISupportCompareData supportCompareData2 = y as ISupportCompareData;
			if (supportCompareData == null && supportCompareData2 == null)
			{
				return 0;
			}
			if (supportCompareData == null)
			{
				return -1;
			}
			if (supportCompareData2 == null)
			{
				return 1;
			}
			return supportCompareData.CompareTo(supportCompareData2);
		}
	}

	protected class CellValueSortComparer_StringYearMonth : CellValueSortComparerBase, IComparer<object>
	{
		protected class ElementData_StringYearMonth<TSource> : EnumerableElement<TSource>
		{
			public DateTime DateValue;

			public string StrValue;

			public ElementData_StringYearMonth(TSource data)
			{
				Data = data;
				DateValue = DateTime.MinValue;
				StrValue = string.Empty;
			}

			public ElementData_StringYearMonth(TSource data, DateTime dateValue, string strValue)
			{
				Data = data;
				DateValue = dateValue;
				StrValue = strValue;
			}

			public override int CompareTo(ISupportCompareData target)
			{
				ElementData_StringYearMonth<TSource> elementData_StringYearMonth = target as ElementData_StringYearMonth<TSource>;
				if (target == null)
				{
					return 1;
				}
				int num = StrValue.CompareTo(elementData_StringYearMonth.StrValue);
				if (num != 0)
				{
					return num;
				}
				return DateValue.CompareTo(elementData_StringYearMonth.DateValue);
			}
		}

		public static CellValueSortComparer_StringYearMonth Instance = new CellValueSortComparer_StringYearMonth();

		public const string RegExp = "^([\\s\\S]*)[\\-|—|¦|_|\\|]{1}(\\d{1,4}[年|\\-|/|\\\\|\\.]\\d{1,2}[月]{0,1})$";

		public virtual EnumerableElement<TSource> ParseCompareKey<TSource>(TSource data, Regex reg, string keyValue)
		{
			if (string.IsNullOrWhiteSpace(keyValue))
			{
				return new ElementData_StringYearMonth<TSource>(data);
			}
			Match match = reg.Match(keyValue);
			if (!match.Success)
			{
				return null;
			}
			string value = match.Groups[2].Value;
			string value2 = match.Groups[1].Value;
			if (!TryParseYearMonth(value, out var dateTime))
			{
				return null;
			}
			return new ElementData_StringYearMonth<TSource>(data, dateTime, value2);
		}

		protected virtual bool TryParseYearMonth(string dayText, out DateTime dateTime)
		{
			if (DateTime.TryParse(dayText, out var result))
			{
				dateTime = new DateTime(result.Year, result.Month, 1);
				return true;
			}
			try
			{
				DateTime dateTime2 = Convert.ToDateTime(dayText);
				dateTime = new DateTime(dateTime2.Year, dateTime2.Month, 1);
				return true;
			}
			catch
			{
			}
			try
			{
				string text = dayText.Split(' ')[0];
				text = text.Replace('年', '-');
				text = text.Replace('月', '-');
				text = text.Replace('日', '-');
				text = text.Replace('号', '-');
				if (text.EndsWith("-"))
				{
					text = text.Substring(0, text.Length - 1);
				}
				if (DateTime.TryParse(text, out var result2))
				{
					dateTime = new DateTime(result2.Year, result2.Month, 1);
					return true;
				}
				DateTime dateTime3 = Convert.ToDateTime(text);
				dateTime = new DateTime(dateTime3.Year, dateTime3.Month, 1);
				return true;
			}
			catch
			{
			}
			dateTime = DateTime.Now;
			return false;
		}

		public int Compare(object x, object y)
		{
			ISupportCompareData supportCompareData = x as ISupportCompareData;
			ISupportCompareData supportCompareData2 = y as ISupportCompareData;
			if (supportCompareData == null && supportCompareData2 == null)
			{
				return 0;
			}
			if (supportCompareData == null)
			{
				return -1;
			}
			if (supportCompareData2 == null)
			{
				return 1;
			}
			return supportCompareData.CompareTo(supportCompareData2);
		}
	}

	protected class CellValueSortComparer_CharNumberCharString : CellValueSortComparerBase, IComparer<object>
	{
		protected class ElementData_CharNumberCharString<TSource> : EnumerableElement<TSource>
		{
			public string StrValue;

			public double NumberValue;

			public ElementData_CharNumberCharString(TSource data)
			{
				Data = data;
				StrValue = string.Empty;
				NumberValue = 0.0;
			}

			public ElementData_CharNumberCharString(TSource data, string strValue, double numberValue)
			{
				Data = data;
				StrValue = strValue;
				NumberValue = numberValue;
			}

			public override int CompareTo(ISupportCompareData target)
			{
				ElementData_CharNumberCharString<TSource> elementData_CharNumberCharString = target as ElementData_CharNumberCharString<TSource>;
				if (target == null)
				{
					return 1;
				}
				int num = NumberValue.CompareTo(elementData_CharNumberCharString.NumberValue);
				if (num != 0)
				{
					return num;
				}
				return StrValue.CompareTo(elementData_CharNumberCharString.StrValue);
			}
		}

		public static CellValueSortComparer_CharNumberCharString Instance = new CellValueSortComparer_CharNumberCharString();

		public const string RegExp = "^([\\(|（|\\[|【]{1})(\\d+)([\\)|）|\\]|】]{1})([\\s\\S]*)$";

		public EnumerableElement<TSource> ParseCompareKey<TSource>(TSource data, Regex reg, string keyValue)
		{
			if (string.IsNullOrWhiteSpace(keyValue))
			{
				return new ElementData_CharNumberCharString<TSource>(data);
			}
			Match match = reg.Match(keyValue);
			if (!match.Success)
			{
				return null;
			}
			string value = match.Groups[2].Value;
			string value2 = match.Groups[4].Value;
			if (!double.TryParse(value, out var result))
			{
				return null;
			}
			return new ElementData_CharNumberCharString<TSource>(data, value2, result);
		}

		public int Compare(object x, object y)
		{
			ISupportCompareData supportCompareData = x as ISupportCompareData;
			ISupportCompareData supportCompareData2 = y as ISupportCompareData;
			if (supportCompareData == null && supportCompareData2 == null)
			{
				return 0;
			}
			if (supportCompareData == null)
			{
				return -1;
			}
			if (supportCompareData2 == null)
			{
				return 1;
			}
			return supportCompareData.CompareTo(supportCompareData2);
		}
	}

	protected class CellValueSortComparer_Date : CellValueSortComparerBase, IComparer<object>
	{
		protected class ElementData_Date<TSource> : EnumerableElement<TSource>
		{
			public DateTime DateValue;

			public ElementData_Date(TSource data)
			{
				Data = data;
				DateValue = DateTime.MinValue;
			}

			public ElementData_Date(TSource data, DateTime dateValue)
			{
				Data = data;
				DateValue = dateValue;
			}

			public override int CompareTo(ISupportCompareData target)
			{
				ElementData_Date<TSource> elementData_Date = target as ElementData_Date<TSource>;
				if (target == null)
				{
					return 1;
				}
				return DateValue.CompareTo(elementData_Date.DateValue);
			}
		}

		public static CellValueSortComparer_Date Instance = new CellValueSortComparer_Date();

		public const string RegExp = "^(\\d{1,4}[年|\\-|/|\\\\|\\.]\\d{1,2}[月|\\-|/|\\\\|\\.]\\d{1,2}[日|号]{0,1})$";

		public virtual EnumerableElement<TSource> ParseCompareKey<TSource>(TSource data, Regex reg, string keyValue)
		{
			if (string.IsNullOrWhiteSpace(keyValue))
			{
				return new ElementData_Date<TSource>(data);
			}
			Match match = reg.Match(keyValue);
			if (!match.Success)
			{
				return null;
			}
			string value = match.Groups[1].Value;
			if (!TryParseDataTime(value, out var dateTime))
			{
				return null;
			}
			return new ElementData_Date<TSource>(data, dateTime);
		}

		protected virtual bool TryParseDataTime(string dayText, out DateTime dateTime)
		{
			if (DateTime.TryParse(dayText, out var result))
			{
				dateTime = result;
				return true;
			}
			try
			{
				DateTime dateTime2 = Convert.ToDateTime(dayText);
				dateTime = dateTime2;
				return true;
			}
			catch
			{
			}
			try
			{
				string text = dayText.Split(' ')[0];
				text = text.Replace('年', '-');
				text = text.Replace('月', '-');
				text = text.Replace('日', '-');
				text = text.Replace('号', '-');
				if (text.EndsWith("-"))
				{
					text = text.Substring(0, text.Length - 1);
				}
				DateTime dateTime3 = Convert.ToDateTime(text);
				dateTime = dateTime3;
				return true;
			}
			catch
			{
			}
			dateTime = DateTime.Now;
			return false;
		}

		public int Compare(object x, object y)
		{
			ISupportCompareData supportCompareData = x as ISupportCompareData;
			ISupportCompareData supportCompareData2 = y as ISupportCompareData;
			if (supportCompareData == null && supportCompareData2 == null)
			{
				return 0;
			}
			if (supportCompareData == null)
			{
				return -1;
			}
			if (supportCompareData2 == null)
			{
				return 1;
			}
			return supportCompareData.CompareTo(supportCompareData2);
		}
	}

	protected class CellValueSortComparer_YearMonth : CellValueSortComparerBase, IComparer<object>
	{
		protected class ElementData_YearMonth<TSource> : EnumerableElement<TSource>
		{
			public DateTime DateValue;

			public ElementData_YearMonth(TSource data)
			{
				Data = data;
				DateValue = DateTime.MinValue;
			}

			public ElementData_YearMonth(TSource data, DateTime dateValue)
			{
				Data = data;
				DateValue = dateValue;
			}

			public override int CompareTo(ISupportCompareData target)
			{
				ElementData_YearMonth<TSource> elementData_YearMonth = target as ElementData_YearMonth<TSource>;
				if (target == null)
				{
					return 1;
				}
				return DateValue.CompareTo(elementData_YearMonth.DateValue);
			}
		}

		public static CellValueSortComparer_YearMonth Instance = new CellValueSortComparer_YearMonth();

		public const string RegExp = "^(\\d{1,4}[年|\\-|/|\\\\|\\.]\\d{1,2}[月]{0,1})$";

		public virtual EnumerableElement<TSource> ParseCompareKey<TSource>(TSource data, Regex reg, string keyValue)
		{
			if (string.IsNullOrWhiteSpace(keyValue))
			{
				return new ElementData_YearMonth<TSource>(data);
			}
			Match match = reg.Match(keyValue);
			if (!match.Success)
			{
				return null;
			}
			string value = match.Groups[1].Value;
			if (!TryParseDataTime(value, out var dateTime))
			{
				return null;
			}
			return new ElementData_YearMonth<TSource>(data, dateTime);
		}

		protected virtual bool TryParseDataTime(string dayText, out DateTime dateTime)
		{
			dayText = dayText.Replace('.', '-');
			dayText = dayText.Replace('/', '-');
			dayText = dayText.Replace('\\', '-');
			dayText = dayText.Replace('年', '-');
			dayText = dayText.Replace('月', '-');
			dayText = ((!dayText.EndsWith("-")) ? (dayText + "-1") : (dayText + "1"));
			if (DateTime.TryParse(dayText, out var result))
			{
				dateTime = result;
				return true;
			}
			try
			{
				DateTime dateTime2 = Convert.ToDateTime(dayText);
				dateTime = dateTime2;
				return true;
			}
			catch
			{
			}
			try
			{
				string text = dayText.Split(' ')[0];
				if (text.EndsWith("-"))
				{
					text = text.Substring(0, text.Length - 1);
				}
				DateTime dateTime3 = Convert.ToDateTime(text);
				dateTime = dateTime3;
				return true;
			}
			catch
			{
			}
			dateTime = DateTime.Now;
			return false;
		}

		public int Compare(object x, object y)
		{
			ISupportCompareData supportCompareData = x as ISupportCompareData;
			ISupportCompareData supportCompareData2 = y as ISupportCompareData;
			if (supportCompareData == null && supportCompareData2 == null)
			{
				return 0;
			}
			if (supportCompareData == null)
			{
				return -1;
			}
			if (supportCompareData2 == null)
			{
				return 1;
			}
			return supportCompareData.CompareTo(supportCompareData2);
		}
	}

	private static List<ComparerRuleSetting> CompareRuleList;

	static CellValueSortExtend()
	{
		CompareRuleList = new List<ComparerRuleSetting>();
		CompareRuleList.Add(new ComparerRuleSetting("^(\\D+)(\\d+)$", CellValueSortComparer_StringNumber.Instance));
		CompareRuleList.Add(new ComparerRuleSetting("^(\\d{1,4}[年|\\-|/|\\\\|\\.]\\d{1,2}[月|\\-|/|\\\\|\\.]\\d{1,2}[日|号]{0,1})[\\-|—|¦|_|\\|]{1}([\\s\\S]*)$", CellValueSortComparer_DateString.Instance));
		CompareRuleList.Add(new ComparerRuleSetting("^(\\d{1,4}[年|\\-|/|\\\\|\\.]\\d{1,2}[月]{0,1})[\\-|—|¦|_|\\|]{1}([\\s\\S]*)$", CellValueSortComparer_YearMonthString.Instance));
		CompareRuleList.Add(new ComparerRuleSetting("^([\\(|（|\\[|【]{1})(\\d+)([\\)|）|\\]|】]{1})([\\s\\S]*)$", CellValueSortComparer_CharNumberCharString.Instance));
		CompareRuleList.Add(new ComparerRuleSetting("^(\\d+)(\\D+)$", CellValueSortComparer_NumberString.Instance));
		CompareRuleList.Add(new ComparerRuleSetting("^([\\s\\S]*)[\\-|—|¦|_|\\|]{1}(\\d{1,4}[年|\\-|/|\\\\|\\.]\\d{1,2}[月|\\-|/|\\\\|\\.]\\d{1,2}[日|号]{0,1})$", CellValueSortComparer_StringDate.Instance));
		CompareRuleList.Add(new ComparerRuleSetting("^([\\s\\S]*)[\\-|—|¦|_|\\|]{1}(\\d{1,4}[年|\\-|/|\\\\|\\.]\\d{1,2}[月]{0,1})$", CellValueSortComparer_StringYearMonth.Instance));
		CompareRuleList.Add(new ComparerRuleSetting("^(\\d{1,4}[年|\\-|/|\\\\|\\.]\\d{1,2}[月|\\-|/|\\\\|\\.]\\d{1,2}[日|号]{0,1})$", CellValueSortComparer_Date.Instance));
		CompareRuleList.Add(new ComparerRuleSetting("^(\\d{1,4}[年|\\-|/|\\\\|\\.]\\d{1,2}[月]{0,1})$", CellValueSortComparer_YearMonth.Instance));
	}

	public static IEnumerable<TSource> OrderByCellValueDescending<TSource>(this IEnumerable<TSource> source, Func<TSource, object> keySelector)
	{
		return source.OrderByCellValueImpl(keySelector, descending: true);
	}

	public static IEnumerable<TSource> OrderByCellValue<TSource>(this IEnumerable<TSource> source, Func<TSource, object> keySelector)
	{
		return source.OrderByCellValueImpl(keySelector, descending: false);
	}

	private static IEnumerable<TSource> OrderByCellValueImpl<TSource>(this IEnumerable<TSource> source, Func<TSource, object> keySelector, bool descending)
	{
		string text = null;
		foreach (TSource item in source)
		{
			object obj = keySelector(item);
			if (!(obj is string text2))
			{
				if (descending)
				{
					return source.OrderByDescending(keySelector, CellValueSortComparer.Instance);
				}
				return source.OrderBy(keySelector, CellValueSortComparer.Instance);
			}
			if (!string.IsNullOrWhiteSpace(text2))
			{
				text = text2;
				break;
			}
		}
		if (text == null)
		{
			if (descending)
			{
				return source.OrderByDescending(keySelector, CellValueSortComparer.Instance);
			}
			return source.OrderBy(keySelector, CellValueSortComparer.Instance);
		}
		ComparerRuleSetting comparerRuleSetting = null;
		foreach (ComparerRuleSetting compareRule in CompareRuleList)
		{
			if (compareRule.RegExp.IsMatch(text))
			{
				comparerRuleSetting = compareRule;
				break;
			}
		}
		if (comparerRuleSetting == null)
		{
			if (descending)
			{
				return source.OrderByDescending(keySelector, CellValueSortComparer.Instance);
			}
			return source.OrderBy(keySelector, CellValueSortComparer.Instance);
		}
		bool flag = true;
		List<EnumerableElement<TSource>> list = new List<EnumerableElement<TSource>>();
		foreach (TSource item2 in source)
		{
			object obj2 = keySelector(item2);
			if (!(obj2 is string keyValue))
			{
				break;
			}
			EnumerableElement<TSource> enumerableElement = comparerRuleSetting.Comparator.ParseCompareKey(item2, comparerRuleSetting.RegExp, keyValue);
			if (enumerableElement == null)
			{
				flag = false;
				break;
			}
			list.Add(enumerableElement);
		}
		if (!flag)
		{
			if (descending)
			{
				return source.OrderByDescending(keySelector, CellValueSortComparer.Instance);
			}
			return source.OrderBy(keySelector, CellValueSortComparer.Instance);
		}
		if (descending)
		{
			return from u in list.OrderByDescending((EnumerableElement<TSource> u) => u, comparerRuleSetting.Comparator)
				select u.Data;
		}
		return from u in list.OrderBy((EnumerableElement<TSource> u) => u, comparerRuleSetting.Comparator)
			select u.Data;
	}
}
