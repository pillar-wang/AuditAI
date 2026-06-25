using System;
using System.Drawing;
using System.Threading.Tasks;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

/// <summary>
/// "解除校验点" Ribbon 命令：移除当前光标所在位置的 DocValidation 域校验规则。
/// 仅当光标位于 DocValidation 域上时可用。
/// </summary>
public class AppCommandRemoveValidationPoint : AppCommandButton
{
	public override string Text => "解除校验点";

	public override Image LargeIcon => Resources.ReviewCancel;

	protected override Func<Task> ClickedTask => delegate
	{
		var editor = Program.MainForm?.CurrentDocumentEditor;
		if (editor != null)
		{
			var field = editor.GetCurrentApplicationField();
			if (field != null && field.TypeName == "MERGEFIELD"
				&& field.Parameters != null && field.Parameters.Length >= 1
				&& field.Parameters[0] == "DocValidation")
			{
				editor.RemoveValidationPoint();
			}
		}
		return Task.CompletedTask;
	};

	protected override string Tooltip => "解除当前光标所在的文档校验点，移除校验规则但保留文字内容";

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		Visible = state.ViewKind == MainFormView.Document;
	}
}