﻿﻿﻿using System;
using System.Drawing;
using System.Threading.Tasks;
using Leqisoft.UI.Controls.Properties;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandCustomFillConfig : AppCommandButton
{
	public override string Text => "填充配置";

	public override Image LargeIcon => Leqisoft.UI.Platform.Properties.Resources.TableCollect;

	protected override Func<Task> ClickedTask => async delegate
	{
		Program.MainForm.ShowCustomFillConfig();
	};

	protected override string Tooltip => TipResource.采数填充按钮;
}