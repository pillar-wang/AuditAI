using System.Collections.Generic;
using System.Linq;

namespace Leqisoft.Model;

public class FormulaHostHeaderCell : FormulaHost
{
	private Cell _host;

	public Cell Host
	{
		get
		{
			if (_host == null)
			{
				_host = Project.Current.FormulaManager.Resolver.ResolveTableCell(base.HostInfo.TableId, base.HostInfo.Id1);
			}
			return _host;
		}
		set
		{
			_host = value;
		}
	}

	protected override bool ReferredBy(IEnumerable<FormulaRefInfo> refInfos)
	{
		int headerLastRow = Host.GetHeaderLastRow();
		return refInfos.Any(delegate(FormulaRefInfo r)
		{
			FormulaHostKind kind = r.Kind;
			Cell headerCell;
			return kind == FormulaHostKind.Cell && Project.Current.FormulaManager.Resolver.ResolveTableCell(r.TableId, r.Id1).TryGetHeaderCellFormulaCell(out headerCell) && headerCell == Host;
		});
	}

	public override void Eval()
	{
		try
		{
			Host.TryApplyHeaderFormula();
		}
		catch (FormulaBadReferenceException)
		{
		}
	}
}
