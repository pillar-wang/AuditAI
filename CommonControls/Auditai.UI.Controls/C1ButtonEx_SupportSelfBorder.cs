using System.Drawing;
using System.Windows.Forms;
using C1.Win.C1Input;

namespace Auditai.UI.Controls;

public class C1ButtonEx_SupportSelfBorder : C1Button
{
	public Color BorderColor { get; set; } = Color.Transparent;


	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		Size size = base.Size;
		using Pen pen = new Pen(BorderColor);
		e.Graphics.DrawLine(pen, 0, 0, size.Width - 1, 0);
		e.Graphics.DrawLine(pen, 0, 0, 0, size.Height - 1);
		e.Graphics.DrawLine(pen, size.Width - 1, 0, size.Width - 1, size.Height - 1);
		e.Graphics.DrawLine(pen, 0, size.Height - 1, size.Width, size.Height - 1);
	}
}
