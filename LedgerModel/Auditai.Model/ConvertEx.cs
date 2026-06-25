namespace Auditai.Model;

public static class ConvertEx
{
	public static decimal ToDecimalSafe(this double doubleValue)
	{
		if (doubleValue < -7.922816251426434E+28)
		{
			return decimal.MinValue;
		}
		if (doubleValue > 7.922816251426434E+28)
		{
			return decimal.MaxValue;
		}
		return (decimal)doubleValue;
	}
}
