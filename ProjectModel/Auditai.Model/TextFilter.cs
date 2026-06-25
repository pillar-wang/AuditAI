using System;
using Newtonsoft.Json;

namespace Auditai.Model;

public sealed class TextFilter : ByIndividualValueFilter
{
	[JsonProperty]
	public string Value { get; set; }

	public sealed override bool Execute(FilterValue value)
	{
		if (value.DataType == FilterDataType.Text)
		{
			return Predicate(value.Text);
		}
		return false;
	}

	public override string ToFormula(string colText)
	{
		return base.Kind switch
		{
			FilterKind.TextEq => colText + "=\"" + Value + "\"", 
			FilterKind.Contains => colText + "=\"*" + Value + "*\"", 
			FilterKind.StartsWith => colText + "=\"" + Value + "*\"", 
			FilterKind.EndsWith => colText + "=\"*" + Value + "\"", 
			FilterKind.TextNe => colText + "<>\"" + Value + "\"", 
			_ => throw new NotSupportedException(), 
		};
	}

	private bool Predicate(string fv)
	{
		return base.Kind switch
		{
			FilterKind.TextEq => fv.Equals(Value), 
			FilterKind.Contains => fv.Contains(Value), 
			FilterKind.StartsWith => fv.StartsWith(Value), 
			FilterKind.EndsWith => fv.EndsWith(Value), 
			FilterKind.TextNe => !fv.Equals(Value), 
			FilterKind.NotContains => !fv.Contains(Value), 
			FilterKind.NotStartsWith => !fv.StartsWith(Value), 
			FilterKind.NotEndsWith => !fv.EndsWith(Value), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
