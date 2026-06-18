using System.Drawing;
using System.Windows.Forms;
using Leqisoft.UI.Controls;
using Leqisoft.UI.Controls.Properties;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandFillToTable : AppCommandButton
{
	public override string Text => "填充至底稿";

	public override Image LargeIcon => Leqisoft.UI.Platform.Properties.Resources.batchFill;

	protected override string Tooltip => TipResource.填充至底稿按钮;

	protected override void Clicked()
	{
		if (Program.MainForm.CurrentLedgerViewer == null)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先打开账套！");
		}
		else
		{
			Program.MainForm.CurrentLedgerViewer.FillToTable();
		}
	}
}
