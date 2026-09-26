using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppGroupRiskCheck : AppCommandGroup
{
	public override string Text => "风险检查";

	public override Image Image => IconRes.Shield;

	public AppGroupRiskCheck()
	{
		base.Commands.Add(AppCommands.RiskCheck);
	}
}
