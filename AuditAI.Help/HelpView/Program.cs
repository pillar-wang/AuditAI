using System;
using System.Windows.Forms;

namespace HelpView;

internal static class Program
{
	public static string DefaultBookmark;

	[STAThread]
	private static void Main(params string[] args)
	{
		DefaultBookmark = ((args == null) ? null : ((args.Length == 0) ? null : args[0]?.Replace("#", " ")));
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(defaultValue: false);
		Application.Run(new HelpViewerForm());
	}
}
