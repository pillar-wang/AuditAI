namespace Auditai.UI.Controls;

using System;
using Auditai.DTO;

/// <summary>
/// IProgress&lt;ProgressInfo&gt; → ProgressRuntimeData 适配器。
/// 用于旧 ProgressForm&lt;T&gt;（IProgress 上报）向 ProgressForm2（ProgressRuntimeData 上报）迁移：
/// 任务体内的 iProg.Report(new ProgressInfo { MainCaption = ..., MainProgress = ... }) 调用保持原样，
/// 由本适配器转发到 ProgressRuntimeData（MainProgress 0-100 → 0-1 归一化）。
/// </summary>
public sealed class ProgressRuntimeDataReporter : IProgress<ProgressInfo>
{
	private readonly ProgressRuntimeData _data;

	public ProgressRuntimeDataReporter(ProgressRuntimeData data)
	{
		_data = data ?? new ProgressRuntimeData();
	}

	public void Report(ProgressInfo value)
	{
		if (value == null)
		{
			return;
		}
		if (!string.IsNullOrEmpty(value.MainCaption))
		{
			_data.UpdateMessage(value.MainCaption);
		}
		_data.UpdateProgress(value.MainProgress / 100f);
	}
}

public class ProgressRuntimeData
{
	private object _lockObject = new object();

	protected int _step;

	protected string _postfixString;

	protected float _progressValue;

	protected bool _isTaskEnd;

	protected string _message;

	public void Finish()
	{
		lock (_lockObject)
		{
			_isTaskEnd = true;
			_progressValue = 1f;
		}
	}

	public void NextStep()
	{
		lock (_lockObject)
		{
			_step++;
			_progressValue = 0f;
		}
	}

	public void NextStepWithResetPostfix(string postfixString)
	{
		lock (_lockObject)
		{
			_step++;
			_progressValue = 0f;
			_postfixString = postfixString;
		}
	}

	public void NextStepIfProgressNotZero(string message)
	{
		lock (_lockObject)
		{
			if (_progressValue == 0f)
			{
				_message = message;
				_postfixString = null;
				return;
			}
			_step++;
			_progressValue = 0f;
			_message = message;
			_postfixString = null;
		}
	}

	public void NextStep(string message)
	{
		lock (_lockObject)
		{
			_step++;
			_progressValue = 0f;
			_message = message;
			_postfixString = null;
		}
	}

	public void NextStep(string message, string postfixString)
	{
		lock (_lockObject)
		{
			_step++;
			_progressValue = 0f;
			_message = message;
			_postfixString = postfixString;
		}
	}

	public void UpdateMessage(string message)
	{
		lock (_lockObject)
		{
			_message = message;
			_postfixString = null;
		}
	}

	public void UpdateMessage(string message, string postfix)
	{
		lock (_lockObject)
		{
			_message = message;
			_postfixString = postfix;
		}
	}

	public void UpdatePostfix(string postfix)
	{
		lock (_lockObject)
		{
			_postfixString = postfix;
		}
	}

	public void UpdateProgress(float progressValue)
	{
		if (progressValue < 0f)
		{
			progressValue = 0f;
		}
		else if (progressValue > 1f)
		{
			progressValue = 1f;
		}
		lock (_lockObject)
		{
			if (_progressValue < progressValue)
			{
				_progressValue = progressValue;
			}
		}
	}

	public void UpdateProgress(int hasProcessCount, int totalCount)
	{
		float num = 0f;
		if (totalCount > 0)
		{
			num = (float)hasProcessCount * 1f / (float)totalCount;
			if (num < 0f)
			{
				num = 0f;
			}
			else if (num > 1f)
			{
				num = 1f;
			}
		}
		lock (_lockObject)
		{
			if (_progressValue < num)
			{
				_progressValue = num;
			}
		}
	}

	public ProgressSnapshotData CreateSnapshot()
	{
		lock (_lockObject)
		{
			ProgressSnapshotData progressSnapshotData = new ProgressSnapshotData();
			progressSnapshotData.ProgressValue = _progressValue;
			progressSnapshotData.Step = _step;
			progressSnapshotData.IsTaskEnd = _isTaskEnd;
			progressSnapshotData.Message = _message;
			progressSnapshotData.PostfixString = _postfixString;
			return progressSnapshotData;
		}
	}
}
