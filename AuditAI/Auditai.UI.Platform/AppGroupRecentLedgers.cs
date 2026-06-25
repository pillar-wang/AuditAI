namespace Auditai.UI.Platform;

public class AppGroupRecentLedgers : AppCommandGroup
{
	public override string Text => "已打开账套";

	public override void GenerateRibbonGroup()
	{
		base.GenerateRibbonGroup();
		base.Visible = false;
	}
}
