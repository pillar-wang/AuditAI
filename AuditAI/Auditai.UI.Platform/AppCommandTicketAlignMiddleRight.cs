using System.Drawing;
using Auditai.Model;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandTicketAlignMiddleRight : AppCommandButton
{
	public override System.Drawing.Image SmallIcon => IconRes.tb_AlignMiddleRight;

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.SetAlign(CellTextAlign.MiddleRight);
	}
}
