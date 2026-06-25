using System.Drawing;
using System.Windows.Forms;
using Auditai.UI.Controls;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandFillToTable : AppCommandButton
{
	public override string Text => "填充至底稿";

	public override Image LargeIcon => Auditai.UI.Platform.Properties.Resources.batchFill;

	protected override string Tooltip => TipResource.填充至底稿按钮;

	protected override void Clicked()
	{
		if (Program.MainForm.CurrentLedgerViewer == null)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先打开账套！");
		}
		else
		{
			Program.MainForm.CurrentLedgerViewer.FillToTable();
		}
	}
}
