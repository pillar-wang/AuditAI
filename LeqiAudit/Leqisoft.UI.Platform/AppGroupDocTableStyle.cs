﻿﻿﻿using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppGroupDocTableStyle : AppCommandGroup
{
	public override string Text => "表格样式";

	public override Image Image => Resources.TableStyle0;

	public AppGroupDocTableStyle()
	{
		base.Commands.Add(AppCommands.TableStyle);
		base.Commands.Add(AppCommands.TableStyleCustom);
		base.Commands.Add(AppCommands.BatchApplyTableStyle);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		base.Visible = state.ViewKind == MainFormView.Document;
	}
}
