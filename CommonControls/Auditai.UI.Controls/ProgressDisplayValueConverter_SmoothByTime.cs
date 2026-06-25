using System;

namespace Auditai.UI.Controls;

public class ProgressDisplayValueConverter_SmoothByTime : ProgressDisplayValueConverter
{
	protected DateTime _startTime = DateTime.Now;

	protected double _changeSpeed;

	protected double _startValue;

	protected double _currentSpeed;

	protected double _currentStartValue;

	protected int _currentStep = int.MinValue;

	protected float _lastReturnProgressValue;

	protected object _lockObject = new object();

	protected float LastReturnProgressValue
	{
		get
		{
			lock (_lockObject)
			{
				return _lastReturnProgressValue;
			}
		}
		set
		{
			lock (_lockObject)
			{
				_lastReturnProgressValue = value;
			}
		}
	}

	public ProgressDisplayValueConverter_SmoothByTime(float changeSpeed = 0.01f)
	{
		_changeSpeed = changeSpeed / 1000f;
		_currentSpeed = _changeSpeed;
		_startValue = 0.0;
		_currentStartValue = 0.0;
	}

	public ProgressDisplayValueConverter_SmoothByTime(float startValue, float changeSpeed)
	{
		_changeSpeed = changeSpeed / 1000f;
		_currentSpeed = _changeSpeed;
		_startValue = startValue;
		_currentStartValue = startValue;
	}

	public void StartTimer()
	{
		_startTime = DateTime.Now;
	}

	public float GetLastReturnProgressValue()
	{
		return LastReturnProgressValue;
	}

	public float GetProgressDislayValue(ProgressSnapshotData progressRealValue)
	{
		if (_currentStep != progressRealValue.Step)
		{
			_currentStep = progressRealValue.Step;
			_startTime = DateTime.Now;
			_currentSpeed = _changeSpeed;
			_currentStartValue = _startValue;
			LastReturnProgressValue = 0f;
			return 0f;
		}
		float lastReturnProgressValue = LastReturnProgressValue;
		float num = 0f;
		double totalMilliseconds = DateTime.Now.Subtract(_startTime).TotalMilliseconds;
		float num2 = (float)(_currentStartValue + totalMilliseconds * _currentSpeed);
		_startTime = DateTime.Now;
		float num3 = num2 - progressRealValue.ProgressValue;
		if (num3 < 0f)
		{
			if (num3 <= -0.4f)
			{
				_currentSpeed = _changeSpeed * 4.0;
				_currentStartValue = num2;
				num = num2;
			}
			else if (num3 <= -0.2f)
			{
				_currentSpeed = _changeSpeed * 2.0;
				_currentStartValue = num2;
				num = num2;
			}
			else if (num3 <= -0.1f)
			{
				_currentSpeed = _changeSpeed * 1.5;
				_currentStartValue = num2;
				num = num2;
			}
			else
			{
				_currentSpeed = _changeSpeed;
				_currentStartValue = num2;
				num = num2;
			}
		}
		else if (num3 < 0.05f)
		{
			_currentSpeed = _changeSpeed * 0.5;
			num2 = (float)(_currentStartValue + totalMilliseconds * _currentSpeed);
			_currentStartValue = num2;
			num = num2;
		}
		else if (num3 < 0.1f)
		{
			_currentSpeed = _changeSpeed * 0.25;
			num2 = (float)(_currentStartValue + totalMilliseconds * _currentSpeed);
			_currentStartValue = num2;
			num = num2;
		}
		else if (num3 < 0.15f)
		{
			_currentSpeed = _changeSpeed * 0.10000000149011612;
			num2 = (float)(_currentStartValue + totalMilliseconds * _currentSpeed);
			_currentStartValue = num2;
			num = num2;
		}
		else
		{
			num = lastReturnProgressValue;
		}
		if (num < lastReturnProgressValue)
		{
			num = lastReturnProgressValue;
		}
		if (num >= 0.98f)
		{
			num = 0.98f;
		}
		LastReturnProgressValue = num;
		return num;
	}
}
