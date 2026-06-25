using System.Windows.Forms;
using C1.Win.C1Command;

namespace Auditai.UI.Controls;

public class C1ToolBarEx : C1ToolBar
{
	public delegate bool BarBackgroundPaintHandle(object sender, DrawBarEventArgs e);

	public BarBackgroundPaintHandle BarBackgroundPaintCallback { get; set; }

	public C1ToolBarEx()
	{
		base.DrawBar += C1ToolBarEx_DrawBar;
	}

	private void C1ToolBarEx_DrawBar(object sender, DrawBarEventArgs e)
	{
		if (BarBackgroundPaintCallback != null && BarBackgroundPaintCallback(this, e))
		{
			e.Done = true;
		}
	}

	protected override void OnPaintBackground(PaintEventArgs pevent)
	{
	}
}
