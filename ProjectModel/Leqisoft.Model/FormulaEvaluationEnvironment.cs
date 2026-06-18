namespace Leqisoft.Model;

public class FormulaEvaluationEnvironment
{
	public FormulaReferenceResolver Resolver { get; set; }

	public DataReferenceEvaluationContext RefEvalContext { get; set; }

	public DataReferenceManager RefManager { get; set; }

	public int RowIndex { get; set; }

	public int TicketDataRowIndex { get; set; } = -1;


	public Table HostTable { get; set; }

	public FormulaContextKind ContextKind { get; set; }

	public ControlFormulaContext ControlFormulaContext { get; set; }

	public FormulaReferenceTicketInputDataResolver TicketInputDataResolver { get; set; }

	public FormulaReferenceTableTitleCellResolver TableTitleCellResolver { get; set; }

	public bool IsInTicketEnvironment { get; set; }

	public bool IsFormulaComeFromTable { get; set; }

	public Row TicketCellRefTableRow { get; set; }

	public long CurrentUserId { get; set; }

	public bool IsIgnoreColSheetFunBadRefrence { get; set; } = true;


	public bool IsAllowExecuteCancelFunction { get; set; }
}
