using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppGroupFormulaTip : AppCommandGroup
{
	public override string Text => "公式编辑";

	public override Image Image => Resources.FormulaCommit;

	public AppGroupFormulaTip()
	{
		base.Commands.Add(AppCommands.FormulaCommit);
		base.Commands.Add(AppCommands.FormulaCancel);
		base.Commands.Add(new AppCommandSeparator());
		base.Commands.Add(AppCommands.FormulaTip1);
		base.Commands.Add(AppCommands.FormulaTip2);
		base.Commands.Add(AppCommands.FormulaTip3);
	}
}
