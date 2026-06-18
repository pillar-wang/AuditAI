using System;

namespace Leqisoft.Model;

public class LedgerInfo
{
	private int _progress;

	public int Year { get; set; }

	public string CompanyName { get; set; }

	public string LedgerNumber { get; set; }

	public DatabaseInfo DbInfo { get; set; }

	public event EventHandler<GetLedgerProgressEventArgs> ProgressChanged;

	public void ReportProgress(string message)
	{
		_progress++;
		this.ProgressChanged?.Invoke(this, new GetLedgerProgressEventArgs(_progress, message));
	}

	public override string ToString()
	{
		return $"{Year}\t{CompanyName}\t{LedgerNumber}";
	}
}
