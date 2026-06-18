using System;
using System.ComponentModel;

namespace CrawlerForm;

public class WorkerContext
{
	private DoWorkEventArgs doWorkEventArgs;

	public BackgroundWorker Worker { get; private set; }

	public WorkerContext(BackgroundWorker worker, DoWorkEventArgs e)
	{
		Worker = worker;
		doWorkEventArgs = e;
	}

	public void ThrowOperationCanceledExceptionIfCanceled()
	{
		if (Worker.CancellationPending)
		{
			doWorkEventArgs.Cancel = true;
			throw new OperationCanceledException();
		}
	}

	public bool IsCanceled()
	{
		return Worker.CancellationPending;
	}
}
