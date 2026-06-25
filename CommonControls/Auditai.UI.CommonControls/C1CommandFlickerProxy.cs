using System.Drawing;
using C1.Win.C1Command;

namespace Auditai.UI.CommonControls;

public class C1CommandFlickerProxy : AbstractFlickerProxy
{
	private readonly C1Command _c1command;

	public C1CommandFlickerProxy(C1Command c1Command)
	{
		_c1command = c1Command;
		orignContent = c1Command.Text;
		twinkleContent = c1Command.Text;
		orignImage = c1Command.Image;
		twinkleImage = c1Command.Image;
	}

	public override bool IsDisposed()
	{
		return _c1command == null;
	}

	protected override string GetContent()
	{
		return _c1command.Text;
	}

	protected override Image GetImage()
	{
		return _c1command.Image;
	}

	protected override void SetView(Image image, string content)
	{
		_c1command.Image = image;
	}
}
