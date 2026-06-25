using System.Drawing;
using Auditai.Model;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandTicketAlignBottomRight : AppCommandButton
{
	public override System.Drawing.Image SmallIcon => Resources.tb_AlignBottomRight;

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.SetAlign(CellTextAlign.BottomRight);
	}
}
