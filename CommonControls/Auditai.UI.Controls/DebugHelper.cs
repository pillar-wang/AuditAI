using System.Diagnostics;

namespace Auditai.UI.Controls;

public class DebugHelper
{
	private Stopwatch watch = new Stopwatch();

	private string prefix = string.Empty;

	public DebugHelper(string prefix)
	{
		this.prefix = prefix.ToUpper();
	}

	public DebugHelper Start()
	{
		watch.Start();
		return this;
	}

	public void WriteLog(int position)
	{
		watch.Restart();
	}
}
