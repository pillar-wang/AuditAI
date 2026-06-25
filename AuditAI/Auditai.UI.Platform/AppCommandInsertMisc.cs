using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandInsertMisc : AppCommandMenu
{
	public override string Text => "插入其他";

	public override Image LargeImage => Resources.InsertOther;

	public AppCommandInsertMisc()
		: base(new AppCommandBase[10]
		{
			AppCommands.InsertTable,
			AppCommands.InsertImage,
			AppCommands.InsertTextFrame,
			new AppCommandSeparator(),
			AppCommands.InsertSectionBreak,
			AppCommands.InsertPageBreak,
			AppCommands.InsertSymbol,
			new AppCommandSeparator(),
			AppCommands.InsertHeader,
			AppCommands.InsertFooter
		})
	{
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		Visible = state.ViewKind != MainFormView.EditingNote;
	}
}
