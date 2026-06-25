namespace Auditai.UI.Platform;

public class AppCommandHeightScale : AppCommandNumericBox
{
	public override string Text => "纵向缩放";

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
			Program.MainForm.Preview.ChangeVertZoom((double)value);
			if (Program.MainForm.Preview.HeightLock && value > 1m)
			{
				base.Value = 1m;
			}
			break;
		case MainFormView.TicketInput:
			Program.MainForm.TicketInputEditor.SetHeightScale((double)value);
			break;
		case MainFormView.TicketPrint:
			Program.MainForm.TicketPrinter.SetHeightScale((double)value);
			break;
		}
	}
}
