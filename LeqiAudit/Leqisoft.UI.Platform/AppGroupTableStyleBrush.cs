using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppGroupTableStyleBrush : AppCommandGroup
{
	public override string Text => "样式刷";

	public override Image Image => Resources.TableStyleBrush;

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		base.Visible = state.ViewKind == MainFormView.Table;
	}
}
