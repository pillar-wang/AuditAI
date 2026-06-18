using System.Collections.Generic;

namespace Leqisoft.Model;

public class HasLqCrossTableListener : FunctionNameExistsListener
{
	private static IEnumerable<string> _funcNames = new string[2] { "LqCrossTable", "CrossTable" };

	public HasLqCrossTableListener()
		: base(_funcNames)
	{
	}
}
