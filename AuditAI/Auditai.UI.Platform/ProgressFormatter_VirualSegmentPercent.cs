using System;
using Auditai.DTO;

namespace Auditai.UI.Platform;

public class ProgressFormatter_VirualSegmentPercent : IProgressDisplayStringFormatter
{
	protected Func<int, float, string> _segmentCallback;

	protected int _segmentDuration;

	protected int _segmentIndex;

	protected DateTime _startTime = DateTime.Now;

	public ProgressFormatter_VirualSegmentPercent(Func<int, float, string> segmentChangeCallback, int segmentTime = 5000)
	{
		_segmentCallback = segmentChangeCallback;
		_segmentDuration = segmentTime;
	}

	public ProgressInfo OnGetFormProgressInfo(FormProgressFrameUpdater updater)
	{
		int num = 0;
		double totalMilliseconds = DateTime.Now.Subtract(_startTime).TotalMilliseconds;
		if (totalMilliseconds > (double)_segmentDuration)
		{
			_startTime = DateTime.Now;
			_segmentIndex++;
			num = 0;
		}
		else
		{
			num = (int)(totalMilliseconds / (double)_segmentDuration * 100.0);
		}
		ProgressInfo progressInfo = new ProgressInfo();
		progressInfo.MainCaption = _segmentCallback(_segmentIndex, (float)num * 0.01f);
		progressInfo.MainProgress = num;
		return progressInfo;
	}

	public static string FormatStringWithAppendPercentValue(string formatString, int segmentIndex, float segmentPercent, int segmentValue = 10000)
	{
		long num = ((segmentIndex == 0) ? 1 : (segmentIndex * segmentValue));
		long num2 = (segmentIndex + 1) * segmentValue;
		long num3 = (long)((float)segmentValue * segmentPercent) + num;
		if (num3 > num2)
		{
			num3 = num2;
		}
		string arg = $"({num}-{num2})/{num3}";
		return string.Format(formatString, arg);
	}

	public static string FormatStringWithAppendBatch(string formatString, int segmentIndex)
	{
		string arg = $"正在处理第 {segmentIndex + 1} 批数据";
		return string.Format(formatString, arg);
	}
}
