using System.Drawing;
using System.Windows.Forms;

namespace Auditai.UI.CommonControls;

public class LabelFlickerProxy : AbstractFlickerProxy
{
	private Label _label;

	public LabelFlickerProxy(Label label)
	{
		_label = label;
	}

	public override bool IsDisposed()
	{
		if (_label != null)
		{
			return _label.IsDisposed;
		}
		return true;
	}

	protected override void SetView(Image image, string content)
	{
		_label.Text = content;
	}

	protected override string GetContent()
	{
		return _label.Text;
	}

	protected override Image GetImage()
	{
		return _label.Image;
	}

	public override void Start()
	{
		orignContent = GetContent();
		twinkleContent = GetContent();
		base.Start();
	}
}
