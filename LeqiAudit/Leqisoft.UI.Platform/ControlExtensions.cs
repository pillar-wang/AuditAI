using System.Windows.Forms;

namespace Leqisoft.UI.Platform;

public static class ControlExtensions
{
	public static void SetHighLight(this Control control, object value)
	{
		var prop = control.GetType().GetProperty("HighLight");
		prop?.SetValue(control, value);
	}

	public static object GetHighLight(this Control control)
	{
		var prop = control.GetType().GetProperty("HighLight");
		return prop?.GetValue(control);
	}

	public static dynamic Rows(this Control control)
	{
		var prop = control.GetType().GetProperty("Rows");
		return prop?.GetValue(control);
	}
}