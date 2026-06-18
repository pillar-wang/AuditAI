namespace Leqisoft.Util;

public class TaskProgressValueUpdater
{
	protected float _baseValue;

	protected float _percent = 1f;

	protected float _lastProgressValue;

	protected TaskProgressValueReportCallback _progressChangeCallback;

	protected TaskProgressMessageReportCallback _messageReportCallback;

	protected TaskProgressValueResetCallback _resetProgressValueCallback;

	public float MinProgressValue => _baseValue;

	public float MaxProgressValue => _baseValue + _percent;

	public float CurrentProgressValue => _lastProgressValue;

	public float TotalPercent => _percent;

	public string UpdaterName { get; set; }

	public TaskProgressValueUpdater(float baseValue, float percent, TaskProgressValueReportCallback progressChangeCallback)
	{
		_baseValue = baseValue;
		_percent = percent;
		_lastProgressValue = baseValue;
		_progressChangeCallback = progressChangeCallback;
		_messageReportCallback = null;
		_resetProgressValueCallback = null;
	}

	public TaskProgressValueUpdater(float baseValue, float percent, TaskProgressValueReportCallback progressChangeCallback, TaskProgressMessageReportCallback messageChangeCallback)
	{
		_baseValue = baseValue;
		_percent = percent;
		_lastProgressValue = baseValue;
		_progressChangeCallback = progressChangeCallback;
		_messageReportCallback = messageChangeCallback;
		_resetProgressValueCallback = null;
	}

	public TaskProgressValueUpdater(float baseValue, float percent, TaskProgressValueReportCallback progressChangeCallback, TaskProgressMessageReportCallback messageChangeCallback, TaskProgressValueResetCallback resetProgressCallback)
	{
		_baseValue = baseValue;
		_percent = percent;
		_lastProgressValue = baseValue;
		_progressChangeCallback = progressChangeCallback;
		_messageReportCallback = messageChangeCallback;
		_resetProgressValueCallback = resetProgressCallback;
	}

	public void ResetProgressValue(string message)
	{
		_lastProgressValue = _baseValue;
		if (_resetProgressValueCallback != null)
		{
			_resetProgressValueCallback(message);
		}
	}

	public virtual void UpdateMessage(string message)
	{
		if (_messageReportCallback != null)
		{
			_messageReportCallback(message);
		}
	}

	public virtual void UpdateProgress(long hasProcessCount, long totalCount)
	{
		if (totalCount == 0L)
		{
			return;
		}
		float num = (float)hasProcessCount * 1f / (float)totalCount;
		if (num < 0f)
		{
			num = 0f;
		}
		else if (num > 1f)
		{
			num = 1f;
		}
		float num2 = _baseValue + num * _percent;
		_ = UpdaterName;
		if (!(num2 - _lastProgressValue < 0.01f))
		{
			_lastProgressValue = num2;
			if (_progressChangeCallback != null)
			{
				_progressChangeCallback(num2);
			}
		}
	}

	public virtual void UpdateProgress(float progressValue)
	{
		float num = progressValue;
		if (num < 0f)
		{
			num = 0f;
		}
		else if (num > 1f)
		{
			num = 1f;
		}
		float num2 = _baseValue + num * _percent;
		_ = UpdaterName;
		if (!(num2 - _lastProgressValue < 0.01f))
		{
			_lastProgressValue = num2;
			if (_progressChangeCallback != null)
			{
				_progressChangeCallback(num2);
			}
		}
	}
}
