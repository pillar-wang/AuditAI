﻿using System;
using System.Drawing;
using System.Windows.Forms;
using C1.Win.C1Command;
using C1.Win.C1SplitContainer;

namespace Auditai.UI.LedgerView;

public static class ComponentFactory
{
	public static C1SplitContainer BuildSidebar(Control grid, C1ToolBar toolBar, out C1SplitterPanel pnlSidebar)
	{
		// C1ToolBar (C1Command 2.x) 按 96dpi 位图绘制，PerMonitorV2 下文字发虚；
		// 这里只把它当作"命令源"，真正显示用原生 ToolStrip（系统 GDI/ClearType，任意 DPI 文字清晰）。
		ToolStrip nativeToolBar = BuildNativeVerticalToolBar(toolBar);
		pnlSidebar = new C1SplitterPanel
		{
			Collapsible = false,
			KeepRelativeSize = false,
			Width = nativeToolBar.Width + 6,
			Resizable = false,
			Dock = PanelDockStyle.Right,
			BorderWidth = 0,
			BackColor = Color.White
		};
		pnlSidebar.Controls.Add(nativeToolBar);
		C1SplitterPanel pnlContent = new C1SplitterPanel
		{
			Collapsible = false,
			KeepRelativeSize = false,
			SizeRatio = 100.0,
			Resizable = false,
			Dock = PanelDockStyle.Left,
			BorderWidth = 0
		};
		pnlContent.Controls.Add(grid);
		C1SplitContainer container = new C1SplitContainer
		{
			Dock = DockStyle.Fill,
			BorderWidth = 0,
			FixedLineWidth = 0,
			SplitterWidth = 0,
			BackColor = Color.White
		};
		container.Panels.Add(pnlSidebar);
		container.Panels.Add(pnlContent);
		return container;
	}

	/// <summary>
	/// 把 C1ToolBar 的命令链接镜像成原生垂直 ToolStrip。
	/// 点击经 <see cref="C1Command.PerformClick"/> 走原命令分发，功能零改动；
	/// 构建时执行 StateQuery 让 CommandStateQuery 处理器刷新 Text/Visible/Enabled/Checked。
	/// 按钮为正方形：边长按最长文字实测宽度 + 边距计算，保证任意 DPI 下文字完整显示。
	/// </summary>
	private static ToolStrip BuildNativeVerticalToolBar(C1ToolBar source)
	{
		Font font = new Font("微软雅黑", 9f);
		int minSide = 84;
		int squareSide = minSide;
		// 第一遍：StateQuery 刷新状态 + 实测最长文字宽度
		foreach (C1CommandLink link in source.CommandLinks)
		{
			C1Command cmd = link.Command;
			if (cmd == null)
			{
				continue;
			}
			try
			{
				cmd.StateQuery();
			}
			catch (Exception)
			{
			}
			if (!cmd.Visible)
			{
				continue;
			}
			int w = TextRenderer.MeasureText(cmd.Text ?? string.Empty, font).Width + 18;
			if (w > squareSide)
			{
				squareSide = w;
			}
		}
		ToolStrip ts = new ToolStrip
		{
			LayoutStyle = ToolStripLayoutStyle.VerticalStackWithOverflow,
			RenderMode = ToolStripRenderMode.System,
			ShowItemToolTips = false,
			GripStyle = ToolStripGripStyle.Hidden,
			AutoSize = false,
			Width = squareSide + 8,
			Font = font,
			ForeColor = Color.FromArgb(17, 24, 39),
			ImageScalingSize = new Size(24, 24),
			Padding = new Padding(0),
			Dock = DockStyle.Fill
		};
		foreach (C1CommandLink link in source.CommandLinks)
		{
			C1Command cmd = link.Command;
			if (cmd == null)
			{
				continue;
			}
			try
			{
				cmd.StateQuery();
			}
			catch (Exception)
			{
			}
			if (!cmd.Visible)
			{
				continue;
			}
			if (link.Delimiter)
			{
				ts.Items.Add(new ToolStripSeparator());
			}
			ToolStripButton btn = new ToolStripButton
			{
				Text = cmd.Text,
				Image = cmd.Image,
				ImageTransparentColor = Color.Magenta,
				TextImageRelation = TextImageRelation.ImageAboveText,
				AutoSize = false,
				Size = new Size(squareSide, squareSide),
				Margin = new Padding(2, 3, 2, 3),
				Padding = new Padding(2, 2, 2, 2),
				Enabled = cmd.Enabled,
				Checked = cmd.Checked,
				CheckOnClick = cmd.CheckAutoToggle,
				ToolTipText = cmd.ToolTipText
			};
			C1Command captured = cmd;
			ToolStripButton capturedBtn = btn;
			btn.Click += delegate
			{
				try
				{
					captured.PerformClick();
				}
				catch (Exception)
				{
				}
				// PerformClick 内部已按 AutoToggle 切换 Checked，回写显示使其即时生效
				capturedBtn.Checked = captured.Checked;
			};
			ts.Items.Add(btn);
		}
		return ts;
	}

	/// <summary>
	/// 在主题应用后恢复垂直工具栏的文字显示。
	/// 在每个编辑器的 SetTheme() 方法中调用。
	/// </summary>
	public static void RestoreSidebarToolBar(C1SplitterPanel pnlSidebar)
	{
		if (pnlSidebar == null) return;
		foreach (Control control in pnlSidebar.Controls)
		{
			if (control is C1ToolBar toolBar && !toolBar.Horizontal)
			{
				toolBar.ButtonLookVert = ButtonLookFlags.TextAndImage;
				foreach (C1CommandLink link in toolBar.CommandLinks)
				{
					link.ButtonLook = ButtonLookFlags.TextAndImage;
				}
				toolBar.Invalidate();
			}
		}
	}
}
