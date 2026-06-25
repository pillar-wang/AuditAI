using System;
using System.Drawing;
using System.Threading.Tasks;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandConsolidateSetting : AppCommandButton
{
	public override string Text => "合并报表设置";

	public override Image LargeIcon => Auditai.UI.Platform.Properties.Resources.ConsolidateSettings;

	protected override Func<Task> ClickedTask => async delegate
	{
		await Program.MainForm.TableEditor.EditConsolidateSettings();
	};

	protected override string Tooltip => TipResource.合并设置按钮;
}
