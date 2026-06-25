using Antlr4.Runtime.Misc;
using Auditai.DTO;

namespace Auditai.Model;

public class FormulaEvaluationVisitorVirtualTable : FormulaEvaluationVisitor
{
	private readonly VirtualTableEvalContext _context;

	private readonly Id64 _virtualTableId;

	public FormulaEvaluationVisitorVirtualTable(FormulaEvaluationEnvironment env, VirtualTableEvalContext context, Id64 virtualTableId)
		: base(env)
	{
		_context = context;
		_virtualTableId = virtualTableId;
	}

	public override Operand VisitRefColumnWildcard([NotNull] FormulaParser.RefColumnWildcardContext context)
	{
		Id64 id = Id64.Parse(context.Int(0).GetText());
		if (id == _virtualTableId)
		{
			Id64 arg = Id64.Parse(context.Int(1).GetText());
			Cell cell = _context.ResolveColumnWildcard(arg, _env.RowIndex);
			if (cell == null)
			{
				return ErrorOperand.BadReference;
			}
			return new CellOperand(cell);
		}
		return base.VisitRefColumnWildcard(context);
	}

	public override Operand VisitRefColumn([NotNull] FormulaParser.RefColumnContext context)
	{
		Id64 id = Id64.Parse(context.Int(0).GetText());
		if (id == _virtualTableId)
		{
			Id64 arg = Id64.Parse(context.Int(1).GetText());
			CellsOperand cellsOperand = _context.ResolveColumn(arg);
			if (cellsOperand == null)
			{
				return ErrorOperand.BadReference;
			}
			return cellsOperand;
		}
		return base.VisitRefColumn(context);
	}

	public override Operand VisitRefRange([NotNull] FormulaParser.RefRangeContext context)
	{
		Id64 id = Id64.Parse(context.Int(0).GetText());
		if (id == _virtualTableId)
		{
			Id64 arg = Id64.Parse(context.Int(1).GetText());
			Id64 arg2 = Id64.Parse(context.Int(2).GetText());
			Cell cell = _context.ResolveTableCell(arg);
			Cell cell2 = _context.ResolveTableCell(arg2);
			if (cell == null || cell2 == null)
			{
				return ErrorOperand.BadReference;
			}
			return new RangeOperand(cell, cell2);
		}
		return base.VisitRefRange(context);
	}

	public override Operand VisitRefCell([NotNull] FormulaParser.RefCellContext context)
	{
		Id64 id = Id64.Parse(context.Int(0).GetText());
		if (id == _virtualTableId)
		{
			Id64 arg = Id64.Parse(context.Int(1).GetText());
			Cell cell = _context.ResolveTableCell(arg);
			if (cell == null)
			{
				return ErrorOperand.BadReference;
			}
			return new CellOperand(cell);
		}
		return base.VisitRefCell(context);
	}

	public override Operand VisitRefHeaderCell([NotNull] FormulaParser.RefHeaderCellContext context)
	{
		return ErrorOperand.BadReference;
	}

	public override Operand VisitRefHeaderCellWildcard([NotNull] FormulaParser.RefHeaderCellWildcardContext context)
	{
		return ErrorOperand.BadReference;
	}

	public override Operand VisitRefTicketCell([NotNull] FormulaParser.RefTicketCellContext context)
	{
		return base.VisitRefTicketCell(context);
	}

	public override Operand VisitRefTicketColumn([NotNull] FormulaParser.RefTicketColumnContext context)
	{
		return base.VisitRefTicketColumn(context);
	}

	public override Operand VisitRefTicketRange([NotNull] FormulaParser.RefTicketRangeContext context)
	{
		return base.VisitRefTicketRange(context);
	}
}
