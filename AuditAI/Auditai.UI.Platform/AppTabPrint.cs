namespace Auditai.UI.Platform;

public class AppTabPrint : AppCommandTab
{
	public override string Text => "打印输出";

	public AppTabPrint()
	{
		base.Groups.Add(AppCommandGroups.ApplyRange);
		base.Groups.Add(AppCommandGroups.Paper);
		base.Groups.Add(AppCommandGroups.PrintScale);
		base.Groups.Add(AppCommandGroups.Margin);
		base.Groups.Add(AppCommandGroups.HeaderMargin);
		base.Groups.Add(AppCommandGroups.Header);
		base.Groups.Add(AppCommandGroups.PrintMisc);
		base.Groups.Add(AppCommandGroups.PrintPreview);
		base.Groups.Add(AppCommandGroups.Export);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		base.Visible = state.ViewKind == MainFormView.Table || state.ViewKind == MainFormView.TablePreview || state.ViewKind == MainFormView.Document || state.ViewKind == MainFormView.DocumentPreview || state.ViewKind == MainFormView.Image || state.ViewKind == MainFormView.ImagePreview || state.ViewKind == MainFormView.Pdf || state.ViewKind == MainFormView.PdfPreview || state.ViewKind == MainFormView.TicketInput || state.ViewKind == MainFormView.TicketPrint;
		if (state.ViewKind == MainFormView.TablePreview || state.ViewKind == MainFormView.DocumentPreview || state.ViewKind == MainFormView.ImagePreview || state.ViewKind == MainFormView.PdfPreview || state.ViewKind == MainFormView.TicketPrint)
		{
			Select();
		}
	}
}
