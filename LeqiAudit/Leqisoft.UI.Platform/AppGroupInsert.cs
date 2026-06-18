using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppGroupInsert : AppCommandGroup
{
	public override string Text => "插入元素";

	public override Image Image => Resources.InsertOther;

	public AppGroupInsert()
	{
		base.Commands.Add(AppCommands.InsertRefTable);
		base.Commands.Add(AppCommands.InsertMisc);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		base.Visible = state.ViewKind != MainFormView.DocFormatBrush;
	}
}
