using System.Drawing;
using Auditai.Model;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandTicketAlignTopLeft : AppCommandButton
{
	public override System.Drawing.Image SmallIcon => Resources.tb_AlignTopLeft;

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.SetAlign(CellTextAlign.TopLeft);
	}
}
