using Antlr4.Runtime.Misc;
using Leqisoft.DTO;

namespace Leqisoft.Model;

public class FormulaEvaluationVisitorLedgerVirtualTable : FormulaEvaluationVisitor
{
	private readonly LedgerVirtualTableEvalContext _context;

	private readonly Id64 _balanceVirtualTableId;

	private readonly Id64 _voucherVirtualTableId;

	public FormulaEvaluationVisitorLedgerVirtualTable(FormulaEvaluationEnvironment env, LedgerVirtualTableEvalContext context, Id64 balanceVirtualTableId, Id64 voucherVirtualTableId)
		: base(env)
	{
		_context = context;
		_balanceVirtualTableId = balanceVirtualTableId;
		_voucherVirtualTableId = voucherVirtualTableId;
	}

	public override Operand VisitRefColumnWildcard([NotNull] FormulaParser.RefColumnWildcardContext context)
	{
		Id64 id = Id64.Parse(context.Int(0).GetText());
		if (id == _balanceVirtualTableId || id == _voucherVirtualTableId)
		{
			return ErrorOperand.BadReference;
		}
		return base.VisitRefColumnWildcard(context);
	}

	public override Operand VisitRefColumn([NotNull] FormulaParser.RefColumnContext context)
	{
		Id64 id = Id64.Parse(context.Int(0).GetText());
		if (id == _balanceVirtualTableId)
		{
			Id64 arg = Id64.Parse(context.Int(1).GetText());
			CellsOperand cellsOperand = _context.BalanceTable_ResolveColumn(arg);
			if (cellsOperand == null)
			{
				return ErrorOperand.BadReference;
			}
			return cellsOperand;
		}
		if (id == _voucherVirtualTableId)
		{
			Id64 arg2 = Id64.Parse(context.Int(1).GetText());
			CellsOperand cellsOperand2 = _context.VoucherTable_ResolveColumn(arg2);
			if (cellsOperand2 == null)
			{
				return ErrorOperand.BadReference;
			}
			return cellsOperand2;
		}
		return base.VisitRefColumn(context);
	}

	public override Operand VisitRefRange([NotNull] FormulaParser.RefRangeContext context)
	{
		Id64 id = Id64.Parse(context.Int(0).GetText());
		if (id == _balanceVirtualTableId || id == _voucherVirtualTableId)
		{
			return ErrorOperand.BadReference;
		}
		return base.VisitRefRange(context);
	}

	public override Operand VisitRefCell([NotNull] FormulaParser.RefCellContext context)
	{
		Id64 id = Id64.Parse(context.Int(0).GetText());
		if (id == _balanceVirtualTableId || id == _voucherVirtualTableId)
		{
			return ErrorOperand.BadReference;
		}
		return base.VisitRefCell(context);
	}

	public override Operand VisitRefHeaderCell([NotNull] FormulaParser.RefHeaderCellContext context)
	{
		Id64 id = Id64.Parse(context.Int(0).GetText());
		if (id == _balanceVirtualTableId || id == _voucherVirtualTableId)
		{
			return ErrorOperand.BadReference;
		}
		return base.VisitRefHeaderCell(context);
	}

	public override Operand VisitRefHeaderCellWildcard([NotNull] FormulaParser.RefHeaderCellWildcardContext context)
	{
		Id64 id = Id64.Parse(context.Int(0).GetText());
		if (id == _balanceVirtualTableId || id == _voucherVirtualTableId)
		{
			return ErrorOperand.BadReference;
		}
		return base.VisitRefHeaderCellWildcard(context);
	}

	public override Operand VisitRefTreeNode([NotNull] FormulaParser.RefTreeNodeContext context)
	{
		Id64 id = Id64.Parse(context.Int().GetText());
		if (id == _balanceVirtualTableId || id == _voucherVirtualTableId)
		{
			return ErrorOperand.BadReference;
		}
		return base.VisitRefTreeNode(context);
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
