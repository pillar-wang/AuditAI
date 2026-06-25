namespace Auditai.Model;

public sealed class BlankFilter : ByIndividualValueFilter
{
	public sealed override bool Execute(FilterValue value)
	{
		return string.IsNullOrWhiteSpace(value.Display);
	}

	public override string ToFormula(string colText)
	{
		return colText + "=\"\"";
	}
}
