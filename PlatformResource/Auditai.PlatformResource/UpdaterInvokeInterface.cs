namespace Auditai.PlatformResource;

public class UpdaterInvokeInterface
{
	protected PlatformType _platformType = PlatformType.UnKnown;

	public bool ParseKeyFileData(byte[] keyFileData)
	{
		_platformType = PlatformTypeParser.ParsePlatformType(keyFileData);
		if (_platformType == PlatformType.UnKnown)
		{
			return false;
		}
		return true;
	}

	public string GetApplicationIconFilePath()
	{
		if (_platformType == PlatformType.UnKnown)
		{
			return null;
		}
		return ApplicationIconManger.GetApplicationIconFilePath(_platformType);
	}

	public string GetApplicationLinkName()
	{
		return ApplicationNameManager.GetApplicationName(_platformType) + ".lnk";
	}

	public string GetApplicationLinkDescription()
	{
		return ApplicationNameManager.GetApplicationName(_platformType);
	}

	public bool IsApplicationIconChanged()
	{
		return ApplicationIconManger.IsIconVersionChanged;
	}
}
