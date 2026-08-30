using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;

namespace Auditai.UI.Platform;

/// <summary>
/// 顶部水平命令栏：显示当前模块（左侧 SideCommandBar 选中的大项）的明细命令分组。
/// 组标题在按钮下方居中（类似 Ribbon 组标题），组间竖线分隔；命令按注册顺序水平排列，
/// 超出宽度时可用滚轮横向滚动。命令模型与 SimpleCommand 共享（同名命令跨模块状态同步）。
/// </summary>
public class TopCommandBar : UserControl
{
	private sealed class BarGroup
	{
		public string Name;

		public string Text;

		public List<BtnInfo> Items = new List<BtnInfo>();
	}

	private sealed class ModuleModel
	{
		public string Name;

		public List<BarGroup> Groups = new List<BarGroup>();
	}

	private sealed class BtnInfo
	{
		public SimpleCommand Command;

		public float Hover;
	}

	private const int BarHeightBase = 96;

	private const int ButtonHeightBase = 52;

	private const int ButtonMinWidthBase = 72;

	private const int IconSizeBase = 40;

	private const int GroupCaptionHeightBase = 18;

	private const float AnimStep = 0.28f;

	/// <summary>按当前 DPI 缩放 96dpi 基准像素值。</summary>
	private int S(int value)
	{
		return (int)Math.Round((float)value * (_dpi / 96f));
	}

	private int BarHeight => S(BarHeightBase);

	private int ButtonHeight => S(ButtonHeightBase);

	private int ButtonMinWidth => S(ButtonMinWidthBase);

	private int IconSize => S(IconSizeBase);

	private int GroupCaptionHeight => S(GroupCaptionHeightBase);

	private readonly List<ModuleModel> _modules = new List<ModuleModel>();

	private readonly Dictionary<string, SimpleCommand> _commands = new Dictionary<string, SimpleCommand>(StringComparer.OrdinalIgnoreCase);

	private readonly List<Tuple<BtnInfo, Rectangle>> _btnRects = new List<Tuple<BtnInfo, Rectangle>>();

	private readonly Timer _animTimer;

	private Font _textFont;

	private Font _captionFont;

	/// <summary>当前 DPI（句柄创建后更新，默认 96）。</summary>
	private int _dpi = 96;

	private string _currentModule;

	private BtnInfo _hoverBtn;

	private float _scrollX;

	private int _contentWidth;

	private Color _backColor = Color.FromArgb(232, 243, 251);

	private Color _hoverColor = Color.FromArgb(214, 234, 247);

	private Color _textColor = Color.FromArgb(52, 64, 84);

	private Color _captionColor = Color.FromArgb(122, 138, 158);

	private Color _disabledTextColor = Color.FromArgb(158, 170, 184);

	private Color _separatorColor = Color.FromArgb(204, 222, 236);

	private ModuleModel CurrentModuleModel => _modules.FirstOrDefault((ModuleModel m) => m.Name == _currentModule) ?? _modules.FirstOrDefault();

