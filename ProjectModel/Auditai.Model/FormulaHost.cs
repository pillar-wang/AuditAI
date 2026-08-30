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
		// 修复：RefInfos/HostInfo 无初始化默认值，实现类未赋值时原直接访问 NRE。
		if (h == null || h.RefInfos == null || HostInfo == null)
		{
			return false;
		}
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
		if (RefInfos == null)
		{
			return false;
		}
		HashSet<Id64> cellIds = new HashSet<Id64>(from c in row.GetCells()
			select c.Id);
		return RefInfos.Any((FormulaRefInfo r) => r.Kind == FormulaHostKind.Cell && cellIds.Contains(r.Id1));
	}

	public bool DependsOnColumn(Column column)
	{
		if (RefInfos == null || column == null)
		{
			return false;
		}
		return RefInfos.Any((FormulaRefInfo r) => r.Id1 == column.Id);
	}

	public bool DependsOnTable(Table table)
	{
		if (RefInfos == null || table == null)
		{
			return false;
		}
		return RefInfos.Any((FormulaRefInfo i) => i.TableId == table.Id);
	}

	public abstract void Eval();
}
