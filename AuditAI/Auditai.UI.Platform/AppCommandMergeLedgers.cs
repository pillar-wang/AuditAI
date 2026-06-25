using System;
using System.Drawing;
using System.Threading.Tasks;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandMergeLedgers : AppCommandButton
{
	public override string Text => "合并账套";

	public override Image LargeIcon => Auditai.UI.Platform.Properties.Resources.LedgerMerge;

	protected override Func<Task> ClickedTask => async delegate
	{
		await Program.MainForm.MergeLedger();
	};

	protected override string Tooltip => TipResource.合并账套按钮;
}
