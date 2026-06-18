using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Leqisoft.DTO;

namespace Leqisoft.Model;

public class LedgerCollectFormulaDisplayRewriter : DisplayRewriter
{
	private readonly ILegderVirtualTableSetting _legderVirtualTableSetting;

	public LedgerCollectFormulaDisplayRewriter(TokenStreamRewriter rewriter, FormulaReferenceResolver resolver, Table contextTable, ILegderVirtualTableSetting legderVirtualTableSetting)
		: base(rewriter, resolver, contextTable)
	{
		_legderVirtualTableSetting = legderVirtualTableSetting;
	}

	public override void ExitRefColumn([NotNull] FormulaParser.RefColumnContext context)
	{
		Id64 id = Id64.Parse(context.Int(0).GetText());
		if (id.Value == _legderVirtualTableSetting.GetBalanceVirtualTableId())
		{
			long value = Id64.Parse(context.Int(1).GetText()).Value;
			string balanceVirtualTableColumnName = _legderVirtualTableSetting.GetBalanceVirtualTableColumnName(value);
			string text = ((balanceVirtualTableColumnName != null) ? ("{" + _legderVirtualTableSetting.GetBalanceVirtualTableName() + "}[" + balanceVirtualTableColumnName + "]") : "[无效列引用]");
			_rewriter.Replace(context.Start, context.Stop, text);
		}
		else if (id.Value == _legderVirtualTableSetting.GetVoucherVirtualTableId())
		{
			long value2 = Id64.Parse(context.Int(1).GetText()).Value;
			string voucherVirtualTableColumnName = _legderVirtualTableSetting.GetVoucherVirtualTableColumnName(value2);
			string text2 = ((voucherVirtualTableColumnName != null) ? ("{" + _legderVirtualTableSetting.GetVoucherVirtualTableName() + "}[" + voucherVirtualTableColumnName + "]") : "[无效列引用]");
			_rewriter.Replace(context.Start, context.Stop, text2);
		}
		else
		{
			base.ExitRefColumn(context);
		}
	}
}
