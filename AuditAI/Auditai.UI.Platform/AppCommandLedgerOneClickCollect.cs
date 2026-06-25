using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Auditai.UI.Controls;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandLedgerOneClickCollect : AppCommandButton
{
	public override string Text => "一键批量生成底稿";

	public override Image LargeIcon => Resources.OneClickCollect;

	protected override Func<Task> ClickedTask => async delegate
	{
		if (!SoftwareLicenseManager.IsLedgerOneClickCollectOutOfLicenseLimit())
		{
			if (Program.MainForm.IsLedgerEmpty())
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先打开账套！");
			}
			else if (Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Question, "一键批量生成底稿需要一定时间，确定要执行该操作吗？", MessageBoxButtons.YesNo, "确认对话框") == DialogResult.Yes)
			{
				Program.MainForm.SwitchMainView();
				await Program.MainForm.OneClickCollect();
				Program.MainForm.SwitchFinanceView();
			}
		}
	};
}
