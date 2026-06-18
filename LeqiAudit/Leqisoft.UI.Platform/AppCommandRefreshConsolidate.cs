﻿﻿﻿﻿﻿using System;
using System.Drawing;
using System.Threading.Tasks;
using Leqisoft.UI.Controls.Properties;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

/// <summary>
/// 刷新跨项目数据按钮
/// </summary>
public class AppCommandRefreshConsolidate : AppCommandButton
{
	public override string Text => "刷新跨项目数据";

	public override Image LargeIcon => Leqisoft.UI.Platform.Properties.Resources.ConsolidateStatements;

	protected override Func<Task> ClickedTask => async delegate
	{
		await Program.MainForm.TableEditor.RefreshConsolidate();
	};

	protected override string Tooltip => "从来源项目重新读取最新数据，刷新当前表的跨项目汇总结果";
}