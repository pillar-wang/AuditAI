using System;
using System.Drawing;
using System.Threading.Tasks;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandRefreshDocument : AppCommandButton
{
	public override string Text => "全文刷新";

	public override Image LargeIcon => Auditai.UI.Platform.Properties.Resources.DocWholeRefresh;

	protected override Func<Task> ClickedTask => async delegate
	{
		await Program.MainForm.CurrentDocumentEditor.RefreshAllTablesAndFormulas();
	};

	protected override string Tooltip => TipResource.全文刷新按钮;
}
