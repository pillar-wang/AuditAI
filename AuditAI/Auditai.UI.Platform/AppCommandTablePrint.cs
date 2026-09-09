using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandTablePrint : AppCommandButton
{
	public override string Text => "直接打印";

	public override Image LargeIcon => IconRes.Print;

	protected override void Clicked()
	{
		Program.MainForm.Print();
	}
}
