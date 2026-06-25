using System.Drawing;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandRefreshRefTable : AppCommandButton
{
	public override string Text => "表格刷新";

	public override Image LargeIcon => Auditai.UI.Platform.Properties.Resources.TableWholeReflush;

	protected override string Tooltip => TipResource.表格刷新按钮;

	protected override void Clicked()
	{
		Program.MainForm.CurrentDocumentEditor.RefreshTableWithFormat();
	}
}
