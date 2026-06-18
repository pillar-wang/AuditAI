using System.Windows.Forms;
using C1.Win.C1SplitContainer;

namespace Leqisoft.UI.Controls;

public class C1SplitterPanelEx : C1SplitterPanel
{
	public delegate bool PaintHandle(object sender, PaintEventArgs e);

	public PaintHandle BackgroundRenderCallback { get; set; }

	public PaintHandle PaintCallback { get; set; }

	protected override void OnPaintBackground(PaintEventArgs e)
	{
		if (BackgroundRenderCallback == null)
		{
			base.OnPaintBackground(e);
		}
		else if (!BackgroundRenderCallback(this, e))
		{
			base.OnPaintBackground(e);
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		if (PaintCallback == null)
		{
			base.OnPaint(e);
		}
		else if (!PaintCallback(this, e))
		{
			base.OnPaint(e);
		}
	}
}
