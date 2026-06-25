namespace Auditai.UI.Platform;

public class AppCommandStartPage : AppCommandNumericBox
{
	public override string Text => "起始页号";

	public override int Width => 20;

	protected override void Changed(decimal value)
	{
		if (Program.MainForm.CurrentView == MainFormView.TicketPrint)
		{
			Program.MainForm.TicketPrinter.SetStartPage((int)value);
		}
		else
		{
			Program.MainForm.Preview.SetStartPageNo((int)value);
		}
	}
}
