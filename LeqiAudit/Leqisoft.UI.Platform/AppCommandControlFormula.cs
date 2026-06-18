using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandControlFormula : AppCommandButton
{
	public override string Text => "控制公式";

	public override Image LargeIcon => Resources.ControlFormula;

	protected override void Clicked()
	{
		Program.MainForm.TableEditor.SetControlFormula();
	}
}
