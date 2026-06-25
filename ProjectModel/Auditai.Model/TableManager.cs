using System.Collections.Generic;
using Auditai.DTO;

namespace Auditai.Model;

public class TableManager
{
	private readonly Project _project;

	private readonly Dictionary<Id64, Table> _dic = new Dictionary<Id64, Table>();

	internal TableManager(Project project)
	{
		_project = project;
	}

	public IEnumerable<Table> Enumerate()
	{
		return _dic.Values;
	}

	public Table GetById(Id64 id)
	{
		if (!_dic.TryGetValue(id, out var value))
		{
			return null;
		}
		return value;
	}
}
