using System.Drawing;

namespace Auditai.UI.Platform;

/// <summary>
/// “设置”选项卡：标准科目字典维护入口（与“系统设置”同组）
/// </summary>
public class AppCommandStandardAccountDic : AppCommandButton
{
	public override string Text => "标准科目字典";

	public override Image LargeIcon => IconRes.TicketNav;

	protected override void Clicked()
	{
		Program.MainForm.ShowStandardAccountDic();
	}
}
