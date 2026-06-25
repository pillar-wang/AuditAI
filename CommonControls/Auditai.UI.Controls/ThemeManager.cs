using System.Collections.Generic;

namespace Auditai.UI.Controls;

public class ThemeManager
{
	private static ThemeManager _themeManager;

	private List<ISetTheme> _setThemes;

	public static ThemeManager GetInstance()
	{
		if (_themeManager == null)
		{
			_themeManager = new ThemeManager();
		}
		return _themeManager;
	}

	private ThemeManager()
	{
		_setThemes = new List<ISetTheme>();
	}

	public void Register(ISetTheme setTheme)
	{
		_setThemes.Add(setTheme);
	}

	public void Unregister(ISetTheme obj)
	{
		_setThemes.Remove(obj);
	}

	public void ApplyTheme()
	{
		foreach (ISetTheme setTheme in _setThemes)
		{
			setTheme.SetTheme();
		}
	}
}
