using System;
using Newtonsoft.Json;

namespace Leqisoft.Model;

public sealed class NumberFilter : ByIndividualValueFilter
{
	[JsonProperty]
	public double Value { get; set; }

	[JsonProperty]
	public double Value2 { get; set; }

	public sealed override bool Execute(FilterValue value)
	{
		if (value.DataType == FilterDataType.Number)
		{
			return Predicate(value.Number);
		}
		return false;
	}

	public override string ToFormula(string colText)
	{
		return base.Kind switch
		{
			FilterKind.Eq => $"{colText}={Value}", 
			FilterKind.Gt => $"{colText}>{Value}", 
			FilterKind.Lt => $"{colText}<{Value}", 
			FilterKind.Ne => $"{colText}<>{Value}", 
			FilterKind.Gte => $"{colText}>={Value}", 
			FilterKind.Lte => $"{colText}<={Value}", 
			FilterKind.Between => $"{colText}>={Value} And {colText}<={Value2}", 
			FilterKind.Outside => $"{colText}<{Value} Or {colText}>{Value2}", 
			_ => throw new NotSupportedException(), 
		};
	}

	private bool Predicate(double fv)
	{
		switch (base.Kind)
		{
		case FilterKind.Eq:
			return fv == Value;
		case FilterKind.Gt:
			return fv > Value;
		case FilterKind.Lt:
			return fv < Value;
		case FilterKind.Ne:
			return fv != Value;
		case FilterKind.Gte:
			return fv >= Value;
		case FilterKind.Lte:
			return fv <= Value;
		case FilterKind.Between:
			if (fv >= Value)
			{
				return fv <= Value2;
			}
			return false;
		case FilterKind.Outside:
			if (!(fv < Value))
			{
				return fv > Value2;
			}
			return true;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}
}
