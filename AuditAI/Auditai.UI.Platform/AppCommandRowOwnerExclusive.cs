using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandRowOwnerExclusive : AppCommandToggleButton
{
	public override string Text => "增行独占编辑保护";

	public override Image LargeIcon => Resources.RowOwnerExclusive;

	protected override void Pressed()
	{
		Program.MainForm.TableEditor.SetRowOwnerExclusive(set: true);
	}

	protected override void Unpressed()
	{
		Program.MainForm.TableEditor.SetRowOwnerExclusive(set: false);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		if (state.ViewKind == MainFormView.Table)
		{
			base.ToggleButton.Text = "增行独占编辑保护";
		}
		else
		{
			base.ToggleButton.Text = "新增人独占编辑";
		}
	}
}
