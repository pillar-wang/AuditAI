﻿﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Auditai.UI.Controls;

namespace Auditai.UI.Platform;

/// <summary>
/// 命令模型（对应原 RibbonButton，供 TopCommandBar 使用）。
/// 同名命令跨模块共享同一实例：SetCommandEnabled/Visible/Text 对所有同名条目同时生效，
/// 与原 C1Ribbon 中共享 RibbonButton 的行为一致。
/// </summary>
public class SimpleCommand
{
	internal Action Changed;

	internal EventHandler ClickHandler;

	private string _text;

	private Image _image;

	private string _iconName;

	private bool _enabled = true;

	private bool _visible = true;

	public SimpleCommand(string name)
	{
		Name = name;
	}

	public string Name { get; }

	public string Text
	{
		get
		{
			return _text;
		}
		set
		{
			_text = value;
			Changed?.Invoke();
		}
	}

	public Image Image
	{
		get
		{
			return _image;
		}
		set
		{
			_image = value;
			Changed?.Invoke();
		}
	}

	/// <summary>字体图标语义名（非空时优先生效，随主题色矢量渲染）。</summary>
	public string IconName
	{
		get
		{
			return _iconName;
		}
		set
		{
			_iconName = value;
			Changed?.Invoke();
		}
	}

	public bool Enabled
	{
		get
		{
			return _enabled;
		}
		set
		{
			_enabled = value;
			Changed?.Invoke();
		}
	}

	public bool Visible
	{
		get
		{
			return _visible;
		}
		set
		{
			_visible = value;
			Changed?.Invoke();
		}
	}
}

/// <summary>
/// 自绘垂直左侧模块导航栏：仅承载模块大项（图标+文字、选中高亮、悬浮过渡动画）。
/// 明细命令由 TopCommandBar（顶部水平命令栏）承载。
/// </summary>
public class SideCommandBar : UserControl
{
	private sealed class ModuleInfo
	{
		public string Name;

		public string Text;

		public Image Icon;

		/// <summary>字体图标语义名（非空时优先生效）。</summary>
		public string IconName;

		public float Hover;
	}

	private const int ModuleItemHeightBase = 52;

	private const int ModuleIconSizeBase = 40;

	private const int BarWidth = 200;

	private const float AnimStep = 0.28f;

	/// <summary>GDI 文本渲染格式（TextRenderer 走系统 GDI 通道，雅黑 hinting 下笔画更饱满清晰；左对齐与原 DrawString Near 一致）。</summary>
	private const TextFormatFlags TextFlags = TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix;

	/// <summary>按当前 DPI 缩放 96dpi 基准像素值（app.manifest 为 PerMonitorV2，需随 DPI 缩放自绘尺寸）。</summary>
	private int S(int value)
	{
		return (int)Math.Round((float)value * (_dpi / 96f));
	}

	private int ModuleItemHeight => S(ModuleItemHeightBase);

	private int ModuleIconSize => S(ModuleIconSizeBase);

	private readonly List<ModuleInfo> _modules = new List<ModuleInfo>();

	private readonly List<Tuple<ModuleInfo, Rectangle>> _moduleRects = new List<Tuple<ModuleInfo, Rectangle>>();

	private readonly Timer _animTimer;

	private Font _moduleFont;

	/// <summary>当前 DPI（句柄创建后更新，默认 96）。</summary>
	private int _dpi = 96;

	private ModuleInfo _selectedModule;

	private ModuleInfo _hoverModule;

	private bool _moduleNavVisible = true;

	private Color _backColor = AuditTheme.SurfaceMuted;

	private Color _hoverColor = Color.FromArgb(239, 246, 255);

	private Color _selectedColor = Color.FromArgb(59, 130, 246);

	private Color _textColor = Color.FromArgb(71, 85, 105);

	private Color _separatorColor = Color.FromArgb(229, 231, 235);

	private ModuleInfo CurrentModule => _selectedModule ?? _modules.FirstOrDefault();

	public event Action<string> ModuleSelected;

	public SideCommandBar()
	{
		SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
		Dock = DockStyle.Left;
		Width = BarWidth;
		BackColor = _backColor;
		_animTimer = new Timer
		{
			Interval = 16,
			Enabled = false
		};
		_animTimer.Tick += AnimTick;
		RecreateFonts();
		DpiChangedAfterParent += delegate
		{
			UpdateDpi();
		};
	}

