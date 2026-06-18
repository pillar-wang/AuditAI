using System;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace Leqisoft.Model;

public class TicketExportExcelRewriter : FormulaBaseListener
{
	private readonly TokenStreamRewriter _rewriter;

	private readonly int _dataRowStart;

	private readonly int _dataRowCount;

	private readonly int _titleRowsCount;

	private readonly Func<int, int> _convertDesignRowIndexToVMRowIndexHandle;

	public TicketExportExcelRewriter(TokenStreamRewriter rewriter, int titleRowsCount, int dataRowStart, int dataRowCount, Func<int, int> convertDesignRowIndexToVMRowIndexHandle)
	{
		_rewriter = rewriter;
		_dataRowStart = dataRowStart;
		_dataRowCount = dataRowCount;
		_titleRowsCount = titleRowsCount;
		_convertDesignRowIndexToVMRowIndexHandle = convertDesignRowIndexToVMRowIndexHandle;
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
		arg = _convertDesignRowIndexToVMRowIndexHandle(arg);
		_rewriter.Replace(context.Start, context.Stop, $"{excelColumnName}{_titleRowsCount + arg + 1}");
	}

	public override void EnterRefTicketColumn([NotNull] FormulaParser.RefTicketColumnContext context)
	{
		string excelColumnName = Column.GetExcelColumnName(int.Parse(context.Int().GetText()));
		_rewriter.Replace(context.Start, context.Stop, $"{excelColumnName}{_dataRowStart + 1}:{excelColumnName}{_dataRowStart + _dataRowCount}");
	}

	public override void EnterRefTicketRange([NotNull] FormulaParser.RefTicketRangeContext context)
	{
		int arg = int.Parse(context.Int(0).GetText());
		string excelColumnName = Column.GetExcelColumnName(int.Parse(context.Int(1).GetText()));
		int arg2 = int.Parse(context.Int(2).GetText());
		string excelColumnName2 = Column.GetExcelColumnName(int.Parse(context.Int(3).GetText()));
		arg = _convertDesignRowIndexToVMRowIndexHandle(arg);
		arg2 = _convertDesignRowIndexToVMRowIndexHandle(arg2);
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
