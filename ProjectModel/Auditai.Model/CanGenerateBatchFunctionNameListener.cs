using System.Collections.Generic;

namespace Auditai.Model;

public class CanGenerateBatchFunctionNameListener : FunctionNameExistsListener
{
	private static IEnumerable<string> _funcNames = new string[12]
	{
		"LqDistinct", "LqFilter", "LqAsc", "LqDesc", "LqSumIf", "LqVLookUp", "Distinct", "DistinctF", "DistinctUp", "DistinctDown",
		"SumIf", "VLookUp"
	};

	public CanGenerateBatchFunctionNameListener()
		: base(_funcNames)
	{
	}
}
