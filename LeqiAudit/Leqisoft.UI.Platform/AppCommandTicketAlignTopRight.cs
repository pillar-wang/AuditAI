using System.Drawing;
using Leqisoft.Model;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandTicketAlignTopRight : AppCommandButton
{
	public override System.Drawing.Image SmallIcon => Resources.tb_AlignTopRight;

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.SetAlign(CellTextAlign.TopRight);
	}
}
