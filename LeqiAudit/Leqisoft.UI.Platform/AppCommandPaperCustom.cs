using System.Drawing.Printing;
using Leqisoft.UI.Controls;

namespace Leqisoft.UI.Platform;

public class AppCommandPaperCustom : AppCommandButton
{
	public override string Text => "自定义";

	protected override void Clicked()
	{
		if (InputForm.NumRange("自定义纸张", "请输入纸张大小，宽度×高度，单位为毫米：", out var min, out var max).HasValue)
		{
			double width = (double)min;
			double height = (double)max;
			switch (Program.MainForm.State.ViewKind)
			{
			case MainFormView.Table:
			case MainFormView.TablePreview:
				Program.MainForm.Preview.SetPaperCustom(width, height);
				break;
			case MainFormView.Document:
			case MainFormView.DocumentPreview:
				Program.MainForm.CurrentDocumentEditor.SetPaperCustom(width, height);
				break;
			case MainFormView.TicketInput:
				Program.MainForm.TicketInputEditor.SetPaperCustom(width, height);
				break;
			case MainFormView.TicketPrint:
				Program.MainForm.TicketPrinter.SetPaperCustom(width, height);
				break;
			}
			AppCommands.Paper.SelectPaper(PaperKind.Custom);
		}
	}
}
