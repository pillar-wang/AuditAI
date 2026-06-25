using System;
using Auditai.DTO;
using Newtonsoft.Json;

namespace Auditai.Model;

public sealed class DateYearMonthFilter : ByIndividualValueFilter
{
	[JsonProperty]
	public DateYearMonth Value { get; set; }

	[JsonProperty]
	public DateYearMonth Value2 { get; set; }

	public sealed override bool Execute(FilterValue value)
	{
		if (value.DataType == FilterDataType.DateYearMonth)
		{
			return Predicate(value.DateYearMonth);
		}
		return false;
	}

	private bool Predicate(DateYearMonth fv)
	{
		switch (base.Kind)
		{
		case FilterKind.DateYearMonthEq:
			return fv.CompareTo(Value) == 0;
		case FilterKind.DateYearMonthBefore:
			return fv.CompareTo(Value) < 0;
		case FilterKind.DateYearMonthAfter:
			return fv.CompareTo(Value) > 0;
		case FilterKind.DateYearMonthNe:
			return fv.CompareTo(Value) != 0;
		case FilterKind.DateYearMonthBeforeEq:
			return fv.CompareTo(Value) <= 0;
		case FilterKind.DateYearMonthAfterEq:
			return fv.CompareTo(Value) >= 0;
		case FilterKind.DateYearMonthBetween:
			if (fv.CompareTo(Value) >= 0)
			{
				return fv.CompareTo(Value2) <= 0;
			}
			return false;
		case FilterKind.DateYearMonthOutside:
			if (fv.CompareTo(Value) >= 0)
			{
				return fv.CompareTo(Value2) > 0;
			}
			return true;
		case FilterKind.DateYearMonthYear:
			return fv.Date.Year == Value.Date.Year;
		case FilterKind.DateYearMonthSeason:
			if (fv.Date.Year == Value.Date.Year)
			{
				return (fv.Date.Month - 1) / 3 == (Value.Date.Month - 1) / 3;
			}
			return false;
		case FilterKind.DateYearMonthMonth:
			if (fv.Date.Year == Value.Date.Year)
			{
				return fv.Date.Month == Value.Date.Month;
			}
			return false;
		case FilterKind.DateYearMonthCurrentMonth:
			if (fv.Date.Year == DateTime.Today.Year)
			{
				return fv.Date.Month == DateTime.Today.Month;
			}
			return false;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public override string ToFormula(string colText)
	{
		return base.Kind switch
		{
			FilterKind.DateYearMonthEq => colText + "=" + DateYearMonthToFormula(Value), 
			FilterKind.DateYearMonthBefore => colText + "<" + DateYearMonthToFormula(Value), 
			FilterKind.DateYearMonthAfter => colText + ">" + DateYearMonthToFormula(Value), 
			FilterKind.DateYearMonthNe => colText + "<>" + DateYearMonthToFormula(Value), 
			FilterKind.DateYearMonthBeforeEq => colText + "<=" + DateYearMonthToFormula(Value), 
			FilterKind.DateYearMonthAfterEq => colText + ">=" + DateYearMonthToFormula(Value), 
			FilterKind.DateYearMonthBetween => colText + ">=" + DateYearMonthToFormula(Value) + " And " + colText + "<=" + DateYearMonthToFormula(Value2), 
			FilterKind.DateYearMonthOutside => colText + "<" + DateYearMonthToFormula(Value) + " Or " + colText + ">" + DateYearMonthToFormula(Value2), 
			FilterKind.DateYearMonthYear => $"Year({colText})={Value.Date.Year}", 
			FilterKind.DateYearMonthSeason => $"Year({colText})={Value.Date.Year} And Quotient(Month({colText})-1,3)={(Value.Date.Month - 1) / 3}", 
			FilterKind.DateYearMonthMonth => $"Year({colText})={Value.Date.Year} And Month({colText})={Value.Date.Month}", 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	private static string DateYearMonthToFormula(DateYearMonth dt)
	{
		return $"DateYearMonth({dt.Date.Year},{dt.Date.Month})";
	}
}
