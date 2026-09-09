﻿using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using C1.Win.C1Ribbon;

namespace Auditai.UI.Platform;

// 账务数据独立非模态窗口：承载 MultiLedgerViewer.View，与主窗口同屏对照使用
public class LedgerWindow : Form
{
	// 真关闭标志：false 时点 X 仅隐藏窗口（账套保持打开），true 时允许真正关闭销毁
	private bool _realClose;

	// 顶部 Ribbon 工具栏：主窗口的"账务数据"标签（含全部命令组）随窗口显隐迁移到这里
	public C1Ribbon LedgerRibbon { get; }

	// 窗口被隐藏（用户点 X）时触发，主窗口借此隐藏关联账套提示
	public event EventHandler WindowHidden;

	// 主窗口引用：Ctrl+Q 转发切换用
	private readonly MainForm _mainForm;

	public LedgerWindow(MainForm owner)
	{
		_mainForm = owner;
		// 主窗口是普通类，其 Form 为 View 属性（C1RibbonForm），窗口外观属性从其上获取
		Form form = owner?.View;
		// 顶部 Ribbon：与主窗口一致隐藏应用菜单和 QAT，样式跟随主窗口 Custom 主题
		LedgerRibbon = new C1Ribbon
		{
			Dock = DockStyle.Top,
			AllowContextMenu = false,
			VisualStyle = VisualStyle.Custom
		};
		LedgerRibbon.ApplicationMenu.Visible = false;
		LedgerRibbon.Qat.MenuVisible = false;
		Text = "账务数据";
		Icon = form?.Icon;
		Size = new Size(1100, 720);
		MinimumSize = new Size(760, 480);
		// KeyPreview：焦点在账务窗口内时 Ctrl+Q 也能切换窗口显隐（与主窗口快捷键一致）
		KeyPreview = true;
		// 手动定位：在主窗口位置基础上偏移 (120, 80)，并钳制在虚拟屏幕范围内避免开出屏幕外
		StartPosition = FormStartPosition.Manual;
		int x = (form?.Location.X ?? 0) + 120;
		int y = (form?.Location.Y ?? 0) + 80;
		Rectangle virtualScreen = SystemInformation.VirtualScreen;
		if (x + Width > virtualScreen.Right)
		{
			x = virtualScreen.Right - Width;
		}
		if (y + Height > virtualScreen.Bottom)
		{
			y = virtualScreen.Bottom - Height;
		}
		if (x < virtualScreen.Left)
		{
			x = virtualScreen.Left;
		}
		if (y < virtualScreen.Top)
		{
			y = virtualScreen.Top;
		}
		Location = new Point(x, y);
		ShowInTaskbar = true;
		MinimizeBox = true;
		MaximizeBox = true;
		Font = form?.Font;
	}

	// 焦点在账务窗口内时响应 Ctrl+Q，转发给主窗口切换账务窗口显隐
	protected override void OnKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);
		if (e.Control && e.KeyCode == Keys.Q)
		{
			e.Handled = true;
			_mainForm?.ToggleLedgerWindow();
		}
	}

	// 挂载账套查看器整体控件树（由主窗口 Reparent 到本窗口）
	public void AttachViewer(Control viewer)
	{
		viewer.Dock = DockStyle.Fill;
		Controls.Add(viewer);
		// Ribbon 最后加入（后加入者先布局）：占据顶部区域，viewer 填充其余空间
		Controls.Add(LedgerRibbon);
	}

	// 点 X 关闭时仅隐藏不销毁，已打开账套保持在内存中；程序退出走 RealClose 真关闭
	protected override void OnFormClosing(FormClosingEventArgs e)
	{
		if (!_realClose)
		{
			e.Cancel = true;
			Hide();
			WindowHidden?.Invoke(this, EventArgs.Empty);
			return;
		}
		base.OnFormClosing(e);
	}

	// 真关闭（主窗口退出时调用），绕过隐藏逻辑
	public void RealClose()
	{
		_realClose = true;
		Close();
	}

	// 标题联动：无打开账套时显示"账务数据"，有账套时附加账套文件名
	public void UpdateTitle(string ledgerFilePath)
	{
		if (string.IsNullOrEmpty(ledgerFilePath))
		{
			Text = "账务数据";
		}
		else
		{
			Text = "账务数据 - " + Path.GetFileNameWithoutExtension(ledgerFilePath);
		}
	}
}
