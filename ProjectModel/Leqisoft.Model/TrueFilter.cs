namespace Leqisoft.Model;

public sealed class TrueFilter : ByIndividualValueFilter
{
	public override bool Execute(FilterValue value)
	{
		if (value.DataType == FilterDataType.Bool)
		{
			return value.Bool;
		}
		return false;
	}

	public override string ToFormula(string colText)
	{
		return colText + "=True()";
	}
}
