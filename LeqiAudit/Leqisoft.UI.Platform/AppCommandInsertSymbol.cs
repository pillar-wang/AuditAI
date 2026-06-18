using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandInsertSymbol : AppCommandButton
{
	public override string Text => "特殊符号";

	public override Image LargeIcon => Resources.Symbols;

	protected override void Clicked()
	{
		Program.MainForm.TableEditor.InsertSymbolsDialog();
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		Visible = state.ViewKind == MainFormView.Table;
	}
}
