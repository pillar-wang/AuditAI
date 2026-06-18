using System.Collections.Generic;
using System.Linq;

namespace Leqisoft.Model;

public class FormulaHostTitleCell : FormulaHost
{
	public TableTitleCell Host { get; set; }

	protected override bool ReferredBy(IEnumerable<FormulaRefInfo> refInfos)
	{
		return refInfos.Any(delegate(FormulaRefInfo r)
		{
			FormulaHostKind kind = r.Kind;
			return kind == FormulaHostKind.TitleCell && r.Int1 == base.HostInfo.Int1 && r.Int2 == base.HostInfo.Int2;
		});
	}

	public override void Eval()
	{
		Host.EvaluateFormula();
	}
}
