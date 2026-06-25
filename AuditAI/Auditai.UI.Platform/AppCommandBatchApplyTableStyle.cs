﻿using System.Windows.Forms;

namespace Auditai.UI.Platform;

/// <summary>
/// 批量应用表格样式命令按钮。
/// 点击弹出 frmBatchApplyRange 对话框进行范围选择和批量应用。
/// </summary>
public class AppCommandBatchApplyTableStyle : AppCommandButton
{
	public override string Text => "批量应用样式";

	protected override void Clicked()
	{
		var mainForm = Program.MainForm;
		if (mainForm == null) return;

		var docEditor = mainForm.CurrentDocumentEditor;
		if (docEditor == null)
		{
			MessageBox.Show("请先打开文档", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
			return;
		}

		// 确保文档结构图已显示，便于用户在结构树上选择范围
		docEditor.ShowStructure();

		using (var dlg = new frmBatchApplyRange(docEditor.Structure, docEditor))
		{
			dlg.ShowDialog();
		}
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		Visible = state.ViewKind == MainFormView.Document;
	}
}
