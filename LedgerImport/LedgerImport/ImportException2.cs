using System;

namespace LedgerImport;

public class ImportException2 : Exception
{
	public FailureReasonEnum FailureReason { get; set; }

	public FailureContext FailureContext { get; set; }
}
