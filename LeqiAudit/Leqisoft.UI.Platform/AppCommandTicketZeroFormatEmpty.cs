using Leqisoft.Model;

namespace Leqisoft.UI.Platform;

public class AppCommandTicketZeroFormatEmpty : AppCommandButton
{
	public override string Text => "显示为空值";

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.SetZeroFormat(ZeroFormat.Empty);
	}
}
