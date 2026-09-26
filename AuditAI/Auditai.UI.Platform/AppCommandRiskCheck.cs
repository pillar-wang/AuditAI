using System.Drawing;
using System.Windows.Forms;
using Auditai.UI.Controls;

namespace Auditai.UI.Platform;

public class AppCommandRiskCheck : AppCommandButton
{
	public override string Text => "风险检查";

	// TODO: gen-iconres.ps1 / icon-map.csv 生成流程暂缺（仓库中未找到脚本与映射表），临时复用现有 Shield 图标；
	// 待恢复生成流程后，在 icon-map.csv 增加 shield-warning（语义色 sky）并重新生成 IconRes.RiskCheck 专用图标。
	public override Image LargeIcon => IconRes.Shield;

	protected override string Tooltip => "对当前账套执行风险检查，识别账务数据中的潜在风险项";

	protected override void Clicked()
	{
		if (Program.MainForm.CurrentLedgerViewer == null)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先打开账套！");
		}
		else
		{
			Program.MainForm.CurrentLedgerViewer.ShowRiskCheck();
		}
	}
}
