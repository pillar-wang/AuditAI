﻿using TXTextControl;

namespace Leqisoft.UI.Platform;

public class ServerTXWrapper
{
	private static ServerTextControl _stc;

	public static ServerTXWrapper Instance { get; }

	public ServerTextControl GetTx()
	{
		return _stc;
	}

	static ServerTXWrapper()
	{
		_stc = new ServerTextControl();
		Instance = new ServerTXWrapper();
		_stc.Create();
	}

	private ServerTXWrapper()
	{
	}
}
