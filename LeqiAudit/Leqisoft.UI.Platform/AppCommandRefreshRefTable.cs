using System.Drawing;
using Leqisoft.UI.Controls.Properties;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandRefreshRefTable : AppCommandButton
{
	public override string Text => "表格刷新";

	public override Image LargeIcon => Leqisoft.UI.Platform.Properties.Resources.TableWholeReflush;

	protected override string Tooltip => TipResource.表格刷新按钮;

	protected override void Clicked()
	{
		Program.MainForm.CurrentDocumentEditor.RefreshTableWithFormat();
	}
}
