using System;
using C1.Win.C1FlexGrid;

namespace Auditai.UI.LedgerView;

public class ValidateException : Exception
{
	public ValidateErrorTypeEnum FailureReason { get; set; }

	public C1FlexGrid Grid { get; set; }

	public Row Row { get; set; }

	public Column Col { get; set; }
}
