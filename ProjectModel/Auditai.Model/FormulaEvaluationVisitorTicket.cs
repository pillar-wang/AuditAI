using Antlr4.Runtime.Misc;
using Auditai.DTO;

namespace Auditai.Model;

public class FormulaEvaluationVisitorTicket : FormulaEvaluationVisitor
{
	private readonly TicketEvalContext _context;

	public FormulaEvaluationVisitorTicket(FormulaEvaluationEnvironment env, TicketEvalContext context)
		: base(env)
	{
		_context = context;
	}

	public override Operand VisitRefColumnWildcard([NotNull] FormulaParser.RefColumnWildcardContext context)
	{
		Id64 id = Id64.Parse(context.Int(0).GetText());
		Id64 arg = Id64.Parse(context.Int(1).GetText());
		Cell cell = _context.ResolveColumnWildcard(arg, _env.RowIndex, _env.TicketDataRowIndex);
		if (cell == null)
		{
			return base.VisitRefColumnWildcard(context);
		}
		return new CellOperand(cell);
	}

	public override Operand VisitRefColumn([NotNull] FormulaParser.RefColumnContext context)
	{
		Id64 id = Id64.Parse(context.Int(0).GetText());
		Id64 arg = Id64.Parse(context.Int(1).GetText());
		CellsOperand cellsOperand = _context.ResolveColumn(arg);
		return cellsOperand ?? base.VisitRefColumn(context);
	}

	public override Operand VisitRefTicketCell([NotNull] FormulaParser.RefTicketCellContext context)
	{
		int arg = int.Parse(context.Int(0).GetText());
		int arg2 = int.Parse(context.Int(1).GetText());
		Cell cell = _context.ResolveTicketCell(arg, arg2);
		return new CellOperand(cell);
	}

	public override Operand VisitRefTicketColumn([NotNull] FormulaParser.RefTicketColumnContext context)
	{
		int arg = int.Parse(context.Int().GetText());
		return _context.ResolveTicketColumn(arg);
	}

	public override Operand VisitRefTicketRange([NotNull] FormulaParser.RefTicketRangeContext context)
	{
		int arg = int.Parse(context.Int(0).GetText());
		int arg2 = int.Parse(context.Int(1).GetText());
		int arg3 = int.Parse(context.Int(2).GetText());
		int arg4 = int.Parse(context.Int(3).GetText());
		return _context.ResolveTicketRange(arg, arg2, arg3, arg4);
	}
}
