﻿﻿﻿﻿using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Leqisoft.UI.Controls;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandRefreshCrossProjectRefs : AppCommandButton
{
	public override string Text => "刷新引用";

	public override System.Drawing.Image LargeIcon => Resources.RefreshProject;

	protected override string Tooltip => "刷新当前表的所有跨项目数据引用";

	protected override Func<Task> ClickedTask => async () =>
	{
		try
		{
			var project = Leqisoft.Model.Project.Current;
			var table = Program.MainForm?.TableEditor?.Table;
			if (project == null || table == null) return;

			var manager = new Leqisoft.Model.CrossProjectDataRefManager(project);
			var results = await manager.ExecuteAll(table.Id);
			int success = results.Results.Count(r => r.Success);
			int failed = results.Results.Count(r => !r.Success);
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, $"刷新完成：成功 {success} 个，失败 {failed} 个");
		}
		catch (Exception ex)
		{
			ex.Log();
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, $"刷新跨项目引用失败: {ex.Message}");
		}
	};
}