using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandControlFormula : AppCommandButton
{
	public override string Text => "控制公式";

	public override Image LargeIcon => Resources.ControlFormula;

	protected override void Clicked()
	{
		Program.MainForm.TableEditor.SetControlFormula();
	}
}
