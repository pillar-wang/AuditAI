﻿﻿using System;
using System.Drawing;
using System.Windows.Forms;
using Auditai.UI.Controls;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandCrossProjectDataRef : AppCommandButton
{
	public override string Text => "跨项目引用";

	public override System.Drawing.Image LargeIcon => Resources.ReferenceManager;

	protected override string Tooltip => "打开跨项目数据引用管理对话框，配置从其他项目引用数据到当前表";

	protected override void Clicked()
	{
		try
		{
			var project = Auditai.Model.Project.Current;
			var table = Program.MainForm?.TableEditor?.Table;
			if (project == null || table == null) return;
			using var frm = new frmCrossProjectDataRef(project, table.Id);
		frm.ShowDialog();
		// 如果有引用被刷新过，重新从数据库加载表格数据
		if (frm.DataRefreshed)
		{
			Program.MainForm?.TableEditor?.ReloadFromDb();
		}
		}
		catch (Exception ex)
		{
			ex.Log();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, $"打开跨项目数据引用失败:\n{ex.Message}\n\n堆栈:\n{ex.StackTrace}");
		}
	}
}