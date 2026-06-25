using System;
using System.Globalization;
using Newtonsoft.Json;

namespace Auditai.Model;

public sealed class DateFilter : ByIndividualValueFilter
{
	[JsonProperty]
	public DateTime Value { get; set; }

	[JsonProperty]
	public DateTime Value2 { get; set; }

	public sealed override bool Execute(FilterValue value)
	{
		if (value.DataType == FilterDataType.Date)
		{
			return Predicate(value.Date);
		}
		return false;
	}

	private bool Predicate(DateTime fv)
	{
		switch (base.Kind)
		{
		case FilterKind.DateEq:
			return fv == Value;
		case FilterKind.Before:
			return fv < Value;
		case FilterKind.After:
			return fv > Value;
		case FilterKind.DateNe:
			return fv != Value;
		case FilterKind.BeforeEq:
			return fv <= Value;
		case FilterKind.AfterEq:
			return fv >= Value;
		case FilterKind.DateBetween:
			if (fv >= Value)
			{
				return fv <= Value2;
			}
			return false;
		case FilterKind.DateOutside:
			if (!(fv < Value))
			{
				return fv > Value2;
			}
			return true;
		case FilterKind.Year:
			return fv.Year == Value.Year;
		case FilterKind.Season:
			if (fv.Year == Value.Year)
			{
				return (fv.Month - 1) / 3 == (Value.Month - 1) / 3;
			}
			return false;
		case FilterKind.Month:
			if (fv.Year == Value.Year)
			{
				return fv.Month == Value.Month;
			}
			return false;
		case FilterKind.Week:
			return SameWeek(fv, Value);
		case FilterKind.Today:
			return fv == DateTime.Today;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public override string ToFormula(string colText)
	{
		return base.Kind switch
		{
			FilterKind.DateEq => colText + "=" + DateTimeToFormula(Value), 
			FilterKind.Before => colText + "<" + DateTimeToFormula(Value), 
			FilterKind.After => colText + ">" + DateTimeToFormula(Value), 
			FilterKind.DateNe => colText + "<>" + DateTimeToFormula(Value), 
			FilterKind.BeforeEq => colText + "<=" + DateTimeToFormula(Value), 
			FilterKind.AfterEq => colText + ">=" + DateTimeToFormula(Value), 
			FilterKind.DateBetween => colText + ">=" + DateTimeToFormula(Value) + " And " + colText + "<=" + DateTimeToFormula(Value2), 
			FilterKind.DateOutside => colText + "<" + DateTimeToFormula(Value) + " Or " + colText + ">" + DateTimeToFormula(Value2), 
			FilterKind.Year => $"Year({colText})={Value.Year}", 
			FilterKind.Season => $"Year({colText})={Value.Year} And Quotient(Month({colText})-1,3)={(Value.Month - 1) / 3}", 
			FilterKind.Month => $"Year({colText})={Value.Year} And Month({colText})={Value.Month}", 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	private static string DateTimeToFormula(DateTime dt)
	{
		return $"Date({dt.Year},{dt.Month},{dt.Day})";
	}

	private static bool SameWeek(DateTime d1, DateTime d2)
	{
		Calendar calendar = DateTimeFormatInfo.CurrentInfo.Calendar;
		d1 = d1.AddDays(0 - calendar.GetDayOfWeek(d1));
		d2 = d2.AddDays(0 - calendar.GetDayOfWeek(d2));
		return d1 == d2;
	}
}
