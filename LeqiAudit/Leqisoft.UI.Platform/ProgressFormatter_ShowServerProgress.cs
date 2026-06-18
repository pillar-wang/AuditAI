using System;
using Leqisoft.DTO;

namespace Leqisoft.UI.Platform;

public class ProgressFormatter_ShowServerProgress : IProgressDisplayStringFormatter
{
	protected Func<string, int, string, ProgressInfo> _serverProgressFormatCallback;

	protected ProgressInfo _defaultProgressInfo;

	public ProgressFormatter_ShowServerProgress(Func<string, int, string, ProgressInfo> serverProgressFormatCallback, ProgressInfo defaultProgress)
	{
		_serverProgressFormatCallback = serverProgressFormatCallback;
		_defaultProgressInfo = defaultProgress;
	}

	public ProgressInfo OnGetFormProgressInfo(FormProgressFrameUpdater updater)
	{
		FormProgressFrameUpdater.ServerProgressData serverProgressData = updater.GetServerProgressData();
		if (serverProgressData == null)
		{
			if (_defaultProgressInfo == null)
			{
				return new ProgressInfo();
			}
			return _defaultProgressInfo;
		}
		return _serverProgressFormatCallback(serverProgressData.ActionId, serverProgressData.Progress, serverProgressData.Message);
	}
}
