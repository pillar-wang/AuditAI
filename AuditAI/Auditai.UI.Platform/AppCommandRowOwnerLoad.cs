using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandRowOwnerLoad : AppCommandToggleButton
{
	public override string Text => "增行独占可见保护";

	public override Image LargeIcon => Resources.RowOwnerLoad;

	protected override void Pressed()
	{
		Program.MainForm.TableEditor.SetRowOwnerLoad(set: true);
	}

	protected override void Unpressed()
	{
		Program.MainForm.TableEditor.SetRowOwnerLoad(set: false);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		if (state.ViewKind == MainFormView.Table)
		{
			base.ToggleButton.Text = "增行独占可见保护";
		}
		else
		{
			base.ToggleButton.Text = "新增人独占可见";
		}
	}
}
