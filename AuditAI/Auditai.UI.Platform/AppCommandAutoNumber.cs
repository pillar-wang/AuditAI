﻿using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Auditai.UI.Controls;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandAutoNumber : AppCommandButton
{
	public override string Text => "自动编号";

	public override Image LargeIcon => IconRes.IndexNumber;

	public override Image SmallIcon => IconRes.IndexNumber;

	protected override string Tooltip => "自动重排文档中所有编号";

	protected override Func<Task> ClickedTask => async delegate
	{
		try
		{
			var editor = Program.MainForm?.CurrentDocumentEditor;
			if (editor == null) return;

			if (Auditai.UI.Controls.MessageBox.Show(
				MessageBoxIcon.Question, "将自动重排文档中所有编号，是否继续？",
				MessageBoxButtons.YesNo, "自动编号") != DialogResult.Yes)
			{
				return;
			}

			var structure = editor.Structure;
			int changed = structure.AutoNumber();
			if (changed == 0)
			{
				Auditai.UI.Controls.MessageBox.Show(
					MessageBoxIcon.Information, "编号已正确，无需调整。",
					MessageBoxButtons.OK, "自动编号");
			}
			else
			{
				editor.MakeIds();
				await structure.Populate();
			}
		}
		catch (Exception ex)
		{
			ex.Log("AppCommandAutoNumber.Clicked");
		}
	};
}
