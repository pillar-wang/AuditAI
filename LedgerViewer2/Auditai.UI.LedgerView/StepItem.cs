using System;

namespace Auditai.UI.LedgerView;

public class StepItem
{
	public StepArgs StepArgs;

	public Action<StepArgs> BackAction;

	public void Apply()
	{
		BackAction?.Invoke(StepArgs);
	}

	public StepItem(StepArgs args, Action<StepArgs> action)
	{
		StepArgs = args;
		BackAction = action;
	}
}
