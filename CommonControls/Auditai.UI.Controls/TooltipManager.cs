using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using C1.Win.C1SuperTooltip;
using Auditai.DTO;

namespace Auditai.UI.Controls;

public class TooltipManager
{
	private Color _gradient;

	private C1SuperTooltip superTooltip = new C1SuperTooltip();

	private Dictionary<Component, TipInfo> _tooltips = new Dictionary<Component, TipInfo>();

	public static TooltipManager Instance { get; } = new TooltipManager();


	public bool ShouldDisplay => UserSet.Config.Tooltip;

	public TooltipManager()
	{
		superTooltip.Active = true;
		superTooltip.IsBalloon = true;
		superTooltip.BackgroundGradient = BackgroundGradient.None;
	}

	public TipInfo Get(Component component)
	{
		if (_tooltips.ContainsKey(component))
		{
			return _tooltips[component];
		}
		return null;
	}

	public void Attach(Component component, TipInfo tipInfo)
	{
		if (component != null && tipInfo != null && !_tooltips.ContainsKey(component))
		{
			_tooltips.Add(component, tipInfo);
		}
	}

	public void Show(TipInfo tipInfo, Control control, int x, int y)
	{
		if (control != null && tipInfo != null && QueryTooltipShow() && !string.IsNullOrWhiteSpace(tipInfo.Body))
		{
			string text = ApplyStyle1(tipInfo);
			superTooltip.Show(text, control, x, y);
		}
	}

	public void Hide()
	{
		superTooltip.Hide();
	}

	private bool QueryTooltipShow()
	{
		return UserSet.Config.Tooltip;
	}

	private System.Drawing.Image GradientImage(Color start, Color end)
	{
		Bitmap bitmap = new Bitmap(50, 150);
		Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
		LinearGradientBrush brush = new LinearGradientBrush(rect, start, end, LinearGradientMode.Vertical);
		using Graphics graphics = Graphics.FromImage(bitmap);
		graphics.FillRectangle(brush, rect);
		return bitmap;
	}

	private string ApplyStyle1(TipInfo tipInfo)
	{
		try
		{
			_ = _gradient;
			if (!_gradient.Equals(Theme.SelectedAuditaiTheme.ThemeContext.GradientColor))
			{
				_gradient = Theme.SelectedAuditaiTheme.ThemeContext.GradientColor;
				System.Drawing.Image backgroundImage = GradientImage(_gradient, Color.White);
				if (superTooltip.BackgroundImage != null)
				{
					superTooltip.BackgroundImage.Dispose();
				}
				superTooltip.BackgroundImage = backgroundImage;
			}
		}
		catch
		{
		}
		string arg = tipInfo.Body.Replace("\n", "<br>");
		return $"<div style='width:320px;color:Black;font-family:微软雅黑;font-size:12'><div style='font-weight:Bold'>{tipInfo.Title}</div><parm><hr noshade size=1 style='margin:2' color=Gray></parm><div>{arg}</div><parm><hr noshade size=1 style='margin:2' color=Gray></parm><div style='color:red;'>注：可点击主窗体右上角的“？”按钮关闭此动态提示框！</div> </div>";
	}
}