	/// <summary>按当前 DPI 重建像素基准字体（10pt≈13px @96dpi）。</summary>
	private void RecreateFonts()
	{
		_moduleFont?.Dispose();
		_moduleFont = new Font("微软雅黑", S(13), GraphicsUnit.Pixel);
	}

	private void UpdateDpi()
	{
		if (_dpi == DeviceDpi)
		{
			return;
		}
		_dpi = DeviceDpi;
		// 跨显示器拖动时按新 DPI 提升图标位图生成分辨率（取到过最大，保证任意屏位图足够清晰）
		IconLibrary.DpiScale = Math.Max(IconLibrary.DpiScale, DeviceDpi / 96f);
		int width = S(BarWidth);
		if (Width != width)
		{
			Width = width;
		}
		RecreateFonts();
		Invalidate();
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);
		UpdateDpi();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_animTimer.Dispose();
			_moduleFont?.Dispose();
		}
		base.Dispose(disposing);
	}

	/// <summary>注册顶部模块导航项（如 TAB_PROJECT）。</summary>
	public void RegisterModule(string name, string text, Image icon)
	{
		RegisterModule(name, text, icon, null);
	}

	/// <summary>注册模块导航项（字体图标语义名版本，矢量渲染不随 DPI 模糊）。</summary>
	public void RegisterModule(string name, string text, string iconName)
	{
		RegisterModule(name, text, null, iconName);
	}

	private void RegisterModule(string name, string text, Image icon, string iconName)
	{
		if (_modules.Any((ModuleInfo m) => m.Name == name))
		{
			return;
		}
		_modules.Add(new ModuleInfo
		{
			Name = name,
			Text = text,
			Icon = icon,
			IconName = iconName
		});
		if (_selectedModule == null)
		{
			_selectedModule = _modules[0];
		}
		Invalidate();
	}

	/// <summary>编程切换模块（仅在模块变化时触发 ModuleSelected，与原 RibbonTab.Selected 语义一致）。</summary>
	public void SelectModule(string name)
	{
		if (_selectedModule != null && _selectedModule.Name == name)
		{
			return;
		}
		SetSelectedModule(name);
		ModuleSelected?.Invoke(name);
	}

	/// <summary>静默设置选中模块（不触发 ModuleSelected，仅同步视觉状态）。</summary>
	public void SetSelectedModule(string name)
	{
		ModuleInfo module = _modules.FirstOrDefault((ModuleInfo m) => m.Name == name);
		if (module == null)
		{
			return;
		}
		_selectedModule = module;
		_hoverModule = null;
		foreach (ModuleInfo m in _modules)
		{
			m.Hover = 0f;
		}
		Invalidate();
	}

	/// <summary>隐藏模块导航区（AppEditionGeneral 下与原 HideTabHeaderRow 行为对齐，整个导航栏收起）。</summary>
	public void SetModuleNavVisible(bool visible)
	{
		if (_moduleNavVisible == visible)
		{
			return;
		}
		_moduleNavVisible = visible;
		Visible = visible;
	}

	/// <summary>主题配色注入：背景/悬浮/选中/文字色（悬浮与选中色可带透明度，绘制时与背景混合）。</summary>
	public void ApplyTheme(Color back, Color hover, Color selected, Color text)
	{
		_backColor = back;
		_hoverColor = hover;
		_selectedColor = selected;
		_textColor = text;
		_separatorColor = Color.FromArgb(46, text.R, text.G, text.B);
		BackColor = back;
		Invalidate();
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		Graphics g = e.Graphics;
		g.SmoothingMode = SmoothingMode.AntiAlias;
		_moduleRects.Clear();
		g.Clear(_backColor);
		int y = 0;
		foreach (ModuleInfo m in _modules)
		{
			Rectangle rect = new Rectangle(S(6), y + S(2), Width - S(12), ModuleItemHeight - S(4));
			DrawItemBack(g, rect, m.Hover, m == _selectedModule);
			Rectangle iconRect = new Rectangle(rect.X + S(10), y + (ModuleItemHeight - ModuleIconSize) / 2, ModuleIconSize, ModuleIconSize);
			if (!string.IsNullOrEmpty(m.IconName))
			{
				DrawIconName(g, m.IconName, iconRect, m == _selectedModule);
			}
			else
			{
				DrawIcon(g, m.Icon, iconRect);
			}
			Color textColor = (m == _selectedModule) ? Color.White : _textColor;
			DrawText(g, m.Text, _moduleFont, new Rectangle(rect.X + S(10) + ModuleIconSize + S(10), y, Width - rect.X * 2 - S(10) - ModuleIconSize - S(10), ModuleItemHeight), textColor);
			_moduleRects.Add(Tuple.Create(m, rect));
			y += ModuleItemHeight;
		}
		using Pen pen = new Pen(_separatorColor);
		g.DrawLine(pen, Width - 1, 0, Width - 1, Height);
		if (!Enabled)
		{
			using SolidBrush veil = new SolidBrush(Color.FromArgb(96, Color.White));
			g.FillRectangle(veil, ClientRectangle);
		}
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);
		ModuleInfo hoverModule = HitTest(e.Location) as ModuleInfo;
		if (hoverModule != _hoverModule)
		{
			_hoverModule = hoverModule;
			Cursor = ((hoverModule != null) ? Cursors.Hand : Cursors.Default);
			StartAnim();
		}
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		base.OnMouseLeave(e);
		if (_hoverModule != null)
		{
			_hoverModule = null;
			Cursor = Cursors.Default;
			StartAnim();
		}
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		base.OnMouseUp(e);
		if (e.Button != MouseButtons.Left)
		{
			return;
		}
		if (HitTest(e.Location) is ModuleInfo module && module != _selectedModule)
		{
			SelectModule(module.Name);
		}
	}

	private ModuleInfo HitTest(Point p)
	{
		foreach (Tuple<ModuleInfo, Rectangle> moduleRect in _moduleRects)
		{
			if (moduleRect.Item2.Contains(p))
			{
				return moduleRect.Item1;
			}
		}
		return null;
	}

	private void StartAnim()
	{
		if (!_animTimer.Enabled)
		{
			_animTimer.Start();
		}
	}

	private void AnimTick(object sender, EventArgs e)
	{
		bool animating = false;
		foreach (ModuleInfo m in _modules)
		{
			animating |= StepHover(m, m == _hoverModule);
		}
		if (animating)
		{
			Invalidate();
		}
		else
		{
			_animTimer.Stop();
		}
	}

	private static bool StepHover(ModuleInfo item, bool hovered)
	{
		float target = (hovered ? 1f : 0f);
		float progress = item.Hover;
		if (progress == target)
		{
			return false;
		}
		progress += (target - progress) * AnimStep;
		if (Math.Abs(target - progress) < 0.02f)
		{
			progress = target;
		}
		item.Hover = progress;
		return true;
	}

	private void DrawItemBack(Graphics g, Rectangle rect, float hover, bool selected)
	{
		if (selected)
		{
			using GraphicsPath path = CreateRounded(rect, S(4));
			using SolidBrush brush = new SolidBrush(_selectedColor);
			g.FillPath(brush, path);
		}
		else if (hover > 0.01f)
		{
			using GraphicsPath path2 = CreateRounded(rect, S(4));
			int alpha = (int)((float)_hoverColor.A * hover);
			using SolidBrush brush2 = new SolidBrush(Color.FromArgb(alpha, _hoverColor.R, _hoverColor.G, _hoverColor.B));
			g.FillPath(brush2, path2);
		}
	}

	private void DrawIcon(Graphics g, Image image, Rectangle rect)
	{
		if (image == null)
		{
			return;
		}
		InterpolationMode mode = g.InterpolationMode;
		g.InterpolationMode = InterpolationMode.HighQualityBicubic;
		g.DrawImage(image, rect);
		g.InterpolationMode = mode;
	}

	private void DrawIconName(Graphics g, string iconName, Rectangle rect, bool selected)
	{
		// 选中项用白色（蓝色背景上），未选按图标语义色（IconPalette 13 色板）；实心渲染
		Color color = selected ? Color.White : IconPalette.Get(iconName);
		IconLibrary.DrawGlyph(g, iconName, rect, color, IconLibrary.StyleFill);
	}

	private void DrawText(Graphics g, string text, Font font, Rectangle rect, Color color)
	{
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		TextRenderer.DrawText(g, text, font, rect, color, TextFlags);
	}

	private static GraphicsPath CreateRounded(Rectangle rect, int radius)
	{
		GraphicsPath path = new GraphicsPath();
		int d = radius * 2;
		path.AddArc(rect.X, rect.Y, d, d, 180f, 90f);
		path.AddArc(rect.Right - d, rect.Y, d, d, 270f, 90f);
		path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0f, 90f);
		path.AddArc(rect.X, rect.Bottom - d, d, d, 90f, 90f);
		path.CloseFigure();
		return path;
	}
}
