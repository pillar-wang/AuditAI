using Antlr4.Runtime.Misc;
using Leqisoft.DTO;

namespace Leqisoft.Model;

public class HasCurrentTableColumnListener : FormulaBaseListener
{
	private readonly Table _table;

	public bool Result { get; private set; }

	public HasCurrentTableColumnListener(Table table)
	{
		_table = table;
	}

	public override void EnterRefColumn([NotNull] FormulaParser.RefColumnContext context)
	{
		Id64 id = Id64.Parse(context.Int(0).GetText());
		Id64 id2 = Id64.Parse(context.Int(1).GetText());
		if (id == _table.Id)
		{
			Result = true;
		}
	}
}
