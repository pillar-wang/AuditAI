using System;
using System.Drawing;
using System.Threading.Tasks;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

/// <summary>
/// "添加校验点" Ribbon 命令：将当前文档中的选定文本包裹为 DocValidation 域。
/// 若无可选文本或不在 Document 视图，按钮不可用。
/// </summary>
public class AppCommandAddValidationPoint : AppCommandButton
{
	public override string Text => "添加校验点";

	public override Image LargeIcon => Resources.Shield;

	protected override Func<Task> ClickedTask => delegate
	{
		var editor = Program.MainForm?.CurrentDocumentEditor;
		if (editor != null && editor.Tx?.Selection?.Length > 0)
		{
			editor.AddValidationPoint();
		}
		return Task.CompletedTask;
	};

	protected override string Tooltip => "将当前选定文本添加为文档校验点（DocValidation 域）";

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		Visible = state.ViewKind == MainFormView.Document;
	}
}