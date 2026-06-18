using System;

namespace Leqisoft.Model;

public class TreeNodeNull : TreeNodeBase
{
	public TreeNodeNull()
	{
		_formulaUniqueName = Guid.NewGuid().ToString("D");
	}

	protected internal override int GetCode()
	{
		return int.MaxValue;
	}

	public override void Remove()
	{
	}
}
