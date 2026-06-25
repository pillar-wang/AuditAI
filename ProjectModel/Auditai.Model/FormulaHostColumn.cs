using System.Collections.Generic;
using System.Linq;

namespace Auditai.Model;

public class FormulaHostColumn : FormulaHost
{
	private Column _host;

	public Column Host
	{
		get
		{
			if (_host == null)
			{
				_host = Project.Current.FormulaManager.Resolver.ResolveTableColumn(base.HostInfo.TableId, base.HostInfo.Id1);
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
			FormulaHostKind.Cell => Project.Current.FormulaManager.Resolver.ResolveTableCell(r.TableId, r.Id1).Column.Id == base.HostInfo.Id1, 
			FormulaHostKind.Column => r.Id1 == base.HostInfo.Id1, 
			FormulaHostKind.ColumnWildcard => r.Id1 == base.HostInfo.Id1, 
			FormulaHostKind.Range => Project.Current.FormulaManager.Resolver.ResolveTableRange(r.TableId, r.Id1, r.Id2).Cells.Any((Cell c) => c.Column.Id == base.HostInfo.Id1), 
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
