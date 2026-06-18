using System;
using Leqisoft.DTO;

namespace Leqisoft.UI.Platform;

public class FormProgressFrameUpdater
{
	public class ServerProgressData
	{
		public string ActionId { get; set; }

		public int Progress { get; set; }

		public string Message { get; set; }
	}

	protected object _lockObject = new object();

	protected ServerProgressData _serverProgressData;

	protected IProgressDisplayStringFormatter _progressFormatter;

	protected object _formatterLockObject = new object();

	public FormProgressFrameUpdater(IProgressDisplayStringFormatter progressFrommater)
	{
		_progressFormatter = progressFrommater;
	}

	public ServerProgressData GetServerProgressData()
	{
		lock (_lockObject)
		{
			if (_serverProgressData == null)
			{
				return null;
			}
			ServerProgressData serverProgressData = new ServerProgressData();
			serverProgressData.ActionId = _serverProgressData.ActionId;
			serverProgressData.Progress = _serverProgressData.Progress;
			serverProgressData.Message = _serverProgressData.Message;
			return serverProgressData;
		}
	}

	public void OnServerProgressChanged(string actionId, float progress, string msg)
	{
		lock (_lockObject)
		{
			if (_serverProgressData == null)
			{
				_serverProgressData = new ServerProgressData();
			}
			_serverProgressData.ActionId = actionId;
			_serverProgressData.Progress = (int)Math.Max(Math.Min(progress * 100f, 100f), 0f);
			_serverProgressData.Message = msg;
		}
	}

	public ProgressInfo OnGetFormProgressInfo()
	{
		if (_progressFormatter != null)
		{
			lock (_formatterLockObject)
			{
				return _progressFormatter.OnGetFormProgressInfo(this);
			}
		}
		return new ProgressInfo
		{
			MainCaption = "正在处理中，请稍后...",
			MainProgress = 0
		};
	}
}
