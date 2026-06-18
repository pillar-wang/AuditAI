namespace Leqisoft.UI.Platform;

public class AppCommandWidthScale : AppCommandNumericBox
{
	public override string Text => "横向缩放";

	public override int Width => 20;

	public override void GenerateRibbonItem()
	{
		base.GenerateRibbonItem();
		base.NumericBox.TextAreaWidth = 35;
		base.NumericBox.Increment = 0.1m;
		base.NumericBox.Format = "P0";
	}

	protected override void Changed(decimal value)
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
		case MainFormView.TablePreview:
			Program.MainForm.Preview.ChangeHorizZoom((double)value);
			if (Program.MainForm.Preview.WidthLock && value > 1m)
			{
				base.Value = 1m;
			}
			break;
		case MainFormView.TicketInput:
			Program.MainForm.TicketInputEditor.SetWidthScale((double)value);
			break;
		case MainFormView.TicketPrint:
			Program.MainForm.TicketPrinter.SetWidthScale((double)value);
			break;
		}
	}
}
