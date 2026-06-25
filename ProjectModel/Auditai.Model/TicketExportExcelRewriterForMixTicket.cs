using System;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace Auditai.Model;

public class TicketExportExcelRewriterForMixTicket : FormulaBaseListener
{
	private readonly TokenStreamRewriter _rewriter;

	private readonly int _titleRowsCount;

	private readonly Func<int, Tuple<int, int, int>> _getTicketColSettingHandle;

	private readonly Func<int, int> _convertFixedRowIndexToVMRowIndexHandle;

	public TicketExportExcelRewriterForMixTicket(TokenStreamRewriter rewriter, int titleRowsCount, Func<int, Tuple<int, int, int>> getTicketColumnSettingHandle, Func<int, int> convertFixedRowIndexToVMRowIndexHandle)
	{
		_rewriter = rewriter;
		_titleRowsCount = titleRowsCount;
		_getTicketColSettingHandle = getTicketColumnSettingHandle;
		_convertFixedRowIndexToVMRowIndexHandle = convertFixedRowIndexToVMRowIndexHandle;
	}

	public override void EnterFunc([NotNull] FormulaParser.FuncContext context)
	{
		if (!"Sum".Equals(context.FuncName().GetText(), StringComparison.OrdinalIgnoreCase))
		{
			throw new FormulaNotApplicableException("");
		}
	}

	public override void EnterRefTicketCell([NotNull] FormulaParser.RefTicketCellContext context)
	{
		int arg = int.Parse(context.Int(0).GetText());
		string excelColumnName = Column.GetExcelColumnName(int.Parse(context.Int(1).GetText()));
		arg = _convertFixedRowIndexToVMRowIndexHandle(arg);
		_rewriter.Replace(context.Start, context.Stop, $"{excelColumnName}{_titleRowsCount + arg + 1}");
	}

	public override void EnterRefTicketColumn([NotNull] FormulaParser.RefTicketColumnContext context)
	{
		Tuple<int, int, int> tuple = _getTicketColSettingHandle(int.Parse(context.Int().GetText()));
		string excelColumnName = Column.GetExcelColumnName(tuple.Item1);
		_rewriter.Replace(context.Start, context.Stop, $"{excelColumnName}{tuple.Item2 + 1}:{excelColumnName}{tuple.Item3 + 1}");
	}

	public override void EnterRefTicketRange([NotNull] FormulaParser.RefTicketRangeContext context)
	{
		int arg = int.Parse(context.Int(0).GetText());
		string excelColumnName = Column.GetExcelColumnName(int.Parse(context.Int(1).GetText()));
		int arg2 = int.Parse(context.Int(2).GetText());
		string excelColumnName2 = Column.GetExcelColumnName(int.Parse(context.Int(3).GetText()));
		arg = _convertFixedRowIndexToVMRowIndexHandle(arg);
		arg2 = _convertFixedRowIndexToVMRowIndexHandle(arg2);
		_rewriter.Replace(context.Start, context.Stop, $"{excelColumnName}{_titleRowsCount + arg + 1}:{excelColumnName2}{_titleRowsCount + arg2 + 1}");
	}

	public override void EnterRefCell([NotNull] FormulaParser.RefCellContext context)
	{
		throw new FormulaNotApplicableException("");
	}

	public override void EnterRefColumn([NotNull] FormulaParser.RefColumnContext context)
	{
		throw new FormulaNotApplicableException("");
	}

	public override void EnterRefColumnWildcard([NotNull] FormulaParser.RefColumnWildcardContext context)
	{
		throw new FormulaNotApplicableException("");
	}

	public override void EnterRefHeaderCell([NotNull] FormulaParser.RefHeaderCellContext context)
	{
		throw new FormulaNotApplicableException("");
	}

	public override void EnterRefHeaderCellWildcard([NotNull] FormulaParser.RefHeaderCellWildcardContext context)
	{
		throw new FormulaNotApplicableException("");
	}

	public override void EnterRefRange([NotNull] FormulaParser.RefRangeContext context)
	{
		throw new FormulaNotApplicableException("");
	}

	public override void EnterRefTreeNode([NotNull] FormulaParser.RefTreeNodeContext context)
	{
		throw new FormulaNotApplicableException("");
	}
}
