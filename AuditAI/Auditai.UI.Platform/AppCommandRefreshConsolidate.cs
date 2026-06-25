﻿﻿﻿using System;
using System.Drawing;
using System.Threading.Tasks;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

/// <summary>
/// 刷新合并报表数据按钮
/// </summary>
public class AppCommandRefreshConsolidate : AppCommandButton
{
	public override string Text => "刷新合并报表";

	public override Image LargeIcon => Auditai.UI.Platform.Properties.Resources.ConsolidateStatements;

	protected override Func<Task> ClickedTask => async delegate
	{
		await Program.MainForm.TableEditor.RefreshConsolidate();
	};

	protected override string Tooltip => "从来源单体重新读取最新数据，刷新当前表的合并报表结果";
}