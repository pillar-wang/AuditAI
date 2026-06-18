﻿﻿﻿﻿﻿using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandUndo : AppCommandButton
{
	public override string Text => "撤销";

	public override Image LargeIcon => Resources.Undo;

	protected override void Clicked()
	{
		Program.MainForm.Undo();
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		// 在表格视图和 Word 文档视图下启用撤销，具体 Enabled 状态由 UpdateUndoRedoButtonState 控制
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
