using System.Drawing;
using System.Windows.Forms;
using Auditai.UI.Controls;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandGeneralLedger : AppCommandButton
{
	public override string Text => "\u3000总账\u3000";

	public override Image LargeIcon => Resources.GeneralLedger;

	protected override void Clicked()
	{
		if (Program.MainForm.CurrentLedgerViewer == null)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先打开账套！");
		}
		else
		{
			Program.MainForm.CurrentLedgerViewer.SelectTotal();
		}
	}
}
