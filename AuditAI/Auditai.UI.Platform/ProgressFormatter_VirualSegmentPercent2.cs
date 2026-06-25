using System;
using System.Threading.Tasks;
using Auditai.DTO;

namespace Auditai.UI.Platform;

public class ProgressFormatter_VirualSegmentPercent2 : IProgressDisplayStringFormatter
{
	protected class ProgressRepoter : Progress<ProgressInfo>
	{
		protected ProgressFormatter_VirualSegmentPercent2 _parent;

		public ProgressRepoter(ProgressFormatter_VirualSegmentPercent2 parent)
		{
			_parent = parent;
		}

		protected override void OnReport(ProgressInfo value)
		{
			_parent._progressReporter_ProgressChanged(this, value);
		}
	}

	protected int _segmentDuration;

	protected int _segmentIndex;

	protected DateTime _segmentStartTime = DateTime.Now;

	protected string _formatString;

	private object _lockObject = new object();

	protected ProgressRepoter _progressReporter;

	protected bool _isAllowResetSegmentIndex = true;

	protected ProgressInfo _lastDisplayValue;

	protected object _lockLastDisplayProgoressValue = new object();

	protected ProgressInfo _forceToDisplayValue;

	protected object _lockForceToDisplayValue = new object();

	protected bool _isShowBatchNumber = true;

	protected string FormatString
	{
		get
		{
			lock (_lockObject)
			{
				return _formatString;
			}
		}
		set
		{
			lock (_lockObject)
			{
				_formatString = value;
			}
		}
	}

	protected int SegmentIndex
	{
		get
		{
			lock (_lockObject)
			{
				return _segmentIndex;
			}
		}
		set
		{
			lock (_lockObject)
			{
				_segmentStartTime = DateTime.Now;
				_segmentIndex = value;
			}
		}
	}

	protected ProgressInfo LastDisplayProgressInfo
	{
		get
		{
			lock (_lockLastDisplayProgoressValue)
			{
				if (_lastDisplayValue == null)
				{
					return null;
				}
				return new ProgressInfo
				{
					MainCaption = _lastDisplayValue.MainCaption,
					MainProgress = _lastDisplayValue.MainProgress
				};
			}
		}
		set
		{
			lock (_lockLastDisplayProgoressValue)
			{
				if (_lastDisplayValue == null)
				{
					_lastDisplayValue = new ProgressInfo();
				}
				_lastDisplayValue.MainCaption = value.MainCaption;
				_lastDisplayValue.MainProgress = value.MainProgress;
			}
		}
	}

	protected ProgressInfo ForceToDisplayProgressValue
	{
		get
		{
			lock (_lockForceToDisplayValue)
			{
				if (_forceToDisplayValue == null)
				{
					return null;
				}
				return new ProgressInfo
				{
					MainCaption = _forceToDisplayValue.MainCaption,
					MainProgress = _forceToDisplayValue.MainProgress
				};
			}
		}
		set
		{
			lock (_lockForceToDisplayValue)
			{
				if (value == null)
				{
					_forceToDisplayValue = null;
					return;
				}
				if (_forceToDisplayValue == null)
				{
					_forceToDisplayValue = new ProgressInfo();
				}
				_forceToDisplayValue.MainCaption = value.MainCaption;
				_forceToDisplayValue.MainProgress = value.MainProgress;
				if (value.MainCaption != null)
				{
					FormatString = value.MainCaption;
				}
			}
		}
	}

	public bool AllowResetSegmentIndex
	{
		get
		{
			lock (_lockObject)
			{
				return _isAllowResetSegmentIndex;
			}
		}
		set
		{
			lock (_lockObject)
			{
				_isAllowResetSegmentIndex = value;
			}
		}
	}

	public bool ShowBatchNumber
	{
		get
		{
			lock (_lockObject)
			{
				return _isShowBatchNumber;
			}
		}
		set
		{
			lock (_lockObject)
			{
				_isShowBatchNumber = false;
			}
		}
	}

	public ProgressFormatter_VirualSegmentPercent2(int segmentTime = 5000)
	{
		_segmentDuration = segmentTime;
		_progressReporter = new ProgressRepoter(this);
	}

	protected void _progressReporter_ProgressChanged(object sender, ProgressInfo e)
	{
		lock (_lockObject)
		{
			string text = ((!ShowBatchNumber) ? e.MainCaption : ((!e.MainCaption.EndsWith("...")) ? (e.MainCaption + " {0}") : (e.MainCaption.Substring(0, e.MainCaption.Length - 3) + "{0}...")));
			if (!(text == _formatString))
			{
				_formatString = text;
				if (_isAllowResetSegmentIndex)
				{
					_segmentIndex = 0;
					_segmentStartTime = DateTime.Now;
				}
			}
		}
	}

	public void ResetSegmentIndex()
	{
		SegmentIndex = 0;
	}

	public ProgressInfo OnGetFormProgressInfo(FormProgressFrameUpdater updater)
	{
		GetCurrentSegmentInfo(out var segmentStartTime, out var segmentIndex);
		int num = 0;
		double totalMilliseconds = DateTime.Now.Subtract(segmentStartTime).TotalMilliseconds;
		if (totalMilliseconds > (double)_segmentDuration)
		{
			SegmentIndex = segmentIndex + 1;
			num = 0;
		}
		else
		{
			num = (int)(totalMilliseconds / (double)_segmentDuration * 100.0);
		}
		string text = FormatString;
		if (text == null)
		{
			text = string.Empty;
		}
		string arg = ((segmentIndex == 0) ? string.Empty : $"(正在处理第 {segmentIndex + 1} 批数据)");
		ProgressInfo progressInfo = new ProgressInfo();
		progressInfo.MainCaption = string.Format(text, arg);
		progressInfo.MainProgress = num;
		ProgressInfo forceToDisplayProgressValue = ForceToDisplayProgressValue;
		if (forceToDisplayProgressValue != null)
		{
			progressInfo.MainProgress = forceToDisplayProgressValue.MainProgress;
			if (forceToDisplayProgressValue.MainCaption != null)
			{
				progressInfo.MainCaption = forceToDisplayProgressValue.MainCaption;
			}
		}
		LastDisplayProgressInfo = progressInfo;
		return progressInfo;
	}

	private void GetCurrentSegmentInfo(out DateTime segmentStartTime, out int segmentIndex)
	{
		lock (_lockObject)
		{
			segmentStartTime = _segmentStartTime;
			segmentIndex = _segmentIndex;
		}
	}

	public IProgress<ProgressInfo> GetProgressReporter()
	{
		return _progressReporter;
	}

	public async Task DisplayProgressValueWithCondition(int progressValue, string progressCaption, Func<ProgressInfo, bool> condition)
	{
		if (!condition(LastDisplayProgressInfo))
		{
			return;
		}
		ForceToDisplayProgressValue = new ProgressInfo
		{
			MainCaption = progressCaption,
			MainProgress = progressValue
		};
		for (int i = 0; i < 10; i++)
		{
			ProgressInfo lastDisplayProgressInfo = LastDisplayProgressInfo;
			if (lastDisplayProgressInfo != null && lastDisplayProgressInfo.MainProgress == progressValue)
			{
				ForceToDisplayProgressValue = null;
				break;
			}
			await Task.Delay(TimeSpan.FromMilliseconds(200.0)).ConfigureAwait(continueOnCapturedContext: false);
		}
	}
}
