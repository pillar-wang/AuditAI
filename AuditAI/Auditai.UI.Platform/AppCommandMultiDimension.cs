using System.Drawing;
using System.Windows.Forms;
using Auditai.UI.Controls;

namespace Auditai.UI.Platform;

public class AppCommandMultiDimension : AppCommandButton
{
	public override string Text => "多维核算";

	public override Image LargeIcon => IconRes.CalculateTable;

	protected override string Tooltip => "按辅助核算维度组合查询科目余额与明细";

	protected override void Clicked()
	{
		if (Program.MainForm.CurrentLedgerViewer == null)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先打开账套！");
		}
		else
		{
			Program.MainForm.CurrentLedgerViewer.ShowMultiDimension();
		}
	}
}
