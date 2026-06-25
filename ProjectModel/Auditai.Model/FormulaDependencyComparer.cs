using System.Collections.Generic;

namespace Auditai.Model;

public class FormulaDependencyComparer : IEqualityComparer<FormulaDependency>
{
	public static FormulaDependencyComparer Instance { get; } = new FormulaDependencyComparer();


	private FormulaDependencyComparer()
	{
	}

	public bool Equals(FormulaDependency x, FormulaDependency y)
	{
		if (x.HostTable == y.HostTable)
		{
			return x.ReferredTable == y.ReferredTable;
		}
		return false;
	}

	public int GetHashCode(FormulaDependency obj)
	{
		return obj.HostTable.GetHashCode() ^ obj.ReferredTable.GetHashCode();
	}
}
