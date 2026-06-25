using System;
using System.Drawing;
using System.Threading.Tasks;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

/// <summary>
/// "校验域管理" Ribbon 命令：打开文档校验域管理对话框，列出所有 DocValidation 域和 Formula 域稽核规则。
/// </summary>
public class AppCommandDocValidationMgmt : AppCommandButton
{
	public override string Text => "校验域管理";

	public override Image LargeIcon => Resources.ValidationSettings;

	protected override Func<Task> ClickedTask => delegate
	{
		var editor = Program.MainForm?.CurrentDocumentEditor;
		if (editor != null)
		{
			using (var dlg = new frmDocValidationMgmt(editor))
			{
				dlg.ShowDialog();
			}
		}
		return Task.CompletedTask;
	};

	protected override string Tooltip => "管理当前文档中的所有校验点与稽核规则";

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		Visible = state.ViewKind == MainFormView.Document;
	}
}