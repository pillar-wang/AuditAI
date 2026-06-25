using System.Drawing;
using System.Windows.Forms;
using Auditai.UI.Controls;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

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
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先打开账套！");
			}
			else
			{
				Program.MainForm.LedgerExport();
			}
		}
	}
}
