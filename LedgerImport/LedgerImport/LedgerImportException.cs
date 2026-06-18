using System;

namespace LedgerImport;

public class LedgerImportException : Exception
{
	public LedgerImportException()
	{
	}

	public LedgerImportException(string message)
		: base(message)
	{
	}

	public LedgerImportException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
