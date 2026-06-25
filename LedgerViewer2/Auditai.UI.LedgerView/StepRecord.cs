using System.Collections.Generic;

namespace Auditai.UI.LedgerView;

public class StepRecord
{
	private Stack<StepItem> stackLeft { get; set; }

	private Stack<StepItem> stackRight { get; set; }

	public StepStatus StepStatus { get; private set; }

	public StepRecord()
	{
		stackLeft = new Stack<StepItem>();
		stackRight = new Stack<StepItem>();
		StepStatus = StepStatus.Normal;
	}

	public void Add(StepItem item)
	{
		if (StepStatus == StepStatus.Normal)
		{
			stackLeft.Push(item);
			stackRight.Clear();
		}
		StepStatus = StepStatus.Normal;
	}

	public void Back()
	{
		if (stackLeft.Count > 1)
		{
			stackRight.Push(stackLeft.Pop());
			if (stackLeft.Count > 0)
			{
				StepStatus = StepStatus.Back;
				stackLeft.Peek().Apply();
			}
		}
	}

	public void Forward()
	{
		if (stackRight.Count > 0)
		{
			StepStatus = StepStatus.Forward;
			stackLeft.Push(stackRight.Pop());
			stackLeft.Peek().Apply();
		}
	}

	public void Clear()
	{
		stackLeft.Clear();
		stackRight.Clear();
	}
}
