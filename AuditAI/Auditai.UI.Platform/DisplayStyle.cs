using System;
using System.IO;
using Newtonsoft.Json;

namespace Auditai.UI.Platform;

public enum ListTileViewMode
{
	List,
	Tile
}

public class DisplayStyle
{
	public ListTileViewMode ViewMode { get; set; } = ListTileViewMode.List;
	public SortKind SortKind { get; set; }
	public int Width { get; set; }
	public int Height { get; set; }
	public object WindowState { get; set; }
	
	public void Save(string config)
	{
		try
		{
			var json = JsonConvert.SerializeObject(this);
			File.WriteAllText(config, json);
		}
		catch
		{
			// 保存失败时静默处理
		}
	}

	public void Load(params object[] args)
	{
		try
		{
			if (args.Length > 0 && args[0] is string config && File.Exists(config))
			{
				var json = File.ReadAllText(config);
				var loaded = JsonConvert.DeserializeObject<DisplayStyle>(json);
				if (loaded != null)
				{
					ViewMode = loaded.ViewMode;
					SortKind = loaded.SortKind;
					Width = loaded.Width;
					Height = loaded.Height;
					WindowState = loaded.WindowState;
				}
			}
		}
		catch
		{
			// 加载失败时保留默认值
		}
	}
}