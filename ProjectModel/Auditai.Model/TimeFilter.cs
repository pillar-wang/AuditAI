using System;
using Newtonsoft.Json;

namespace Auditai.Model;

public sealed class TimeFilter : ByIndividualValueFilter
{
	[JsonProperty]
	public TimeSpan Value { get; set; }

	[JsonProperty]
	public TimeSpan Value2 { get; set; }

	public override bool Execute(FilterValue value)
	{
		if (value.DataType == FilterDataType.Time)
		{
			return Predicate(value.Time);
		}
		return false;
	}

	public override string ToFormula(string colText)
	{
		return base.Kind switch
		{
			FilterKind.TimeEq => colText + "=" + TimeToFormula(Value), 
			FilterKind.TimeBefore => colText + "<" + TimeToFormula(Value), 
			FilterKind.TimeAfter => colText + ">" + TimeToFormula(Value), 
			FilterKind.TimeNe => colText + "<>" + TimeToFormula(Value), 
			FilterKind.TimeBeforeEq => colText + "<=" + TimeToFormula(Value), 
			FilterKind.TimeAfterEq => colText + ">=" + TimeToFormula(Value), 
			FilterKind.TimeBetween => colText + ">=" + TimeToFormula(Value) + " And " + colText + "<=" + TimeToFormula(Value2), 
			FilterKind.TimeOutside => colText + "<" + TimeToFormula(Value) + " Or " + colText + ">" + TimeToFormula(Value2), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	private bool Predicate(TimeSpan fv)
	{
		switch (base.Kind)
		{
		case FilterKind.TimeEq:
			return fv == Value;
		case FilterKind.TimeBefore:
			return fv < Value;
		case FilterKind.TimeAfter:
			return fv > Value;
		case FilterKind.TimeBeforeEq:
			return fv <= Value;
		case FilterKind.TimeAfterEq:
			return fv >= Value;
		case FilterKind.TimeNe:
			return fv != Value;
		case FilterKind.TimeBetween:
			if (fv >= Value)
			{
				return fv <= Value2;
			}
			return false;
		case FilterKind.TimeOutside:
			if (!(fv < Value))
			{
				return fv > Value2;
			}
			return true;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private static string TimeToFormula(TimeSpan t)
	{
		return $"Time({t.Hours},{t.Minutes},{t.Seconds})";
	}
}
