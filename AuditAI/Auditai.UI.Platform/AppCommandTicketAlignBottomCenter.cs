using System.Drawing;
using Auditai.Model;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandTicketAlignBottomCenter : AppCommandButton
{
	public override System.Drawing.Image SmallIcon => Resources.AlignBottomCenter;

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.SetAlign(CellTextAlign.BottomCenter);
	}
}
