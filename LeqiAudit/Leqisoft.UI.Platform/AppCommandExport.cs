using System.Drawing;
using System.Windows.Forms;
using Leqisoft.UI.Controls;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandExport : AppCommandButton
{
	public override string Text => "Excel文件";

	public override Image LargeIcon => Resources.ExportExcel;

	protected override void Clicked()
	{
		if (!SoftwareLicenseManager.IsExportLedgerOutOfLicenseLimit())
		{
			if (Program.MainForm.CurrentLedgerViewer == null)
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先打开账套！");
			}
			else
			{
				Program.MainForm.LedgerExport();
			}
		}
	}
}
