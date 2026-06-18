using System.Collections.Generic;
using System.Linq;

namespace Leqisoft.Model;

public class FormulaHostCell : FormulaHost
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
		return refInfos.Any((FormulaRefInfo r) => r.Kind switch
		{
			FormulaHostKind.Cell => r.Id1 == base.HostInfo.Id1, 
			FormulaHostKind.Column => r.Id1 == Host.Column.Id, 
			FormulaHostKind.ColumnWildcard => r.Id1 == Host.Column.Id, 
			FormulaHostKind.Range => Project.Current.FormulaManager.Resolver.ResolveTableRange(r.TableId, r.Id1, r.Id2).Cells.Contains(Host), 
			_ => false, 
		});
	}

	public override void Eval()
	{
		try
		{
			Host.TryApplyFormula();
		}
		catch (FormulaBadReferenceException)
		{
		}
	}
}
