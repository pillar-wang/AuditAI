using System.Drawing;
using System.Windows.Forms;
using C1.Win.C1Ribbon;

namespace Auditai.UI.Platform;

public static class FormFactory
{
	private static readonly Font _font;

	static FormFactory()
	{
		_font = new Font("微软雅黑", 9f);
	}

	public static C1RibbonForm Create()
	{
		return new C1RibbonForm
		{
			Font = _font,
			StartPosition = FormStartPosition.CenterScreen,
			VisualStyle = VisualStyle.Custom
		};
	}
}
