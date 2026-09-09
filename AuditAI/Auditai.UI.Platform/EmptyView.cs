﻿using System;
using System.Drawing;
using System.Windows.Forms;
using C1.Win.C1Input;
using C1.Win.C1SplitContainer;
using C1.Win.C1SuperTooltip;
using Auditai.PlatformResource;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public static class EmptyView
{
	private static C1SuperLabel _sl;

	public static C1SplitContainer View { get; }

	static EmptyView()
	{
		View = new C1SplitContainer();
		View.Size = new Size(500, 400);
		View.Dock = DockStyle.Fill;
		C1SplitterPanel c1SplitterPanel = new C1SplitterPanel
		{
			SizeRatio = 100.0,
			BackColor = Color.FromArgb(247, 250, 253),
			DoubleBuffered = true
		};
		_sl = new C1SuperLabel
		{
			Dock = DockStyle.Top,
			Height = 210,
			BackColor = Color.Transparent
		};
		c1SplitterPanel.Controls.Add(_sl);
		View.Panels.Add(c1SplitterPanel);
	}

	public static void SetWelcome()
	{
		_sl.Text = GetHtml();
	}

	private static string GetHtml()
	{
		return "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\">\r\n<html>\r\n<head><title></title></head>\r\n<body>\r\n<span style=\"height:80px;\"></span>\r\n<p align = 'center' style = \"color:#909090;font: bold 15px '微软雅黑'\" > AuditAI 提供全程性服务，为您在使用上保驾护航 </ p >\r\n</body>\r\n</html>";
	}
}
