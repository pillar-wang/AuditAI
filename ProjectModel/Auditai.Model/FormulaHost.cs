using System.Collections.Generic;
using System.Linq;
using Auditai.DTO;

namespace Auditai.Model;

public abstract class FormulaHost
{
	public FormulaRefInfo HostInfo { get; set; }

	public List<FormulaRefInfo> RefInfos { get; set; }

	public bool ReferredBy(FormulaHost h)
	{
		IEnumerable<FormulaRefInfo> enumerable = h.RefInfos.Where((FormulaRefInfo ri) => ri.TableId == HostInfo.TableId);
		if (enumerable.Any())
		{
			return ReferredBy(enumerable);
		}
		return false;
	}

	protected abstract bool ReferredBy(IEnumerable<FormulaRefInfo> refInfos);

	public bool DependsOnRow(Row row)
	{
		HashSet<Id64> cellIds = new HashSet<Id64>(from c in row.GetCells()
			select c.Id);
		return RefInfos.Any((FormulaRefInfo r) => r.Kind == FormulaHostKind.Cell && cellIds.Contains(r.Id1));
	}

	public bool DependsOnColumn(Column column)
	{
		return RefInfos.Any((FormulaRefInfo r) => r.Id1 == column.Id);
	}

	public bool DependsOnTable(Table table)
	{
		return RefInfos.Any((FormulaRefInfo i) => i.TableId == table.Id);
	}

	public abstract void Eval();
}
