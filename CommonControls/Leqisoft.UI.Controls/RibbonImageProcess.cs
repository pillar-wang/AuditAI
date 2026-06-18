using System.Collections.Generic;

namespace Leqisoft.UI.Controls;

public class RibbonImageProcess
{
	private ImageStrategy _strategy;

	private List<ImageControl> _controls;

	public RibbonImageProcess()
	{
		_strategy = new DefaultImageStrategy();
		_controls = new List<ImageControl>();
	}

	public void SetImageStrategy(ImageStrategy strategy)
	{
		_strategy = strategy;
	}

	public void Register(ImageControl control)
	{
		_controls.Add(control);
	}

	public void Register(IEnumerable<ImageControl> controls)
	{
		_controls.AddRange(controls);
	}

	public void ProcessImage()
	{
		foreach (ImageControl control in _controls)
		{
			control.SetImage(_strategy.ProcessImage(control.GetImage() ?? control.GetOrignImage()));
		}
	}
}
