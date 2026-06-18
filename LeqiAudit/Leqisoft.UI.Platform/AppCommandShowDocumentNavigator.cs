﻿﻿﻿﻿﻿﻿using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandShowDocumentNavigator : AppCommandToggleButton
{
	public override string Text => "文档结构图";

	public override Image LargeIcon => Resources.DocumentStructure ?? CreateDefaultIcon();

	public override Image SmallIcon => Resources.DocumentStructure ?? CreateSmallIcon();

	private static Image CreateDefaultIcon()
	{
		Bitmap bmp = new Bitmap(32, 32);
		using (Graphics g = Graphics.FromImage(bmp))
		{
			g.Clear(Color.Transparent);
			Pen pen = new Pen(Color.Gray, 2);
			g.DrawRectangle(pen, 2, 2, 28, 28);
			pen.Color = Color.Blue;
			g.DrawLine(pen, 6, 8, 26, 8);
			g.DrawLine(pen, 10, 14, 22, 14);
			g.DrawLine(pen, 14, 20, 18, 20);
			g.DrawLine(pen, 10, 14, 10, 26);
			g.DrawLine(pen, 22, 14, 22, 26);
		}
		return bmp;
	}

	private static Image CreateSmallIcon()
	{
		Bitmap bmp = new Bitmap(16, 16);
		using (Graphics g = Graphics.FromImage(bmp))
		{
			g.Clear(Color.Transparent);
			Pen pen = new Pen(Color.Gray, 1);
			g.DrawRectangle(pen, 1, 1, 14, 14);
			pen.Color = Color.Blue;
			g.DrawLine(pen, 3, 4, 13, 4);
			g.DrawLine(pen, 5, 7, 11, 7);
			g.DrawLine(pen, 7, 10, 9, 10);
			g.DrawLine(pen, 5, 7, 5, 13);
			g.DrawLine(pen, 11, 7, 11, 13);
		}
		return bmp;
	}

	public override void GenerateRibbonItem()
	{
		base.GenerateRibbonItem();
		base.IsPressed = true;
	}

	protected override void Pressed()
	{
		Program.MainForm.CurrentDocumentEditor.ShowStructure();
	}

	protected override void Unpressed()
	{
		Program.MainForm.CurrentDocumentEditor.HideStructure();
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		Visible = state.ViewKind == MainFormView.Document || state.ViewKind == MainFormView.DocumentPreview;
	}
}
