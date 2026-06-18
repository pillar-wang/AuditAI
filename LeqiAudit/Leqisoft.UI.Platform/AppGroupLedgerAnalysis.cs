using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppGroupLedgerAnalysis : AppCommandGroup
{
	public override string Text => "数据分析";

	public override Image Image => Resources.TrendAnalysis;

	public AppGroupLedgerAnalysis()
	{
		base.Commands.Add(AppCommands.AgeAnalysis);
		base.Commands.Add(AppCommands.TrendAnalysis);
		base.Commands.Add(AppCommands.StructureAnalysis);
	}
}
