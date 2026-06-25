namespace Auditai.Model;

public class ValidationResult
{
	public ValidationFormula Source { get; set; }

	public bool IsValid { get; set; }

	public FormulaReferences Refs { get; set; }

	public object LeftValue { get; set; }

	public object RightValue { get; set; }

	public bool Passed { get; set; }

	public int RowIndex { get; set; }

	public bool HasWildcard { get; set; }

	public static string ValueToString(object value)
	{
		if (!(value is string text))
		{
			if (value is double num)
			{
				return num.ToString("0.####");
			}
			return value.ToString();
		}
		return "\"" + text + "\"";
	}
}