	public TopCommandBar()
	{
		SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
		Dock = DockStyle.Top;
		Height = BarHeight;
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

	/// <summary>按当前 DPI 重建像素基准字体（9pt≈12px、8pt≈11px @96dpi）。</summary>
	private void RecreateFonts()
	{
		_textFont?.Dispose();
		_captionFont?.Dispose();
		_textFont = new Font("Noto Sans SC", S(12), GraphicsUnit.Pixel);
		_captionFont = new Font("Noto Sans SC", S(11), GraphicsUnit.Pixel);
	}

	private void UpdateDpi()
	{
		if (_dpi == DeviceDpi)
		{
			return;
		}
		_dpi = DeviceDpi;
		int height = BarHeight;
		if (Height != height)
		{
			Height = height;
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
			_textFont?.Dispose();
			_captionFont?.Dispose();
		}
		base.Dispose(disposing);
	}

	/// <summary>
	/// 向指定模块的命令分组添加命令条目。同名命令共享同一 SimpleCommand 实例（状态同步）。
	/// </summary>
	public void AddCommand(string moduleName, string groupName, string cmdName, string text, Image icon, EventHandler onClick, string groupText = null)
	{
		ModuleModel module = _modules.FirstOrDefault((ModuleModel m) => m.Name == moduleName);
		if (module == null)
		{
			module = new ModuleModel
			{
				Name = moduleName
			};
			_modules.Add(module);
		}
		if (!_commands.TryGetValue(cmdName, out var command))
		{
			command = new SimpleCommand(cmdName)
			{
				Text = text,
				Image = icon,
				ClickHandler = onClick
			};
			command.Changed = Invalidate;
			_commands[cmdName] = command;
		}
		BarGroup group = module.Groups.FirstOrDefault((BarGroup g) => g.Name == groupName);
		if (group == null)
		{
			group = new BarGroup
			{
				Name = groupName,
				Text = groupText ?? groupName
			};
			module.Groups.Add(group);
		}
		group.Items.Add(new BtnInfo
		{
			Command = command
		});
		Invalidate();
	}

	public SimpleCommand GetCommand(string name)
	{
		_commands.TryGetValue(name, out var command);
		return command;
	}

	public void SetCommandEnabled(string cmdName, bool enabled)
	{
		if (_commands.TryGetValue(cmdName, out var command))
		{
			command.Enabled = enabled;
		}
	}

	public void SetCommandVisible(string cmdName, bool visible)
	{
		if (_commands.TryGetValue(cmdName, out var command))
		{
			command.Visible = visible;
		}
	}

	public void SetCommandText(string cmdName, string text)
	{
		if (_commands.TryGetValue(cmdName, out var command))
		{
			command.Text = text;
		}
	}

	/// <summary>同步当前模块（由 SideCommandBar.ModuleSelected 驱动）。</summary>
	public void SetModule(string name)
	{
		if (_currentModule == name)
		{
			return;
		}
		_currentModule = name;
		_scrollX = 0f;
		_hoverBtn = null;
		Invalidate();
	}

	/// <summary>主题配色注入：背景/悬浮/文字色。</summary>
	public void ApplyTheme(Color back, Color hover, Color selected, Color text)
	{
		_backColor = back;
		_hoverColor = hover;
		_textColor = text;
		_captionColor = Color.FromArgb(150, text.R, text.G, text.B);
		_disabledTextColor = Color.FromArgb(125, text.R, text.G, text.B);
		_separatorColor = Color.FromArgb(46, text.R, text.G, text.B);
		BackColor = back;
		Invalidate();
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		Graphics g = e.Graphics;
		g.SmoothingMode = SmoothingMode.AntiAlias;
		g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
		_btnRects.Clear();
		g.Clear(_backColor);
		ModuleModel module = CurrentModuleModel;
		if (module != null)
		{
			List<BarGroup> visibleGroups = module.Groups.Where((BarGroup grp) => grp.Items.Any((BtnInfo i) => i.Command.Visible)).ToList();
			_contentWidth = MeasureContentWidth(visibleGroups);
			int maxScroll = Math.Max(0, _contentWidth - Width);
			if (_scrollX > (float)maxScroll)
			{
				_scrollX = maxScroll;
			}
			if (_scrollX < 0f)
			{
				_scrollX = 0f;
			}
			int scroll = (int)_scrollX;
			int btnAreaHeight = Height - GroupCaptionHeight;
			int x = S(8) - scroll;
			foreach (BarGroup group in visibleGroups)
			{
				List<BtnInfo> items = group.Items.Where((BtnInfo i) => i.Command.Visible).ToList();
				if (items.Count == 0)
				{
					continue;
				}
				int groupLeft = x;
				foreach (BtnInfo item in items)
				{
					SimpleCommand command = item.Command;
					int btnWidth = Math.Max(ButtonMinWidth, MeasureTextWidth(g, command.Text, _textFont) + S(10));
					Rectangle rect = new Rectangle(x, S(2), btnWidth, btnAreaHeight - S(4));
					if (rect.Right > 0)
					{
						DrawBtnBack(g, rect, item.Hover);
						Rectangle iconRect = new Rectangle(rect.X + (rect.Width - IconSize) / 2, rect.Y + S(5), IconSize, IconSize);
						DrawIcon(g, command.Image, iconRect, command.Enabled);
						DrawText(g, command.Text, _textFont, new Rectangle(rect.X + S(2), rect.Y + S(5) + IconSize + S(2), rect.Width - S(4), rect.Height - S(5) - IconSize - S(6)), command.Enabled ? _textColor : _disabledTextColor);
						_btnRects.Add(Tuple.Create(item, rect));
					}
					x += btnWidth;
				}
				int captionX = (groupLeft + x - S(4)) / 2 - S(60);
				DrawText(g, group.Text, _captionFont, new Rectangle(captionX, btnAreaHeight, S(120), GroupCaptionHeight), _captionColor);
				x += S(8);
				using Pen pen = new Pen(_separatorColor);
				g.DrawLine(pen, x, S(8), x, btnAreaHeight - S(8));
				x += S(9);
			}
			if (maxScroll > 0)
			{
				int thumbWidth = Math.Max(S(24), Width * Width / _contentWidth);
				int thumbX = (int)((float)(Width - thumbWidth) * (_scrollX / maxScroll));
				using SolidBrush brush = new SolidBrush(Color.FromArgb(64, _textColor.R, _textColor.G, _textColor.B));
				g.FillRectangle(brush, thumbX, Height - S(3), thumbWidth, S(2));
			}
		}
		using Pen borderPen = new Pen(_separatorColor);
		g.DrawLine(borderPen, 0, Height - 1, Width, Height - 1);
		if (!Enabled)
		{
			using SolidBrush veil = new SolidBrush(Color.FromArgb(96, Color.White));
			g.FillRectangle(veil, ClientRectangle);
		}
	}

	private int MeasureContentWidth(List<BarGroup> visibleGroups)
	{
		int width = S(8);
		using Graphics g = CreateGraphics();
		foreach (BarGroup group in visibleGroups)
		{
			foreach (BtnInfo item in group.Items.Where((BtnInfo i) => i.Command.Visible))
			{
				width += Math.Max(ButtonMinWidth, MeasureTextWidth(g, item.Command.Text, _textFont) + S(10));
			}
			width += S(8) + S(9);
		}
		return width;
	}

	private int MeasureTextWidth(Graphics g, string text, Font font)
	{
		if (string.IsNullOrEmpty(text))
		{
			return 0;
		}
		return (int)Math.Ceiling(g.MeasureString(text, font).Width);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);
		BtnInfo hoverBtn = HitTest(e.Location);
		if (hoverBtn != _hoverBtn)
		{
			_hoverBtn = hoverBtn;
			Cursor = ((hoverBtn != null) ? Cursors.Hand : Cursors.Default);
			StartAnim();
		}
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		base.OnMouseLeave(e);
		if (_hoverBtn != null)
		{
			_hoverBtn = null;
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
		BtnInfo btn = HitTest(e.Location);
		if (btn != null)
		{
			SimpleCommand command = btn.Command;
			if (command.Enabled && command.Visible)
			{
				command.ClickHandler?.Invoke(command, EventArgs.Empty);
			}
		}
	}

	protected override void OnMouseWheel(MouseEventArgs e)
	{
		base.OnMouseWheel(e);
		int maxScroll = Math.Max(0, _contentWidth - Width);
		if (maxScroll <= 0)
		{
			return;
		}
		float newScroll = Math.Max(0f, Math.Min(maxScroll, _scrollX - (float)e.Delta * 0.5f));
		if (newScroll != _scrollX)
		{
			_scrollX = newScroll;
			Invalidate();
		}
	}

	private BtnInfo HitTest(Point p)
	{
		foreach (Tuple<BtnInfo, Rectangle> btnRect in _btnRects)
		{
			if (btnRect.Item2.Contains(p))
			{
				return btnRect.Item1;
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
		ModuleModel module = CurrentModuleModel;
		if (module != null)
		{
			foreach (BarGroup group in module.Groups)
			{
				foreach (BtnInfo item in group.Items)
				{
					float target = ((item == _hoverBtn) ? 1f : 0f);
					if (item.Hover != target)
					{
						item.Hover += (target - item.Hover) * AnimStep;
						if (Math.Abs(target - item.Hover) < 0.02f)
						{
							item.Hover = target;
						}
						animating = true;
					}
				}
			}
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

	private void DrawBtnBack(Graphics g, Rectangle rect, float hover)
	{
		if (hover > 0.01f)
		{
			using GraphicsPath path = CreateRounded(rect, S(4));
			int alpha = (int)((float)_hoverColor.A * hover);
			using SolidBrush brush = new SolidBrush(Color.FromArgb(alpha, _hoverColor.R, _hoverColor.G, _hoverColor.B));
			g.FillPath(brush, path);
		}
	}

	private void DrawIcon(Graphics g, Image image, Rectangle rect, bool enabled)
	{
		if (image == null)
		{
			return;
		}
		InterpolationMode mode = g.InterpolationMode;
		g.InterpolationMode = InterpolationMode.HighQualityBicubic;
		if (enabled)
		{
			g.DrawImage(image, rect);
			g.InterpolationMode = mode;
			return;
		}
		using ImageAttributes attributes = new ImageAttributes();
		attributes.SetColorMatrix(new ColorMatrix
		{
			Matrix33 = 0.35f
		});
		g.DrawImage(image, rect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
		g.InterpolationMode = mode;
	}

	private void DrawText(Graphics g, string text, Font font, Rectangle rect, Color color)
	{
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		using SolidBrush brush = new SolidBrush(color);
		using StringFormat format = new StringFormat
		{
			Alignment = StringAlignment.Center,
			LineAlignment = StringAlignment.Center,
			FormatFlags = StringFormatFlags.NoWrap,
			Trimming = StringTrimming.EllipsisCharacter
		};
		g.DrawString(text, font, brush, rect, format);
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
