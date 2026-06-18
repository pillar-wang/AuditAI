using Leqisoft.DTO;

namespace Leqisoft.UI.Platform;

public class ProgressFormatter_ConstString : IProgressDisplayStringFormatter
{
	protected string _progressCaption;

	protected int _progressValue;

	public ProgressFormatter_ConstString(string progressCaption, int progressValue)
	{
		_progressCaption = progressCaption;
		_progressValue = progressValue;
	}

	public ProgressInfo OnGetFormProgressInfo(FormProgressFrameUpdater updater)
	{
		return new ProgressInfo
		{
			MainCaption = _progressCaption,
			MainProgress = _progressValue
		};
	}
}
