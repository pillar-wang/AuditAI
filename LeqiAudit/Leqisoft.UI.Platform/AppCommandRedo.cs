﻿﻿﻿﻿﻿using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandRedo : AppCommandButton
{
	public override string Text => "重做";

	public override Image LargeIcon => Resources.Redo;

	protected override void Clicked()
	{
		Program.MainForm.Redo();
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		// 在表格视图和 Word 文档视图下启用重做，具体 Enabled 状态由 UpdateUndoRedoButtonState 控制
		if (state.ViewKind == MainFormView.Table || state.ViewKind == MainFormView.Document)
		{
			Program.MainForm.UpdateUndoRedoButtonState();
		}
		else
		{
			Enabled = false;
		}
	}
}
