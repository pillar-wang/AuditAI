using System.Collections.Generic;

namespace Leqisoft.Model;

public class HasInputListListener : FunctionNameExistsListener
{
	private static IEnumerable<string> _funcNames = new string[1] { "InputList" };

	public HasInputListListener()
		: base(_funcNames)
	{
	}
}
