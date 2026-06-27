using TXTextControl;

namespace Auditai.UI.Platform;

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
		// 确保 TXTextControl 许可证已注入，避免 ServerTextControl 构造函数触发 LicenseException
		Program.EnsureTXTextControlLicense();
		_stc = new ServerTextControl();
		Instance = new ServerTXWrapper();
		_stc.Create();
	}

	private ServerTXWrapper()
	{
	}
}
