using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandPageColumns : AppCommandMenu
{
	public override string Text => "页面分栏";

	public override Image LargeImage => Resources.DocPageColumns;

	public AppCommandPageColumns()
		: base(new AppCommandBase[4]
		{
			AppCommands.Page1Column,
			AppCommands.Page2Columns,
			AppCommands.Page3Columns,
			AppCommands.PageMultiColumns
		})
	{
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		Visible = state.ViewKind == MainFormView.Document || state.ViewKind == MainFormView.DocumentPreview;
	}
}
