using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Leqisoft.DTO;

namespace Leqisoft.Model;

public class GenerateBatchFormulaRewriter : FormulaBaseListener
{
	private TokenStreamRewriter _rewriter;

	private readonly FormulaReferenceResolver _resolver;

	private Table _currentTable;

	private Table _newTable;

	public GenerateBatchFormulaRewriter(TokenStreamRewriter rewriter, FormulaReferenceResolver resolver, Table currentTable, Table newTable)
	{
		_rewriter = rewriter;
		_resolver = resolver;
		_currentTable = currentTable;
		_newTable = newTable;
	}

	public override void EnterRefColumn([NotNull] FormulaParser.RefColumnContext context)
	{
		Id64 id = Id64.Parse(context.Int(0).GetText());
		if (_currentTable.Id != id)
		{
			Id64 arg = Id64.Parse(context.Int(1).GetText());
			Column column = _resolver.ResolveTableColumn(id, arg);
			string uniqueFormulaName = column.GetUniqueFormulaName();
			Column byCaption = _newTable.Columns.GetByCaption(uniqueFormulaName);
			if (byCaption != null)
			{
				_rewriter.Replace(context.Int(0).Symbol, _newTable.Id.ToString());
				_rewriter.Replace(context.Int(1).Symbol, byCaption.Id.ToString());
			}
		}
	}
}
