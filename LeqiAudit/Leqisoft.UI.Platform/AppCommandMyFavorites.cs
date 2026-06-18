using System.Drawing;
using System.Windows.Forms;
using Leqisoft.UI.Controls;
using Leqisoft.UI.Controls.Properties;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandMyFavorites : AppCommandButton
{
	public override string Text => "我的关注";

	public override Image LargeIcon => Leqisoft.UI.Platform.Properties.Resources.MarkFocus;

	protected override string Tooltip => TipResource.我的关注按钮;

	protected override void Clicked()
	{
		if (Program.MainForm.CurrentLedgerViewer == null)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先打开账套！");
		}
		else
		{
			Program.MainForm.CurrentLedgerViewer.ShowMarkedVouchers();
		}
	}
}
