using System.Drawing;
using System.Windows.Forms;
using Leqisoft.UI.Controls;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandVouchers : AppCommandButton
{
	public override string Text => "记账凭证";

	public override Image LargeIcon => Resources.Vouchers;

	protected override void Clicked()
	{
		if (Program.MainForm.CurrentLedgerViewer == null)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先打开账套！");
		}
		else
		{
			Program.MainForm.CurrentLedgerViewer.ShowVoucherList();
		}
	}
}
