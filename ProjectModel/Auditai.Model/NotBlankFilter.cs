namespace Auditai.Model;

public sealed class NotBlankFilter : ByIndividualValueFilter
{
	public sealed override bool Execute(FilterValue value)
	{
		return !string.IsNullOrWhiteSpace(value.Display);
	}

	public override string ToFormula(string colText)
	{
		return colText + "<>\"\"";
	}
}
