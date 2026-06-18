using System.Drawing;
using System.Windows.Forms;
using Leqisoft.UI.Controls;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandMonthSummary : AppCommandButton
{
	public override string Text => "月度汇总表";

	public override Image LargeIcon => Resources.MonthSummary;

	protected override void Clicked()
	{
		if (Program.MainForm.CurrentLedgerViewer == null)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先打开账套！");
		}
		else
		{
			Program.MainForm.CurrentLedgerViewer.ShowMonthSummary();
		}
	}
}
