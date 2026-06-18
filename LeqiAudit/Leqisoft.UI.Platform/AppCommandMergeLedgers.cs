using System;
using System.Drawing;
using System.Threading.Tasks;
using Leqisoft.UI.Controls.Properties;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandMergeLedgers : AppCommandButton
{
	public override string Text => "合并账套";

	public override Image LargeIcon => Leqisoft.UI.Platform.Properties.Resources.LedgerMerge;

	protected override Func<Task> ClickedTask => async delegate
	{
		await Program.MainForm.MergeLedger();
	};

	protected override string Tooltip => TipResource.合并账套按钮;
}
