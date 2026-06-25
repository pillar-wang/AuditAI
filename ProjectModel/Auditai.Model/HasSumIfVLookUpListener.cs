using System.Collections.Generic;

namespace Auditai.Model;

public class HasSumIfVLookUpListener : FunctionNameExistsListener
{
	private static IEnumerable<string> _funcNames = new string[4] { "LqSumIf", "LqVLookUp", "SumIf", "VLookUp" };

	public HasSumIfVLookUpListener()
		: base(_funcNames)
	{
	}
}
