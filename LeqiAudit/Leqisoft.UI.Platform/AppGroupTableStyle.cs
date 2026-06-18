using System.Drawing;
using Leqisoft.UI.Controls.Properties;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppGroupTableStyle : AppCommandGroup
{
	public override string Text => "表格样式";

	public override Image Image => Leqisoft.UI.Platform.Properties.Resources.TableStyle0;

	protected override string Tooltip => TipResource.表格样式;

	public AppGroupTableStyle()
	{
		base.Commands.Add(AppCommands.TableStyle);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		base.Visible = state.ViewKind == MainFormView.Table;
	}
}
