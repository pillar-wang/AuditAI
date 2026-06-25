using System.Drawing;
using System.Windows.Forms;
using Auditai.UI.Controls;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandStructureAnalysis : AppCommandButton
{
	public override string Text => "结构分析";

	public override Image LargeIcon => Resources.CommonSizeAnalysis;

	protected override void Clicked()
	{
		if (Program.MainForm.CurrentLedgerViewer == null)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先打开账套！");
		}
		else
		{
			Program.MainForm.CurrentLedgerViewer.ShowPie();
		}
	}
}
