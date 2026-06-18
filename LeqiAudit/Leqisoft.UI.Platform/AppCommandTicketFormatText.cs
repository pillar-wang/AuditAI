namespace Leqisoft.UI.Platform;

public class AppCommandTicketFormatText : AppCommandButton
{
	public override string Text => "文本格式";

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.SetFormatText();
	}
}
