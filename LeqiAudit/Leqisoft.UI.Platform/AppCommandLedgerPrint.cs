using System.Drawing;
using System.Windows.Forms;
using Leqisoft.UI.Controls;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandLedgerPrint : AppCommandButton
{
	public override string Text => "直接打印";

	public override Image LargeIcon => Resources.Print;

	protected override void Clicked()
	{
		if (!SoftwareLicenseManager.IsPrintLedgerOutOfLicenseLimit())
		{
			if (Program.MainForm.CurrentLedgerViewer == null)
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先打开账套！");
			}
			else
			{
				Program.MainForm.LedgerPrint();
			}
		}
	}
}
