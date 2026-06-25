using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandGenerateBatchFormula : AppCommandButton
{
	public override Image LargeIcon => Resources.GenerateBatchFormula;

	public override string Text => "智能扩充跨表公式";

	protected override void Clicked()
	{
		Program.MainForm.TableEditor.GenerateBatchFormula();
	}
}
