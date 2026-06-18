using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandGenerateBatchFormula : AppCommandButton
{
	public override Image LargeIcon => Resources.GenerateBatchFormula;

	public override string Text => "智能扩充跨表公式";

	protected override void Clicked()
	{
		Program.MainForm.TableEditor.GenerateBatchFormula();
	}
}
