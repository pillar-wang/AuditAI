using System.Drawing;
using System.Windows.Forms;
using Auditai.UI.Controls;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandLedgerPortrait : AppCommandButton
{
	public override string Text => "\u3000纵向\u3000";

	public override Image LargeIcon => Resources.Portrait;

	public override void GenerateRibbonItem()
	{
		base.GenerateRibbonItem();
		Visible = false;
	}

	protected override void Clicked()
	{
		if (Program.MainForm.CurrentLedgerViewer == null)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先打开账套！");
		}
		else
		{
			Program.MainForm.PreviewDirection(landscape: false);
		}
	}
}
