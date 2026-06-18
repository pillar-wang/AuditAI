namespace DbAccess;

public class DefaultProgressInfo : ProgressInfo
{
	public bool done { get; set; }

	public bool success { get; set; }

	public int percent { get; set; }

	public string msg { get; set; }

	public DefaultProgressInfo()
	{
	}

	public DefaultProgressInfo(bool done, bool success, int percent, string msg)
	{
		this.done = done;
		this.success = success;
		this.percent = percent;
		this.msg = msg;
	}
}
