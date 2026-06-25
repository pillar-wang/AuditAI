using System.Collections.Generic;

namespace Auditai.Model;

public class StringNumberComparer : IComparer<string>
{
	public static StringNumberComparer Instance { get; } = new StringNumberComparer();


	public int Compare(string s1, string s2)
	{
		double result;
		bool flag = double.TryParse(s1, out result);
		double result2;
		bool flag2 = double.TryParse(s2, out result2);
		if (flag)
		{
			if (flag2)
			{
				return result.CompareTo(result2);
			}
			return -1;
		}
		if (flag2)
		{
			return 1;
		}
		return s1.CompareTo(s2);
	}
}
