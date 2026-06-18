namespace Leqisoft.Model;

public sealed class NotTrueFilter : ByIndividualValueFilter
{
	public override bool Execute(FilterValue value)
	{
		if (value.DataType == FilterDataType.Bool)
		{
			return !value.Bool;
		}
		return true;
	}

	public override string ToFormula(string colText)
	{
		return colText + "<>True()";
	}
}
