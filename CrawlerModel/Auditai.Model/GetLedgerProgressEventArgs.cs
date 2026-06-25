using System;

namespace Auditai.Model;

public class GetLedgerProgressEventArgs : EventArgs
{
	public int Progress { get; set; }

	public string Message { get; set; }

	public GetLedgerProgressEventArgs(int progress, string message)
	{
		Progress = progress;
		Message = message;
	}
}
