using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandTablePrint : AppCommandButton
{
	public override string Text => "直接打印";

	public override Image LargeIcon => Resources.Print;

	protected override void Clicked()
	{
		Program.MainForm.Print();
	}
}
