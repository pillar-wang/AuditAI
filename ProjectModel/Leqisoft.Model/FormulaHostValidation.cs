using System.Collections.Generic;

namespace Leqisoft.Model;

public class FormulaHostValidation : FormulaHost
{
	protected override bool ReferredBy(IEnumerable<FormulaRefInfo> refInfos)
	{
		return false;
	}

	public override void Eval()
	{
	}
}
