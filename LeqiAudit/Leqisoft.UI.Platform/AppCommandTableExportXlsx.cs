using System.Drawing;
using Leqisoft.UI.Controls.Properties;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandTableExportXlsx : AppCommandButton
{
	public override string Text => "Excel文件";

	public override Image LargeIcon => Leqisoft.UI.Platform.Properties.Resources.ExportExcel;

	protected override string Tooltip => TipResource.excel文件按钮;

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
		case MainFormView.TablePreview:
			Program.MainForm.ExportExcelDialog();
			break;
		case MainFormView.TicketInput:
		case MainFormView.TicketPrint:
			Program.MainForm.TicketInputEditor.ExportXlsx();
			break;
		}
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		Visible = state.ViewKind == MainFormView.Table || state.ViewKind == MainFormView.TablePreview || state.ViewKind == MainFormView.TicketPrint || state.ViewKind == MainFormView.TicketInput;
	}
}
